using Sandbox.Events;

namespace Warlocks;

/// <summary>
/// Instantly defuse the bomb if all opponents are dead, and no grenades are nearby.
/// </summary>
public sealed class AllowInstantDefuse : Component,
	IGameEventHandler<BombDefuseStartEvent>
{
	public void OnGameEvent( BombDefuseStartEvent eventArgs )
	{
		var explosive = eventArgs.Bomb?.GetComponent<TimedExplosive>();

		if ( explosive is null ) return;
		if ( AnyTerroristsAlive ) return;
		if ( !HasEnoughTime( explosive ) ) return;

		explosive.FinishDefusing();
	}

	private bool AnyTerroristsAlive => GameUtils.GetPlayerPawns( Team.DarkFollowers )
		.Any( x => x.HealthComponent.State == LifeState.Alive );

	private bool HasEnoughTime( TimedExplosive explosive )
	{
		if ( explosive is null ) return false;

		var untilExplode = explosive.Duration - explosive.TimeSincePlanted;

		// If it's going to be close, let them suffer

		return explosive.DefuseTime < untilExplode - 1;
	}
}

