
/// <summary>
/// Sectumsempra: hitscan com headshot bonus e bleeding no T2.
/// Tier 0: 100 dmg, headshot 2x   | Mana 30 | CD 8s  (600G / +700G / +1100G)
/// Tier 1: 130 dmg, headshot 2.2x
/// Tier 2: 160 dmg, headshot 2.5x + bleeding 15 dps por 3s
/// </summary>
public sealed class SectumsempraAbility : BaseAbility
{
	private static readonly int[] DamageByTier = { 100, 130, 160 };
	private static readonly float[] HeadshotMultByTier = { 2f, 2.2f, 2.5f };
	protected override float[] CooldownByTier => new[] { 8f, 7f, 6f };

	[Sync] private int _activateCount { get; set; } = 0;
	private int _lastActivateCount = 0;

	protected override void OnStart()
	{
		base.OnStart();
		AbilityName = "Sectumsempra";
		ManaCost = 30f;
		Tier1Cost = 700;
		Tier2Cost = 1800;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Networking.IsHost && _activateCount != _lastActivateCount )
		{
			_lastActivateCount = _activateCount;
			if ( Player.IsValid() )
				FireHitscan();
		}
	}

	protected override void Activate() => _activateCount++;

	private void FireHitscan()
	{
		int t = Math.Clamp( CurrentTier, 0, 2 );

		var muzzle = Player.GetSpellMuzzle();
		var dir    = Player.GetSpellDirection( muzzle );
		var tr = Scene.Trace
			.Ray( muzzle, muzzle + dir * 5000f )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( Player.GameObject )
			.Run();

		if ( !tr.Hit ) return;

		var victim = tr.GameObject?.Components.Get<WizardPlayer>();
		if ( victim == null || victim.Team == Player.Team ) return;

		// Headshot: ponto de impacto acima de 75% da altura do alvo
		float hitHeight = tr.EndPosition.z - victim.WorldPosition.z;
		bool isHeadshot = hitHeight > WizardPlayer.EyeHeight * 0.75f;
		float mult = isHeadshot ? HeadshotMultByTier[t] : 1f;
		int dmg = (int)(DamageByTier[t] * mult);

		victim.TakeDamage( dmg, Player, isHeadshot );

		// T2: bleeding
		if ( CurrentTier >= 2 )
			victim.ApplyBurning( 15f, 3f, Player ); // reusa burning como bleeding

		// Mastery
		var mastery = Player.Components.Get<MasterySystem>( FindMode.EverythingInSelf );
		mastery?.RegisterSpellHit( "SectumsempraAbility" );

		BroadcastEffect( tr.EndPosition );
	}

	[Rpc.Broadcast]
	private void BroadcastEffect( Vector3 pos )
	{
		// TODO: VFX de corte roxo
	}
}
