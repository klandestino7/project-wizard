# WIZARDING WARFARE - Game Design Document
## Shooter Tático 6v6 Round-Based | 100% Temática Harry Potter + Elementos MOBA

---

## 🎯 CONCEITO CENTRAL

**Wizarding Warfare** é um shooter tático 6v6 onde bruxos das trevas enfrentam aurores do Ministério da Magia em combates round-based intensos. Inspirado na **Guerra Mágica** do universo Harry Potter, o jogo combina a estrutura de rounds do **Valorant** com elementos de progressão do **Deadlock**, mas SEM ser um MOBA - a progressão serve apenas para criar profundidade tática dentro de cada partida.

**Fórmula:** Estrutura de Valorant + Progressão de Deadlock + 100% Harry Potter

---

## 🎮 ESTRUTURA DE PARTIDA

### Formato Round-Based

**6v6 - Melhor de 24 Rounds**
- Primeiro time a 13 rounds vence
- Swap de lados no round 12
- Overtime se empatar 12-12
- Duração total: 30-40 minutos

**Cada Round:**
- **Fase de Preparação:** 30 segundos
  - Compra de itens/upgrades
  - Escolha de loadout de feitiços
  - Estratégia de time
- **Fase de Combate:** 90 segundos
  - Objetivo principal ativo
  - Eliminação ou objetivo completado = round ganho
- **Pós-Round:** 7 segundos
  - Replay da jogada decisiva
  - Mostra economia do próximo round

### Sistema de Sides

**AURORES (Defensores)** - Ministério da Magia
- Defendem objetivos mágicos
- Robes azuis/dourados
- Acesso a feitiços defensivos extras

**COMENSAIS (Atacantes)** - Seguidores das Trevas
- Atacam/plantam objetivos
- Robes pretos/verdes
- Acesso a feitiços ofensivos extras

---

## 🎯 MODOS DE JOGO

### 1. **HORCRUX ASSAULT** (Modo Principal)

**Objetivo Atacantes (Comensais):**
- Plantar "Fragmento de Horcrux" em 1 de 3 sites (A, B ou C)
- Proteger por 45 segundos até ativação
- Se explodir = Round ganho

**Objetivo Defensores (Aurores):**
- Impedir o plant
- OU desarmar com "Finite Incantatem" (7 segundos de channel)
- Eliminar todos atacantes

**Twist Mágico:**
- Horcrux emite AoE de dano ao defensor que desarma
- Atacantes podem "re-plant" se pegarem o Horcrux desarmado

### 2. **FORBIDDEN ARTIFACT** (Tipo Control Point)

**Objetivo:**
- Artefato mágico spawn no centro do mapa (round 1, 4, 7, etc)
- Time que segurar por 60 segundos vence o round
- Artefato dá buffs para quem carrega
- Pode ser dropado ao morrer

### 3. **DEATHMATCH MÁGICO** (Warmup/Casual)

- Primeiro time a 40 kills
- Respawn instantâneo
- Dinheiro infinito
- Testar feitiços e combos

---

## 💰 ECONOMIA E PROGRESSÃO (NÃO É MOBA, É ROUND-BASED)

### Sistema de Dinheiro: "Galeões"

**Diferente de MOBA:** Dinheiro NÃO acumula para comprar itens permanentes. Serve para:
1. Comprar **upgrades de feitiços** para aquele round
2. Comprar **itens consumíveis** (poções, shields)
3. Desbloquear **habilidades extras** temporárias

**Ganho de Galeões:**

| Ação | Galeões |
|------|---------|
| Início de Round | 800 (base) |
| Kill | 200 |
| Assistência | 100 |
| Plant da Horcrux | 300 |
| Desarme bem-sucedido | 300 |
| Vitória do Round | 3000 (time todo) |
| Derrota do Round | 1900 (consolação) |
| Perder 2+ rounds seguidos | +500 bônus (loss streak) |

**Carrega entre rounds:** Sim! Como Valorant, você acumula dinheiro para rounds futuros.

### O Que Comprar com Galeões

#### **CATEGORIA 1: SPELL UPGRADES (Temporários por Round)**

Cada feitiço pode ser upado durante a fase de compra:

**Exemplo - Stupefy (Feitiço Básico):**
- **Tier 0 (Grátis):** 80 damage, 1s stun, 6s cooldown
- **Tier 1 (400G):** 120 damage, 1.5s stun, 5s cooldown
- **Tier 2 (1000G):** 160 damage, 2s stun, 4s cooldown, +penetra shields

**Exemplo - Protego (Shield):**
- **Tier 0 (Grátis):** Absorve 100 HP, 3s duração
- **Tier 1 (500G):** Absorve 200 HP, 4s duração
- **Tier 2 (1200G):** Absorve 350 HP, 5s duração, reflete 30% damage

**Sistema:**
- Você escolhe quais feitiços upar a cada round
- Upgrades duram APENAS aquele round
- Round seguinte = volta ao tier 0

#### **CATEGORIA 2: ITENS CONSUMÍVEIS**

**Poções (Uso Único por Round):**
- **Felix Felicis (800G):** Próximo feitiço não tem cooldown
- **Poção Vigorizante (600G):** Regenera 50 HP instantâneo
- **Antídoto Universal (400G):** Remove DoTs e debuffs
- **Poção de Resistência (700G):** +50 armor por 15 segundos

