namespace Warlocks;

/// <summary>
/// Gameplay registry for spells. Legacy economy fields remain temporarily for compatibility,
/// but new systems should prefer affinity, type, energy and combo metadata.
/// </summary>
public static class SpellCatalog
{
	public sealed class Entry
	{
		public string ClassName { get; init; }
		public string Name { get; init; }
		public string Description { get; init; }
		public SpellType SpellType { get; init; }
		public AffinityType Affinity { get; init; }
		public int EnergyCost { get; init; }
		public ComboTag ComboTags { get; init; }
		public string[] TierBehaviors { get; init; } = Array.Empty<string>();
		public int BuyCost { get; init; }
		public int Tier1Cost { get; init; }
		public int Tier2Cost { get; init; }
		public float ManaCost { get; init; }
		public float BaseCooldown { get; init; }
		public string AccentColor { get; init; }
		public string Image { get; init; }

		public string Category => SpellType switch
		{
			SpellType.Offense => "Ofensiva",
			SpellType.Control => "Controle",
			SpellType.Defense => "Defesa",
			_ => "Utilidade"
		};
	}

	public static readonly Entry[] All =
	{
		new()
		{
			ClassName = nameof( StupefySpell ),
			Name = "Stupefy",
			Description = "Projetil de atordoamento. Tier 2 ignora escudos e lanca ao ar.",
			SpellType = SpellType.Offense,
			Affinity = AffinityType.Arcane,
			EnergyCost = 2,
			ComboTags = ComboTag.Arcane | ComboTag.Impact | ComboTag.Projectile,
			TierBehaviors = new[]
			{
				"Single projectile stun.",
				"More reliable stagger pressure.",
				"Pierces shields and launches targets airborne."
			},
			BuyCost = 300,
			Tier1Cost = 400,
			Tier2Cost = 700,
			ManaCost = 20f,
			BaseCooldown = 6f,
			AccentColor = "#ff4444",
			Image = "ui/spells/stupefy.png"
		},
		new()
		{
			ClassName = nameof( IncendioSpell ),
			Name = "Incendio",
			Description = "Projetil de fogo com burn. Tier 2 causa explosao no impacto.",
			SpellType = SpellType.Offense,
			Affinity = AffinityType.Fire,
			EnergyCost = 3,
			ComboTags = ComboTag.Fire | ComboTag.Projectile,
			TierBehaviors = new[]
			{
				"Single fire projectile with burn.",
				"Stronger burn application.",
				"Explodes on impact and threatens an area."
			},
			BuyCost = 400,
			Tier1Cost = 500,
			Tier2Cost = 900,
			ManaCost = 25f,
			BaseCooldown = 7f,
			AccentColor = "#ff7722",
			Image = "ui/spells/incendio.png"
		},
		new()
		{
			ClassName = nameof( SectumsempraSpell ),
			Name = "Sectumsempra",
			Description = "Feixe hitscan com bonus de headshot. Tier 2 aplica bleeding.",
			SpellType = SpellType.Offense,
			Affinity = AffinityType.Shadow,
			EnergyCost = 4,
			ComboTags = ComboTag.Shadow | ComboTag.Impact,
			TierBehaviors = new[]
			{
				"Precise burst hitscan.",
				"Higher pressure on precision hits.",
				"Applies bleeding for finisher pressure."
			},
			BuyCost = 600,
			Tier1Cost = 700,
			Tier2Cost = 1100,
			ManaCost = 30f,
			BaseCooldown = 8f,
			AccentColor = "#cc44ff",
			Image = "ui/spells/avada_kedavra.png"
		},
		new()
		{
			ClassName = nameof( ImpedimentaSpell ),
			Name = "Impedimenta",
			Description = "Slow pesado no alvo. Tier 1+ aplica em area.",
			SpellType = SpellType.Control,
			Affinity = AffinityType.Ice,
			EnergyCost = 2,
			ComboTags = ComboTag.Ice | ComboTag.Slow | ComboTag.Projectile,
			TierBehaviors = new[]
			{
				"Single target heavy slow.",
				"Slow expands to nearby targets.",
				"Area control denial with stronger crowd control."
			},
			BuyCost = 500,
			Tier1Cost = 600,
			Tier2Cost = 1000,
			ManaCost = 30f,
			BaseCooldown = 9f,
			AccentColor = "#4488ff",
			Image = "ui/spells/bomba.png"
		},
		new()
		{
			ClassName = nameof( ProtegoSpell ),
			Name = "Protego",
			Description = "Escudo magico. Perfect block stuna o atacante.",
			SpellType = SpellType.Defense,
			Affinity = AffinityType.Arcane,
			EnergyCost = 2,
			ComboTags = ComboTag.Arcane | ComboTag.Shield,
			TierBehaviors = new[]
			{
				"Reactive shield.",
				"More stable protection window.",
				"Perfect block punishes the attacker harder."
			},
			BuyCost = 400,
			Tier1Cost = 500,
			Tier2Cost = 900,
			ManaCost = 30f,
			BaseCooldown = 6f,
			AccentColor = "#22ddcc",
			Image = "ui/spells/lumos.png"
		},
		new()
		{
			ClassName = nameof( EpiskeySpell ),
			Name = "Episkey",
			Description = "Cura instantanea em si mesmo. Tier 1+ remove debuffs.",
			SpellType = SpellType.Defense,
			Affinity = AffinityType.Arcane,
			EnergyCost = 2,
			ComboTags = ComboTag.Arcane | ComboTag.Healing,
			TierBehaviors = new[]
			{
				"Immediate self-heal.",
				"Heal becomes safer and more reliable.",
				"Also clears debuffs and stabilizes recovery."
			},
			BuyCost = 500,
			Tier1Cost = 600,
			Tier2Cost = 1000,
			ManaCost = 25f,
			BaseCooldown = 10f,
			AccentColor = "#44ff88",
			Image = "ui/spells/silencio.png"
		},
		new()
		{
			ClassName = nameof( DashSpell ),
			Name = "Apparition",
			Description = "Teleporte rapido na direcao do olhar. Sem custo de mana.",
			SpellType = SpellType.Utility,
			Affinity = AffinityType.Shadow,
			EnergyCost = 1,
			ComboTags = ComboTag.Shadow | ComboTag.Mobility,
			TierBehaviors = new[]
			{
				"Quick reposition teleport.",
				"Faster recovery after blink.",
				"More aggressive mobility and spacing control."
			},
			BuyCost = 700,
			Tier1Cost = 900,
			Tier2Cost = 1400,
			ManaCost = 0f,
			BaseCooldown = 20f,
			AccentColor = "#ffcc44",
			Image = "ui/spells/revelium.png"
		}
	};

	public static Entry Find( string className ) =>
		Array.Find( All, e => e.ClassName == className );

	public static Entry Get( int index )
	{
		if ( index < 0 || index >= All.Length )
			return null;

		return All[index];
	}

	public static BaseSpell CreateInstance( string className ) => className switch
	{
		nameof( StupefySpell ) => new StupefySpell(),
		nameof( IncendioSpell ) => new IncendioSpell(),
		nameof( SectumsempraSpell ) => new SectumsempraSpell(),
		nameof( ImpedimentaSpell ) => new ImpedimentaSpell(),
		nameof( ProtegoSpell ) => new ProtegoSpell(),
		nameof( EpiskeySpell ) => new EpiskeySpell(),
		nameof( DashSpell ) => new DashSpell(),
		_ => null
	};
}
