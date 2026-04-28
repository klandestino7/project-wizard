namespace Warlocks;

public partial class MinimapRenderer
{
    [Property, ImageAssetPath, Group( "This Map" )] public string CurrentMinimapPath { get; set; }
}