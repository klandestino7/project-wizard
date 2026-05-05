using Sandbox;
using Sandbox.Network;

namespace Warlocks;

/// <summary>
/// Manages the matchmaking flow: search → lobby → match proposal (15s) → game start.
/// Add this SingletonComponent to a GameObject in the menu scene.
/// </summary>
public sealed class MatchmakingSystem : SingletonComponent<MatchmakingSystem>
{
	public enum SearchState
	{
		Idle,
		Searching,     // Querying for lobbies
		InLobby,       // Host created lobby, waiting for players
		JoinedLobby,   // Client joined a lobby, waiting for host to start
		MatchProposed, // Enough players — countdown running
		Starting,      // Loading game scene
	}

	[Property] public int MinPlayers { get; set; } = 2;
	[Property] public int MaxPlayers { get; set; } = 10;
	[Property] public int AcceptTimeoutSeconds { get; set; } = 15;

	/// <summary>Set to false before loading Sandbox Mode.</summary>
	public static bool IsMultiplayerSession { get; set; } = true;

	// ── Read-only state consumed by UI ───────────────────────────────────────

	public static SearchState CurrentState { get; private set; } = SearchState.Idle;
	public static int CountdownSeconds { get; private set; }
	public static string ProposedMapTitle { get; private set; } = "";
	public static int CurrentPlayerCount => Connection.All.Count();

	/// <summary>Fires on every state/countdown change so UI can refresh.</summary>
	public static event Action OnStateChanged;

	// ── Private fields ───────────────────────────────────────────────────────

	private bool _cancelled;
	private bool _isHost;
	private SceneFile _proposedMap;
	private GameMode _proposedGameMode;

	// ── Public API ───────────────────────────────────────────────────────────

	public static async Task FindOrCreateMatch()
	{
		if ( !Instance.IsValid() )
		{
			Log.Warning( "MatchmakingSystem: No instance in menu scene. Add the component to a GameObject." );
			return;
		}

		await Instance.RunMatchmaking();
	}

	public static void CancelSearch()
	{
		if ( !Instance.IsValid() ) return;
		Instance.DoCancel();
	}

	/// <summary>Host only: skip countdown and start immediately.</summary>
	public static void ForceStartMatch()
	{
		if ( !Instance.IsValid() ) return;
		if ( CurrentState != SearchState.MatchProposed ) return;
		if ( !Networking.IsHost ) return;

		Instance.LoadGameScene();
	}

	// ── State machine ────────────────────────────────────────────────────────

	private async Task RunMatchmaking()
	{
		_cancelled = false;
		_isHost = false;
		IsMultiplayerSession = true;

		SetState( SearchState.Searching );

		// Step 1 – Query for an open lobby
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

		// Step 2a – Join existing lobby
		if ( foundLobby.HasValue )
		{
			Log.Info( "MatchmakingSystem: Found a lobby, joining..." );
			var joined = await Networking.TryConnectSteamId( foundLobby.Value.LobbyId );
			if ( joined )
			{
				SetState( SearchState.JoinedLobby );
				// Client waits here. When host loads the scene, client follows automatically.
				return;
			}
			Log.Warning( "MatchmakingSystem: Failed to join found lobby, creating a new one." );
		}

		if ( _cancelled ) return;

		// Step 2b – Create a new lobby and wait for players
		_isHost = true;
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

		// Auto-select a random available map
		var maps = GameUtils.GetAvailableMaps().ToList();
		if ( maps.Count == 0 )
		{
			Log.Warning( "MatchmakingSystem: No maps found with IsVisibleInMenu=true metadata." );
			return;
		}

		_proposedMap = maps[Random.Shared.Next( maps.Count )];
		_proposedGameMode = GameMode.GetAll( _proposedMap ).FirstOrDefault();
		ProposedMapTitle = _proposedMap.GetMetadata( "Title", _proposedMap.ResourceName );

		CountdownSeconds = AcceptTimeoutSeconds;
		SetState( SearchState.MatchProposed );

		Log.Info( $"MatchmakingSystem: Match proposed — '{ProposedMapTitle}', {AcceptTimeoutSeconds}s countdown." );

		while ( CountdownSeconds > 0 && !_cancelled )
		{
			// If a player left during countdown, reset
			if ( Connection.All.Count() < MinPlayers )
			{
				Log.Info( "MatchmakingSystem: Player dropped out, resetting countdown." );
				SetState( SearchState.InLobby );
				await WaitForPlayers();
				return;
			}

			await Task.DelayRealtimeSeconds( 1f );
			CountdownSeconds--;
			NotifyStateChanged();
		}

		if ( _cancelled ) return;

		LoadGameScene();
	}

	private void DoCancel()
	{
		Log.Info(" DoCancel ");
		_cancelled = true;
		_isHost = false;
		SetState( SearchState.Idle );

		if ( Networking.IsActive )
			Networking.Disconnect();
	}

	private void LoadGameScene()
	{
		SetState( SearchState.Starting );

		if ( _proposedGameMode.IsValid() )
			GameMode.SetCurrent( _proposedGameMode );

		Log.Info( $"MatchmakingSystem: Loading scene '{ProposedMapTitle}'" );
		Game.ActiveScene.Load( _proposedMap );
	}

	private static void SetState( SearchState state )
	{
		CurrentState = state;
		NotifyStateChanged();
	}

	private static void NotifyStateChanged() => OnStateChanged?.Invoke();
}
