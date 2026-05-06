namespace Warlocks;

/// <summary>
/// Horcrux plant / defuse objective used by the Warlocks main mode.
/// </summary>
public sealed class HorcruxSite : Component
{
	[Property] public string SiteName { get; set; } = "A";
	[Property] public float PlantTime { get; set; } = 3f;
	[Property] public float DefuseTime { get; set; } = 7f;
	[Property] public float ExplosionDelay { get; set; } = 45f;
	[Property] public float InteractDistance { get; set; } = 150f;
	[Property] public bool EnabledForRound { get; set; } = true;

	[Sync] public bool IsPlanted { get; private set; }
	[Sync] public bool IsDefused { get; private set; }
	[Sync] public bool HasExploded { get; private set; }
	[Sync] public float ExplosionTime { get; private set; }
	[Sync] public float InteractProgress { get; private set; }
	[Sync] public bool IsBeingInteracted { get; private set; }
	[Sync] public Team InteractingTeam { get; private set; } = Team.Unassigned;

	private PlayerPawn _interactingPlayer;
	private float _interactStartTime;

	public float TimeUntilExplosion => IsPlanted
		? Math.Max( 0f, (float)(ExplosionTime - Time.Now) )
		: 0f;

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( IsPlanted && !IsDefused && !HasExploded && Time.Now >= ExplosionTime )
		{
			Explode();
			return;
		}

		if ( !IsBeingInteracted )
			return;

		if ( !_interactingPlayer.IsValid() || !_interactingPlayer.IsAlive )
		{
			CancelInteraction();
			return;
		}

		if ( _interactingPlayer.WorldPosition.Distance( WorldPosition ) > InteractDistance )
		{
			CancelInteraction();
			return;
		}

		var duration = IsPlanted ? DefuseTime : PlantTime;
		InteractProgress = (Time.Now - _interactStartTime) / duration;

		if ( InteractProgress >= 1f )
		{
			InteractProgress = 1f;
			CompleteInteraction();
		}
	}

	public bool CanInteract( PlayerPawn player )
	{
		if ( !EnabledForRound || player == null )
			return false;

		if ( IsDefused || HasExploded )
			return false;

		var round = RoundManager.Instance;
		if ( !round.IsValid() )
			return false;

		if ( !IsPlanted )
			return round.CanPlantHorcruxAt( this, player );

		return round.CanDefuseHorcruxAt( this, player );
	}

	public void TryInteract( PlayerPawn player )
	{
		if ( !Networking.IsHost || !CanInteract( player ) )
			return;

		if ( IsBeingInteracted && _interactingPlayer != player )
			return;

		if ( !IsBeingInteracted )
			StartInteraction( player );
	}

	public void StopInteract( PlayerPawn player )
	{
		if ( _interactingPlayer == player )
			CancelInteraction();
	}

	public new void Reset()
	{
		IsPlanted = false;
		IsDefused = false;
		HasExploded = false;
		ExplosionTime = 0f;
		InteractProgress = 0f;
		IsBeingInteracted = false;
		InteractingTeam = Team.Unassigned;
		_interactingPlayer = null;
	}

	private void StartInteraction( PlayerPawn player )
	{
		IsBeingInteracted = true;
		InteractingTeam = player.Team;
		_interactingPlayer = player;
		_interactStartTime = Time.Now;
		InteractProgress = 0f;
	}

	private void CancelInteraction()
	{
		IsBeingInteracted = false;
		InteractingTeam = Team.Unassigned;
		_interactingPlayer = null;
		InteractProgress = 0f;
	}

	private void CompleteInteraction()
	{
		IsBeingInteracted = false;
		InteractingTeam = Team.Unassigned;

		if ( !IsPlanted )
			Plant( _interactingPlayer );
		else
			Defuse( _interactingPlayer );

		_interactingPlayer = null;
	}

	private void Plant( PlayerPawn planter )
	{
		IsPlanted = true;
		ExplosionTime = Time.Now + ExplosionDelay;

		planter?.GiveGalleons( RoundManager.PlantMoney );

		var round = RoundManager.Instance;
		round?.OnHorcruxPlanted( this );

		BroadcastPlant( SiteName );
		Log.Info( $"[Warlocks] Horcrux planted at site {SiteName}." );
	}

	private void Defuse( PlayerPawn defuser )
	{
		IsDefused = true;

		defuser?.GiveGalleons( RoundManager.DefuseMoney );

		var round = RoundManager.Instance;
		round?.OnHorcruxDefused( this );

		BroadcastDefuse( SiteName );
		Log.Info( $"[Warlocks] Horcrux defused at site {SiteName}." );
	}

	private void Explode()
	{
		HasExploded = true;

		var round = RoundManager.Instance;
		round?.OnHorcruxExploded( this );

		BroadcastExplosion( WorldPosition );
		Log.Info( $"[Warlocks] Horcrux exploded at site {SiteName}." );
	}

	[Rpc.Broadcast]
	private void BroadcastPlant( string site )
	{
	}

	[Rpc.Broadcast]
	private void BroadcastDefuse( string site )
	{
	}

	[Rpc.Broadcast]
	private void BroadcastExplosion( Vector3 position )
	{
	}
}
