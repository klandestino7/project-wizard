
/// <summary>
/// Componente principal do jogador. Adicione em um GameObject junto com
/// CharacterController. Crie um filho "Camera" com CameraComponent.
/// </summary>
public sealed class WizardPlayer : Component, Component.INetworkListener
{
	// ─── Constantes ───────────────────────────────────────────────────
	public const int MaxHealth = 150;
	public const int MaxShield = 50;
	public const float EyeHeight = 64f;
	public const float WalkSpeed = 180f;
	public const float RunSpeed = 300f;
	public const float CrouchSpeed = 80f;

	// ─── Propriedades sincronizadas ───────────────────────────────────
	[Property, Sync] public int Health { get; set; } = MaxHealth;
	[Property, Sync] public int Shield { get; set; } = 0;
	[Property, Sync] public int Galleons { get; set; } = 0;
	[Property, Sync] public Team Team { get; set; } = Team.None;
	[Property, Sync] public bool IsAlive { get; set; } = true;
	[Property, Sync] public Angles EyeAngles { get; set; }
	[Property, Sync] public int Kills { get; set; } = 0;
	[Property, Sync] public int Deaths { get; set; } = 0;

	// ─── Estado de combate (Hogwarts Legacy style) ────────────────────
	[Sync] public CombatState CombatState { get; set; } = CombatState.Normal;
	[Sync] public float CombatStateEndTime { get; set; } = 0f;

	/// <summary>I-frames ativos (dodge). TakeDamage ignorado enquanto true.</summary>
	[Sync] public bool IsInvincible { get; set; } = false;

	// ─── Stun / Burning / Slow ────────────────────────────────────────
	[Sync] public float StunEndTime { get; set; } = 0f;
	public bool IsStunned => CombatState == CombatState.Stunned;

	[Sync] public float BurningEndTime { get; set; } = 0f;
	[Sync] public float BurningDPS { get; set; } = 0f;
	public WizardPlayer BurningSource { get; set; }

	[Sync] public float SlowEndTime { get; set; } = 0f;
	[Sync] public float SlowFraction { get; set; } = 0f;
	public bool IsSlowed => Time.Now < SlowEndTime && SlowFraction > 0f;

	/// <summary>True enquanto no ar (lançado). Spells de burst fazem +50% dmg.</summary>
	public bool IsAirborne => CombatState == CombatState.Airborne;

	// ─── Referências (setar no editor ou via GameManager) ─────────────
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public ModelRenderer BodyRenderer { get; set; }
	[Property] public ManaSystem ManaSystem { get; set; }
	[Property] public DodgeSystem DodgeSystem { get; set; }
	[Property] public LockOnSystem LockOnSystem { get; set; }
	[Property] public WandHolder WandHolder { get; set; }

	// ─── Sistema de feitiços ──────────────────────────────────────────
	[Property] public Wand       Wand       { get; set; }
	[Property] public SpellsDeck SpellsDeck { get; set; }

	// ─── Itens consumíveis ────────────────────────────────────────────
	[Property] public BaseConsumable ItemSlot1 { get; set; }
	[Property] public BaseConsumable ItemSlot2 { get; set; }

	// ─── Helpers ──────────────────────────────────────────────────────
	public Vector3 EyePosition => WorldPosition + Vector3.Up * EyeHeight;
	public static WizardPlayer Local => Game.ActiveScene
		.GetAllComponents<WizardPlayer>()
		.FirstOrDefault( p => !p.IsProxy );

	// ─── Lifecycle ────────────────────────────────────────────────────
	protected override void OnStart()
	{
		// 3ª pessoa: corpo visível no cliente local também
		if ( !IsProxy && BodyRenderer.IsValid() )
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
	
    	LockOnSystem = Components.Get<LockOnSystem>();
	
		WandHolder = Components.Get<WandHolder>();
		Camera     = Scene.Get<CameraComponent>();
		Wand       = Components.Get<Wand>();
		SpellsDeck = Components.Get<SpellsDeck>();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
		{
			UpdateProxy();
			return;
		}

		if ( !IsAlive ) return;

		// NÃO PRECISA DESSE HandleLook, O prefab do player já possui
		// HandleLook();
		HandleAbilityInput();
		HandleInteractInput();

		// Expirar estados de combate
		if ( Networking.IsHost && CombatStateEndTime > 0f && Time.Now >= CombatStateEndTime )
			SetCombatState( CombatState.Normal );
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy || !IsAlive ) return;

		// NÃO PRECISA DESSE HandleMovement, O prefab do player já possui
		// HandleMovement();

		if ( !Networking.IsHost ) return;

