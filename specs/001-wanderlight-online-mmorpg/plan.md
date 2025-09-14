# Implementation Plan: Wanderlight Online — MMORPG foundation (2D, browser-first, v1 auth = unique display name)

**Branch**: `001-wanderlight-online-mmorpg` | **Date**: September 14, 2025 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-wanderlight-online-mmorpg/spec.md`

## Execution Flow (/plan command scope)

```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
4. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
5. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, or `GEMINI.md` for Gemini CLI).
6. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
7. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
8. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:

- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary

Wanderlight Online aims to deliver a minimal, scalable 2D MMORPG foundation for browser-first play, with unique display name authentication, real-time movement, inventory, and world sync. The technical approach leverages Godot (C#), SpacetimeDB, and WebSocket networking for authoritative state and fast iteration.

## Technical Context

**Language/Version**: C# (Godot 4.x), PowerShell, SpacetimeDB (cloud)
**Primary Dependencies**: Godot Engine, Mono/C#, SpacetimeDB, WebSocket
**Storage**: SpacetimeDB (player/inventory/world state)
**Testing**: Godot C# unit test framework (NEEDS CLARIFICATION: preferred test runner for Godot C#?)
**Target Platform**: Browser (HTML5 export), Windows/Linux dev
**Project Type**: Single project (Godot game with C# scripts)
**Performance Goals**: 60 FPS client, ≤150ms p95 update latency, ≤2s p95 join snapshot
**Constraints**: ≤2% missed update frames, 50+ CCU, atomic item actions, unique display names
**Scale/Scope**: 1 zone, 50+ concurrent players, 12 inventory slots, 100 visible entities

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

**Simplicity**:

- Projects: 1 (Godot game)
- Using framework directly? Yes (Godot, C#)
- Single data model? Yes (player, inventory, world item)
- Avoiding patterns? Yes (no unnecessary abstraction)

**Architecture**:

- EVERY feature as library? Game logic as C# classes, testable
- Libraries listed: PlayerManager, DatabaseManager, Inventory, NetworkManager
- CLI per library: (NEEDS CLARIFICATION: CLI for game logic?)
- Library docs: llms.txt format planned

**Testing (NON-NEGOTIABLE)**:

- RED-GREEN-Refactor cycle enforced? Yes
- Git commits show tests before implementation? (NEEDS CLARIFICATION: enforceable in Godot workflow?)
- Order: Contract→Integration→E2E→Unit strictly followed? Yes
- Real dependencies used? Yes (SpacetimeDB)
- Integration tests for: DatabaseManager, NetworkManager
- FORBIDDEN: Implementation before test, skipping RED phase

**Observability**:

- Structured logging included? Yes
- Frontend logs → backend? Yes (telemetry/logs to server)
- Error context sufficient? Yes

**Versioning**:

- Version number assigned? (NEEDS CLARIFICATION: versioning for Godot project?)
- BUILD increments on every change? Yes
- Breaking changes handled? Yes (migration plan for DB)

## Project Structure

- Godot project root
  - Scripts/
    - PlayerManager.cs
    - DatabaseManager.cs
    - Inventory.cs
    - NetworkManager.cs
    - PlayerController.cs
  - Scenes/
  - Assets/
  - tests/
  - README.md

## Progress Tracking

- [x] Initial Constitution Check
- [x] Phase 0: research.md
- [x] Phase 1: data-model.md, contracts/, quickstart.md, .github/copilot-instructions.md
- [ ] Phase 2: tasks.md (planned)
- [x] Post-Design Constitution Check

---
