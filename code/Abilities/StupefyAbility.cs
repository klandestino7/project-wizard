
/// <summary>
/// Q – Stupefy: projétil que causa dano, stun e (T2) lança ao ar.
/// Tier 0: 80 dmg, 1s stun              | Mana 20 | CD 6s
/// Tier 1: 120 dmg, 1.5s stun           | CD 5s   (400G)
/// Tier 2: 160 dmg, 2s stun, ignora shield + lança alvo no ar  (1000G)
/// </summary>
public sealed class StupefyAbility : BaseAbility
{
	private static readonly int[] DamageByTier = { 80, 120, 160 };
	private static readonly float[] StunByTier = { 1f, 1.5f, 2f };
	protected override float[] CooldownByTier => new[] { 6f, 5f, 4f };

	private int TierIndex => Math.Clamp( CurrentTier, 0, 2 );
	public int CurrentDamage => DamageByTier[TierIndex];
	public float CurrentStun => StunByTier[TierIndex];
	public bool PierceShield => CurrentTier >= 2;

	[Sync] private int _activateCount { get; set; } = 0;
	private int _lastActivateCount = 0;

	protected override void OnStart()
	{
		base.OnStart();
		AbilityName = "Stupefy";
		ManaCost = 20f;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Networking.IsHost && _activateCount != _lastActivateCount )
		{
			_lastActivateCount = _activateCount;
			if ( Player.IsValid() )
			{
				// Usa direção do LockOn se disponível
				var dir = Player.LockOnSystem?.GetAimDirection()
					?? Player.EyeAngles.ToRotation().Forward;
				SpawnProjectile( Player.EyePosition, dir );
			}
		}
	}

	protected override void Activate() => _activateCount++;

	private void SpawnProjectile( Vector3 origin, Vector3 direction )
	{
		var go = new GameObject( true, "Stupefy_Projectile" );
		go.WorldPosition = origin;
		go.WorldRotation = Rotation.LookAt( direction );
		go.Tags.Add( "projectile" );

		SpellProjectile.PendingShooter = Player;
		var proj = go.Components.Create<SpellProjectile>();
		proj.ShooterTeam = Player.Team;
		proj.Damage = CurrentDamage;
		proj.StunDuration = CurrentStun;
		proj.Speed = 2000f;
		proj.PierceShield = PierceShield;
		proj.LaunchAirborne = CurrentTier >= 2; // T2: lança ao ar após stun
		proj.SpellColor = new Color( 0.9f, 0.1f, 0.1f );
		proj.SourceSpellClass = nameof( StupefyAbility );

		go.NetworkSpawn();
	}
}