**Equipamentos (Duram o Round):**
- **Manto de Invisibilidade (2000G):** 1 uso de 8s de invisibilidade
- **Mapa do Maroto (400G):** Revela inimigos em 20m por 5s, 40s CD
- **Timeturner Fragment (1500G):** Revive no local da morte (1x por round)
- **Deluminador (300G):** Apaga luzes de uma área, cria escuridão

#### **CATEGORIA 3: HABILIDADES EXTRAS (Unlock Temporário)**

**Ultimate Spells (Compra Desbloqueia por 1 Round):**
- **Expecto Patronum (3800G):** Invulnerabilidade 3s + empurra inimigos
- **Fiendfyre (4200G):** Wall de fogo que bloqueia passagem por 10s
- **Protego Maxima (3500G):** Domo de proteção para time inteiro (5s)
- **Avada Kedavra (5000G):** One-shot kill, mas 2s cast time + revela posição

**Habilidades Utilitárias:**
- **Apparition (1800G):** Teleporte curto 1x no round
- **Accio (1200G):** Puxa inimigo ou objeto
- **Wingardium Leviosa (900G):** Cria cobertura levitando objetos

### Progressão DENTRO da Partida (Elemento MOBA)

**Sistema de Maestria de Feitiços:**

À medida que você USA feitiços durante a partida, eles ganham "maestria":

- **5 acertos com Stupefy:** Tier automático +1 grátis (não precisa comprar)
- **3 kills com Incendio:** Burning dura +1 segundo
- **10 danos bloqueados com Protego:** Shield absorve +20 HP base

**Isso NÃO é level-up tipo MOBA.** É apenas recompensa por usar bem os feitiços, criando micro-progressão dentro da partida.

**Diferença Crucial:**
- MOBA: Level 1 → 25, desbloqueia novas habilidades, fica muito mais forte
- Wizarding Warfare: Começa completo, maestria dá pequenos buffs (5-10% boost)

---

## 🧙 SISTEMA DE WIZARDS (CLASSES)

### 12 Wizards Únicos (Heróis)

Cada jogador escolhe 1 wizard antes da partida. Cada wizard tem:
- **4 Feitiços Únicos** (Q, E, R, Ultimate)
- **Passiva** (sempre ativa)
- **Role:** Duelista, Sentinela, Controlador, Iniciador

Diferente de Valorant: você pode trocar de wizard entre rounds (custo: 200G)

### Exemplos de Wizards

---

#### **AUROR ELITE** - Duelista
*Ex-auror especializado em duelos mágicos diretos*

**Passiva - Reflexos de Combate:**
- Primeiro feitiço após matar inimigo tem cooldown reduzido em 50%

**Q - Stupefy Rápido (5s CD):**
- Projétil rápido, 90 damage, 0.8s stun
- Custo upgrade: 400/1000G

**E - Expelliarmus (12s CD):**
- Desarma inimigo (remove feitiço equipado por 3s)
- 60 damage
- Custo upgrade: 600/1400G

**R - Duelist's Dash (18s CD):**
- Dash rápido em direção à mira
- +30% fire rate por 4s após dash
- Custo upgrade: 900/2000G

**ULTIMATE - Protego Diabolica (Compra: 3800G):**
- Círculo de fogo azul ao redor (8m radius)
- Aliados dentro: +20% damage
- Inimigos dentro: 40 damage/segundo

---

#### **METAMORPHMAGUS** - Controlador
*Bruxo capaz de alterar terreno e controlar posicionamento*

**Passiva - Forma Adaptativa:**
- Ao ficar parado 3s, ganha camuflagem parcial (50% invisível)

**Q - Conjure Wall (8s CD):**
- Cria parede mágica sólida (10s duração)
- Pode ser destruída (300 HP)
- Custo upgrade: 500/1200G

**E - Smoke Serpent (20s CD):**
- Lança cobra de fumaça que viaja e explode
- Cria nuvem de fumaça 8s
- Custo upgrade: 700/1600G

**R - Transfiguration Trap (25s CD):**
- Coloca armadilha invisível
- Inimigo que pisar: transformado em sapo por 2s (não pode atacar)
- Custo upgrade: 1100/2500G

**ULTIMATE - Piertotum Locomotor (Compra: 4000G):**
- Anima 3 estátuas que patrulham área
- Atiram em inimigos (50 damage/tiro)
- 15s duração

---

#### **OCLUMÊNTE** - Sentinela
*Mestre em defesa mental e proteção de áreas*

**Passiva - Mental Fortress:**
- Imune a confusão e efeitos de controle mental

**Q - Protego (6s CD):**
- Shield frontal absorve 150 HP
- Reflete projéteis
- Custo upgrade: 500/1200G

**E - Legilimens Pulse (30s CD):**
- Revela inimigos em cone 25m por 3s
- Atravessa paredes
- Custo upgrade: 400/900G

**R - Anchor Point (35s CD - 2 cargas):**
- Coloca orbe mágico
- Aliados próximos ao orbe: regeneram 10 HP/s
- Custo upgrade: 800/1800G

**ULTIMATE - Mind Palace (Compra: 3500G):**
- Cria domo de proteção mental
- Aliados dentro: +30% redução de dano mágico
- 8s duração, 12m radius

---

#### **MORTÍFAGO** - Iniciador (Apenas time Comensais)
*Seguidor das Trevas especializado em iniciar combates*

**Passiva - Dark Mark:**
- Inimigos eliminados deixam marca que revela área por 5s

**Q - Crucio Beam (8s CD):**
- Feixe channeled (2s)
- 40 damage/segundo + slow 40%
- Custo upgrade: 600/1400G

