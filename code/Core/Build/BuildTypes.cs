namespace Warlocks;

public enum AffinityType
{
	Neutral = 0,
	Fire,
	Ice,
	Arcane,
	Shadow
}

public enum DisciplineType
{
	Generalist = 0,
	Duelist,
	Guardian,
	Controller
}

public enum PassiveType
{
	None = 0,
	SwiftCaster,
	ManaSurge,
	Executioner
}

public enum SpellType
{
	Offense = 0,
	Defense,
	Control,
	Utility
}

[Flags]
public enum ComboTag
{
	None = 0,
	Fire = 1 << 0,
	Ice = 1 << 1,
	Arcane = 1 << 2,
	Shadow = 1 << 3,
	Impact = 1 << 4,
	Projectile = 1 << 5,
	Mobility = 1 << 6,
	Shield = 1 << 7,
	Healing = 1 << 8,
	Slow = 1 << 9
}
