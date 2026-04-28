namespace Warlocks;

public sealed class DefaultRespawnState : Component
{
	[Property] public RespawnState RespawnState { get; set; } = RespawnState.Delayed;
}
