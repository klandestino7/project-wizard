
/// <summary>
/// F – Incendio: projétil de fogo com burning DoT.
/// Tier 0: 60 dmg + 20 dps por 3s  | Mana 25 | CD 7s  (400G / +500G / +900G)
/// Tier 1: 80 dmg + 25 dps por 4s
/// Tier 2: 100 dmg + 30 dps por 5s
/// </summary>
public sealed class IncendioAbility : BaseAbility
{
	private static readonly int[] DamageByTier = { 60, 80, 100 };
	private static readonly float[] BurnDpsByTier = { 20f, 25f, 30f };
	private static readonly float[] BurnDurationByTier = { 3f, 4f, 5f };
	protected override float[] CooldownByTier => new[] { 7f, 6f, 5f };

	[Sync] private int _activateCount { get; set; } = 0;
	private int _lastActivateCount = 0;

	protected override void OnStart()
	{
		base.OnStart();
		AbilityName = "Incendio";
		ManaCost = 25f;
		Tier1Cost = 500;
		Tier2Cost = 1400;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Networking.IsHost && _activateCount != _lastActivateCount )
		{
			_lastActivateCount = _activateCount;
			if ( Player.IsValid() )
				SpawnProjectile( Player.EyePosition, Player.EyeAngles.ToRotation().Forward );
		}
	}

	protected override void Activate() => _activateCount++;

	private void SpawnProjectile( Vector3 origin, Vector3 direction )
	{
		int t = Math.Clamp( CurrentTier, 0, 2 );
		var go = new GameObject( true, "Incendio_Projectile" );
		go.WorldPosition = origin;
		go.WorldRotation = Rotation.LookAt( direction );
		go.Tags.Add( "projectile" );

		SpellProjectile.PendingShooter = Player;
		var proj = go.Components.Create<SpellProjectile>();
		proj.ShooterTeam = Player.Team;
		proj.Damage = DamageByTier[t];
		proj.BurningDPS = BurnDpsByTier[t];
		proj.BurningDuration = BurnDurationByTier[t];
		proj.Speed = 1800f;
		proj.SpellColor = new Color( 1f, 0.4f, 0.05f );

		go.NetworkSpawn();
	}
}
