global using Sandbox;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

/// <summary>
/// Site de plant/defuse do Horcrux (equivalente ao bomb site do CS/Valorant).
/// Adicione em um GameObject posicionado nos sites A, B, C do mapa.
/// </summary>
public sealed class HorcruxSite : Component
{
	[Property] public string SiteName { get; set; } = "A";

	// ─── Timings ──────────────────────────────────────────────────────
	[Property] public float PlantTime { get; set; } = 3f;
	[Property] public float DefuseTime { get; set; } = 7f;
	[Property] public float ExplosionDelay { get; set; } = 45f;

	// ─── Estado (sincronizado) ────────────────────────────────────────
	[Sync] public bool IsPlanted { get; private set; } = false;
	[Sync] public bool IsDefused { get; private set; } = false;
	[Sync] public bool HasExploded { get; private set; } = false;
	[Sync] public float ExplosionTime { get; private set; } = 0f;

	// ─── Interação em progresso ───────────────────────────────────────
	[Sync] public float InteractProgress { get; private set; } = 0f;  // 0..1
	[Sync] public bool IsBeingInteracted { get; private set; } = false;

	private WizardPlayer _interactingPlayer;
	private float _interactStartTime;

	public float TimeUntilExplosion => IsPlanted 
		? Math.Max( 0f, (float)(ExplosionTime - Time.Now) ) 
		: 0f;
	// ─── Lifecycle ────────────────────────────────────────────────────
	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;

		// Checar explosão
		if ( IsPlanted && !IsDefused && !HasExploded && Time.Now >= ExplosionTime )
		{
			Explode();
			return;
		}

		// Tick de interação
		if ( IsBeingInteracted && _interactingPlayer.IsValid() )
		{
			// Verificar se o jogador ainda está próximo e segurando F
			if ( _interactingPlayer.WorldPosition.Distance( WorldPosition ) > 150f )
			{
				CancelInteraction();
				return;
			}

			float duration = IsPlanted ? DefuseTime : PlantTime;
			InteractProgress = (Time.Now - _interactStartTime) / duration;

			if ( InteractProgress >= 1f )
			{
				InteractProgress = 1f;
				CompleteInteraction();
			}
		}
	}

	// ─── API pública ──────────────────────────────────────────────────
	public void TryInteract( WizardPlayer player )
	{
		if ( !Networking.IsHost ) return;
		if ( IsDefused || HasExploded ) return;

		// Atacante planta, defensor desarma
		bool canPlant = !IsPlanted && player.Team == Team.DarkFollowers;
		bool canDefuse = IsPlanted && player.Team == Team.Aurors;

		if ( !canPlant && !canDefuse ) return;
		if ( IsBeingInteracted && _interactingPlayer != player ) return;

		if ( !IsBeingInteracted )
			StartInteraction( player );
	}

	public void StopInteract( WizardPlayer player )
	{
		if ( _interactingPlayer == player )
			CancelInteraction();
	}

	public new void Reset()
	{
		IsPlanted = false;
		IsDefused = false;
		HasExploded = false;
		ExplosionTime = 0f;
		InteractProgress = 0f;
		IsBeingInteracted = false;
		_interactingPlayer = null;
	}

	// ─── Interação ────────────────────────────────────────────────────
	private void StartInteraction( WizardPlayer player )
	{
		IsBeingInteracted = true;
		_interactingPlayer = player;
		_interactStartTime = Time.Now;
		InteractProgress = 0f;
	}

	private void CancelInteraction()
	{
		IsBeingInteracted = false;
		_interactingPlayer = null;
		InteractProgress = 0f;
	}

	private void CompleteInteraction()
	{
		IsBeingInteracted = false;

		if ( !IsPlanted )
		{
			Plant( _interactingPlayer );
		}
		else
		{
			Defuse( _interactingPlayer );
		}

		_interactingPlayer = null;
	}

	private void Plant( WizardPlayer planter )
	{
		IsPlanted = true;
		ExplosionTime = Time.Now + ExplosionDelay;

		planter.GiveGalleons( 300 ); // bônus de plant

		var rm = RoundManager.Instance;
		rm?.OnHorcruxPlanted();

		BroadcastPlant( SiteName );
		Log.Info( $"[Horcrux] Plantada no Site {SiteName}!" );
	}

	private void Defuse( WizardPlayer defuser )
	{
		IsDefused = true;

		defuser.GiveGalleons( 300 ); // bônus de desarme

		var rm = RoundManager.Instance;
		rm?.OnHorcruxDefused();

		BroadcastDefuse( SiteName );
		Log.Info( $"[Horcrux] Desarmada no Site {SiteName}!" );
	}

	private void Explode()
	{
		HasExploded = true;

		var rm = RoundManager.Instance;
		rm?.OnHorcruxExploded();

		BroadcastExplosion( WorldPosition );
		Log.Info( $"[Horcrux] Explodiu no Site {SiteName}!" );
	}

	// ─── Broadcasts ───────────────────────────────────────────────────
	[Rpc.Broadcast]
	private void BroadcastPlant( string site )
	{
		// TODO: tocar som de plant + anuncio "Horcrux plantada no Site X!"
	}

	[Rpc.Broadcast]
	private void BroadcastDefuse( string site )
	{
		// TODO: tocar som de defuse + anuncio "Horcrux desarmada!"
	}

	[Rpc.Broadcast]
	private void BroadcastExplosion( Vector3 position )
	{
		// TODO: spawnar VFX de explosão mágica
	}
}
