# 🧙‍♂️ WARLOCKS — CORE GAME DESIGN (PROTÓTIPO)

## 🎯 Visão Geral

Warlocks é um jogo competitivo baseado em rounds onde todos os jogadores possuem o mesmo “corpo base”, mas constroem seu estilo de jogo através de um sistema de:

- Afinidade (elemento)
- Disciplina (estilo de combate)
- Deck de magias (customização ativa)

O objetivo é manter **liberdade de build**, mas com **controle de balanceamento** e **identidade de gameplay**.

---

# 🧩 Estrutura do Player

Cada jogador monta seu “personagem” antes da partida escolhendo:

## 1. Afinidade
Define o elemento principal do jogador.

**Exemplos:**
- Fire
- Ice
- Arcane
- Shadow

### Função:
- Buffa magias relacionadas
- Cria sinergias
- Define estilo indireto

### Regras:
- NÃO limita magias
- Apenas fortalece algumas

### Exemplo:
| Afinidade | Efeito |
|----------|--------|
| Fire | +15% dano e aplica burn |
| Ice | +20% duração de slow |
| Arcane | -15% cooldown |
| Shadow | bônus de mobilidade/interações |

---

## 2. Disciplina
Define o estilo de combate do jogador.

**Exemplos:**
- Duelist
- Guardian
- Controller

### Função:
- Modifica atributos base
- Cria vantagens e desvantagens

### Exemplo:

| Disciplina | Buff | Nerf |
|-----------|------|------|
| Duelist | +dano | -defesa |
| Guardian | +escudo | -mobilidade |
| Controller | +controle | -dano |

---

## 3. Passiva
Habilidade fixa escolhida pelo jogador.

### Função:
- Personalização adicional
- Criação de identidade única

### Exemplo:
- +10% velocidade após usar magia
- recuperar mana ao causar dano
- reduzir cooldown ao eliminar inimigo

---

# 🔮 Sistema de Magias

## Tipos de Magia

Cada magia pertence a um tipo:

- **Offense** → dano
- **Defense** → proteção
- **Control** → controle de área ou inimigo

---

## Deck

Cada jogador possui:

- 6 slots de magia
- limite de energia (ex: 10 pontos)

---

## Sistema de Energia

Cada magia possui custo:

| Tier | Custo |
|------|------|
| T1 | 1 |
| T2 | 2 |
| T3 | 4 |

### Regra:
O total não pode ultrapassar o limite.

👉 Evita builds quebradas automaticamente.

---

## Tiers de Magia

Magias evoluem durante a partida.

### Importante:
Tier NÃO aumenta só números — muda comportamento.

### Exemplo (Fireball):

- T1 → projétil simples  
- T2 → explosão em área  
- T3 → cria zona de fogo  

---

# ⚖️ Sistema de Balanceamento

## 1. Penalidade por Excesso

Evita stacking de um único tipo.

### Regras:

- Muitas **offense** → +cooldown
- Muitas **defense** → -mobilidade
- Muitas **control** → -dano

### Objetivo:
Permitir liberdade, mas punir exagero.

---

## 2. Afinidade vs Magia

- Magia da mesma afinidade → buff
- Magia diferente → neutra ou leve penalidade

---

## 3. Disciplina vs Magia

Disciplina influencia eficiência:

### Exemplo:
Guardian:
- Defense → mais forte
- Offense → mais fraco

---

# 🔗 Sistema de Combos

Magias interagem entre si.

### Exemplos:

| Combinação | Resultado |
|-----------|----------|
| Ice + Impact | Freeze |
| Fire + Wind | espalha fogo |
| Shadow + Control | Silence |

### Objetivo:
- Incentivar trabalho em equipe
- Criar gameplay emergente

---

# 💥 Ultimate

## Características:

- NÃO é comprada
- Carrega durante a partida
- Baseada no estilo do jogador

### Carregamento:
- Dano causado
- Assistências
- Objetivos

### Exemplo:
- Fire → chuva de fogo em área
- Ice → congelamento massivo
- Shadow → invisibilidade + burst

---

# 🔁 Loop de Gameplay

## Início do Round:
- Jogador monta deck
- Escolhe magias dentro do limite

## Combate:
- Uso de habilidades
- Sinergias e combos
- Evolução de magias

## Fim do Round:
- Ajuste de 1 magia OU upgrade
- Recomeça ciclo

---

# 🎲 Sistema de Draft (Opcional)

Para evitar meta repetitivo:

- jogador recebe 3 opções
- escolhe 1

---

# 🧠 Filosofia do Sistema

## O que NÃO fazer:
- travar magias por afinidade
- permitir build sem custo
- tier só aumentar número

## O que FAZER:
- incentivar sinergia
- permitir liberdade com consequência
- criar identidade sem personagem fixo

---

# ✅ Resultado Esperado

- Alta rejogabilidade
- Builds variadas
- Meta controlado
- Gameplay dinâmico
- Forte identidade por jogador

---

# 🚀 Expansão futura

- novas afinidades
- novas disciplinas
- novas magias
- novas passivas
- modos de jogo

---

## 📌 Resumo

O jogador NÃO escolhe um personagem.

Ele CRIA um personagem através de:
- Afinidade
- Disciplina
- Passiva
- Deck

👉 Isso entrega liberdade + balanceamento + profundidade.