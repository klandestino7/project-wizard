# Warlocks Matchmaking Server

Go WebSocket server that manages matchmaking queues, parties, and lobbies.

## Setup

```bash
cd matchmaking-server
go mod tidy        # baixa gorilla/websocket
go run .           # roda na porta 8080
```

Variável de ambiente opcional:
```bash
PORT=9000 go run .
```

## Endpoints

| Endpoint | Descrição |
|---|---|
| `GET /health` | Status do servidor (HTTP) |
| `WS /ws` | WebSocket — toda a comunicação de matchmaking |

## Protocolo WebSocket

### Cliente → Servidor

```jsonc
// 1. Autenticar (obrigatório antes de qualquer outra mensagem)
{ "type": "auth", "steamId": "1234", "name": "Gandalf", "partyId": "abc" }

// 2. Entrar na fila
{ "type": "queue.join", "partyId": "abc" }   // partyId opcional

// 3. Sair da fila
{ "type": "queue.leave" }

// 4. Aceitar partida
{ "type": "match.accept" }

// 5. Recusar partida
{ "type": "match.decline" }

// 6. (Host) Registrar lobby após criar no S&Box
{ "type": "lobby.register", "lobbyId": "987654321" }

// Party
{ "type": "party.create" }
{ "type": "party.join",  "partyId": "abc" }
{ "type": "party.leave" }

// Heartbeat
{ "type": "ping" }
```

### Servidor → Cliente

```jsonc
{ "type": "welcome",   "steamId": "..." }
{ "type": "queued" }

// Partida encontrada — mostra popup de aceitar
{ "type": "match.proposed", "matchId": "...", "playerCount": 4,
  "isHost": true, "timeoutAt": "2025-01-01T00:00:20Z", "acceptedCount": 0 }

// Atualização de aceites
{ "type": "match.update", "acceptedCount": 3, "totalPlayers": 4 }

// Todos aceitaram
// isHost=true  → criar lobby no S&Box e enviar lobby.register
// isHost=false → aguardar lobby.ready
{ "type": "match.ready", "matchId": "...", "isHost": false }

// (Para não-host) Lobby criado — conectar
{ "type": "lobby.ready", "lobbyId": "987654321" }

// Partida cancelada (recusa / timeout / desconexão) — servidor re-enfileira automaticamente
{ "type": "match.cancelled", "reason": "a player declined" }

{ "type": "party.state", "party": { "id": "...", "leaderId": "...", "members": [...] } }
{ "type": "error",  "message": "..." }
{ "type": "pong" }
```

## Fluxo completo

```
Client A & B conectam WS
  → auth
  → queue.join
  [servidor forma partida]
  ← match.proposed (isHost: A=true, B=false)
  → match.accept (ambos)
  ← match.update (2/2)
  ← match.ready
  
  [A (host)]                      [B]
  Networking.CreateLobby()        aguarda lobby.ready
  → lobby.register {lobbyId}      ← lobby.ready {lobbyId}
                                  Networking.TryConnectSteamId(lobbyId)
```

## Build para produção

```bash
go build -o matchmaking-server .
./matchmaking-server
```
