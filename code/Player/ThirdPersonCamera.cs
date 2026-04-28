namespace Warlocks;

/// <summary>
/// Câmera em terceira pessoa com spring arm (Hogwarts Legacy style).
/// Coloque este componente no mesmo GameObject que o WizardPlayer.
/// A CameraComponent deve ser filha do GameObject do player.
/// Offset padrão: 50u à direita, 80u acima, 220u atrás.
/// </summary>
public sealed class ThirdPersonCamera : Component
{
	[Property] public CameraComponent Camera { get; set; }

	[Property] public Vector3 Offset { get; set; } = new Vector3( -50f, 80f, -220f ); // right, up, back (local)
	[Property] public float SpringArmLength { get; set; } = 220f;
	[Property] public float PitchMin { get; set; } = -40f;
	[Property] public float PitchMax { get; set; } = 70f;
	[Property] public float LookSensitivity { get; set; } = 1f;

	private WizardPlayer Player { get; set; }
	private Angles _lookAngles;

	protected override void OnStart()
	{
		Player = Components.Get<WizardPlayer>( FindMode.InAncestors );
	}

	protected override void OnUpdate()
	{
		if ( IsProxy || !Camera.IsValid() || !Player.IsValid() ) return;

		// Se lock-on ativo, câmera já foi ajustada pelo LockOnSystem
		var lockOn = Player.LockOnSystem;

		// Input de câmera
		var delta = Input.AnalogLook * LookSensitivity;
		_lookAngles.yaw += delta.yaw;
		_lookAngles.pitch = (_lookAngles.pitch + delta.pitch).Clamp( PitchMin, PitchMax );
		_lookAngles.roll = 0f;

		// Sincroniza EyeAngles do player com a câmera
		Player.EyeAngles = _lookAngles;

		// Rotação base (yaw only) do corpo
		WorldRotation = Rotation.FromYaw( _lookAngles.yaw );

		// Posição da câmera com spring arm
		var camPos = CalculateCameraPosition();
		Camera.WorldPosition = camPos;
		Camera.WorldRotation = Rotation.From( _lookAngles );
	}

	private Vector3 CalculateCameraPosition()
	{
		var rot = Rotation.From( _lookAngles );
		var pivot = Player.WorldPosition + Vector3.Up * WizardPlayer.EyeHeight * 0.8f;

		// Offset em espaço local da câmera
		var desiredPos = pivot
			+ rot.Right * Offset.x
			+ rot.Up * Offset.y
			+ rot.Forward * Offset.z; // Offset.z negativo = atrás

		// Spring arm: encurta se houver colisão com parede
		var tr = Scene.Trace
			.Ray( pivot, desiredPos )
			.WithoutTags( "player", "projectile" )
			.Radius( 8f )
			.Run();

		return tr.Hit ? tr.EndPosition + tr.Normal * 8f : desiredPos;
	}

	public void SetLookAngles( Angles angles )
	{
		_lookAngles = angles;
	}
}