**E - Shadow Step (20s CD):**
- Teleporte para sombra visível (15m max)
- Deixa clone de sombra 3s no local original
- Custo upgrade: 1000/2200G

**R - Fear Aura (30s CD):**
- AoE que causa "fear" (inimigos se afastam do centro)
- 3s duração
- Custo upgrade: 1200/2600G

**ULTIMATE - Morsmordre (Compra: 4500G):**
- Summon Marca Negra no céu
- Toda área fica escura por 12s
- Comensais: veem normalmente + 20% movimento
- Aurores: visão reduzida 70%

---

#### **FEITICEIRA DE CURA** - Suporte
*Especialista em cura e buffs para o time*

**Passiva - Healing Touch:**
- Habilidades que curam também removem 1 debuff

**Q - Episkey (7s CD):**
- Cura aliado 80 HP instantâneo
- Custo upgrade: 400/1000G

**E - Rejuvenation Field (25s CD):**
- Cria área de cura
- 20 HP/s para aliados dentro (6s duração)
- Custo upgrade: 800/1800G

**R - Wiggenweld Dart (15s CD):**
- Dispara poção à distância
- Aliado: cura 120 HP
- Inimigo: 70 damage + 30% heal reduction por 5s
- Custo upgrade: 700/1600G

**ULTIMATE - Phoenix Rebirth (Compra: 4200G):**
- Revive 1 aliado morto com 60% HP
- Pode ser pré-castado (guarda ressurreição por 15s)
- Range: 20m

---

#### **LEGILIMANTE** - Duelista/Assassino
*Mestre em invasão mental e combate psicológico*

**Passiva - Mind Reader:**
- Inimigos com <30% HP aparecem no minimap

**Q - Confundus Shot (6s CD):**
- Projétil que confunde (inverte controles por 1.5s)
- 50 damage
- Custo upgrade: 500/1200G

**E - Obliviate (18s CD):**
- Apaga "memória" do mapa (remove callouts/pings em área)
- Inimigos afetados: não aparecem no minimap por 6s
- Custo upgrade: 900/2000G

**R - Legilimens Strike (20s CD):**
- Dash + ataque corpo-a-corpo
- 150 damage + copia 1 feitiço do inimigo (pode usar 1x)
- Custo upgrade: 1100/2400G

**ULTIMATE - Mass Imperius (Compra: 5000G):**
- Mind control em 1 inimigo por 4s
- Você controla movimento dele
- Ele ataca aliados (reduced damage)

---

### Roles e Composição

**Duelista (2-3 por time):**
- Foco em kills e entry fragging
- High damage, low utility

**Sentinela (1-2 por time):**
- Segura sites e protege time
- Heals, shields, anchors

**Controlador (1-2 por time):**
- Controla espaço do mapa
- Smokes, walls, slows

**Iniciador (1 por time):**
- Inicia teamfights e revela inimigos
- Info gathering, engage tools

**Composição Ideal:** 2 Duelistas, 1 Sentinela, 1 Controlador, 1 Iniciador, 1 Flex

---

## 🗺️ DESIGN DE MAPAS

### 3 Mapas de Launch

Cada mapa inspirado em locais icônicos de Harry Potter:

---

#### **MAPA 1: MINISTRY ATRIUM**
*Átrio do Ministério da Magia*

**Layout:**
- 3 sites (A, B, C)
- **Mid:** Átrio central com estátua grande (cobertura)
- **Site A:** Departamento de Mistérios (corredores estreitos)
- **Site B:** Hall of Prophecies (vertical, prateleiras altas)
- **Site C:** Escritórios abertos (long sightlines)

**Elementos Únicos:**
- Lareiras Flu (teleporte entre 2 pontos fixos)
- Elevadores mágicos (movimento vertical)
- Portões de segurança (podem ser fechados/abertos)

**Vibe:** Arquitetura gótica, opulenta, muitas colunas

---

#### **MAPA 2: FORBIDDEN FOREST**
*Floresta Proibida de Hogwarts*

**Layout:**
- 3 sites (A, B, C)
- **Mid:** Clareira central com árvore gigante
- **Site A:** Acampamento de Centauros (alto, plataformas)
- **Site B:** Covil de Acrômantulas (baixo, teias)
- **Site C:** Ruínas antigas (mix de altura)

**Elementos Únicos:**
- Névoa mágica (reduz visão em certas áreas)
- Árvores destrutíveis (criam novas linhas de visão)
- Criaturas neutras (fazem barulho se inimigo passa)

**Vibe:** Escuro, claustrofóbico, natureza selvagem

---

#### **MAPA 3: DIAGON ALLEY**
*Beco Diagonal*

**Layout:**
- 2 sites (A, B) + área central disputada
- **Mid:** Rua principal (longa, aberta)
- **Site A:** Gringotts (interior, cofres, verticalidade)
- **Site B:** Florean Fortescue (praça aberta, sorveteria)

**Elementos Únicos:**
- Vitrines de lojas (quebram com feitiços, barulho)
- Beco Diagonal secreto (rota de flank escondida)
- Dragão de Gringotts (decorativo mas bloqueia visão)

**Vibe:** Urbano, medieval, movimentado

---

### Elementos Comuns a Todos Mapas

- **Escadas/Rampas:** Sempre 2+ formas de acessar altura
- **Cobertura Destrutível:** Alguns objetos quebram com dano
- **Interativos:** Portas, alavancas, plataformas móveis
- **Sound Cues:** Pisos de madeira fazem barulho, carpetes não
- **Callouts:** Nomes temáticos de HP para cada área

