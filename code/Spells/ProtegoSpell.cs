
/// <summary>
/// Protego (E): escudo mágico reativo.
/// Define custo, cooldown e tier do feitiço.
/// O estado do escudo (sync, VFX, absorção) fica no ProtegoComponent (Component separado).
/// </summary>
public sealed class ProtegoSpell : BaseSpell
{
	public override string  SpellName      => "Protego";
	public override string Image => "ui/spells/lumos.png";
	public override float   ManaCost       => 30f;
	public override float   BaseCooldown   => 6f;
	public override float[] CooldownByTier => new[] { 6f, 5f, 4f };

	public override void Execute( Wand wand )
	{
		var shield = wand.Player.Components.Get<ProtegoComponent>( FindMode.EverythingInSelf );
		var shieldHp = wand.ResolveDamageInt( Tier switch
		{
			0 => 100,
			1 => 200,
			_ => 350
		} );
		var duration = wand.ResolveDuration( Tier switch
		{
			0 => 3f,
			1 => 4f,
			_ => 5f
		} );
		shield?.Activate( Tier, shieldHp, duration );
	}
}
