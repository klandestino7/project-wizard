
/// <summary>
/// Trail de linha padrão para todos os projéteis de feitiço.
/// Renderiza uma linha de partículas do projétil de volta à posição de origem (muzzle).
///
/// Agrupa automaticamente: ParticleEffect + ParticleLineEmitter + ParticleLineRenderer + ParticleLineTarget.
/// Chamado pelo SpellProjectile.OnStart via Setup().
///
/// Para customizar por spell: sete UseDefaultParticle = false no SpellProjectile
/// e forneça um prefab via SpellEffectsLibrary.
/// </summary>
public sealed class SpellLineTrail : Component
{
	// ─── Aparência (ajustável por spell via código ou inspector) ──────
	/// <summary>Cor da linha. Normalmente vem do SpellColor do projétil.</summary>
	[Property] public Color Color { get; set; } = Color.White;

	/// <summary>Espessura visual da linha (Scale do renderer).</summary>
	[Property, Range( 0.1f, 10f )] public float Thickness { get; set; } = 2f;

	/// <summary>Partículas emitidas por segundo.</summary>
	[Property] public float Rate { get; set; } = 120f;

	/// <summary>Blend aditivo (recomendado para feitiços).</summary>
	[Property] public bool Additive { get; set; } = true;

	// ─── Internos ─────────────────────────────────────────────────────
	private ParticleLineTarget _lineTarget;
	private Vector3 _origin;

	// ─── Setup ────────────────────────────────────────────────────────

	/// <summary>
	/// Inicializa o sistema de partículas com a posição de origem e cor do feitiço.
	/// Chamado pelo SpellProjectile logo após ser adicionado.
	/// </summary>
	public void Setup( Vector3 worldOrigin, Color color, float thickness = 2f )
	{
		_origin   = worldOrigin;
		Color     = color;
		Thickness = thickness;

		BuildParticleSystem();
	}

	private void BuildParticleSystem()
	{
		// ── ParticleEffect (gerenciador do pool de partículas) ────────
		// if ( !Components.TryGet<ParticleEffect>( out var effect ) )
			// effect = Components.Create<ParticleEffect>();

		// effect.Loop = true;

		// ── ParticleLineEmitter ───────────────────────────────────────
		if ( !Components.TryGet<ParticleLineEmitter>( out var emitter ) )
			emitter = Components.Create<ParticleLineEmitter>();

		emitter.Rate = Rate;

		// ── ParticleLineRenderer ──────────────────────────────────────
		if ( !Components.TryGet<ParticleLineRenderer>( out var renderer ) )
			renderer = Components.Create<ParticleLineRenderer>();

		renderer.Scale    = Thickness;
		renderer.Additive = Additive;

		// ── ParticleLineTarget (controla TargetPosition) ─────────────
		if ( !Components.TryGet<ParticleLineTarget>( out _lineTarget ) )
			_lineTarget = Components.Create<ParticleLineTarget>();

		_lineTarget.Emitter = emitter;
		_lineTarget.SetWorldTarget( _origin );
	}

	// ─── Runtime ──────────────────────────────────────────────────────

	protected override void OnUpdate()
	{
		// Mantém a linha sempre apontando de volta para a origem (muzzle)
		_lineTarget?.SetWorldTarget( _origin );
	}
}