---

## ⚔️ SISTEMA DE COMBATE

### Mecânica Core: Varinha como Arma

**Ataque Básico (Clique Esquerdo):**
- Feixe de luz da varinha
- Funciona como "pistola" sempre disponível
- 40 damage/tiro
- 3 tiros/segundo
- Headshot: 80 damage (2x)
- Infinito (sem munição)

**Diferença de Armas Tradicionais:**
- Não existe recarga
- Dano é fixo (não há "rifles melhores")
- Diferencial está nas HABILIDADES compradas

### Sistema de Habilidades

Cada wizard tem 4 habilidades:
- **Q, E, R:** Habilidades únicas
- **Ultimate:** Precisa comprar cada round (3500-5000G)

**Cooldowns:**
- Cooldowns são individuais por habilidade
- Variam de 5s (básicas) a 35s (poderosas)
- Resetam entre rounds
- Alguns itens reduzem cooldowns globalmente

### Tipos de Feitiços

**Projectiles (Skillshots):**
- Stupefy, Expelliarmus, Incendio
- Precisa mirar e acertar
- Pode errar

**Hitscan (Instantâneo):**
- Sectumsempra, Difindo
- Acerta instantâneo na mira

**AoE (Área de Efeito):**
- Bombarda, Confringo
- Target ground, explode

**Channeled (Segurar botão):**
- Crucio, Protego
- Mantém efeito enquanto segura

**Utility:**
- Accio, Wingardium Leviosa
- Não causam dano direto

### Recursos do Jogador

- **HP:** 150 (padrão)
- **Shield:** 0-50 (comprado com itens)
- **Mana:** Infinita (cooldowns controlam uso)
- **Stamina:** Para dash/movimento (100, regen 20/s)

### Combate Avançado

**Spell Parry:**
- Com Protego ativo, pode refletir feitiços
- Timing perfeito (0.3s window) = dano 2x refletido
- Timing normal = apenas bloqueia

**Combos:**
- Alguns feitiços interagem:
  - Aguamenti + Glacius = Parede de gelo
  - Incendio + Diffindo = Explosão de fogo
  - Stupefy em inimigo stunado = 1.5x damage

**Headshots:**
- Varinha básica: 2x damage
- Maioria dos feitiços: sem headshot bonus
- Exceção: Sectumsempra e Difindo

---

## 📊 ELEMENTOS MOBA INTEGRADOS (Sutis)

### O Que Pegamos do MOBA:

✅ **Progressão de Itens Durante Partida**
- Sistema de economia persiste entre rounds
- Compra upgrades táticos
- Builds diferentes por situação

✅ **Habilidades Upgradáveis**
- Feitiços tem tiers (0, 1, 2)
- Escolha estratégica do que upar

✅ **Maestria de Feitiços**
- Micro-progressão por uso (não level-up)
- Recompensa skill expression

✅ **Cooldowns e Resource Management**
- Gerenciar cooldowns é crucial
- Timing de habilidades = skill ceiling alto

✅ **Roles Definidas**
- Cada wizard tem papel no time
- Composição importa

### O Que NÃO Pegamos do MOBA:

❌ **Sem Lanes/Creeps**
- É um shooter tático, não MOBA

❌ **Sem Farming**
- Dinheiro vem de round economy, não farm

❌ **Sem Level System**
- Não tem XP ou níveis 1-25
- Todos começam "completos"

❌ **Sem Torres/Objetivos MOBA**
- Objetivos são plant/defuse, não destruir estruturas

❌ **Sem Respawn in Round**
- Morreu = fica fora até próximo round

**Resumo:** Pegamos só a **profundidade tática** do MOBA (builds, habilidades, economia) e colocamos em um **shooter tático round-based**.

---

## 🎨 IDENTIDADE VISUAL 100% HARRY POTTER

### Estética Geral

**Tone:** Dark Fantasy, Guerra Mágica, Tom Sombrio
- Paleta: Azuis escuros, dourados, verdes, roxos
- Iluminação: Dramática, velas/tochas, magias brilhantes
- Atmosfera: Tensão, perigo, batalha épica

### UI/HUD

```
┌─────────────────────────────────────────────────────┐
│ [Minimap]          ROUND 7/24          [💰 2400G]  │
│   🗺️ Top-L         ATACANTES            Top-R 💰   │
├─────────────────────────────────────────────────────┤
│                                                     │
│                                                     │
│              🎮 VIEWPORT PRINCIPAL                  │
│                                                     │
│                                                     │
├─────────────────────────────────────────────────────┤
│ HP: ████████░░ 120/150    Shield: ███░ 30/50       │
│                                                     │
│ [Q] Stupefy    [E] Protego    [R] Dash    [ULT]    │
│   Ready          8s CD        Ready       🔒 3800G  │
│                                                     │
│ ITEMS: Felix Felicis [F] | Mapa Maroto [C]         │
└─────────────────────────────────────────────────────┘
```

**Elementos Únicos:**
- Minimap estilo Mapa do Maroto (pegadas de inimigos avistados)
- Damage numbers em fonte manuscrita mágica
- Kill feed com nomes de feitiços
- Reticle customizada por wizard (formato da varinha)

### Audio Design

**Feitiços:**
- Vozes em latim (Stupefy!, Protego!, etc)
- SFX distintos por tipo de magia
- Spatial audio preciso (ouve de onde vem)

