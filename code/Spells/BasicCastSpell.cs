/// <summary>
/// Ataque básico da varinha: hitscan, 40 dmg (80 headshot), 3 tiros/segundo.
/// Sem custo de mana. Tier não muda stats base — é escalado pelo MasterySystem se necessário.
/// </summary>
public sealed class BasicCastSpell : BaseSpell
{
	private const float ImpactDamageRadius = 64f;

	public override string SpellName => "Basic Cast";
	public override float ManaCost => 0f;
	public override float BaseCooldown => 1f / 2f; // 3 tiros/segundo
	public override string Image => "ui/spells/stupefy.png";

	public int BaseDamage { get; set; } = 40;
	public int HeadshotDamage { get; set; } = 80;
	public float Range { get; set; } = 5000f;

	public override void Execute( Wand wand )
	{
		int damage = wand.ResolveDamageInt( BaseDamage );
		wand?.SpawnProjectile(
			sourceClass:    nameof( StupefySpell ),
			speed:          500f,
			damage:         damage,
			color:          new Color( 0.9f, 0.1f, 0.1f ),
			stun:           0,
			pierceShield:   Tier >= 2,
			launchAirborne: Tier >= 2,
			impactDamageRadius: ImpactDamageRadius
		); 
	}
}
