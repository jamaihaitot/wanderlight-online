# Manual Testing Guide - Wanderlight Online

## Overview

This guide provides comprehensive manual testing scenarios to validate the Wanderlight Online MMORPG foundation. Use this alongside automated tests to ensure quality and gather feedback for iteration.

## Test Environment Setup

### Prerequisites

1. **SpacetimeDB Server Running**

   ```bash
   spacetime start
   ```

2. **Module Deployed**

   ```bash
   cd spacetime-module/wanderlight-server
   dotnet build -c Release
   spacetime publish
   ```

3. **Godot Editor**
   - Open project: `wanderlight-online/project.godot`
   - Build C# project: `dotnet build wanderlight-online`

## Test Scenarios

### 1. Player Authentication & Join

**Objective**: Verify unique display name authentication works correctly.

**Steps**:

1. Launch the game
2. Enter display name: `TestPlayer1`
3. Click "Join Game"
4. Observe player spawns at position (0, 0)
5. Open a second instance
6. Try to join with same name: `TestPlayer1`
7. Confirm rejection message appears
8. Use unique name: `TestPlayer2`
9. Confirm successful join

**Expected Results**:

- ✅ Unique names join successfully
- ✅ Duplicate names rejected with clear error
- ✅ Players spawn at origin by default
- ✅ Both players visible on both clients

**Performance Target**: Join latency < 200ms

---

### 2. Movement & Synchronization

**Objective**: Validate real-time movement with 20Hz delta updates.

**Steps**:

1. Join with 2 clients (`PlayerA`, `PlayerB`)
2. Move `PlayerA` using WASD or arrow keys
3. Observe movement on both clients
4. Move both players simultaneously in different directions
5. Stop movement and verify both clients show identical positions
6. Rapidly change direction (zigzag pattern)
7. Observe smooth interpolation on remote client

**Expected Results**:

- ✅ Movement updates within 50ms (20Hz = 50ms intervals)
- ✅ No "rubber-banding" or position snapping
- ✅ Consistent positions across all clients
- ✅ Smooth interpolation between updates

**Performance Target**: Movement sync latency < 200ms

---

### 3. Inventory Operations

**Objective**: Test 12-slot inventory with stacking rules.

**Steps**:

#### Basic Operations

1. Pick up 5 `Wood` items (Generic, MaxStack=20)
2. Verify slot shows "Wood (5)"
3. Pick up 10 more `Wood`
4. Confirm stacks to "Wood (15)"
5. Pick up 10 more `Wood`
6. Confirm one slot has "Wood (20)", second slot has "Wood (5)"

#### Category Testing

7. Pick up `HealthPotion` (Consumable, MaxStack=20)
8. Verify stacks up to 20
9. Pick up `IronSword` (Equipment, MaxStack=1)
10. Pick up second `IronSword`
11. Confirm each occupies separate slot

#### Full Inventory

12. Fill all 12 slots
13. Attempt to pick up additional item
14. Verify rejection message: "Inventory Full"
15. Drop one item
16. Pick up previously rejected item
17. Confirm successful pickup

**Expected Results**:

- ✅ Generic items stack to 20
- ✅ Consumables stack to 20
- ✅ Equipment doesn't stack (1 per slot)
- ✅ Full inventory prevents pickup
- ✅ Dropped items appear in world at player position

**Performance Target**: Inventory operations < 200ms

---

### 4. Item Drop & World Persistence

**Objective**: Verify item drop atomicity and world item tracking.

**Steps**:

1. Add `Stone (10)` to inventory
2. Drop `Stone (5)` from inventory
3. Observe WorldItem appears at player position
4. Verify inventory shows `Stone (5)` remaining
5. Move away from dropped item
6. Reconnect with same player
7. Return to drop location
8. Confirm WorldItem still exists
9. Pick up WorldItem
10. Verify inventory shows `Stone (10)` again

**Expected Results**:

- ✅ Drop operation atomic (both inventory update and world spawn)
- ✅ WorldItem position matches drop location
- ✅ WorldItem persists across sessions
- ✅ Pickup operation atomic (world removal and inventory add)

---

### 5. Operator Telemetry & Logs

**Objective**: Validate operator-level monitoring and control.

**Steps**:

#### Telemetry Collection

1. Join as operator (owner permission)
2. Open telemetry dashboard
3. Join with 3 additional players
4. Verify "Active Players" metric updates to 4
5. Perform 10 movement actions
6. Verify "Movement Events" increments
7. Perform 5 item pickups
8. Verify "Item Operations" increments

#### Structured Logging

9. Open SpacetimeDB logs
10. Search for: `[PlayerManager]`
11. Verify structured log format:
    ```
    [Info] YYYY-MM-DD HH:MM:SS - [PlayerManager] TryAddPlayer: Success for 'TestPlayer'
    ```
12. Perform world reset
13. Verify log entry:
    ```
    [Info] YYYY-MM-DD HH:MM:SS - World reset initiated via SpacetimeDB
    ```

#### World Reset

14. Create test data (10 players, 50 items)
15. Execute world reset command
16. Verify all players disconnected
17. Verify all items removed from world
18. Verify clean slate for new session

**Expected Results**:

- ✅ Metrics update in real-time
- ✅ Logs use consistent structured format
- ✅ Timestamp accuracy within 1 second
- ✅ World reset completes < 500ms
- ✅ All state cleared after reset

---

### 6. Reconnection & State Restoration

**Objective**: Ensure player state persists across disconnects.

**Steps**:

1. Join as `ReconnectTest`
2. Move to position (50.0, 75.0)
3. Add items to inventory:
   - `Wood (15)` in slot 0
   - `HealthPotion (8)` in slot 1
   - `IronSword (1)` in slot 2
4. Note current state
5. Disconnect client
6. Wait 5 seconds
7. Reconnect with same display name
8. Verify position restored to (50.0, 75.0)
9. Verify inventory matches previous state
10. Perform movement to confirm active connection

**Expected Results**:

- ✅ Position restored exactly
- ✅ Inventory contents match
- ✅ Inventory slot order preserved
- ✅ Reconnection latency < 500ms
- ✅ No duplicate player entries

---

### 7. Concurrent User Load Test

**Objective**: Stress test with multiple simultaneous users.

**Setup**: Launch 10 client instances (use scripting or manual).

**Steps**:

1. Join all 10 clients with unique names
2. All clients move simultaneously in random directions
3. Each client performs 5 item pickups
4. Each client drops 3 items
5. Monitor performance metrics on all clients

**Expected Results**:

- ✅ All 10 clients join successfully
- ✅ Frame rate maintains 60 FPS on all clients
- ✅ No packet loss or disconnections
- ✅ Position sync remains accurate for all players
- ✅ Server maintains 20Hz update rate

**Performance Targets**:

- Movement sync: < 200ms
- Item operations: < 200ms
- Server CPU: < 50%
- Memory usage: < 500MB

---

### 8. Edge Cases & Error Handling

**Objective**: Validate graceful handling of error conditions.

**Scenarios**:

#### Empty Name

1. Attempt join with empty display name
2. **Expected**: Rejection with message "Display name cannot be empty"

#### Invalid Movement

1. Attempt to move to invalid position (e.g., NaN, Infinity)
2. **Expected**: Movement rejected, player position unchanged

#### Item Quantity Overflow

1. Attempt to add more than MaxStack to inventory
2. **Expected**: Excess quantity handled (split into multiple slots or rejected)

#### Rapid Actions

1. Spam pickup/drop commands (50 times in 1 second)
2. **Expected**: All commands processed or queued, no crashes

#### Server Disconnect

1. Stop SpacetimeDB server mid-session
2. **Expected**: Client shows "Connection Lost" message
3. Restart server
4. **Expected**: Client attempts reconnection automatically