**Ambiente:**
- Música orquestral adaptativa (intensifica em combate)
- Sons do mapa (vento, correntes, criaturas)
- Callouts de wizards (VO contextual)

**Announcer:**
- Voz de Ministério da Magia
- "Comensais tomaram o Site A!"
- "30 segundos restantes!"
- "Horcrux plantada, desarme agora!"

### Efeitos Visuais

**Feitiços:**
- Cores únicas por tipo:
  - Stupefy: Vermelho
  - Protego: Azul translúcido
  - Avada Kedavra: Verde néon
  - Incendio: Laranja/amarelo
- Rastros de partículas
- Impactos com brilho mágico

**Mortes:**
- Inimigo morto: desintegra em partículas
- Drop de varinha no chão (visual, não coletável)
- Skull icon no minimap

---

## 🎯 LOOP DE JOGO DETALHADO

### Round Típico (Exemplo)

**ROUND 5 - Comensais Atacando**

**0:00 - Fase de Compra (30s):**
```
Você (Auror Elite): 3200G acumulados
- Upa Stupefy para Tier 2 (1000G)
- Compra Felix Felicis (800G)
- Compra Mapa do Maroto (400G)
- Sobra: 1000G para próximo round
```

**0:30 - Saída da Base:**
- Time decide: "Rush B fake, then rotate A"
- Controlador vai primeiro com smoke
- Você segue atrás como entry fragger

**0:45 - Mid Control:**
- Inimigo aparece, você lança Stupefy
- Acerta! 160 damage + 2s stun (Tier 2 effect)
- Time avança

**1:05 - Execute no Site A:**
- Iniciador usa Ultimate, revela 2 inimigos
- Você dash para dentro (R)
- Ativa Felix Felicis (poção), lança Expelliarmus sem cooldown
- Kill!
- +200G no próximo round

**1:20 - Plant:**
- Teammate planta Horcrux
- Você se posiciona para defender
- +300G por assist no plant

**1:30 - Defesa Pós-Plant:**
- Ouve passos pelo Mapa do Maroto
- Pre-aim no canto
- Defensor aparece, você headshot com varinha
- Kill! +200G

**1:45 - Clutch:**
- 1v2 situação
- Usa Protego para bloquear feitiço
- Retalia com Stupefy
- Horcrux explode!

**ROUND GANHO:**
- Time ganha +3000G total
- Você pessoalmente: 3000 (vitória) + 200 (kill) + 200 (kill) + 300 (plant) = 3700G
- Próximo round: vai ter dinheiro para comprar Ultimate

---

## 🏆 PROGRESSÃO E META

### Sistema de Ranks

**Ranked Mode:**
- Iron → Bronze → Silver → Gold → Platinum → Diamond → Immortal → Radiant
- Placement matches: 5 jogos
- MMR hidden, RR (Rank Rating) visível
- Seasonal resets (a cada 3 meses)

**RR System:**
- Vitória: +15 a +30 RR (baseado em performance)
- Derrota: -10 a -25 RR
- MVP: +5 RR bonus
- Promote: 100 RR no rank

### Progressão Permanente

**Account Level:**
- XP por partidas jogadas
- Level 1-50: Unlocks cosméticos
- Level 50+: Prestígio, borders

**Battle Pass (Sazonal):**
- 50 tiers de recompensas
- Skins de varinhas
- Efeitos de feitiços customizados
- Sprays temáticos de HP
- Emotes (acenos, provocações)

**Desafios Diários/Semanais:**
- "Acerte 20 Stupefy em partidas"
- "Vença 3 rounds sem morrer"
- "Faça 5 desarmes de Horcrux"
- Recompensa: XP, currency cosmética

### Unlocks de Wizards

**Free Rotation:**
- 4 wizards gratuitos toda semana (rotação)

**Permanente:**
- Compra com "Grimório Points" (ganho jogando)
- OU compra com dinheiro real
- 12 wizards no launch, +2 por season

**Maestria de Wizard:**
- Jogue com 1 wizard para ganhar maestria
- Level 1-10 por wizard
- Unlock: títulos, skins exclusivas, estatísticas

---

## 🔧 IMPLEMENTAÇÃO TÉCNICA S&BOX

### Arquitetura do Projeto

```
WizardingWarfare/
├── code/
│   ├── Wizards/
│   │   ├── BaseWizard.cs (classe base)
│   │   ├── AurorElite.cs
│   │   ├── Metamorphmagus.cs
│   │   ├── Oclumente.cs
│   │   ├── Mortifago.cs
│   │   ├── Feiticeira.cs
│   │   └── Legilimante.cs
│   ├── Abilities/
│   │   ├── BaseAbility.cs
│   │   ├── ProjectileAbility.cs
│   │   ├── HitscanAbility.cs
│   │   ├── AoEAbility.cs
│   │   └── ChanneledAbility.cs
│   ├── GameMode/
│   │   ├── RoundSystem.cs (gerencia rounds)
│   │   ├── BombPlantDefuse.cs (horcrux)
│   │   ├── TeamManager.cs
│   │   ├── EconomySystem.cs (galeões)
│   │   └── BuyPhase.cs
│   ├── Items/
│   │   ├── BaseItem.cs
│   │   ├── Consumables/
│   │   │   ├── FelixFelicis.cs
│   │   │   ├── HealingPotion.cs
│   │   │   └── Antidote.cs
│   │   └── Equipment/
│   │       ├── InvisibilityCloak.cs
│   │       ├── MaraudersMap.cs
│   │       └── Timeturner.cs
│   ├── UI/
│   │   ├── HUD.cs
│   │   ├── BuyMenu.cs
│   │   ├── Scoreboard.cs
│   │   ├── KillFeed.cs
│   │   └── Minimap.cs
│   ├── Weapons/
│   │   └── Wand.cs (arma básica)
│   └── Systems/
│       ├── SpellUpgradeSystem.cs
│       ├── MaestrySystem.cs
│       ├── DamageSystem.cs
│       └── ReplicationManager.cs
```

