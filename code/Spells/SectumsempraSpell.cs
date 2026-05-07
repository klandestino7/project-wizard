
/// <summary>
/// Sectumsempra: hitscan com headshot bônus e bleeding no T2.
/// T0: 100 dmg, headshot 2x   | Mana 30 | CD 8s  (700G)
/// T1: 130 dmg, headshot 2.2x | CD 7s
/// T2: 160 dmg, headshot 2.5x + bleeding 15 dps/3s
/// </summary>
public sealed class SectumsempraSpell : BaseSpell
{
	private static readonly int[]   DamageByTier       = { 100, 130, 160 };
	private static readonly float[] HeadshotMultByTier = { 2f, 2.2f, 2.5f };

	public override string  SpellName      => "Sectumsempra";
	public override string Image => "ui/spells/avada_kedavra.png";
	public override float   ManaCost       => 30f;
	public override float   BaseCooldown   => 8f;
	public override float[] CooldownByTier => new[] { 8f, 7f, 6f };

	public override void Execute( Wand wand )
	{
		int t  = Math.Clamp( Tier, 0, 2 );
		var tr = wand.FireHitscan( 5000f );

		if ( !tr.Hit ) return;

		var victim = GameUtils.GetPlayerFromGameObject( tr.GameObject );
		if ( victim == null || victim.Team == wand.Player.Team ) return;

		float hitHeight = tr.EndPosition.z - victim.WorldPosition.z;
		bool  isHeadshot = hitHeight > PlayerPawn.EyeHeight * 0.75f;
		float mult = isHeadshot ? HeadshotMultByTier[t] : 1f;
		float baseDamage = wand.ResolveDamage( DamageByTier[t] );
		float finalDamage = baseDamage * mult;

		victim.HealthComponent.TakeDamage( new Warlocks.DamageInfo( wand.Player, finalDamage, Position: tr.EndPosition ) );
		wand.BroadcastImpactSound( tr.EndPosition );

		if ( Tier >= 2 )
			victim.ApplyBurning( wand.ResolveDamage( 15f ), wand.ResolveDuration( 3f ), wand.Player ); // bleeding via burning

		// Feed combat systems (hitscan bypasses SpellProjectile pipeline)
		wand.Player.UltimateCharge?.AddDamageCharge( finalDamage );
		wand.Player.PassiveEffects?.OnDamageDealt( finalDamage );

		var catalogEntry = SpellCatalog.Find( nameof( SectumsempraSpell ) );
		if ( catalogEntry != null && catalogEntry.ComboTags != ComboTag.None )
			victim.ComboSystem?.RecordHit( catalogEntry.ComboTags, wand.Player );

		var mastery = wand.Player.Components.Get<MasterySystem>( FindMode.EverythingInSelf );
		mastery?.RegisterSpellHit( nameof( SectumsempraSpell ) );
	}
}
