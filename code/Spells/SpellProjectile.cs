
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
			if ( PierceShield )
			{
				// Bônus de estado aplicado manualmente (bypassa TakeDamage)
				int dmg = ApplyStateBonuses( Damage, victim );
				int newHp = victim.Health - dmg;
				victim.Health = newHp < 0 ? 0 : newHp;
				if ( victim.Health <= 0 )
					victim.Die( _shooter );
			}
			else
			{
				victim.TakeDamage( Damage, _shooter );
			}

			// Stun → seta CombatState
			if ( StunDuration > 0f )
			{
				victim.StunEndTime = Time.Now + StunDuration;
				victim.SetCombatState( CombatState.Stunned, StunDuration );
			}

			// Lança ao ar (Stupefy T2)
			if ( LaunchAirborne )
				victim.LaunchIntoAir();

			// Burning DoT
			if ( BurningDPS > 0f && BurningDuration > 0f )
				victim.ApplyBurning( BurningDPS, BurningDuration, _shooter );

			// Slow
			if ( SlowFraction > 0f && SlowDuration > 0f )
			{
				victim.SlowEndTime = Time.Now + SlowDuration;
				victim.SlowFraction = SlowFraction;
			}

			// Mastery — usa o nome da spell de origem, não "SpellProjectile"
			if ( _shooter != null && !string.IsNullOrEmpty( SourceSpellClass ) )
			{
				var mastery = _shooter.Components.Get<MasterySystem>( FindMode.EverythingInSelf );
				mastery?.RegisterSpellHit( SourceSpellClass );
			}
		}

		PlayHitEffect( tr.EndPosition, tr.Normal );
	}

	/// <summary>Aplica bônus de dano pelo CombatState do alvo (+50% Airborne, +25% Stunned).</summary>
	private static int ApplyStateBonuses( int damage, PlayerPawn victim )
	{
		if ( victim.CombatState == CombatState.Airborne )
			return (int)(damage * 1.5f);
		if ( victim.CombatState == CombatState.Stunned )
			return (int)(damage * 1.25f);
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
