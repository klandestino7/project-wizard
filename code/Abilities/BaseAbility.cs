
/// <summary>
/// Classe base para todos os feitiços. Gerencia cooldown, mana, tier e custo de upgrade.
/// </summary>
public abstract class BaseAbility : Component
{
	[Property] public string AbilityName { get; set; } = "Feitiço";
	[Property] public int Tier1Cost { get; set; } = 400;
	[Property] public int Tier2Cost { get; set; } = 1000;
	[Property] public float ManaCost { get; set; } = 20f;

	[Property, Sync] public int CurrentTier { get; set; } = 0;

	[Sync] protected float CooldownEnd { get; set; } = 0f;

	protected WizardPlayer Player { get; private set; }

	/// <summary>Cooldowns em segundos para cada tier. Override nas subclasses.</summary>
	protected virtual float[] CooldownByTier => new[] { 10f, 8f, 6f };

	private int SafeTier => CurrentTier < 0 ? 0 : CurrentTier > 2 ? 2 : CurrentTier;

	public float CurrentCooldownDuration => CooldownByTier[SafeTier];
	public bool IsReady => Time.Now >= CooldownEnd;
	public float CooldownRemaining
	{
		get
		{
			float r = CooldownEnd - Time.Now;
			return r < 0f ? 0f : r;
		}
	}
	public float CooldownFraction => IsReady ? 1f : 1f - CooldownRemaining / CurrentCooldownDuration;

	protected override void OnStart()
	{
		Player = Components.Get<WizardPlayer>( FindMode.EverythingInSelf );
	}

	// ─── Ativação ─────────────────────────────────────────────────────
	public void TryActivate()
	{
		if ( !IsReady ) return;
		if ( !Player.IsValid() || !Player.IsAlive || Player.IsStunned ) return;

		// Felix Felicis ignora mana; caso contrário, verifica mana
		var mana = Player.ManaSystem;
		if ( mana != null && ManaCost > 0f && !mana.HasMana( ManaCost ) && !mana.FelixActive )
			return;

		Activate();

		// Animação de cast na varinha
		Player.WandHolder?.TriggerCastAnimation();

		// Consome mana no servidor
		if ( Networking.IsHost )
			mana?.TrySpend( ManaCost );

		// Felix Felicis: não consome CD
		bool isFelixFree = mana != null && ManaCost > 0f;
		if ( !isFelixFree )
			CooldownEnd = Time.Now + CurrentCooldownDuration;
		else
			CooldownEnd = Time.Now + CurrentCooldownDuration;
	}

	protected abstract void Activate();

	// ─── Cooldown / Reset ─────────────────────────────────────────────
	public void ResetCooldown() => CooldownEnd = 0f;
	public void ResetTier() => CurrentTier = 0;

	// ─── Upgrade (chamado pelo BuyMenu) ───────────────────────────────
	public bool TryUpgrade( int targetTier )
	{
		if ( !Networking.IsHost ) return false;
		if ( CurrentTier >= targetTier ) return false;

		int cost = targetTier == 1 ? Tier1Cost : Tier2Cost;
		if ( !Player.SpendGalleons( cost ) ) return false;

		CurrentTier = targetTier;
		ApplyTierBonuses();
		return true;
	}

	public virtual void ApplyTierBonuses() { }
}
