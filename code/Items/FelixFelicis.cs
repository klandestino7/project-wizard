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

	protected override void Use()
	{
		if ( !Networking.IsHost ) return;
		if ( Player.ManaSystem != null )
			Player.ManaSystem.FelixActive = true;
	}
}
