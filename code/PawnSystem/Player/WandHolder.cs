namespace Warlocks;

/// <summary>
/// Mantém a varinha na mão direita do player e gerencia animações de holdtype.
/// Adicione no mesmo GameObject que PlayerPawn e AnimationHelper.
///
/// Setup:
///   - PlayerRenderer → SkinnedModelRenderer do corpo
///   - AnimHelper     → AnimationHelper do player
///   - WandPrefab     → prefab com ModelRenderer + filhos "muzzle" e "base_hand"
///
/// A varinha é clonada em OnStart e parenteada ao osso "hand_R" do Citizen.
/// AttachOffset / AttachAngles permitem ajuste fino pelo inspector.
/// </summary>
public sealed class WandHolder : Component
{
	// ─── Referências (setar no editor) ────────────────────────────────
	[Property] public SkinnedModelRenderer PlayerRenderer { get; set; }
	[Property] public AnimationHelper AnimHelper { get; set; }

	/// <summary>Prefab da varinha (deve ter filhos "muzzle" e "base_hand").</summary>
	[Property] public GameObject WandPrefab { get; set; }

	// ─── Ajuste de attach ────────────────────────────────────────────
	[Property, Group( "Attachment" )] public Vector3 AttachOffset { get; set; } = Vector3.Zero;
	[Property, Group( "Attachment" )] public Angles AttachAngles { get; set; } = Angles.Zero;

	// ─── Estado sincronizado ─────────────────────────────────────────
	/// <summary>True enquanto o jogador segura Attack2 (modo mira).</summary>
	[Sync] public bool IsAiming { get; private set; } = false;

	/// <summary>Incrementado a cada cast para notificar todos os clientes.</summary>
	[Sync] private int _castCount { get; set; } = 0;

	// ─── Internos ─────────────────────────────────────────────────────
	private PlayerPawn _player;
	private GameObject _wand;
	private GameObject _muzzle;
	private int _lastCastCount;

	// ─── Lifecycle ────────────────────────────────────────────────────
	protected override void OnStart()
	{
		_player = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
		AttachWand();
	}

	protected override void OnUpdate()
	{
		// Apenas o dono lê input
		if ( !IsProxy && _player.IsValid() )
		{
			IsAiming = Input.Down( "Attack2" ) && _player.IsAlive && !_player.IsStunned;
		}

		// Trigger de cast detectado em todos os clientes
		if ( _castCount != _lastCastCount )
		{
			_lastCastCount = _castCount;
			AnimHelper?.Target?.Set( "b_attack", true );
		}

		UpdateAnimation();
	}

	// ─── API pública ──────────────────────────────────────────────────

	/// <summary>
	/// Chame isto quando qualquer feitiço for lançado (WandAttack, BaseAbility).
	/// Dispara a animação Pistol_RH_Attack_01 em todos os clientes.
	/// </summary>
	public void TriggerCastAnimation()
	{
		_castCount++;
	}

	/// <summary>Posição do cano da varinha (origin dos projéteis).</summary>
	public Vector3 GetMuzzlePosition()
	{
		if ( _muzzle is not null && _muzzle.IsValid() )
			return _muzzle.WorldPosition;

		return WorldPosition;
	}

	// ─── Attach ───────────────────────────────────────────────────────
	private void AttachWand()
	{
		if ( PlayerRenderer == null || WandPrefab == null ) return;

		// // Localiza o índice do osso da mão direita no Citizen
		// int boneIdx = PlayerRenderer.SceneModel?.GetBoneIndex( "hand_R" ) ?? -1;
		// if ( boneIdx < 0 )
		// {
		// 	Log.Warning( "WandHolder: osso 'hand_R' não encontrado no modelo." );
		// 	return;
		// }

		var handBone = PlayerRenderer.GetBoneObject( "hand_R" );
		if ( !handBone.IsValid() )
		{
			Log.Warning( "WandHolder: GetBoneObject retornou inválido." );
			return;
		}

		_wand = WandPrefab.Clone();
		_wand.Parent = handBone;
		_wand.LocalPosition = AttachOffset;
		_wand.LocalRotation = AttachAngles.ToRotation();

		// Encontra filhos por nome
		_muzzle = FindChild( _wand, "muzzle" );

		// "base_hand" serve de guia visual no editor; não precisa de referência em runtime
	}

	private static GameObject FindChild( GameObject root, string name )
	{
		foreach ( var child in root.Children )
		{
			if ( child.Name == name ) return child;
			var found = FindChild( child, name );
			if ( found != null ) return found;
		}
		return null;
	}

	// ─── Animação ─────────────────────────────────────────────────────
	private void UpdateAnimation()
	{
		if ( AnimHelper == null || !_player.IsValid() ) return;

		// HoldType: HoldItem em idle, Pistol quando mirando
		AnimHelper.HoldType = IsAiming ? AnimationHelper.HoldTypes.Pistol
										 : AnimationHelper.HoldTypes.HoldItem;
		AnimHelper.Handedness = AnimationHelper.Hand.Right;

		// Direção de mira do corpo
		var aimRot = _player.EyeAngles.ToRotation();
		AnimHelper.AimAngle = aimRot;

		// Peso de look no corpo: total ao mirar, suave em idle
		float bodyWeight = IsAiming ? 1f : 0.3f;
		AnimHelper.WithLook( aimRot.Forward, 1f, 1f, bodyWeight );
	}
}
