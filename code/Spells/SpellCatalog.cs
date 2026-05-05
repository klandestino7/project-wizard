
/// <summary>
/// Registro estático de todos os feitiços disponíveis para compra.
/// Fonte de verdade para custos, nomes, descrições e cores de UI.
/// </summary>
public static class SpellCatalog
{
	public sealed class Entry
	{
		public string ClassName    { get; init; }
		public string Name         { get; init; }
		public string Description  { get; init; }
		public string Category     { get; init; }
		public int    BuyCost      { get; init; }   // Custo de compra (Tier 0)
		public int    Tier1Cost    { get; init; }
		public int    Tier2Cost    { get; init; }
		public float  ManaCost     { get; init; }
		public float  BaseCooldown { get; init; }
		public string AccentColor  { get; init; }   // Cor da UI (#rrggbb)
		public string Image  	   { get; init; }
	}

	public static readonly Entry[] All =
	{
		new() {
			ClassName = nameof(StupefySpell),
			Name = "Stupefy",
			Description = "Projétil de atordoamento. Tier 2 ignora escudos e lança ao ar.",
			Category = "Ofensiva",
			BuyCost = 300, Tier1Cost = 400, Tier2Cost = 700,
			ManaCost = 20f, BaseCooldown = 6f,
			AccentColor = "#ff4444",
			Image = "ui/spells/stupefy.png",
		},
		new() {
			ClassName = nameof(IncendioSpell),
			Name = "Incendio",
			Description = "Projétil de fogo com burning DoT. Tier 2 causa explosão no impacto.",
			Category = "Ofensiva",
			BuyCost = 400, Tier1Cost = 500, Tier2Cost = 900,
			ManaCost = 25f, BaseCooldown = 7f,
			AccentColor = "#ff7722",
			Image = "ui/spells/incendio.png",
		},
		new() {
			ClassName = nameof(SectumsempraSpell),
			Name = "Sectumsempra",
			Description = "Feixe hitscan com bônus de headshot. Tier 2 aplica bleeding.",
			Category = "Ofensiva",
			BuyCost = 600, Tier1Cost = 700, Tier2Cost = 1100,
			ManaCost = 30f, BaseCooldown = 8f,
			AccentColor = "#cc44ff",
			Image = "ui/spells/avada_kedavra.png",
		},
		new() {
			ClassName = nameof(ImpedimentaSpell),
			Name = "Impedimenta",
			Description = "Slow pesado no alvo. Tier 1+ aplica em área.",
			Category = "Controle",
			BuyCost = 500, Tier1Cost = 600, Tier2Cost = 1000,
			ManaCost = 30f, BaseCooldown = 9f,
			AccentColor = "#4488ff",
			Image = "ui/spells/bomba.png",
		},
		new() {
			ClassName = nameof(ProtegoSpell),
			Name = "Protego",
			Description = "Escudo mágico. Ativado em 0.3s = Perfect Block: stuna atacante.",
			Category = "Defesa",
			BuyCost = 400, Tier1Cost = 500, Tier2Cost = 900,
			ManaCost = 30f, BaseCooldown = 6f,
			AccentColor = "#22ddcc",
			Image = "ui/spells/lumos.png",
		},
		new() {
			ClassName = nameof(EpiskeySpell),
			Name = "Episkey",
			Description = "Cura instantânea em si mesmo. Tier 1+ remove debuffs.",
			Category = "Defesa",
			BuyCost = 500, Tier1Cost = 600, Tier2Cost = 1000,
			ManaCost = 25f, BaseCooldown = 10f,
			AccentColor = "#44ff88",
			Image = "ui/spells/silencio.png",
		},
		new() {
			ClassName = nameof(DashSpell),
			Name = "Apparition",
			Description = "Teleporte rápido na direção do olhar. Sem custo de mana.",
			Category = "Utilidade",
			BuyCost = 700, Tier1Cost = 900, Tier2Cost = 1400,
			ManaCost = 0f, BaseCooldown = 20f,
			AccentColor = "#ffcc44",
			Image = "ui/spells/revelium.png",
		},
	};

	/// <summary>Retorna a entrada do catálogo para o tipo de spell dado.</summary>
	public static Entry Find( string className ) =>
		Array.Find( All, e => e.ClassName == className );

	/// <summary>Cria uma nova instância do feitiço dado o nome da classe.</summary>
	public static BaseSpell CreateInstance( string className ) => className switch
	{
		nameof( StupefySpell )      => new StupefySpell(),
		nameof( IncendioSpell )     => new IncendioSpell(),
		nameof( SectumsempraSpell ) => new SectumsempraSpell(),
		nameof( ImpedimentaSpell )  => new ImpedimentaSpell(),
		nameof( ProtegoSpell )      => new ProtegoSpell(),
		nameof( EpiskeySpell )      => new EpiskeySpell(),
		nameof( DashSpell )         => new DashSpell(),
		_                           => null,
	};
}
