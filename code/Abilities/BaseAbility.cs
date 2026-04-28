
/// <summary>
/// Classe base para todos os feitiços. Gerencia cooldown, tier e custo de upgrade.
/// </summary>
public abstract class BaseAbility : Component
{
	[Property] public string AbilityName { get; set; } = "Feitiço";
	[Property] public int Tier1Cost { get; set; } = 400;
	[Property] public int Tier2Cost { get; set; } = 1000;

	[Property, Sync] public int CurrentTier { get; set; } = 0;
	[Sync] private float _cooldownEnd { get; set; } = 0f;

	protected WizardPlayer Player { get; private set; }

	/// <summary>Cooldowns em segundos para cada tier. Override nas subclasses.</summary>
	protected virtual float[] CooldownByTier => new[] { 10f, 8f, 6f };
	public float CurrentCooldownDuration => CooldownByTier[Math.Clamp( CurrentTier, 0, 2 )];

	public bool IsReady => Time.Now >= _cooldownEnd;
	public float CooldownRemaining => MathX.Max( 0f, _cooldownEnd - Time.Now );
	public float CooldownFraction => IsReady ? 1f : 1f - CooldownRemaining / CurrentCooldownDuration;

	protected override void OnStart()
	{
		Player = Components.Get<WizardPlayer>( FindMode.InAncestors );
	}

	// ─── Ativação ─────────────────────────────────────────────────────
	public void TryActivate()
	{
		if ( !IsReady || !Player.IsValid() || !Player.IsAlive || Player.IsStunned ) return;
		Activate();
		_cooldownEnd = Time.Now + CurrentCooldownDuration;
	}

	protected abstract void Activate();

	// ─── Cooldown ─────────────────────────────────────────────────────
	public void ResetCooldown() => _cooldownEnd = 0f;
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

	protected virtual void ApplyTierBonuses() { }
}
