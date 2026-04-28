# WIZARDING WARFARE - Game Design Document
## Shooter Tático 6v6 Round-Based | 100% Temática Harry Potter + Elementos MOBA

---

## 🎯 CONCEITO CENTRAL

**Wizarding Warfare** é um shooter tático 6v6 onde bruxos das trevas enfrentam aurores do Ministério da Magia em combates round-based intensos. Inspirado na **Guerra Mágica** do universo Harry Potter, o jogo combina a estrutura de rounds do **Valorant** com elementos de progressão do **Deadlock**, mas SEM ser um MOBA - a progressão serve apenas para criar profundidade tática dentro de cada partida.

**Fórmula:** Estrutura de Valorant + Progressão de Deadlock + 100% Harry Potter

> **Mudança de Design:** Não há wizards/agentes pré-definidos. Cada jogador monta seu próprio mago comprando magias livremente durante a fase de preparação, formando um deck de até 4 magias simultâneas.

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
  - Compra de magias e upgrades com Galeões
  - Montagem do deck (escolha até 4 magias para o round)
  - Compra de itens consumíveis e estratégia de time
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
1. **Comprar magias** para o seu deck do round
2. **Upar magias** que você já tem (Tier 1 → Tier 2)
3. **Comprar itens consumíveis** (poções, equipamentos)

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

---

### Sistema de Deck de Magias

**O jogador monta seu próprio mago.** Não há classes ou agentes fixos — você compra as magias que quiser e equipa até **4 simultâneas** no seu deck.

**Regras do Deck:**
- Capacidade: **4 magias ativas** por round
- Magias são compradas durante a fase de preparação com Galeões
- Magias compradas **permanecem entre rounds** (como armas no Valorant)
- Você pode **vender** uma magia para recuperar parte do valor
- Se morrer, perde as magias compradas naquele round (exceto as que já tinha de rounds anteriores)

**Round Inicial (Round 1 - Pistol Round equivalente):**
- Todo jogador começa com **800G**
- Dá para comprar 1-2 magias básicas (Tier 0)
- Deck de pistol = magias baratas + ataque básico de varinha

---

### Sistema de Mana

As magias consomem **Mana** além de ter cooldown. Mana funciona como uma stamina mágica:

**Mana:**
- **Máximo:** 100 mana
- **Regeneração:** 15 mana/segundo (começa 2s após usar a última magia)
- **Custo por Magia:** Varia (magias poderosas custam mais)
- **Não existe "sem mana"** no estilo de MOBA — as magias simplesmente ficam bloqueadas até ter mana suficiente

**Poção de Mana (500G):**
- Regenera 60 mana instantaneamente
- 1 uso por round
- Permite usar magias sem esperar a regen natural

**Interação Cooldown + Mana:**
- Magia só pode ser usada se: cooldown zerado **E** mana suficiente
- Isso cria decisões táticas (guardar mana para situação crítica vs usar agora)

---

### O Que Comprar com Galeões

#### **MAGIAS (Compra Permanece entre Rounds)**

Magias são compradas igual armas no Valorant. Cada magia tem:
- **Custo de compra** (Tier 0)
- **Custo de upgrade** para Tier 1 e Tier 2
- **Custo de mana** por uso

Tiers persistem entre rounds se você tiver mantido a magia.

#### **ITENS CONSUMÍVEIS (Uso no Round)**

**Poções:**
- **Felix Felicis (800G):** Próximo feitiço não tem cooldown nem custo de mana
- **Poção Vigorizante (600G):** Regenera 50 HP instantâneo
- **Poção de Mana (500G):** Restaura 60 mana instantaneamente
- **Antídoto Universal (400G):** Remove DoTs e debuffs
- **Poção de Resistência (700G):** +50 armor por 15 segundos

**Equipamentos (Duram o Round):**
- **Manto de Invisibilidade (2000G):** 1 uso de 8s de invisibilidade
- **Mapa do Maroto (400G):** Revela inimigos em 20m por 5s, 40s CD
- **Timeturner Fragment (1500G):** Revive no local da morte (1x por round)
- **Deluminador (300G):** Apaga luzes de uma área, cria escuridão

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

