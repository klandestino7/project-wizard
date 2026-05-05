package main

import (
	"encoding/json"
	"fmt"
	"log"
	"sort"
	"sync"
	"time"
)

const (
	minPlayers         = 2
	maxPlayers         = 10
	acceptTimeout      = 20 * time.Second
	matchCheckInterval = 2 * time.Second
	staleMatchAge      = 10 * time.Minute
)

// Conn is the send-only handle to a connected WebSocket client.
type Conn struct {
	send chan []byte
}

func newConn() *Conn { return &Conn{send: make(chan []byte, 128)} }

func (c *Conn) Push(v any) {
	b, err := json.Marshal(v)
	if err != nil {
		return
	}
	select {
	case c.send <- b:
	default:
		// buffer full — client is too slow, drop the message
	}
}

// ── Service ───────────────────────────────────────────────────────────────────

type Service struct {
	mu sync.Mutex

	active bool

	conns       map[string]*Conn        // steamId → websocket handle
	players     map[string]*Player      // steamId → player info
	parties     map[string]*Party       // partyId → party
	queue       []*QueueEntry           // ordered by JoinedAt
	playerQueue map[string]*QueueEntry  // steamId → queue entry (reverse)
	playerMatch map[string]string       // steamId → matchId
	matches     map[string]*Match       // matchId → match
}

func NewService() *Service {
	s := &Service{
		active:      true,
		conns:       make(map[string]*Conn),
		players:     make(map[string]*Player),
		parties:     make(map[string]*Party),
		queue:       make([]*QueueEntry, 0),
		playerQueue: make(map[string]*QueueEntry),
		playerMatch: make(map[string]string),
		matches:     make(map[string]*Match),
	}
	go s.loop()
	return s
}

// ── Connection lifecycle ──────────────────────────────────────────────────────

// Auth registers (or re-registers) a connection and sends the current state.
func (s *Service) Auth(steamID, name, partyID string, conn *Conn) {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.conns[steamID] = conn

	p, exists := s.players[steamID]
	if !exists {
		p = &Player{}
		s.players[steamID] = p
	}
	p.SteamID = steamID
	p.Name = name
	p.LastSeen = time.Now()
	if partyID != "" {
		p.PartyID = partyID
	}

	conn.Push(msgWelcome(steamID))

	// Restore state for reconnecting client
	if matchID, inMatch := s.playerMatch[steamID]; inMatch {
		m := s.matches[matchID]
		switch m.State {
		case MatchStateProposed:
			conn.Push(msgMatchProposed(m.ID, m.HostSteamID, m.Players, m.HostSteamID == steamID, m.TimeoutAt))
			conn.Push(msgMatchUpdate(len(m.AcceptedBy), len(m.Players)))
		case MatchStateAccepted:
			conn.Push(msgMatchReady(m.ID, m.HostSteamID == steamID))
		case MatchStateStarted:
			if steamID == m.HostSteamID {
				conn.Push(msgMatchReady(m.ID, true))
			} else {
				conn.Push(msgLobbyReady(m.LobbyID))
			}
		}
		return
	}

	if _, inQueue := s.playerQueue[steamID]; inQueue {
		conn.Push(msgQueued())
	}
}

// Disconnect cleans up a player's connection on WebSocket close.
// If they were in a proposed match, it counts as a decline.
func (s *Service) Disconnect(steamID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	delete(s.conns, steamID)

	// If they were in a proposed match, treat as decline
	if matchID, ok := s.playerMatch[steamID]; ok {
		m := s.matches[matchID]
		if m.State == MatchStateProposed {
			log.Printf("[WS] %s disconnected during proposed match — treating as decline", steamID)
			m.DeclinedBy[steamID] = true
			s.cancelMatchLocked(m, "a player disconnected")
		}
	}

	// Remove from queue
	s.removeFromQueueLocked(steamID)
}

// ── Health ────────────────────────────────────────────────────────────────────

type HealthInfo struct {
	Active         bool `json:"active"`
	PlayersInQueue int  `json:"playersInQueue"`
	ActiveMatches  int  `json:"activeMatches"`
	Connections    int  `json:"connections"`
}

