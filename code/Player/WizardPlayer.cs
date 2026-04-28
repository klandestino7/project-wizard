
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

	/// <summary>Hora (Time.Now) em que o stun termina. Derivado: IsStunned.</summary>
	[Sync] public float StunEndTime { get; set; } = 0f;
	public bool IsStunned => Time.Now < StunEndTime;

	// ─── Referências (setar no editor ou via GameManager) ─────────────
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public ModelRenderer BodyRenderer { get; set; }

	// ─── Abilities (setadas via inspetor ou GameManager) ──────────────
	[Property] public WandAttack WandAttack { get; set; }
	[Property] public BaseAbility AbilityQ { get; set; }
	[Property] public BaseAbility AbilityE { get; set; }
	[Property] public BaseAbility AbilityR { get; set; }

	// ─── Helpers ──────────────────────────────────────────────────────
	public Vector3 EyePosition => WorldPosition + Vector3.Up * EyeHeight;
	public static WizardPlayer Local => Game.ActiveScene
		.GetAllComponents<WizardPlayer>()
		.FirstOrDefault( p => !p.IsProxy );

	// ─── Lifecycle ────────────────────────────────────────────────────
	protected override void OnStart()
	{
		// Esconde o corpo no cliente local (first-person)
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

		HandleLook();
		HandleAbilityInput();
		HandleInteractInput();

		if ( StunEndTime > 0f && !IsStunned )
			StunEndTime = 0f;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy || !IsAlive ) return;
		HandleMovement();
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

		// Gravidade
		if ( !cc.IsOnGround )
			cc.Velocity += Vector3.Down * 850f * Time.Delta;

		// Direção desejada
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
		{
			WandAttack?.TryFire();
		}

		if ( Input.Pressed( "Ability1" ) ) AbilityQ?.TryActivate();
		if ( Input.Pressed( "Ability2" ) ) AbilityE?.TryActivate();
		if ( Input.Pressed( "Ability3" ) ) AbilityR?.TryActivate();
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

	// ─── Morte ────────────────────────────────────────────────────────
	public void Die( WizardPlayer killer = null )
	{
		if ( !IsAlive ) return;
		IsAlive = false;

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

		if ( BodyRenderer.IsValid() )
			BodyRenderer.Enabled = true;

		AbilityQ?.ResetCooldown();
		AbilityE?.ResetCooldown();
		AbilityR?.ResetCooldown();
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
