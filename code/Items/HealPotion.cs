namespace Warlocks;

/// <summary>
/// Poção Vigorizante (600G): regenera 50 HP instantâneo.
/// </summary>
public sealed class HealPotion : BaseConsumable
{
	protected override void OnStart()
	{
		base.OnStart();
		ItemName = "Poção Vigorizante";
		PurchaseCost = 600;
	}

	protected override void Use()
	{
		if ( !Networking.IsHost ) return;
		var hc = Player.HealthComponent;
		if ( hc.IsValid() )
			hc.Health = Math.Min( hc.Health + 50f, hc.MaxHealth );
	}
}
