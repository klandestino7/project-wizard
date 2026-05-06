namespace Warlocks;

/// <summary>
/// Dispatches the ultimate ability effect based on the caster's affinity.
/// Called by PlayerPawn.TryUseUltimate() after charge is consumed.
/// All logic runs on the host.
/// </summary>
public static class UltimateEffects
{
	private const float AoeRadius = 400f;
	private const float AoeDamage = 80f;
	private const float BurnDPS   = 25f;
	private const float BurnDur   = 4f;
	private const float SlowFrac  = 0.55f;
	private const float SlowDur   = 3f;
	private const float ShadowDmg = 120f;
	private const float StunDur   = 1.5f;

	public static void Fire( PlayerPawn caster, AffinityType affinity )
	{
		if ( !Networking.IsHost || !caster.IsValid() ) return;

		switch ( affinity )
		{
			case AffinityType.Fire:   FireUltimate( caster );    break;
			case AffinityType.Ice:    IceUltimate( caster );     break;
			case AffinityType.Shadow: ShadowUltimate( caster );  break;
			case AffinityType.Arcane: ArcaneUltimate( caster );  break;
			default:                  NeutralUltimate( caster );  break;
		}
	}

	// ─── Fire: AoE burst + lingering burn ────────────────────────────
	private static void FireUltimate( PlayerPawn caster )
	{
		foreach ( var victim in GetNearbyEnemies( caster, AoeRadius ) )
		{
			victim.HealthComponent.TakeDamage( new DamageInfo( caster, AoeDamage, Flags: DamageFlags.Burn ) );
			victim.ApplyBurning( BurnDPS, BurnDur, caster );
		}
	}

	// ─── Ice: mass slow + brief stun ─────────────────────────────────
	private static void IceUltimate( PlayerPawn caster )
	{
		foreach ( var victim in GetNearbyEnemies( caster, AoeRadius ) )
		{
			victim.SetCombatState( CombatState.Stunned, StunDur );
			victim.SlowFraction = SlowFrac;
			victim.SlowEndTime  = Time.Now + SlowDur;
		}
	}

	// ─── Shadow: blink to locked target + burst + AoE half-damage ────
	private static void ShadowUltimate( PlayerPawn caster )
	{
		var target = caster.LockOnSystem?.LockedTarget
		          ?? GetNearestEnemy( caster, AoeRadius );

		if ( target.IsValid() )
		{
			var behind = target.WorldPosition - target.EyeAngles.ToRotation().Forward * 80f;
			caster.WorldPosition = behind;

			target.HealthComponent.TakeDamage( new DamageInfo( caster, ShadowDmg ) );
			target.SetCombatState( CombatState.Stunned, StunDur );
		}

		foreach ( var victim in GetNearbyEnemies( caster, AoeRadius ) )
		{
			if ( victim == target ) continue;
			victim.HealthComponent.TakeDamage( new DamageInfo( caster, ShadowDmg * 0.5f ) );
		}
	}

	// ─── Arcane: reset all cooldowns + restore 50% mana ──────────────
	private static void ArcaneUltimate( PlayerPawn caster )
	{
		caster.SpellsDeck?.ResetAllCooldowns();
		caster.ManaSystem?.Restore( ManaSystem.MaxMana * 0.5f );
	}

	// ─── Neutral: AoE impact + launch airborne ────────────────────────
	private static void NeutralUltimate( PlayerPawn caster )
	{
		foreach ( var victim in GetNearbyEnemies( caster, AoeRadius ) )
		{
			victim.HealthComponent.TakeDamage( new DamageInfo( caster, AoeDamage ) );
			victim.LaunchIntoAir( 200f, 0.8f );
		}
	}

	// ─── Helpers ──────────────────────────────────────────────────────
	private static IEnumerable<PlayerPawn> GetNearbyEnemies( PlayerPawn caster, float radius )
	{
		return caster.Scene.GetAllComponents<PlayerPawn>()
			.Where( p => p.IsValid()
			          && p != caster
			          && p.Team != caster.Team
			          && p.IsAlive
			          && p.WorldPosition.Distance( caster.WorldPosition ) <= radius );
	}

	private static PlayerPawn GetNearestEnemy( PlayerPawn caster, float radius )
	{
		return GetNearbyEnemies( caster, radius )
			.OrderBy( p => p.WorldPosition.Distance( caster.WorldPosition ) )
			.FirstOrDefault();
	}
}
