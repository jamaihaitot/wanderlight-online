# Feature Specification: Wanderlight Online — MMORPG foundation (2D, browser-first, v1 auth = unique display name)

**Feature Branch**: `001-wanderlight-online-mmorpg`  
**Created**: September 14, 2025  
**Status**: Draft  
**Input**: User description: "Wanderlight Online — MMORPG foundation (2D, browser-first, v1 auth = unique display name)

Goal

- Create a minimal, scalable online RPG foundation enabling fast gameplay iteration without infra friction.
- Maintain a single authoritative game state with smooth real-time sync and basic persistence.

Scope (v1)

- 2D single-zone world; players join/leave and spawn at a defined location.
- Real-time player movement with collision and visibility of nearby players (area-of-interest).
- Basic interactions: pick up/drop world items visible to all.
- Simple inventory: view/add/remove; stacking rules; persistence across sessions.
- World synchronization: join snapshot + incremental deltas.
- Disconnection/reconnection with state restoration.
- Lightweight auth: guest join with unique display name only (no tokens/accounts). If name is taken, choose another.

Out of Scope (v1)

- Combat, NPCs, quests, trading, chat, friends/clans, crafting, complex UI, anti-cheat, monetization, multi-zone streaming.

Primary Users

- Player: join world, move, see others, pick up/drop items, view inventory.
- Operator: observe basic telemetry and logs.

User Stories

- As a player, I enter a display name and join the world at a known spawn.
- As a player, I move and see other players moving within proximity.
- As a player, I pick up nearby items (capacity permitting) and they disappear from the world.
- As a player, I drop items and they appear in the world for others.
- As a player, when I disconnect and rejoin, my inventory and last position are restored.

Acceptance Criteria

- Display names are unique among connected players; taken names are rejected with a clear message.
- Optional name reservation on disconnect for a short grace period to allow quick reconnect; after timeout, name becomes available.
- Join snapshot ≤2s p95 in a desktop browser; incremental updates ≤150ms p95 end-to-end.
- Movement updates apply in order; no dupes/out-of-order client state.
- Pick up/drop are atomic under contention; no dupes or lost items.
- Inventory capacity/stacking enforced server-side.
- Reconnect restores last persisted state ≤2s p95.
- Supports ≥50 concurrent players in one zone with ≤2% missed update frames.

Constraints and Assumptions

- 2D, top-down; placeholder art/UX acceptable.
- Browser-first (desktop), architecture portable to mobile and desktop clients later.
- Authoritative server; clients are non-authoritative.
- Persistence for player position and inventory; simplified world item respawn rules acceptable.
- Minimal security for v1; display name only, no PII.

Non-Functional Requirements

- Observability: metrics (connected players, join time, update latency p50/p95, errors) and structured logs.
- Stability: graceful disconnects; idempotent reconnect.
- Performance: render loop remains smooth; network loop does not block frames.

Risks

- State divergence under jitter; periodic reconciliation/snapshots on anomalies.
- Contention on world items/inventory; require server-side transactions/locks.

Decisions (v1 MVP)

- Movement model
  - 2D top-down, free movement (no grid).
  - Speed: 3.5 units/s; Accel: 25 u/s²; decel via friction.
  - Client sends input intents at 20 Hz; server simulates authoritative movement at 20 Hz.
  - Client interpolates to 60 FPS; server clamps speed and resolves tilemap collisions.
- Area-of-interest (AoI) and updates
  - AoI radius: 25 tiles (~800 px) around the player; hard cap 100 visible entities.
  - Interest grid: 32×32 tile cells; recompute AoI on move or every 250 ms (whichever first).
  - Server sends state deltas at 20 Hz; full join snapshot on connect; mini-snapshot every 2 s for correction.
- Inventory model
  - 12 slots; slots-based; no weight.
  - Stack sizes: 20 for consumables/materials; 1 for equipment.
  - Categories: generic, consumable, equipment (non-stack).
  - Server enforces capacity and stacking rules.
- Persistence cadence
  - Inventory: persist on change (atomic).
  - Player position: persist every 5 s and on disconnect.
  - World items: persist on change (spawn, pickup, drop).
  - Join loads last known position/inventory from persistence.
- World item spawn/despawn rules
  - Static placements defined in the map; no respawn in v1 (deterministic).
  - Dropped items persist in-world until picked up or server restart.
  - Operator command to reset world state during testing.
- Name policy (unique display name only)
  - Uniqueness: case-insensitive among connected players.
  - Format: 3–16 chars, [A–Z a–z 0–9 _]; trim whitespace.
  - Profanity: basic denylist filter.
  - Reservation: reserve name for 2 minutes after disconnect for quick reconnect.
