using Sandbox.Events;

namespace Warlocks;

/// <summary>
/// Swap teams when entering this state.
/// </summary>
public sealed class SwapTeams : Component,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Swap();
	}

	[DeveloperCommand( "Swap Teams", "Game Loop" )]
	public static void Swap()
	{
		var ts = GameUtils.GetPlayers( Team.DarkFollowers ).ToArray();
		var cts = GameUtils.GetPlayers( Team.Aurors ).ToArray();

		foreach ( var player in ts )
		{
			player.AssignTeam( Team.Aurors );
		}

		foreach ( var player in cts )
		{
			player.AssignTeam( Team.DarkFollowers );
		}

		Game.ActiveScene.Dispatch( new TeamsSwappedEvent() );
	}
}
