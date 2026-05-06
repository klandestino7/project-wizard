# Warlocks Core Refactor Blueprint

## Goal

Move the project away from a "Counter-Strike with spells" core and into the player-driven system described in [new-concept.md](./new-concept.md):

- affinity defines synergy
- discipline defines combat profile
- passive defines identity
- spell deck defines active playstyle
- energy budget controls build power
- spell tiers change behavior, not just numbers

This document maps the current project structure to that target and proposes the refactor order.

## Modularity Principle

The project should be split into two layers:

- `CORE`: systems that define what a wizard is and how magical combat works
- `GAME MODE`: systems that define how a match is won, what the objective is, and what rules are active

The rule for future development should be simple:

- if a system is needed by every wizard in every mode, it belongs to `CORE`
- if a system changes from mode to mode, it belongs to `GAME MODE`

This is the most important protection against future rewrites.

## Transition Rule

Anything inherited from the old "CS-like" structure that is not part of the long-term Warlocks vision can stay temporarily during migration, but should be treated as transitional only.

That means:

- keep it only if it helps preserve stability while the new core is being built
- isolate it from the new core as much as possible
- remove it once the new core and its replacement mode rules are ready

The project should not carry legacy tactical-shooter assumptions longer than necessary.

## Current Diagnosis

The project already has a strong magical combat base, but the core loop is still anchored to a tactical shooter structure.

### What is already valuable

- `code/Spells/*`: the game already has concrete spell execution paths
- `code/PawnSystem/Player/Wand.cs`: there is a single casting executor, which is good for future stat modifiers
- `code/PawnSystem/Player/ManaSystem.cs`: mana exists and can remain part of the combat loop
- `code/PawnSystem/Player/PlayerController/PlayerPawn.OldMethods.cs`: combat state, burn, stun, airborne, dodge and lock-on already create a wizard identity
- `code/UI/*`: there is enough UI infrastructure to support a new pre-round build flow

### What is fighting the new concept

- `code/GameMode/RoundManager.cs` is fully centered on buy phase, attacker/defender sides and Horcrux plant/defuse
- `code/PawnSystem/Player/SpellsDeck.cs` models spell ownership as buy, sell and upgrade through money
- `code/Spells/SpellCatalog.cs` is still defined as a shop catalog instead of a build catalog
- `code/UI/HUD/BuyMenu.razor` is a purchase screen, not a build-composition screen
- `code/PawnSystem/Client.Money.cs` and `GiveGalleons` / `SpendGalleons` keep the player economy as a first-class system
- objective, money and round-state terminology leak into the whole game instead of staying mode-specific

## Main Architectural Problem

Today the player is defined like this:

- base pawn
- team
- money
- owned spells
- equipped slots

The target concept needs the player to be defined like this:

- base pawn
- team
- affinity
- discipline
- passive
- spell loadout
- spell progression
- combo state
- ultimate charge

That means the refactor must start by changing the player's domain model.

## Target Modular Architecture

### CORE layer

This layer must be mode-agnostic.

Responsibilities:

- player build identity
- spell registry
- deck validation
- mana and cooldowns
- spell cast pipeline
- combat statuses
- combo logic
- ultimate charge
- generic spawn / respawn hooks
- shared HUD data contracts

Suggested modules:

- `Core/Build`
- `Core/Spells`
- `Core/Combat`
- `Core/Progression`
- `Core/UI`
- `Core/Shared`

### GAME MODE layer

This layer composes rules on top of the core.

Responsibilities:

- score rules
- win conditions
- round rules
- objective rules
- economy rules if a mode needs economy
- respawn policy
- team policy
- map-specific rule bindings

Suggested modules:

- `GameModes/Shared`
- `GameModes/SearchAndDestroy`
- `GameModes/Domination`
- `GameModes/Arena`
- `GameModes/Payload`

### Composition rule

A game mode should configure the match by composing reusable rule components, instead of editing player systems directly.

Good:

- a mode enables `RoundRule`, `TeamScoreRule`, `HorcruxObjectiveRule`
- a mode enables `RespawnRule` and `LoadoutLockRule`

Bad:

- a mode directly edits `PlayerPawn` combat code
- a mode hardcodes spell behavior changes inside objective code
- a mode makes the core deck system depend on buy zones or cash

## Core Refactor Direction

### 1. Separate match rules from player identity

