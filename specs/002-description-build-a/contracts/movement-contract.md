# Movement Contract

**Integration Point**: LocalPlayerController ↔ PlayerController + NetworkManager  
**Purpose**: Define the interface for player movement input handling and network synchronization  
**Status**: Active  
**Version**: 1.0.0

---

## Overview

This contract specifies the interaction between the LocalPlayerController (client input handling) and the backend movement systems (PlayerController for validation, NetworkManager for synchronization).

---

## Interface Definition

### Client Component

**Class**: `LocalPlayerController.cs`  
**Scene**: `Player.tscn` (CharacterBody2D)  
**Responsibility**: Capture keyboard input, move character with physics, send position updates

### Backend Components

**Classes**:

- `PlayerController.cs` (Phase 3): Movement validation logic
- `NetworkManager.cs` (Phase 3): WebSocket communication to SpacetimeDB

**Responsibility**: Validate movement, broadcast to other players, persist state

---

## Method Signatures

### Client Method: Movement Input

```csharp
// LocalPlayerController.cs
public override void _PhysicsProcess(double delta)
{
    Vector2 inputDir = GetInputDirection();
    Velocity = inputDir.Normalized() * Speed;
    MoveAndSlide();

    // Send to backend (throttled to avoid spam)
    SendPositionUpdate();
}

private Vector2 GetInputDirection()
{
    Vector2 dir = Vector2.Zero;
    if (Input.IsActionPressed("move_up")) dir.Y -= 1;
    if (Input.IsActionPressed("move_down")) dir.Y += 1;
    if (Input.IsActionPressed("move_left")) dir.X -= 1;
    if (Input.IsActionPressed("move_right")) dir.X += 1;
    return dir;
}
```

### Backend Method: TryMove

```csharp
// PlayerController.cs (Phase 3 - already implemented)
public Result<Vector2> TryMove(ulong playerId, Vector2 newPosition)
{
    // Validate position is within world bounds
    // Check for valid movement (no teleporting)
    // Return validated position or error
}
```

### Network Method: SendMovement

```csharp
// NetworkManager.cs (Phase 3 - already implemented)
public void SendMovement(ulong playerId, float x, float y)
{
    // Send position update to SpacetimeDB via WebSocket
    // Updates player table, triggers OnPlayerMoved event
}
```

---

## Events

### OnPlayerMoved (20Hz Updates)

```csharp
// Backend event (triggered by SpacetimeDB subscription)
public event Action<ulong, float, float> OnPlayerMoved;

// Fired when: Any player's position updates in SpacetimeDB
// Frequency: ~20Hz (50ms intervals)
// Parameters:
//   - playerId: Player who moved
//   - x: New X coordinate
//   - y: New Y coordinate
```

**Client Handler**:

```csharp
// GameManager.cs
DatabaseManager.Instance.OnPlayerMoved += (playerId, x, y) =>
{
    if (playerId == LocalPlayerId)
    {
        // Ignore own movements (already applied locally)
        return;
    }

    // Update remote player (enqueue for interpolation)
    remotePlayerUpdates.Enqueue(new PlayerUpdate
    {
        PlayerId = playerId,
        Position = new Vector2(x, y)
    });
};
```

---

## Data Flow

### Input → Movement → Network Sync

```
1. Player presses W key (every frame, 60 FPS)
2. LocalPlayerController._PhysicsProcess():
   - GetInputDirection() detects move_up
   - Calculate velocity: (0, -1) * 200 px/s
   - MoveAndSlide() moves CharacterBody2D (Godot physics)
   - New position: (100, 95) [moved 5 pixels up this frame]
3. SendPositionUpdate() (throttled to 20Hz):
   - Check if 50ms elapsed since last send
   - If yes: NetworkManager.SendMovement(localPlayerId, 100, 95)
4. NetworkManager sends WebSocket message to SpacetimeDB
5. SpacetimeDB updates player table (atomic write)
6. OnPlayerMoved event fires for ALL connected clients
7. Remote clients update their RemotePlayer instances (interpolation)
```

### Throttling Logic (60 FPS → 20Hz)

```csharp
private float lastSendTime = 0.0f;
private const float SendInterval = 0.05f; // 50ms = 20Hz

private void SendPositionUpdate()
{
    float currentTime = (float)GetTree().Root.GetProcessTime();

    if (currentTime - lastSendTime >= SendInterval)
    {
        NetworkManager.Instance.SendMovement(PlayerId, Position.X, Position.Y);
        lastSendTime = currentTime;
    }
}
```

