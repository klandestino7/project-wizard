namespace Warlocks;

/// <summary>
/// Classe base para todos os feitiços. NÃO é Component — apenas define comportamento.
/// Instâncias vivem no SpellsDeck. O Wand é o único executor.
/// Pipeline: Input → Wand.TryCast → (sync) → Spell.Execute(wand)
/// </summary>
public abstract class BaseSpell
{
	/// <summary>Nome exibido na HUD e usado pelo MasterySystem.</summary>
	public abstract string SpellName { get; }

	/// <summary>Imagem exibido na HUD.</summary>
	public abstract string Image { get; }

	/// <summary>Custo de mana por lançamento. 0 = sem custo.</summary>
	public abstract float ManaCost { get; }

	/// <summary>Cooldown base em segundos (tier 0).</summary>
	public abstract float BaseCooldown { get; }

	/// <summary>Tier atual: 0, 1 ou 2. Modificado por TryUpgrade no SpellsDeck.</summary>
	public int Tier { get; set; } = 0;

	/// <summary>Cooldowns por tier. Override para valores personalizados.</summary>
	public virtual float[] CooldownByTier => new[] { BaseCooldown, BaseCooldown * 0.85f, BaseCooldown * 0.7f };

	/// <summary>Custo em galleons para tier 1.</summary>
	public virtual int Tier1Cost => 400;

	/// <summary>Custo em galleons para tier 2.</summary>
	public virtual int Tier2Cost => 1000;

	private float _cooldownEnd = 0f;
	private int SafeTier => Math.Clamp( Tier, 0, 2 );

	public bool IsReady => Time.Now >= _cooldownEnd;
	public float CooldownEndTime => _cooldownEnd;
	public float CooldownRemaining => MathF.Max( 0f, _cooldownEnd - Time.Now );
	public float CooldownDuration => CooldownByTier[SafeTier];
	public float CooldownFraction => IsReady ? 1f : 1f - CooldownRemaining / CooldownDuration;

	public void StartCooldown() => _cooldownEnd = Time.Now + CooldownDuration;
	public void StartCooldown( float durationOverride ) => _cooldownEnd = Time.Now + MathF.Max( 0f, durationOverride );
	public void ResetCooldown() => _cooldownEnd = 0f;

	/// <summary>
	/// Executado pelo Wand no host após validação de cooldown e mana.
	/// Use wand.SpawnProjectile() para projéteis, wand.FireHitscan() para hitscan,
	/// ou aplique efeito diretamente para spells instantâneas.
	/// </summary>
	public abstract void Execute( Wand wand );

	/// <summary>Chamado após upgrade bem-sucedido. Override para ajustar stats.</summary>
	public virtual void OnTierChanged() { }
}
