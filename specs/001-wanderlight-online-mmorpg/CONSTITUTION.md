# Wanderlight Online MVP - Project Constitution

## Phase 4.0: Client Implementation & Browser Playability

**Last Updated:** October 8, 2025  
**Status:** Active Development

---

## 🎯 Project Vision

Build a **browser-first 2D MMORPG** that demonstrates real-time multiplayer gameplay with a clean, maintainable codebase. This is an MVP focused on proving core systems work together seamlessly in a browser environment.

---

## 🏛️ Core Principles

### 1. **Browser-First Architecture**

- **Primary Platform:** HTML5 export via Godot 4.x
- **Target:** Modern browsers (Chrome, Firefox, Edge)
- **Delivery:** Single-page web application with minimal dependencies
- **Rationale:** Maximum accessibility, no installation barriers, rapid iteration

### 2. **Real-Time Multiplayer Foundation**

- **Backend:** SpacetimeDB for authoritative state and persistence
- **Transport:** WebSocket for low-latency communication
- **Sync Model:** Server-authoritative with client prediction (where appropriate)
- **Rationale:** Ensure fair gameplay and prevent cheating while maintaining responsiveness

### 3. **Test-Driven Development (TDD)**

- **Mandate:** Write tests before implementation (Red-Green-Refactor)
- **Coverage:** Unit tests for logic, integration tests for systems, contract tests for SpacetimeDB
- **Verification:** All tests must pass before merging
- **Rationale:** Catch bugs early, enable fearless refactoring, serve as living documentation

### 4. **Simplicity Over Cleverness**

- **Code Style:** Clear, explicit code over clever abstractions
- **Architecture:** Flat hierarchies, minimal indirection
- **Dependencies:** Only add libraries when truly necessary
- **Rationale:** Maintainability, onboarding ease, debugging simplicity

### 5. **Performance Targets (Non-Negotiable)**

- **Latency:** <200ms p95 for player actions
- **Frame Rate:** Stable 60 FPS on client
- **Concurrency:** Support 50+ concurrent players
- **Rationale:** Playability requires responsive, smooth gameplay

### 6. **Functional UI Over Fancy**

- **Design Priority:** Working features > visual polish
- **MVP Aesthetics:** Simple sprites, clean layouts, clear feedback
- **User Experience:** Intuitive controls, obvious interactions
- **Rationale:** Ship working gameplay first, iterate on polish later

### 7. **Production-Ready Code Quality**

- **Logging:** Structured logs for all key events (join, move, pickup, drop)
- **Error Handling:** Graceful degradation, user-friendly error messages
- **Validation:** Input sanitization at all boundaries
- **Rationale:** Operational excellence, debuggability, security

---

## 📦 Phase 4.0 Scope

### **Already Complete (Phase 3)**

✅ All backend C# classes implemented and tested:

- `PlayerManager` - Player lifecycle management
- `DatabaseManager` - SpacetimeDB integration
- `Inventory` - Item management with 12 slots
- `NetworkManager` - WebSocket communication
- `PlayerController` - Movement logic
- `Operator` - Administrative functions
- `WorldItem` - Ground item entities

✅ 130+ automated tests passing  
✅ SpacetimeDB schema and reducers deployed  
✅ Manual testing guide created

### **Phase 4.0 Focus: Client-Side Implementation**

The backend is proven and stable. Now we build the user-facing layer:

#### **Client Deliverables:**

1. **Login Scene** - Name input, join validation, error handling
2. **Game World Scene** - 2D environment with camera, spawn points
3. **Player Character** - Visual sprite, movement input, collision
4. **Remote Player Rendering** - Real-time sync of other players
5. **World Items Rendering** - Visual items on ground, pickup interactions
6. **Inventory UI** - 12-slot panel, drag/drop, quantity display
7. **Network Integration** - Wire UI to existing backend classes
8. **Browser Export** - HTML5 build with loading screen, connection status