### Código Exemplo: Sistema de Rounds

```csharp
public partial class RoundSystem : Entity
{
    [Net] public int CurrentRound { get; set; } = 0;
    [Net] public int AttackerScore { get; set; } = 0;
    [Net] public int DefenderScore { get; set; } = 0;
    [Net] public RoundState State { get; set; } = RoundState.Warmup;
    [Net] public float RoundTimeRemaining { get; set; }
    
    public const int RoundsToWin = 13;
    public const int MaxRounds = 24;
    public const float BuyPhaseTime = 30f;
    public const float CombatPhaseTime = 100f;
    
    public override void Spawn()
    {
        base.Spawn();
        
        if(Game.IsServer)
        {
            State = RoundState.BuyPhase;
            StartNewRound();
        }
    }
    
    [GameEvent.Tick.Server]
    public void ServerTick()
    {
        if(State == RoundState.BuyPhase)
        {
            RoundTimeRemaining -= Time.Delta;
            
            if(RoundTimeRemaining <= 0)
            {
                State = RoundState.Combat;
                RoundTimeRemaining = CombatPhaseTime;
                OnCombatPhaseStart();
            }
        }
        else if(State == RoundState.Combat)
        {
            RoundTimeRemaining -= Time.Delta;
            
            // Check win conditions
            if(CheckRoundEnd())
            {
                EndRound();
            }
            
            // Time ran out
            if(RoundTimeRemaining <= 0)
            {
                DefenderWins();
            }
        }
    }
    
    private void StartNewRound()
    {
        CurrentRound++;
        State = RoundState.BuyPhase;
        RoundTimeRemaining = BuyPhaseTime;
        
        // Swap sides at round 12
        if(CurrentRound == 13)
        {
            SwapTeams();
        }
        
        // Reset all players
        foreach(var player in Game.Clients)
        {
            var wizard = player.Pawn as BaseWizard;
            if(wizard == null) continue;
            
            wizard.Respawn();
            wizard.ResetAbilities();
            wizard.ClearItems();
            
            // Grant money
            GiveRoundMoney(wizard);
        }
        
        // Broadcast
        BroadcastRoundStart();
    }
    
    private void GiveRoundMoney(BaseWizard wizard)
    {
        int baseAmount = 800;
        
        // Loss streak bonus
        if(IsOnLosingStreak(wizard.Team))
        {
            baseAmount += 500;
        }
        
        wizard.Galleons += baseAmount;
    }
    
    private bool CheckRoundEnd()
    {
        // All attackers dead = defenders win
        if(GetAliveCount(Team.Attackers) == 0)
        {
            if(!IsBombPlanted())
            {
                DefenderWins();
                return true;
            }
        }
        
        // All defenders dead = attackers win
        if(GetAliveCount(Team.Defenders) == 0)
        {
            AttackerWins();
            return true;
        }
        
        // Bomb exploded
        if(BombExploded())
        {
            AttackerWins();
            return true;
        }
        
        // Bomb defused
        if(BombDefused())
        {
            DefenderWins();
            return true;
        }
        
        return false;
    }
    
    private void EndRound()
    {
        State = RoundState.PostRound;
        
        // Check match end
        if(AttackerScore >= RoundsToWin || DefenderScore >= RoundsToWin)
        {
            EndMatch();
            return;
        }
        
        // Overtime
        if(CurrentRound >= MaxRounds && AttackerScore == DefenderScore)
        {
            // Continue playing
        }
        
        // Wait 7 seconds, then start new round
        _ = Task.DelaySeconds(7f).ContinueWith(t => 
        {
            if(Game.IsServer)
                StartNewRound();
        });
    }
    
    private void AttackerWins()
    {
        AttackerScore++;
        DistributeRoundRewards(Team.Attackers, 3000);
        DistributeRoundRewards(Team.Defenders, 1900); // Loss money
    }
    
    private void DefenderWins()
    {
        DefenderScore++;
        DistributeRoundRewards(Team.Defenders, 3000);
        DistributeRoundRewards(Team.Attackers, 1900);
    }
}
```

### Código Exemplo: Spell Upgrade System

```csharp
public partial class SpellUpgradeSystem : Entity
{
    public enum UpgradeTier
    {
        Base = 0,
        Tier1 = 1,
        Tier2 = 2
    }
    
    public class SpellUpgrade
    {
        public string SpellName { get; set; }
        public UpgradeTier CurrentTier { get; set; } = UpgradeTier.Base;
        public int Tier1Cost { get; set; } = 400;
        public int Tier2Cost { get; set; } = 1000;
    }
    
    public static bool CanUpgrade(BaseWizard wizard, BaseAbility spell, UpgradeTier targetTier)
    {
        if(spell.CurrentTier >= targetTier)
            return false;
            
        int cost = targetTier == UpgradeTier.Tier1 ? spell.Tier1Cost : spell.Tier2Cost;
        
        return wizard.Galleons >= cost;
    }
    
    public static void UpgradeSpell(BaseWizard wizard, BaseAbility spell, UpgradeTier targetTier)
    {
        if(!CanUpgrade(wizard, spell, targetTier))
            return;
            
        int cost = targetTier == UpgradeTier.Tier1 ? spell.Tier1Cost : spell.Tier2Cost;
        
        wizard.Galleons -= cost;
        spell.CurrentTier = targetTier;
        spell.ApplyTierBonuses();
        
        // VFX e SFX
        ShowUpgradeEffect(wizard, spell);
    }
    
    [ClientRpc]
    private static void ShowUpgradeEffect(BaseWizard wizard, BaseAbility spell)
    {
        Particles.Create("particles/spell_upgrade.vpcf", wizard.Position);
        Sound.FromScreen("sounds/ui/spell_upgraded.vsnd");
    }
}
```