## 🧙 SISTEMA DE CUSTOMIZAÇÃO DO MAGO

### Montando Seu Próprio Wizard

**Não existem classes ou agentes fixos.** Cada jogador é um mago genérico que monta seu estilo de jogo comprando magias durante a fase de preparação — igual a comprar armas no Valorant.

**Regras:**
- Deck ativo: **até 4 magias simultâneas** (slots Q, E, R, F)
- Todas as magias estão disponíveis para todos os jogadores
- A composição do time emerge naturalmente das escolhas individuais
- Você pode reorganizar o deck entre rounds na fase de compra

**Estratégia de Composição:**
O time pode naturalmente dividir papéis: 1-2 jogadores ofensivos, 1-2 com utilidade/controle, 1 com defesa/suporte. Não há restrição — é resultado orgânico das compras.

---

### Catálogo de Magias

Todas as magias usam **mana + cooldown**. Cada magia tem Tier 0 (compra básica), com opção de upgrade para Tier 1 e Tier 2 na fase de preparação.

**Formato:**
> **Nome** | Custo: Tier0 / Tier1 upgrade / Tier2 upgrade | Mana: X | CD: Xs

---

#### ⚔️ OFENSIVAS — Dano Direto

Magias para bater e destruir inimigos.

---

**Stupefy** | 300G / +400G / +700G | Mana: 20 | CD: 5s
- *Projétil direto de atordoamento*
- **T0:** 80 dmg, 0.8s stun
- **T1:** 120 dmg, 1.2s stun, -0.5s CD
- **T2:** 160 dmg, 2s stun, penetra shields amarelos

**Incendio** | 400G / +500G / +900G | Mana: 25 | CD: 7s
- *Projétil de fogo que causa burning*
- **T0:** 60 dmg + 20 dmg/s por 3s (burning)
- **T1:** 80 dmg + 25 dmg/s por 4s
- **T2:** 100 dmg + 30 dmg/s por 5s, AoE ao impacto (2m)

**Sectumsempra** | 600G / +700G / +1100G | Mana: 30 | CD: 8s
- *Feixe hitscan de corte, headshot bonus*
- **T0:** 100 dmg, headshot 2x
- **T1:** 130 dmg, headshot 2.2x
- **T2:** 160 dmg, headshot 2.5x, causa bleeding (15 dmg/s por 3s)

**Confringo** | 500G / +600G / +1000G | Mana: 35 | CD: 10s
- *Explosão em área no ponto de impacto*
- **T0:** 70 dmg AoE (3m radius)
- **T1:** 90 dmg AoE (3.5m), knockback
- **T2:** 120 dmg AoE (4m), knockback forte, destrói coberturas

**Difindo** | 350G / +450G / +800G | Mana: 20 | CD: 6s
- *Corte hitscan rápido, baixo cooldown*
- **T0:** 70 dmg, headshot 1.8x
- **T1:** 90 dmg, headshot 2x
- **T2:** 110 dmg, headshot 2.2x, 2 cargas

---

#### 🧠 CONTROLE — Crowd Control

Servem para travar, puxar ou manipular inimigos.

---

**Impedimenta** | 500G / +600G / +1000G | Mana: 30 | CD: 9s
- *Slow pesado em área*
- **T0:** Slow 50% por 2s
- **T1:** Slow 60% por 2.5s, AoE (2m)
- **T2:** Slow 70% por 3s, AoE (3m), também afeta quem entra na área

**Petrificus Totalus** | 700G / +800G / +1200G | Mana: 40 | CD: 12s
- *Paralisa completamente um inimigo*
- **T0:** Root 1.5s (inimigo não move, ainda pode atirar)
- **T1:** Root 2s + silência habilidades
- **T2:** Root 2.5s + silência + reduz HP max em 20 durante efeito

**Accio** | 400G / +500G / +900G | Mana: 25 | CD: 10s
- *Puxa inimigo ou objeto em direção a você*
- **T0:** Puxa inimigo 6m em direção a você
- **T1:** Puxa 8m, causa 40 dmg no final
- **T2:** Puxa 10m, 60 dmg, stun 0.5s ao chegar

