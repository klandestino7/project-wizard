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
///   1. Connect WS -> send "auth"
///   2. Send "queue.join"
///   3. Server pushes "match.proposed" when a match is found
///   4. Player accepts / declines -> server manages accept phase
///   5. Server pushes "match.ready":
///      - isHost=true  -> create invisible S&Box lobby -> send "lobby.register"
///      - isHost=false -> wait for "lobby.ready" then connect
/// </summary>
public sealed class MatchmakingSystem : SingletonComponent<MatchmakingSystem>
{
	[Property] public string ServerUrl { get; set; } = "ws://localhost:8080/ws";
	[Property] public int MaxPlayers { get; set; } = 10;

	public static bool IsMultiplayerSession { get; set; } = true;

	public enum SearchState
	{
		Idle,
		Connecting,
		Queued,
		MatchProposed,
		WaitingForLobby,
		Starting,
	}

	public static SearchState CurrentState { get; private set; } = SearchState.Idle;
	public static int CountdownSeconds { get; private set; }
	public static int AcceptedCount { get; private set; }
	public static int TotalPlayers { get; private set; }
	public static bool IsHost { get; private set; }
	public static string CurrentMatchId { get; private set; }

	public static event Action OnStateChanged;

	private WebSocket _socket;
	private bool _hasResponded;
	private DateTime _matchTimeoutAt;

	public static async Task FindOrCreateMatch()
	{
		if ( !Instance.IsValid() ) return;
		await Instance.StartMatchmaking();
	}

	public static async Task<bool> IsServerAvailable()
	{
		if ( !Instance.IsValid() ) return false;
		return await Instance.CheckServerAvailability();
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

	private async Task StartMatchmaking()
	{
		if ( CurrentState != SearchState.Idle ) return;

		IsMultiplayerSession = true;
		SetState( SearchState.Connecting );

		try
		{
			if ( !await CheckServerAvailability() )
			{
				SetState( SearchState.Idle );
				return;
			}
		}
		catch ( Exception e )
		{
			Log.Warning( $"MatchmakingSystem: Health check failed - {e.Message}" );
			SetState( SearchState.Idle );
			return;
		}

		_socket = new WebSocket();
		_socket.OnMessageReceived += OnMessage;
		_socket.OnDisconnected += OnDisconnected;

		try
		{
			await _socket.Connect( ServerUrl );
		}
		catch ( Exception e )
		{
			Log.Warning( $"MatchmakingSystem: WS connect failed - {e.Message}" );
			SetState( SearchState.Idle );
			return;
		}

		var steamId = Connection.Local.SteamId.ToString();
		var name = Connection.Local.DisplayName;
		var partyId = "";

		await SendJson( new
		{
			type = "auth",
			steamId = steamId,
			name = name,
			partyId = partyId
		} );
	}

	private async Task<bool> CheckServerAvailability()
	{
		var healthUrl = ServerUrl
			.Replace( "ws://", "http://" )
			.Replace( "wss://", "https://" )
			.Replace( "/ws", "/health" );

		var healthUri = new Uri( healthUrl );
		if ( !Http.IsAllowed( healthUri ) )
		{
			Log.Warning( $"MatchmakingSystem: Health check URL is not allowed: {healthUrl}" );
			return false;
		}

		var response = await Http.RequestStringAsync( healthUrl );
		var document = JsonDocument.Parse( response );
		var active = document.RootElement.GetProperty( "active" ).GetBoolean();
		if ( !active )
		{
			Log.Warning( "MatchmakingSystem: Server is not accepting queues." );
			return false;
		}

		return true;
	}

	private async Task SendJson( object obj )
	{
		if ( _socket == null ) return;
		var json = JsonSerializer.Serialize( obj );
		await _socket.Send( json );
	}

	private async void OnMessage( string json )
	{
		JsonNode root;
		try { root = JsonNode.Parse( json ); }
		catch { return; }

		var type = root?["type"]?.GetValue<string>();
		if ( type == null ) return;

		switch ( type )
		{
			case "welcome":
				Log.Info( "MatchmakingSystem: Authenticated with matchmaking server." );
				await SendJson( new { type = "queue.join" } );
				break;

			case "queued":
				SetState( SearchState.Queued );
				break;

			case "match.proposed":
				_hasResponded = false;
				CurrentMatchId = root["matchId"]?.GetValue<string>() ?? "";
				TotalPlayers = root["playerCount"]?.GetValue<int>() ?? 0;
				AcceptedCount = root["acceptedCount"]?.GetValue<int>() ?? 0;
				IsHost = root["isHost"]?.GetValue<bool>() ?? false;

				var timeoutStr = root["timeoutAt"]?.GetValue<string>();
				_matchTimeoutAt = timeoutStr != null
					? DateTime.Parse( timeoutStr, null, System.Globalization.DateTimeStyles.RoundtripKind )
					: DateTime.UtcNow.AddSeconds( 20 );

				CountdownSeconds = Math.Max( 0, (int)(_matchTimeoutAt - DateTime.UtcNow).TotalSeconds );
				SetState( SearchState.MatchProposed );
				_ = TickCountdown();
				break;

			case "match.update":
				AcceptedCount = root["acceptedCount"]?.GetValue<int>() ?? AcceptedCount;
				TotalPlayers = root["totalPlayers"]?.GetValue<int>() ?? TotalPlayers;
				NotifyStateChanged();
				break;

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

			case "lobby.ready":
				var hostSteamIdStr = root["lobbyId"]?.GetValue<string>();
				if ( !string.IsNullOrEmpty( hostSteamIdStr ) )
				{
					await ConnectToLobby( hostSteamIdStr );
				}
				break;

			case "match.cancelled":
				var reason = root["reason"]?.GetValue<string>() ?? "unknown";
				Log.Info( $"MatchmakingSystem: Match cancelled - {reason}" );
				SetState( SearchState.Queued );
				break;

			case "party.state":
				break;

			case "error":
				Log.Warning( $"MatchmakingSystem: Server error - {root["message"]?.GetValue<string>()}" );
				break;

			case "pong":
				break;
		}
	}

	private void OnDisconnected( int status, string reason )
	{
		Log.Warning( $"MatchmakingSystem: WebSocket disconnected ({status}) - {reason}" );
		if ( CurrentState != SearchState.Idle )
			SetState( SearchState.Idle );
	}

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
		_socket?.Dispose();
		_socket = null;
		SetState( SearchState.Idle );
	}

