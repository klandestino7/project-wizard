namespace Warlocks;

/// <summary>
/// Mode-agnostic player identity. This is the new core entry point for wizard build data.
/// </summary>
public sealed class PlayerBuildComponent : Component, IPlayerBuildProvider
{
	public const int DefaultEnergyBudget = 10;
	public const int DefaultPreparedSpellLimit = 6;

	[Property, Sync] public AffinityType Affinity { get; set; } = AffinityType.Arcane;
	[Property, Sync] public DisciplineType Discipline { get; set; } = DisciplineType.Generalist;
	[Property, Sync] public PassiveType Passive { get; set; } = PassiveType.None;
	[Property, Sync] public int EnergyBudget { get; set; } = DefaultEnergyBudget;
	[Property, Sync] public int PreparedSpellLimit { get; set; } = DefaultPreparedSpellLimit;

	public PlayerBuildSnapshot GetBuildSnapshot()
	{
		return new()
		{
			Affinity = Affinity,
			Discipline = Discipline,
			Passive = Passive,
			EnergyBudget = EnergyBudget,
			PreparedSpellLimit = PreparedSpellLimit
		};
	}
}

public struct PlayerBuildSnapshot
{
	public AffinityType Affinity { get; init; }
	public DisciplineType Discipline { get; init; }
	public PassiveType Passive { get; init; }
	public int EnergyBudget { get; init; }
	public int PreparedSpellLimit { get; init; }
}
