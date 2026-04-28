
/// <summary>
/// Stupefy (Q): projétil de atordoamento.
/// T0: 80 dmg, 1s stun        | Mana 20 | CD 6s  (400G)
/// T1: 120 dmg, 1.5s stun     | CD 5s   (+600G)
/// T2: 160 dmg, 2s stun, ignora escudo, lança ao ar
/// </summary>
public sealed class StupefySpell : BaseSpell
{
	private static readonly int[]   DamageByTier = { 80, 120, 160 };
	private static readonly float[] StunByTier   = { 1f, 1.5f, 2f };

	public override string  SpellName       => "Stupefy";
	public override float   ManaCost        => 20f;
	public override float   BaseCooldown    => 6f;
	public override float[] CooldownByTier  => new[] { 6f, 5f, 4f };
	public override int     Tier1Cost       => 400;
	public override int     Tier2Cost       => 1000;

	public override void Execute( Wand wand )
	{
		int t = Math.Clamp( Tier, 0, 2 );
		wand.SpawnProjectile(
			sourceClass:    nameof( StupefySpell ),
			speed:          2000f,
			damage:         DamageByTier[t],
			color:          new Color( 0.9f, 0.1f, 0.1f ),
			stun:           StunByTier[t],
			pierceShield:   Tier >= 2,
			launchAirborne: Tier >= 2
		);
	}
}
