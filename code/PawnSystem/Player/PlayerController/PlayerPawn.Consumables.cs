namespace Warlocks;

public partial class PlayerPawn
{
	public void EnsureConsumableSlots()
	{
		ItemSlot1 ??= Components.Get<HealPotion>() ?? Components.Create<HealPotion>();
		ItemSlot2 ??= Components.Get<ManaPotion>() ?? Components.Create<ManaPotion>();
	}

	public BaseConsumable GetConsumable( ConsumableType type ) => type switch
	{
		ConsumableType.HealthPotion => ItemSlot1,
		ConsumableType.ManaPotion => ItemSlot2,
		_ => null
	};

	public bool TryAddConsumable( ConsumableType type, int amount )
	{
		if ( !Networking.IsHost || amount <= 0 )
			return false;

		EnsureConsumableSlots();
		return GetConsumable( type )?.TryAddCharges( amount ) == true;
	}

	public void ClientBuyConsumable( ConsumableType type )
	{
		using var _ = Rpc.FilterInclude( Connection.Host );
		BuyConsumableHost( type );
	}

	public void DropConsumablesOnDeath()
	{
		if ( !Networking.IsHost )
			return;

		EnsureConsumableSlots();
		DropConsumable( ItemSlot1, ConsumableType.HealthPotion );
		DropConsumable( ItemSlot2, ConsumableType.ManaPotion );
	}

	private void DropConsumable( BaseConsumable consumable, ConsumableType type )
	{
		if ( consumable == null )
			return;

		int amount = consumable.RemoveAllCharges();
		if ( amount <= 0 )
			return;

		DroppedConsumablePickup.Spawn( Scene, WorldPosition, type, amount );
	}

	[Rpc.Owner]
	private void BuyConsumableHost( ConsumableType type )
	{
		if ( !Networking.IsHost || !IsAlive || Client == null )
			return;

		if ( Client.BuyMenuMode == BuyMenuMode.Disabled )
			return;

		EnsureConsumableSlots();

		var consumable = GetConsumable( type );
		if ( consumable == null )
			return;

		if ( Client.Balance < consumable.PurchaseCost )
			return;

		if ( !consumable.TryAddCharges( 1 ) )
			return;

		Client.Balance -= consumable.PurchaseCost;
	}
}
