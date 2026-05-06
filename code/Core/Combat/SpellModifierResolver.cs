namespace Warlocks;

public static class SpellModifierResolver
{
	public static ResolvedSpellData Resolve( PlayerPawn player, BaseSpell spell )
	{
		var entry = SpellCatalog.Find( spell?.GetType().Name );
		var build = player?.PlayerBuild?.GetBuildSnapshot() ?? new PlayerBuildSnapshot
		{
			Affinity = AffinityType.Neutral,
			Discipline = DisciplineType.Generalist,
			Passive = PassiveType.None,
			EnergyBudget = PlayerBuildComponent.DefaultEnergyBudget,
			PreparedSpellLimit = PlayerBuildComponent.DefaultPreparedSpellLimit
		};

		var loadoutValidation = player?.SpellsDeck?.ValidateCurrentLoadout();

		float manaMultiplier = 1f;
		float cooldownMultiplier = 1f;
		float damageMultiplier = 1f;
		float durationMultiplier = 1f;

		if ( entry is not null && entry.Affinity == build.Affinity )
		{
			switch ( build.Affinity )
			{
				case AffinityType.Fire:
					damageMultiplier *= 1.15f;
					break;
				case AffinityType.Ice:
					durationMultiplier *= 1.20f;
					break;
				case AffinityType.Arcane:
					cooldownMultiplier *= 0.85f;
					break;
				case AffinityType.Shadow:
					manaMultiplier *= 0.9f;
					break;
			}
		}

		if ( entry is not null )
		{
			switch ( build.Discipline )
			{
				case DisciplineType.Duelist:
					if ( entry.SpellType == SpellType.Offense )
						damageMultiplier *= 1.15f;
					if ( entry.SpellType == SpellType.Defense )
						durationMultiplier *= 0.9f;
					break;
				case DisciplineType.Guardian:
					if ( entry.SpellType == SpellType.Defense )
						durationMultiplier *= 1.15f;
					if ( entry.SpellType == SpellType.Utility )
						cooldownMultiplier *= 1.1f;
					break;
				case DisciplineType.Controller:
					if ( entry.SpellType == SpellType.Control )
						durationMultiplier *= 1.15f;
					if ( entry.SpellType == SpellType.Offense )
						damageMultiplier *= 0.9f;
					break;
			}
		}

		if ( loadoutValidation is not null )
		{
			if ( loadoutValidation.OffenseCount >= 4 && entry?.SpellType == SpellType.Offense )
				cooldownMultiplier *= 1.15f;

			if ( loadoutValidation.ControlCount >= 4 && entry?.SpellType == SpellType.Control )
				damageMultiplier *= 0.9f;
		}

		switch ( build.Passive )
		{
			case PassiveType.SwiftCaster:
				cooldownMultiplier *= 0.95f;
				break;
			case PassiveType.ManaSurge:
				manaMultiplier *= 0.9f;
				break;
			case PassiveType.Executioner:
				damageMultiplier *= 1.05f;
				break;
		}

		var baseManaCost = spell?.ManaCost ?? 0f;
		var baseCooldown = spell?.CooldownDuration ?? 0f;

		return new ResolvedSpellData
		{
			Spell = spell,
			Entry = entry,
			ManaCost = MathF.Max( 0f, baseManaCost * manaMultiplier ),
			Cooldown = MathF.Max( 0f, baseCooldown * cooldownMultiplier ),
			DamageMultiplier = damageMultiplier,
			DurationMultiplier = durationMultiplier
		};
	}
}

public sealed class ResolvedSpellData
{
	public BaseSpell Spell { get; init; }
	public SpellCatalog.Entry Entry { get; init; }
	public float ManaCost { get; init; }
	public float Cooldown { get; init; }
	public float DamageMultiplier { get; init; } = 1f;
	public float DurationMultiplier { get; init; } = 1f;
}
