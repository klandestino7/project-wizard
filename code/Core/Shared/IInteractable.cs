namespace Warlocks;

/// <summary>
/// Generic interaction contract used by the core pawn input system.
/// Mode-specific objectives (HorcruxSite, capture points, etc.) implement this
/// so PlayerPawn never depends on any specific game mode type.
/// </summary>
public interface IInteractable
{
	float InteractDistance { get; }
	bool CanInteract( PlayerPawn player );
	void TryInteract( PlayerPawn player );
	void StopInteract( PlayerPawn player );
}
