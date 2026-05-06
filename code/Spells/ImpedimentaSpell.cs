
/// <summary>
/// Impedimenta: projétil de slow pesado.
/// T0: 50% slow/2s, single target  | Mana 30 | CD 9s  (600G)
/// T1: 60% slow/2.5s, AoE 2m       | CD 8s
/// T2: 70% slow/3s, AoE 3m         | CD 7s
/// </summary>
public sealed class ImpedimentaSpell : BaseSpell
{
	private static readonly float[] SlowByTier     = { 0.5f, 0.6f, 0.7f };
	private static readonly float[] DurationByTier = { 2f, 2.5f, 3f };
	private static readonly float[] AoEByTier      = { 0f, 2f * 39.37f, 3f * 39.37f }; // metros para unidades do engine (aprox 1m = 39u)

	public override string  SpellName      => "Impedimenta";
	public override string Image => "ui/spells/bomba.png";
	public override float   ManaCost       => 30f;
	public override float   BaseCooldown   => 9f;
	public override float[] CooldownByTier => new[] { 9f, 8f, 7f };
	public override int     Tier1Cost      => 600;
	public override int     Tier2Cost      => 1600;

	public override void Execute( Wand wand )
	{
		int t      = Math.Clamp( Tier, 0, 2 );
		var origin = wand.Player.GetSpellMuzzle();
		var dir    = wand.Player.GetSpellDirection( origin );
		var duration = wand.ResolveDuration( DurationByTier[t] );

		var go = new GameObject( true, "Impedimenta_Projectile" );
		go.WorldPosition = origin;
		go.WorldRotation = Rotation.LookAt( dir );
		go.Tags.Add( "projectile" );

		var proj = go.Components.Create<ImpedimentaProjectile>();
		proj.OwnerTeam    = wand.Player.Team;
		proj.Shooter      = wand.Player;
		proj.SlowFraction = SlowByTier[t];
		proj.SlowDuration = duration;
		proj.AoERadius    = AoEByTier[t];
		proj.SpellColor   = new Color( 0.3f, 0.3f, 0.9f );

		go.NetworkSpawn();
	}
}

/// <summary>Projétil de Impedimenta com suporte a slow em AoE.</summary>
public sealed class ImpedimentaProjectile : Component
{
	[Property, Sync] public Team  OwnerTeam    { get; set; }
	[Property, Sync] public float SlowFraction { get; set; }
	[Property, Sync] public float SlowDuration { get; set; }
	[Property, Sync] public float AoERadius    { get; set; }
	[Property, Sync] public Color SpellColor   { get; set; }

	public PlayerPawn Shooter { get; set; }

	private const float Speed    = 1600f;
	private const float Lifetime = 3f;
	private float _spawnTime;
	private bool  _hit;

	protected override void OnStart()
	{
		_spawnTime = Time.Now;

		var light = Components.Create<PointLight>();
		light.LightColor = SpellColor;
		light.Radius = 60f;

		var lib = SpellEffectsLibrary.Get( Scene );
		var trailPrefab = lib?.GetTrailPrefab( nameof( ImpedimentaSpell ) );
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
		var tr   = Scene.Trace
			.Ray( WorldPosition, WorldPosition + step )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( Shooter?.GameObject )
			.WithoutTags( "projectile" )
			.Run();

		if ( tr.Hit )
		{
			_hit = true;
			ApplySlow( tr.EndPosition );
			BroadcastHitEffect( tr.EndPosition, tr.Normal );
			GameObject.Destroy();
			return;
		}

		WorldPosition += step;
	}

	[Rpc.Broadcast]
	private void BroadcastHitEffect( Vector3 position, Vector3 normal )
	{
		var lib = SpellEffectsLibrary.Get( Scene );
		var hitPrefab = lib?.GetHitPrefab( nameof( ImpedimentaSpell ) );
		if ( hitPrefab is null ) return;

		var fx = hitPrefab.Clone();
		fx.WorldPosition = position;
		fx.WorldRotation = Rotation.LookAt( normal );
		if ( !fx.Components.TryGet<AutoDestroy>( out _ ) )
			fx.Components.Create<AutoDestroy>();
	}

	private void ApplySlow( Vector3 hitPos )
	{
		if ( AoERadius <= 0f )
		{
			// Single target: usa o objeto que parou o trace
			foreach ( var p in Scene.GetAllComponents<PlayerPawn>() )
			{
				if ( p.Team == OwnerTeam || !p.IsAlive ) continue;
				if ( p.WorldPosition.Distance( hitPos ) < 50f )
				{
					p.SlowEndTime  = Time.Now + SlowDuration;
					p.SlowFraction = SlowFraction;
				}
			}
			return;
		}

		// AoE
		foreach ( var p in Scene.GetAllComponents<PlayerPawn>() )
		{
			if ( p.Team == OwnerTeam || !p.IsAlive ) continue;
			if ( p.WorldPosition.Distance( hitPos ) <= AoERadius )
			{
				p.SlowEndTime  = Time.Now + SlowDuration;
				p.SlowFraction = SlowFraction;
			}
		}
	}
}
