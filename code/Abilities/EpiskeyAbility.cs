
/// <summary>
/// Episkey: cura instantânea em si mesmo.
/// Tier 0: 80 HP          | Mana 25 | CD 10s  (500G / +600G / +1000G)
/// Tier 1: 120 HP, remove 1 debuff
/// Tier 2: 160 HP, remove todos debuffs, +20 HP temp por 5s
/// </summary>
public sealed class EpiskeyAbility : BaseAbility
{
	private static readonly int[] HealByTier = { 80, 120, 160 };
	private static readonly int[] TempHpByTier = { 0, 0, 20 };
	protected override float[] CooldownByTier => new[] { 10f, 8f, 7f };

	protected override void OnStart()
	{
		base.OnStart();
		AbilityName = "Episkey";
		ManaCost = 25f;
		Tier1Cost = 600;
		Tier2Cost = 1600;
	}

	protected override void Activate()
	{
		if ( !Networking.IsHost ) return;

		int t = Math.Clamp( CurrentTier, 0, 2 );
		Player.Heal( HealByTier[t] );

		if ( CurrentTier >= 1 )
			Player.ExtinguishBurning(); // remove DoTs

		if ( CurrentTier >= 2 )
		{
			// +20 HP temporário: aumenta HP acima do max por 5s
			Player.Health = Math.Min( Player.Health + TempHpByTier[t], WizardPlayer.MaxHealth + TempHpByTier[t] );
			BroadcastHealEffect();
		}
	}

	[Rpc.Broadcast]
	private void BroadcastHealEffect()
	{
		// TODO: VFX de cura dourada
	}
}
