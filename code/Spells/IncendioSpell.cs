
/// <summary>
/// Incendio: projétil de fogo com burning DoT.
/// T0: 60 dmg + 20 dps/3s  | Mana 25 | CD 7s  (500G)
/// T1: 80 dmg + 25 dps/4s  | CD 6s
/// T2: 100 dmg + 30 dps/5s | CD 5s
/// </summary>
public sealed class IncendioSpell : BaseSpell
{
	private static readonly int[]   DamageByTier  = { 60, 80, 100 };
	private static readonly float[] BurnDpsByTier  = { 20f, 25f, 30f };
	private static readonly float[] BurnDurByTier  = { 3f, 4f, 5f };

	public override string  SpellName      => "Incendio";
	public override string Image => "ui/spells/incendio.png";
	public override float   ManaCost       => 25f;
	public override float   BaseCooldown   => 7f;
	public override float[] CooldownByTier => new[] { 7f, 6f, 5f };

	public override void Execute( Wand wand )
	{
		int t = Math.Clamp( Tier, 0, 2 );
		var damage = wand.ResolveDamageInt( DamageByTier[t] );
		var burnDps = wand.ResolveDamage( BurnDpsByTier[t] );
		var burnDuration = wand.ResolveDuration( BurnDurByTier[t] );
		wand.SpawnProjectile(
			sourceClass:  nameof( IncendioSpell ),
			speed:        1800f,
			damage:       damage,
			color:        new Color( 1f, 0.4f, 0.05f ),
			burnDPS:      burnDps,
			burnDuration: burnDuration
		);
	}
}
