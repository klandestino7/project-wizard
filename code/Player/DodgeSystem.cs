
/// <summary>
/// Esquiva direcional com i-frames (Hogwarts Legacy style).
/// Stamina: 100 max, regen 20/s. Cada dodge custa 25.
/// Pressione "Dodge" (Space) para ativar na direção do movimento.
/// Adicione no mesmo GameObject que WizardPlayer.
/// </summary>
public sealed class DodgeSystem : Component
{
	public const float MaxStamina = 100f;
	public const float RegenRate = 20f;
	public const float DodgeCost = 25f;
	public const float DodgeDuration = 0.35f;  // duração dos i-frames
	public const float DodgeSpeed = 700f;       // velocidade do dash
	public const float DodgeCooldown = 0.8f;

	[Property, Sync] public float Stamina { get; set; } = MaxStamina;
	public float StaminaPercent => Stamina / MaxStamina;

	private WizardPlayer Player { get; set; }
	private float _dodgeEndTime = 0f;
	private float _nextDodgeTime = 0f;
	private bool _isDodging = false;

	protected override void OnStart()
	{
		Player = Components.Get<WizardPlayer>( FindMode.InAncestors );
	}

	// ─── Regen de stamina ─────────────────────────────────────────────
	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost ) return;

		// Regen stamina
		if ( Stamina < MaxStamina )
			Stamina = Math.Min( Stamina + RegenRate * Time.Delta, MaxStamina );

		// Finalizar dodge
		if ( _isDodging && Time.Now >= _dodgeEndTime )
		{
			_isDodging = false;
			if ( Player.IsValid() )
				Player.IsInvincible = false;
		}

		// Aplicar velocidade do dodge no CharacterController
		if ( _isDodging && Player.IsValid() )
		{
			var cc = Player.CharacterController;
			if ( cc.IsValid() )
			{
				cc.Velocity = _dodgeDir * DodgeSpeed;
				cc.Move();
			}
		}
	}

	private Vector3 _dodgeDir;

	// ─── Tentar dodge ─────────────────────────────────────────────────
	public void TryDodge()
	{
		if ( !Player.IsValid() || !Player.IsAlive ) return;
		if ( Player.IsStunned ) return;
		if ( Time.Now < _nextDodgeTime ) return;
		if ( Stamina < DodgeCost ) return;

		// Direção: WASD ou backward se sem input
		var yaw = Rotation.FromYaw( Player.EyeAngles.yaw );
		var dir = Vector3.Zero;

		if ( Input.Down( "Forward" ) )  dir += yaw.Forward;
		if ( Input.Down( "Backward" ) ) dir -= yaw.Forward;
		if ( Input.Down( "Left" ) )     dir -= yaw.Right;
		if ( Input.Down( "Right" ) )    dir += yaw.Right;

		if ( dir.IsNearlyZero() )
			dir = -yaw.Forward; // dodge para trás se sem input

		_dodgeDir = dir.WithZ( 0f ).Normal;

		// Gasta stamina e ativa i-frames
		if ( Networking.IsHost )
		{
			Stamina -= DodgeCost;
			Player.IsInvincible = true;
		}

		_isDodging = true;
		_dodgeEndTime = Time.Now + DodgeDuration;
		_nextDodgeTime = Time.Now + DodgeCooldown;

		BroadcastDodgeVfx( Player.WorldPosition, _dodgeDir );
	}

	public void ResetStamina()
	{
		if ( !Networking.IsHost ) return;
		Stamina = MaxStamina;
	}

	[Rpc.Broadcast]
	private void BroadcastDodgeVfx( Vector3 pos, Vector3 dir )
	{
		// TODO: trail de dodge + partícula de fumaça mágica
	}
}
