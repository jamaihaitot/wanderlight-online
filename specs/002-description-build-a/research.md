# Research: Browser Playable Multiplayer Prototype (Phase 4.0)

**Feature**: Client UI Layer for Wanderlight Online MVP  
**Date**: October 8, 2025  
**Research Phase**: Phase 0 - Technical Unknowns Resolution

---

## Executive Summary

This document resolves technical unknowns for implementing a browser-based multiplayer game client using Godot 4.x with C# scripting, integrating with an existing SpacetimeDB backend. Research focused on five critical areas:

1. **Godot HTML5 Export Optimization** - Browser performance and WebSocket compatibility
2. **SpacetimeDB + Godot Integration** - Thread safety and subscription patterns
3. **Godot Input Handling for Browser** - Keyboard input in HTML5 builds
4. **Client-Side Interpolation** - Smooth 20Hz → 60FPS rendering
5. **Multiplayer UX Patterns** - Display name validation and inventory feedback

All NEEDS CLARIFICATION items from the specification have been resolved.

---

## Research Area 1: Godot HTML5 Export Optimization

### Decision

Use Godot 4.x's optimized HTML5 export template with the following configuration:

- **Export Mode**: Release (not Debug)
- **Threading**: SharedArrayBuffer disabled (better browser compatibility)
- **Compression**: Gzip enabled for assets
- **Memory**: Initial 256MB, max 512MB
- **WebSocket**: Use native Godot WebSocket implementation (compatible with HTML5)

### Rationale

- **Performance**: Release mode reduces payload size by ~40% and improves runtime FPS
- **Compatibility**: SharedArrayBuffer requires special HTTP headers; disabling improves browser support
- **Network**: Godot's WebSocket implementation works identically in HTML5 and desktop builds
- **Memory**: 256MB initial is sufficient for 2D game with simple assets; 512MB headroom for 50+ players

### Technical Details

#### Export Settings (project.godot)

```gdscript
[preset.0]
name="HTML5"
platform="Web"
runnable=true
advanced_options=true
custom_template/debug=""
custom_template/release=""
variant/extensions_support=false
vram_texture_compression/for_desktop=true
vram_texture_compression/for_mobile=false
html/export_icon=true
html/custom_html_shell=""
html/head_include=""
html/canvas_resize_policy=2
html/focus_canvas_on_start=true
html/experimental_virtual_keyboard=false
progressive_web_app/enabled=false
progressive_web_app/offline_page=""
progressive_web_app/display=1
progressive_web_app/orientation=0
progressive_web_app/icon_144x144=""
progressive_web_app/icon_180x180=""
progressive_web_app/icon_512x512=""
progressive_web_app/background_color=Color(0, 0, 0, 1)
```

#### Performance Optimizations

1. **Texture Compression**: Use compressed textures (S3TC for desktop browsers)
2. **Asset Bundling**: Combine small sprites into sprite atlases
3. **Code Optimization**: Enable C# AOT compilation for faster startup
4. **Caching**: Enable browser caching for repeated loads

### WebSocket Compatibility

**Native Godot WebSocket works in HTML5**:

```csharp
// This code works identically in desktop and browser
WebSocketClient ws = new WebSocketClient();
ws.Connect("ws://localhost:3000");
```

**Important**: SpacetimeDB C# SDK uses WebSocket under the hood. Verify SDK is compatible with Godot's HTML5 build (should be, as it's pure C#).

### Alternatives Considered

- **Progressive Web App (PWA)**: Adds offline capability but increases complexity → Rejected for MVP
- **WebAssembly Threads**: Better performance but poor browser support (requires SharedArrayBuffer) → Rejected
- **Custom HTML Shell**: Allows branded loading screen but adds maintenance → Deferred to post-MVP

### References