---

## Feedback Collection

### Usability Testing

**Questions for testers**:

1. Was the join process intuitive?
2. Did movement feel responsive?
3. Was inventory management clear?
4. Were error messages helpful?
5. Did you encounter any confusing behavior?

### Performance Feedback

**Metrics to collect**:

- Average FPS across all clients
- Perceived latency (subjective 1-10 scale)
- Number of disconnections per hour
- Time to complete common tasks (join, pick up 10 items, etc.)

### Bug Reporting Template

```markdown
**Bug Title**: Brief description

**Steps to Reproduce**:

1. Step one
2. Step two
3. ...

**Expected Behavior**: What should happen

**Actual Behavior**: What actually happened

**Client Version**: [Git commit hash]
**SpacetimeDB Version**: [Version number]
**OS**: Windows/Linux/macOS

**Severity**: Critical / High / Medium / Low

**Additional Context**: Screenshots, logs, etc.
```

---

## Iteration Process

### 1. Collect Feedback

- Run manual tests with 3-5 testers
- Record observations and metrics
- Gather subjective feedback

### 2. Analyze Results

- Identify common issues
- Prioritize by severity and frequency
- Group related feedback

### 3. Plan Improvements

- Create tasks for bug fixes
- Design solutions for UX issues
- Define acceptance criteria

### 4. Implement Changes

- Follow TDD: write tests first
- Implement fixes/features
- Validate with automated tests

### 5. Validate

- Re-run affected manual tests
- Confirm issue resolution
- Check for regressions

### 6. Repeat

- Schedule next testing session
- Iterate until quality goals met

---

## Success Criteria

### Functional Requirements

- ✅ All manual test scenarios pass
- ✅ Zero critical bugs
- ✅ < 5 medium-severity bugs

### Performance Requirements

- ✅ Join latency < 200ms
- ✅ Movement sync < 200ms
- ✅ Inventory operations < 200ms
- ✅ 60 FPS with 10 concurrent players

### User Experience

- ✅ Average usability rating > 7/10
- ✅ Error messages rated "helpful" by > 80% of testers
- ✅ Core gameplay loop (join → move → collect items) completable without assistance

---

## Tools & Resources

### Monitoring

- **SpacetimeDB Dashboard**: Monitor server health, query performance
- **Client Debug Panel**: Display FPS, latency, player count
- **Log Aggregation**: Collect logs from all clients for analysis

### Automation Helpers

- **Multi-Client Launcher**: Script to launch N clients with unique names
- **Traffic Generator**: Simulate user actions programmatically
- **Performance Profiler**: Identify bottlenecks in client code

### Documentation

- **API Reference**: `Scripts/` folder class documentation
- **Design Docs**: `specs/001-wanderlight-online-mmorpg/`
- **Test Reports**: `wanderlight-online/reports/`

---

## Appendix: Quick Reference

### Common Test Commands

```bash
# Start server
spacetime start

# Deploy module
cd spacetime-module/wanderlight-server && dotnet build -c Release && spacetime publish

# Build client
dotnet build wanderlight-online

# Run all automated tests
dotnet test WanderlightOnline.Tests/WanderlightOnline.Tests.csproj

# Run specific test category
dotnet test --filter "FullyQualifiedName~Performance"
```

### Key Metrics to Monitor

| Metric           | Target  | Critical Threshold |
| ---------------- | ------- | ------------------ |
| Join Latency     | < 200ms | < 500ms            |
| Movement Sync    | < 200ms | < 500ms            |
| Inventory Ops    | < 200ms | < 500ms            |
| FPS (10 players) | 60 FPS  | > 30 FPS           |
| Server CPU       | < 50%   | < 80%              |
| Memory           | < 500MB | < 1GB              |

---

## Changelog

### Version 1.0 (Phase 3.5 - Polish)

- Initial manual testing guide created
- Covers all core functionality
- Includes feedback collection process
- Defines success criteria
