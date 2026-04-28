
/// <summary>
/// Atualiza automaticamente o TargetPosition do ParticleLineEmitter
/// para apontar para um GameObject ou posição mundial definida via código.
///
/// Uso no editor:
///   - Adicione no mesmo GO do ParticleLineEmitter
///   - Arraste o GO alvo no campo Target
///
/// Uso via código:
///   var ctrl = go.Components.Get&lt;ParticleLineTarget&gt;();
///   ctrl.SetWorldTarget( someWorldPosition );
/// </summary>
public sealed class ParticleLineTarget : Component
{
	/// <summary>Emitter alvo. Se null, busca no mesmo GameObject.</summary>
	[Property] public ParticleLineEmitter Emitter { get; set; }

	/// <summary>GO para onde a linha aponta. Sobreposto por SetWorldTarget().</summary>
	[Property] public GameObject Target { get; set; }

	private Vector3? _overrideWorldPos;

	protected override void OnStart()
	{
		if ( Emitter == null )
			Emitter = Components.Get<ParticleLineEmitter>();
	}

	protected override void OnUpdate()
	{
		if ( Emitter == null ) return;

		Vector3 worldTarget;

		if ( _overrideWorldPos.HasValue )
		{
			worldTarget = _overrideWorldPos.Value;
		}
		else if ( Target != null )
		{
			worldTarget = Target.WorldPosition;
		}
		else
		{
			return;
		}

		// Converte posição mundial → local do emitter
		Emitter.TargetPosition = Emitter.WorldTransform.PointToLocal( worldTarget );
	}

	/// <summary>Define o alvo por posição mundial (sobrepõe o campo Target).</summary>
	public void SetWorldTarget( Vector3 worldPos )
	{
		_overrideWorldPos = worldPos;
	}

	/// <summary>Remove o override e volta a usar o campo Target.</summary>
	public void ClearOverride()
	{
		_overrideWorldPos = null;
	}
}
