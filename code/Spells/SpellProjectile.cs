
/// <summary>
/// Projétil genérico de feitiço. Criado pelo servidor, posição sincronizada
/// automaticamente pelo networking do S&Box.
/// Antes de NetworkSpawn(), setar SpellProjectile.PendingShooter com o atirador.
/// </summary>
public sealed class SpellProjectile : Component
{
	// ─── Propriedades sincronizadas ───────────────────────────────────
	[Property, Sync] public Team ShooterTeam { get; set; }
	[Property, Sync] public int Damage { get; set; } = 80;
	[Property, Sync] public float StunDuration { get; set; } = 0f;
	[Property, Sync] public float Speed { get; set; } = 2000f;
	[Property, Sync] public bool PierceShield { get; set; } = false;
	[Property, Sync] public Color SpellColor { get; set; } = Color.Red;

	/// <summary>Se true, lança a vítima no ar após aplicar stun (Stupefy T2).</summary>
	[Property, Sync] public bool LaunchAirborne { get; set; } = false;

	// ─── Burning DoT (Incendio) ───────────────────────────────────────
	[Property, Sync] public float BurningDPS { get; set; } = 0f;
	[Property, Sync] public float BurningDuration { get; set; } = 0f;

	// ─── Slow (Impedimenta) ───────────────────────────────────────────
	[Property, Sync] public float SlowFraction { get; set; } = 0f;
	[Property, Sync] public float SlowDuration { get; set; } = 0f;

	/// <summary>Nome da classe da ability de origem — usado pelo MasterySystem e SpellEffectsLibrary.</summary>
	[Property, Sync] public string SourceSpellClass { get; set; } = "";

	/// <summary>Posição do muzzle no momento do cast — usada pelo SpellLineTrail em todos os clientes.</summary>
	[Sync] public Vector3 SpawnOrigin { get; set; }

	/// <summary>Bridge server-side: setar antes de NetworkSpawn().</summary>
	public static PlayerPawn PendingShooter { get; set; }

	private const float Lifetime = 3f;

	private PlayerPawn _shooter;
	private float _spawnTime;
	private bool _hit = false;

	protected override void OnStart()
	{
		_spawnTime = Time.Now;

		if ( Networking.IsHost )
		{
			_shooter = PendingShooter;
			PendingShooter = null;
		}

		var light = Components.Create<PointLight>();
		light.LightColor = SpellColor;
		light.Radius = 80f;
		light.Enabled = true;

		// Trail de partícula — roda em todos os clientes (OnStart não filtra IsHost)
		var lib = SpellEffectsLibrary.Get( Scene );
		var trailPrefab = lib?.GetTrailPrefab( SourceSpellClass );
		if ( trailPrefab is not null )
		{
			var trail = trailPrefab.Clone();
			trail.Parent = GameObject;
			trail.LocalPosition = Vector3.Zero;
			trail.LocalRotation = Rotation.Identity;

			var lineTrail = trail.Components.Get<SpellLineTrail>( FindMode.EverythingInSelfAndDescendants );
			lineTrail?.Setup( SpawnOrigin, SpellColor );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost || _hit ) return;

		if ( Time.Now - _spawnTime > Lifetime )
		{
			GameObject.Destroy();
			return;
		}

		var step = WorldRotation.Forward * Speed * Time.Delta;

		var tr = Scene.Trace
			.Ray( WorldPosition, WorldPosition + step )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( _shooter?.GameObject )
			.WithoutTags( "projectile" )
			.Run();

		if ( tr.Hit )
		{
			_hit = true;
			OnHit( tr );
			GameObject.Destroy();
			return;
		}

		WorldPosition += step;
	}

	private void OnHit( SceneTraceResult tr )
	{
		var victim = tr.GameObject?.Components.Get<PlayerPawn>();

		if ( victim != null && victim.Team != ShooterTeam )
		{
			var finalDamage = ApplyStateBonuses( Damage, victim );

			// Protego absorve dano antes do HealthComponent (exceto PierceShield)
			if ( !PierceShield )
			{
				var protego = victim.Components.Get<ProtegoComponent>( FindMode.EverythingInSelf );
				if ( protego != null && protego.IsShieldUp )
				{
					var (passthrough, reflected) = protego.AbsorbDamage( (int)finalDamage, _shooter );
					if ( reflected > 0 && _shooter != null )
						_shooter.HealthComponent.TakeDamage( new Warlocks.DamageInfo( victim, reflected, Position: tr.EndPosition ) );
					finalDamage = passthrough;
				}
			}

			if ( finalDamage > 0f )
				victim.HealthComponent.TakeDamage( new Warlocks.DamageInfo( _shooter, finalDamage, Position: tr.EndPosition ) );

			if ( StunDuration > 0f )
			{
				victim.StunEndTime = Time.Now + StunDuration;
				victim.SetCombatState( CombatState.Stunned, StunDuration );
			}

			if ( LaunchAirborne )
				victim.LaunchIntoAir();

			if ( BurningDPS > 0f && BurningDuration > 0f )
				victim.ApplyBurning( BurningDPS, BurningDuration, _shooter );

			if ( SlowFraction > 0f && SlowDuration > 0f )
			{
				victim.SlowEndTime = Time.Now + SlowDuration;
				victim.SlowFraction = SlowFraction;
			}

			if ( _shooter != null && !string.IsNullOrEmpty( SourceSpellClass ) )
			{
				var mastery = _shooter.Components.Get<MasterySystem>( FindMode.EverythingInSelf );
				mastery?.RegisterSpellHit( SourceSpellClass );
			}
		}

		PlayHitEffect( tr.EndPosition, tr.Normal );
	}

	/// <summary>Aplica bônus de dano pelo CombatState do alvo (+50% Airborne, +25% Stunned).</summary>
	private static float ApplyStateBonuses( float damage, PlayerPawn victim )
	{
		if ( victim.CombatState == CombatState.Airborne ) return damage * 1.5f;
		if ( victim.CombatState == CombatState.Stunned ) return damage * 1.25f;
		return damage;
	}

	[Rpc.Broadcast]
	private void PlayHitEffect( Vector3 position, Vector3 normal )
	{
		var lib = SpellEffectsLibrary.Get( Scene );
		var hitPrefab = lib?.GetHitPrefab( SourceSpellClass );
		if ( hitPrefab is null ) return;

		var fx = hitPrefab.Clone();
		fx.WorldPosition = position;
		fx.WorldRotation = Rotation.LookAt( normal );

		if ( !fx.Components.TryGet<AutoDestroy>( out _ ) )
			fx.Components.Create<AutoDestroy>();
	}
}