### Networking

**Client-Side Prediction:**
- Movimento do wizard
- Mira da varinha
- Animação de cast (cosmética)
- Trajetória inicial de projectiles

**Server Authority:**
- Hit detection (raycast ou projectile collision)
- Damage application
- Economy (compras, ganhos)
- Round state
- Bomb plant/defuse

**Tick Rate:**
- 64 tick server (padrão)
- 128 tick para competitivo (opcional)

**Lag Compensation:**
- Rewind time para hit detection
- Interpolation de movimento inimigo
- Extrapolation limitado (max 100ms)

---

## 🎯 BALANCEAMENTO E DESIGN

### Filosofia

1. **Skill Ceiling Alto:** Mestria recompensada
2. **Counterplay:** Toda estratégia tem resposta
3. **Build Diversity:** Múltiplas formas de gastar dinheiro
4. **Economia Importa:** Eco rounds vs full buy
5. **Teamplay Essencial:** 1v5 clutches são raros

### Métricas de Balanceamento

**Wizards:**
- Win rate: 48-52% ideal
- Pick rate: Nenhum acima de 30%
- Ban rate (ranked): <20%

**Mapas:**
- Win rate atacante/defensor: 50/50 ideal
- Variação aceitável: 45/55

**Economia:**
- Eco round win rate: <20%
- Pistol round win rate: ~50%
- Full buy win rate differential: <10%

### Ciclo de Patches

- **Hotfix:** Bugs críticos (24-48h)
- **Balance Patch:** A cada 2 semanas
- **Content Patch:** Novo wizard/mapa a cada 6-8 semanas
- **Season Patch:** Major changes a cada 3 meses

---

## 🚀 ROADMAP DE DESENVOLVIMENTO

### FASE 1: CORE PROTOTYPE (Mês 1-3)

**Objetivos:**
- Provar que o combate é divertido
- Round system funcional
- 1 mapa jogável

**Deliverables:**
- [x] Movimento básico de wizard (WASD, jump, crouch)
- [ ] Sistema de varinha (ataque básico funcionando)
- [ ] 2 wizards jogáveis com 4 abilities cada
- [ ] 1 mapa simples (3 sites)
- [ ] Round system básico (buy phase → combat → reset)
- [ ] UI mínima (HP, ability cooldowns, round timer)
- [ ] Sistema de bomb plant/defuse
- [ ] Networking básico (2 players testando)

**Milestone:** 2v2 playable match

---

### FASE 2: SYSTEMS & CONTENT (Mês 4-7)

**Objetivos:**
- Completar todos sistemas core
- 6 wizards balanceados
- 2 mapas polidos

**Deliverables:**
- [ ] 6 wizards únicos completos
- [ ] Sistema de economia (galeões, compra, persistência)
- [ ] 15+ itens compráveis (poções, equipamentos)
- [ ] Spell upgrade system (tier 0/1/2)
- [ ] Maestria de feitiços (micro-progressão)
- [ ] 2 mapas completos e balanceados
- [ ] UI completa (buy menu, scoreboard, minimap)
- [ ] Sistema de ranks básico
- [ ] Matchmaking simples
- [ ] Tutorial interativo

**Milestone:** 6v6 full match com economia

---

### FASE 3: POLISH & BETA (Mês 8-10)

**Objetivos:**
- Jogo completamente jogável
- Beta fechado
- Balanceamento intensivo

**Deliverables:**
- [ ] 12 wizards balanceados
- [ ] 3 mapas otimizados
- [ ] Sistema de progressão permanente (account level, unlocks)
- [ ] Cosméticos básicos (skins de varinha, efeitos)
- [ ] Ranked mode completo (placement, MMR, tiers)
- [ ] Replay system
- [ ] Spectator mode
- [ ] Anti-cheat básico
- [ ] Otimização de performance
- [ ] Beta fechado com 1000 players

**Milestone:** Closed Beta Launch

---

### FASE 4: LAUNCH (Mês 11-12)

**Objetivos:**
- Launch público
- Marketing
- Suporte pós-launch

**Deliverables:**
- [ ] Open Beta → Full Release
- [ ] Servidor dedicado infrastructure
- [ ] Matchmaking global (regiões)
- [ ] Battle Pass Season 1
- [ ] Ranked Season 1
- [ ] Trailer cinematográfico
- [ ] Press kit e marketing
- [ ] Discord community setup
- [ ] Patch cycle estabelecido
- [ ] Roadmap público para ano 1

**Milestone:** Public Launch

---

### FASE 5: LIVE SERVICE (Ano 1+)

- Novo wizard a cada 6-8 semanas
- Novo mapa a cada 3-4 meses
- Battle Pass sazonal (3 meses)
- Eventos temáticos (Halloween, Natal)
- Torneios oficiais
- Esports support (se tiver tração)

