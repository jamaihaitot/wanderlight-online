# Implementation Plan: Browser Playable Multiplayer Prototype (Phase 4.0)

**Branch**: `002-description-build-a` | **Date**: October 8, 2025 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `specs/002-description-build-a/spec.md`

## Execution Flow (/plan command scope)

```
1. ✅ Load feature spec from Input path
   → Feature spec loaded successfully
2. ✅ Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Project Type: Godot game engine project with C# (existing backend complete)
   → Structure Decision: Godot project structure (Scenes/, Scripts/, Assets/)
   → Minor clarifications: Display name length, inventory full message
3. ✅ Evaluate Constitution Check section below
   → Constitution compliance documented
   → Backend already follows library-first (Phase 3 complete)
   → Phase 4 focuses on UI layer integration
   → Update Progress Tracking: Initial Constitution Check
4. 🔄 Execute Phase 0 → research.md
5. ⏸️ Execute Phase 1 → contracts, data-model.md, quickstart.md, .github/copilot-instructions.md
6. ⏸️ Re-evaluate Constitution Check section
7. ⏸️ Plan Phase 2 → Describe task generation approach
8. ⏸️ STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:

- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

---

## Summary

**Primary Requirement**: Build a browser-playable multiplayer game client that connects players to an existing, tested backend system. Players can join with unique names, move in a 2D world, see other players in real-time, pick up and drop items, and manage a 12-slot inventory.

**Technical Approach (from user input)**:

- **Engine**: Godot 4.x with C# scripting for scene and UI management
- **Backend Integration**: Wire Godot UI to existing C# classes (PlayerManager, Inventory, NetworkManager, etc.)
- **Real-Time Sync**: SpacetimeDB subscriptions for player/item updates at 20Hz, interpolated to 60 FPS
- **Architecture**: Scene-based hierarchy (MainMenu → GameWorld) with component-based entities
- **Testing**: Manual testing guide with 8 scenarios, validated in multiple browser tabs
- **Deployment**: HTML5 export for modern browsers (Chrome, Firefox, Edge)

---

## Technical Context

**Language/Version**: C# 11 / .NET 7.0 (Godot 4.x compatible)  
**Primary Dependencies**:

- Godot 4.x game engine (C# scripting)
- SpacetimeDB C# client SDK (already integrated in Phase 3)
- Existing backend classes: `PlayerManager`, `DatabaseManager`, `Inventory`, `NetworkManager`, `PlayerController`, `Operator`, `WorldItem`

**Storage**: SpacetimeDB (real-time relational database) - backend connection established  
**Testing**:

- Manual testing guide (8 scenarios defined)
- 130+ automated backend tests (xUnit) - already passing
- Browser-based multiplayer testing (multiple tabs)

**Target Platform**:

- Primary: HTML5 export for modern browsers (Chrome, Firefox, Edge)
- Development: Godot editor with C# debugger
- Testing: Localhost SpacetimeDB + browser instances

**Project Type**: Godot game engine project with custom structure (Scenes/, Scripts/, Assets/)

**Performance Goals**:

- 60 FPS stable frame rate on client
- <200ms p95 latency for player actions
- 20Hz network update rate from SpacetimeDB
- Smooth interpolation for 20Hz → 60FPS rendering

**Constraints**:

- Browser-only deployment (no native downloads)
- 12 inventory slots per player (hard limit from Phase 3 spec)
- 100 max visible entities for performance
- Session-based identity (no persistent accounts/passwords)
- Simple placeholder graphics (MVP focused on functionality)

**Scale/Scope**:

- 50+ concurrent players per game instance
- 8 primary user scenarios
- 44 functional requirements + 8 performance requirements
- Single game world (no server selection/rooms)

---

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Simplicity

- **Projects**: 1 (Godot project - client UI layer only)
  - Backend already exists as separate tested system (Phase 3 complete)
  - This phase adds Godot scenes and client-side scripts only
  - ✅ Complies with "≤3 projects" principle
- **Using framework directly?**: ✅ YES
  - Using Godot's built-in nodes (CharacterBody2D, Control, Panel, etc.) without wrappers
  - Using Godot's signal system for events (no custom event bus)
  - Using SpacetimeDB SDK directly (no abstraction layer)
- **Single data model?**: ✅ YES
  - Backend entities (Player, Inventory, WorldItem) already defined in Phase 3
  - Client scripts use same data structures from backend classes
  - No DTOs or transformations - direct binding to SpacetimeDB tables
- **Avoiding patterns?**: ✅ YES
  - No Repository pattern (DatabaseManager already handles this)
  - No Service layer abstraction (calling backend classes directly)
  - No dependency injection container (Godot autoload for singletons)
  - Using Godot's built-in scene system, not custom component framework

### Architecture

- **EVERY feature as library?**: ⚠️ ADAPTED FOR GAME ENGINE
  - Backend (Phase 3) follows library-first: PlayerManager, Inventory, etc. are reusable C# classes
  - Phase 4 (client UI) is Godot-specific: scenes (.tscn) and UI scripts
  - Godot scenes are inherently modular and reusable (Player.tscn, WorldItem.tscn, etc.)
  - **Justification**: Game engines use scene-based architecture, not traditional library pattern
- **Libraries listed**:
  - Backend libraries (already complete): `PlayerManager`, `DatabaseManager`, `Inventory`, `NetworkManager`, `PlayerController`, `Operator`, `WorldItem`
  - Client components (new): Scene prefabs (Player.tscn, WorldItem.tscn, InventoryPanel.tscn) + controller scripts
- **CLI per library**: ⚠️ NOT APPLICABLE
  - Backend classes tested via xUnit (130+ tests), not CLI
  - Client scenes tested via manual testing guide (8 scenarios)
  - **Justification**: Game engines don't expose CLI interfaces; testing via editor and runtime
- **Library docs**: ✅ YES
  - Backend already documented with XML comments
  - Client scripts will have XML doc comments
  - Manual testing guide serves as integration documentation

### Testing (NON-NEGOTIABLE)

- **RED-GREEN-Refactor cycle enforced?**: ⚠️ ADAPTED FOR PHASE 4
  - **Phase 3 Backend**: Strict TDD with 130+ tests (RED → GREEN → Refactor) ✅
  - **Phase 4 Client UI**: Manual testing guide with 8 scenarios
  - **Why adapted**: Godot UI/scene testing requires visual validation, not traditional unit tests
  - **Validation**: Manual testing scenarios written BEFORE implementation (test-first spirit)
- **Git commits show tests before implementation?**:
  - Phase 3: YES (backend test files committed before implementation)
  - Phase 4: Manual testing guide already exists from Phase 3
- **Order: Contract→Integration→E2E→Unit strictly followed?**: ⚠️ ADAPTED
  - Backend (Phase 3): Contract tests → Integration tests → Unit tests ✅
  - Client (Phase 4): Manual scenarios (E2E-equivalent) → Scene validation
  - **Justification**: UI layer validated through user scenarios, not traditional test pyramid
- **Real dependencies used?**: ✅ YES
  - Testing with actual SpacetimeDB server (not mocks)
  - Testing with real WebSocket connections
  - Multiple browser instances for true multiplayer testing
- **Integration tests for**: ✅ YES
  - UI → Backend integration: Manual scenarios validate MainMenuController calls PlayerManager.TryAddPlayer()
  - Client → SpacetimeDB: Scenarios validate subscription updates
  - Network sync: Multi-browser tests validate real-time synchronization
- **FORBIDDEN: Implementation before test**: ✅ COMPLIANT
  - Manual testing guide exists before Phase 4 implementation begins
  - Each scenario serves as acceptance test for implementation

### Observability

- **Structured logging included?**: ✅ YES
  - Backend already has structured logging (Phase 3)
  - Client scripts will log: Join events, Movement, Pickup/Drop, Inventory changes
  - Log format: JSON-structured for operational monitoring
- **Frontend logs → backend?**: ⚠️ NOT IN MVP
  - Client logs to browser console (development)
  - Backend logs to server-side structured output
  - **Future enhancement**: Aggregate client logs via telemetry endpoint
- **Error context sufficient?**: ✅ YES
  - Client displays user-friendly error messages (duplicate name, connection lost, etc.)
  - Backend logs include stack traces and context
  - Network errors include latency and retry information

### Versioning

- **Version number assigned?**: ✅ YES
  - Phase 4.0 (MAJOR=4, MINOR=0, BUILD=incremental)
  - Version tracked in project files and documentation
- **BUILD increments on every change?**: ✅ YES
  - Following semantic versioning
  - Each commit increments build number
- **Breaking changes handled?**: ✅ YES
  - Backend API stable (Phase 3 complete)
  - Client changes won't break backend (client consumes, doesn't modify)
  - If backend changes needed: Parallel tests + migration plan

---

## Constitution Compliance Summary

| Principle            | Status       | Notes                                                              |
| -------------------- | ------------ | ------------------------------------------------------------------ |
| Library-First        | ✅ Adapted   | Backend follows strictly; client uses Godot scene architecture     |
| Specification-Driven | ✅ Compliant | This plan derives from approved specification                      |
| Test-First           | ✅ Adapted   | Backend has 130+ unit tests; client has manual test scenarios      |
| Simplicity           | ✅ Compliant | 1 project (Godot), no abstraction layers, using framework directly |
| Safety-First         | ✅ Compliant | Backend at 95%+ coverage; client validated through manual testing  |

**Adaptations Justified**: Game engine development (Godot) requires scene-based architecture and visual testing, which differs from traditional library + CLI pattern. Core principles (test-first, simplicity, no abstraction) are maintained within game engine paradigm.

---

## Project Structure

### Documentation (this feature)

```
specs/002-description-build-a/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (Godot + SpacetimeDB patterns)
├── data-model.md        # Phase 1 output (client-side entities)
├── quickstart.md        # Phase 1 output (setup & testing guide)
├── contracts/           # Phase 1 output (UI-backend integration contracts)
│   ├── login-contract.md
│   ├── movement-contract.md
│   ├── inventory-contract.md
│   └── item-interaction-contract.md
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (Godot project structure)

