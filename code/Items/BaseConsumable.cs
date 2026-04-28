
/// <summary>
/// Base para itens consumíveis (poções, equipamentos de 1 uso).
/// Adicione como componente no mesmo GameObject que WizardPlayer.
/// </summary>
public abstract class BaseConsumable : Component
{
	[Property] public string ItemName { get; set; } = "Item";
	[Property] public int PurchaseCost { get; set; } = 500;
	[Property, Sync] public bool IsUsed { get; set; } = false;

	protected WizardPlayer Player { get; private set; }

	protected override void OnStart()
	{
		Player = Components.Get<WizardPlayer>( FindMode.InAncestors );
	}

	public void TryUse()
	{
		if ( IsUsed ) return;
		if ( !Player.IsValid() || !Player.IsAlive ) return;
		Use();
		IsUsed = true;
	}

	protected abstract void Use();

	/// <summary>Chamado no início de cada round.</summary>
	public void ResetForRound()
	{
		IsUsed = false;
		OnRoundReset();
	}

	protected virtual void OnRoundReset() { }

	public bool CanBuy( WizardPlayer player ) =>
		!IsUsed && player.Galleons >= PurchaseCost;
}
