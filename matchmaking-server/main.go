package main

import (
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"time"

	"github.com/gorilla/websocket"
)

// generateID produces a random 8-byte hex string.
func generateID() string {
	b := make([]byte, 8)
	if _, err := rand.Read(b); err != nil {
		return fmt.Sprintf("%d", time.Now().UnixNano())
	}
	return hex.EncodeToString(b)
}

var upgrader = websocket.Upgrader{
	CheckOrigin:     func(r *http.Request) bool { return true }, // Allow all origins (game client)
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
}

func main() {
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	svc := NewService()

	mux := http.NewServeMux()

	// Health check — no auth required
	mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}
		w.Header().Set("Content-Type", "application/json")
		json.NewEncoder(w).Encode(svc.Health())
	})

	// WebSocket endpoint — all matchmaking goes through here
	mux.HandleFunc("/ws", func(w http.ResponseWriter, r *http.Request) {
		wsHandler(svc, w, r)
	})

	log.Printf("Matchmaking server listening on :%s", port)
	log.Printf("  GET  http://localhost:%s/health", port)
	log.Printf("  WS   ws://localhost:%s/ws", port)

	if err := http.ListenAndServe(":"+port, mux); err != nil {
		log.Fatal(err)
	}
}

// wsHandler upgrades the connection, then drives the read/write loops for one client.
func wsHandler(svc *Service, w http.ResponseWriter, r *http.Request) {
	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("[WS] Upgrade error: %v", err)
		return
	}

	conn := newConn()
	var steamID string

	// Writer goroutine — drains conn.send into the WebSocket
	go func() {
		defer ws.Close()
		for msg := range conn.send {
			if err := ws.WriteMessage(websocket.TextMessage, msg); err != nil {
				log.Printf("[WS] Write error (%s): %v", steamID, err)
				return
			}
		}
	}()

	// Reader loop (main goroutine for this connection)
	ws.SetReadDeadline(time.Now().Add(120 * time.Second))
	ws.SetPongHandler(func(string) error {
		ws.SetReadDeadline(time.Now().Add(120 * time.Second))
		return nil
	})

	for {
		_, raw, err := ws.ReadMessage()
		if err != nil {
			break
		}
		ws.SetReadDeadline(time.Now().Add(120 * time.Second))

		var msg InMsg
		if err := json.Unmarshal(raw, &msg); err != nil {
			conn.Push(msgError("invalid JSON"))
			continue
		}

		// All actions except "auth" require authentication first
		if msg.Type != "auth" && steamID == "" {
			conn.Push(msgError("send {\"type\":\"auth\",...} first"))
			continue
		}

		switch msg.Type {

		case "auth":
			if msg.SteamID == "" || msg.Name == "" {
				conn.Push(msgError("auth requires steamId and name"))
				continue
			}
			steamID = msg.SteamID
			svc.Auth(steamID, msg.Name, msg.PartyID, conn)
			log.Printf("[WS] Authenticated: %s (%s)", msg.Name, steamID)

		case "queue.join":
			svc.JoinQueue(steamID, msg.PartyID)

		case "queue.leave":
			svc.LeaveQueue(steamID)

		case "match.accept":
			svc.AcceptMatch(steamID)

		case "match.decline":
			svc.DeclineMatch(steamID)

		case "lobby.register":
			if msg.LobbyID == "" {
				conn.Push(msgError("lobby.register requires lobbyId"))
				continue
			}
			svc.RegisterLobby(steamID, msg.LobbyID)

		case "party.create":
			svc.CreateParty(steamID)

		case "party.join":
			if msg.PartyID == "" {
				conn.Push(msgError("party.join requires partyId"))
				continue
			}
			svc.JoinParty(steamID, msg.PartyID)

		case "party.leave":
			svc.LeaveParty(steamID)

		case "ping":
			conn.Push(msgPong())

		default:
			conn.Push(msgError(fmt.Sprintf("unknown message type: %s", msg.Type)))
		}
	}

	// Cleanup on disconnect
	log.Printf("[WS] Disconnected: %s", steamID)
	if steamID != "" {
		svc.Disconnect(steamID)
	}
	close(conn.send)
}