```
wanderlight-online/
├── Scenes/                         # NEW - Phase 4.0
│   ├── MainMenu.tscn               # Login screen scene
│   ├── GameWorld.tscn              # Main game scene
│   ├── Player.tscn                 # Local player prefab
│   ├── RemotePlayer.tscn           # Remote player instance prefab
│   ├── WorldItem.tscn              # Ground item prefab
│   └── UI/
│       ├── InventoryPanel.tscn     # Inventory UI
│       └── HUD.tscn                # Connection status, etc.
│
├── Scripts/                        # Backend (Phase 3) + Client (Phase 4)
│   ├── PlayerManager.cs            # EXISTING - Backend
│   ├── DatabaseManager.cs          # EXISTING - Backend
│   ├── Inventory.cs                # EXISTING - Backend
│   ├── NetworkManager.cs           # EXISTING - Backend
│   ├── PlayerController.cs         # EXISTING - Backend
│   ├── Operator.cs                 # EXISTING - Backend
│   ├── WorldItem.cs                # EXISTING - Backend
│   ├── SpacetimeBindings/          # EXISTING - Generated bindings
│   └── Client/                     # NEW - Phase 4.0
│       ├── MainMenuController.cs   # Login UI controller
│       ├── GameManager.cs          # Main game coordinator
│       ├── LocalPlayerController.cs # Local player input handler
│       ├── RemotePlayerRenderer.cs # Remote player sync
│       ├── WorldItemRenderer.cs    # World item sync
│       └── InventoryUI.cs          # Inventory panel controller
│
├── Assets/                         # NEW - Phase 4.0
│   ├── Sprites/
│   │   ├── player.png              # Simple player sprite
│   │   ├── item_potion.png         # Item icons
│   │   └── world_tileset.png       # Ground tiles
│   └── Fonts/
│       └── default_font.ttf        # Display name labels
│
├── Tests/                          # EXISTING - Phase 3
│   ├── Contract/                   # Backend contract tests
│   ├── Integration/                # Backend integration tests
│   └── Unit/                       # Backend unit tests
│
├── MANUAL_TESTING_GUIDE.md         # EXISTING - Phase 3
├── CONSTITUTION.md                 # NEW - Phase 4.0 principles
├── project.godot                   # Godot project configuration
└── Wanderlight Online.csproj       # C# project file
```