func (s *Service) Health() HealthInfo {
	s.mu.Lock()
	defer s.mu.Unlock()

	qPlayers := 0
	for _, e := range s.queue {
		qPlayers += len(e.SteamIDs)
	}
	activeMatches := 0
	for _, m := range s.matches {
		if m.State == MatchStateProposed || m.State == MatchStateAccepted || m.State == MatchStateStarted {
			activeMatches++
		}
	}
	return HealthInfo{
		Active:         s.active,
		PlayersInQueue: qPlayers,
		ActiveMatches:  activeMatches,
		Connections:    len(s.conns),
	}
}

// ── Queue ─────────────────────────────────────────────────────────────────────

func (s *Service) JoinQueue(steamID, partyID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	conn := s.conns[steamID]

	if !s.active {
		if conn != nil {
			conn.Push(msgError("matchmaking is not active"))
		}
		return
	}
	if _, inMatch := s.playerMatch[steamID]; inMatch {
		if conn != nil {
			conn.Push(msgError("already in a match"))
		}
		return
	}
	if _, inQueue := s.playerQueue[steamID]; inQueue {
		if conn != nil {
			conn.Push(msgQueued())
		}
		return
	}

	if p, ok := s.players[steamID]; ok {
		if partyID != "" {
			p.PartyID = partyID
		}
	}

	if partyID != "" {
		party, ok := s.parties[partyID]
		if !ok {
			if conn != nil {
				conn.Push(msgError(fmt.Sprintf("party %s not found", partyID)))
			}
			return
		}
		if !partyHasMember(party, steamID) {
			if conn != nil {
				conn.Push(msgError("player is not in that party"))
			}
			return
		}
		if existing := s.partyQueueEntry(partyID); existing != nil {
			existing.SteamIDs = append(existing.SteamIDs, steamID)
			s.playerQueue[steamID] = existing
		} else {
			e := &QueueEntry{SteamIDs: []string{steamID}, PartyID: partyID, JoinedAt: time.Now()}
			s.queue = append(s.queue, e)
			s.playerQueue[steamID] = e
		}
		log.Printf("[Queue] %s joined (party %s)", steamID, partyID)
	} else {
		e := &QueueEntry{SteamIDs: []string{steamID}, JoinedAt: time.Now()}
		s.queue = append(s.queue, e)
		s.playerQueue[steamID] = e
		log.Printf("[Queue] %s joined (solo)", steamID)
	}

	if conn != nil {
		conn.Push(msgQueued())
	}
}

func (s *Service) LeaveQueue(steamID string) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.removeFromQueueLocked(steamID)
}

func (s *Service) removeFromQueueLocked(steamID string) {
	entry, ok := s.playerQueue[steamID]
	if !ok {
		return
	}
	delete(s.playerQueue, steamID)

	newIDs := make([]string, 0, len(entry.SteamIDs))
	for _, id := range entry.SteamIDs {
		if id != steamID {
			newIDs = append(newIDs, id)
		}
	}
	if len(newIDs) == 0 {
		for i, e := range s.queue {
			if e == entry {
				s.queue = append(s.queue[:i], s.queue[i+1:]...)
				break
			}
		}
	} else {
		entry.SteamIDs = newIDs
	}
}

func (s *Service) partyQueueEntry(partyID string) *QueueEntry {
	for _, e := range s.queue {
		if e.PartyID == partyID {
			return e
		}
	}
	return nil
}

// ── Match formation (background loop) ────────────────────────────────────────

func (s *Service) loop() {
	ticker := time.NewTicker(matchCheckInterval)
	defer ticker.Stop()
	for range ticker.C {
		s.mu.Lock()
		s.tryFormMatch()
		s.checkTimeouts()
		s.cleanupStale()
		s.mu.Unlock()
	}
}

