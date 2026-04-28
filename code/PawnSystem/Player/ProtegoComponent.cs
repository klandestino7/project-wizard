namespace Warlocks;

/// <summary>
/// Estado persistente do escudo Protego: sync de rede, VFX e absorção de dano.
/// Separado de ProtegoSpell (que define custo/cooldown como BaseSpell).
/// Consultado por PlayerPawn.TakeDamage() para absorver dano.
///
/// Adicione no mesmo GameObject do PlayerPawn.
/// </summary>
public sealed class ProtegoComponent : Component
{
	public const float PerfectBlockWindow = 0.3f;

	private static readonly int[] ShieldByTier = { 100, 200, 350 };
	private static readonly float[] DurationByTier = { 3f, 4f, 5f };

	// ─── Estado sincronizado ──────────────────────────────────────────
	[Sync] public bool ShieldActive { get; private set; } = false;
	[Sync] public float ShieldEndTime { get; private set; } = 0f;
	[Sync] public float ActivatedAt { get; private set; } = 0f;
	[Sync] public int ShieldHP { get; private set; } = 0;
	[Sync] private int _activatedTier { get; set; } = 0;

	[Sync] private GameObject ShieldFx { get; set; }

	public bool IsShieldUp => ShieldActive && Time.Now < ShieldEndTime && ShieldHP > 0;
	public bool IsPerfectBlockWindow => ShieldActive && Time.Now < ActivatedAt + PerfectBlockWindow;
	private float ReflectFraction => _activatedTier >= 2 ? 0.3f : 0f;

	private PlayerPawn _player;

	protected override void OnStart()
	{
		_player = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
	}

	protected override void OnUpdate()
	{
		if ( Networking.IsHost && ShieldActive && Time.Now >= ShieldEndTime )
			DeactivateShield();
	}

	// ─── Ativação (chamada por ProtegoSpell.Execute) ──────────────────

	/// <summary>Ativa o escudo com o tier do feitiço. Host-only.</summary>
	public void Activate( int tier )
	{
		if ( !Networking.IsHost ) return;

		int t = Math.Clamp( tier, 0, 2 );
		_activatedTier = t;
		ShieldActive = true;
		ShieldHP = ShieldByTier[t];
		ShieldEndTime = Time.Now + DurationByTier[t];
		ActivatedAt = Time.Now;

		_player.ApplyShield( ShieldHP );
		_player.SetCombatState( CombatState.Shielded, DurationByTier[t] );
		ShowEffect();
	}

	// ─── Absorção de dano (chamada por PlayerPawn.TakeDamage) ───────

	/// <summary>
	/// Processa dano recebido com Protego ativo.
	/// Perfect block: stuna o atacante + reflete 100%.
	/// Retorna (dano que passa, dano refletido).
	/// </summary>
	public (int passthrough, int reflected) AbsorbDamage( int incoming, PlayerPawn attacker = null )
	{
		if ( !IsShieldUp ) return (incoming, 0);

		if ( IsPerfectBlockWindow && attacker != null )
		{
			attacker.SetCombatState( CombatState.Stunned, 1.5f );
			attacker.StunEndTime = Time.Now + 1.5f;
			BroadcastPerfectBlock();
			DeactivateShield();
			return (0, incoming);
		}

		int absorbed = ShieldHP < incoming ? ShieldHP : incoming;
		ShieldHP -= absorbed;

		if ( ShieldHP <= 0 )
			DeactivateShield();

		int reflected = (int)(absorbed * ReflectFraction);
		int passthrough = incoming - absorbed;
		return (passthrough, reflected);
	}

	// ─── Desativação ─────────────────────────────────────────────────

	private void DeactivateShield()
	{
		ShieldActive = false;
		_player.ApplyShield( -_player.Shield );

		if ( _player.CombatState == CombatState.Shielded )
			_player.SetCombatState( CombatState.Normal );

		HideEffect();
	}

	// ─── VFX (todos os clientes) ──────────────────────────────────────

	[Rpc.Broadcast]
	private void ShowEffect()
	{
		if ( ShieldFx != null ) return;

		ShieldFx = new GameObject( true, "ProtegoFX" );
		ShieldFx.SetParent( _player.GameObject, true );
		ShieldFx.Transform.LocalPosition = Vector3.Up * 40f;
		ShieldFx.Transform.LocalScale = 1.5f;

		var renderer = ShieldFx.Components.Create<ModelRenderer>();
		renderer.Model = Model.Load( "models/dev/sphere.vmdl" );
		renderer.MaterialOverride = Material.Load( "materials/protego/unlit_translucent.vmat" );
	}

	[Rpc.Broadcast]
	private void HideEffect()
	{
		if ( ShieldFx == null ) return;
		ShieldFx.Destroy();
		ShieldFx = null;
	}

	[Rpc.Broadcast]
	private void BroadcastPerfectBlock()
	{
		if ( ShieldFx == null ) return;
		var renderer = ShieldFx.Components.Get<ModelRenderer>();
		if ( renderer != null )
			renderer.MaterialOverride.Set( "Color", new Color( 1f, 0.85f, 0.2f, 0.6f ) );

		Log.Info( "[Protego] Perfect Block!" );
	}
}
