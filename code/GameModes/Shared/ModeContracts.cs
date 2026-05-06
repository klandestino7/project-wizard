namespace Warlocks;

public interface IMatchRule
{
	string Id { get; }
	bool IsEnabled { get; }
}

public interface IObjectiveRule : IMatchRule
{
}

public interface IRespawnRule : IMatchRule
{
}

public interface IScoringRule : IMatchRule
{
}

public interface ISpellModifierSource
{
	float ModifyDamage( PlayerPawn player, SpellCatalog.Entry spell, float damage );
	float ModifyCooldown( PlayerPawn player, SpellCatalog.Entry spell, float cooldown );
}
