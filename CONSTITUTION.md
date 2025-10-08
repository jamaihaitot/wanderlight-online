# Wanderlight Online MVP - Phase 4.0 Constitution

**Browser-First 2D MMORPG | Godot 4.x + C# + SpacetimeDB**

> **Full Constitution:** See `specs/001-wanderlight-online-mmorpg/CONSTITUTION.md`

---

## 🏛️ Core Principles

### 1. **Browser-First Architecture**

Primary platform is HTML5 export. Target modern browsers with minimal dependencies.

### 2. **Real-Time Multiplayer Foundation**

SpacetimeDB backend with WebSocket transport for low-latency, server-authoritative gameplay.

### 3. **Test-Driven Development (TDD)**

Write tests before implementation. Red-Green-Refactor. All tests must pass.

### 4. **Simplicity Over Cleverness**

Clear, explicit code. Flat hierarchies. No unnecessary abstractions.

### 5. **Performance Targets (Non-Negotiable)**

- **Latency:** <200ms p95
- **Frame Rate:** 60 FPS stable
- **Concurrency:** 50+ players

### 6. **Functional UI Over Fancy**

Working features first, visual polish later. Simple, intuitive, obvious.

### 7. **Production-Ready Code Quality**

Structured logging, graceful error handling, input validation everywhere.

---

## 📦 Phase 4.0 Scope

### ✅ **Already Complete (Phase 3)**

- All backend C# classes implemented and tested
- 130+ automated tests passing
- SpacetimeDB schema and reducers deployed
- Manual testing guide created

### 🎯 **Phase 4.0 Focus: Client Layer**

Build user-facing Godot scenes and UI:

1. Login Scene
2. Game World Scene
3. Player Character
4. Remote Player Rendering
5. World Items Rendering
6. Inventory UI
7. Network Integration
8. Browser Export

### ❌ **Out of Scope**

Combat, NPCs, quests, trading, chat, animations, sound, mobile, accounts.

---

## 🛠️ Tech Stack

- **Client:** Godot 4.x (C#), HTML5 export
- **Backend:** SpacetimeDB (C#)
- **Transport:** WebSocket
- **Testing:** xUnit + Manual testing guide

---

## 🎮 User Experience

1. Open game in browser → Enter name → Join
2. Move with WASD → See other players in real-time
3. Walk to item → Press E → Pick up
4. Press TAB → Open inventory → Click item → Drop
5. All actions visible to all players with <200ms latency

---

## 📐 Architecture

```
UI (Godot Scenes)
    ↓ calls
Backend Classes (already tested)
    ↓ uses
DatabaseManager + NetworkManager
    ↓ communicates
SpacetimeDB
```

---

## 🧪 Testing Strategy

- Manual testing guide with 8 scenarios
- Multiple browser tabs for multiplayer validation
- Performance monitoring (FPS, latency)
- All acceptance criteria must pass

---

## 🚀 Implementation Workflow

1. Spec Review → 2. Test Writing → 3. UI Design → 4. Implementation
2. Local Testing → 6. Browser Testing → 7. Validation → 8. Commit

---

## 📊 Definition of Done

- [ ] All 8 client deliverables implemented
- [ ] HTML5 export works in 3+ browsers
- [ ] 5+ concurrent players tested
- [ ] Manual testing scenarios pass
- [ ] No critical bugs
- [ ] Code documented
- [ ] README updated

---

## 🔒 Hard Constraints

- 12 inventory slots (per spec)
- 50+ CCU target, 100 max visible entities
- Atomic item actions (no partial operations)
- Unique display names per session

---

## 📚 Key Documents

- **Constitution:** `specs/001-wanderlight-online-mmorpg/CONSTITUTION.md`
- **Copilot Instructions:** `specs/001-wanderlight-online-mmorpg/.github/copilot-instructions.md`
- **Specification:** `specs/001-wanderlight-online-mmorpg/spec.md`
- **Testing Guide:** `MANUAL_TESTING_GUIDE.md`
- **Tasks:** `specs/001-wanderlight-online-mmorpg/tasks.md`

---

## 🎓 Quick Reference

### **Build Commands:**

```bash
dotnet build wanderlight-online
dotnet build spacetime-module/wanderlight-server -c Release
dotnet test WanderlightOnline.Tests/WanderlightOnline.Tests.csproj
```

### **SpacetimeDB:**

```bash
spacetime start
spacetime list
spacetime publish --project-path spacetime-module/wanderlight-server wanderlight-db
```

---

**Last Updated:** October 8, 2025  
**Status:** Phase 4.0 Active Development

**Remember:** Backend is solid. Focus on clean, functional UI that leverages existing tested classes. When in doubt, choose simplicity and testability.
