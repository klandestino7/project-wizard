
/// <summary>
/// Repositório central de prefabs de partículas para cada feitiço.
/// Adicione este componente no GameManager (ou qualquer GO persistente na cena).
/// Arraste os prefabs de partícula nos slots pelo inspector.
/// </summary>
public sealed class SpellEffectsLibrary : Component
{
	// ─── Stupefy ─────────────────────────────────────────────────────
	[Property, Group( "Stupefy" )] public GameObject StupefyTrail { get; set; }
	[Property, Group( "Stupefy" )] public GameObject StupefyHit   { get; set; }

	// ─── Incendio ─────────────────────────────────────────────────────
	[Property, Group( "Incendio" )] public GameObject IncendioTrail { get; set; }
	[Property, Group( "Incendio" )] public GameObject IncendioHit   { get; set; }

	// ─── Impedimenta ──────────────────────────────────────────────────
	[Property, Group( "Impedimenta" )] public GameObject ImpedimentaTrail { get; set; }
	[Property, Group( "Impedimenta" )] public GameObject ImpedimentaHit   { get; set; }

	// ─── Wand Attack (hitscan) ────────────────────────────────────────
	[Property, Group( "WandAttack" )] public GameObject WandAttackBeam { get; set; }
	[Property, Group( "WandAttack" )] public GameObject WandAttackHit  { get; set; }

	// ─── Lookup ───────────────────────────────────────────────────────
	public GameObject GetTrailPrefab( string spellClass ) => spellClass switch
	{
		nameof( StupefyAbility )    => StupefyTrail,
		nameof( IncendioAbility )   => IncendioTrail,
		nameof( ImpedimentaAbility) => ImpedimentaTrail,
		_                           => null
	};

	public GameObject GetHitPrefab( string spellClass ) => spellClass switch
	{
		nameof( StupefyAbility )     => StupefyHit,
		nameof( IncendioAbility )    => IncendioHit,
		nameof( ImpedimentaAbility ) => ImpedimentaHit,
		_                            => null
	};

	/// <summary>Retorna a instância na cena (helper estático).</summary>
	public static SpellEffectsLibrary Get( Scene scene ) =>
		scene.GetAllComponents<SpellEffectsLibrary>().FirstOrDefault();
}
