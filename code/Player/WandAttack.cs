
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

	/// <summary>Prefab do efeito de feixe (instanciado no ponto de origem, orientado ao alvo).</summary>
	[Property, Group( "Particles" )] public GameObject BeamEffectPrefab { get; set; }
	/// <summary>Prefab do efeito de impacto (instanciado no ponto de hit).</summary>
	[Property, Group( "Particles" )] public GameObject HitEffectPrefab { get; set; }

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
		_player.WandHolder?.TriggerCastAnimation();
		FireHitscan();
	}

	private void FireHitscan()
	{
		var origin = _player.GetSpellMuzzle();
		var dir = _player.GetSpellDirection( origin );

		// 1. Fazemos o Trace PRIMEIRO para saber o destino exato
		var tr = Scene.Trace
			.Ray( origin, origin + dir * Range )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "projectile" )
			.Run();

		// 2. O destino visual será onde o raio parou (tr.EndPosition)
		// Se não bater em nada, ele vai até o limite do Range automaticamente
		ShowBeam( origin, tr.EndPosition );

		// Logica de Host/Dano
		if ( !Networking.IsHost ) return;
		if ( !tr.Hit ) return;

		var victim = tr.GameObject?.Components.Get<WizardPlayer>();
		if ( victim == null || victim.Team == _player.Team ) return;

		bool headshot = tr.Hitbox?.Tags.Has( "head" ) ?? false;
		int dmg = headshot ? HeadshotDamage : BaseDamage;

		victim.TakeDamage( dmg, _player, headshot );

		if ( headshot )
			GiveHeadshotGalleons();
	}

	private void GiveHeadshotGalleons()
	{
		// Kill galleons são dados pelo RoundManager via OnPlayerDied
	}

	[Rpc.Broadcast]
	private void ShowBeam( Vector3 start, Vector3 end )
	{
		var dir = (end - start).Normal;
		var distance = start.Distance( end );

		if ( BeamEffectPrefab is not null )
		{
			// Criamos o objeto visual
			var beam = BeamEffectPrefab.Clone();
			beam.WorldPosition = start;
			beam.WorldRotation = Rotation.LookAt( dir );

			// DICA: Se o seu prefab usa o componente 'SpellProjectile' para o visual:
			if ( beam.Components.TryGet<SpellProjectile>( out var visualProj ) )
			{
				visualProj.Speed = 10000f; // Muito rápido para parecer hitscan
										   // Desative o dano no componente visual se houver uma flag, 
										   // ou garanta que ele seja destruído ao chegar no 'end'
			}

			// Se for uma partícula de rastro (Trail), ela precisa de um tempo para sumir
			if ( !beam.Components.TryGet<AutoDestroy>( out _ ) )
				beam.Components.Create<AutoDestroy>().Delay = 1.0f;
		}

		// Efeito de impacto (só aparece se o ponto final não for o "vazio")
		if ( HitEffectPrefab is not null )
		{
			var fx = HitEffectPrefab.Clone();
			fx.WorldPosition = end;
			fx.WorldRotation = Rotation.LookAt( -dir );
		}
	}
}
