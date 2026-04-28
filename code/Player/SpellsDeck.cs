
/// <summary>
/// Gerencia os slots de feitiços do jogador.
/// Não instancia Components — armazena apenas referências de BaseSpell.
/// Adicione no mesmo GameObject do WizardPlayer.
/// </summary>
public sealed class SpellsDeck : Component
{
	// ─── Slots ────────────────────────────────────────────────────────

	/// <summary>Ataque básico — slot especial, sem mana, sem ability cooldown longo.</summary>
	public BasicCastSpell BasicCast { get; } = new();

	// Slots Q / E / R / F (índices 0–3)
	private readonly BaseSpell[] _slots = new BaseSpell[4]
	{
		new StupefySpell(),    // Q
		new ProtegoSpell(),    // E
		new DashSpell(),       // R
		new EpiskeySpell(),    // F
	};

	/// <summary>Retorna o feitiço no slot 0–3. Null se vazio.</summary>
	public BaseSpell GetSlot( int index )
	{
		if ( index < 0 || index >= _slots.Length ) return null;
		return _slots[index];
	}

	/// <summary>Substitui o feitiço em um slot (ex: mudança de loadout).</summary>
	public void SetSlot( int index, BaseSpell spell )
	{
		if ( index < 0 || index >= _slots.Length ) return;
		_slots[index] = spell;
	}

	// ─── Cooldown ─────────────────────────────────────────────────────

	/// <summary>Reseta cooldowns de todos os feitiços (chamado no respawn).</summary>
	public void ResetAllCooldowns()
	{
		BasicCast.ResetCooldown();
		foreach ( var s in _slots ) s?.ResetCooldown();
	}

	// ─── Upgrade ──────────────────────────────────────────────────────

	/// <summary>
	/// Faz upgrade de um slot gastando galleons. slotIndex = -1 para BasicCast, 0–3 para slots.
	/// Deve ser chamado no host.
	/// </summary>
	public bool TryUpgrade( int slotIndex, int targetTier )
	{
		if ( !Networking.IsHost ) return false;

		BaseSpell spell = slotIndex == -1 ? (BaseSpell)BasicCast : GetSlot( slotIndex );
		if ( spell == null || spell.Tier >= targetTier ) return false;

		var player = Components.Get<WizardPlayer>( FindMode.EverythingInSelf );
		if ( player == null ) return false;

		int cost = targetTier == 1 ? spell.Tier1Cost : spell.Tier2Cost;
		if ( !player.SpendGalleons( cost ) ) return false;

		spell.Tier = targetTier;
		spell.OnTierChanged();
		return true;
	}

	/// <summary>Reseta tiers de todos os feitiços (novo round/partida).</summary>
	public void ResetTiers()
	{
		BasicCast.Tier = 0;
		foreach ( var s in _slots ) { if ( s != null ) s.Tier = 0; }
	}
}
