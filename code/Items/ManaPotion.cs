
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

	protected override void Use()
	{
		if ( !Networking.IsHost ) return;
		Player.ManaSystem?.Restore( 60f );
	}
}