**Why Throttle?**

- Godot runs at 60 FPS (16.6ms per frame)
- Sending 60 updates/sec wastes bandwidth
- SpacetimeDB can handle 20Hz efficiently
- Interpolation makes 20Hz look smooth at 60 FPS

---

## Validation Rules

### Client-Side Validation (Immediate)

- **Collision Detection**: Godot's MoveAndSlide() handles automatically
- **World Bounds**: Client allows movement anywhere (backend enforces bounds)
- **Speed Limit**: Max velocity = Speed property (200 px/s default)

### Backend Validation (Authoritative)

- **World Bounds**: Position must be within defined world area
- **Anti-Cheat**: Detect impossible movement (teleporting, speed hacking)
- **Rate Limiting**: Max 25 updates per second per player

**Validation Example**:

```csharp
// PlayerController.TryMove() (Phase 3 implementation)
public Result<Vector2> TryMove(ulong playerId, Vector2 newPosition)
{
    var player = GetPlayer(playerId);
    if (player == null)
        return Result.Error("Player not found");

    // Check world bounds
    if (newPosition.X < 0 || newPosition.X > WorldWidth ||
        newPosition.Y < 0 || newPosition.Y > WorldHeight)
    {
        return Result.Error("Position out of bounds");
    }

    // Check for teleporting (distance too large)
    float distance = player.Position.DistanceTo(newPosition);
    float maxDistance = MaxSpeed * TimeSinceLastUpdate;
    if (distance > maxDistance * 1.5f) // 50% tolerance
    {
        LogWarning($"Player {playerId} attempted teleport: {distance}px");
        return Result.Error("Invalid movement");
    }

    // Validated - update database
    player.Position = newPosition;
    return Result.Success(newPosition);
}
```

---

## Performance Requirements

| Metric               | Target     | Actual | Notes                              |
| -------------------- | ---------- | ------ | ---------------------------------- |
| Input Latency        | <16ms      | ~1ms   | Godot \_PhysicsProcess immediate   |
| Local Movement       | 60 FPS     | 60 FPS | MoveAndSlide() every physics frame |
| Network Send Rate    | 20Hz       | 20Hz   | Throttled via timer                |
| Network Latency      | <200ms p95 | TBD    | SpacetimeDB WebSocket              |
| Remote Player Update | 20Hz       | 20Hz   | OnPlayerMoved frequency            |
| Interpolation        | 60 FPS     | 60 FPS | Smooth lerp in \_Process           |

---

## Movement Patterns

### Continuous Movement (WASD Held)

```
Frame 0: W pressed
  → Input detected
  → Velocity = (0, -200)
  → Position = (100, 100)
  → Send to network

Frame 1-2: W still pressed (16ms later each)
  → Position = (100, 93.4) [moved 6.6px]
  → Don't send (throttle)

Frame 3: W still pressed (50ms total)
  → Position = (100, 90)
  → Send to network (50ms elapsed)

...repeat every 50ms while W pressed...
```

### Diagonal Movement (W+D Held)

```
Input: move_up AND move_right
  → inputDir = (-1, 1) raw
  → inputDir.Normalized() = (0.707, 0.707)
  → Velocity = (141, 141) [200 * 0.707]
  → Diagonal movement at same speed as cardinal
```

### Stop Movement (Release Keys)

```
Frame 0: W released
  → inputDir = (0, 0)
  → Velocity = (0, 0)
  → Character stops immediately
  → Send final position to network
```

---

## Error Handling

### Network Disconnection

```
Scenario: Player loses internet connection while moving

Client Behavior:
  - Continue local movement (Godot physics still works)
  - SendMovement() calls fail silently (log warning)
  - After 5 seconds, show "Connection Lost" message
  - Disable input, display reconnect dialog

Backend Behavior:
  - Player's last known position is preserved
  - After 30 seconds, mark player as disconnected
  - Remove from active player list (OnPlayerRemoved event)
```

### Position Desync

```
Scenario: Client position differs from server position

Detection:
  - Server sends authoritative position periodically
  - Client compares with local position
  - If difference > 50 pixels, desync detected

Resolution:
  - Client snaps to server position (server is authoritative)
  - Log desync event for debugging
  - Apply smooth correction over 0.5 seconds (not instant teleport)
```

### Lag Spike Handling

```
Scenario: Player experiences 500ms lag spike

Client Behavior:
  - Local movement continues smoothly (no stutter)
  - Outgoing position updates queue up
  - When connection recovers, send latest position only

Backend Behavior:
  - Receives delayed position update
  - Validates against last known good position
  - If valid, apply and broadcast
  - If invalid (too far), reject and send correction
```

