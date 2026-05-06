namespace Warlocks;

/// <summary>
/// Mode-agnostic player identity. This is the new core entry point for wizard build data.
/// Uses an action bus (same pattern as SpellsDeck) so the owning client can request changes
/// that the host validates and applies.
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

	// ─── Action bus (client → host) ──────────────────────────────────
	[Sync] public int BuildActionId { get; set; } = 0;
	[Sync] public int PendingAffinityRaw { get; set; } = (int)AffinityType.Arcane;
	[Sync] public int PendingDisciplineRaw { get; set; } = (int)DisciplineType.Generalist;
	[Sync] public int PendingPassiveRaw { get; set; } = (int)PassiveType.None;

	private int _lastActionId;

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;

		if ( BuildActionId != _lastActionId )
		{
			_lastActionId = BuildActionId;
			Affinity   = (AffinityType)PendingAffinityRaw;
			Discipline = (DisciplineType)PendingDisciplineRaw;
			Passive    = (PassiveType)PendingPassiveRaw;
		}
	}

	// ─── Client API ───────────────────────────────────────────────────

	public void ClientSetAffinity( AffinityType value )
	{
		PendingAffinityRaw   = (int)value;
		PendingDisciplineRaw = (int)Discipline;
		PendingPassiveRaw    = (int)Passive;
		BuildActionId++;
	}

	public void ClientSetDiscipline( DisciplineType value )
	{
		PendingAffinityRaw   = (int)Affinity;
		PendingDisciplineRaw = (int)value;
		PendingPassiveRaw    = (int)Passive;
		BuildActionId++;
	}

	public void ClientSetPassive( PassiveType value )
	{
		PendingAffinityRaw   = (int)Affinity;
		PendingDisciplineRaw = (int)Discipline;
		PendingPassiveRaw    = (int)value;
		BuildActionId++;
	}

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
