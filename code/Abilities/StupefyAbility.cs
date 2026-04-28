
/// <summary>
/// Q – Stupefy: projétil que causa dano e stun.
/// Tier 0: 80 dmg, 1s stun, 6s CD
/// Tier 1: 120 dmg, 1.5s stun, 5s CD  (400G)
/// Tier 2: 160 dmg, 2s stun, 4s CD + ignora shield  (1000G)
/// </summary>
public sealed class StupefyAbility : BaseAbility
{
	private static readonly int[] DamageByTier = { 80, 120, 160 };
	private static readonly float[] StunByTier = { 1f, 1.5f, 2f };
	protected override float[] CooldownByTier => new[] { 6f, 5f, 4f };

	private int TierIndex => CurrentTier < 0 ? 0 : CurrentTier > 2 ? 2 : CurrentTier;
	public int CurrentDamage => DamageByTier[TierIndex];
	public float CurrentStun => StunByTier[TierIndex];
	public bool PierceShield => CurrentTier >= 2;

	/// <summary>
	/// Contador incrementado pelo owner ao ativar. O host detecta a mudança
	/// e spawna o projétil. Evita [Broadcast] com structs (que causam erros de System.Runtime).
	/// </summary>
	[Sync] private int _activateCount { get; set; } = 0;
	private int _lastActivateCount = 0;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Host: detecta nova ativação e spawna projétil
		if ( Networking.IsHost && _activateCount != _lastActivateCount )
		{
			_lastActivateCount = _activateCount;
			if ( Player.IsValid() )
				SpawnProjectile( Player.EyePosition, Player.EyeAngles.ToRotation().Forward );
		}
	}

	protected override void Activate()
	{
		// Owner incrementa o contador → host detecta via [Sync] e spawna
		_activateCount++;
	}

	private void SpawnProjectile( Vector3 origin, Vector3 direction )
	{
		Log.Info("  SpawnProjectile :: ");
		var go = new GameObject( true, "Stupefy_Projectile" );
		go.WorldPosition = origin;
		go.WorldRotation = Rotation.LookAt( direction );
		go.Tags.Add( "projectile" );

		SpellProjectile.PendingShooter = Player;
		var proj = go.Components.Create<SpellProjectile>();
		proj.ShooterTeam = Player.Team;
		proj.Damage = CurrentDamage;
		proj.StunDuration = CurrentStun;
		proj.Speed = 2000f;
		proj.PierceShield = PierceShield;
		proj.SpellColor = new Color( 0.9f, 0.1f, 0.1f );

		go.NetworkSpawn();
	}
}