func (s *Service) tryFormMatch() {
	if !s.active || len(s.queue) == 0 {
		return
	}

	total := 0
	for _, e := range s.queue {
		total += len(e.SteamIDs)
	}
	if total < minPlayers {
		return
	}

	sort.Slice(s.queue, func(i, j int) bool {
		return s.queue[i].JoinedAt.Before(s.queue[j].JoinedAt)
	})

	var selected []*QueueEntry
	var players []string

	for _, entry := range s.queue {
		if len(players)+len(entry.SteamIDs) <= maxPlayers {
			selected = append(selected, entry)
			players = append(players, entry.SteamIDs...)
		}
		if len(players) >= maxPlayers {
			break
		}
	}

	if len(players) < minPlayers {
		return
	}

	// Remove selected entries from queue
	for _, sel := range selected {
		for _, sid := range sel.SteamIDs {
			delete(s.playerQueue, sid)
		}
		for i, e := range s.queue {
			if e == sel {
				s.queue = append(s.queue[:i], s.queue[i+1:]...)
				break
			}
		}
	}

	matchID := generateID()
	m := &Match{
		ID:          matchID,
		Players:     players,
		HostSteamID: players[0],
		State:       MatchStateProposed,
		AcceptedBy:  make(map[string]bool),
		DeclinedBy:  make(map[string]bool),
		TimeoutAt:   time.Now().Add(acceptTimeout),
		CreatedAt:   time.Now(),
	}
	s.matches[matchID] = m
	for _, sid := range players {
		s.playerMatch[sid] = matchID
	}

	log.Printf("[Match] %s proposed — %d players, host=%s", matchID, len(players), m.HostSteamID)

	// Notify all players
	for _, sid := range players {
		if conn, ok := s.conns[sid]; ok {
			conn.Push(msgMatchProposed(m.ID, m.HostSteamID, m.Players, m.HostSteamID == sid, m.TimeoutAt))
		}
	}
}

// ── Match actions ─────────────────────────────────────────────────────────────

func (s *Service) AcceptMatch(steamID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	conn := s.conns[steamID]
	matchID, ok := s.playerMatch[steamID]
	if !ok {
		if conn != nil {
			conn.Push(msgError("not in a match"))
		}
		return
	}
	m := s.matches[matchID]
	if m.State != MatchStateProposed {
		if conn != nil {
			conn.Push(msgError("match is no longer in proposed state"))
		}
		return
	}

	m.AcceptedBy[steamID] = true
	log.Printf("[Match] %s — %s accepted (%d/%d)", matchID, steamID, len(m.AcceptedBy), len(m.Players))

	// Broadcast updated accept count to all players in the match
	updateMsg := msgMatchUpdate(len(m.AcceptedBy), len(m.Players))
	for _, sid := range m.Players {
		if c, ok := s.conns[sid]; ok {
			c.Push(updateMsg)
		}
	}

	// All accepted?
	if len(m.AcceptedBy) >= len(m.Players) {
		m.State = MatchStateAccepted
		log.Printf("[Match] %s — all players accepted!", matchID)
		for _, sid := range m.Players {
			if c, ok := s.conns[sid]; ok {
				c.Push(msgMatchReady(m.ID, m.HostSteamID == sid))
			}
		}
	}
}

func (s *Service) DeclineMatch(steamID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	matchID, ok := s.playerMatch[steamID]
	if !ok {
		return
	}
	m := s.matches[matchID]
	if m.State != MatchStateProposed {
		return
	}

	log.Printf("[Match] %s — %s declined", matchID, steamID)
	m.DeclinedBy[steamID] = true
	s.cancelMatchLocked(m, "a player declined")
}

func (s *Service) checkTimeouts() {
	now := time.Now()
	for _, m := range s.matches {
		if m.State == MatchStateProposed && now.After(m.TimeoutAt) {
			log.Printf("[Match] %s — accept timeout", m.ID)
			s.cancelMatchLocked(m, "accept timed out")
		}
	}
}

// cancelMatchLocked cancels a match and re-queues eligible players.
// Must be called with s.mu held.
func (s *Service) cancelMatchLocked(m *Match, reason string) {
	m.State = MatchStateCancelled

	cancelMsg := msgMatchCancelled(reason)

	// Collect re-queue groups
	partyGroups := make(map[string][]string)
	var solos []string

	for _, sid := range m.Players {
		delete(s.playerMatch, sid)

		if m.DeclinedBy[sid] {
			// Decliners get notified but not re-queued
			if conn, ok := s.conns[sid]; ok {
				conn.Push(cancelMsg)
			}
			continue
		}

		if conn, ok := s.conns[sid]; ok {
			conn.Push(cancelMsg)
		}

		p := s.players[sid]
		if p != nil && p.PartyID != "" {
			partyGroups[p.PartyID] = append(partyGroups[p.PartyID], sid)
		} else {
			solos = append(solos, sid)
		}
	}

	// Re-queue solos
	for _, sid := range solos {
		e := &QueueEntry{SteamIDs: []string{sid}, JoinedAt: time.Now()}
		s.queue = append(s.queue, e)
		s.playerQueue[sid] = e
		if conn, ok := s.conns[sid]; ok {
			conn.Push(msgQueued())
		}
	}

	// Re-queue parties (grouped)
	for partyID, members := range partyGroups {
		e := &QueueEntry{SteamIDs: members, PartyID: partyID, JoinedAt: time.Now()}
		s.queue = append(s.queue, e)
		for _, sid := range members {
			s.playerQueue[sid] = e
			if conn, ok := s.conns[sid]; ok {
				conn.Push(msgQueued())
			}
		}
		log.Printf("[Queue] Re-queued party %s (%d members)", partyID, len(members))
	}
}