---

## Testing Scenarios

### Manual Test 1: Basic Movement

```
GIVEN player spawned in world at (100, 100)
WHEN player presses W key for 1 second
THEN character moves upward smoothly at 60 FPS
  AND final position is approximately (100, -100) [200 px/s * 1s]
  AND position updates sent 20 times (20Hz)
  AND other players see smooth movement
```

### Manual Test 2: Diagonal Movement

```
GIVEN player at (0, 0)
WHEN player holds W+D keys simultaneously
THEN character moves diagonally up-right
  AND speed is same as cardinal direction (200 px/s)
  AND angle is 45 degrees
```

### Manual Test 3: Multi-Player Sync

```
GIVEN two players in game: Alice and Bob
WHEN Alice moves from (100, 100) to (200, 100)
THEN Bob sees Alice's character move smoothly
  AND latency is <200ms (Alice moves → Bob sees)
  AND interpolation makes it look 60 FPS for Bob
```

### Manual Test 4: Collision

```
GIVEN world has solid obstacles (walls)
WHEN player walks into wall
THEN character stops at wall (no clipping)
  AND position sent to network is blocked position
  AND character can slide along wall (Godot physics)
```

### Manual Test 5: Stop and Resume

```
GIVEN player moving right at 200 px/s
WHEN player releases D key
THEN character stops immediately (no inertia)
  AND final position sent to network
WHEN player presses D again
THEN character resumes movement instantly
```

---

## Dependencies

### Client Dependencies

- Godot `CharacterBody2D`: Physics movement
- Godot `InputMap`: Key bindings (move_up, move_down, etc.)
- `NetworkManager.cs`: Position synchronization
- `GameManager.cs`: Update queue processing

### Backend Dependencies

- `PlayerController.cs`: Movement validation
- `NetworkManager.cs`: WebSocket communication
- `SpacetimeDB player table`: Position persistence

---

## Example Implementation

### Client (LocalPlayerController.cs)

```csharp
public partial class LocalPlayerController : CharacterBody2D
{
    [Export] public float Speed = 200.0f;

    public ulong PlayerId { get; private set; }
    private float lastSendTime = 0.0f;
    private const float SendInterval = 0.05f;

    public override void _PhysicsProcess(double delta)
    {
        // Get input direction
        Vector2 inputDir = Vector2.Zero;
        if (Input.IsActionPressed("move_up")) inputDir.Y -= 1;
        if (Input.IsActionPressed("move_down")) inputDir.Y += 1;
        if (Input.IsActionPressed("move_left")) inputDir.X -= 1;
        if (Input.IsActionPressed("move_right")) inputDir.X += 1;

        // Apply movement
        Velocity = inputDir.Normalized() * Speed;
        MoveAndSlide();

        // Send position update (throttled)
        SendPositionUpdate();
    }

    private void SendPositionUpdate()
    {
        float currentTime = (float)Time.GetTicksMsec() / 1000.0f;

        if (currentTime - lastSendTime >= SendInterval)
        {
            NetworkManager.Instance.SendMovement(PlayerId, Position.X, Position.Y);
            lastSendTime = currentTime;
        }
    }
}
```

### Backend (NetworkManager.cs - Phase 3 Reference)

```csharp
// Phase 3 implementation (reference only)
public void SendMovement(ulong playerId, float x, float y)
{
    // Validate
    var validation = PlayerController.Instance.TryMove(playerId, new Vector2(x, y));
    if (!validation.IsSuccess)
    {
        LogWarning($"Invalid movement rejected for player {playerId}");
        return;
    }

    // Update SpacetimeDB
    DatabaseManager.Instance.UpdatePlayerPosition(playerId, x, y);

    // Event fires automatically via SpacetimeDB subscription
}
```

---

## Versioning

**Version**: 1.0.0  
**Breaking Changes**: None (initial version)  
**Migration**: N/A

**Future Enhancements** (out of scope for MVP):

- Client-side prediction (move before server confirms)
- Lag compensation (rewind for hit detection)
- Pathfinding (click-to-move)
- Sprint/dash abilities

---

## Status

- [x] Contract defined
- [x] Backend implementation exists (Phase 3)
- [ ] Client implementation pending (Phase 4.0)
- [ ] Manual testing pending (Phase 4.0)

---

**Last Updated**: October 8, 2025  
**Reviewed By**: Phase 4.0 Planning Process
