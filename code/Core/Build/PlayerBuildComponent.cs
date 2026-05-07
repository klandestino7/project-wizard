using Sandbox;

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

	/// <summary>
	/// Set to true by the client (network owner) when they confirm their build during BuildPhase.
	/// The host reads this to track how many players have confirmed.
	/// </summary>
	[Sync] public bool BuildConfirmed { get; set; } = false;

	// ─── Action bus (client → host) ──────────────────────────────────
	[Sync] public int BuildActionId { get; set; } = 0;
	[Sync] public int ConfirmBuildActionId { get; set; } = 0;
	[Sync] public int PendingAffinityRaw { get; set; } = (int)AffinityType.Arcane;
	[Sync] public int PendingDisciplineRaw { get; set; } = (int)DisciplineType.Generalist;
	[Sync] public int PendingPassiveRaw { get; set; } = (int)PassiveType.None;

	private int _lastActionId;
	private int _lastConfirmBuildActionId;

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;

		if ( BuildActionId != _lastActionId )
		{
			_lastActionId = BuildActionId;
			var rm = RoundManager.Instance;
			Affinity   = (rm?.EnableAffinity   ?? true) ? (AffinityType)PendingAffinityRaw     : AffinityType.Neutral;
			Discipline = (rm?.EnableDiscipline ?? true) ? (DisciplineType)PendingDisciplineRaw : DisciplineType.Generalist;
			Passive    = (rm?.EnablePassive    ?? true) ? (PassiveType)PendingPassiveRaw       : PassiveType.None;
		}

		if ( ConfirmBuildActionId != _lastConfirmBuildActionId )
		{
			_lastConfirmBuildActionId = ConfirmBuildActionId;
			BuildConfirmed = true;
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

	public void ClientConfirmBuild()
	{
		ConfirmBuildActionId++;
	}

	/// <summary>Host-only. Assigns a random identity and marks the player as confirmed.</summary>
	public void AssignRandomBuild()
	{
		if ( !Networking.IsHost ) return;

		var rm  = RoundManager.Instance;
		var rng = new Random();

		if ( rm?.EnableAffinity ?? true )
		{
			var affinities = Enum.GetValues<AffinityType>()
				.Where( a => a != AffinityType.Neutral ).ToArray();
			Affinity = affinities[rng.Next( affinities.Length )];
		}
		else
		{
			Affinity = AffinityType.Neutral;
		}

		if ( rm?.EnableDiscipline ?? true )
		{
			var disciplines = Enum.GetValues<DisciplineType>().ToArray();
			Discipline = disciplines[rng.Next( disciplines.Length )];
		}
		else
		{
			Discipline = DisciplineType.Generalist;
		}

		if ( rm?.EnablePassive ?? true )
		{
			var passives = Enum.GetValues<PassiveType>().ToArray();
			Passive = passives[rng.Next( passives.Length )];
		}
		else
		{
			Passive = PassiveType.None;
		}

		BuildConfirmed = true;
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
