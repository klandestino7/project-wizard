package main

import "time"

// ── Domain types ──────────────────────────────────────────────────────────────

type MatchState string

const (
	MatchStateProposed  MatchState = "proposed"
	MatchStateAccepted  MatchState = "accepted"
	MatchStateCancelled MatchState = "cancelled"
	MatchStateStarted   MatchState = "started"
)

type Player struct {
	SteamID  string
	Name     string
	PartyID  string
	LastSeen time.Time
}

type Party struct {
	ID       string   `json:"id"`
	LeaderID string   `json:"leaderId"`
	Members  []string `json:"members"`
}

type QueueEntry struct {
	SteamIDs []string
	PartyID  string
	JoinedAt time.Time
}

type Match struct {
	ID          string
	Players     []string
	HostSteamID string
	State       MatchState
	AcceptedBy  map[string]bool
	DeclinedBy  map[string]bool
	TimeoutAt   time.Time
	LobbyID     string
	CreatedAt   time.Time
}

// ── Incoming message (client → server) ───────────────────────────────────────
// All fields are optional depending on the Type.

type InMsg struct {
	Type    string `json:"type"`
	SteamID string `json:"steamId,omitempty"`
	Name    string `json:"name,omitempty"`
	PartyID string `json:"partyId,omitempty"`
	LobbyID string `json:"lobbyId,omitempty"`
}

// ── Outgoing message helpers (server → client) ────────────────────────────────

func msgWelcome(steamID string) any {
	return map[string]any{"type": "welcome", "steamId": steamID}
}

func msgQueued() any {
	return map[string]string{"type": "queued"}
}

func msgMatchProposed(matchID, hostSteamID string, players []string, isHost bool, timeoutAt time.Time) any {
	return map[string]any{
		"type":        "match.proposed",
		"matchId":     matchID,
		"playerCount": len(players),
		"isHost":      isHost,
		"hostSteamId": hostSteamID,
		"timeoutAt":   timeoutAt.UTC().Format(time.RFC3339),
		"acceptedCount": 0,
	}
}

func msgMatchUpdate(accepted, total int) any {
	return map[string]any{
		"type":          "match.update",
		"acceptedCount": accepted,
		"totalPlayers":  total,
	}
}

func msgMatchReady(matchID string, isHost bool) any {
	return map[string]any{
		"type":    "match.ready",
		"matchId": matchID,
		"isHost":  isHost,
	}
}

func msgLobbyReady(lobbyID string) any {
	return map[string]any{
		"type":    "lobby.ready",
		"lobbyId": lobbyID,
	}
}

func msgMatchCancelled(reason string) any {
	return map[string]any{
		"type":   "match.cancelled",
		"reason": reason,
	}
}

func msgPartyState(party *Party) any {
	return map[string]any{"type": "party.state", "party": party}
}

func msgError(message string) any {
	return map[string]any{"type": "error", "message": message}
}

func msgPong() any {
	return map[string]string{"type": "pong"}
}
