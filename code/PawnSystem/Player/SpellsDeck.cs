namespace Warlocks;

/// <summary>
/// Transitional deck system. It still keeps sync-friendly bitmasks, but the core meaning
/// of "owned" is now "selected into the current build", not "purchased with money".
/// </summary>
public sealed class SpellsDeck : Component
{
	public const int RuntimeSlotCount = 4;

	[Sync] public int OwnedMask { get; set; } = 0;
	[Sync] public int TierPacked { get; set; } = 0;

	[Sync] public int Slot0Idx { get; set; } = -1;
	[Sync] public int Slot1Idx { get; set; } = -1;
	[Sync] public int Slot2Idx { get; set; } = -1;
	[Sync] public int Slot3Idx { get; set; } = -1;

	[Sync] public float SlotCD0 { get; set; } = 0f;
	[Sync] public float SlotCD1 { get; set; } = 0f;
	[Sync] public float SlotCD2 { get; set; } = 0f;
	[Sync] public float SlotCD3 { get; set; } = 0f;

	[Sync] public int ActionId { get; set; } = 0;
	[Sync] public int ActionType { get; set; } = 0; // 0=select 1=assign 2=unassign 3=deselect 4=upgrade
	[Sync] public int ActionArg0 { get; set; } = 0;
	[Sync] public int ActionArg1 { get; set; } = 0;

	private int _lastActionId;

	private readonly BaseSpell[] _owned = new BaseSpell[SpellCatalog.All.Length];
	private readonly BaseSpell[] _slots = new BaseSpell[RuntimeSlotCount]
	{
		new StupefySpell(),
		new ProtegoSpell(),
		new DashSpell(),
		new EpiskeySpell()
	};

	public BasicCastSpell BasicCast { get; } = new();

	private PlayerPawn _player;

	protected override void OnStart()
	{
		_player = Components.Get<PlayerPawn>( FindMode.EverythingInSelf );
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( ActionId != _lastActionId )
		{
			_lastActionId = ActionId;
			ExecuteAction( ActionType, ActionArg0, ActionArg1 );
		}

		SlotCD0 = _slots[0]?.CooldownEndTime ?? 0f;
		SlotCD1 = _slots[1]?.CooldownEndTime ?? 0f;
		SlotCD2 = _slots[2]?.CooldownEndTime ?? 0f;
		SlotCD3 = _slots[3]?.CooldownEndTime ?? 0f;
	}

	public BaseSpell GetSlot( int index )
	{
		if ( index < 0 || index >= RuntimeSlotCount )
			return null;

		return _slots[index];
	}

	public IReadOnlyList<int> GetEquippedSpellIndices()
	{
		var result = new List<int>( RuntimeSlotCount );

		for ( int i = 0; i < RuntimeSlotCount; i++ )
		{
			var idx = GetSlotIdx( i );
			if ( idx >= 0 && !result.Contains( idx ) )
				result.Add( idx );
		}

		return result;
	}

	public IReadOnlyList<int> GetSelectedSpellIndices()
	{
		var result = new List<int>( SpellCatalog.All.Length );

		for ( int i = 0; i < SpellCatalog.All.Length; i++ )
		{
			if ( IsOwned( i ) )
				result.Add( i );
		}

		return result;
	}

	public SpellLoadout GetCurrentLoadout()
	{
		return new SpellLoadout
		{
			SpellIndices = GetSelectedSpellIndices()
		};
	}

	public BuildValidationResult ValidateCurrentLoadout()
	{
		var build = _player?.PlayerBuild?.GetBuildSnapshot() ?? new PlayerBuildSnapshot
		{
			Affinity = AffinityType.Neutral,
			Discipline = DisciplineType.Generalist,
			Passive = PassiveType.None,
			EnergyBudget = PlayerBuildComponent.DefaultEnergyBudget,
			PreparedSpellLimit = PlayerBuildComponent.DefaultPreparedSpellLimit
		};

		return BuildValidationService.Validate( build, GetCurrentLoadout() );
	}

	public void ResetAllCooldowns()
	{
		BasicCast.ResetCooldown();

		for ( int i = 0; i < _owned.Length; i++ )
			_owned[i]?.ResetCooldown();
	}

	public void ClearOwned()
	{
		if ( !Networking.IsHost )
			return;

		for ( int i = 0; i < _owned.Length; i++ )
			_owned[i] = null;

		for ( int i = 0; i < RuntimeSlotCount; i++ )
			_slots[i] = null;

		OwnedMask = 0;
		TierPacked = 0;
		Slot0Idx = Slot1Idx = Slot2Idx = Slot3Idx = -1;
	}

	public void ClientBuy( int catalogIdx ) => Send( 0, catalogIdx, 0 );
	public void ClientSelect( int catalogIdx ) => Send( 0, catalogIdx, 0 );
	public void ClientAssign( int slot, int catalogIdx ) => Send( 1, catalogIdx, slot );
	public void ClientUnassign( int slot ) => Send( 2, -1, slot );
	public void ClientSell( int catalogIdx ) => Send( 3, catalogIdx, 0 );
	public void ClientDeselect( int catalogIdx ) => Send( 3, catalogIdx, 0 );
	public void ClientUpgrade( int catalogIdx, int targetTier ) => Send( 4, catalogIdx, targetTier );

