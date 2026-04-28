namespace Warlocks;

/// <summary>
/// A component that has a team on it.
/// </summary>
public interface ITeam : IValid
{
	public Team Team { get; set; }
}

public enum Team
{
	Unassigned = 0,
	Aurors = 1,         // Defenders - Ministério da Magia
	DarkFollowers = 2   // Attackers - Comensais
}


public enum RoundState
{
	Warmup,
	BuyPhase,
	Combat,
	PostRound,
	MatchEnd
}

public enum WizardRole
{
	Duelist,
	Sentinel,
	Controller,
	Initiator,
	Support
}

public enum RoundEndReason
{
	AttackersEliminated,
	DefendersEliminated,
	HorcruxExploded,
	HorcruxDefused,
	TimeExpired
}

/// <summary>
/// Estado de combate do wizard. Determina bônus de dano e disponibilidade de ações.
/// Influenciado por feitiços de controle (Stupefy, Impedimenta, etc.)
/// </summary>
public enum CombatState
{
	Normal,
	Stunned,    // Stupefy — não age, +25% dano recebido
	Airborne,   // Levitado / lançado — +50% dano recebido por burst
	Burning,    // Incendio DoT ativo
	Shielded    // Protego ativo — reflete projéteis
}
