namespace Warlocks;

/// <summary>
/// Core match flow for the Warlocks round-based Horcrux mode.
/// </summary>
public sealed class RoundManager : Component
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<GameObject> AurorSpawns { get; set; } = new();
	[Property] public List<GameObject> ComensalSpawns { get; set; } = new();
	[Property] public float WarmupTime { get; set; } = 3f;
	[Property] public bool FreezePlayersOutsideCombat { get; set; } = true;

	public const int RoundsToWin = 13;
	public const int MaxRounds = 24;
	public const int FirstHalfRounds = 12;
	public const float BuyPhaseTime = 30f;
	public const float CombatPhaseTime = 90f;
	public const float PostRoundTime = 7f;

	public const int BaseRoundMoney = 800;
	public const int KillMoney = 200;
	public const int AssistMoney = 100;
	public const int PlantMoney = 300;
	public const int DefuseMoney = 300;
	public const int WinMoney = 3000;
	public const int LossMoney = 1900;
	public const int LossStreakBonus = 500;

	[Property, Sync] public int CurrentRound { get; set; }
	[Property, Sync] public int AurorScore { get; set; }
	[Property, Sync] public int ComensalScore { get; set; }
	[Property, Sync] public RoundState State { get; set; } = RoundState.Warmup;
	[Property, Sync] public float PhaseEndTime { get; set; }
	[Property, Sync] public Team LastRoundWinner { get; set; } = Team.Unassigned;
	[Property, Sync] public RoundEndReason LastRoundReason { get; set; } = RoundEndReason.TimeExpired;
	[Property, Sync] public string ActiveObjectiveSiteName { get; set; } = "";
	[Property, Sync] public bool IsOvertime { get; set; }

	private int _aurorLossStreak;
	private int _comensalLossStreak;
	private bool _sidesSwapped;
	private HorcruxSite _plantedSite;

	public static RoundManager Instance => Game.ActiveScene
		.GetAllComponents<RoundManager>()
		.FirstOrDefault();

	public bool HasPlantedHorcrux => _plantedSite.IsValid() && _plantedSite.IsPlanted && !_plantedSite.IsDefused && !_plantedSite.HasExploded;

	public float TimeRemaining
	{
		get
		{
			var remaining = PhaseEndTime - Time.Now;
			return remaining > 0f ? remaining : 0f;
		}
	}

	protected override void OnStart()
	{
		if ( !Networking.IsHost )
			return;

		State = RoundState.Warmup;
		PhaseEndTime = Time.Now + WarmupTime;
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( Time.Now < PhaseEndTime )
			return;

		switch ( State )
		{
			case RoundState.Warmup:
				StartNewRound();
				break;
			case RoundState.BuyPhase:
				StartCombat();
				break;
			case RoundState.Combat:
				EndRound( Team.Aurors, RoundEndReason.TimeExpired );
				break;
			case RoundState.PostRound:
				if ( !CheckMatchEnd() )
					StartNewRound();
				break;
		}
	}

	private void StartNewRound()
	{
		CurrentRound++;
		IsOvertime = CurrentRound > MaxRounds;
		_plantedSite = null;
		ActiveObjectiveSiteName = "";
		LastRoundWinner = Team.Unassigned;
		LastRoundReason = RoundEndReason.TimeExpired;

		if ( !_sidesSwapped && CurrentRound == FirstHalfRounds + 1 )
		{
			SwapTeams();
			_sidesSwapped = true;
		}

		foreach ( var site in Scene.GetAllComponents<HorcruxSite>() )
			site.Reset();

		RespawnAllPlayers();
		DistributeRoundStartMoney();
		SetFrozenState( true );

		State = RoundState.BuyPhase;
		PhaseEndTime = Time.Now + BuyPhaseTime;

		BroadcastRoundStart( CurrentRound );
	}

	private void StartCombat()
	{
		State = RoundState.Combat;
		PhaseEndTime = Time.Now + CombatPhaseTime;
		SetFrozenState( false );
		BroadcastCombatStart();
	}

	private void EndRound( Team winner, RoundEndReason reason )
	{
		if ( State != RoundState.Combat )
			return;

		State = RoundState.PostRound;
		PhaseEndTime = Time.Now + PostRoundTime;
		LastRoundWinner = winner;
		LastRoundReason = reason;
		SetFrozenState( true );

		if ( winner == Team.Aurors )
		{
			AurorScore++;
			_aurorLossStreak = 0;
			_comensalLossStreak++;
		}
		else
		{
			ComensalScore++;
			_comensalLossStreak = 0;
			_aurorLossStreak++;
		}

		DistributeEndMoney( winner );
		BroadcastRoundEnd( winner, reason );
	}

	private void RespawnAllPlayers()
	{
		RespawnTeam( GetPlayers( Team.Aurors ), ResolveSpawns( Team.Aurors, AurorSpawns ) );
		RespawnTeam( GetPlayers( Team.DarkFollowers ), ResolveSpawns( Team.DarkFollowers, ComensalSpawns ) );
	}

	private static void RespawnTeam( List<PlayerPawn> players, List<GameObject> spawns )
	{
		for ( var i = 0; i < players.Count; i++ )
		{
			var spawn = spawns.Count > 0 ? spawns[i % spawns.Count] : null;
			var spawnTransform = spawn?.Transform.World ?? new Transform( Vector3.Zero );
			players[i].SetSpawnPoint( new SpawnPointInfo( spawnTransform, Array.Empty<string>() ) );
			players[i].OnRespawn();
		}
	}

	private void DistributeRoundStartMoney()
	{
		foreach ( var player in GetAllPlayers() )
		{
			var bonus = 0;

			if ( player.Team == Team.Aurors && _aurorLossStreak >= 2 )
				bonus = LossStreakBonus;

			if ( player.Team == Team.DarkFollowers && _comensalLossStreak >= 2 )
				bonus = LossStreakBonus;

			// player.GiveGalleons( BaseRoundMoney + bonus );
		}
	}

	private void DistributeEndMoney( Team winner )
	{
		foreach ( var player in GetAllPlayers() )
		{
			// player.GiveGalleons( player.Team == winner ? WinMoney : LossMoney );
		}
	}

	public void OnPlayerDied( PlayerPawn victim, PlayerPawn killer )
	{
		if ( !Networking.IsHost || State != RoundState.Combat )
			return;

		if ( killer != null && killer.Team != victim.Team )
		{
			if ( killer.Client is not null )
				killer.Client.Balance += KillMoney;

			killer.PassiveEffects?.OnKill();
			killer.UltimateCharge?.AddKillCharge();
		}

		CheckRoundEndConditions();
	}

	public bool CanPlantHorcruxAt( HorcruxSite site, PlayerPawn player )
	{
		if ( State != RoundState.Combat )
			return false;

		if ( player?.Team != Team.DarkFollowers )
			return false;

		if ( HasPlantedHorcrux )
			return false;

		return site.IsValid() && !site.IsPlanted && !site.IsDefused && !site.HasExploded;
	}

	public bool CanDefuseHorcruxAt( HorcruxSite site, PlayerPawn player )
	{
		if ( State != RoundState.Combat )
			return false;

		if ( player?.Team != Team.Aurors )
			return false;

		return site.IsValid() && site == _plantedSite && site.IsPlanted && !site.IsDefused && !site.HasExploded;
	}

	public void OnHorcruxPlanted( HorcruxSite site )
	{
		if ( !Networking.IsHost || !site.IsValid() )
			return;

		_plantedSite = site;
		ActiveObjectiveSiteName = site.SiteName;
		PhaseEndTime = Time.Now + site.ExplosionDelay;
	}

	public void OnHorcruxExploded( HorcruxSite site )
	{
		if ( !Networking.IsHost || site != _plantedSite )
			return;

		EndRound( Team.DarkFollowers, RoundEndReason.HorcruxExploded );
	}

	public void OnHorcruxDefused( HorcruxSite site )
	{
		if ( !Networking.IsHost || site != _plantedSite )
			return;

		EndRound( Team.Aurors, RoundEndReason.HorcruxDefused );
	}

	private void CheckRoundEndConditions()
	{
		if ( GetAlivePlayers( Team.DarkFollowers ).Count == 0 && !HasPlantedHorcrux )
		{
			EndRound( Team.Aurors, RoundEndReason.AttackersEliminated );
			return;
		}

		if ( GetAlivePlayers( Team.Aurors ).Count == 0 )
			EndRound( Team.DarkFollowers, RoundEndReason.DefendersEliminated );
	}

	private bool CheckMatchEnd()
	{
		if ( !IsOvertime )
		{
			if ( AurorScore >= RoundsToWin )
			{
				EndMatch( Team.Aurors );
				return true;
			}

			if ( ComensalScore >= RoundsToWin )
			{
				EndMatch( Team.DarkFollowers );
				return true;
			}
		}
		else
		{
			if ( AurorScore >= RoundsToWin && AurorScore - ComensalScore >= 2 )
			{
				EndMatch( Team.Aurors );
				return true;
			}

			if ( ComensalScore >= RoundsToWin && ComensalScore - AurorScore >= 2 )
			{
				EndMatch( Team.DarkFollowers );
				return true;
			}
		}

		if ( CurrentRound == MaxRounds && AurorScore != ComensalScore )
		{
			EndMatch( AurorScore > ComensalScore ? Team.Aurors : Team.DarkFollowers );
			return true;
		}

		return false;
	}

	private void EndMatch( Team winner )
	{
		State = RoundState.MatchEnd;
		SetFrozenState( true );
		BroadcastMatchEnd( winner );
	}

	private void SwapTeams()
	{
		foreach ( var player in GetAllPlayers() )
		{
			player.Team = player.Team == Team.Aurors ? Team.DarkFollowers : Team.Aurors;
		}
	}

	private void SetFrozenState( bool frozen )
	{
		if ( !FreezePlayersOutsideCombat )
			return;

		foreach ( var player in GetAllPlayers() )
			player.IsFrozen = frozen;
	}

	private List<GameObject> ResolveSpawns( Team team, List<GameObject> configuredSpawns )
	{
		var resolved = configuredSpawns?
			.Where( spawn => spawn.IsValid() )
			.ToList() ?? new List<GameObject>();

		if ( resolved.Count > 0 )
			return resolved;

		resolved = Scene.GetAllComponents<TeamSpawnPoint>()
			.Where( spawn => spawn.Team == team )
			.Select( spawn => spawn.GameObject )
			.Where( gameObject => gameObject.IsValid() )
			.ToList();

		if ( resolved.Count > 0 )
			return resolved;

		return Scene.GetAllComponents<SpawnPoint>()
			.Select( spawn => spawn.GameObject )
			.Where( gameObject => gameObject.IsValid() )
			.ToList();
	}

	private List<PlayerPawn> GetAllPlayers() =>
		Scene.GetAllComponents<PlayerPawn>().ToList();

	private List<PlayerPawn> GetPlayers( Team team ) =>
		Scene.GetAllComponents<PlayerPawn>()
			.Where( player => player.Team == team )
			.ToList();

	private List<PlayerPawn> GetAlivePlayers( Team team ) =>
		Scene.GetAllComponents<PlayerPawn>()
			.Where( player => player.Team == team && player.IsAlive )
			.ToList();

	// public void OnActive( Connection connection )
	// {
	// 	if ( !Networking.IsHost || PlayerPrefab == null )
	// 		return;

	// 	var aurors = GetAllPlayers().Count( player => player.Team == Team.Aurors );
	// 	var comensais = GetAllPlayers().Count( player => player.Team == Team.DarkFollowers );
	// 	var newTeam = aurors <= comensais ? Team.Aurors : Team.DarkFollowers;

	// 	var spawns = ResolveSpawns( newTeam, newTeam == Team.Aurors ? AurorSpawns : ComensalSpawns );
	// 	var spawnPosition = spawns.Count > 0
	// 		? spawns[Game.Random.Int( spawns.Count - 1 )].WorldPosition
	// 		: Vector3.Zero;

	// 	var gameObject = PlayerPrefab.Clone( spawnPosition );
	// 	var player = gameObject.Components.Get<PlayerPawn>( FindMode.EverythingInSelfAndDescendants );

	// 	if ( player != null )
	// 		player.Team = newTeam;

	// 	gameObject.NetworkSpawn( connection );
	// }

	// public void OnDisconnected( Connection connection )
	// {
	// }

	[Rpc.Broadcast]
	private void BroadcastRoundStart( int round )
	{
		Log.Info( $"[Warlocks] Round {round} start. Overtime={IsOvertime}" );
	}

	[Rpc.Broadcast]
	private void BroadcastCombatStart()
	{
		Log.Info( $"[Warlocks] Round {CurrentRound} combat live." );
	}

	[Rpc.Broadcast]
	private void BroadcastRoundEnd( Team winner, RoundEndReason reason )
	{
		Log.Info( $"[Warlocks] Round {CurrentRound} winner={winner} reason={reason}" );
	}

	[Rpc.Broadcast]
	private void BroadcastMatchEnd( Team winner )
	{
		Log.Info( $"[Warlocks] Match ended. Winner={winner}" );
	}
}