	private void Send( int type, int arg0, int arg1 )
	{
		ActionType = type;
		ActionArg0 = arg0;
		ActionArg1 = arg1;
		ActionId++;
	}

	public bool IsOwned( int catalogIdx ) =>
		catalogIdx >= 0 && (OwnedMask & (1 << catalogIdx)) != 0;

	public int GetTier( int catalogIdx )
	{
		if ( catalogIdx < 0 )
			return 0;

		return (TierPacked >> (catalogIdx * 2)) & 0x3;
	}

	public int GetSlotIdx( int slot ) => slot switch
	{
		0 => Slot0Idx,
		1 => Slot1Idx,
		2 => Slot2Idx,
		3 => Slot3Idx,
		_ => -1
	};

	public float GetSlotCDRemaining( int slot )
	{
		float end = slot switch
		{
			0 => SlotCD0,
			1 => SlotCD1,
			2 => SlotCD2,
			3 => SlotCD3,
			_ => 0f
		};

		return MathF.Max( 0f, end - Time.Now );
	}

	private void ExecuteAction( int type, int arg0, int arg1 )
	{
		switch ( type )
		{
			case 0:
				ServerSelect( arg0 );
				break;
			case 1:
				ServerAssign( arg1, arg0 );
				break;
			case 2:
				ServerUnassign( arg1 );
				break;
			case 3:
				ServerDeselect( arg0 );
				break;
			case 4:
				ServerUpgrade( arg0, arg1 );
				break;
		}
	}

	private PlayerBuildSnapshot ResolveBuild()
	{
		return _player?.PlayerBuild?.GetBuildSnapshot() ?? new PlayerBuildSnapshot
		{
			Affinity = AffinityType.Neutral,
			Discipline = DisciplineType.Generalist,
			Passive = PassiveType.None,
			EnergyBudget = PlayerBuildComponent.DefaultEnergyBudget,
			PreparedSpellLimit = PlayerBuildComponent.DefaultPreparedSpellLimit
		};
	}

	private bool CanSelectSpell( int idx )
	{
		if ( idx < 0 || idx >= SpellCatalog.All.Length )
			return false;

		if ( _owned[idx] != null )
			return true;

		var selected = GetSelectedSpellIndices().ToList();
		selected.Add( idx );

		var validation = BuildValidationService.Validate( ResolveBuild(), new SpellLoadout
		{
			SpellIndices = selected
		} );

		return validation.IsValid;
	}

	private void ServerSelect( int idx )
	{
		if ( idx < 0 || idx >= SpellCatalog.All.Length )
			return;

		if ( _owned[idx] != null )
			return;

		if ( !CanSelectSpell( idx ) )
			return;

		_owned[idx] = SpellCatalog.CreateInstance( SpellCatalog.All[idx].ClassName );
		SyncOwned();
	}

	private void ServerAssign( int slot, int idx )
	{
		if ( slot < 0 || slot >= RuntimeSlotCount )
			return;

		if ( idx < 0 || idx >= SpellCatalog.All.Length )
			return;

		if ( _owned[idx] == null )
			return;

		for ( int i = 0; i < RuntimeSlotCount; i++ )
		{
			if ( i != slot && _slots[i] == _owned[idx] )
				_slots[i] = null;
		}

		_slots[slot] = _owned[idx];
		SyncSlots();
	}

	private void ServerUnassign( int slot )
	{
		if ( slot < 0 || slot >= RuntimeSlotCount )
			return;

		_slots[slot] = null;
		SyncSlots();
	}

	private void ServerDeselect( int idx )
	{
		if ( idx < 0 || idx >= SpellCatalog.All.Length )
			return;

		if ( _owned[idx] == null )
			return;

		var spell = _owned[idx];

		for ( int i = 0; i < RuntimeSlotCount; i++ )
		{
			if ( _slots[i] == spell )
				_slots[i] = null;
		}

		_owned[idx] = null;
		SyncOwned();
		SyncSlots();
	}

	private void ServerUpgrade( int idx, int targetTier )
	{
		if ( idx < 0 || idx >= SpellCatalog.All.Length )
			return;

		if ( _owned[idx] == null )
			return;

		var spell = _owned[idx];

		if ( spell.Tier >= targetTier )
			return;

		spell.Tier = targetTier;
		spell.OnTierChanged();
		SyncOwned();
	}

	private void SyncOwned()
	{
		int mask = 0;
		int tiers = 0;

		for ( int i = 0; i < _owned.Length; i++ )
		{
			if ( _owned[i] == null )
				continue;

			mask |= 1 << i;
			tiers |= (_owned[i].Tier & 0x3) << (i * 2);
		}

		OwnedMask = mask;
		TierPacked = tiers;
	}

	private void SyncSlots()
	{
		Slot0Idx = IdxOf( _slots[0] );
		Slot1Idx = IdxOf( _slots[1] );
		Slot2Idx = IdxOf( _slots[2] );
		Slot3Idx = IdxOf( _slots[3] );
	}

	private int IdxOf( BaseSpell spell )
	{
		if ( spell == null )
			return -1;

		for ( int i = 0; i < _owned.Length; i++ )
		{
			if ( _owned[i] == spell )
				return i;
		}

		return -1;
	}
}
