# Copilot Implementation Instructions

## Overview

This file provides agent-specific instructions for GitHub Copilot to guide implementation of **Wanderlight Online MVP Phase 4.0** — Client-side implementation and browser playability.

**See:** `CONSTITUTION.md` for full project principles and vision.

---

## Phase 4.0 Context

### ✅ **Backend Complete (Phase 3)**

All core C# classes are implemented and tested (130+ tests passing):

- `PlayerManager` - Player lifecycle management
- `DatabaseManager` - SpacetimeDB integration
- `Inventory` - 12-slot item management
- `NetworkManager` - WebSocket communication
- `PlayerController` - Movement logic
- `Operator` - Administrative functions
- `WorldItem` - Ground item entities

### 🎯 **Phase 4.0 Focus: Client Layer**

Build the user-facing Godot scenes and UI that wire into the existing backend.

---

## Key Principles (Phase 4.0)

### 1. **Browser-First**

- Primary target: HTML5 export via Godot 4.x
- Minimal dependencies, maximum compatibility
- Test in multiple browsers (Chrome, Firefox, Edge)

### 2. **Real-Time Multiplayer**

- SpacetimeDB backend (already implemented)
- WebSocket for low-latency communication
- Server-authoritative state management

### 3. **Test-Driven Development (TDD)**

- Write tests before implementation (Red-Green-Refactor)
- Manual testing guide already exists (`MANUAL_TESTING_GUIDE.md`)
- All tests must pass before merging

### 4. **Simplicity Over Cleverness**

- Clear, explicit code over clever abstractions
- Flat hierarchies, minimal indirection
- Only add dependencies when necessary

### 5. **Performance Targets (Non-Negotiable)**

- **Latency:** <200ms p95 for player actions
- **Frame Rate:** Stable 60 FPS on client
- **Concurrency:** Support 50+ concurrent players

### 6. **Functional UI Over Fancy**

- Working features > visual polish
- Simple sprites, clean layouts, clear feedback
- Intuitive controls, obvious interactions

### 7. **Production-Ready Code Quality**

- Structured logging for all key events
- Graceful error handling with user-friendly messages
- Input validation at all boundaries

---

## Implementation Steps (Phase 4.0)

### **Client Deliverables:**

1. **Login Scene** - Name input, join validation, error display
2. **Game World Scene** - 2D environment, camera, spawn points
3. **Player Character** - Visual sprite, movement input, collision
4. **Remote Player Rendering** - Real-time sync of other players
5. **World Items Rendering** - Visual items, pickup interactions
6. **Inventory UI** - 12-slot panel, drag/drop, quantity display
7. **Network Integration** - Wire UI to existing backend classes
8. **Browser Export** - HTML5 build with loading screen, connection status

### **Architecture Pattern:**

```
UI (Godot Scenes/Scripts)
    ↓ calls
Backend Classes (already implemented)
    ↓ uses
DatabaseManager + NetworkManager
    ↓ communicates
SpacetimeDB (real-time backend)
```

### **Scene Structure:**

```
Scenes/
├── MainMenu.tscn (login screen)
├── GameWorld.tscn (main game scene)
├── Player.tscn (player character prefab)
├── RemotePlayer.tscn (remote player instance)
├── WorldItem.tscn (item on ground)
└── UI/
    ├── InventoryPanel.tscn
    └── HUD.tscn
```

### **Script Organization:**

```
Scripts/
├── [Existing Backend - DO NOT MODIFY]
│   ├── PlayerManager.cs
│   ├── DatabaseManager.cs
│   ├── Inventory.cs
│   └── ...
└── Client/ [NEW - Phase 4.0]
    ├── MainMenuController.cs
    ├── GameManager.cs
    ├── LocalPlayerController.cs
    ├── RemotePlayerRenderer.cs
    ├── WorldItemRenderer.cs
    └── InventoryUI.cs
```

---

## Implementation Guidelines

### **When Creating Godot Scenes:**

