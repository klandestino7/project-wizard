namespace Warlocks;

/// <summary>
/// Tracks and manages the player's ultimate charge.
/// Charge is earned through damage dealt, assists and objectives — never purchased.
/// Each affinity has its own ultimate fantasy; the mode or spell layer decides what fires.
///
/// Add to the same GameObject as PlayerPawn.
/// </summary>
public sealed class UltimateChargeComponent : Component
{
	public const float MaxCharge = 100f;

	// Charge thresholds earned per event type
	public const float ChargePerDamagePoint = 0.08f;
	public const float ChargePerKill        = 12f;
	public const float ChargePerAssist      = 6f;
	public const float ChargePerObjective   = 10f;

	[Sync] public float Charge { get; private set; } = 0f;
	[Sync] public bool  IsReady { get; private set; } = false;

	public float ChargeFraction => Charge / MaxCharge;

	private PlayerPawn _player;

	protected override void OnStart()
	{
		_player = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
	}

	/// <summary>Award charge from damage dealt. Call this whenever the player deals damage.</summary>
	public void AddDamageCharge( float damageDealt )
	{
		if ( !Networking.IsHost ) return;
		AddCharge( damageDealt * ChargePerDamagePoint );
	}

	/// <summary>Award flat charge for a kill.</summary>
	public void AddKillCharge() => AddCharge( ChargePerKill );

	/// <summary>Award flat charge for an assist.</summary>
	public void AddAssistCharge() => AddCharge( ChargePerAssist );

	/// <summary>Award flat charge for an objective interaction (plant, defuse, capture, etc.).</summary>
	public void AddObjectiveCharge() => AddCharge( ChargePerObjective );

	/// <summary>
	/// Consumes the full charge to trigger the ultimate.
	/// Returns true if the ultimate was ready and is now consumed.
	/// </summary>
	public bool TryConsumeUltimate()
	{
		if ( !Networking.IsHost || !IsReady ) return false;
		Charge  = 0f;
		IsReady = false;
		return true;
	}

	/// <summary>Reset charge (e.g. on round start).</summary>
	public void Reset()
	{
		if ( !Networking.IsHost ) return;
		Charge  = 0f;
		IsReady = false;
	}

	private void AddCharge( float amount )
	{
		if ( !Networking.IsHost || amount <= 0f ) return;
		Charge  = MathF.Min( Charge + amount, MaxCharge );
		IsReady = Charge >= MaxCharge;
	}
}
