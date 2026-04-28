
/// <summary>
/// Q – Stupefy: projétil que causa dano e stun.
/// Tier 0: 80 dmg, 1s stun, 6s CD
/// Tier 1: 120 dmg, 1.5s stun, 5s CD  (400G)
/// Tier 2: 160 dmg, 2s stun, 4s CD + ignora shield  (1000G)
/// </summary>
public sealed class StupefyAbility : BaseAbility
{
	private static readonly int[] DamageByTier = { 80, 120, 160 };
	private static readonly float[] StunByTier = { 1f, 1.5f, 2f };
	protected override float[] CooldownByTier => new[] { 6f, 5f, 4f };

	private int TierIndex => CurrentTier < 0 ? 0 : CurrentTier > 2 ? 2 : CurrentTier;
	public int CurrentDamage => DamageByTier[TierIndex];
	public float CurrentStun => StunByTier[TierIndex];
	public bool PierceShield => CurrentTier >= 2;

	protected override void Activate()
	{
		// Broadcast para todos os peers; apenas o host executa o spawn real.
		BroadcastActivate( Player.EyePosition, Player.EyeAngles.ToRotation() );
	}

	[Broadcast]
	private void BroadcastActivate( Vector3 origin, Rotation dir )
	{
		if ( !Networking.IsHost ) return;
		SpawnProjectile( origin, dir.Forward );
	}

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
		proj.SpellColor = new Color( 0.9f, 0.1f, 0.1f );

		go.NetworkSpawn();
	}
}