**Glacius** | 600G / +700G / +1100G | Mana: 35 | CD: 11s
- *Congela inimigo no lugar*
- **T0:** Freeze 1.5s (inimigo não move nem atira)
- **T1:** Freeze 2s + 30 dmg inicial
- **T2:** Freeze 2.5s + 50 dmg + inimigo fica frágil (recebe +20% dano por 3s após)

---

#### 🟡 FORÇA — Force

Quebram defesas específicas (escudos amarelos / Protego).

---

**Expelliarmus** | 450G / +550G / +950G | Mana: 25 | CD: 8s
- *Desarma inimigo removendo feitiço ativo*
- **T0:** Remove 1 feitiço equipado por 2s, 50 dmg
- **T1:** Remove 1 feitiço por 3s, 70 dmg, break shield amarelo
- **T2:** Remove 2 feitiços por 3s, 90 dmg, break shield + knockback

**Bombarda** | 650G / +800G / +1200G | Mana: 40 | CD: 12s
- *Explosão que destrói escudos e coberturas*
- **T0:** 90 dmg, destrói Protego instantaneamente
- **T1:** 120 dmg, destrói Protego + knockback forte
- **T2:** 150 dmg, destrói Protego + knockback + AoE (3m)

**Depulso** | 400G / +500G / +900G | Mana: 30 | CD: 9s
- *Empurra inimigo com força, quebra posicionamento*
- **T0:** Empurra 8m, 40 dmg
- **T1:** Empurra 10m, 60 dmg, break Protego parcial
- **T2:** Empurra 12m, 80 dmg, destrói shields e atordoa 0.5s

---

#### 🟣 CONTROLE PSÍQUICO — Manipulação

Afetam comportamento ou estado mental.

---

**Confundus** | 600G / +700G / +1100G | Mana: 35 | CD: 11s
- *Inverte os controles do inimigo por alguns segundos*
- **T0:** Inverte controles por 1.5s
- **T1:** Inverte controles por 2s + 40 dmg
- **T2:** Inverte controles por 2.5s + 60 dmg + reduz mana em 20

**Obliviate** | 500G / +600G / +1000G | Mana: 30 | CD: 15s
- *Remove informação do mapa — inimigos afetados somem do minimap*
- **T0:** Alvo some do minimap do time inimigo por 5s
- **T1:** Alvo + aliados do alvo somem por 6s
- **T2:** Alvo + aliados somem por 8s + remove callouts/pings da área

**Legilimens** | 700G / +900G / +1400G | Mana: 45 | CD: 20s
- *Revela posição e intenção — mostra o que o inimigo está fazendo*
- **T0:** Revela inimigo alvo por 3s (posição no minimap)
- **T1:** Revela inimigo + 2 aliados mais próximos por 4s
- **T2:** Revela time todo do alvo por 4s + mostra se estão comprando ou plantando

---

#### 🛡️ DEFESA — Sobrevivência e Contra-Ataque

---

**Protego** | 400G / +500G / +900G | Mana: 30 | CD: 6s
- *Escudo frontal que absorve dano e reflete projéteis*
- **T0:** Absorve 100 HP por 3s
- **T1:** Absorve 200 HP por 4s
- **T2:** Absorve 350 HP por 5s, reflete 30% do dano recebido

**Episkey** | 500G / +600G / +1000G | Mana: 25 | CD: 10s
- *Cura instantânea — funciona em si mesmo ou aliado próximo*
- **T0:** Cura 80 HP
- **T1:** Cura 120 HP, remove 1 debuff
- **T2:** Cura 160 HP, remove todos debuffs, +20 HP temporário por 5s

**Protego Horribilis** | 800G / +1000G / +1600G | Mana: 50 | CD: 20s
- *Escudo em área que protege aliados próximos*
- **T0:** Domo de 4m, absorve 80 HP para todos dentro por 3s
- **T1:** Domo de 5m, absorve 120 HP por 4s
- **T2:** Domo de 6m, absorve 200 HP por 5s, reflete projéteis

