using Sandbox;
using Sandbox.Network;

namespace Warlocks;

/// <summary>
/// CS2-style matchmaking: search → lobby → accept phase (all must confirm) → game start.
/// The match only loads after every player accepts. Decliners are kicked and must re-queue.
/// </summary>
public sealed class MatchmakingSystem : SingletonComponent<MatchmakingSystem>
{
	public enum SearchState
	{
		Idle,
		Searching,       // Querying for / creating a lobby
		InLobby,         // Host: lobby open, waiting for players
		JoinedLobby,     // Client: waiting in lobby
		MatchProposed,   // Accept popup active — everyone must confirm
		Starting,        // Scene loading
	}

	[Property] public int MinPlayers { get; set; } = 2;
	[Property] public int MaxPlayers { get; set; } = 10;
	[Property] public int AcceptTimeoutSeconds { get; set; } = 15;

	/// <summary>Set to false before loading Sandbox Mode.</summary>
	public static bool IsMultiplayerSession { get; set; } = true;

	// ── UI-readable state ────────────────────────────────────────────────────

	public static SearchState CurrentState { get; private set; } = SearchState.Idle;
	public static int CountdownSeconds { get; private set; }
	public static string ProposedMapTitle { get; private set; } = "";
	public static int CurrentPlayerCount => Connection.All.Count();
	public static int AcceptedCount { get; private set; }

	/// <summary>Fires on every state/countdown change so the UI can refresh.</summary>
	public static event Action OnStateChanged;

	// ── Private state ────────────────────────────────────────────────────────

	private bool _cancelled;
	private bool _hasResponded;
	private bool _proposalActive;
	private readonly HashSet<ulong> _acceptedSteamIds = new();
	private SceneFile _proposedMap;
	private GameMode _proposedGameMode;

	// ── Public API ───────────────────────────────────────────────────────────

	public static async Task FindOrCreateMatch()
	{
		if ( !Instance.IsValid() )
		{
			Log.Warning( "MatchmakingSystem: No instance found." );
			return;
		}
		await Instance.RunMatchmaking();
	}

	public static void CancelSearch()
	{
		if ( !Instance.IsValid() ) return;
		Instance.DoCancel();
	}

	/// <summary>Called when the local player clicks Accept in the popup.</summary>
	public static void AcceptMatch()
	{
		if ( !Instance.IsValid() ) return;
		if ( CurrentState != SearchState.MatchProposed ) return;
		Instance.DoAccept();
	}

	/// <summary>Called when the local player clicks Decline in the popup.</summary>
	public static void DeclineMatch()
	{
		if ( !Instance.IsValid() ) return;
		if ( CurrentState != SearchState.MatchProposed ) return;
		Instance.DoDecline();
	}

	// ── Matchmaking flow ─────────────────────────────────────────────────────

	private async Task RunMatchmaking()
	{
		_cancelled = false;
		IsMultiplayerSession = true;
		SetState( SearchState.Searching );

		LobbyInformation? foundLobby = null;
		try
		{
			var lobbies = await Networking.QueryLobbies();
			foundLobby = lobbies
				.Where( l => !l.IsFull )
				.OrderByDescending( l => l.Members )
				.Cast<LobbyInformation?>()
				.FirstOrDefault();
		}
		catch ( Exception e )
		{
			Log.Warning( $"MatchmakingSystem: Lobby query failed: {e.Message}" );
		}

		if ( _cancelled ) return;

		if ( foundLobby.HasValue )
		{
			Log.Info( "MatchmakingSystem: Joining existing lobby..." );
			var joined = await Networking.TryConnectSteamId( foundLobby.Value.LobbyId );
			if ( joined )
			{
				SetState( SearchState.JoinedLobby );
				return; // Host will broadcast when ready
			}
			Log.Warning( "MatchmakingSystem: Failed to join, creating new lobby." );
		}

		if ( _cancelled ) return;

		Log.Info( "MatchmakingSystem: Creating new lobby..." );
		Networking.CreateLobby( new LobbyConfig { MaxPlayers = MaxPlayers } );
		SetState( SearchState.InLobby );
		await WaitForPlayers();
	}