Create a new player-build layer that is independent from:

- money
- buy zones
- bomb/defuse logic
- attacker/defender assumptions

And create a mode-rule layer that depends on core, but core never depends on mode rules.

Suggested new domain objects:

- `PlayerBuildComponent`
- `AffinityDefinition`
- `DisciplineDefinition`
- `PassiveDefinition`
- `SpellLoadout`
- `SpellProgressionState`
- `UltimateChargeComponent`

### 2. Replace shop ownership with build composition

`SpellsDeck` should stop representing:

- bought spells
- sold spells
- refund logic
- tier purchase cost

It should start representing:

- selected spell ids
- slot assignment
- energy cost validation
- type distribution
- progression state per spell
- loadout legality

In practice, `OwnedMask`, `ClientBuy`, `ClientSell`, `SpendGalleons` and refund logic should disappear from the core deck system.

If a future game mode wants a shop, that shop should live in a mode module and produce a valid `SpellLoadout`, not redefine the core deck rules.

### 3. Turn `SpellCatalog` into gameplay metadata

`SpellCatalog` should stop being a store table and become a gameplay registry.

Add metadata such as:

- `Affinity`
- `SpellType` (`Offense`, `Defense`, `Control`, maybe `Mobility` or `Utility`)
- `EnergyCost`
- `TierBehaviors`
- `ComboTags`
- `BaseUltimateContribution`

This keeps all balancing rules data-driven.

### 4. Make tiers behavior-driven

The concept explicitly says tiers should change behavior. Right now the code supports tiers, but the structure still reads like an upgrade shop.

Refactor spells so each spell can express:

- tier 1 behavior
- tier 2 behavior
- tier 3 behavior

Instead of only:

- damage increase
- cooldown reduction
- cost increase

### 5. Add a build validator layer

The new concept depends on systemic constraints, not economy constraints.

Create a validator that checks:

- max spell slots
- total energy budget
- affinity synergy
- discipline modifiers
- excess-type penalties

Suggested object:

- `BuildValidationService`

Outputs:

- valid or invalid
- total energy used
- offense / defense / control counts
- derived penalties
- derived bonuses

### 6. Move balance rules out of `PlayerPawn`

`PlayerPawn` should not directly know balance formulas like:

- "same affinity gives bonus"
- "too much offense adds cooldown"
- "guardian weakens offense"

Instead, casting should flow like this:

1. input requests cast
2. `Wand` asks build/modifier systems for resolved spell stats
3. resolved stats are used by the spell
4. combat events feed progression and ultimate charge

Suggested systems:

- `SpellModifierResolver`
- `BuildPenaltyResolver`
- `UltimateChargeSystem`
- `SpellComboSystem`

These systems should be callable by any mode.

## Contract Between Core And Modes

Modes should only interact with the core through stable contracts.

Examples:

- `IPlayerBuildProvider`
- `ISpellModifierSource`
- `IMatchRule`
- `IObjectiveRule`
- `IRespawnRule`
- `IScoringRule`

That way:

- the core exposes capabilities
- the mode composes behavior
- no mode needs to fork core classes just to add a new ruleset

## Example Of The Right Dependency Direction

Correct:

- `DominationMode -> uses PlayerBuildComponent`
- `SearchAndDestroyMode -> uses PlayerBuildComponent`
- `PlayerBuildComponent -> knows nothing about Domination or SearchAndDestroy`

Incorrect:

- `PlayerBuildComponent -> checks if current mode has buy phase`
- `SpellCatalog -> contains Horcrux-specific objective rules`
- `Wand -> branches around specific game mode names`

## Recommended New Core Shape

### Keep as reusable foundation

- `PlayerPawn`
- `Wand`
- `ManaSystem`
- damage and status systems
- projectile / hitscan execution
- round loop if you still want rounds

### Convert into build-driven systems

- `SpellsDeck`
- `SpellCatalog`
- `BuyMenu`
- spell upgrade flow

### Push out of the core into a specific mode

- Horcrux plant/defuse
- buy phase
- buy zones
- money rewards
- side swap assumptions
- attacker/defender round win conditions
- any objective-specific interaction flow
- any scoring system tied to one mode fantasy

### Mark as legacy and remove later if they do not fit Warlocks

