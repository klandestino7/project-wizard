
public enum Team
{
	None = 0,
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
