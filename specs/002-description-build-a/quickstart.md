# Quickstart Guide: Phase 4.0 Client Implementation

**Feature**: Browser Playable Multiplayer Prototype  
**Date**: October 8, 2025  
**Purpose**: Rapid setup and validation testing for Phase 4.0 client layer

---

## Prerequisites

### Required Software

- ✅ Godot 4.x Engine with C# support (already installed)
- ✅ .NET 7.0 SDK (already installed)
- ✅ SpacetimeDB CLI (already installed)
- ✅ Modern web browser (Chrome, Firefox, or Edge)

### Existing Infrastructure (Phase 3)

- ✅ Backend C# classes implemented and tested
- ✅ SpacetimeDB module built and ready to publish
- ✅ 130+ automated tests passing
- ✅ Manual testing guide created

### Development Environment

```bash
# Verify installations
godot --version          # Should show 4.x
dotnet --version         # Should show 7.0.x
spacetime --version      # Should show installed version
```

---

## Quick Setup (5 Minutes)

### Step 1: Start SpacetimeDB Server

```powershell
# Terminal 1: Start local SpacetimeDB instance
spacetime start

# Wait for output: "Server listening on http://localhost:3000"
```

### Step 2: Build and Publish Module

```powershell
# Terminal 2: Navigate to project root
cd C:\Users\jamai\Documents\vscode\spec-it\wanderlight-online

# Build SpacetimeDB module
dotnet build spacetime-module/wanderlight-server -c Release

# Publish to local server
spacetime publish --project-path spacetime-module/wanderlight-server wanderlight-db

# Verify: Should see "Module published successfully"
```

### Step 3: Verify Backend Tests

```powershell
# Terminal 2: Run backend tests (should already pass)
dotnet test WanderlightOnline.Tests/WanderlightOnline.Tests.csproj --settings .runsettings

# Expected: 130+ tests passed, 0 failed
```

### Step 4: Build Godot Project

```powershell
# Terminal 2: Build C# project for Godot
dotnet build "wanderlight-online"

# Expected: Build succeeded
```

---

## Phase 4.0 Implementation Steps

### Step 5: Create Godot Scenes (See tasks.md for detailed steps)

1. Open Godot Editor
2. Create scenes in order:
   - `Player.tscn` (CharacterBody2D)
   - `RemotePlayer.tscn` (CharacterBody2D)
   - `WorldItem.tscn` (Node2D with Area2D)
   - `InventoryPanel.tscn` (Panel with GridContainer)
   - `MainMenu.tscn` (Control with LineEdit + Button)
   - `GameWorld.tscn` (Node2D with TileMap + Camera2D)

### Step 6: Implement Client Scripts (See contracts/\*.md for interfaces)

1. Create `Scripts/Client/` directory
2. Implement controllers:
   - `LocalPlayerController.cs`
   - `RemotePlayerRenderer.cs`
   - `WorldItemRenderer.cs`
   - `InventoryUI.cs`
   - `MainMenuController.cs`
   - `GameManager.cs` (autoload)

### Step 7: Configure HTML5 Export

```
In Godot:
1. Project → Export
2. Add Preset → HTML5
3. Settings:
   - Export Mode: Release
   - Memory: Initial 256MB, Max 512MB
   - Compression: Gzip enabled
4. Export Project → Save to wanderlight-online/build/html5/
```

---

## Minimal Validation Test (2 Minutes)

**Purpose**: Verify all core systems work together end-to-end.

### Test Environment Setup

1. **Start SpacetimeDB**: `spacetime start` (should already be running)
2. **Open HTML5 Build**: Open `build/html5/index.html` in **two browser tabs**

### Test Sequence

#### Test 1: Dual Player Join (30 seconds)

```
Tab 1 (Alice):
1. Enter display name: "Alice"
2. Click "Join Game"
3. ✅ EXPECT: Spawn in 2D world at spawn point
4. ✅ EXPECT: See "Alice" label above character

Tab 2 (Bob):
1. Enter display name: "Bob"
2. Click "Join Game"
3. ✅ EXPECT: Spawn in 2D world
4. ✅ EXPECT: See "Bob" label above own character
5. ✅ EXPECT: See Alice's character with "Alice" label

Tab 1 (Alice):
6. ✅ EXPECT: See Bob's character appear with "Bob" label
```