- Reconnect behavior
  - Keep player entity “ghosted” up to 30 s after disconnect; rejoin with same name restores seamlessly.
  - After 30 s, despawn entity; state remains persisted.
- Performance/timing targets
  - Server tick: 20 Hz authoritative sim; network updates at 20 Hz.
  - Client render: 60 FPS target with interpolation.
  - Snapshots: initial join ≤2 s p95; incremental updates ≤150 ms p95.

Success Metrics

- Time-to-first-playable < 1 week.
- 50 CCU stable for 30 minutes with p95 update latency ≤150ms and zero data corruption incidents."

## Execution Flow (main)

```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
5. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
6. Identify Key Entities (if data involved)
7. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
8. Return: SUCCESS (spec ready for planning)
```

---

## User Scenarios & Testing

### Primary User Story

A player enters a unique display name and joins a 2D world at a known spawn point. The player can move freely, see other players within proximity, pick up and drop items, and view/manage their inventory. If disconnected, the player can rejoin and have their last position and inventory restored. Operators can observe basic telemetry and logs.

### Acceptance Scenarios

1. **Given** a player enters a display name, **When** the name is unique, **Then** the player joins the world at the spawn point.
2. **Given** a player is in the world, **When** they move, **Then** other nearby players see their movement in real time.
3. **Given** a player is near an item, **When** they pick it up (inventory capacity permitting), **Then** the item disappears from the world and appears in their inventory.
4. **Given** a player has an item in inventory, **When** they drop it, **Then** the item appears in the world for others to pick up.
5. **Given** a player disconnects, **When** they rejoin within the grace period, **Then** their last position and inventory are restored.
6. **Given** a display name is already taken, **When** a player tries to join, **Then** they receive a clear rejection message.

### Edge Cases

- What happens if two players attempt to pick up the same item simultaneously? (Atomicity required)
- How does the system handle network jitter or out-of-order movement updates?
- What if a player disconnects and does not rejoin within the reservation period?
- How are profanity or invalid display names handled?
- What if inventory is full when picking up an item?
- How does the system handle more than 50 concurrent players?

## Requirements

### Functional Requirements

- **FR-001**: System MUST allow players to join the world with a unique display name (case-insensitive).
- **FR-002**: System MUST reject duplicate or invalid display names with a clear message.
- **FR-003**: System MUST spawn players at a defined location upon joining.
- **FR-004**: System MUST allow real-time movement and visibility of nearby players (area-of-interest).
- **FR-005**: System MUST support picking up and dropping world items, visible to all players.
- **FR-006**: System MUST enforce inventory capacity and stacking rules server-side.
- **FR-007**: System MUST persist player position and inventory across sessions.
- **FR-008**: System MUST restore player state on reconnect within the grace period.
- **FR-009**: System MUST synchronize world state via join snapshot and incremental deltas.
- **FR-010**: System MUST ensure atomicity for pick up/drop actions under contention.
- **FR-011**: System MUST provide basic telemetry and logs for operators.
- **FR-012**: System MUST enforce display name format and profanity filtering.
- **FR-013**: System MUST reserve display names for 2 minutes after disconnect for quick reconnect.
- **FR-014**: System MUST despawn player entity after 30 seconds of disconnect, but persist state.
- **FR-015**: System MUST support ≥50 concurrent players in one zone with ≤2% missed update frames.
- **FR-016**: System MUST meet performance targets: join snapshot ≤2s p95, incremental updates ≤150ms p95.
- **FR-017**: System MUST allow operator to reset world state during testing.
- **FR-018**: System MUST provide observability metrics: connected players, join time, update latency, errors.
- **FR-019**: System MUST handle graceful disconnects and idempotent reconnects.
- **FR-020**: System MUST avoid implementation details in specification. [NEEDS CLARIFICATION: Is there a preferred format for operator logs/metrics?]

### Key Entities

- **Player**: Unique display name, position, inventory, connection state, reservation status.
- **World Item**: Type, position, stack size, owner (if picked up), persistence status.
- **Inventory**: Slots, stack rules, categories (generic, consumable, equipment).
- **Operator**: Access to telemetry, logs, and world reset command.

---

## Review & Acceptance Checklist

### Content Quality

- [ ] No implementation details (languages, frameworks, APIs)
- [ ] Focused on user value and business needs
- [ ] Written for non-technical stakeholders
- [ ] All mandatory sections completed

### Requirement Completeness

- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous
- [ ] Success criteria are measurable
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified

---

## Execution Status

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---
