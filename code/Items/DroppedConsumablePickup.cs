namespace Warlocks;

/// <summary>
/// Simple world pickup spawned when a player drops consumables on death.
/// Hold the interaction key near it to claim the stack.
/// </summary>
public sealed class DroppedConsumablePickup : Component, IInteractable
{
	[Property] public ConsumableType Type { get; set; }
	[Property] public int Amount { get; set; } = 1;
	[Property] public float InteractDistance { get; set; } = 90f;

	public static void Spawn( Scene scene, Vector3 position, ConsumableType type, int amount )
	{
		if ( !Networking.IsHost || scene == null || amount <= 0 || type == ConsumableType.None )
			return;

		var go = new GameObject( true, $"{type}_Pickup" );
		go.WorldPosition = position + Vector3.Up * 12f;

		var pickup = go.Components.Create<DroppedConsumablePickup>();
		pickup.Type = type;
		pickup.Amount = amount;
		pickup.RefreshVisuals();

		go.Components.Create<DestroyBetweenRounds>();
		go.NetworkSpawn();
	}

	protected override void OnStart()
	{
		RefreshVisuals();
	}

	public bool CanInteract( PlayerPawn player )
	{
		return Networking.IsHost
			&& player.IsValid()
			&& player.IsAlive
			&& Amount > 0
			&& player.WorldPosition.Distance( WorldPosition ) <= InteractDistance;
	}

	public void TryInteract( PlayerPawn player )
	{
		if ( !CanInteract( player ) )
			return;

		if ( !player.TryAddConsumable( Type, Amount ) )
			return;

		Amount = 0;
		GameObject.Destroy();
	}

	public void StopInteract( PlayerPawn player )
	{
	}

	private void RefreshVisuals()
	{
		var renderer = Components.Get<ModelRenderer>() ?? Components.Create<ModelRenderer>();
		renderer.Model = Model.Load( "models/dev/sphere.vmdl" );

		var scale = Type == ConsumableType.HealthPotion ? 0.22f : 0.18f;
		Transform.LocalScale = scale;
	}
}