**Structure Decision**: Godot project structure (specialized game engine pattern)

- **Rationale**: Godot enforces Scenes/ + Scripts/ organization
- **Deviation from standard**: Game engines don't follow src/tests library pattern
- **Compliance**: Maintains simplicity, no abstraction, test-first principles within Godot paradigm

---

## Phase 0: Outline & Research

### Unknowns to Resolve

From Technical Context analysis:

1. **Display Name Validation** [NEEDS CLARIFICATION from spec]

   - Maximum length not specified
   - Allowed characters not specified
   - **Research Task**: Best practices for display names in multiplayer games

2. **Inventory Full Feedback** [NEEDS CLARIFICATION from spec]

   - Should system show "Inventory full" message?
   - **Research Task**: UX patterns for inventory feedback

3. **Godot HTML5 Export**

   - Optimization settings for browser performance
   - WebSocket compatibility in HTML5 builds
   - **Research Task**: Godot 4.x HTML5 export best practices

4. **SpacetimeDB Client Subscriptions in Godot**

   - Integration patterns for reactive updates
   - Threading model (SpacetimeDB callbacks → Godot main thread)
   - **Research Task**: SpacetimeDB C# SDK with Godot integration

5. **Input Handling in Godot C#**

   - Keyboard input patterns for CharacterBody2D
   - Input mapping for browser (keyboard focus handling)
   - **Research Task**: Godot input system for browser builds