- cash economy as a default player progression model
- round-start shopping as a core interaction
- bomb-site style objective assumptions
- side-swap structure if a mode does not need it
- attacker/defender naming embedded into shared systems
- shooter-derived rule logic that exists only because the original base came from CS

## Suggested Folder Direction

One possible structure:

```text
code/
  Core/
    Build/
    Combat/
    Spells/
    Progression/
    UI/
    Shared/
  GameModes/
    Shared/
    SearchAndDestroy/
    Domination/
    Arena/
  PawnSystem/
  World/
  Utils/
```

You do not need to physically move everything on day one, but the refactor should follow this separation.

## Refactor Phases

### Phase 1. Establish the new player build model

Deliverables:

- `AffinityType` enum or definitions
- `DisciplineType` enum or definitions
- `PassiveType` enum or definitions
- `PlayerBuildComponent`
- `SpellType` and `EnergyCost` in catalog

Do not touch spell visuals yet. Focus on the model.

Output expectation:

- a wizard can exist with a valid build even if no specific mode is active

### Phase 2. Rebuild the deck as a legal loadout system

Deliverables:

- replace buy/sell with select/unselect
- support 6 spell slots as described in the concept
- energy budget validation
- type counting
- build preview data for UI

This is the biggest turning point in the refactor.

Output expectation:

- any mode can ask the core for the player's resolved loadout

### Phase 3. Resolve combat through build modifiers

Deliverables:

- affinity bonuses applied on cast or on effect
- discipline buffs and nerfs
- passive triggers
- excess-type penalties

At this point the player starts to feel like a custom wizard instead of a spell shopper.

### Phase 4. Rework progression

Deliverables:

- round-end choose-one upgrade flow
- behavior-changing tiers
- progression state stored outside shop logic

### Phase 5. Add combo and ultimate systems

Deliverables:

- combo tags on spells
- elemental interaction resolver
- ultimate charge from damage, assists and objectives
- one ultimate branch per build identity

### Phase 6. Extract the old CS-like mode into its own rules package

Deliverables:

- current Horcrux mode remains playable as a legacy mode
- build-driven core becomes mode-agnostic
- future modes can share the same wizard-build systems

Output expectation:

- creating a new mode no longer requires editing spell, pawn or mana core logic
- legacy CS-like systems can be removed without damaging the new core

## Concrete File-Level Recommendations

### Files to refactor first

- `code/PawnSystem/Player/SpellsDeck.cs`
- `code/Spells/SpellCatalog.cs`
- `code/PawnSystem/Player/Wand.cs`
- `code/PawnSystem/Player/PlayerController/PlayerPawn.OldMethods.cs`
- `code/UI/HUD/BuyMenu.razor`

### Files that should become mode-specific instead of core

- `code/GameMode/RoundManager.cs`
- `code/GameLoop/BuyZone.cs`
- `code/GameLoop/Rules/Equipment/Money.cs`
- Horcrux objective files

## Proposed First Milestone

If we want the safest first implementation step, the best milestone is:

"replace money-based deck ownership with a build-based loadout component while keeping the current combat and round loop alive."

That gives us:

- minimal gameplay breakage
- fast validation of the new concept
- a stable base for affinity, discipline and passive systems
- the first real decoupling between core and mode rules

## Important Warning

Do not try to refactor everything in one pass.

If we rewrite:

- round logic
- objective logic
- deck logic
- spell metadata
- UI
- balance systems

at the same time, the project will become hard to debug.

The safest strategy is:

1. replace player build data model
2. replace loadout rules
3. inject build modifiers into combat
4. only then extract or rewrite mode rules

## Removal Policy For Legacy CS-Like Systems

After the new core is functional, any remaining system that exists only because of the old CS-style foundation should be reviewed with one question:

"Does this support Warlocks as a modular wizard-combat platform, or is it leftover structure from the previous design?"

If the answer is "leftover structure", it should be:

1. isolated
2. deprecated
3. removed

This applies especially to:

- economy-first loadout logic
- buy-zone dependencies
- hardcoded attack/defend round assumptions
- CS-derived terminology inside shared systems
- objective code embedded into general match flow

## Suggested Next Implementation Task

Start with a new `PlayerBuildComponent` and a new build-oriented `SpellCatalog` schema, while keeping existing spell execution intact.

That is the cleanest bridge between the current game and the target concept.

After that, introduce an `IMatchRule`-style rule composition layer so each new game type plugs into core instead of modifying it.
