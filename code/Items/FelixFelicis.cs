namespace Warlocks;

/// <summary>
/// Felix Felicis (800G): próximo feitiço sem cooldown nem custo de mana.
/// </summary>
public sealed class FelixFelicis : BaseConsumable
{
	protected override void OnStart()
	{
		base.OnStart();
		ItemName = "Felix Felicis";
		PurchaseCost = 800;
	}

	protected override bool Use()
	{
		if ( !Networking.IsHost ) return false;
		if ( Player.ManaSystem == null || Player.ManaSystem.FelixActive )
			return false;

		Player.ManaSystem.FelixActive = true;
		return true;
	}
}
