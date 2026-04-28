using Sandbox.Events;

namespace Warlocks;

/// <summary>
/// Respawn players after a delay.
/// </summary>
public sealed class PlayerAutoRespawner : Respawner,
	IGameEventHandler<UpdateStateEvent>
{
}
