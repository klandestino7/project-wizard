/// <summary>
/// Gerencia rounds, economia e vitória da partida.
/// Adicione em um GameObject vazio na cena. Requer referências a SpawnPoints
/// e ao prefab de jogador configuradas no inspetor.
/// </summary>
public sealed class RoundManager : Component, Component.INetworkListener
{
	// ─── Configuração ─────────────────────────────────────────────────
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<GameObject> AurorSpawns { get; set; } = new();
	[Property] public List<GameObject> ComensalSpawns { get; set; } = new();

	// ─── Constantes de partida ────────────────────────────────────────
	public const int RoundsToWin = 13;
	public const int MaxRounds = 24;
	public const float BuyPhaseTime = 30f;
	public const float CombatPhaseTime = 90f;
	public const float PostRoundTime = 7f;

	// ─── Constantes de economia ───────────────────────────────────────
	public const int BaseRoundMoney = 800;
	public const int KillMoney = 200;
	public const int WinMoney = 3000;
	public const int LossMoney = 1900;
	public const int LossStreakBonus = 500;

	// ─── Estado sincronizado ──────────────────────────────────────────
	[Property, Sync] public int CurrentRound { get; set; } = 0;
	[Property, Sync] public int AurorScore { get; set; } = 0;
	[Property, Sync] public int ComensalScore { get; set; } = 0;
	[Property, Sync] public RoundState State { get; set; } = RoundState.Warmup;
	[Property, Sync] public float PhaseEndTime { get; set; } = 0f;

	public float TimeRemaining
	{
		get
		{
			float r = PhaseEndTime - Time.Now;
			return r > 0f ? r : 0f;
		}
	}

	// ─── Estado interno (servidor) ────────────────────────────────────
	private int _aurorLossStreak = 0;
	private int _comensalLossStreak = 0;
	private bool _horcruxPlanted = false;
	// ─── Singleton ────────────────────────────────────────────────────
	public static RoundManager Instance => Game.ActiveScene
		.GetAllComponents<RoundManager>().FirstOrDefault();

	// ─── Lifecycle ────────────────────────────────────────────────────
	protected override void OnStart()
	{
		if ( !Networking.IsHost ) return;
		State = RoundState.Warmup;
		PhaseEndTime = Time.Now + 3f; // pequeno delay antes do primeiro round
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;
		TickState();
	}