---

## 💰 MODELO DE MONETIZAÇÃO

### Free-to-Play Base

**Grátis:**
- 4 wizards em rotação semanal
- Acesso a todos modos de jogo
- Ranked mode
- Progressão de account level

**Premium (Compra):**
- Wizards permanentes (individual ou pack)
- Battle Pass sazonal
- Cosméticos premium

### Cosméticos (Sem Pay-to-Win)

**Skins de Varinhas:**
- Raridades: Comum, Raro, Épico, Lendário
- Efeitos visuais únicos
- Preço: $5-$20

**Efeitos de Feitiços:**
- Customiza cor/partículas de magias
- Preço: $3-$10

**Robes/Outfits:**
- Variações de visual do wizard
- Preço: $8-$15

**Outros:**
- Sprays de mapa
- Emotes
- Títulos/borders
- Kill banners

### Battle Pass

- $10 por season (3 meses)
- 50 tiers de recompensas
- Free track (todos jogadores)
- Premium track (quem comprou)
- Inclui: skins, efeitos, currency, XP boosts

---

## 📝 CONSIDERAÇÕES FINAIS

### Diferencial Competitivo

**vs Valorant:**
- ✨ Temática 100% Harry Potter (única no gênero)
- 🧙 Wizards com identidades fortes (não só habilidades genéricas)
- 💰 Economia + Spell Upgrades (mais profundidade)
- 🎯 Combate mágico (skillshots com varinhas, não guns)

**vs Deadlock:**
- 🎮 Round-based, não partidas de 40min
- ⚡ Ritmo mais rápido e acessível
- 🏆 Sem farming/lanes, foco em gunplay tático
- 🧩 Progressão mais simples (não overwhelm)

**vs Harry Potter Games Existentes:**
- 🎯 Primeiro shooter competitivo 6v6 de HP
- 🏅 Foco em skill, não casual
- 🌍 Multiplayer profundo, não single-player

### Riscos e Mitigações

**Risco 1: Licença de HP**
- Mitigação: Usar inspiração genérica, consultar advogado, evitar nomes registrados

**Risco 2: Balanceamento de 12 wizards**
- Mitigação: Alpha/Beta extensivo, patches frequentes, listen to community

**Risco 3: Competir com Valorant/CS**
- Mitigação: Nicho (fãs de HP + shooter tático), marketing focado

**Risco 4: Development scope**
- Mitigação: MVP primeiro (6 wizards, 2 mapas), iterate depois

### Próximos Passos Imediatos

**Semana 1:**
1. Setup S&Box project
2. Prototipo de movimento de wizard
3. Ataque básico de varinha funcional

**Semana 2:**
1. 1 habilidade projectile (Stupefy)
2. 1 habilidade defensive (Protego)
3. Cooldown system

**Semana 3-4:**
1. Arena teste simples
2. Round system básico (timer, reset)
3. Networking 2 players
4. First playtest 1v1

**Mês 2:**
1. 2 wizards completos (4 abilities cada)
2. Buy menu básico
3. Economia de galeões
4. Bomb plant/defuse
5. Playtest 2v2

**Mês 3:**
1. Mapa completo (3 sites)
2. 4 wizards
3. 10 itens compráveis
4. UI polida
5. **Milestone: First 6v6 match**

---

## 📞 RECURSOS E EQUIPE

### Stack Técnico

- **Engine:** S&Box (Source 2)
- **Linguagem:** C# (.NET)
- **3D:** Blender (wizards, props, mapas)
- **Texturas:** Substance Painter
- **VFX:** Houdini / EmberGen (magias)
- **UI:** Figma → Razor (S&Box UI)
- **Audio:** FMOD / Wwise
- **Version Control:** Git + LFS

### Equipe Mínima

- **2 Programmers** (gameplay, systems, networking)
- **1 Level Designer** (mapas, layout, flow)
- **1 Character Artist** (wizards, animations)
- **1 VFX Artist** (magias, particles, ambience)
- **1 UI/UX Designer** (menus, HUD, buy menu)
- **1 Sound Designer** (SFX, music, VO)
- **1 Game Designer** (balanceamento, economy, content)

Total: 8 pessoas

### Orçamento Estimado (Indie)

- **Desenvolvimento:** 12 meses
- **Salários:** $100k-$180k (depende de região/seniority)
- **Ferramentas/Licenses:** $10k
- **Marketing (launch):** $30k-$50k
- **Infrastructure (servers):** $5k-$10k (beta)

**Total:** ~$150k-$250k

---

**Documento vivo - Atualizar conforme desenvolvimento**

**Versão:** 2.0 - Round-Based Tactical Shooter  
**Última atualização:** [Data]  
**Status:** Pre-Production Design Complete

---

## 🎓 ANEXO: GLOSSÁRIO DE TERMOS

**Aurores:** Wizards do Ministério da Magia (defensores)  
**Comensais:** Seguidores das Trevas (atacantes)  
**Galeões:** Moeda do jogo (economia)  
**Horcrux:** Objetivo tipo bomb (plant/defuse)  
**Spell Tier:** Nível de upgrade do feitiço (0, 1, 2)  
**Maestria:** Micro-progressão por uso de feitiços  
**Felix Felicis:** Poção que remove cooldown  
**Mapa do Maroto:** Item que revela inimigos  
**Protego Diabolica:** Ultimate do Auror Elite  
**RR:** Rank Rating (pontos de ranqueada)

---

Quero que o visual seja 100% inspirado no deadlock