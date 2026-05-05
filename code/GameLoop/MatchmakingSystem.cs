using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sandbox;
using Sandbox.Network;

namespace Warlocks;

/// <summary>
/// Matchmaking via external Go server over WebSocket.
///
/// Flow:
///   1. Connect WS → send "auth"
///   2. Send "queue.join"
///   3. Server pushes "match.proposed" when a match is found
///   4. Player accepts / declines → server manages accept phase
///   5. Server pushes "match.ready":
///      - isHost=true  → create invisible S&amp;Box lobby → send "lobby.register"
///      - isHost=false → wait for "lobby.ready" then connect
/// </summary>
public sealed class MatchmakingSystem : SingletonComponent<MatchmakingSystem>
{
	// ── Config ────────────────────────────────────────────────────────────────

	/// <summary>WebSocket address of the Go matchmaking server.</summary>
	[Property] public string ServerUrl { get; set; } = "ws://localhost:8080/ws";

	[Property] public int MaxPlayers { get; set; } = 10;

	/// <summary>Set to false before loading Sandbox Mode.</summary>
	public static bool IsMultiplayerSession { get; set; } = true;

	// ── State (UI-readable) ───────────────────────────────────────────────────

	public enum SearchState
	{
		Idle,
		Connecting,
		Queued,
		MatchProposed,
		WaitingForLobby,
		Starting,
	}

	public static SearchState CurrentState    { get; private set; } = SearchState.Idle;
	public static int  CountdownSeconds       { get; private set; }
	public static int  AcceptedCount          { get; private set; }
	public static int  TotalPlayers           { get; private set; }
	public static bool IsHost                 { get; private set; }
	public static string CurrentMatchId       { get; private set; }

	/// <summary>Fires on every state or counter change so UI panels can refresh.</summary>
	public static event Action OnStateChanged;

	// ── Private ───────────────────────────────────────────────────────────────

	private WebSocket _socket;
	private bool      _hasResponded;
	private DateTime  _matchTimeoutAt;

	// ── Public API (called from UI / other systems) ───────────────────────────

	public static async Task FindOrCreateMatch()
	{
		if ( !Instance.IsValid() ) return;
		await Instance.StartMatchmaking();
	}

	public static void CancelSearch()
	{
		if ( !Instance.IsValid() ) return;
		Instance.DoCancel();
	}

	public static void AcceptMatch()
	{
		if ( !Instance.IsValid() ) return;
		if ( CurrentState != SearchState.MatchProposed ) return;
		Instance.SendAccept();
	}

	public static void DeclineMatch()
	{
		if ( !Instance.IsValid() ) return;
		Instance.SendDecline();
	}

	// ── Matchmaking flow ──────────────────────────────────────────────────────

	private async Task StartMatchmaking()
	{
		if ( CurrentState != SearchState.Idle ) return;

		IsMultiplayerSession = true;
		SetState( SearchState.Connecting );

		// Health check first (plain HTTP)
		var healthUrl = ServerUrl
			.Replace( "ws://",  "http://" )
			.Replace( "wss://", "https://" )
			.Replace( "/ws", "/health" );

		try
		{
			var http = new System.Net.Http.HttpClient();
			http.Timeout = TimeSpan.FromSeconds( 5 );
			var resp = await http.GetStringAsync( healthUrl );
			var doc  = JsonDocument.Parse( resp );
			var active = doc.RootElement.GetProperty( "active" ).GetBoolean();
			if ( !active )
			{
				Log.Warning( "MatchmakingSystem: Server is not accepting queues." );
				SetState( SearchState.Idle );
				return;
			}
		}
		catch ( Exception e )
		{
			Log.Warning( $"MatchmakingSystem: Health check failed — {e.Message}" );
			SetState( SearchState.Idle );
			return;
		}

		// Open WebSocket
		_socket = new WebSocket();
		_socket.OnMessageReceived += OnMessage;
		_socket.OnDisconnected += OnDisconnected;

		try
		{
			await _socket.Connect( ServerUrl );
		}
		catch ( Exception e )
		{
			Log.Warning( $"MatchmakingSystem: WS connect failed — {e.Message}" );
			SetState( SearchState.Idle );
			return;
		}

		// Authenticate
		var steamId = Connection.Local.SteamId.ToString();
		var name    = Connection.Local.DisplayName;

		// TODO: attach partyId here if the local player is in a party
		// var partyId = PartySystem.Instance?.LocalPartyId ?? "";
		var partyId = "";

		await SendJson( new
		{
			type    = "auth",
			steamId = steamId,
			name    = name,
			partyId = partyId
		} );
	}

	private async Task SendJson( object obj )
	{
		if ( _socket == null ) return;
		var json = JsonSerializer.Serialize( obj );
		await _socket.Send( json );
	}

	// ── WebSocket message handler ─────────────────────────────────────────────

