namespace Warlocks;

/// <summary>
/// Applies real runtime effects based on the player's chosen passive.
/// This is the single authority for passive triggers — no passive logic should
/// live in Wand, SpellProjectile or RoundManager directly.
///
/// Hooks consumed externally:
///   OnSpellCast(spell)   — called by Wand after every successful cast
///   OnDamageDealt(dmg)   — called by SpellProjectile.OnHit for the shooter
///   OnKill()             — called by RoundManager.OnPlayerDied for the killer
///
/// Add to the same GameObject as PlayerPawn.
/// </summary>
public sealed class PassiveEffectSystem : Component
{
	// ─── SwiftCaster: speed boost after casting ───────────────────────
	public const float SwiftCasterBoost    = 0.20f; // +20% move speed
	public const float SwiftCasterDuration = 2f;    // seconds

	[Sync] public float SpeedBoostFraction { get; set; } = 0f;
	[Sync] public float SpeedBoostEndTime  { get; set; } = 0f;
	public bool IsSpeedBoosted => Time.Now < SpeedBoostEndTime && SpeedBoostFraction > 0f;

	// ─── ManaSurge: restore mana on damage ───────────────────────────
	public const float ManaSurgeRestorePerDmg = 0.12f; // 12% of damage dealt → mana

	// ─── Executioner: cooldown reset on kill ─────────────────────────
	// (no extra state needed — just calls ResetAllCooldowns)

	private PlayerPawn _player;

	protected override void OnStart()
	{
		_player = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
	}

	/// <summary>Call this from Wand immediately after a spell is successfully executed.</summary>
	public void OnSpellCast( BaseSpell spell )
	{
		if ( !Networking.IsHost ) return;
		if ( _player?.PlayerBuild?.Passive != PassiveType.SwiftCaster ) return;

		SpeedBoostFraction = SwiftCasterBoost;
		SpeedBoostEndTime  = Time.Now + SwiftCasterDuration;
	}

	/// <summary>Call this from SpellProjectile.OnHit for the attacking player.</summary>
	public void OnDamageDealt( float damage )
	{
		if ( !Networking.IsHost || damage <= 0f ) return;
		if ( _player?.PlayerBuild?.Passive != PassiveType.ManaSurge ) return;

		_player.ManaSystem?.Restore( damage * ManaSurgeRestorePerDmg );
	}

	/// <summary>Call this from RoundManager.OnPlayerDied for the killing player.</summary>
	public void OnKill()
	{
		if ( !Networking.IsHost ) return;
		if ( _player?.PlayerBuild?.Passive != PassiveType.Executioner ) return;

		_player.SpellsDeck?.ResetAllCooldowns();
	}

	protected override void OnFixedUpdate()
	{
		// Expire speed boost
		if ( Networking.IsHost && SpeedBoostEndTime > 0f && Time.Now >= SpeedBoostEndTime )
		{
			SpeedBoostFraction = 0f;
			SpeedBoostEndTime  = 0f;
		}
	}
}
