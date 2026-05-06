# 🧙‍♂️ WARLOCKS — MODO DOMINATION (CORE DESIGN)

## 🎯 Objetivo

Modo competitivo em equipes onde o objetivo é **capturar e manter controle de uma zona central** para acumular pontos até atingir a vitória.

---

# 🧩 Estrutura da Partida

## Formato
- Times: 2 (ex: 5v5 ou 6v6)
- Duração: até um time atingir 100%
- Respawn: ativo durante toda a partida

---

## Condição de Vitória
- Equipe acumula progresso ao controlar o ponto
- Primeiro time a atingir **100% de controle** vence

---

# 🗺️ Mapa

## Características
- Mapa pequeno/médio
- Foco em combate constante
- Rotas laterais (flanco)
- 1 ponto central (capture zone)

---

## Estrutura recomendada

- Área central aberta (zona de captura)
- 2–3 rotas de acesso
- Cobertura parcial (não totalmente aberta)

---

# 📍 Zona de Captura

## Regras básicas

- Jogadores dentro da área contam para captura
- Mais jogadores = captura mais rápida
- Inimigos dentro da área → ponto contestado

---

## Lógica de captura

```ts
if (teamA > teamB) {
  captureProgress += rate * (teamA - teamB);
}

if (teamB > teamA) {
  captureProgress -= rate * (teamB - teamA);
}
````

---

## Estados do ponto

* Neutro
* Capturando (A ou B)
* Controlado (A ou B)
* Contestando

---

# 🔥 Sistema Dinâmico (DIFERENCIAL DO WARLOCKS)

A zona reage às magias usadas nela.

---

## Interações por Afinidade

### 🔥 Fire

* Cria zona de dano contínuo no ponto
* Dificulta permanência

### ❄️ Ice

* Reduz velocidade de movimento
* Aumenta controle da área

### 🌑 Shadow

* Reduz visão dentro do ponto
* Facilita emboscadas

### ✨ Arcane

* Reduz cooldown de aliados dentro da área

---

## Regra

* Efeitos são temporários (ex: 3–5 segundos)
* Não stack infinito (refresh, não acumula)

---

# ⚔️ Combate no Ponto

## Influência dos Sistemas

### Afinidade

* Buffa magias dentro do ponto

### Disciplina

* Define papel no objetivo

Ex:

* Guardian → segura ponto
* Controller → trava inimigo
* Duelist → elimina rápido

---

### Tipos de Magia

| Tipo    | Função no ponto   |
| ------- | ----------------- |
| Offense | limpar inimigos   |
| Defense | sustentar posição |
| Control | impedir captura   |

---

# 💀 Sistema de Respawn

## Regras

* Respawn fixo por equipe
* Tempo: 5–10 segundos

---

## Objetivo

* Manter pressão constante
* Evitar downtime longo

---

# 🔁 Loop de Gameplay

1. Jogadores spawnam
2. Correm para o ponto
3. Combate intenso
4. Controle muda constantemente
5. Equipe segura ponto → ganha %
6. Respawn e repetição

---

# ⚙️ Balanceamento

## 1. Anti Snowball

* Captura desacelera perto de 100%
* Time adversário ganha leve bônus de recuperação

---

## 2. Penalidade por excesso de build

Aplicado normalmente:

* muitas offense → +cooldown
* muitas defense → -mobilidade
* muito control → -dano

---

## 3. Tempo de efeito

* Nenhum efeito domina sozinho
* Tudo é temporário

---

# 💥 Ultimate

## Regras

* Carrega durante o jogo
* Impacta o ponto fortemente

---

## Exemplos

* Fire → chuva de fogo no ponto
* Ice → congelamento em área
* Shadow → invisibilidade em massa

---

# 🎮 UI / Feedback

## Necessário

* Barra de progresso (0–100%)
* Indicador de controle (A/B/Contestado)
* Feedback visual das magias no ponto
* Contador de jogadores na área

---

# 🧠 Objetivo de Design

Esse modo foi criado para:

* Testar o sistema de magias
* Forçar combate constante
* Incentivar sinergia de equipe
* Validar builds e balanceamento

---

# 🚀 Expansão futura

* múltiplos pontos (Domination avançado)
* eventos no mapa
* zonas móveis

---

# 📌 Resumo

Domination em Warlocks não é só capturar ponto.

É um sistema onde:

* magias influenciam o ambiente
* builds impactam diretamente o objetivo
* controle de área define a vitória

👉 É o melhor modo para validar o core do jogo.

