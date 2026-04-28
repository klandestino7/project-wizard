/// <summary>
/// Rastreia quantas vezes cada feitiço acertou durante a partida.
/// Ao atingir o threshold, faz upgrade automático gratuito.
/// Adicione no mesmo GameObject que PlayerPawn.
/// </summary>
public sealed class MasterySystem : Component
{
	// Hits necessários para upgrade automático gratuito (nome da classe → threshold)
	private static readonly Dictionary<string, int> UpgradeThreshold = new()
	{
		{ nameof( StupefySpell ),      5 },
		{ nameof( IncendioSpell ),     4 },
		{ nameof( SectumsempraSpell ), 3 },
		{ nameof( ImpedimentaSpell ),  4 },
		{ nameof( ProtegoSpell ),      6 },
		{ nameof( EpiskeySpell ),      3 },
		{ nameof( DashSpell ),         5 },
	};

	// Contagem acumulada por tipo (nome da classe → hits)
	private readonly Dictionary<string, int> _hits = new();

	private PlayerPawn Player { get; set; }

	protected override void OnStart()
	{
		Player = Components.Get<PlayerPawn>( FindMode.InAncestors );
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
		var spell = GetSpellByClassName( spellClassName );
		if ( spell == null || spell.Tier >= 2 ) return;

		spell.Tier++;
		spell.OnTierChanged();

		BroadcastMasteryUpgrade( spell.SpellName, spell.Tier );
	}

	private BaseSpell GetSpellByClassName( string className )
	{
		var deck = Player.SpellsDeck;
		if ( deck == null ) return null;

		if ( deck.BasicCast.GetType().Name == className ) return deck.BasicCast;

		for ( int i = 0; i < 4; i++ )
		{
			var s = deck.GetSlot( i );
			if ( s != null && s.GetType().Name == className ) return s;
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
