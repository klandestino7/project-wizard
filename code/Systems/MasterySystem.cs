
/// <summary>
/// Rastreia quantas vezes cada feitiço acertou durante a partida.
/// Ao atingir o threshold, faz upgrade automático gratuito.
/// Adicione no mesmo GameObject que WizardPlayer.
/// </summary>
public sealed class MasterySystem : Component
{
	// Hits necessários para cada upgrade automático (nome da classe → threshold)
	private static readonly Dictionary<string, int> UpgradeThreshold = new()
	{
		{ "StupefyAbility",       5  },
		{ "IncendioAbility",      4  },
		{ "SectumsempraAbility",  3  },
		{ "ImpedimentaAbility",   4  },
		{ "ProtegoAbility",       6  },
		{ "EpiskeyAbility",       3  },
		{ "DashAbility",          5  },
	};

	// Contagem acumulada por tipo (nome da classe → hits)
	private readonly Dictionary<string, int> _hits = new();

	private WizardPlayer Player { get; set; }

	protected override void OnStart()
	{
		Player = Components.Get<WizardPlayer>( FindMode.InAncestors );
	}

	/// <summary>
	/// Chamado pelos feitiços quando acertam um inimigo.
	/// spellClassName = GetType().Name da ability.
	/// </summary>
	public void RegisterSpellHit( string spellClassName )
	{
		if ( !Networking.IsHost ) return;

		if ( !_hits.ContainsKey( spellClassName ) )
			_hits[spellClassName] = 0;

		_hits[spellClassName]++;

		if ( !UpgradeThreshold.TryGetValue( spellClassName, out int threshold ) ) return;
		if ( _hits[spellClassName] < threshold ) return;

		// Reseta contador para o próximo tier
		_hits[spellClassName] = 0;

		TryFreeUpgrade( spellClassName );
	}

	private void TryFreeUpgrade( string spellClassName )
	{
		var ability = GetAbilityByClassName( spellClassName );
		if ( ability == null || ability.CurrentTier >= 2 ) return;

		int nextTier = ability.CurrentTier + 1;
		ability.CurrentTier = nextTier;
		ability.ApplyTierBonuses();

		BroadcastMasteryUpgrade( ability.AbilityName, nextTier );
	}

	private BaseAbility GetAbilityByClassName( string className )
	{
		foreach ( var ability in new[] { Player.AbilityQ, Player.AbilityE, Player.AbilityR, Player.AbilityF } )
		{
			if ( ability == null ) continue;
			if ( ability.GetType().Name == className ) return ability;
		}
		return null;
	}

	[Rpc.Broadcast]
	private void BroadcastMasteryUpgrade( string spellName, int tier )
	{
		Log.Info( $"[Maestria] {spellName} → Tier {tier} (grátis)" );
		// TODO: mostrar notificação na HUD
	}

	public void ResetForMatch()
	{
		_hits.Clear();
	}
}
