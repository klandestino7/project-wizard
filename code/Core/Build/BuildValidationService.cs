namespace Warlocks;

public static class BuildValidationService
{
	public static BuildValidationResult Validate( PlayerBuildSnapshot build, SpellLoadout loadout )
	{
		var result = new BuildValidationResult
		{
			SpellCount = loadout.SpellIndices.Count,
			EnergyUsed = loadout.SpellIndices
				.Select( SpellCatalog.Get )
				.Where( x => x is not null )
				.Sum( x => x.EnergyCost )
		};

		foreach ( var spellIndex in loadout.SpellIndices )
		{
			var entry = SpellCatalog.Get( spellIndex );
			if ( entry is null )
				continue;

			switch ( entry.SpellType )
			{
				case SpellType.Offense:
					result.OffenseCount++;
					break;
				case SpellType.Defense:
					result.DefenseCount++;
					break;
				case SpellType.Control:
					result.ControlCount++;
					break;
				case SpellType.Utility:
					result.UtilityCount++;
					break;
			}

			if ( entry.Affinity == build.Affinity )
				result.MatchingAffinityCount++;
		}

		if ( result.SpellCount > build.PreparedSpellLimit )
			result.Errors.Add( $"Prepared spell limit exceeded: {result.SpellCount}/{build.PreparedSpellLimit}" );

		if ( result.EnergyUsed > build.EnergyBudget )
			result.Errors.Add( $"Energy budget exceeded: {result.EnergyUsed}/{build.EnergyBudget}" );

		if ( result.OffenseCount >= 4 )
			result.DerivedPenalties.Add( "Offense-heavy build: apply extra cooldown penalty." );

		if ( result.DefenseCount >= 4 )
			result.DerivedPenalties.Add( "Defense-heavy build: apply mobility penalty." );

		if ( result.ControlCount >= 4 )
			result.DerivedPenalties.Add( "Control-heavy build: apply damage penalty." );

		switch ( build.Discipline )
		{
			case DisciplineType.Duelist:
				result.DerivedBonuses.Add( "Duelist: offensive spells gain bonus damage." );
				result.DerivedPenalties.Add( "Duelist: lower defensive efficiency." );
				break;
			case DisciplineType.Guardian:
				result.DerivedBonuses.Add( "Guardian: defensive spells gain bonus power." );
				result.DerivedPenalties.Add( "Guardian: lower mobility." );
				break;
			case DisciplineType.Controller:
				result.DerivedBonuses.Add( "Controller: control spells gain bonus duration." );
				result.DerivedPenalties.Add( "Controller: lower direct damage." );
				break;
		}

		if ( result.MatchingAffinityCount > 0 )
			result.DerivedBonuses.Add( $"Affinity synergy active on {result.MatchingAffinityCount} prepared spell(s)." );

		return result;
	}
}

public sealed class BuildValidationResult
{
	public int SpellCount { get; set; }
	public int EnergyUsed { get; set; }
	public int OffenseCount { get; set; }
	public int DefenseCount { get; set; }
	public int ControlCount { get; set; }
	public int UtilityCount { get; set; }
	public int MatchingAffinityCount { get; set; }
	public List<string> Errors { get; } = new();
	public List<string> DerivedBonuses { get; } = new();
	public List<string> DerivedPenalties { get; } = new();
	public bool IsValid => Errors.Count == 0;
}

public sealed class SpellLoadout
{
	public IReadOnlyList<int> SpellIndices { get; init; } = Array.Empty<int>();
}
