namespace Warlocks;

public partial class PlayerPawn
{
	// ─── Constantes ───────────────────────────────────────────────────
	public const float EyeHeight = 64f;

	// ─── Propriedades sincronizadas ───────────────────────────────────
	[Property, Sync] public int Balance { get; set; } = 0;
	[Property, Sync] public Team Team { get; set; } = Team.Unassigned;
	[Property, Sync] public int Kills { get; set; } = 0;
	[Property, Sync] public int Deaths { get; set; } = 0;

	/// <summary>Alias de conveniência — delega para HealthComponent.</summary>
	public bool IsAlive => HealthComponent.IsValid() && HealthComponent.State == LifeState.Alive;

	// ─── Estado de combate (Hogwarts Legacy style) ────────────────────
	[Sync] public CombatState CombatState { get; set; } = CombatState.Normal;
	[Sync] public float CombatStateEndTime { get; set; } = 0f;

	/// <summary>I-frames ativos (dodge). Dano ignorado enquanto true.</summary>
	[Sync] public bool IsInvincible { get; set; } = false;

	// ─── Stun / Burning / Slow ────────────────────────────────────────
	[Sync] public float StunEndTime { get; set; } = 0f;
	public bool IsStunned => CombatState == CombatState.Stunned;

	[Sync] public float BurningEndTime { get; set; } = 0f;
	[Sync] public float BurningDPS { get; set; } = 0f;
	public PlayerPawn BurningSource { get; set; }

	[Sync] public float SlowEndTime { get; set; } = 0f;
	[Sync] public float SlowFraction { get; set; } = 0f;
	public bool IsSlowed => Time.Now < SlowEndTime && SlowFraction > 0f;

	/// <summary>True enquanto no ar (lançado). Spells de burst fazem +50% dmg.</summary>
	public bool IsAirborne => CombatState == CombatState.Airborne;

	// ─── Referências ─────────────────────────────────────────────────
	[Property] public ManaSystem ManaSystem { get; set; }
	[Property] public DodgeSystem DodgeSystem { get; set; }
	[Property] public LockOnSystem LockOnSystem { get; set; }
	[Property] public WandHolder WandHolder { get; set; }

	// ─── Sistema de feitiços ──────────────────────────────────────────
	[Property] public Wand Wand { get; set; }
	[Property] public SpellsDeck SpellsDeck { get; set; }

	// ─── Itens consumíveis ────────────────────────────────────────────
	[Property] public BaseConsumable ItemSlot1 { get; set; }
	[Property] public BaseConsumable ItemSlot2 { get; set; }

	// ─── Helpers ──────────────────────────────────────────────────────
	public Vector3 EyePosition => WorldPosition + Vector3.Up * EyeHeight;
	public static PlayerPawn Local => Client.Local?.Pawn as PlayerPawn;

	// ─── Lifecycle ────────────────────────────────────────────────────
	public void OnStartOldMethods()
	{
		if ( !IsProxy && BodyRenderer.IsValid() )
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;

		if ( !IsLocallyControlled )
			return;

		LockOnSystem ??= Components.Get<LockOnSystem>();
		WandHolder ??= Components.Get<WandHolder>();
		Wand ??= Components.Get<Wand>();
		SpellsDeck ??= Components.Get<SpellsDeck>();
	}

	public void OnUpdateOldMethods()
	{
		if ( IsProxy )
			return;

		HandleAbilityInput();
		HandleInteractInput();

		if ( Networking.IsHost && CombatStateEndTime > 0f && Time.Now >= CombatStateEndTime )
			SetCombatState( CombatState.Normal );
	}

	public void OnFixedUpdateOldMethods()
	{
		if ( IsProxy || !IsAlive || !Networking.IsHost ) return;

		// Burning DoT — delega dano ao HealthComponent
		if ( BurningEndTime > 0f && Time.Now < BurningEndTime )
		{
			var dot = BurningDPS * Time.Delta;
			if ( dot > 0f )
				HealthComponent.TakeDamage( new DamageInfo( BurningSource, dot, Flags: DamageFlags.Burn ) );
		}
		else if ( BurningEndTime > 0f )
		{
			BurningEndTime = 0f;
			BurningDPS = 0f;
			if ( CombatState == CombatState.Burning )
				SetCombatState( CombatState.Normal );
		}
	}

