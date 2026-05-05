using Sandbox;
using Sandbox.Network;

namespace Warlocks;

/// <summary>
/// Handles matchmaking: queries for existing lobbies, joins if found, or creates a new one.
/// Place this component in the menu scene on a persistent GameObject.
/// </summary>
public sealed class MatchmakingSystem : SingletonComponent<MatchmakingSystem>
{
	/// <summary>
	/// Minimum players required to auto-start a match.
	/// </summary>
	[Property] public int MinPlayers { get; set; } = 2;

	/// <summary>
	/// Maximum players per match.
	/// </summary>
	[Property] public int MaxPlayers { get; set; } = 10;

	/// <summary>
	/// The game scene to load when a match is ready.
	/// </summary>
	[Property] public SceneFile GameScene { get; set; }

	/// <summary>
	/// Whether the next game session should be multiplayer.
	/// Set to false before loading Sandbox Mode.
	/// </summary>
	public static bool IsMultiplayerSession { get; set; } = true;

	private bool _isSearching = false;
	private bool _cancelled = false;
	private bool _isHost = false;
	private Action<int, int> _lobbyCreatedCallback;

	/// <summary>
	/// Finds an available match or creates a new lobby and waits.
	/// </summary>
	public static async Task FindOrCreateMatch(
		Action<LobbyInformation> onMatchFound,
		Action<int, int> onLobbyCreated )
	{
		if ( !Instance.IsValid() )
		{
			Log.Warning( "MatchmakingSystem: No instance found in scene." );
			return;
		}

		await Instance.RunMatchmaking( onMatchFound, onLobbyCreated );
	}

	/// <summary>
	/// Cancels an ongoing search and disconnects/closes the lobby.
	/// </summary>
	public static void CancelSearch()
	{
		if ( !Instance.IsValid() ) return;
		Instance.DoCancel();
	}

	/// <summary>
	/// If we are the host, force-starts the match immediately.
	/// </summary>
	public static void ForceStartMatch()
	{
		if ( !Instance.IsValid() ) return;
		if ( !Instance._isHost ) return;
		Instance.LoadGameScene();
	}

	private async Task RunMatchmaking(
		Action<LobbyInformation> onMatchFound,
		Action<int, int> onLobbyCreated )
	{
		_isSearching = true;
		_cancelled = false;
		_isHost = false;

		IsMultiplayerSession = true;

		// Step 1: Query existing lobbies for an available match
		Log.Info( "MatchmakingSystem: Searching for available matches..." );

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

		// Step 2a: Found an existing lobby — join it
		if ( foundLobby.HasValue )
		{
			Log.Info( "MatchmakingSystem: Found a lobby, joining..." );
			onMatchFound?.Invoke( foundLobby.Value );

			var joined = await Networking.TryConnectSteamId( foundLobby.Value.LobbyId );
			if ( !joined )
			{
				Log.Warning( "MatchmakingSystem: Failed to join found lobby, falling back to creating one." );
			}
			else
			{
				_isSearching = false;
				return;
			}
		}

		if ( _cancelled ) return;

		// Step 2b: No lobby found — create one and wait for players
		Log.Info( "MatchmakingSystem: No lobby found, creating a new one..." );
		_isHost = true;
		_lobbyCreatedCallback = onLobbyCreated;

		Networking.CreateLobby( new LobbyConfig
		{
			MaxPlayers = MaxPlayers,
		} );

		onLobbyCreated?.Invoke( 1, MaxPlayers );

		// Poll until enough players join or search is cancelled
		while ( _isSearching && !_cancelled )
		{
			var playerCount = Connection.All.Count();

			_lobbyCreatedCallback?.Invoke( playerCount, MaxPlayers );

			if ( playerCount >= MinPlayers )
			{
				Log.Info( $"MatchmakingSystem: {playerCount} players ready, starting match!" );
				LoadGameScene();
				_isSearching = false;
				return;
			}

			await Task.DelayRealtimeSeconds( 1f );
		}
	}

	private void DoCancel()
	{
		_cancelled = true;
		_isSearching = false;
		_isHost = false;
		_lobbyCreatedCallback = null;

		if ( Networking.IsActive )
		{
			Networking.Disconnect();
		}
	}

	private void LoadGameScene()
	{
		if ( GameScene is null )
		{
			GameScene = GameUtils.GetAvailableMaps().FirstOrDefault();
		}

		if ( GameScene is null )
		{
			Log.Warning( "MatchmakingSystem: No GameScene assigned and no available maps found." );
			return;
		}

		Log.Info( $"MatchmakingSystem: Loading game scene: {GameScene.ResourceName}" );
		Game.ActiveScene.Load( GameScene );
	}
}
