package main

import (
	"encoding/json"
	"net/http"
	"strings"
)

type Handler struct {
	svc    *MatchmakingService
	apiKey string // empty = no auth required
}

func NewHandler(svc *MatchmakingService, apiKey string) *Handler {
	return &Handler{svc: svc, apiKey: apiKey}
}

// ── Helpers ───────────────────────────────────────────────────────────────────

func writeJSON(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(v)
}

func writeError(w http.ResponseWriter, status int, msg string) {
	writeJSON(w, status, ErrorResponse{Error: msg})
}

func decode[T any](r *http.Request) (T, error) {
	var v T
	err := json.NewDecoder(r.Body).Decode(&v)
	return v, err
}

// authMiddleware checks the X-API-Key header when an API key is configured.
func (h *Handler) authMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if h.apiKey != "" && r.Header.Get("X-API-Key") != h.apiKey {
			writeError(w, http.StatusUnauthorized, "invalid API key")
			return
		}
		next(w, r)
	}
}

// corsMiddleware adds permissive CORS headers (game client, not browser).
func corsMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, X-API-Key")
		if r.Method == http.MethodOptions {
			w.WriteHeader(http.StatusNoContent)
			return
		}
		next(w, r)
	}
}

func methodOnly(method string, next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != method {
			writeError(w, http.StatusMethodNotAllowed, "method not allowed")
			return
		}
		next(w, r)
	}
}

// wrap applies cors + auth + method check
func (h *Handler) wrap(method string, fn http.HandlerFunc) http.HandlerFunc {
	return corsMiddleware(h.authMiddleware(methodOnly(method, fn)))
}

// ── Health ────────────────────────────────────────────────────────────────────

func (h *Handler) Health(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, h.svc.GetHealth())
}

// ── Queue ─────────────────────────────────────────────────────────────────────

func (h *Handler) QueueJoin(w http.ResponseWriter, r *http.Request) {
	req, err := decode[JoinQueueRequest](r)
	if err != nil || req.SteamID == "" || req.Name == "" {
		writeError(w, http.StatusBadRequest, "steamId and name are required")
		return
	}

	if err := h.svc.JoinQueue(req.SteamID, req.Name, req.PartyID); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}
	writeJSON(w, http.StatusOK, map[string]string{"status": "queued"})
}

func (h *Handler) QueueLeave(w http.ResponseWriter, r *http.Request) {
	req, err := decode[LeaveQueueRequest](r)
	if err != nil || req.SteamID == "" {
		writeError(w, http.StatusBadRequest, "steamId is required")
		return
	}
	h.svc.LeaveQueue(req.SteamID)
	writeJSON(w, http.StatusOK, map[string]string{"status": "left"})
}

// GET /queue/status/{steamId}
func (h *Handler) QueueStatus(w http.ResponseWriter, r *http.Request) {
	steamID := strings.TrimPrefix(r.URL.Path, "/queue/status/")
	if steamID == "" {
		writeError(w, http.StatusBadRequest, "steamId is required")
		return
	}
	writeJSON(w, http.StatusOK, h.svc.GetQueueStatus(steamID))
}

// ── Match ─────────────────────────────────────────────────────────────────────

// matchIDFrom extracts the match ID from a path like /match/{id}/action
func matchIDFrom(path, prefix string) string {
	trimmed := strings.TrimPrefix(path, prefix)
	parts := strings.SplitN(trimmed, "/", 2)
	return parts[0]
}

// POST /match/{id}/accept
func (h *Handler) MatchAccept(w http.ResponseWriter, r *http.Request) {
	matchID := matchIDFrom(r.URL.Path, "/match/")
	req, err := decode[MatchActionRequest](r)
	if err != nil || req.SteamID == "" {
		writeError(w, http.StatusBadRequest, "steamId is required")
		return
	}
	if err := h.svc.AcceptMatch(matchID, req.SteamID); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}
	writeJSON(w, http.StatusOK, h.svc.GetQueueStatus(req.SteamID))
}

