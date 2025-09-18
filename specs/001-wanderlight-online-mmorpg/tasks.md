# Tasks: Wanderlight Online — MMORPG foundation (2D, browser-first, v1 auth = unique display name)

**Input**: Design documents from `/specs/001-wanderlight-online-mmorpg/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)

```
1. Load plan.md from feature directory
2. Load optional design documents: data-model.md, contracts/, research.md, quickstart.md
3. Generate tasks by category: setup, tests, core, integration, polish
4. Apply task rules: parallel/sequential, TDD order
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness
9. Return: SUCCESS (tasks ready for execution)
```

## Phase 3.1: Setup

- [x] T001 Create Godot project structure (`Wanderlight Online`) with folders: Scripts/, Scenes/, Assets/
- [x] T002 Initialize Godot C# project and install Mono dependencies
- [x] T003 [P] Set up SpacetimeDB instance and obtain connection details
- [x] T004 [P] Configure linting and formatting tools for C#

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

- [x] T005 [P] Contract test for PlayerManager in `tests/contract/test_player_manager.cs`
- [x] T006 [P] Contract test for Inventory in `tests/contract/test_inventory.cs`
- [x] T007 [P] Contract test for NetworkManager in `tests/contract/test_network_manager.cs`
- [x] T008 [P] Contract test for Operator telemetry/logs in `tests/contract/test_operator.cs`
- [x] T009 [P] Model test for Player entity in `tests/model/test_player.cs`
- [x] T010 [P] Model test for Inventory entity in `tests/model/test_inventory.cs`
- [x] T011 [P] Model test for WorldItem entity in `tests/model/test_worlditem.cs`
- [x] T012 [P] Model test for Operator entity in `tests/model/test_operator.cs`
- [x] T013 [P] Integration test for player join/auth flow in `tests/integration/test_player_join.cs`
- [x] T014 [P] Integration test for inventory persistence in `tests/integration/test_inventory_persistence.cs`
- [x] T015 [P] Integration test for real-time movement in `tests/integration/test_movement.cs`
- [x] T016 [P] Integration test for item pick up/drop atomicity in `tests/integration/test_item_atomicity.cs`
- [x] T017 [P] Integration test for operator world reset in `tests/integration/test_world_reset.cs`

## Phase 3.3: Core Implementation (ONLY after tests are failing)

- [ ] T018 Implement PlayerManager class in `Scripts/PlayerManager.cs`
- [ ] T019 Implement Inventory class in `Scripts/Inventory.cs`
- [ ] T020 Implement NetworkManager class in `Scripts/NetworkManager.cs`
- [ ] T021 Implement Operator telemetry/logs in `Scripts/Operator.cs`
- [ ] T022 Implement WorldItem logic in `Scripts/WorldItem.cs`
- [ ] T023 Implement DatabaseManager class in `Scripts/DatabaseManager.cs`
- [ ] T024 Implement PlayerController class in `Scripts/PlayerController.cs`

## Phase 3.4: Integration

- [ ] T025 Integrate SpacetimeDB with DatabaseManager
- [ ] T026 Integrate WebSocket networking in NetworkManager
- [ ] T027 Integrate structured logging and telemetry for Operator
- [ ] T028 Integrate inventory persistence and atomic actions
- [ ] T029 Integrate player state restoration on reconnect

## Phase 3.5: Polish

- [ ] T030 [P] Unit tests for all core classes in `tests/unit/`
- [ ] T031 [P] Performance tests for movement, sync, and join latency
- [ ] T032 [P] Update documentation in `README.md` and design notes
- [ ] T033 [P] Manual playtesting and feedback iteration

## Parallel Execution Examples

- T003, T004 can run in parallel (setup)
- T005–T017 can run in parallel (tests for different files)
- T030–T033 can run in parallel (polish)

## Dependencies

- Setup (T001–T004) before tests
- Tests (T005–T017) before core implementation (T018–T024)
- Core implementation before integration (T025–T029)
- Integration before polish (T030–T033)

---
