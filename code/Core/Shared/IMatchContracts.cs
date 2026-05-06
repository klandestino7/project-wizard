namespace Warlocks;

/// <summary>
/// Contract between the core wizard systems and any game mode.
/// A mode implements these to plug into the core without modifying it.
/// </summary>

/// <summary>
/// Top-level match rule. A game mode attaches one or more of these to configure
/// how a match begins, updates and ends.
/// </summary>
public interface IMatchRule
{
	/// <summary>Called once on match start (host only).</summary>
	void OnMatchStart();

	/// <summary>Called every frame by the mode host tick (host only).</summary>
	void OnMatchTick();

	/// <summary>Called when a player is eliminated (host only).</summary>
	void OnPlayerEliminated( PlayerPawn victim, PlayerPawn killer );
}

/// <summary>
/// Objective rule: a mode implements this when it has a spatial objective
/// that players interact with (plants, captures, payloads, etc.).
/// </summary>
public interface IObjectiveRule
{
	/// <summary>Returns true if <paramref name="player"/> can currently interact with an objective.</summary>
	bool CanInteractWithObjective( PlayerPawn player );

	/// <summary>Called when an objective interaction completes (host only).</summary>
	void OnObjectiveCompleted( PlayerPawn player, string objectiveTag );
}

/// <summary>
/// Respawn rule: controls when and where players are allowed to respawn.
/// </summary>
public interface IRespawnRule
{
	/// <summary>Returns true if <paramref name="player"/> is allowed to respawn right now.</summary>
	bool CanRespawn( PlayerPawn player );

	/// <summary>Returns the world transform the player should spawn at.</summary>
	Transform GetRespawnTransform( PlayerPawn player );
}

/// <summary>
/// Scoring rule: awards points in a mode-specific way.
/// Core systems call these hooks; the mode decides what they mean for score.
/// </summary>
public interface IScoringRule
{
	void OnKill( PlayerPawn killer, PlayerPawn victim );
	void OnAssist( PlayerPawn assistant, PlayerPawn victim );
	void OnObjectiveScore( PlayerPawn player, string objectiveTag );
	void OnRoundWon( Team winner );
}
