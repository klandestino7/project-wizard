namespace Warlocks;

/// <summary>
/// Episkey (F): cura instantânea em si mesmo.
/// T0: 80 HP                           | Mana 25 | CD 10s  (600G)
/// T1: 120 HP, remove debuffs           | CD 8s
/// T2: 160 HP, remove debuffs, +20 HP temp
/// </summary>
public sealed class EpiskeySpell : BaseSpell
{
	private static readonly int[] HealByTier   = { 80, 120, 160 };
	private static readonly int[] TempHpByTier = { 0, 0, 20 };

	public override string  SpellName      => "Episkey";
	public override string Image => "ui/spells/silencio.png";
	public override float   ManaCost       => 25f;
	public override float   BaseCooldown   => 10f;
	public override float[] CooldownByTier => new[] { 10f, 8f, 7f };

	public override void Execute( Wand wand )
	{
		if ( !Networking.IsHost ) return;

		var hc = wand.Player.HealthComponent;
		if ( !hc.IsValid() ) return;

		int t = Math.Clamp( Tier, 0, 2 );
		var heal = wand.ResolveDamageInt( HealByTier[t] );
		var tempHp = wand.ResolveDamageInt( TempHpByTier[t] );
		hc.Health = Math.Min( hc.Health + heal, hc.MaxHealth );

		if ( Tier >= 1 )
			wand.Player.ExtinguishBurning();

		if ( Tier >= 2 )
			hc.Health = Math.Min( hc.Health + tempHp, hc.MaxHealth + tempHp );
	}
}
