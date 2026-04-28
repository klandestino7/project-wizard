
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

	// ─── Burning DoT (Incendio) ───────────────────────────────────────
	[Property, Sync] public float BurningDPS { get; set; } = 0f;
	[Property, Sync] public float BurningDuration { get; set; } = 0f;

	// ─── Slow (Impedimenta) ───────────────────────────────────────────
	[Property, Sync] public float SlowFraction { get; set; } = 0f;
	[Property, Sync] public float SlowDuration { get; set; } = 0f;

	/// <summary>Bridge server-side: setar antes de NetworkSpawn().</summary>
	public static WizardPlayer PendingShooter { get; set; }

	private const float Lifetime = 3f;

	private WizardPlayer _shooter;
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
		var victim = tr.GameObject?.Components.Get<WizardPlayer>();

		if ( victim != null && victim.Team != ShooterTeam )
		{
			if ( PierceShield )
			{
				int newHp = victim.Health - Damage;
				victim.Health = newHp < 0 ? 0 : newHp;
				if ( victim.Health <= 0 )
					victim.Die( _shooter );
			}
			else
			{
				victim.TakeDamage( Damage, _shooter );
			}

			if ( StunDuration > 0f )
				victim.StunEndTime = Time.Now + StunDuration;

			if ( BurningDPS > 0f && BurningDuration > 0f )
				victim.ApplyBurning( BurningDPS, BurningDuration, _shooter );

			// Slow: implementado via WizardPlayer.SlowEndTime
			if ( SlowFraction > 0f && SlowDuration > 0f )
				ApplySlowToVictim( victim );

			// Notifica o MasterySystem do atirador
			if ( _shooter != null )
			{
				var mastery = _shooter.Components.Get<MasterySystem>( FindMode.EverythingInSelf );
				mastery?.RegisterSpellHit( GetType().Name );
			}
		}

		PlayHitEffect( tr.EndPosition, tr.Normal );
	}

	private void ApplySlowToVictim( WizardPlayer victim )
	{
		// Slow é aplicado reduzindo velocidade via campos no WizardPlayer
		victim.SlowEndTime = Time.Now + SlowDuration;
		victim.SlowFraction = SlowFraction;
	}

	[Rpc.Broadcast]
	private void PlayHitEffect( Vector3 position, Vector3 normal )
	{
		// TODO: spawnar partícula de impacto
	}
}