	private async Task WaitForPlayers()
	{
		while ( !_cancelled )
		{
			NotifyStateChanged();

			if ( Connection.All.Count() >= MinPlayers )
			{
				await ProposeMatch();
				return;
			}

			await Task.DelayRealtimeSeconds( 1f );
		}
	}

	private async Task ProposeMatch()
	{
		if ( _cancelled ) return;

		var maps = GameUtils.GetAvailableMaps().ToList();
		if ( maps.Count == 0 )
		{
			Log.Warning( "MatchmakingSystem: No maps found." );
			return;
		}

		_proposedMap = maps[Random.Shared.Next( maps.Count )];
		_proposedGameMode = GameMode.GetAll( _proposedMap ).FirstOrDefault();
		var mapTitle = _proposedMap.GetMetadata( "Title", _proposedMap.ResourceName );

		_proposalActive = true;
		_acceptedSteamIds.Clear();
		AcceptedCount = 0;
		_hasResponded = false;

		// Update host state locally
		ProposedMapTitle = mapTitle;
		CountdownSeconds = AcceptTimeoutSeconds;
		SetState( SearchState.MatchProposed );

		// Broadcast to clients
		using ( Rpc.Broadcast() )
			ClientOnMatchProposed( mapTitle, AcceptTimeoutSeconds );

		Log.Info( $"MatchmakingSystem: Proposed '{mapTitle}', waiting for accepts ({AcceptTimeoutSeconds}s)..." );

		// Host countdown loop
		while ( CountdownSeconds > 0 && _proposalActive && !_cancelled )
		{
			if ( Connection.All.Count() < MinPlayers )
			{
				Log.Info( "MatchmakingSystem: Player dropped during accept phase." );
				_proposalActive = false;
				SetState( SearchState.InLobby );
				using ( Rpc.Broadcast() )
					ClientOnMatchCancelled();
				await WaitForPlayers();
				return;
			}

			await Task.DelayRealtimeSeconds( 1f );
			CountdownSeconds--;
			NotifyStateChanged();
		}

		if ( !_proposalActive || _cancelled ) return;

		// Timeout — kick anyone who didn't accept
		Log.Info( "MatchmakingSystem: Timeout, kicking non-acceptors." );
		var nonAcceptors = Connection.All
			.Where( c => !_acceptedSteamIds.Contains( c.SteamId ) )
			.ToList();

		foreach ( var conn in nonAcceptors )
			conn.Kick();

		var remaining = Connection.All.Count() - nonAcceptors.Count;
		if ( remaining >= MinPlayers )
		{
			LoadGameScene();
		}
		else
		{
			_proposalActive = false;
			SetState( SearchState.InLobby );
			using ( Rpc.Broadcast() )
				ClientOnMatchCancelled();
			await WaitForPlayers();
		}
	}

	// ── Accept / Decline (local player) ──────────────────────────────────────

	private void DoAccept()
	{
		if ( _hasResponded ) return;
		_hasResponded = true;

		if ( Networking.IsHost )
		{
			HandleAccept( Connection.Local?.SteamId ?? 0 );
		}
		else
		{
			// Send to host
			using ( Rpc.Owner() )
				ServerAccept( Connection.Local?.SteamId ?? 0 );
		}
	}

	private void DoDecline()
	{
		if ( _hasResponded ) return;
		_hasResponded = true;

		if ( Networking.IsHost )
		{
			_proposalActive = false;
			SetState( SearchState.Idle );
			using ( Rpc.Broadcast() )
				ClientOnMatchCancelled();
			if ( Networking.IsActive )
				Networking.Disconnect();
		}
		else
		{
			// Notify host, then clean up locally
			using ( Rpc.Owner() )
				ServerDecline( Connection.Local?.SteamId ?? 0 );
			SetState( SearchState.Idle );
			if ( Networking.IsActive )
				Networking.Disconnect();
		}
	}

