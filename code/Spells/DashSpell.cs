
/// <summary>
/// Apparition (R): dash rápido na direção do olhar.
/// T0: 400u | CD 20s  | T1: 500u | CD 16s  | T2: 600u | CD 12s
/// </summary>
public sealed class DashSpell : BaseSpell
{
	private static readonly float[] DashDistance = { 400f, 500f, 600f };

	public override string  SpellName      => "Apparition";
	public override float   ManaCost       => 0f;
	public override float   BaseCooldown   => 20f;
	public override float[] CooldownByTier => new[] { 20f, 16f, 12f };
	public override int     Tier1Cost      => 400;
	public override int     Tier2Cost      => 1000;

	public override void Execute( Wand wand )
	{
		int   t    = Math.Clamp( Tier, 0, 2 );
		float dist = DashDistance[t];

		var dir    = Rotation.FromYaw( wand.Player.EyeAngles.yaw ).Forward;
		var target = wand.Player.WorldPosition + dir * dist;

		var tr = wand.Scene.Trace
			.Ray( wand.Player.EyePosition, wand.Player.EyePosition + dir * dist )
			.WithoutTags( "player" )
			.Run();

		var finalPos = tr.Hit ? tr.HitPosition - dir * 24f : target;
		finalPos = finalPos.WithZ( wand.Player.WorldPosition.z );

		wand.BroadcastTeleport( finalPos );
	}
}