		// Burning DoT
		if ( BurningEndTime > 0f && Time.Now < BurningEndTime )
		{
			int dot = (int)(BurningDPS * Time.Delta);
			if ( dot > 0 )
				TakeDamage( dot, BurningSource );
		}
		else if ( BurningEndTime > 0f )
		{
			BurningEndTime = 0f;
			BurningDPS = 0f;
			if ( CombatState == CombatState.Burning )
				SetCombatState( CombatState.Normal );
		}
	}

	// ─── Input: Look ──────────────────────────────────────────────────
	private void HandleLook()
	{
		var delta = Input.AnalogLook;
		var angles = EyeAngles;
		angles += delta;
		angles.pitch = angles.pitch.Clamp( -89f, 89f );
		angles.roll = 0f;
		EyeAngles = angles;

		WorldRotation = Rotation.FromYaw( EyeAngles.yaw );

		if ( Camera.IsValid() )
			Camera.LocalRotation = Rotation.FromPitch( EyeAngles.pitch );
	}

	// ─── Input: Movimento ─────────────────────────────────────────────
	private void HandleMovement()
	{
		var cc = CharacterController;
		if ( !cc.IsValid() ) return;

		if ( !cc.IsOnGround )
			cc.Velocity += Vector3.Down * 850f * Time.Delta;

		var forward = Rotation.FromYaw( EyeAngles.yaw ).Forward;
		var right = Rotation.FromYaw( EyeAngles.yaw ).Right;
		var wishDir = Vector3.Zero;

		if ( Input.Down( "Forward" ) ) wishDir += forward;
		if ( Input.Down( "Backward" ) ) wishDir -= forward;
		if ( Input.Down( "Left" ) ) wishDir -= right;
		if ( Input.Down( "Right" ) ) wishDir += right;

		wishDir = wishDir.WithZ( 0f ).Normal;

		float speed = Input.Down( "Duck" ) ? CrouchSpeed
				: Input.Down( "Run" ) ? RunSpeed
				: WalkSpeed;

		if ( IsStunned ) speed *= 0.2f;
		if ( IsSlowed ) speed *= (1f - SlowFraction);

		cc.Accelerate( wishDir * speed );
		cc.ApplyFriction( cc.IsOnGround ? 6f : 0.5f );

		if ( cc.IsOnGround && Input.Pressed( "Jump" ) )
			cc.Punch( Vector3.Up * 330f );

		cc.Move();
	}

	// ─── Input: Abilities + Dodge + Lock-on ───────────────────────────
	private void HandleAbilityInput()
	{
		// Dodge — Space (Hogwarts Legacy style, i-frames)
		if ( Input.Pressed( "Dodge" ) )
			DodgeSystem?.TryDodge();

		// Lock-on — Middle Mouse / L
		if ( Input.Pressed( "LockOn" ) )
			LockOnSystem?.ToggleLockOn();

		if ( IsStunned ) return; // stunado não lança feitiços

		if ( Input.Pressed( "Attack1" ) )
			Wand?.TryCastBasic();

		if ( Input.Pressed( "Ability1" ) ) Wand?.TryCastSlot( 0 ); // Q
		if ( Input.Pressed( "Ability2" ) ) Wand?.TryCastSlot( 1 ); // E (Protego)
		if ( Input.Pressed( "Ability3" ) ) Wand?.TryCastSlot( 2 ); // R
		if ( Input.Pressed( "Ability4" ) ) Wand?.TryCastSlot( 3 ); // F

		if ( Input.Pressed( "Item1" ) ) ItemSlot1?.TryUse();
		if ( Input.Pressed( "Item2" ) ) ItemSlot2?.TryUse();
	}

	// ─── Input: Interação (plant/defuse) ──────────────────────────────
	private void HandleInteractInput()
	{
		if ( !Input.Down( "PlantDefuse" ) ) return;

		var site = Scene.GetAllComponents<HorcruxSite>()
			.FirstOrDefault( s => s.WorldPosition.Distance( WorldPosition ) < 150f );

		site?.TryInteract( this );
	}

	// ─── Proxy (outros jogadores) ─────────────────────────────────────
	private void UpdateProxy()
	{
		if ( BodyRenderer.IsValid() )
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;

		WorldRotation = Rotation.FromYaw( EyeAngles.yaw );
	}

	// ─── Estado de combate ────────────────────────────────────────────
	public void SetCombatState( CombatState state, float duration = 0f )
	{
		if ( !Networking.IsHost ) return;
		CombatState = state;
		CombatStateEndTime = duration > 0f ? Time.Now + duration : 0f;
	}

	// ─── Dano ─────────────────────────────────────────────────────────
	public void TakeDamage( int amount, WizardPlayer attacker = null, bool isHeadshot = false )
	{
		if ( !Networking.IsHost || !IsAlive ) return;
		if ( IsInvincible ) return; // i-frames do dodge

		// Bônus de dano por estado (Hogwarts Legacy combo system)
		if ( CombatState == CombatState.Airborne )
			amount = (int)(amount * 1.5f);  // +50% em alvo no ar
		else if ( CombatState == CombatState.Stunned )
			amount = (int)(amount * 1.25f); // +25% em alvo stunado

		// ProtegoComponent absorve dano (verifica perfect block)
		var protego = Components.Get<ProtegoComponent>( FindMode.EverythingInSelf );
		if ( protego != null && protego.IsShieldUp )
		{
			var (passthrough, reflected) = protego.AbsorbDamage( amount, attacker );
			if ( reflected > 0 && attacker != null )
				attacker.TakeDamage( reflected );
			amount = passthrough;
		}

		if ( amount <= 0 ) return;

		int remaining = amount;
		if ( Shield > 0 )
		{
			int abs = Shield < remaining ? Shield : remaining;
			Shield -= abs;
			remaining -= abs;
		}

		int newHealth = Health - remaining;
		Health = newHealth < 0 ? 0 : newHealth;

		if ( Health <= 0 )
			Die( attacker );
	}

	public void Heal( int amount )
	{
		if ( !Networking.IsHost ) return;
		int healed = Health + amount;
		Health = healed > MaxHealth ? MaxHealth : healed;
	}

	public void ApplyShield( int amount )
	{
		if ( !Networking.IsHost ) return;
		int newShield = Shield + amount;
		Shield = newShield > MaxShield ? MaxShield : newShield;
	}

	public void ApplyBurning( float dps, float duration, WizardPlayer source )
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

	// ─── Morte ────────────────────────────────────────────────────────
	public void Die( WizardPlayer killer = null )
	{
		if ( !IsAlive ) return;
		IsAlive = false;
		Deaths++;

		if ( killer != null )
			killer.Kills++;

		BurningEndTime = 0f;
		BurningDPS = 0f;
		SlowEndTime = 0f;
		SlowFraction = 0f;
		CombatState = CombatState.Normal;
		IsInvincible = false;

		var rm = Scene.GetAllComponents<RoundManager>().FirstOrDefault();
		rm?.OnPlayerDied( this, killer );

		BroadcastDeath();
	}

	[Rpc.Broadcast]
	private void BroadcastDeath()
	{
		if ( BodyRenderer.IsValid() )
			BodyRenderer.Enabled = false;
	}

	// ─── Respawn ──────────────────────────────────────────────────────
	public void Respawn( Vector3 position )
	{
		WorldPosition = position;
		Health = MaxHealth;
		Shield = 0;
		IsAlive = true;
		StunEndTime = 0f;
		BurningEndTime = 0f;
		BurningDPS = 0f;
		SlowEndTime = 0f;
		SlowFraction = 0f;
		CombatState = CombatState.Normal;
		IsInvincible = false;

		if ( BodyRenderer.IsValid() )
			BodyRenderer.Enabled = true;

		SpellsDeck?.ResetAllCooldowns();
		ManaSystem?.ResetFull();
		DodgeSystem?.ResetStamina();
	}

	// ─── Economia ─────────────────────────────────────────────────────
	public void GiveGalleons( int amount )
	{
		if ( !Networking.IsHost ) return;
		Galleons += amount;
	}

	public bool SpendGalleons( int amount )
	{
		if ( !Networking.IsHost || Galleons < amount ) return false;
		Galleons -= amount;
		return true;
	}

	// ─── Spell aim ───────────────────────────────────────────────────
	/// <summary>Ponto de origem do feitiço: muzzle da varinha ou EyePosition.</summary>
	public Vector3 GetSpellMuzzle()
	{
		if ( WandHolder is not null )
			return WandHolder.GetMuzzlePosition();
		return EyePosition;
	}

	/// <summary>
	/// Direção do feitiço: muzzle → aim point (centro da tela via trace, ou lock-on).
	/// Passe o muzzle calculado por GetSpellMuzzle() para que a direção aponte corretamente.
	/// </summary>
	public Vector3 GetSpellDirection( Vector3 fromMuzzle )
	{
		// 1. Pegamos a posição e a rotação da câmera
		var camPos = Camera.WorldPosition;
		
		// O SEGREDO: Pegamos o Forward da rotação para virar um Vector3 de direção
		var camForward = Camera.WorldRotation.Forward; 

		var tr = Scene.Trace
			.Ray( camPos, camPos + camForward * 10000f ) // Agora é Vector3 + (Vector3 * float)
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( "projectile" )
			.Run();

		Vector3 aimPoint;

		if ( tr.Hit )
		{
			aimPoint = tr.HitPosition;
		}
		else
		{
			// Se não bater em nada, projeta um ponto no "infinito" à frente da câmera
			aimPoint = camPos + camForward * 5000f; 
		}

		// 3. Retorna a direção do muzzle até esse ponto de impacto
		return (aimPoint - fromMuzzle).Normal;
	}

	// ─── INetworkListener ─────────────────────────────────────────────
	public void OnActive( Connection connection ) { }
	public void OnDisconnected( Connection connection ) { }
}
