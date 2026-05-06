namespace Warlocks;

/// <summary>
/// Poção de Mana (500G): restaura 60 mana instantaneamente.
/// </summary>
public sealed class ManaPotion : BaseConsumable
{
	protected override void OnStart()
	{
		base.OnStart();
		ItemName = "Poção de Mana";
		PurchaseCost = 500;
	}

	protected override bool Use()
	{
		if ( !Networking.IsHost ) return false;
		if ( Player.ManaSystem == null || Player.ManaSystem.Mana >= ManaSystem.MaxMana )
			return false;

		Player.ManaSystem.Restore( 60f );
		return true;
	}
}
