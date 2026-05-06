
namespace Warlocks;

/// <summary>
/// Incendio: projétil de fogo com burning DoT.
/// T0: 60 dmg + 20 dps/3s  — single projectile
/// T1: 80 dmg + 25 dps/4s  — stronger burn
/// T2: 100 dmg + 30 dps/5s — explodes on impact (AoE burn to nearby enemies)
/// </summary>
public sealed class IncendioSpell : BaseSpell
{
	private static readonly int[]   DamageByTier  = { 60, 80, 100 };
	private static readonly float[] BurnDpsByTier  = { 20f, 25f, 30f };
	private static readonly float[] BurnDurByTier  = { 3f, 4f, 5f };

	// T2 AoE explosion
	private const float ExplosionRadius = 300f; // ~7.5m
	private const int   ExplosionDamage = 40;

	public override string  SpellName      => "Incendio";
	public override string Image => "ui/spells/incendio.png";
	public override float   ManaCost       => 25f;
	public override float   BaseCooldown   => 7f;
	public override float[] CooldownByTier => new[] { 7f, 6f, 5f };

	public override void Execute( Wand wand )
	{
		int t = Math.Clamp( Tier, 0, 2 );
		var damage = wand.ResolveDamageInt( DamageByTier[t] );
		var burnDps = wand.ResolveDamage( BurnDpsByTier[t] );
		var burnDuration = wand.ResolveDuration( BurnDurByTier[t] );

		var proj = wand.SpawnProjectile(
			sourceClass:  nameof( IncendioSpell ),
			speed:        1800f,
			damage:       damage,
			color:        new Color( 1f, 0.4f, 0.05f ),
			burnDPS:      burnDps,
			burnDuration: burnDuration
		);

		if ( Tier >= 2 )
		{
			var explosion = proj.GameObject.Components.Create<IncendioExplosionOnHit>();
			explosion.Shooter     = wand.Player;
			explosion.BurnDPS     = burnDps;
			explosion.BurnDuration = burnDuration;
			explosion.SplashDamage = wand.ResolveDamageInt( ExplosionDamage );
		}
	}
}

/// <summary>
/// Attached to the Incendio projectile at T2 to apply AoE burn on impact.
/// </summary>
public sealed class IncendioExplosionOnHit : Component
{
	private const float AoERadius = 300f;

	public PlayerPawn Shooter     { get; set; }
	public float      BurnDPS     { get; set; }
	public float      BurnDuration { get; set; }
	public int        SplashDamage { get; set; }

	private SpellProjectile _proj;

	protected override void OnStart()
	{
		_proj = Components.Get<SpellProjectile>( FindMode.EverythingInSelf );
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;
		if ( _proj == null ) return;
		// SpellProjectile destroys itself on hit; watch for the GO being destroyed
		if ( !GameObject.IsValid() ) return;
	}

	/// <summary>Called by the projectile system when this GO is about to be destroyed.</summary>
	protected override void OnDestroy()
	{
		if ( !Networking.IsHost || Shooter == null ) return;

		var hitPos = WorldPosition;
		foreach ( var p in Scene.GetAllComponents<PlayerPawn>() )
		{
			if ( p == null || !p.IsAlive || p.Team == Shooter.Team ) continue;
			if ( p.WorldPosition.Distance( hitPos ) > AoERadius ) continue;

			p.HealthComponent?.TakeDamage( new DamageInfo( Shooter, SplashDamage, Flags: DamageFlags.Burn ) );
			p.ApplyBurning( BurnDPS, BurnDuration, Shooter );
		}
	}
}