	// ─── Input: Abilities + Dodge + Lock-on ───────────────────────────
	private void HandleAbilityInput()
	{
		if ( Input.Pressed( "Dodge" ) )
			DodgeSystem?.TryDodge();

		if ( Input.Pressed( "LockOn" ) )
			LockOnSystem?.ToggleLockOn();

		if ( IsStunned ) return;

		if ( Input.Pressed( "Attack1" ) )
			Wand?.TryCastBasic();

		if ( Input.Pressed( "Ability1" ) ) Wand?.TryCastSlot( 0 );
		if ( Input.Pressed( "Ability2" ) ) Wand?.TryCastSlot( 1 );
		if ( Input.Pressed( "Ability3" ) ) Wand?.TryCastSlot( 2 );
		if ( Input.Pressed( "Ability4" ) ) Wand?.TryCastSlot( 3 );

		if ( Input.Pressed( "Item1" ) ) ItemSlot1?.TryUse();
		if ( Input.Pressed( "Item2" ) ) ItemSlot2?.TryUse();
	}

	// ─── Input: Interação (plant/defuse) ──────────────────────────────
	private void HandleInteractInput()
	{
		var nearbySites = Scene.GetAllComponents<HorcruxSite>()
			.Where( site => site.WorldPosition.Distance( WorldPosition ) < site.InteractDistance )
			.OrderBy( site => site.WorldPosition.Distance( WorldPosition ) )
			.ToList();

		if ( !Input.Down( "PlantDefuse" ) )
		{
			foreach ( var site in nearbySites )
				site.StopInteract( this );

			return;
		}

		nearbySites.FirstOrDefault( site => site.CanInteract( this ) )?.TryInteract( this );
	}

	// ─── Estado de combate ────────────────────────────────────────────
	public void SetCombatState( CombatState state, float duration = 0f )
	{
		if ( !Networking.IsHost ) return;
		CombatState = state;
		CombatStateEndTime = duration > 0f ? Time.Now + duration : 0f;
	}

	public void ApplyBurning( float dps, float duration, PlayerPawn source )
	{
		if ( !Networking.IsHost ) return;
		BurningDPS = dps;
		BurningEndTime = Time.Now + duration;
		BurningSource = source;
		SetCombatState( CombatState.Burning, duration );
	}

	public void ExtinguishBurning()
	{
		if ( !Networking.IsHost ) return;
		BurningEndTime = 0f;
		BurningDPS = 0f;
		BurningSource = null;
		if ( CombatState == CombatState.Burning )
			SetCombatState( CombatState.Normal );
	}

	/// <summary>Lança o jogador no ar (estado Airborne). Usado por Accio, Stupefy T2, etc.</summary>
	public void LaunchIntoAir( float height = 250f, float duration = 1.2f )
	{
		if ( !Networking.IsHost ) return;
		SetCombatState( CombatState.Airborne, duration );
		if ( CharacterController.IsValid() )
			CharacterController.Punch( Vector3.Up * height );
	}

	// ─── Economia ─────────────────────────────────────────────────────
	public void GiveGalleons( int amount )
	{
		if ( !Networking.IsHost ) return;
		Client.Balance += amount;
	}

	public bool SpendGalleons( int amount )
	{
		if ( !Networking.IsHost || Client.Balance < amount ) return false;
		Client.Balance -= amount;
		return true;
	}

	// ─── Spell aim ────────────────────────────────────────────────────
	/// <summary>Ponto de origem do feitiço: muzzle da varinha ou EyePosition.</summary>
	public Vector3 GetSpellMuzzle()
	{
		// Bones de animação não são sincronizados confiávelmente para players remotos no host.
		if ( !IsProxy && WandHolder is not null )
			return WandHolder.GetMuzzlePosition();
		return EyePosition;
	}

	/// <summary>
	/// Direção do feitiço: muzzle → aim point (centro da tela via trace).
	/// Passe o muzzle calculado por GetSpellMuzzle() para que a direção aponte corretamente.
	/// </summary>
	public Vector3 GetSpellDirection( Vector3 fromMuzzle )
	{
		Vector3 rayOrigin;
		Vector3 rayForward;

		// Câmera local é confiável apenas para o próprio jogador.
		// Para remotos e bots no host, EyeAngles ([Sync]) é a fonte correta.
		if ( !IsProxy && Camera.IsValid() )
		{
			rayOrigin = Camera.WorldPosition;
			rayForward = Camera.WorldRotation.Forward;
		}
		else
		{
			rayOrigin = EyePosition;
			rayForward = EyeAngles.ToRotation().Forward;
		}

		var tr = Scene.Trace
			.Ray( rayOrigin, rayOrigin + rayForward * 10000f )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "projectile" )
			.Run();

		var aimPoint = tr.Hit ? tr.HitPosition : rayOrigin + rayForward * 5000f;
		return (aimPoint - fromMuzzle).Normal;
	}

	// ─── INetworkListener ─────────────────────────────────────────────
	public void OnActive( Connection connection ) { }
	public void OnDisconnected( Connection connection ) { }
}
