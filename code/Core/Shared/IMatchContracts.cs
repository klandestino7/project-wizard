using Sandbox;

namespace Warlocks;

/// <summary>
/// Contract between the core wizard systems and any game mode.
/// Prefixed with IWarlock to avoid collision with S&box SDK interfaces.
/// </summary>

public interface IWarlockMatchRule
{
	void OnMatchStart();
	void OnMatchTick();
	void OnPlayerEliminated( PlayerPawn victim, PlayerPawn killer );
}

public interface IWarlockObjectiveRule
{
	bool CanInteractWithObjective( PlayerPawn player );
	void OnObjectiveCompleted( PlayerPawn player, string objectiveTag );
}

public interface IWarlockRespawnRule
{
	bool CanRespawn( PlayerPawn player );
	Transform GetRespawnTransform( PlayerPawn player );
}

public interface IWarlockScoringRule
{
	void OnKill( PlayerPawn killer, PlayerPawn victim );
	void OnAssist( PlayerPawn assistant, PlayerPawn victim );
	void OnObjectiveScore( PlayerPawn player, string objectiveTag );
	void OnRoundWon( Team winner );
}