// POST /match/{id}/decline
func (h *Handler) MatchDecline(w http.ResponseWriter, r *http.Request) {
	matchID := matchIDFrom(r.URL.Path, "/match/")
	req, err := decode[MatchActionRequest](r)
	if err != nil || req.SteamID == "" {
		writeError(w, http.StatusBadRequest, "steamId is required")
		return
	}
	if err := h.svc.DeclineMatch(matchID, req.SteamID); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}
	writeJSON(w, http.StatusOK, map[string]string{"status": "declined"})
}

// ── Lobby ─────────────────────────────────────────────────────────────────────

// POST /lobby/register  — host registers the created S&Box lobby ID
func (h *Handler) LobbyRegister(w http.ResponseWriter, r *http.Request) {
	req, err := decode[RegisterLobbyRequest](r)
	if err != nil || req.MatchID == "" || req.SteamID == "" || req.LobbyID == "" {
		writeError(w, http.StatusBadRequest, "matchId, steamId and lobbyId are required")
		return
	}
	if err := h.svc.RegisterLobby(req.MatchID, req.SteamID, req.LobbyID); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}
	writeJSON(w, http.StatusOK, map[string]string{"status": "registered"})
}

// GET /lobby/join/{matchId}/{steamId}
func (h *Handler) LobbyJoin(w http.ResponseWriter, r *http.Request) {
	path := strings.TrimPrefix(r.URL.Path, "/lobby/join/")
	parts := strings.SplitN(path, "/", 2)
	if len(parts) != 2 || parts[0] == "" || parts[1] == "" {
		writeError(w, http.StatusBadRequest, "path must be /lobby/join/{matchId}/{steamId}")
		return
	}
	matchID, steamID := parts[0], parts[1]

	lobbyID, err := h.svc.GetLobbyJoinInfo(matchID, steamID)
	if err != nil {
		if strings.Contains(err.Error(), "waiting") {
			// 202 so the client knows to keep polling
			writeError(w, http.StatusAccepted, err.Error())
		} else {
			writeError(w, http.StatusBadRequest, err.Error())
		}
		return
	}
	writeJSON(w, http.StatusOK, LobbyJoinResponse{LobbyID: lobbyID})
}

// ── Party ─────────────────────────────────────────────────────────────────────

func (h *Handler) PartyCreate(w http.ResponseWriter, r *http.Request) {
	req, err := decode[CreatePartyRequest](r)
	if err != nil || req.SteamID == "" || req.Name == "" {
		writeError(w, http.StatusBadRequest, "steamId and name are required")
		return
	}
	party, err := h.svc.CreateParty(req.SteamID, req.Name)
	if err != nil {
		writeError(w, http.StatusInternalServerError, err.Error())
		return
	}
	writeJSON(w, http.StatusOK, party)
}

func (h *Handler) PartyJoin(w http.ResponseWriter, r *http.Request) {
	req, err := decode[JoinPartyRequest](r)
	if err != nil || req.SteamID == "" || req.PartyID == "" || req.Name == "" {
		writeError(w, http.StatusBadRequest, "steamId, partyId and name are required")
		return
	}
	party, err := h.svc.JoinParty(req.SteamID, req.PartyID, req.Name)
	if err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}
	writeJSON(w, http.StatusOK, party)
}

func (h *Handler) PartyLeave(w http.ResponseWriter, r *http.Request) {
	req, err := decode[LeavePartyRequest](r)
	if err != nil || req.SteamID == "" {
		writeError(w, http.StatusBadRequest, "steamId is required")
		return
	}
	h.svc.LeaveParty(req.SteamID)
	writeJSON(w, http.StatusOK, map[string]string{"status": "left"})
}

// GET /party/{partyId}
func (h *Handler) GetParty(w http.ResponseWriter, r *http.Request) {
	partyID := strings.TrimPrefix(r.URL.Path, "/party/")
	if partyID == "" {
		writeError(w, http.StatusBadRequest, "partyId is required")
		return
	}
	party, ok := h.svc.GetParty(partyID)
	if !ok {
		writeError(w, http.StatusNotFound, "party not found")
		return
	}
	writeJSON(w, http.StatusOK, party)
}
