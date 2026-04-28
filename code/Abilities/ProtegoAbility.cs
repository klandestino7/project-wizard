
/// <summary>
/// E – Protego: escudo mágico frontal que absorve dano.
/// Tier 0: 100 HP shield, 3s duração, 6s CD
/// Tier 1: 200 HP shield, 4s duração, 5s CD  (500G)
/// Tier 2: 350 HP shield, 5s duração, 4s CD + reflete 30%  (1200G)
/// </summary>
public sealed class ProtegoAbility : BaseAbility
{
	private static readonly int[] ShieldByTier = { 100, 200, 350 };
	private static readonly float[] DurationByTier = { 3f, 4f, 5f };
	protected override float[] CooldownByTier => new[] { 6f, 5f, 4f };

	public int CurrentShieldAmount => ShieldByTier[Math.Clamp( CurrentTier, 0, 2 )];
	public float CurrentDuration => DurationByTier[Math.Clamp( CurrentTier, 0, 2 )];
	public float ReflectFraction => CurrentTier >= 2 ? 0.3f : 0f;

	[Sync] public bool ShieldActive { get; private set; } = false;
	[Sync] public float ShieldEndTime { get; private set; } = 0f;
	[Sync] public int ShieldHP { get; private set; } = 0;

	public bool IsShieldUp => ShieldActive && Time.Now < ShieldEndTime && ShieldHP > 0;

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

		// O shield do Protego é separado do armor de itens
		// Usa o campo Shield do jogador como buffer
		Player.ApplyShield( ShieldHP );
		ShowEffect();
	}

	/// <summary>
	/// Chamado pelo sistema de dano quando Protego está ativo.
	/// Retorna dano que passa pelo escudo (+ dano refletido para o atacante).
	/// </summary>
	public (int passthrough, int reflected) AbsorbDamage( int incoming )
	{
		if ( !IsShieldUp ) return (incoming, 0);

		int absorbed = Math.Min( ShieldHP, incoming );
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
		Player.ApplyShield( -Player.Shield ); // zera shield
	}

	[Broadcast]
	private void ShowEffect()
	{
		// TODO: spawnar VFX de Protego (domo azul translúcido)
	}
}