**Aguamenti** | 300G / +400G / +700G | Mana: 20 | CD: 8s
- *Jato de água que apaga burning e dá slow*
- **T0:** Remove burning de aliado, slow 20% em inimigo
- **T1:** Remove burning + bleeding, slow 30%, 40 dmg
- **T2:** Combo com Glacius: congela inimigo molhado instantaneamente

---

#### ☠️ ARTES DAS TREVAS — Magias Proibidas

Magias mais apelonas, alto risco/recompensa. Caras e poderosas.

---

**Crucio** | 900G / +1100G / +1800G | Mana: 50 | CD: 14s
- *Feixe channeled de dor extrema — slow pesado e dano contínuo*
- **T0:** Canal 2s, 45 dmg/s + slow 40%
- **T1:** Canal 2.5s, 55 dmg/s + slow 50%
- **T2:** Canal 3s, 65 dmg/s + slow 60% + causa "trauma" (inimigo fica com CD +20% por 5s)

**Fiendfyre** | 1200G / +1500G / +2200G | Mana: 65 | CD: 25s
- *Parede de fogo mágico que bloqueia passagem e causa dano massivo*
- **T0:** Parede de 6m por 8s, 30 dmg/s em quem tocar
- **T1:** Parede de 8m por 10s, 40 dmg/s
- **T2:** Parede de 10m por 12s, 50 dmg/s, persegue lentamente

**Morsmordre** | 1500G / +1800G / +2600G | Mana: 70 | CD: 30s
- *Marca das Trevas — escurece área e dá vantagem para quem lançou*
- **T0:** Escuridão 8m por 10s, você vê normalmente dentro
- **T1:** Escuridão 10m por 12s, aliados também veem
- **T2:** Escuridão 12m por 15s, aliados +15% movimento dentro

**Avada Kedavra** | 3500G (sem upgrade) | Mana: 90 | CD: Round único
- *A maldição da morte — mata instantaneamente, mas tem custo altíssimo*
- **Custo:** 3500G, só pode ser usada **1 vez por round**
- **Mecânica:** 2.5s de cast time visível + revela sua posição no minimap inimigo por 5s
- **Efeito:** Kill instantâneo em qualquer HP (exceto quem tiver Protego ativo)
- **Contraplay:** Protego T2 absorve completamente; Petrificus cancela o cast

---

#### 🧰 UTILITÁRIAS — Exploração e Suporte

Fora de combate, mas algumas ajudam na luta.

---

**Apparition** | 700G / +900G / +1400G | Mana: 40 | CD: 20s
- *Teleporte rápido em linha de visão (máximo 15m)*
- **T0:** Teleporte 12m, pequeno delay (0.5s)
- **T1:** Teleporte 15m, delay reduzido (0.3s)
- **T2:** Teleporte 18m, sem delay, pode usar através de fumaça

**Wingardium Leviosa** | 400G / +500G / +900G | Mana: 25 | CD: 15s
- *Levita objetos do ambiente criando cobertura temporária*
- **T0:** Levita 1 objeto médio, cobertura 8s
- **T1:** Levita 1 objeto grande, cobertura 10s
- **T2:** Levita até 2 objetos, cobertura 12s, pode empurrar inimigos com o objeto

**Nox / Lumos** | 200G (sem upgrade) | Mana: 10 | CD: 3s
- *Apaga ou acende luz na área — cria escuridão ou revela área escura*
- Utilidade situacional, barata, boa para eco rounds

**Mapa do Maroto** | 400G (item, não magia) | — | CD: 40s
- *Revela inimigos em 20m por 5s*
- Item consumível equipado no slot de item, não ocupa slot de magia

---

### Composição e Sinergia de Time

Sem classes fixas, os papéis emergem das compras. Exemplos de synergias:

| Combo | Efeito |
|-------|--------|
| Glacius + Confringo | Inimigo congelado recebe dano duplo da explosão |
| Aguamenti + Glacius | Inimigo molhado → congelado instantaneamente |
| Accio + Bombarda | Puxa inimigo para dentro da explosão |
| Crucio + Sectumsempra | Crucio stuna/slowa, Sectumsempra acerta headshot fácil |
| Legilimens + Morsmordre | Sabe onde todos estão + cega time inimigo |

