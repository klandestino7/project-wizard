namespace Warlocks;

/// <summary>
/// Executor único de feitiços. É o único ponto de criação de projéteis no jogo.
///
/// Pipeline:
///   Input → TryCastBasic / TryCastSlot(0-3)
///   → valida cooldown, mana, estado do player
///   → incrementa contador [Sync]
///   → host detecta mudança em OnUpdate → Spell.Execute(this)
///
/// Adicione no mesmo GameObject do PlayerPawn.
/// </summary>
public sealed class Wand : Component
{
	// ─── Contadores de cast sincronizados ─────────────────────────────
	// O cliente (dono) incrementa. O host detecta a mudança e executa.
	[Sync] private int _castBasic { get; set; } = 0;
	[Sync] private int _castSlot0 { get; set; } = 0;
	[Sync] private int _castSlot1 { get; set; } = 0;
	[Sync] private int _castSlot2 { get; set; } = 0;
	[Sync] private int _castSlot3 { get; set; } = 0;

	private int _lastBasic, _lastSlot0, _lastSlot1, _lastSlot2, _lastSlot3;

	private PlayerPawn _player;
	private SpellsDeck _deck;

	public PlayerPawn Player => _player;

	protected override void OnStart()
	{
		_player = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
		_deck = Components.Get<SpellsDeck>( FindMode.EverythingInSelf );
	}

	// ─── Host: detecta casts e executa ───────────────────────────────
	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;

		if ( _castBasic != _lastBasic ) { _lastBasic = _castBasic; ExecuteSpell( _deck?.BasicCast ); }
		if ( _castSlot0 != _lastSlot0 ) { _lastSlot0 = _castSlot0; ExecuteSpell( _deck?.GetSlot( 0 ) ); }
		if ( _castSlot1 != _lastSlot1 ) { _lastSlot1 = _castSlot1; ExecuteSpell( _deck?.GetSlot( 1 ) ); }
		if ( _castSlot2 != _lastSlot2 ) { _lastSlot2 = _castSlot2; ExecuteSpell( _deck?.GetSlot( 2 ) ); }
		if ( _castSlot3 != _lastSlot3 ) { _lastSlot3 = _castSlot3; ExecuteSpell( _deck?.GetSlot( 3 ) ); }
	}

	private void ExecuteSpell( BaseSpell spell )
	{
		if ( spell == null || !_player.IsValid() || !_player.IsAlive ) return;
		_player.ManaSystem?.TrySpend( spell.ManaCost );
		spell.Execute( this );
	}

	// ─── API pública (chamada por PlayerPawn.HandleAbilityInput) ────

	/// <summary>Ataque básico (clique principal, sem mana).</summary>
	public void TryCastBasic()
	{
		if ( !ValidateCast( _deck?.BasicCast ) ) return;
		_castBasic++;
	}

	/// <summary>Spell de slot 0–3 (Q / E / R / F).</summary>
	public void TryCastSlot( int slot )
	{
		var spell = _deck?.GetSlot( slot );
		if ( !ValidateCast( spell ) ) return;

		switch ( slot )
		{
			case 0: _castSlot0++; break;
			case 1: _castSlot1++; break;
			case 2: _castSlot2++; break;
			case 3: _castSlot3++; break;
		}
	}

	private bool ValidateCast( BaseSpell spell )
	{
		if ( spell == null ) return false;
		if ( !_player.IsValid() || !_player.IsAlive || _player.IsStunned ) return false;
		if ( !spell.IsReady ) return false;

		var mana = _player.ManaSystem;
		if ( mana != null && spell.ManaCost > 0f && !mana.HasMana( spell.ManaCost ) && !mana.FelixActive )
			return false;

		spell.StartCooldown();
		_player.WandHolder?.TriggerCastAnimation();
		return true;
	}

	// ─── Helpers para spells ──────────────────────────────────────────

	/// <summary>
	/// Spawna um SpellProjectile genérico na direção atual da varinha.
	/// Deve ser chamado apenas no host (dentro de Spell.Execute).
	/// </summary>
	public SpellProjectile SpawnProjectile(
		string sourceClass,
		float speed,
		int damage,
		Color color = default,
		float stun = 0f,
		bool pierceShield = false,
		bool launchAirborne = false,
		float burnDPS = 0f,
		float burnDuration = 0f,
		float slowFraction = 0f,
		float slowDuration = 0f )
	{
		var origin = _player.GetSpellMuzzle();
		var dir = _player.GetSpellDirection( origin );

		var go = new GameObject( true, $"{sourceClass}_Projectile" );
		go.WorldPosition = origin;
		go.WorldRotation = Rotation.LookAt( dir );
		go.Tags.Add( "projectile" );

		SpellProjectile.PendingShooter = _player;
		var proj = go.Components.Create<SpellProjectile>();
		proj.ShooterTeam = _player.Team;
		proj.Damage = damage;
		proj.Speed = speed;
		proj.SpellColor = color;
		proj.StunDuration = stun;
		proj.PierceShield = pierceShield;
		proj.LaunchAirborne = launchAirborne;
		proj.BurningDPS = burnDPS;
		proj.BurningDuration = burnDuration;
		proj.SlowFraction = slowFraction;
		proj.SlowDuration = slowDuration;
		proj.SourceSpellClass = sourceClass;

		go.NetworkSpawn();
		return proj;
	}

	/// <summary>
	/// Dispara um raio hitscan a partir do muzzle na direção da câmera.
	/// </summary>
	public SceneTraceResult FireHitscan( float range = 5000f )
	{
		var origin = _player.GetSpellMuzzle();
		var dir = _player.GetSpellDirection( origin );

		return Scene.Trace
			.Ray( origin, origin + dir * range )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( _player.GameObject )
			.WithoutTags( "projectile" )
			.Run();
	}

	/// <summary>Mostra efeito visual do ataque básico (beam + impacto). Roda em todos os clientes.</summary>
	[Rpc.Broadcast]
	public void ShowBeamEffect( Vector3 start, Vector3 end )
	{
		var lib = SpellEffectsLibrary.Get( Scene );
		var dir = (end - start).Normal;

		if ( lib?.WandAttackBeam is not null )
		{
			var beam = lib.WandAttackBeam.Clone();
			beam.WorldPosition = start;
			beam.WorldRotation = Rotation.LookAt( dir );
			if ( !beam.Components.TryGet<AutoDestroy>( out _ ) )
				beam.Components.Create<AutoDestroy>().Delay = 1f;
		}

		if ( lib?.WandAttackHit is not null )
		{
			var fx = lib.WandAttackHit.Clone();
			fx.WorldPosition = end;
			fx.WorldRotation = Rotation.LookAt( -dir );
		}
	}

	/// <summary>Teleporta o jogador local (usado por DashSpell).</summary>
	[Rpc.Broadcast]
	public void BroadcastTeleport( Vector3 destination )
	{
		if ( !IsProxy )
			_player.WorldPosition = destination;
	}
}