6. **Interpolation Strategy**
   - 20Hz network updates → 60 FPS smooth rendering
   - Position interpolation algorithms
   - **Research Task**: Client-side prediction and interpolation patterns

### Research Agent Dispatch

```
Agent 1: Godot HTML5 Export Optimization
  - Query: "Godot 4.x HTML5 export best practices for multiplayer game performance"
  - Focus: WebSocket compatibility, threading, memory optimization
  - Output: Export settings, threading model, compression options

Agent 2: SpacetimeDB + Godot Integration
  - Query: "SpacetimeDB C# client SDK integration with Godot 4.x"
  - Focus: Subscription callbacks, thread safety, scene updates
  - Output: Integration pattern, code examples, threading considerations

Agent 3: Godot Input Handling for Browser
  - Query: "Godot C# keyboard input handling for HTML5 browser builds"
  - Focus: Input.IsActionPressed() vs _UnhandledInput, browser focus
  - Output: Best practices, code patterns, browser considerations

Agent 4: Client-Side Interpolation
  - Query: "Position interpolation for multiplayer games 20Hz to 60FPS"
  - Focus: Linear interpolation, extrapolation, lag compensation
  - Output: Algorithm, code example, smoothing strategies

Agent 5: Multiplayer UX Patterns
  - Query: "Display name validation and inventory UI feedback best practices"
  - Focus: Name length limits, character restrictions, full inventory messages
  - Output: Industry standards, UX recommendations
```

### Research Consolidation Format

**Output**: `research.md` will contain:

- **Decision**: [Technology/pattern chosen]
- **Rationale**: [Why this approach fits Phase 4.0 requirements]
- **Alternatives Considered**: [Other options and why rejected]
- **Code Examples**: [Patterns to follow in implementation]
- **References**: [Godot docs, SpacetimeDB docs, community resources]

**Deliverable**: research.md with all NEEDS CLARIFICATION resolved

---

## Phase 1: Design & Contracts

_Prerequisites: research.md complete_

### 1. Extract Entities → `data-model.md`

**Client-Side Entities** (UI layer only - backend entities already defined in Phase 3):

#### LocalPlayer (Scene Component)

- **Purpose**: Represents the player's own character with input handling
- **Godot Type**: CharacterBody2D with attached script
- **Fields**:
  - `DisplayName`: string (from login)
  - `Position`: Vector2 (synchronized with backend)
  - `Velocity`: Vector2 (for movement)
  - `Inventory`: Reference to backend Inventory class
- **State Transitions**:
  - Login → Spawned → Moving → Picking Up Item → Inventory Open → Disconnected

#### RemotePlayer (Scene Component)

- **Purpose**: Visual representation of other players
- **Godot Type**: CharacterBody2D with interpolation script
- **Fields**:
  - `PlayerId`: ulong (from SpacetimeDB)
  - `DisplayName`: string
  - `TargetPosition`: Vector2 (latest from network)
  - `CurrentPosition`: Vector2 (interpolated)
- **Relationships**:
  - Subscribed to SpacetimeDB `player` table
  - Managed by RemotePlayerRenderer

#### WorldItemInstance (Scene Component)