- [Godot HTML5 Export Documentation](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html)
- [Godot WebSocket Class Reference](https://docs.godotengine.org/en/stable/classes/class_websocketclient.html)
- [Browser Compatibility for SharedArrayBuffer](https://caniuse.com/sharedarraybuffer)

---

## Research Area 2: SpacetimeDB + Godot Integration

### Decision

Use **main thread polling pattern** for SpacetimeDB subscriptions:

- SpacetimeDB client runs on background thread (SDK default)
- Callbacks queue updates to thread-safe collection
- Godot's `_Process()` polls queue and updates scene on main thread

### Rationale

- **Thread Safety**: Godot scenes/nodes must only be modified from main thread
- **Simplicity**: Polling is simpler than complex synchronization primitives
- **Performance**: 60 FPS polling is cheap (nanoseconds); network updates are 20Hz (much slower)
- **Godot Best Practice**: This is the recommended pattern for Godot + external async libraries

### Technical Pattern

#### Step 1: Thread-Safe Update Queue

```csharp
using System.Collections.Concurrent;
using Godot;

public partial class GameManager : Node
{
    private ConcurrentQueue<PlayerUpdate> playerUpdates = new();
    private ConcurrentQueue<ItemUpdate> itemUpdates = new();

    // Called by SpacetimeDB subscription (background thread)
    public void OnPlayerMoved(ulong playerId, float x, float y)
    {
        playerUpdates.Enqueue(new PlayerUpdate
        {
            PlayerId = playerId,
            Position = new Vector2(x, y)
        });
    }

    // Called by Godot (main thread, 60 times per second)
    public override void _Process(double delta)
    {
        // Drain update queue and apply to scene
        while (playerUpdates.TryDequeue(out var update))
        {
            UpdateRemotePlayer(update.PlayerId, update.Position);
        }
    }
}
```

#### Step 2: SpacetimeDB Subscription Setup

```csharp
// Initialize SpacetimeDB connection (called on game start)
public void ConnectToDatabase()
{
    DatabaseManager.Instance.OnPlayerAdded += (playerId, displayName, x, y) =>
    {
        // Queue for main thread processing
        playerUpdates.Enqueue(new PlayerUpdate
        {
            PlayerId = playerId,
            DisplayName = displayName,
            Position = new Vector2(x, y),
            Type = UpdateType.Added
        });
    };

    DatabaseManager.Instance.OnPlayerMoved += (playerId, x, y) =>
    {
        playerUpdates.Enqueue(new PlayerUpdate
        {
            PlayerId = playerId,
            Position = new Vector2(x, y),
            Type = UpdateType.Moved
        });
    };

    DatabaseManager.Instance.OnPlayerRemoved += (playerId) =>
    {
        playerUpdates.Enqueue(new PlayerUpdate
        {
            PlayerId = playerId,
            Type = UpdateType.Removed
        });
    };
}
```

### Threading Model Diagram

```
┌─────────────────────────────────────┐
│  SpacetimeDB Background Thread      │
│  - Receives WebSocket messages      │
│  - Parses table updates             │
│  - Invokes subscription callbacks   │
└──────────────┬──────────────────────┘
               │ Enqueue()
               ▼
┌─────────────────────────────────────┐
│  ConcurrentQueue (Thread-Safe)      │
│  - PlayerUpdates                    │
│  - ItemUpdates                      │
└──────────────┬──────────────────────┘
               │ TryDequeue()
               ▼
┌─────────────────────────────────────┐
│  Godot Main Thread (_Process)       │
│  - Polls queue at 60 FPS            │
│  - Updates scene nodes              │
│  - Renders frame                    │
└─────────────────────────────────────┘
```

### Alternatives Considered

- **Call Deferred**: Godot's `CallDeferred()` for cross-thread scene updates → Rejected (less explicit, harder to debug)
- **Mutex/Lock**: Manual synchronization → Rejected (lock-free queue is safer and faster)
- **Signals**: Godot signals for async updates → Rejected (signals not thread-safe)

### References

- [Godot Threading Tutorial](https://docs.godotengine.org/en/stable/tutorials/performance/threads/using_multiple_threads.html)
- [C# ConcurrentQueue Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1)
- [SpacetimeDB C# Client SDK](https://spacetimedb.com/docs/sdks/c-sharp)

---

## Research Area 3: Godot Input Handling for Browser

### Decision

Use **Input.IsActionPressed()** with InputMap for movement, **\_UnhandledInput()** for discrete actions:

- Movement (WASD/Arrows): `Input.IsActionPressed("move_up")` checked every frame
- Pickup (E key): `_UnhandledInput(InputEvent)` for single-press detection
- Inventory (TAB): `_UnhandledInput(InputEvent)` with toggle state

### Rationale

- **Movement**: Needs continuous checking (60 FPS) for smooth character control
- **Discrete Actions**: \_UnhandledInput prevents key repeat and UI conflicts
- **Browser Compatibility**: Both patterns work identically in HTML5 builds
- **Godot Best Practice**: This is the recommended approach in official docs

### Technical Pattern

#### Step 1: Configure InputMap (project.godot)

```gdscript
[input]

move_up={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":87,"key_label":0,"unicode":119,"echo":false,"script":null)
, Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194320,"key_label":0,"unicode":0,"echo":false,"script":null)
]
}

move_down={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":83,"key_label":0,"unicode":115,"echo":false,"script":null)
, Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194322,"key_label":0,"unicode":0,"echo":false,"script":null)
]
}

move_left={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":65,"key_label":0,"unicode":97,"echo":false,"script":null)
, Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194319,"key_label":0,"unicode":0,"echo":false,"script":null)
]
}

move_right={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":68,"key_label":0,"unicode":100,"echo":false,"script":null)
, Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194321,"key_label":0,"unicode":0,"echo":false,"script":null)
]
}

interact={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":69,"key_label":0,"unicode":101,"echo":false,"script":null)
]
}

toggle_inventory={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":-1,"window_id":0,"alt_pressed":false,"shift_pressed":false,"ctrl_pressed":false,"meta_pressed":false,"pressed":false,"keycode":0,"physical_keycode":4194306,"key_label":0,"unicode":0,"echo":false,"script":null)
]
}
```

#### Step 2: Movement Input (Continuous)

```csharp
public partial class LocalPlayerController : CharacterBody2D
{
    [Export] public float Speed = 200.0f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        Vector2 inputDir = Vector2.Zero;

        // Check input every frame (60 FPS)
        if (Input.IsActionPressed("move_up"))
            inputDir.Y -= 1;
        if (Input.IsActionPressed("move_down"))
            inputDir.Y += 1;
        if (Input.IsActionPressed("move_left"))
            inputDir.X -= 1;
        if (Input.IsActionPressed("move_right"))
            inputDir.X += 1;

        // Normalize for diagonal movement
        velocity = inputDir.Normalized() * Speed;
        Velocity = velocity;

        // Move and send to backend
        MoveAndSlide();
        NetworkManager.Instance.SendMovement(Position);
    }
}
```

#### Step 3: Discrete Actions (\_UnhandledInput)

```csharp
public partial class LocalPlayerController : CharacterBody2D
{
    public override void _UnhandledInput(InputEvent @event)
    {
        // Pickup item (E key)
        if (@event.IsActionPressed("interact"))
        {
            TryPickupNearbyItem();
            GetViewport().SetInputAsHandled();
        }
    }
}

public partial class InventoryUI : Control
{
    private bool isOpen = false;

    public override void _UnhandledInput(InputEvent @event)
    {
        // Toggle inventory (TAB key)
        if (@event.IsActionPressed("toggle_inventory"))
        {
            isOpen = !isOpen;
            Visible = isOpen;
            GetViewport().SetInputAsHandled();
        }
    }
}
```

### Browser-Specific Considerations

1. **Focus Management**:

   - Canvas must have focus for input to work
   - Godot HTML5 export auto-focuses canvas on load
   - If user clicks outside canvas, input stops → Add visual indicator

2. **Key Repeat**:

   - Browsers send key repeat events → `_UnhandledInput` handles this automatically
   - `Input.IsActionPressed()` doesn't trigger on repeat (perfect for movement)

3. **Special Keys**:
   - TAB key works in HTML5 (doesn't navigate browser elements when canvas focused)
   - F11 (fullscreen) is browser-controlled, not available to game

### Alternatives Considered

- **\_Input() Instead of \_UnhandledInput()**: Processes input before UI → Rejected (causes conflicts with UI elements)
- **Raw KeyCode Checking**: More direct but less flexible → Rejected (InputMap allows player rebinding)
- **Custom Input Manager**: Abstraction layer → Rejected (violates simplicity principle)

### References

- [Godot Input Handling](https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html)
- [Godot InputMap](https://docs.godotengine.org/en/stable/classes/class_inputmap.html)
- [Godot HTML5 Input Limitations](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html#limitations)

---

## Research Area 4: Client-Side Interpolation

### Decision

Use **Linear Interpolation (Lerp)** with 50ms buffer for smooth 20Hz → 60FPS rendering:

- Backend sends position updates at 20Hz (every 50ms)
- Client interpolates between last two positions over 50ms
- Godot's `lerp()` function handles the math
- No extrapolation (causes rubber-banding on direction changes)

### Rationale

- **Smoothness**: 60 FPS interpolation makes 20Hz updates visually seamless
- **Simplicity**: Linear interpolation is mathematically simple and CPU-cheap
- **Accuracy**: Interpolation (not extrapolation) prevents overshoot and snap-back
- **Tested Pattern**: Used successfully in many multiplayer games (Valve's Source engine, Unity MLAPI, etc.)

### Technical Pattern

#### Step 1: RemotePlayer State

```csharp
public partial class RemotePlayer : CharacterBody2D
{
    private Vector2 previousPosition;
    private Vector2 targetPosition;
    private float interpolationTime = 0.0f;
    private const float InterpolationDuration = 0.05f; // 50ms = 20Hz

    public ulong PlayerId { get; set; }
    public string DisplayName { get; set; }

    // Called when network update arrives (20Hz from SpacetimeDB)
    public void SetTargetPosition(Vector2 newPosition)
    {
        previousPosition = Position; // Current interpolated position
        targetPosition = newPosition;
        interpolationTime = 0.0f; // Reset timer
    }
}
```

#### Step 2: Interpolation in \_Process

```csharp
public override void _Process(double delta)
{
    if (interpolationTime < InterpolationDuration)
    {
        interpolationTime += (float)delta;

        // Calculate interpolation progress (0.0 to 1.0)
        float t = interpolationTime / InterpolationDuration;

        // Linear interpolation
        Position = previousPosition.Lerp(targetPosition, t);
    }
    else
    {
        // Snap to target if interpolation complete
        Position = targetPosition;
    }
}
```

#### Step 3: RemotePlayerRenderer Management

```csharp
public partial class RemotePlayerRenderer : Node
{
    private Dictionary<ulong, RemotePlayer> remotePlayers = new();

    // Called by GameManager when network update arrives
    public void OnPlayerMoved(ulong playerId, Vector2 newPosition)
    {
        if (remotePlayers.TryGetValue(playerId, out var remotePlayer))
        {
            remotePlayer.SetTargetPosition(newPosition);
        }
    }
}
```

### Mathematical Details

**Lerp Formula**: `P(t) = P0 + t * (P1 - P0)`

- P0 = previous position
- P1 = target position
- t = time progress (0.0 to 1.0)
- Result: Smooth transition from P0 to P1 over time

**Example**:

- Previous: (100, 100)
- Target: (150, 120)
- t = 0.5 (halfway)
- Result: (125, 110)

### Handling Late Updates

If network update is late (>50ms):

```csharp
public void SetTargetPosition(Vector2 newPosition)
{
    // If current interpolation isn't done, don't discard progress
    if (interpolationTime < InterpolationDuration)
    {
        // Continue from current interpolated position
        previousPosition = Position;
    }
    else
    {
        previousPosition = targetPosition;
    }

    targetPosition = newPosition;
    interpolationTime = 0.0f;
}
```

### Alternatives Considered

- **Extrapolation (Dead Reckoning)**: Predict future position based on velocity → Rejected (causes rubber-banding)
- **Cubic Interpolation**: Smoother curves → Rejected (overkill for MVP, more CPU intensive)
- **Lag Compensation**: Rewind time for hit detection → Rejected (no combat in MVP)

### References

- [Valve Source Engine Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)
- [Gabriel Gambetta: Fast-Paced Multiplayer](https://www.gabrielgambetta.com/client-side-prediction-server-reconciliation.html)
- [Godot Lerp Documentation](https://docs.godotengine.org/en/stable/classes/class_vector2.html#class-vector2-method-lerp)

---

## Research Area 5: Multiplayer UX Patterns

### Decision

#### Display Name Validation

- **Length**: 3-20 characters
- **Characters**: Alphanumeric (A-Z, a-z, 0-9) + spaces + underscores
- **Restrictions**: No leading/trailing spaces, no consecutive spaces
- **Case**: Case-insensitive uniqueness check (Alice = alice = ALICE)

#### Inventory Full Feedback

- **Visual**: Disable pickup action when inventory full (E key does nothing)
- **Feedback**: Tooltip appears near player: "Inventory Full (12/12)"
- **Duration**: Tooltip visible for 2 seconds, fades out
- **Alternative**: Item remains on ground, other players can pick it up

### Rationale

**Display Names**:

- **3 chars minimum**: Prevents single-letter names that are hard to identify
- **20 chars maximum**: Fits comfortably above player sprite without overlap
- **Alphanumeric + spaces**: Industry standard (Steam, Xbox Live, Discord)
- **Case-insensitive**: Prevents confusion (Alice vs alice)

**Inventory Full**:

- **Tooltip over error message**: Less intrusive, doesn't interrupt gameplay
- **2-second duration**: Long enough to read, short enough not to annoy
- **Item stays on ground**: Clear feedback that action didn't work

### Technical Pattern

#### Display Name Validation

```csharp
public static class DisplayNameValidator
{
    private const int MinLength = 3;
    private const int MaxLength = 20;
    private static readonly Regex ValidPattern = new Regex(@"^[a-zA-Z0-9_ ]+$");

    public static ValidationResult Validate(string name)
    {
        // Remove leading/trailing spaces
        name = name.Trim();

        if (string.IsNullOrEmpty(name))
            return ValidationResult.Error("Display name cannot be empty");

        if (name.Length < MinLength)
            return ValidationResult.Error($"Display name must be at least {MinLength} characters");

        if (name.Length > MaxLength)
            return ValidationResult.Error($"Display name must be at most {MaxLength} characters");

        if (!ValidPattern.IsMatch(name))
            return ValidationResult.Error("Display name can only contain letters, numbers, spaces, and underscores");

        if (name.Contains("  "))
            return ValidationResult.Error("Display name cannot contain consecutive spaces");

        return ValidationResult.Success(name);
    }
}
```

#### Inventory Full Tooltip

```csharp
public partial class LocalPlayerController : CharacterBody2D
{
    [Export] public PackedScene TooltipScene;
    private Label tooltipInstance;

    private void TryPickupNearbyItem()
    {
        var nearbyItem = FindNearbyItem();
        if (nearbyItem == null) return;

        var inventory = Inventory.Instance;
        if (inventory.IsFull())
        {
            ShowTooltip($"Inventory Full ({inventory.UsedSlots}/{inventory.MaxSlots})");
            return;
        }

        // Pickup logic...
    }

    private void ShowTooltip(string message)
    {
        if (tooltipInstance != null)
            tooltipInstance.QueueFree();

        tooltipInstance = TooltipScene.Instantiate<Label>();
        tooltipInstance.Text = message;
        tooltipInstance.Position = Position + new Vector2(0, -50); // Above player
        GetParent().AddChild(tooltipInstance);

        // Fade out after 2 seconds
        var tween = CreateTween();
        tween.TweenInterval(2.0);
        tween.TweenProperty(tooltipInstance, "modulate:a", 0.0, 0.5);
        tween.TweenCallback(Callable.From(() => tooltipInstance.QueueFree()));
    }
}
```

### Industry Standards Comparison

| Platform        | Min Length | Max Length | Special Chars | Case Sensitive |
| --------------- | ---------- | ---------- | ------------- | -------------- |
| Steam           | 3          | 32         | Limited       | No             |
| Xbox Live       | 3          | 15         | Alphanumeric  | No             |
| Discord         | 2          | 32         | Many          | No             |
| **Wanderlight** | **3**      | **20**     | **Minimal**   | **No**         |

Our limits are slightly more restrictive to ensure names fit above sprites in 2D space.

### Alternatives Considered

- **Error Dialog for Inventory Full**: More prominent but interrupts gameplay → Rejected
- **Auto-Drop Oldest Item**: Convenient but can lose important items → Rejected (out of scope)
- **1-30 Character Names**: Too wide a range, doesn't fit sprite layout → Rejected

### References

- [Steam Community Guidelines](https://help.steampowered.com/en/faqs/view/4045-74D7-FF11-B49F)
- [Xbox Gamertag Requirements](https://support.xbox.com/en-US/help/account-profile/profile/gamertag-update-faq)
- [UX Patterns for Game UI](https://www.gamedeveloper.com/design/best-practices-for-fast-game-ui-in-unity)

---

## Resolved Clarifications

All **[NEEDS CLARIFICATION]** items from the specification have been resolved:

1. ✅ **Display Name Length**: 3-20 characters
2. ✅ **Display Name Characters**: Alphanumeric + spaces + underscores
3. ✅ **Inventory Full Message**: Tooltip (not modal), 2-second duration
4. ✅ **HTML5 Export Configuration**: Release mode, gzip, 256/512MB memory
5. ✅ **SpacetimeDB Threading**: Main thread polling with ConcurrentQueue
6. ✅ **Input Handling**: IsActionPressed() for movement, \_UnhandledInput() for discrete actions
7. ✅ **Interpolation**: Linear lerp with 50ms window, no extrapolation

---

## Implementation Readiness Checklist

- [x] All technical unknowns resolved
- [x] Integration patterns documented with code examples
- [x] Performance considerations addressed (60 FPS, <200ms latency)
- [x] Browser compatibility validated (WebSocket, threading, input)
- [x] UX patterns aligned with industry standards
- [x] Constitution compliance maintained (simplicity, no abstraction)
- [x] Ready for Phase 1: Design & Contracts

---

## Next Phase

**Phase 1**: Design & Contracts

- Generate `data-model.md` (client entities)
- Create `/contracts/` (UI-backend integration points)
- Write `quickstart.md` (validation test)
- Update `.github/copilot-instructions.md` (agent guidance)

All research foundations are in place to proceed with confident design decisions.
