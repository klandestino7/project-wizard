
/// <summary>
/// Projétil genérico de feitiço. Criado pelo servidor, posição sincronizada
/// automaticamente pelo networking do S&Box.
/// </summary>
public sealed class SpellProjectile : Component
{
	// ─── Propriedades sincronizadas (setar antes de NetworkSpawn) ─────
	[Property, Sync] public ulong ShooterNetId { get; set; }
	[Property, Sync] public int Damage { get; set; } = 80;
	[Property, Sync] public float StunDuration { get; set; } = 1f;
	[Property, Sync] public float Speed { get; set; } = 2000f;
	[Property, Sync] public bool PierceShield { get; set; } = false;
	[Property, Sync] public Color SpellColor { get; set; } = Color.Red;

	private const float Lifetime = 3f;

	private WizardPlayer _shooter;
	private float _spawnTime;
	private bool _hit = false;

	protected override void OnStart()
	{
		_spawnTime = Time.Now;

		// Localiza o atirador pelo NetId (funciona em todos os clientes)
		_shooter = Scene.GetAllComponents<WizardPlayer>()
			.FirstOrDefault( p => p.Network.OwnerId == ShooterNetId );

		// Visual: cria um ponto de luz colorido simples como placeholder
		// TODO: substituir por modelo ou partícula de projétil
		var light = Components.Create<PointLight>();
		light.LightColor = SpellColor;
		light.Radius = 80f;
		light.Enabled = true;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost || _hit ) return;

		// Expirar
		if ( Time.Now - _spawnTime > Lifetime )
		{
			GameObject.Destroy();
			return;
		}

		// Mover para frente
		var step = Transform.Rotation.Forward * Speed * Time.Delta;

		var tr = Scene.Trace
			.Ray( Transform.Position, Transform.Position + step )
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

		Transform.Position += step;
	}

	private void OnHit( SceneTraceResult tr )
	{
		var victim = tr.GameObject?.Components.Get<WizardPlayer>();

		if ( victim != null && _shooter != null && victim.Team != _shooter.Team )
		{
			if ( PierceShield )
			{
				// Ignora shield, vai direto no HP
				victim.Health = Math.Max( 0, victim.Health - Damage );
				if ( victim.Health <= 0 )
					victim.Die( _shooter );
			}
			else
			{
				victim.TakeDamage( Damage, _shooter );
			}

			if ( StunDuration > 0f )
				victim.StunEndTime = Time.Now + StunDuration;
		}

		PlayHitEffect( tr.EndPosition, tr.Normal );
	}

	[Broadcast]
	private void PlayHitEffect( Vector3 position, Vector3 normal )
	{
		// TODO: spawnar partícula de impacto
		// Ex: SceneParticles.PlayInstant( "particles/spell_impact.vpcf", position );
	}
}
