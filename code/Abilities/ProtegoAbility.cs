
/// <summary>
/// E – Protego: escudo mágico que absorve dano.
/// PERFECT BLOCK: se ativado nos primeiros 0.3s, stuna o atacante e reflete dano total.
/// Tier 0: 100 HP shield, 3s   | Mana 30 | CD 6s
/// Tier 1: 200 HP shield, 4s   (500G)
/// Tier 2: 350 HP shield, 5s + reflect 30%  (1200G)
/// </summary>
public sealed class ProtegoAbility : BaseAbility
{
	public const float PerfectBlockWindow = 0.3f; // janela de perfect block

	private static readonly int[] ShieldByTier = { 100, 200, 350 };
	private static readonly float[] DurationByTier = { 3f, 4f, 5f };
	protected override float[] CooldownByTier => new[] { 6f, 5f, 4f };

	public int CurrentShieldAmount => ShieldByTier[Math.Clamp( CurrentTier, 0, 2 )];
	public float CurrentDuration => DurationByTier[Math.Clamp( CurrentTier, 0, 2 )];
	public float ReflectFraction => CurrentTier >= 2 ? 0.3f : 0f;

	[Sync] public bool ShieldActive { get; private set; } = false;
	[Sync] public float ShieldEndTime { get; private set; } = 0f;
	[Sync] public float ActivatedAt { get; private set; } = 0f;
	[Sync] public int ShieldHP { get; private set; } = 0;

	public bool IsShieldUp => ShieldActive && Time.Now < ShieldEndTime && ShieldHP > 0;

	/// <summary>True nos primeiros 0.3s após ativação — perfect block window.</summary>
	public bool IsPerfectBlockWindow => ShieldActive
		&& Time.Now < ActivatedAt + PerfectBlockWindow;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Networking.IsHost && ShieldActive && Time.Now >= ShieldEndTime )
			DeactivateShield();
	}

	protected override void Activate()
	{
		if ( !Networking.IsHost ) return;

		ShieldActive = true;
		ShieldHP = CurrentShieldAmount;
		ShieldEndTime = Time.Now + CurrentDuration;
		ActivatedAt = Time.Now;

		Player.ApplyShield( ShieldHP );
		Player.SetCombatState( CombatState.Shielded, CurrentDuration );
		ShowEffect();
	}

	/// <summary>
	/// Processa dano recebido com Protego ativo.
	/// Se perfect block: stuna o atacante + reflete 100% do dano.
	/// Retorna (dano que passa, dano refletido).
	/// </summary>
	public (int passthrough, int reflected) AbsorbDamage( int incoming, WizardPlayer attacker = null )
	{
		if ( !IsShieldUp ) return (incoming, 0);

		// Perfect block — reflete tudo e stuna atacante
		if ( IsPerfectBlockWindow && attacker != null )
		{
			attacker.SetCombatState( CombatState.Stunned, 1.5f );
			attacker.StunEndTime = Time.Now + 1.5f;
			BroadcastPerfectBlock();
			DeactivateShield();
			return (0, incoming); // reflete tudo
		}

		// Block normal
		int absorbed = ShieldHP < incoming ? ShieldHP : incoming;
		ShieldHP -= absorbed;

		if ( ShieldHP <= 0 )
			DeactivateShield();

		int reflected = (int)(absorbed * ReflectFraction);
		int passthrough = incoming - absorbed;
		return (passthrough, reflected);
	}

	private void DeactivateShield()
	{
		ShieldActive = false;
		Player.ApplyShield( -Player.Shield );
		if ( Player.CombatState == CombatState.Shielded )
			Player.SetCombatState( CombatState.Normal );
	}

	[Rpc.Broadcast]
	private void ShowEffect()
	{
		// TODO: VFX domo azul translúcido
	}

	[Rpc.Broadcast]
	private void BroadcastPerfectBlock()
	{
		// TODO: VFX de perfect block (flash dourado + câmera shake leve)
		Log.Info( "[Protego] Perfect Block!" );
	}
}