**Pass Criteria**: Both players visible to each other, no errors in browser console.

#### Test 2: Movement Sync (30 seconds)

```
Tab 1 (Alice):
1. Press W key (move up) for 2 seconds
2. ✅ EXPECT: Character moves smoothly upward at 60 FPS

Tab 2 (Bob):
3. ✅ EXPECT: Alice's character moves smoothly upward
4. ✅ EXPECT: Latency <200ms (movement appears within 200ms)
5. Press D key (move right) for 2 seconds
6. ✅ EXPECT: Character moves right

Tab 1 (Alice):
7. ✅ EXPECT: Bob's character moves right smoothly
```

**Pass Criteria**: Movement visible to both players, no stuttering, latency <200ms.

#### Test 3: Item Interaction (30 seconds)

```
Setup (if items not spawned):
- Use Godot debugger or Operator class to spawn test item at (200, 200)

Tab 1 (Alice):
1. Walk to item position (200, 200)
2. ✅ EXPECT: Item sprite glows yellow (proximity indicator)
3. Press E key
4. ✅ EXPECT: Item disappears from world

Tab 2 (Bob):
5. ✅ EXPECT: Item disappears for Bob too (instant sync)

Tab 1 (Alice):
6. Press TAB key
7. ✅ EXPECT: Inventory panel opens
8. ✅ EXPECT: Picked item visible in inventory with quantity
9. Click item in inventory
10. ✅ EXPECT: Item appears on ground near Alice

Tab 2 (Bob):
11. ✅ EXPECT: Item appears on ground for Bob
12. Walk to item and press E
13. ✅ EXPECT: Bob picks up item
14. ✅ EXPECT: Item disappears for Alice
```

**Pass Criteria**: Item pickup/drop syncs instantly across both clients.

#### Test 4: Inventory Management (20 seconds)

```
Tab 1 (Alice):
1. Press TAB
2. ✅ EXPECT: Inventory opens showing 12-slot grid
3. ✅ EXPECT: Title shows "Inventory (1/12)" or current count
4. ✅ EXPECT: Picked item visible with icon and quantity
5. Press TAB again
6. ✅ EXPECT: Inventory closes
```

**Pass Criteria**: Inventory UI functional, displays correct items.

---

## Success Checklist

### Functional Validation

- [x] Backend tests passing (130+)
- [ ] Two players can join simultaneously
- [ ] Players see each other moving in real-time
- [ ] Items can be picked up by one player, disappear for all
- [ ] Items can be dropped, appear for all players
- [ ] Inventory opens/closes with TAB key
- [ ] Inventory shows correct items and quantities
- [ ] Display names appear above characters

### Performance Validation

- [ ] Stable 60 FPS (check browser dev tools: F12 → Performance)
- [ ] <200ms latency (movement → visible on other client)
- [ ] No visual stuttering or frame drops
- [ ] No console errors in browser (F12 → Console)

### Browser Compatibility

- [ ] Works in Chrome
- [ ] Works in Firefox
- [ ] Works in Edge

---

## Troubleshooting

### Issue: "Name already taken" error

**Cause**: Player with that name still in database  
**Fix**: Restart SpacetimeDB server (`spacetime stop`, then `spacetime start`)

### Issue: Black screen after "Join Game"

**Cause**: Scene not loaded or script error  
**Fix**: Check browser console (F12) for errors, verify GameWorld.tscn exists

### Issue: Players don't see each other

**Cause**: RemotePlayerRenderer not spawning instances  
**Fix**: Check SpacetimeDB subscriptions are active, verify OnPlayerAdded event

### Issue: Items don't appear/disappear

**Cause**: WorldItemRenderer not connected to events  
**Fix**: Verify OnItemSpawned/OnItemRemoved subscriptions active

### Issue: Low FPS (<60)

**Cause**: Too many entities or inefficient rendering  
**Fix**: Check entity count (<100 players + items), enable Godot profiler

