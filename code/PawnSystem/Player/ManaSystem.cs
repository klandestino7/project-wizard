namespace Warlocks;

/// <summary>
/// Gerencia mana do jogador: máx 100, regen 15/s após 2s sem usar magia.
/// Adicione no mesmo GameObject que PlayerPawn.
/// </summary>
public sealed class ManaSystem : Component
{
	public const float MaxMana = 100f;
	public const float RegenRate = 15f;
	public const float RegenDelay = 2f;

	[Property, Sync] public float Mana { get; set; } = MaxMana;
	[Sync] private float LastUsedTime { get; set; } = -999f;

	// Felix Felicis: próximo feitiço gratuito (sem mana nem CD)
	[Sync] public bool FelixActive { get; set; } = false;

	public float ManaPercent => Mana / MaxMana;
	public bool HasMana( float cost ) => Mana >= cost;

	// ─── Gasto ────────────────────────────────────────────────────────
	public bool TrySpend( float cost )
	{
		if ( !Networking.IsHost ) return false;

		if ( FelixActive )
		{
			FelixActive = false;
			return true; // gratuito
		}

		if ( Mana < cost ) return false;
		Mana -= cost;
		LastUsedTime = Time.Now;
		return true;
	}

	public void Restore( float amount )
	{
		if ( !Networking.IsHost ) return;
		Mana = Math.Min( Mana + amount, MaxMana );
	}

	public void ResetFull()
	{
		if ( !Networking.IsHost ) return;
		Mana = MaxMana;
		LastUsedTime = -999f;
		FelixActive = false;
	}

	// ─── Regen ────────────────────────────────────────────────────────
	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost || Mana >= MaxMana ) return;
		if ( Time.Now >= LastUsedTime + RegenDelay )
			Mana = Math.Min( Mana + RegenRate * Time.Delta, MaxMana );
	}
}