	// ─── Máquina de estados ───────────────────────────────────────────
	private void TickState()
	{
		if ( Time.Now < PhaseEndTime ) return;

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

	// ─── Fases ────────────────────────────────────────────────────────
	private void StartNewRound()
	{
		CurrentRound++;
		_horcruxPlanted = false;

		if ( CurrentRound == 13 )
			SwapTeams();

		RespawnAllPlayers();
		DistributeRoundStartMoney();

		State = RoundState.BuyPhase;
		PhaseEndTime = Time.Now + BuyPhaseTime;

		BroadcastRoundStart( CurrentRound );

		foreach ( var site in Scene.GetAllComponents<HorcruxSite>() )
			site.Reset();
	}

	private void StartCombat()
	{
		State = RoundState.Combat;
		PhaseEndTime = Time.Now + CombatPhaseTime;
		BroadcastCombatStart();
	}

	private void EndRound( Team winner, RoundEndReason reason )
	{
		if ( State != RoundState.Combat ) return;

		State = RoundState.PostRound;
		PhaseEndTime = Time.Now + PostRoundTime;

		if ( winner == Team.Aurors )
		{
			AurorScore++;
			_aurorLossStreak = 0;
			_comensalLossStreak++;
			DistributeEndMoney( Team.Aurors );
		}
		else
		{
			ComensalScore++;
			_comensalLossStreak = 0;
			_aurorLossStreak++;
			DistributeEndMoney( Team.DarkFollowers );
		}

		BroadcastRoundEnd( winner, reason );
	}

	// ─── Spawning ─────────────────────────────────────────────────────
	private void RespawnAllPlayers()
	{
		RespawnTeam( GetAlivePlayers( Team.Aurors ), AurorSpawns );
		RespawnTeam( GetAlivePlayers( Team.DarkFollowers ), ComensalSpawns );
	}

	private void RespawnTeam( List<WizardPlayer> players, List<GameObject> spawns )
	{
		for ( int i = 0; i < players.Count; i++ )
		{
			var sp = spawns.Count > 0 ? spawns[i % spawns.Count] : null;
			players[i].Respawn( sp != null ? sp.WorldPosition : Vector3.Zero );
		}
	}

	// ─── Economia ─────────────────────────────────────────────────────
	private void DistributeRoundStartMoney()
	{
		foreach ( var p in GetAllPlayers() )
		{
			int bonus = 0;
			if ( p.Team == Team.Aurors && _aurorLossStreak >= 2 ) bonus = LossStreakBonus;
			if ( p.Team == Team.DarkFollowers && _comensalLossStreak >= 2 ) bonus = LossStreakBonus;
			p.GiveGalleons( BaseRoundMoney + bonus );
		}
	}

	private void DistributeEndMoney( Team winner )
	{
		foreach ( var p in GetAllPlayers() )
			p.GiveGalleons( p.Team == winner ? WinMoney : LossMoney );
	}

	// ─── Eventos de round ─────────────────────────────────────────────
	public void OnPlayerDied( WizardPlayer victim, WizardPlayer killer )
	{
		if ( !Networking.IsHost || State != RoundState.Combat ) return;
		if ( killer != null && killer.Team != victim.Team )
			killer.GiveGalleons( KillMoney );
		CheckRoundEndConditions();
	}

	public void OnHorcruxPlanted()
	{
		if ( !Networking.IsHost ) return;
		_horcruxPlanted = true;
		PhaseEndTime = Time.Now + CombatPhaseTime;
	}

	public void OnHorcruxExploded()
	{
		if ( !Networking.IsHost ) return;
		EndRound( Team.DarkFollowers, RoundEndReason.HorcruxExploded );
	}

	public void OnHorcruxDefused()
	{
		if ( !Networking.IsHost ) return;
		EndRound( Team.Aurors, RoundEndReason.HorcruxDefused );
	}

	private void CheckRoundEndConditions()
	{
		if ( GetAlivePlayers( Team.DarkFollowers ).Count == 0 && !_horcruxPlanted )
		{
			EndRound( Team.Aurors, RoundEndReason.AttackersEliminated );
			return;
		}
		if ( GetAlivePlayers( Team.Aurors ).Count == 0 )
			EndRound( Team.DarkFollowers, RoundEndReason.DefendersEliminated );
	}

	// ─── Fim de partida ───────────────────────────────────────────────
	private bool CheckMatchEnd()
	{
		if ( AurorScore >= RoundsToWin ) { EndMatch( Team.Aurors ); return true; }
		if ( ComensalScore >= RoundsToWin ) { EndMatch( Team.DarkFollowers ); return true; }
		if ( CurrentRound >= MaxRounds && AurorScore != ComensalScore )
		{
			EndMatch( AurorScore > ComensalScore ? Team.Aurors : Team.DarkFollowers );
			return true;
		}
		return false;
	}

	private void EndMatch( Team winner )
	{
		State = RoundState.MatchEnd;
		BroadcastMatchEnd( winner );
	}

	// ─── Side swap ────────────────────────────────────────────────────
	private void SwapTeams()
	{
		foreach ( var p in GetAllPlayers() )
			p.Team = p.Team == Team.Aurors ? Team.DarkFollowers : Team.Aurors;
	}

	// ─── Helpers ──────────────────────────────────────────────────────
	private List<WizardPlayer> GetAllPlayers() =>
		Scene.GetAllComponents<WizardPlayer>().ToList();

	private List<WizardPlayer> GetAlivePlayers( Team team ) =>
		Scene.GetAllComponents<WizardPlayer>()
			.Where( p => p.Team == team && p.IsAlive )
			.ToList();

	// ─── INetworkListener ─────────────────────────────────────────────
	public void OnActive( Connection connection )
	{
		if ( !Networking.IsHost || PlayerPrefab == null ) return;

		int aurors = GetAllPlayers().Count( p => p.Team == Team.Aurors );
		int comensais = GetAllPlayers().Count( p => p.Team == Team.DarkFollowers );
		var newTeam = aurors <= comensais ? Team.Aurors : Team.DarkFollowers;

		var spawnList = newTeam == Team.Aurors ? AurorSpawns : ComensalSpawns;
		var spawnPos = spawnList.Count > 0
			? spawnList[Game.Random.Int( spawnList.Count - 1 )].WorldPosition
			: Vector3.Zero;

		var go = PlayerPrefab.Clone( spawnPos );
		var player = go.Components.Get<WizardPlayer>( FindMode.EverythingInSelfAndDescendants );
		if ( player != null ) player.Team = newTeam;
		go.NetworkSpawn( connection );
	}

	public void OnDisconnected( Connection connection ) { }

	// ─── Broadcasts (sem parâmetros struct) ───────────────────────────
	[Rpc.Broadcast]
	private void BroadcastRoundStart( int round )
	{
		Log.Info( $"[Round {round}] BuyPhase" );
	}

	[Rpc.Broadcast]
	private void BroadcastCombatStart()
	{
		Log.Info( $"[Round {CurrentRound}] Combate!" );
	}

	[Rpc.Broadcast]
	private void BroadcastRoundEnd( Team winner, RoundEndReason reason )
	{
		Log.Info( $"[Round {CurrentRound}] {winner} venceu - {reason}" );
	}

	[Rpc.Broadcast]
	private void BroadcastMatchEnd( Team winner )
	{
		Log.Info( $"[Partida] {winner} venceu!" );
	}
}
