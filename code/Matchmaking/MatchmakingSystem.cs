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
	/// Tag used to identify Warlocks matchmaking lobbies.
	/// </summary>
	private const string MatchTag = "warlocks-match";

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
	private bool _isHost = false;
	private Action<int, int> _lobbyCreatedCallback;

	/// <summary>
	/// Finds an available match or creates a new lobby and waits.
	/// </summary>
	public static async Task FindOrCreateMatch(
		Action<Lobby> onMatchFound,
		Action<int, int> onLobbyCreated,
		CancellationToken cancellationToken = default )
	{
		if ( !Instance.IsValid() )
		{
			Log.Warning( "MatchmakingSystem: No instance found in scene." );
			return;
		}

		await Instance.RunMatchmaking( onMatchFound, onLobbyCreated, cancellationToken );
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
		Action<Lobby> onMatchFound,
		Action<int, int> onLobbyCreated,
		CancellationToken cancellationToken )
	{
		_isSearching = true;
		_isHost = false;

		IsMultiplayerSession = true;

		// Step 1: Query existing lobbies for an available Warlocks match
		Log.Info( "MatchmakingSystem: Searching for available matches..." );

		Lobby? foundLobby = null;

		try
		{
			// NOTE: If Networking.QueryLobbies() does not exist, replace with the
			// correct S&Box lobby query API (e.g. Sandbox.Services.Lobbies.GetAsync()).
			var lobbies = await Networking.QueryLobbies();

			foundLobby = lobbies
				.Where( l => l.GetData( "type" ) == MatchTag && !l.IsFull )
				.OrderByDescending( l => l.MemberCount )
				.FirstOrDefault();
		}
		catch ( Exception e )
		{
			Log.Warning( $"MatchmakingSystem: Lobby query failed: {e.Message}" );
		}

		cancellationToken.ThrowIfCancellationRequested();

		// Step 2a: Found an existing lobby — join it
		if ( foundLobby.HasValue )
		{
			Log.Info( $"MatchmakingSystem: Found a lobby, joining..." );
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

		cancellationToken.ThrowIfCancellationRequested();

		// Step 2b: No lobby found — create one and wait for players
		Log.Info( "MatchmakingSystem: No lobby found, creating a new one..." );
		_isHost = true;
		_lobbyCreatedCallback = onLobbyCreated;

		Networking.CreateLobby( new LobbyConfig
		{
			MaxPlayers = MaxPlayers,
		} );

		// Set lobby metadata so others can find and filter it
		Networking.SetData( "type", MatchTag );

		onLobbyCreated?.Invoke( 1, MaxPlayers );

		// Wait until enough players have joined or cancellation is requested
		while ( _isSearching && !cancellationToken.IsCancellationRequested )
		{
			var playerCount = Connection.All.Count();

			// Update host callback with current count
			_lobbyCreatedCallback?.Invoke( playerCount, MaxPlayers );

			if ( playerCount >= MinPlayers )
			{
				Log.Info( $"MatchmakingSystem: {playerCount} players ready, starting match!" );
				LoadGameScene();
				_isSearching = false;
				return;
			}

			// Poll every second
			await Task.DelayRealtimeSeconds( 1f );
		}

		cancellationToken.ThrowIfCancellationRequested();
	}

	private void DoCancel()
	{
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
			// Fallback: find any scene tagged as visible in menu
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
