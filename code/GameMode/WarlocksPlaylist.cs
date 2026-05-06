namespace Warlocks;

public static class WarlocksPlaylist
{
	public const string WarlocksScenePath = "scenes/maps/WarlocksAtrium/WarlocksAtrium.scene";

	public static SceneFile GetWarlocksScene()
	{
		return ResourceLibrary.Get<SceneFile>( WarlocksScenePath );
	}
}
