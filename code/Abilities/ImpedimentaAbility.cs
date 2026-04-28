
/// <summary>
/// Impedimenta: projétil de slow pesado em área.
/// Tier 0: Slow 50% por 2s           | Mana 30 | CD 9s  (500G / +600G / +1000G)
/// Tier 1: Slow 60% por 2.5s, AoE 2m
/// Tier 2: Slow 70% por 3s, AoE 3m
/// </summary>
public sealed class ImpedimentaAbility : BaseAbility
{
	private static readonly float[] SlowByTier = { 0.5f, 0.6f, 0.7f };
	private static readonly float[] DurationByTier = { 2f, 2.5f, 3f };
	private static readonly float[] AoEByTier = { 0f, 2f * 39.37f, 3f * 39.37f }; // metros → unidades S&Box (≈ 1m = 39u)
	protected override float[] CooldownByTier => new[] { 9f, 8f, 7f };

	[Sync] private int _activateCount { get; set; } = 0;
	private int _lastActivateCount = 0;

	protected override void OnStart()
	{
		base.OnStart();
		AbilityName = "Impedimenta";
		ManaCost = 30f;
		Tier1Cost = 600;
		Tier2Cost = 1600;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Networking.IsHost && _activateCount != _lastActivateCount )
		{
			_lastActivateCount = _activateCount;
			if ( Player.IsValid() )
				SpawnProjectile( Player.EyePosition, Player.EyeAngles.ToRotation().Forward );
		}
	}

	protected override void Activate() => _activateCount++;

	private void SpawnProjectile( Vector3 origin, Vector3 direction )
	{
		int t = Math.Clamp( CurrentTier, 0, 2 );
		var go = new GameObject( true, "Impedimenta_Projectile" );
		go.WorldPosition = origin;
		go.WorldRotation = Rotation.LookAt( direction );
		go.Tags.Add( "projectile" );

		SpellProjectile.PendingShooter = Player;
		var proj = go.Components.Create<ImpedimentaProjectile>();
		proj.OwnerTeam = Player.Team;
		proj.Shooter = Player;
		proj.SlowFraction = SlowByTier[t];
		proj.SlowDuration = DurationByTier[t];
		proj.AoERadius = AoEByTier[t];
		proj.SpellColor = new Color( 0.3f, 0.3f, 0.9f );

		go.NetworkSpawn();
	}
}

/// <summary>Projétil de Impedimenta com suporte a AoE de slow.</summary>
public sealed class ImpedimentaProjectile : Component
{
	[Property, Sync] public Team OwnerTeam { get; set; }
	[Property, Sync] public float SlowFraction { get; set; }
	[Property, Sync] public float SlowDuration { get; set; }
	[Property, Sync] public float AoERadius { get; set; }
	[Property, Sync] public Color SpellColor { get; set; }

	public WizardPlayer Shooter { get; set; }

	private const float Speed = 1600f;
	private const float Lifetime = 3f;
	private float _spawnTime;
	private bool _hit;

	protected override void OnStart()
	{
		_spawnTime = Time.Now;
		var light = Components.Create<PointLight>();
		light.LightColor = SpellColor;
		light.Radius = 60f;
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
			.IgnoreGameObjectHierarchy( Shooter?.GameObject )
			.WithoutTags( "projectile" )
			.Run();

		if ( tr.Hit )
		{
			_hit = true;
			ApplySlow( tr.EndPosition );
			GameObject.Destroy();
			return;
		}

		WorldPosition += step;
	}

	private void ApplySlow( Vector3 hitPos )
	{
		if ( AoERadius <= 0f )
		{
			// Single target
			var victim = Scene.Trace.Ray( hitPos, hitPos ).Run()
				.GameObject?.Components.Get<WizardPlayer>();
			if ( victim != null && victim.Team != OwnerTeam )
				victim.SlowEndTime = Time.Now + SlowDuration;
			return;
		}

		// AoE
		foreach ( var p in Scene.GetAllComponents<WizardPlayer>() )
		{
			if ( p.Team == OwnerTeam || !p.IsAlive ) continue;
			if ( p.WorldPosition.Distance( hitPos ) <= AoERadius )
			{
				p.SlowEndTime = Time.Now + SlowDuration;
				p.SlowFraction = SlowFraction;
			}
		}
	}
}