- Use Godot Control nodes for UI (Panel, Button, Label, LineEdit, GridContainer)
- Use Node2D/CharacterBody2D for game entities
- Keep scene hierarchies flat and simple
- Name nodes clearly (avoid generic names like "Node2D")

### **When Writing C# Scripts:**

- Follow C# conventions (PascalCase for public, camelCase for private)
- Use Godot naming for signals/nodes (snake_case)
- Add structured logging for key events (join, move, pickup, drop)
- Handle errors gracefully with user-friendly messages
- Validate all user input

### **When Integrating with Backend:**

- Call existing backend classes directly (don't duplicate logic)
- Use DatabaseManager for SpacetimeDB interactions
- Subscribe to table updates for real-time sync
- Follow existing patterns from backend tests

### **When Testing:**

- Follow manual testing guide scenarios
- Test with multiple browser tabs (multiplayer)
- Validate performance (60 FPS, <200ms latency)
- Check all acceptance criteria from spec

---

## Constraints & Limitations

### **Hard Limits:**

- **Inventory:** 12 slots (per spec)
- **Concurrent Players:** 50+ (target), 100 max entities visible
- **Item Actions:** Must be atomic (no partial pickups)
- **Display Names:** Must be unique per session

### **Out of Scope for MVP:**

- Combat, NPCs, quests, trading, chat
- Complex animations or particle effects
- Sound effects or music
- Mobile controls or responsive design
- Persistent accounts or authentication

---

## Performance Considerations

- **Object Pooling:** For remote players and world items
- **Interpolation:** Smooth 20Hz network updates → 60FPS rendering
- **Entity Limits:** Max 100 visible entities (already in spec)
- **HTML5 Optimization:** Disable debug, enable compression
- **Logging:** Use structured logs, avoid excessive console output

---

## Development Workflow

1. **Spec Review** - Understand the feature requirement
2. **Test Writing** - Write manual test case (if needed)
3. **UI Design** - Sketch scene layout, plan node hierarchy
4. **Implementation** - Build scene, write script, wire to backend
5. **Local Testing** - Test in Godot editor
6. **Browser Testing** - Export and test in browser
7. **Validation** - Run full manual testing suite
8. **Commit** - Small, atomic commits with clear messages

---

## Reference Documents

- **Constitution:** `CONSTITUTION.md` (project principles)
- **Specification:** `spec.md` (detailed requirements)
- **Data Model:** `data-model.md` (SpacetimeDB schema)
- **Testing Guide:** `../../MANUAL_TESTING_GUIDE.md` (8 test scenarios)
- **Tasks:** `tasks.md` (implementation checklist)

---

## Quick Commands

### **Build & Test:**

```bash
# Build Godot project
dotnet build "wanderlight-online"

# Build SpacetimeDB module
dotnet build "spacetime-module/wanderlight-server" -c Release

# Run tests
dotnet test WanderlightOnline.Tests/WanderlightOnline.Tests.csproj --settings .runsettings
```

### **SpacetimeDB:**

```bash
# Start server
spacetime start

# List databases
spacetime list

# Publish module (after building)
spacetime publish --project-path spacetime-module/wanderlight-server wanderlight-db
```

### **Manual Testing:**

1. Start SpacetimeDB: `spacetime start`
2. Build and publish module
3. Open Godot and export to HTML5
4. Open multiple browser tabs
5. Follow `MANUAL_TESTING_GUIDE.md` scenarios

---

## Success Criteria (Phase 4.0)

- [ ] All 8 client deliverables implemented
- [ ] HTML5 export functional in 3+ browsers
- [ ] 5+ concurrent players tested successfully
- [ ] Manual testing guide scenarios pass
- [ ] No critical bugs or crashes
- [ ] Code documented and commented
- [ ] README updated with build/run instructions
- [ ] Performance targets met (<200ms latency, 60 FPS)

---

**Remember:** The backend is solid. Focus on creating a clean, functional UI that leverages the existing tested classes. When in doubt, choose simplicity and testability.
