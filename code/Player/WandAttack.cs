
/// <summary>
/// Ataque básico da varinha: hitscan, 40 de dano, headshot 80, 3 tiros/segundo.
/// Adicione no mesmo GameObject do WizardPlayer.
/// </summary>
public sealed class WandAttack : Component
{
	[Property] public float FireRate { get; set; } = 3f;      // tiros por segundo
	[Property] public int BaseDamage { get; set; } = 40;
	[Property] public int HeadshotDamage { get; set; } = 80;
	[Property] public float Range { get; set; } = 5000f;

	private float _nextFireTime = 0f;
	private WizardPlayer _player;

	protected override void OnStart()
	{
		_player = Components.Get<WizardPlayer>();
	}

	public bool CanFire => _player.IsValid()
		&& _player.IsAlive
		&& !_player.IsStunned
		&& Time.Now >= _nextFireTime;

	public void TryFire()
	{
		if ( !CanFire ) return;
		_nextFireTime = Time.Now + 1f / FireRate;
		FireHitscan();
	}

	private void FireHitscan()
	{
		var origin = _player.EyePosition;
		var dir = _player.EyeAngles.ToRotation().Forward;

		// Visual beam (todos clientes)
		ShowBeam( origin, origin + dir * Range );

		if ( !Networking.IsHost ) return;

		var tr = Scene.Trace
			.Ray( origin, origin + dir * Range )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "projectile" )
			.Run();

		if ( !tr.Hit ) return;

		var victim = tr.GameObject?.Components.Get<WizardPlayer>();
		if ( victim == null || victim.Team == _player.Team ) return;

		bool headshot = tr.Hitbox?.Name?.Contains( "head", StringComparison.OrdinalIgnoreCase ) ?? false;
		int dmg = headshot ? HeadshotDamage : BaseDamage;

		victim.TakeDamage( dmg, _player, headshot );

		if ( headshot )
			GiveHeadshotGalleons();
	}

	private void GiveHeadshotGalleons()
	{
		// Kill galleons são dados pelo RoundManager via OnPlayerDied
	}

	[Broadcast]
	private void ShowBeam( Vector3 start, Vector3 end )
	{
		// TODO: spawnar partícula de feixe de varinha
		// Ex: SceneParticles.PlayInstant( "particles/wand_beam.vpcf", start );
	}
}