	private async Task CreateAndRegisterLobby()
	{
		SetState( SearchState.Starting );

		Networking.CreateLobby( new LobbyConfig
		{
			MaxPlayers = MaxPlayers,
		} );

		await Task.DelayRealtimeSeconds( 0.5f );

		var hostSteamId = Connection.Local.SteamId.ToString();
		Log.Info( $"MatchmakingSystem: Lobby created - host steamId {hostSteamId}" );

		await SendJson( new
		{
			type = "lobby.register",
			lobbyId = hostSteamId
		} );

		StartHostedMatch();
	}

	private Task ConnectToLobby( string hostSteamIdStr )
	{
		SetState( SearchState.Starting );
		Log.Info( $"MatchmakingSystem: Connecting to host {hostSteamIdStr}..." );

		if ( !ulong.TryParse( hostSteamIdStr, out var hostSteamId ) )
		{
			Log.Warning( $"MatchmakingSystem: Invalid host steamId '{hostSteamIdStr}'." );
			SetState( SearchState.Idle );
			return Task.CompletedTask;
		}

		Networking.TryConnectSteamId( hostSteamId );
		return Task.CompletedTask;
	}

	private void StartHostedMatch()
	{
		var selectedMap = WarlocksPlaylist.GetWarlocksScene();
		if ( selectedMap is null )
		{
			Log.Warning( $"MatchmakingSystem: Warlocks scene '{WarlocksPlaylist.WarlocksScenePath}' was not found." );
			SetState( SearchState.Idle );
			return;
		}

		Log.Info( $"MatchmakingSystem: Loading hosted match scene {selectedMap.ResourcePath}" );
		Game.ActiveScene.Load( selectedMap );
	}

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

	private static void SetState( SearchState state )
	{
		CurrentState = state;
		NotifyStateChanged();
	}

	private static void NotifyStateChanged() => OnStateChanged?.Invoke();
}