	private async void OnMessage( string json )
	{
		JsonNode root;
		try { root = JsonNode.Parse( json ); }
		catch { return; }

		var type = root?["type"]?.GetValue<string>();
		if ( type == null ) return;

		switch ( type )
		{
			// ── Server acknowledged our connection
			case "welcome":
				Log.Info( "MatchmakingSystem: Authenticated with matchmaking server." );
				// Auto-join the queue right after auth
				await SendJson( new { type = "queue.join" } );
				break;

			// ── We are in the queue
			case "queued":
				SetState( SearchState.Queued );
				break;

			// ── A match was found — show accept popup
			case "match.proposed":
				_hasResponded  = false;
				CurrentMatchId = root["matchId"]?.GetValue<string>() ?? "";
				TotalPlayers   = root["playerCount"]?.GetValue<int>() ?? 0;
				AcceptedCount  = root["acceptedCount"]?.GetValue<int>() ?? 0;
				IsHost         = root["isHost"]?.GetValue<bool>() ?? false;

				var timeoutStr = root["timeoutAt"]?.GetValue<string>();
				_matchTimeoutAt = timeoutStr != null
					? DateTime.Parse( timeoutStr, null, System.Globalization.DateTimeStyles.RoundtripKind )
					: DateTime.UtcNow.AddSeconds( 20 );

				CountdownSeconds = Math.Max( 0, (int)(_matchTimeoutAt - DateTime.UtcNow).TotalSeconds );
				SetState( SearchState.MatchProposed );

				// Start local countdown (display only — server is authoritative)
				_ = TickCountdown();
				break;

			// ── Accept count changed (someone accepted)
			case "match.update":
				AcceptedCount = root["acceptedCount"]?.GetValue<int>() ?? AcceptedCount;
				TotalPlayers  = root["totalPlayers"]?.GetValue<int>() ?? TotalPlayers;
				NotifyStateChanged();
				break;

			// ── Everyone accepted — proceed with lobby
			case "match.ready":
				IsHost = root["isHost"]?.GetValue<bool>() ?? false;
				if ( IsHost )
				{
					await CreateAndRegisterLobby();
				}
				else
				{
					SetState( SearchState.WaitingForLobby );
				}
				break;

			// ── Host registered the lobby — non-host players can connect
			case "lobby.ready":
				var lobbyIdStr = root["lobbyId"]?.GetValue<string>();
				if ( !string.IsNullOrEmpty( lobbyIdStr ) && ulong.TryParse( lobbyIdStr, out var lobbyId ) )
				{
					await ConnectToLobby( lobbyId );
				}
				break;

			// ── Match was cancelled (decline / timeout / disconnect)
			case "match.cancelled":
				var reason = root["reason"]?.GetValue<string>() ?? "unknown";
				Log.Info( $"MatchmakingSystem: Match cancelled — {reason}" );
				// Server auto-re-queues us; update UI
				SetState( SearchState.Queued );
				break;

			// ── Party state update
			case "party.state":
				// TODO: forward to PartySystem if implemented
				break;

			// ── Server error
			case "error":
				Log.Warning( $"MatchmakingSystem: Server error — {root["message"]?.GetValue<string>()}" );
				break;

			case "pong":
				break;
		}
	}

	private void OnDisconnected()
	{
		Log.Warning( "MatchmakingSystem: WebSocket disconnected." );
		if ( CurrentState != SearchState.Idle )
			SetState( SearchState.Idle );
	}

	// ── Accept / Decline ──────────────────────────────────────────────────────

	private async void SendAccept()
	{
		if ( _hasResponded ) return;
		_hasResponded = true;
		await SendJson( new { type = "match.accept" } );
	}

	private async void SendDecline()
	{
		if ( _hasResponded ) return;
		_hasResponded = true;
		await SendJson( new { type = "match.decline" } );
		DoCancel();
	}

	private void DoCancel()
	{
		_ = SendJson( new { type = "queue.leave" } );
		_socket?.Disconnect();
		_socket = null;
		SetState( SearchState.Idle );
	}

	// ── Lobby creation (host only) ────────────────────────────────────────────

	private async Task CreateAndRegisterLobby()
	{
		SetState( SearchState.Starting );

		// Create a private S&Box lobby so it doesn't appear in public queries.
		// Other players connect only via the lobby ID we register with the Go server.
		Networking.CreateLobby( new LobbyConfig
		{
			MaxPlayers = MaxPlayers,
			// NOTE: set Visibility = LobbyVisibility.FriendsOnly (or Invisible) once
			// S&Box exposes that property on LobbyConfig.
		} );

		// Brief wait for the Steam lobby to be fully created before reading its ID.
		await Task.DelayRealtimeSeconds( 0.5f );

		// Networking.LobbyId is the Steam lobby ID assigned after CreateLobby().
		// Other players pass this to Networking.TryConnectSteamId() to join.
		var lobbyId = Networking.LobbyId;
		Log.Info( $"MatchmakingSystem: Lobby created — ID {lobbyId}" );

		await SendJson( new
		{
			type    = "lobby.register",
			lobbyId = lobbyId.ToString()
		} );
	}

	private async Task ConnectToLobby( ulong lobbyId )
	{
		SetState( SearchState.Starting );
		Log.Info( $"MatchmakingSystem: Connecting to lobby {lobbyId}..." );

		var connected = await Networking.TryConnectSteamId( lobbyId );
		if ( !connected )
		{
			Log.Warning( "MatchmakingSystem: Failed to connect to lobby." );
			SetState( SearchState.Idle );
		}
	}

	// ── Countdown (display only) ──────────────────────────────────────────────

	private async Task TickCountdown()
	{
		while ( CurrentState == SearchState.MatchProposed && CountdownSeconds > 0 )
		{
			await Task.DelayRealtimeSeconds( 1f );
			if ( CurrentState != SearchState.MatchProposed ) return;
			CountdownSeconds = Math.Max( 0, (int)(_matchTimeoutAt - DateTime.UtcNow).TotalSeconds );
			NotifyStateChanged();
		}
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	private static void SetState( SearchState state )
	{
		CurrentState = state;
		NotifyStateChanged();
	}

	private static void NotifyStateChanged() => OnStateChanged?.Invoke();
}
