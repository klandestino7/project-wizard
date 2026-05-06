namespace Warlocks;

/// <summary>
/// Tracks recent ComboTags applied to a target and resolves elemental combo interactions.
/// Lives on the target PlayerPawn (victim side).
/// The attacker's Wand calls RecordHit() after each successful spell hit.
///
/// Add to the same GameObject as PlayerPawn.
/// </summary>
public sealed class SpellComboSystem : Component
{
	/// <summary>How long a tag stays active for combo purposes.</summary>
	private const float TagWindow = 3f;

	private struct TagEntry
	{
		public ComboTag Tag;
		public float    ExpiresAt;
	}

	private readonly List<TagEntry> _activeTags = new();

	/// <summary>
	/// Register a hit on this target. Call from the attacker's spell execution context.
	/// Host-only.
	/// </summary>
	public void RecordHit( ComboTag tags, PlayerPawn attacker )
	{
		if ( !Networking.IsHost || tags == ComboTag.None ) return;

		// Expire stale entries
		var now = Time.Now;
		_activeTags.RemoveAll( e => e.ExpiresAt < now );

		// Check combos BEFORE adding the new tag so we don't self-trigger
		var combo = ResolveCombo( tags );
		if ( combo != ComboEffect.None )
		{
			ApplyCombo( combo, attacker );
		}

		// Add all bits as individual entries
		foreach ( ComboTag bit in Enum.GetValues( typeof(ComboTag) ) )
		{
			if ( bit == ComboTag.None ) continue;
			if ( (tags & bit) == 0 ) continue;

			// Refresh or add
			int idx = _activeTags.FindIndex( e => e.Tag == bit );
			if ( idx >= 0 )
				_activeTags[idx] = new TagEntry { Tag = bit, ExpiresAt = now + TagWindow };
			else
				_activeTags.Add( new TagEntry { Tag = bit, ExpiresAt = now + TagWindow } );
		}
	}

	private ComboTag ActiveTagMask()
	{
		var now  = Time.Now;
		var mask = ComboTag.None;
		foreach ( var e in _activeTags )
			if ( e.ExpiresAt >= now )
				mask |= e.Tag;
		return mask;
	}

	private ComboEffect ResolveCombo( ComboTag incoming )
	{
		var existing = ActiveTagMask();

		// Ice + Impact → Freeze (shatter)
		if ( incoming.HasFlag( ComboTag.Impact ) && existing.HasFlag( ComboTag.Ice ) )
			return ComboEffect.Freeze;
		if ( incoming.HasFlag( ComboTag.Ice ) && existing.HasFlag( ComboTag.Impact ) )
			return ComboEffect.Freeze;

		// Fire + Slow → Evaporate (burn clears slow, deals burst)
		if ( incoming.HasFlag( ComboTag.Fire ) && existing.HasFlag( ComboTag.Slow ) )
			return ComboEffect.Evaporate;
		if ( incoming.HasFlag( ComboTag.Slow ) && existing.HasFlag( ComboTag.Fire ) )
			return ComboEffect.Evaporate;

		// Shadow + Control → Silence
		if ( incoming.HasFlag( ComboTag.Shadow ) && existing.HasFlag( ComboTag.Slow ) )
			return ComboEffect.Silence;

		// Arcane + Shield → Shatter (punish blocking)
		if ( incoming.HasFlag( ComboTag.Arcane ) && existing.HasFlag( ComboTag.Shield ) )
			return ComboEffect.ShieldShatter;

		return ComboEffect.None;
	}

	private void ApplyCombo( ComboEffect effect, PlayerPawn attacker )
	{
		var victim = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
		if ( victim == null ) return;

		switch ( effect )
		{
			case ComboEffect.Freeze:
				// Stun + clear slow
				victim.SetCombatState( CombatState.Stunned, 2f );
				victim.SlowEndTime  = 0f;
				victim.SlowFraction = 0f;
				BroadcastCombo( "FREEZE", victim.WorldPosition );
				break;

			case ComboEffect.Evaporate:
				// Extinguish burn + deal bonus burst damage
				victim.ExtinguishBurning();
				victim.HealthComponent?.TakeDamage( new DamageInfo( attacker, 40f, Flags: DamageFlags.Burn ) );
				BroadcastCombo( "EVAPORATE", victim.WorldPosition );
				break;

			case ComboEffect.Silence:
				// Extra stun (no casting)
				victim.SetCombatState( CombatState.Stunned, 1.5f );
				BroadcastCombo( "SILENCE", victim.WorldPosition );
				break;

			case ComboEffect.ShieldShatter:
				// Destroy shield and stagger
				victim.Components.Get<ProtegoComponent>( FindMode.EverythingInSelf )
					?.AbsorbDamage( 9999 );
				BroadcastCombo( "SHATTER", victim.WorldPosition );
				break;
		}

		// Clear tags that were consumed by the combo
		_activeTags.Clear();
	}

	[Rpc.Broadcast]
	private void BroadcastCombo( string label, Vector3 position )
	{
		Log.Info( $"[Combo] {label} at {position}" );
		// TODO: spawn combo VFX / screen notification here
	}
}

public enum ComboEffect
{
	None,
	Freeze,
	Evaporate,
	Silence,
	ShieldShatter
}
