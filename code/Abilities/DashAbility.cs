
/// <summary>
/// Slot R — Apparition: dash rápido na direção do olhar.
/// T0: 400u  CD 20s | T1: 500u  CD 16s | T2: 600u  CD 12s, atravessa jogadores
/// </summary>
public sealed class DashAbility : BaseAbility
{
	[Property] public string AbilityNameOverride { get; set; } = "Apparition";

	protected override float[] CooldownByTier => new[] { 20f, 16f, 12f };

	private static readonly float[] DashDistance = { 400f, 500f, 600f };

	protected override void OnStart()
	{
		base.OnStart();
		AbilityName = AbilityNameOverride;
	}

	protected override void Activate()
	{
		if ( !Player.IsValid() ) return;

		float dist = DashDistance[CurrentTier < 0 ? 0 : CurrentTier > 2 ? 2 : CurrentTier];

		var dir = Rotation.FromYaw( Player.EyeAngles.yaw ).Forward;
		var target = Player.WorldPosition + dir * dist;

		// Verifica se o destino está livre (trace simples)
		var tr = Scene.Trace
			.Ray( Player.EyePosition, Player.EyePosition + dir * dist )
			.WithoutTags( "player" )
			.Run();

		var finalPos = tr.Hit ? tr.HitPosition - dir * 24f : target;
		finalPos = finalPos.WithZ( Player.WorldPosition.z );

		BroadcastDash( finalPos );
	}

	[Rpc.Broadcast]
	private void BroadcastDash( Vector3 destination )
	{
		if ( !IsProxy )
			Player.WorldPosition = destination;
	}
}
