/// <summary>
/// Ataque básico da varinha: hitscan, 40 dmg (80 headshot), 3 tiros/segundo.
/// Sem custo de mana. Tier não muda stats base — é escalado pelo MasterySystem se necessário.
/// </summary>
public sealed class BasicCastSpell : BaseSpell
{
	public override string SpellName => "Basic Cast";
	public override float ManaCost => 0f;
	public override float BaseCooldown => 1f / 3f; // 3 tiros/segundo
	public override string Image => "ui/spells/stypefy.png";

	public int BaseDamage { get; set; } = 40;
	public int HeadshotDamage { get; set; } = 80;
	public float Range { get; set; } = 5000f;

	public override void Execute( Wand wand )
	{

		int t = Math.Clamp( Tier, 0, 2 );
		wand.SpawnProjectile(
			sourceClass:    nameof( StupefySpell ),
			speed:          2000f,
			damage:         BaseDamage,
			color:          new Color( 0.9f, 0.1f, 0.1f ),
			stun:           0,
			pierceShield:   Tier >= 2,
			launchAirborne: Tier >= 2
		);
		// var tr = wand.FireHitscan( Range );
		// var start = wand.Player.GetSpellMuzzle();

		// wand.ShowBeamEffect( start, tr.EndPosition );

		// if ( !tr.Hit ) return;

		// var victim = tr.GameObject?.Components.Get<PlayerPawn>();
		// if ( victim == null || victim.Team == wand.Player.Team ) return;

		// bool headshot = tr.Hitbox?.Tags.Has( "head" ) ?? false;
		// victim.TakeDamage( headshot ? HeadshotDamage : BaseDamage, wand.Player, headshot );
	}
}
