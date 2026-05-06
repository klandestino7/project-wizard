namespace Warlocks;

/// <summary>
/// Base para itens consumíveis (poções, equipamentos de 1 uso).
/// Adicione como componente no mesmo GameObject que PlayerPawn.
/// </summary>
public abstract class BaseConsumable : Component
{
	[Property] public string ItemName { get; set; } = "Item";
	[Property] public int PurchaseCost { get; set; } = 500;
	[Property, Sync] public int Charges { get; private set; } = 0;
	[Property] public int MaxCharges { get; set; } = 9;
	public bool IsUsed => Charges <= 0;

	protected PlayerPawn Player { get; private set; }

	protected override void OnStart()
	{
		Player = Components.Get<PlayerPawn>( FindMode.InAncestors );
	}

	public void TryUse()
	{
		if ( Charges <= 0 ) return;
		if ( !Player.IsValid() || !Player.IsAlive ) return;

		using var _ = Rpc.FilterInclude( Connection.Host );
		TryUseHost();
	}

	public bool TryAddCharges( int amount )
	{
		if ( !Networking.IsHost || amount <= 0 )
			return false;

		if ( Charges >= MaxCharges )
			return false;

		Charges = Math.Min( MaxCharges, Charges + amount );
		return true;
	}

	public int RemoveAllCharges()
	{
		if ( !Networking.IsHost )
			return 0;

		int dropped = Charges;
		Charges = 0;
		return dropped;
	}

	[Rpc.Owner]
	private void TryUseHost()
	{
		if ( !Networking.IsHost ) return;
		if ( Charges <= 0 ) return;
		if ( !Player.IsValid() || !Player.IsAlive ) return;

		if ( !Use() )
			return;

		Charges = Math.Max( 0, Charges - 1 );
	}

	protected abstract bool Use();

	/// <summary>Chamado no início de cada round.</summary>
	public void ResetForRound()
	{
		OnRoundReset();
	}

	protected virtual void OnRoundReset() { }

	// public bool CanBuy( PlayerPawn player ) =>
	// 	!IsUsed && player.Balance >= PurchaseCost;
}
