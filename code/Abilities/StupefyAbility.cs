
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

	public int CurrentDamage => DamageByTier[Math.Clamp( CurrentTier, 0, 2 )];
	public float CurrentStun => StunByTier[Math.Clamp( CurrentTier, 0, 2 )];
	public bool PierceShield => CurrentTier >= 2;

	protected override void Activate()
	{
		if ( Networking.IsHost )
		{
			SpawnProjectile();
		}
		else
		{
			RequestSpawnProjectile();
		}
	}

	[Authority]
	private void RequestSpawnProjectile()
	{
		SpawnProjectile();
	}

	private void SpawnProjectile()
	{
		var origin = Player.EyePosition;
		var direction = Player.EyeAngles.ToRotation().Forward;

		var go = new GameObject( true, "Stupefy_Projectile" );
		go.Transform.Position = origin;
		go.Transform.Rotation = Rotation.LookAt( direction );
		go.Tags.Add( "projectile" );

		var proj = go.Components.Create<SpellProjectile>();
		proj.ShooterNetId = Player.Network.OwnerId;
		proj.Damage = CurrentDamage;
		proj.StunDuration = CurrentStun;
		proj.Speed = 2000f;
		proj.PierceShield = PierceShield;
		proj.SpellColor = new Color( 0.9f, 0.1f, 0.1f ); // Vermelho

		go.NetworkSpawn();
	}
}