#### **Out of Scope (Future Phases)**

❌ Combat, NPCs, quests, trading, chat  
❌ Complex animations or particle effects  
❌ Sound effects or music  
❌ Mobile controls or responsive design  
❌ Persistent accounts or authentication

---

## 🛠️ Technology Stack

### **Client**

- **Engine:** Godot 4.x (C# scripting)
- **Rendering:** 2D sprites, TileMap for world
- **Input:** Keyboard (WASD/Arrows, E for pickup, TAB for inventory)
- **UI:** Godot Control nodes (Panel, Button, Label, GridContainer)
- **Export:** HTML5 with WebSocket support

### **Backend (Already Implemented)**

- **Database:** SpacetimeDB (real-time, relational)
- **Language:** C# (.NET)
- **Communication:** WebSocket via SpacetimeDB client SDK
- **State Model:** Server-authoritative with client subscriptions

---

## 🎮 User Experience Requirements

### **Player Journey:**

1. Open game in browser
2. Enter unique display name
3. Click "Join" → Spawn in 2D world
4. Move with WASD/Arrows → See position update in real-time
5. Walk to item → Press E to pick up → Item disappears for all players
6. Press TAB → Open inventory → See item added
7. Click item in inventory → Drop to world → Item appears for all players
8. Other players see all actions with <200ms latency

### **Acceptance Criteria:**

- ✅ Multiple browser tabs can join as different players
- ✅ Players see each other moving smoothly
- ✅ Items can be picked up by one player and others see it disappear
- ✅ Dropped items appear for all players instantly
- ✅ Inventory shows correct items and counts
- ✅ Display names appear above characters
- ✅ Works in modern browsers without plugins

---

## 📐 Architectural Decisions

### **Scene Hierarchy**

```
Main Menu (Control)
├── VBoxContainer
│   ├── LineEdit (display name input)
│   ├── Button (join)
│   └── Label (error messages)

Game World (Node2D)
├── TileMap (environment)
├── Camera2D (follows local player)
├── LocalPlayer (CharacterBody2D)
│   ├── Sprite2D
│   ├── CollisionShape2D
│   └── Label (display name)
├── RemotePlayers (Node2D container)
│   └── RemotePlayer instances
├── WorldItems (Node2D container)
│   └── WorldItem instances
└── UI (CanvasLayer)
    ├── HUD
    └── InventoryPanel
```

### **Script Organization**

```
Scripts/
├── [Existing Backend Classes]
│   ├── PlayerManager.cs
│   ├── DatabaseManager.cs
│   ├── Inventory.cs
│   ├── NetworkManager.cs
│   ├── PlayerController.cs
│   ├── Operator.cs
│   └── WorldItem.cs
└── Client/ [New for Phase 4]
    ├── MainMenuController.cs
    ├── GameManager.cs
    ├── LocalPlayerController.cs
    ├── RemotePlayerRenderer.cs
    ├── WorldItemRenderer.cs
    └── InventoryUI.cs
```

### **Integration Pattern**

- **UI → Backend:** Client scripts call existing backend classes
- **Backend → SpacetimeDB:** Backend classes use DatabaseManager
- **SpacetimeDB → Client:** Subscribe to table updates, render changes
- **Signal Flow:** Godot signals for UI events → Backend methods → Network calls

---

## 🧪 Testing Strategy

### **Phase 4.0 Testing Approach**

- **Manual Testing:** Follow `MANUAL_TESTING_GUIDE.md` for 8 scenarios
- **Browser Testing:** Open multiple tabs for multiplayer validation
- **Integration Testing:** Ensure UI calls backend correctly
- **Performance Testing:** Monitor frame rate and latency with 5+ concurrent players

### **Testing Checklist Before Release**

1. ✅ All 130+ backend tests still passing
2. ✅ Manual test scenarios 1-8 completed successfully
3. ✅ Browser export builds without errors
4. ✅ Multiple players can join and interact
5. ✅ No console errors in browser developer tools
6. ✅ Frame rate stable at 60 FPS with 10+ players
7. ✅ Network latency <200ms (measured via logging)

---

## 🚀 Implementation Workflow

### **Development Cycle:**

1. **Spec Review** - Understand the feature requirement
2. **Test Writing** - Write manual test case (update guide if needed)
3. **UI Design** - Sketch scene layout, plan node hierarchy
4. **Implementation** - Build scene, write script, wire to backend
5. **Local Testing** - Test in Godot editor
6. **Browser Testing** - Export and test in browser
7. **Validation** - Run full manual testing suite
8. **Commit** - Small, atomic commits with clear messages

### **Git Workflow:**

- Branch: `001-wanderlight-online-mmorpg`
- Commit Format: `[Phase 4.0] <component>: <action>`
- Example: `[Phase 4.0] Login Scene: Add name validation`

---

## 📊 Success Metrics

### **Phase 4.0 Definition of Done:**

- [ ] All 8 client deliverables implemented
- [ ] HTML5 export functional in 3+ browsers
- [ ] 5+ concurrent players tested successfully
- [ ] Manual testing guide scenarios pass
- [ ] No critical bugs or crashes
- [ ] Code documented and commented
- [ ] README updated with build/run instructions

### **Performance Validation:**

- Measure latency with structured logging
- Profile frame rate with Godot's built-in profiler
- Stress test with 10+ browser tabs
- Monitor memory usage over 10-minute session

---

## 🔒 Constraints & Limitations

### **Hard Limits:**

- **Inventory:** 12 slots (per spec)
- **Concurrent Players:** 50+ (target), 100 max entities visible
- **Item Actions:** Must be atomic (no partial pickups)
- **Display Names:** Must be unique per session

### **MVP Boundaries:**

- No persistent accounts (session-based only)
- No chat or social features
- No combat or damage systems
- No quest or progression systems
- No trading between players

---

## 📚 Reference Documents

- **Specification:** `specs/001-wanderlight-online-mmorpg/spec.md`
- **Data Model:** `specs/001-wanderlight-online-mmorpg/data-model.md`
- **Testing Guide:** `MANUAL_TESTING_GUIDE.md`
- **Tasks:** `specs/001-wanderlight-online-mmorpg/tasks.md`
- **Quickstart:** `specs/001-wanderlight-online-mmorpg/quickstart.md`

---

## 🤝 Contributing Guidelines

### **Code Style:**

- C# conventions (PascalCase for public, camelCase for private)
- Godot naming (snake_case for signals and node paths)
- Clear variable names (avoid abbreviations unless obvious)
- Comments for "why", not "what"

### **Commit Messages:**

- Present tense ("Add feature" not "Added feature")
- Imperative mood ("Move cursor to..." not "Moves cursor to...")
- Reference issue/task number if applicable
- Keep under 72 characters

### **Pull Request (Future):**

- All tests passing
- Manual testing checklist completed
- Screenshots/video for UI changes
- Updated documentation

---

## 🎓 Learning Resources

### **Godot 4.x C# Docs:**

- [Official C# Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/)
- [CharacterBody2D](https://docs.godotengine.org/en/stable/classes/class_characterbody2d.html)
- [HTML5 Export](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html)

### **SpacetimeDB:**

- [Client SDK Guide](https://spacetimedb.com/docs/client-sdk)
- [Subscriptions](https://spacetimedb.com/docs/subscriptions)

---

## 📝 Document Revision History

| Version | Date       | Changes                        |
| ------- | ---------- | ------------------------------ |
| 1.0     | 2025-10-08 | Initial Phase 4.0 constitution |

---

**Remember:** This constitution is a living document. Update it as the project evolves, but always preserve the core principles. When in doubt, choose simplicity, testability, and user experience.
