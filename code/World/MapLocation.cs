using Sandbox;

namespace Warlocks;

public sealed class MapLocation : Component
{
	[RequireComponent]
	public Zone Zone { get; private set; }
}