**Eco Round Sugerido (800G):**
- Stupefy T0 (300G) + Protego T0 (400G) = 700G, fica 100G de sobra

**Full Buy (3000G+):**
- Sectumsempra T1 + Glacius T1 + Apparition T0 + Protego T1 = ~3200G

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
- **Mana:** 0-100 (regenera 15/s após 2s de pausa no uso)
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
Você: 3200G acumulados
Deck atual (já tinha de rounds anteriores):
  [Q] Stupefy T1 (comprado no round 3)
  [E] Apparition T0 (comprado no round 2)

Decisão do round 5:
  - Upa Stupefy para T2: +700G
  - Compra Sectumsempra T0: 600G
  - Compra Poção de Mana: 500G
  - Sobra: 1400G para próximo round

Deck final: Stupefy T2 / Apparition T0 / Sectumsempra T0 / (slot vazio)
Mana: 100/100 | HP: 150
```

**0:30 - Saída da Base:**
- Time decide: "Rush B fake, then rotate A"
- Aliado com Morsmordre vai abrir escuridão no mid
- Você segue como entry fragger

**0:45 - Mid Control:**
- Inimigo atrás de cobertura, você lança Stupefy
- Acerta! 160 dmg + 2s stun (T2 effect) — custa 20 mana
- Mana: 80/100 | Time avança

**1:05 - Execute no Site A:**
- Aliado usa Legilimens, revela 2 inimigos atrás da parede
- Você usa Apparition (40 mana) para flanquear
- Lança Sectumsempra (30 mana) + headshot = 100 + 160 = Kill!
- Mana: 10/100 — crítico!
- Usa Poção de Mana → +60 mana → 70/100
- +200G

**1:20 - Plant:**
- Teammate planta Horcrux
- Você se posiciona para defender
- +300G por assist no plant

**1:30 - Defesa Pós-Plant:**
- Ouve passos, lança Stupefy no corner
- Headshot na varinha básica (sem custo de mana) = 80 dmg
- Follow-up Stupefy = Kill! +200G
- Mana regenerou: 55/100

**1:45 - Clutch:**
- 1v2, mana em 55
- Usa Protego (30 mana) para bloquear feitiço
- Retalia com Stupefy (20 mana) = Kill!
- Fica com 5 mana — sem feitiços disponíveis
- Elimina o último com varinha básica
- Horcrux explode!

**ROUND GANHO:**
- Time ganha +3000G total
- Você: 3000 (vitória) + 200 + 200 (kills) + 300 (plant) = 3700G
- Próximo round: 3700 + 1400 sobra = 5100G → pode comprar Crucio T1 + Avada Kedavra

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
│   ├── Player/
│   │   ├── WizardPlayer.cs (jogador base, sem classe fixa)
│   │   ├── SpellDeck.cs (gerencia os 4 slots de magia)
│   │   └── ManaSystem.cs (mana + regeneração)
│   ├── Spells/
│   │   ├── BaseSpell.cs (classe base de magia)
│   │   ├── SpellCatalog.cs (registro de todas magias)
│   │   ├── Offensive/
│   │   │   ├── Stupefy.cs
│   │   │   ├── Incendio.cs
│   │   │   ├── Sectumsempra.cs
│   │   │   ├── Confringo.cs
│   │   │   └── Difindo.cs
│   │   ├── Control/
│   │   │   ├── Impedimenta.cs
│   │   │   ├── PetrifucusTotalus.cs
│   │   │   ├── Accio.cs
│   │   │   └── Glacius.cs
│   │   ├── Force/
│   │   │   ├── Expelliarmus.cs
│   │   │   ├── Bombarda.cs
│   │   │   └── Depulso.cs
│   │   ├── PsychicControl/
│   │   │   ├── Confundus.cs
│   │   │   ├── Obliviate.cs
│   │   │   └── Legilimens.cs
│   │   ├── Defense/
│   │   │   ├── Protego.cs
│   │   │   ├── Episkey.cs
│   │   │   ├── ProtegoHorribilis.cs
│   │   │   └── Aguamenti.cs
│   │   ├── DarkArts/
│   │   │   ├── Crucio.cs
│   │   │   ├── Fiendfyre.cs
│   │   │   ├── Morsmordre.cs
│   │   │   └── AvadaKedavra.cs
│   │   └── Utility/
│   │       ├── Apparition.cs
│   │       ├── WingardiumLeviosa.cs
│   │       └── NoxLumos.cs
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
- [x] Sistema de varinha (ataque básico funcionando) — `WandAttack.cs` hitscan 40/80 dmg
- [~] 2 wizards jogáveis com 4 abilities cada — 3 abilities: Q Stupefy, E Protego, R Apparition (falta slot F)
- [ ] 1 mapa simples (3 sites)
- [x] Round system básico (buy phase → combat → reset) — `RoundManager.cs`
- [x] UI mínima (HP, ability cooldowns, round timer) — `GameHud.razor` + `BuyMenu.razor`
- [x] Sistema de bomb plant/defuse — `HorcruxSite.cs` plant 3s / defuse 7s / fuse 45s
- [ ] Networking básico (2 players testando)

**Milestone:** 2v2 playable match

---

### FASE 2: SYSTEMS & CONTENT (Mês 4-7)

**Objetivos:**
- Completar todos sistemas core
- 6 wizards balanceados
- 2 mapas polidos

**Deliverables:**
- [~] 6 wizards únicos completos — 6 feitiços implementados: Stupefy, Protego, Apparition, Incendio, Sectumsempra, Impedimenta + Episkey (7 total)
- [x] Sistema de economia (galeões, compra, persistência) — `RoundManager` + `BaseAbility.TryUpgrade`
- [~] 15+ itens compráveis (poções, equipamentos) — 3 consumíveis: HealPotion, ManaPotion, FelixFelicis
- [x] Spell upgrade system (tier 0/1/2) — `BaseAbility` com ManaCost + TryUpgrade
- [x] Maestria de feitiços (micro-progressão) — `MasterySystem.cs`
- [ ] 2 mapas completos e balanceados
- [x] UI completa (buy menu, scoreboard, minimap) — `BuyMenu` (Q/E/R/F) + `Scoreboard` (Tab)
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

**Semana 1:** ✅
1. ~~Setup S&Box project~~
2. ~~Prototipo de movimento de wizard~~
3. ~~Ataque básico de varinha funcional~~

**Semana 2:** ✅
1. ~~1 habilidade projectile (Stupefy)~~
2. ~~1 habilidade defensive (Protego)~~
3. ~~Cooldown system~~

**Semana 3-4:**
1. Arena teste simples
2. ~~Round system básico (timer, reset)~~
3. Networking 2 players
4. First playtest 1v1

**Mês 2:**
1. 2 wizards completos (4 abilities cada) — falta slot F + 1 ability
2. ~~Buy menu básico~~
3. ~~Economia de galeões~~
4. ~~Bomb plant/defuse~~
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
**Última atualização:** 2026-04-27  
**Status:** Fase 2 em progresso — 6/10 deliverables concluídos

---

## 🎓 ANEXO: GLOSSÁRIO DE TERMOS

**Aurores:** Time do Ministério da Magia (defensores)  
**Comensais:** Seguidores das Trevas (atacantes)  
**Galeões:** Moeda do jogo (economia)  
**Horcrux:** Objetivo tipo bomb (plant/defuse)  
**Deck:** Os 4 slots de magia ativos do jogador no round  
**Spell Tier:** Nível de upgrade do feitiço (T0 compra base, T1/T2 upgrade)  
**Mana:** Recurso que as magias consomem (regenera 15/s após 2s de pausa)  
**Maestria:** Micro-progressão por uso de feitiços  
**Felix Felicis:** Poção que remove cooldown e custo de mana do próximo feitiço  
**Mapa do Maroto:** Item que revela inimigos em 20m  
**Eco Round:** Round onde se compra pouco (≤800G) para economizar  
**Full Buy:** Round com compra máxima (3000G+)  
**RR:** Rank Rating (pontos de ranqueada)  
**Artes das Trevas:** Categoria de magias proibidas de alto risco/recompensa  
**Force:** Categoria de magias que quebram escudos amarelos (Protego)

---

Quero que o visual seja 100% inspirado no deadlock