- **Purpose**: Visual representation of items on ground
- **Godot Type**: Node2D with Area2D for pickup detection
- **Fields**:
  - `ItemId`: uint (from WorldItem backend class)
  - `ItemType`: ItemCategory enum
  - `Position`: Vector2
  - `Quantity`: uint
- **Relationships**:
  - Bound to backend WorldItem entity
  - Subscribed to SpacetimeDB `world_item` table

#### InventorySlotUI (UI Component)

- **Purpose**: Single slot in inventory grid
- **Godot Type**: Button with TextureRect + Label
- **Fields**:
  - `SlotIndex`: int (0-11)
  - `ItemType`: ItemCategory (nullable)
  - `Quantity`: uint
- **Relationships**:
  - Bound to backend Inventory.Slots array
  - Parent: InventoryPanel

**Validation Rules**:

- Display name: 1-20 characters, alphanumeric + spaces (from research)
- Position: Within world bounds (validated by backend)
- Inventory: Max 12 slots (enforced by backend)

**State Transitions**:

- Player: Connecting → Login Screen → In World → Playing → Disconnected
- Item: Spawned → Visible → Near Player → Picked Up
- Inventory: Closed → Open → Item Selected → Item Dropped → Closed

### 2. Generate API Contracts → `/contracts/`

**Contract Files** (UI ↔ Backend integration points):

#### `login-contract.md`

```
Interface: MainMenuController ↔ PlayerManager
Methods:
  - TryAddPlayer(displayName: string) → Result<PlayerId, ErrorMessage>
  - OnPlayerAdded event → (PlayerId, Position)
  - OnPlayerAddFailed event → (ErrorMessage)

Validation:
  - Display name: 1-20 chars, alphanumeric + spaces
  - Unique across active players

Error Cases:
  - Duplicate name → "Name already taken"
  - Invalid characters → "Name contains invalid characters"
  - Too long/short → "Name must be 1-20 characters"
```

#### `movement-contract.md`

```
Interface: LocalPlayerController ↔ PlayerController + NetworkManager
Methods:
  - TryMove(direction: Vector2) → Result<NewPosition, Error>
  - NetworkManager.SendMovement(position: Vector2)
  - OnPlayerMoved event → (PlayerId, Position)

Update Rate: 20Hz (50ms intervals)
Validation:
  - Collision detection via Godot physics
  - Position sent to backend on change

Error Cases:
  - Network timeout → Show "Connection lost"
  - Invalid position → Rollback to last known good
```

#### `inventory-contract.md`

```
Interface: InventoryUI ↔ Inventory + DatabaseManager
Methods:
  - Inventory.AddItem(itemType, quantity) → Result<Success, InventoryFull>
  - Inventory.RemoveItem(slotIndex) → Result<ItemStack, EmptySlot>
  - Inventory.GetSlots() → ItemStack[12]
  - DatabaseManager.SaveInventory()

Events:
  - OnInventoryChanged → (updatedSlots)

Error Cases:
  - Inventory full → Show "Inventory full" tooltip
  - Empty slot clicked → No action
```

#### `item-interaction-contract.md`

```
Interface: WorldItemRenderer + LocalPlayerController ↔ WorldItem + DatabaseManager
Methods:
  - WorldItem.TryPickup(playerId, itemId) → Result<Success, AlreadyTaken>
  - WorldItem.Drop(itemType, quantity, position) → Result<ItemId, Error>
  - OnItemSpawned event → (ItemId, ItemType, Position, Quantity)
  - OnItemRemoved event → (ItemId)

Validation:
  - Pickup range: <50 pixels from item
  - Atomic operation (only one player succeeds)

Error Cases:
  - Item already taken → Silently fail (another player got it)
  - Network delay → Backend authoritative, client syncs
```

### 3. Generate Contract Tests

**Test Files** (Manual testing scenarios - automated tests not applicable for UI):

Since this is a Godot UI layer, contract tests are defined as **manual test scenarios** in the existing `MANUAL_TESTING_GUIDE.md`. These scenarios serve as acceptance tests:

1. **Login Contract Test**:
   - Scenario 1 & 2 from spec (join success, duplicate name)
2. **Movement Contract Test**:
   - Scenario 3 & 4 from spec (player movement, multi-player sync)
3. **Inventory Contract Test**:
   - Scenario 7 from spec (open/close, display items)