func (s *Service) cleanupStale() {
	cutoff := time.Now().Add(-staleMatchAge)
	for id, m := range s.matches {
		if (m.State == MatchStateCancelled || m.State == MatchStateStarted) && m.CreatedAt.Before(cutoff) {
			delete(s.matches, id)
		}
	}
}

// ── Lobby ─────────────────────────────────────────────────────────────────────

// RegisterLobby is called by the host once the S&Box private lobby is created.
func (s *Service) RegisterLobby(steamID, lobbyID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	conn := s.conns[steamID]
	matchID, ok := s.playerMatch[steamID]
	if !ok {
		if conn != nil {
			conn.Push(msgError("not in a match"))
		}
		return
	}
	m := s.matches[matchID]
	if m.State != MatchStateAccepted {
		if conn != nil {
			conn.Push(msgError("match is not in accepted state"))
		}
		return
	}
	if m.HostSteamID != steamID {
		if conn != nil {
			conn.Push(msgError("only the host can register the lobby"))
		}
		return
	}

	m.LobbyID = lobbyID
	m.State = MatchStateStarted
	log.Printf("[Lobby] Match %s → lobby %s registered by host %s", matchID, lobbyID, steamID)

	// Push lobby info to all non-host players
	lobbyMsg := msgLobbyReady(lobbyID)
	for _, sid := range m.Players {
		if sid == steamID {
			continue
		}
		if c, ok := s.conns[sid]; ok {
			c.Push(lobbyMsg)
		}
	}
}

// ── Party management ──────────────────────────────────────────────────────────

func (s *Service) CreateParty(steamID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.removeFromPartyLocked(steamID)

	partyID := generateID()
	party := &Party{ID: partyID, LeaderID: steamID, Members: []string{steamID}}
	s.parties[partyID] = party

	if p, ok := s.players[steamID]; ok {
		p.PartyID = partyID
	}

	log.Printf("[Party] %s created by %s", partyID, steamID)
	if conn, ok := s.conns[steamID]; ok {
		conn.Push(msgPartyState(party))
	}
}

func (s *Service) JoinParty(steamID, partyID string) {
	s.mu.Lock()
	defer s.mu.Unlock()

	conn := s.conns[steamID]
	party, ok := s.parties[partyID]
	if !ok {
		if conn != nil {
			conn.Push(msgError("party not found"))
		}
		return
	}

	s.removeFromPartyLocked(steamID)
	party.Members = append(party.Members, steamID)
	if p, ok := s.players[steamID]; ok {
		p.PartyID = partyID
	}

	log.Printf("[Party] %s — %s joined", partyID, steamID)

	// Notify all party members
	for _, sid := range party.Members {
		if c, ok := s.conns[sid]; ok {
			c.Push(msgPartyState(party))
		}
	}
}

func (s *Service) LeaveParty(steamID string) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.removeFromPartyLocked(steamID)
}

func (s *Service) removeFromPartyLocked(steamID string) {
	p, ok := s.players[steamID]
	if !ok || p.PartyID == "" {
		return
	}
	partyID := p.PartyID
	p.PartyID = ""

	party, ok := s.parties[partyID]
	if !ok {
		return
	}

	newMembers := make([]string, 0, len(party.Members))
	for _, m := range party.Members {
		if m != steamID {
			newMembers = append(newMembers, m)
		}
	}
	party.Members = newMembers

	if len(party.Members) == 0 {
		delete(s.parties, partyID)
		log.Printf("[Party] %s dissolved", partyID)
		return
	}
	if party.LeaderID == steamID {
		party.LeaderID = party.Members[0]
	}

	// Notify remaining members
	for _, sid := range party.Members {
		if c, ok := s.conns[sid]; ok {
			c.Push(msgPartyState(party))
		}
	}
}

// ── Helpers ───────────────────────────────────────────────────────────────────

func partyHasMember(party *Party, steamID string) bool {
	for _, m := range party.Members {
		if m == steamID {
			return true
		}
	}
	return false
}