### Issue: High latency (>500ms)

**Cause**: Network issue or SpacetimeDB overload  
**Fix**: Check localhost connection, restart SpacetimeDB, reduce update rate

---

## Development Workflow

### Iteration Cycle (Edit → Test → Repeat)

```
1. Edit C# script in VS Code or Godot editor
2. Save file (Ctrl+S)
3. Godot auto-reloads C# assembly (if editor open)
4. Press F5 in Godot to run game
5. Test change locally
6. If good: Export to HTML5, test in browser
7. If issue: Check console, fix, repeat
```

### Debug Tools

- **Godot Console**: Built-in output panel (shows GD.Print, errors)
- **Browser DevTools**: F12 → Console (JavaScript errors, network logs)
- **Remote Debugging**: Godot can attach debugger to HTML5 build
- **SpacetimeDB Logs**: Check terminal running `spacetime start`

### Performance Profiling

```
In Godot Editor:
1. Debug → Profiler
2. Run game (F5)
3. Check "Process" time per frame (should be <16ms for 60 FPS)
4. Check "Physics" time (movement processing)
5. Identify bottlenecks

In Browser:
1. F12 → Performance tab
2. Record gameplay session
3. Check FPS graph (should be steady 60)
4. Check long tasks (should be none)
```

---

## Next Steps After Quickstart

### If All Tests Pass ✅

1. **Run Full Manual Testing Suite**: Follow `MANUAL_TESTING_GUIDE.md` (8 scenarios)
2. **Stress Test**: Open 5+ browser tabs, verify performance
3. **Edge Case Testing**: Test duplicate names, inventory full, disconnects
4. **Documentation**: Update README with Phase 4.0 completion notes

### If Tests Fail ❌

1. **Check Browser Console**: F12 → Console for errors
2. **Check SpacetimeDB Logs**: Terminal running `spacetime start`
3. **Review Contracts**: Verify UI → Backend integration matches contracts/\*.md
4. **Debug Systematically**: Isolate issue (UI only? Network? Backend?)
5. **Consult Implementation Plan**: Review plan.md for architecture guidance

---

## Performance Targets

| Metric           | Target     | How to Measure                      |
| ---------------- | ---------- | ----------------------------------- |
| Join Time        | <2s        | Button click → Player visible       |
| Frame Rate       | 60 FPS     | Browser DevTools → Performance      |
| Movement Latency | <200ms p95 | Log timestamps, measure delta       |
| Item Sync        | <200ms     | Drop item → Visible on other client |
| Inventory Open   | <16ms      | TAB press → Panel visible (instant) |

---

## Resources

### Documentation

- **Specification**: `spec.md` (feature requirements)
- **Implementation Plan**: `plan.md` (architecture and approach)
- **Research**: `research.md` (technical patterns)
- **Data Model**: `data-model.md` (entity definitions)
- **Contracts**: `contracts/*.md` (integration interfaces)

### Code References

- **Backend Classes**: `Scripts/*.cs` (Phase 3, already tested)
- **Client Scripts**: `Scripts/Client/*.cs` (Phase 4.0, to be implemented)
- **Godot Scenes**: `Scenes/*.tscn` (Phase 4.0, to be created)

### External Docs

- **Godot C# Docs**: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/
- **SpacetimeDB Docs**: https://spacetimedb.com/docs
- **Manual Testing Guide**: `../../MANUAL_TESTING_GUIDE.md`

---

## Summary

This quickstart provides a **2-minute validation test** that proves:

1. ✅ Backend systems work (Phase 3 complete)
2. ✅ Client UI connects to backend
3. ✅ Real-time multiplayer synchronization works
4. ✅ All core features functional (join, move, pickup, inventory)

Once these 4 tests pass, proceed to full manual testing suite for comprehensive validation.

---

**Test Duration**: 2 minutes  
**Expected Result**: All 4 tests pass, 0 console errors  
**Next Step**: Full manual testing suite (`MANUAL_TESTING_GUIDE.md`)

**Status**: Ready for Phase 4.0 Implementation

---

**Last Updated**: October 8, 2025  
**Created By**: Phase 4.0 Planning Process
