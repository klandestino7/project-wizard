
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

	/// <summary>Hora (Time.Now) em que o stun termina.</summary>
	[Sync] public float StunEndTime { get; set; } = 0f;
	public bool IsStunned => Time.Now < StunEndTime;

	/// <summary>Burning DoT aplicado por Incendio.</summary>
	[Sync] public float BurningEndTime { get; set; } = 0f;
	[Sync] public float BurningDPS { get; set; } = 0f;
	public WizardPlayer BurningSource { get; set; }

	/// <summary>Slow aplicado por Impedimenta.</summary>
	[Sync] public float SlowEndTime { get; set; } = 0f;
	[Sync] public float SlowFraction { get; set; } = 0f;
	public bool IsSlowed => Time.Now < SlowEndTime && SlowFraction > 0f;

	// ─── Referências (setar no editor ou via GameManager) ─────────────
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public ModelRenderer BodyRenderer { get; set; }
	[Property] public ManaSystem ManaSystem { get; set; }

	// ─── Abilities (setadas via inspetor ou GameManager) ──────────────
	[Property] public WandAttack WandAttack { get; set; }
	[Property] public BaseAbility AbilityQ { get; set; }
	[Property] public BaseAbility AbilityE { get; set; }
	[Property] public BaseAbility AbilityR { get; set; }
	[Property] public BaseAbility AbilityF { get; set; }

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
		if ( !IsProxy && BodyRenderer.IsValid() )
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
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

		if ( StunEndTime > 0f && !IsStunned )
			StunEndTime = 0f;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy || !IsAlive ) return;

		// NÃO PRECISA DESSE HandleMovement, O prefab do player já possui
		// HandleMovement();

		// Burning DoT (servidor)
		if ( Networking.IsHost && BurningEndTime > 0f && Time.Now < BurningEndTime )
		{
			int dot = (int)(BurningDPS * Time.Delta);
			if ( dot > 0 )
				TakeDamage( dot, BurningSource );
		}
		else if ( Networking.IsHost && BurningEndTime > 0f )
		{
			BurningEndTime = 0f;
			BurningDPS = 0f;
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

	// ─── Input: Abilities ─────────────────────────────────────────────
	private void HandleAbilityInput()
	{
		if ( Input.Pressed( "Attack1" ) )
			WandAttack?.TryFire();

		if ( Input.Pressed( "Ability1" ) ) AbilityQ?.TryActivate();
		if ( Input.Pressed( "Ability2" ) ) AbilityE?.TryActivate();
		if ( Input.Pressed( "Ability3" ) ) AbilityR?.TryActivate();
		if ( Input.Pressed( "Ability4" ) ) AbilityF?.TryActivate();

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

	// ─── Dano ─────────────────────────────────────────────────────────
	public void TakeDamage( int amount, WizardPlayer attacker = null, bool isHeadshot = false )
	{
		if ( !Networking.IsHost || !IsAlive ) return;

		// Protego absorve dano
		var protego = Components.Get<ProtegoAbility>( FindMode.EverythingInSelf );
		if ( protego != null && protego.IsShieldUp )
		{
			var (passthrough, reflected) = protego.AbsorbDamage( amount );
			if ( reflected > 0 && attacker != null )
				attacker.TakeDamage( reflected );
			amount = passthrough;
		}

		if ( amount <= 0 ) return;

		int remaining = amount;

		if ( Shield > 0 )
		{
			int shieldAbsorb = Shield < remaining ? Shield : remaining;
			Shield -= shieldAbsorb;
			remaining -= shieldAbsorb;
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
	}

	public void ExtinguishBurning()
	{
		if ( !Networking.IsHost ) return;
		BurningEndTime = 0f;
		BurningDPS = 0f;
		BurningSource = null;
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

		if ( BodyRenderer.IsValid() )
			BodyRenderer.Enabled = true;

		AbilityQ?.ResetCooldown();
		AbilityE?.ResetCooldown();
		AbilityR?.ResetCooldown();
		AbilityF?.ResetCooldown();
		ManaSystem?.ResetFull();
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

	// ─── INetworkListener ─────────────────────────────────────────────
	public void OnActive( Connection connection ) { }
	public void OnDisconnected( Connection connection ) { }
}
