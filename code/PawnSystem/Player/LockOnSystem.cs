namespace Warlocks;

/// <summary>
/// Soft lock-on (Hogwarts Legacy style).
/// Toggle com "LockOn". Mantém câmera centrada no alvo.
/// Spells com tracking usam GetAimTarget() para definir direção.
/// </summary>
public sealed class LockOnSystem : Component
{
	[Property] public float MaxRange { get; set; } = 1200f;
	[Property] public float SwitchTargetRange { get; set; } = 400f;

	[Sync] public bool IsLockedOn { get; private set; } = false;

	public PlayerPawn LockedTarget { get; private set; }

	private PlayerPawn Player { get; set; }

	protected override void OnStart()
	{
		Player = Components.Get<PlayerPawn>( FindMode.InAncestors );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		// Valida alvo: morreu ou saiu do range → cancela
		if ( IsLockedOn )
		{
			if ( !LockedTarget.IsValid() || !LockedTarget.IsAlive
				|| Player.WorldPosition.Distance( LockedTarget.WorldPosition ) > MaxRange * 1.3f )
			{
				ClearLockOn();
				return;
			}

			// Câmera soft: aponta suavemente o EyeAngles para o alvo
			if ( !IsProxy && Player.IsValid() )
				AimTowardsTarget();
		}
	}

	// ─── Toggle ───────────────────────────────────────────────────────
	public void ToggleLockOn()
	{
		if ( Player is null ) return;

		if ( IsLockedOn )
		{
			ClearLockOn();
			return;
		}

		var target = FindBestTarget();
		if ( target != null )
			SetLockOn( target );
	}

	public void SetLockOn( PlayerPawn target )
	{
		LockedTarget = target;
		IsLockedOn = target != null;
	}

	public void ClearLockOn()
	{
		LockedTarget = null;
		IsLockedOn = false;
	}

	// ─── Aiming ───────────────────────────────────────────────────────
	private void AimTowardsTarget()
	{
		var targetPos = LockedTarget.WorldPosition + Vector3.Up * PlayerPawn.EyeHeight;
		var toTarget = (targetPos - Player.EyePosition).Normal;
		var desiredAngles = Rotation.LookAt( toTarget ).Angles();

		var current = Player.EyeAngles;
		// Interpolação suave (soft lock ≠ hard snap)
		current.yaw = Angles.Lerp( current, desiredAngles, Time.Delta * 5f ).yaw;
		current.pitch = Angles.Lerp( current, desiredAngles, Time.Delta * 5f ).pitch;
		Player.EyeAngles = current;
	}

	/// <summary>
	/// Retorna a direção de mira. Se locked-on, aponta para o alvo; senão, para frente.
	/// </summary>
	public Vector3 GetAimDirection()
	{
		if ( IsLockedOn && LockedTarget.IsValid() )
		{
			var toTarget = (LockedTarget.WorldPosition + Vector3.Up * PlayerPawn.EyeHeight
				- Player.EyePosition).Normal;
			return toTarget;
		}
		return Player.EyeAngles.ToRotation().Forward;
	}

	/// <summary>Retorna o centro do alvo para spells de tracking.</summary>
	public Vector3? GetAimTarget()
	{
		if ( IsLockedOn && LockedTarget.IsValid() )
			return LockedTarget.WorldPosition + Vector3.Up * PlayerPawn.EyeHeight * 0.8f;
		return null;
	}

	// ─── Find ─────────────────────────────────────────────────────────
	private PlayerPawn FindBestTarget()
	{
		PlayerPawn best = null;
		float bestScore = float.MaxValue;
		var camForward = Player.EyeAngles.ToRotation().Forward;

		foreach ( var p in Scene.GetAllComponents<PlayerPawn>() )
		{
			if ( p == Player || !p.IsAlive || p.Team == Player.Team ) continue;

			float dist = Player.WorldPosition.Distance( p.WorldPosition );
			if ( dist > MaxRange ) continue;

			// Checar linha de visão
			var tr = Scene.Trace
				.Ray( Player.EyePosition, p.EyePosition )
				.IgnoreGameObjectHierarchy( Player.GameObject )
				.Run();
			if ( tr.Hit && tr.GameObject != p.GameObject ) continue;

			// Preferir alvo próximo ao centro da câmera
			var toTarget = (p.WorldPosition - Player.WorldPosition).Normal;
			float angleDiff = Vector3.Dot( camForward, toTarget ); // 1 = frente, -1 = atrás
			float score = dist * (2f - angleDiff); // menor = melhor

			if ( score < bestScore )
			{
				bestScore = score;
				best = p;
			}
		}

		return best;
	}
}