4. **Item Interaction Contract Test**:
   - Scenario 5 & 6 from spec (pickup, drop, sync)

**Note**: These tests MUST fail initially (no UI implementation exists yet), maintaining test-first principle.

### 4. Extract Test Scenarios → `quickstart.md`

**Quickstart Test** = Minimal validation that all systems work:

```
Prerequisites:
  1. SpacetimeDB server running (spacetime start)
  2. Godot project built and exported to HTML5
  3. Two browser tabs open to game URL

Test Steps:
  1. Tab 1: Enter name "Alice", click Join → Expect: Spawn in world
  2. Tab 2: Enter name "Bob", click Join → Expect: Spawn in world, see Alice
  3. Tab 1: Press W key → Expect: Alice moves up, Bob sees movement
  4. Tab 1: Walk to item, press E → Expect: Item disappears for both
  5. Tab 1: Press TAB → Expect: Inventory opens with picked item
  6. Tab 1: Click item → Expect: Item drops, both players see it

Pass Criteria:
  - All steps complete without errors
  - Latency <200ms (observe smooth movement)
  - 60 FPS maintained (check browser dev tools)
```

This becomes the **quickstart validation** in `quickstart.md`.

### 5. Update Agent File → `.github/copilot-instructions.md`

**Incremental Update Strategy**:

- Load existing file from Phase 3
- Add Phase 4.0 specific guidance:
  - Godot scene structure patterns
  - UI → Backend integration patterns
  - SpacetimeDB subscription handling
  - Browser export considerations
- Preserve existing backend guidance (Phase 3)
- Keep under 150 lines for token efficiency

**Output**: Updated `.github/copilot-instructions.md` in repository root (already exists, will be updated)

**Phase 1 Outputs**:

- ✅ data-model.md (client entities)
- ✅ /contracts/ (4 contract files)
- ✅ Manual test scenarios (already in MANUAL_TESTING_GUIDE.md)
- ✅ quickstart.md (validation steps)
- ✅ .github/copilot-instructions.md (updated incrementally)

---

## Phase 2: Task Planning Approach

_This section describes what the /tasks command will do - DO NOT execute during /plan_

### Task Generation Strategy

**Load Base Template**:

- Use `/templates/tasks-template.md` as structure
- Populate with Phase 4.0 specific tasks

**Generate Tasks From Design Docs**:

1. **From contracts/** (4 files):

   - Each contract → 1 manual test scenario task
   - login-contract.md → Task: "Validate login flow with duplicate name handling"
   - movement-contract.md → Task: "Validate movement sync across multiple clients"
   - inventory-contract.md → Task: "Validate inventory open/close and item display"
   - item-interaction-contract.md → Task: "Validate item pickup/drop synchronization"

2. **From data-model.md** (4 entities):

   - LocalPlayer → Task: "Create Player.tscn scene with CharacterBody2D" [P]
   - RemotePlayer → Task: "Create RemotePlayer.tscn with interpolation script" [P]
   - WorldItemInstance → Task: "Create WorldItem.tscn with Area2D pickup detection" [P]
   - InventorySlotUI → Task: "Create InventoryPanel.tscn with 12-slot grid" [P]

3. **From user scenarios** (spec.md):

   - Scenario 1 → Task: "Implement MainMenu.tscn with name input and join button"
   - Scenario 2 → Task: "Implement duplicate name error handling"
   - Scenario 3-4 → Task: "Implement LocalPlayerController.cs with WASD input"
   - Scenario 4 → Task: "Implement RemotePlayerRenderer.cs with SpacetimeDB subscriptions"
   - Scenario 5-6 → Task: "Implement WorldItemRenderer.cs with pickup/drop logic"
   - Scenario 7 → Task: "Implement InventoryUI.cs with TAB toggle"
   - Scenario 8 → Task: "Implement connection state handling"

4. **Implementation Tasks** (to make tests pass):
   - Task: "Create GameWorld.tscn with TileMap and Camera2D"
   - Task: "Wire MainMenuController to PlayerManager.TryAddPlayer()"
   - Task: "Wire LocalPlayerController to NetworkManager.SendMovement()"
   - Task: "Wire InventoryUI to Inventory.AddItem/RemoveItem()"
   - Task: "Implement position interpolation for smooth 60 FPS rendering"
   - Task: "Configure Godot HTML5 export settings"
   - Task: "Create placeholder sprites for player and items"
   - Task: "Add connection status HUD indicator"

### Ordering Strategy

**TDD Order** (Tests before implementation):

1. Manual test scenarios defined (already exist in MANUAL_TESTING_GUIDE.md)
2. Scene structure created (empty scenes)
3. Scripts implemented (wire up backend)
4. Manual tests executed (validate against scenarios)

**Dependency Order** (Bottom-up):

1. **Tier 1 - Reusable Scenes** [Parallel]:
   - Player.tscn
   - RemotePlayer.tscn
   - WorldItem.tscn
   - InventoryPanel.tscn
2. **Tier 2 - Main Scenes** [Sequential]:
   - MainMenu.tscn (depends on nothing)
   - GameWorld.tscn (depends on Player.tscn)
3. **Tier 3 - Controllers** [Sequential]:
   - MainMenuController.cs (depends on MainMenu.tscn)
   - GameManager.cs (depends on GameWorld.tscn)
   - LocalPlayerController.cs (depends on Player.tscn)
   - RemotePlayerRenderer.cs (depends on RemotePlayer.tscn)
   - WorldItemRenderer.cs (depends on WorldItem.tscn)
   - InventoryUI.cs (depends on InventoryPanel.tscn)
4. **Tier 4 - Integration** [Sequential]:
   - Wire controllers to backend
   - Configure HTML5 export
   - Manual testing validation

**Parallelization**:

- Mark [P] for tasks that can be done simultaneously (independent files)
- Tier 1 scenes are all parallel
- Scripts can be written in parallel once scenes exist

### Estimated Output

**Task Count**: Approximately 30-35 numbered tasks

- 8 manual test scenario validation tasks
- 10 scene creation tasks
- 8 script implementation tasks
- 6 integration/wiring tasks
- 3 asset creation tasks
- 2 HTML5 export configuration tasks

**Format**:

```
## Task [001]: Create Player.tscn scene [P]
**Type**: Scene Creation
**Dependencies**: None
**Estimate**: 30 minutes
**Description**: Create Player.tscn with CharacterBody2D, Sprite2D, CollisionShape2D, and Label for display name.
**Acceptance**: Scene can be instantiated in GameWorld without errors.
```

---

## Progress Tracking

### Execution Status

- [x] User description parsed
- [x] Technical context filled (minor clarifications noted)
- [x] Constitution check completed (adaptations documented)
- [ ] Phase 0: research.md generated
- [ ] Phase 1: data-model.md generated
- [ ] Phase 1: contracts/ generated
- [ ] Phase 1: quickstart.md generated
- [ ] Phase 1: copilot-instructions.md updated
- [ ] Constitution re-check after design
- [ ] Phase 2 task planning described (STOP here for /plan)

### Next Command

**Ready for**: Phase 0 execution → `research.md` generation

---

## Notes

### Phase 4.0 Context

This implementation plan builds directly on Phase 3's completed backend infrastructure. All business logic, data persistence, and network synchronization are already implemented and tested (130+ passing tests). Phase 4.0 focuses exclusively on the visual and interactive layer using Godot 4.x.

### Constitution Adaptations

Game engine development (Godot) requires adaptations to traditional library-first, CLI-based architecture:

- **Library-first** → Scene-based components (reusable .tscn prefabs)
- **CLI interfaces** → Manual testing scenarios (visual validation required)
- **Test-first** → Manual test guide written before implementation (spirit maintained)

These adaptations preserve core principles (simplicity, no abstraction, test-first) within the game engine paradigm.

### Integration Strategy

The key technical challenge is wiring Godot UI to existing C# backend classes. Contracts define these integration points clearly:

- UI controllers call backend methods directly (no intermediary layers)
- SpacetimeDB subscriptions trigger Godot scene updates
- Backend remains authoritative; client is display-only

### Performance Considerations

Critical path for 60 FPS + <200ms latency:

1. **Interpolation**: 20Hz network updates must interpolate smoothly to 60 FPS
2. **Object Pooling**: Reuse RemotePlayer/WorldItem instances (don't constantly create/destroy)
3. **HTML5 Optimization**: Compression, disable debug symbols, optimize texture sizes

Research phase will validate these patterns and provide implementation guidance.