	// ── Server-side RPC receivers (called by clients via Rpc.Owner) ───────────

	private void ServerAccept( ulong callerSteamId )
	{
		if ( !Networking.IsHost ) return;
		if ( !_proposalActive ) return;
		HandleAccept( callerSteamId );
	}

	private void ServerDecline( ulong callerSteamId )
	{
		if ( !Networking.IsHost ) return;
		if ( !_proposalActive ) return;
		var conn = Connection.All.FirstOrDefault( c => c.SteamId == callerSteamId );
		if ( conn != null )
			HandleDecline( conn );
	}

	// ── Client-side RPC receivers (called by host via Rpc.Broadcast) ──────────

	private void ClientOnMatchProposed( string mapTitle, int timeoutSeconds )
	{
		ProposedMapTitle = mapTitle;
		CountdownSeconds = timeoutSeconds;
		AcceptedCount = 0;
		_hasResponded = false;
		SetState( SearchState.MatchProposed );
		_ = ClientCountdown();
	}

	private void ClientOnAcceptCountUpdated( int count )
	{
		AcceptedCount = count;
		NotifyStateChanged();
	}

	private void ClientOnMatchCancelled()
	{
		SetState( SearchState.JoinedLobby );
	}

	// ── Accept / Decline logic (host only) ────────────────────────────────────

	private void HandleAccept( ulong steamId )
	{
		if ( steamId == 0 ) return;
		_acceptedSteamIds.Add( steamId );
		AcceptedCount = _acceptedSteamIds.Count;
		int total = Connection.All.Count();

		using ( Rpc.Broadcast() )
			ClientOnAcceptCountUpdated( AcceptedCount );

		Log.Info( $"MatchmakingSystem: {AcceptedCount}/{total} accepted." );

		if ( _acceptedSteamIds.Count >= total )
		{
			_proposalActive = false;
			Log.Info( "MatchmakingSystem: All accepted! Starting match." );
			LoadGameScene();
		}
	}

	private void HandleDecline( Connection conn )
	{
		Log.Info( $"MatchmakingSystem: {conn.DisplayName} declined." );
		conn.Kick();
		_acceptedSteamIds.Remove( conn.SteamId );

		var remaining = Connection.All.Count() - 1;
		if ( remaining < MinPlayers )
		{
			_proposalActive = false;
			SetState( SearchState.InLobby );
			using ( Rpc.Broadcast() )
				ClientOnMatchCancelled();
			_ = WaitForPlayers();
		}
	}

	// ── Client countdown ─────────────────────────────────────────────────────

	private async Task ClientCountdown()
	{
		while ( CurrentState == SearchState.MatchProposed && CountdownSeconds > 0 )
		{
			await Task.DelayRealtimeSeconds( 1f );
			if ( CurrentState != SearchState.MatchProposed ) return;
			CountdownSeconds--;
			NotifyStateChanged();
		}
	}

	// ── Shared helpers ────────────────────────────────────────────────────────

	private void DoCancel()
	{
		Log.Info( "MatchmakingSystem: DoCancel" );
		_cancelled = true;
		_proposalActive = false;
		SetState( SearchState.Idle );

		if ( Networking.IsActive )
			Networking.Disconnect();
	}

	private void LoadGameScene()
	{
		SetState( SearchState.Starting );

		if ( _proposedGameMode.IsValid() )
			GameMode.SetCurrent( _proposedGameMode );

		Log.Info( $"MatchmakingSystem: Loading '{ProposedMapTitle}'" );
		Game.ActiveScene.Load( _proposedMap );
	}

	private static void SetState( SearchState state )
	{
		CurrentState = state;
		NotifyStateChanged();
	}

	private static void NotifyStateChanged() => OnStateChanged?.Invoke();
}
