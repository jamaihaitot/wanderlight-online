# Tasks: Browser Playable Multiplayer Prototype (Phase 4.0)

**Input**: Design documents from `specs/002-description-build-a/`
**Prerequisites**: plan.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

## Execution Flow (main)

```
1. ✅ Load plan.md from feature directory
   → Tech stack: Godot 4.x + C# + SpacetimeDB
   → Structure: Scenes/, Scripts/Client/, Assets/
2. ✅ Load optional design documents:
   → data-model.md: 7 entities (LocalPlayer, RemotePlayer, WorldItem, etc.)
   → contracts/: 4 files (login, movement, inventory, item-interaction)
   → research.md: HTML5 export, threading, interpolation decisions
3. ✅ Generate tasks by category:
   → Setup: Godot scenes, project structure
   → Tests: 4 contract tests (manual validation)
   → Core: 6 client scripts, scene configurations
   → Integration: Backend wiring, event subscriptions
   → Polish: HTML5 export, optimization, validation
4. ✅ Apply task rules:
   → Different scenes/scripts = [P] for parallel
   → Same file edits = sequential
   → Manual tests before implementation
5. ✅ Number tasks sequentially (T001-T035)
6. ✅ Generate dependency graph
7. ✅ Create parallel execution examples
8. ✅ Validate task completeness
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions
- Manual testing approach (game engine project)

## Path Conventions

This is a Godot game engine project with the following structure:

- **Scenes**: `wanderlight-online/Scenes/` (Godot .tscn files)
- **Scripts**: `wanderlight-online/Scripts/Client/` (C# UI controllers)
- **Backend**: `wanderlight-online/Scripts/` (Phase 3 - already complete)
- **Assets**: `wanderlight-online/Assets/` (sprites, fonts)
- **Tests**: Manual validation using `MANUAL_TESTING_GUIDE.md`

---

## Phase 3.1: Setup & Project Structure

### T001 Create project directory structure

**Path**: `wanderlight-online/`
**Action**: Create missing directories for Phase 4 organization

```
wanderlight-online/
├── Scenes/
│   └── UI/
├── Scripts/
│   └── Client/
└── Assets/
    ├── Sprites/
    └── Fonts/
```

**Validation**: All directories exist and are tracked in git

---

### T002 [P] Configure Godot input actions

**Path**: `wanderlight-online/project.godot`
**Action**: Define input action mappings for movement and interactions

```gdscript
[input]
move_up={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":0,"window_id":0,"pressed":false,"keycode":87,"physical_keycode":0,"unicode":0,"echo":false,"script":null), Object(InputEventKey,"resource_local_to_scene":false,"resource_name":"","device":0,"window_id":0,"pressed":false,"keycode":4194320,"physical_keycode":0,"unicode":0,"echo":false,"script":null)]
}
move_down={...}  # Similar for S/Down
move_left={...}  # Similar for A/Left
move_right={...} # Similar for D/Right
pickup_item={...} # E key
toggle_inventory={...} # TAB key
```

**Validation**: Input actions appear in Project → Project Settings → Input Map

---

### T003 [P] Create placeholder sprite assets

**Path**: `wanderlight-online/Assets/Sprites/`
**Action**: Generate simple placeholder graphics for MVP

- `player_circle.png` (32x32px blue circle for player)
- `item_wood.png` (16x16px brown square)
- `item_stone.png` (16x16px gray square)
- `item_metal.png` (16x16px silver square)
- `background_grass.png` (64x64px green texture)

**Tool**: Use Godot's built-in texture generator or simple image editor
**Validation**: Files exist and import successfully in Godot (check .import files)

---

## Phase 3.2: Scenes (Core Components)

### T004 [P] Create Player scene (LocalPlayer)

**Path**: `wanderlight-online/Scenes/Player.tscn`
**Action**: Build player character prefab for local player control
**Structure**:

```
Player (CharacterBody2D)
├── Sprite2D (texture: player_circle.png)
├── CollisionShape2D (CircleShape2D, radius: 16)
├── DisplayName (Label, offset above player)
└── PickupArea (Area2D)
    └── PickupCollision (CollisionShape2D, CircleShape2D, radius: 50)
```

**Properties**:

- Motion Mode: Floating
- Script: (attach later in T010)

**Validation**:

- Scene opens without errors
- Hierarchy matches structure above
- Can be instantiated in test scene

---

### T005 [P] Create RemotePlayer scene

**Path**: `wanderlight-online/Scenes/RemotePlayer.tscn`
**Action**: Build remote player prefab for network-controlled players
**Structure**:

```
RemotePlayer (CharacterBody2D)
├── Sprite2D (texture: player_circle.png, modulate: Color(0.7, 0.7, 1.0))
├── CollisionShape2D (CircleShape2D, radius: 16)
└── DisplayName (Label)
```

**Properties**:

- Motion Mode: Floating
- Collision Layer: 2 (different from local player)
- Script: (attach later in T011)

**Validation**:

- Scene opens without errors
- Visually distinct from Player.tscn (blue tint)
- Can be instantiated multiple times

---

### T006 [P] Create WorldItem scene

**Path**: `wanderlight-online/Scenes/WorldItem.tscn`
**Action**: Build item prefab for items on ground
**Structure**:

```
WorldItemInstance (Node2D)
├── Sprite2D (texture: item_wood.png, will be set dynamically)
├── PickupArea (Area2D)
│   └── CollisionShape2D (CircleShape2D, radius: 8)
└── QuantityLabel (Label, shows stack count)
```

**Properties**:

- Z Index: -1 (render below players)
- Script: (attach later in T013)

**Validation**:

- Scene opens without errors
- Area2D detects player proximity
- Label positioned correctly

---

### T007 [P] Create InventoryPanel scene

**Path**: `wanderlight-online/Scenes/UI/InventoryPanel.tscn`
**Action**: Build inventory UI with 12-slot grid
**Structure**:

```
InventoryPanel (Panel)
├── VBoxContainer
│   ├── TitleBar (HBoxContainer)
│   │   ├── TitleLabel (Label: "Inventory")
│   │   └── CloseButton (Button: "X")
│   └── SlotGrid (GridContainer, columns: 4)
│       ├── Slot0 (Button with TextureRect + Label)
│       ├── Slot1 (Button)
│       ├── ... (10 more slots)
│       └── Slot11 (Button)
```

**Properties**:

- Panel size: 400x300
- Initially hidden (visible = false)
- GridContainer: 4 columns, 3 rows, 10px separation
- Each slot: 80x80px Button with icon + quantity label

**Validation**:

- Panel displays correctly when visible
- Grid layout is 4x3
- Buttons respond to mouse clicks

---

### T008 Create MainMenu scene

**Path**: `wanderlight-online/Scenes/MainMenu.tscn`
**Action**: Build login screen UI
**Structure**:

```
MainMenu (Control)
└── CenterContainer
    └── VBoxContainer
        ├── TitleLabel (Label: "Wanderlight Online")
        ├── NameInput (LineEdit, placeholder: "Display Name")
        ├── JoinButton (Button: "Join Game")
        └── ErrorLabel (Label, initially hidden, for error messages)
```

**Properties**:

- Control anchors: Full Rect (0,0,1,1)
- VBoxContainer: separation 20px
- LineEdit: max_length 20, placeholder_text
- ErrorLabel: red color, initially hidden

**Validation**:

- UI centered on screen
- LineEdit accepts input
- Button clickable
- Can run scene independently (F6 in Godot)

---

### T009 Create GameWorld scene

**Path**: `wanderlight-online/Scenes/GameWorld.tscn`
**Action**: Build main game scene with camera and spawn point
**Structure**:

```
GameWorld (Node2D)
├── Background (Sprite2D, texture: background_grass.png, tiled)
├── PlayerSpawn (Marker2D, position: 0,0)
├── Camera (Camera2D)
│   └── (enabled, zoom: 2.0)
└── CanvasLayer
    └── InventoryPanel (instance of UI/InventoryPanel.tscn)
```

**Properties**:

- Background: tiled texture covering large area
- Camera: smooth follow (will be scripted in T015)
- InventoryPanel: initially hidden

**Dependencies**: Requires T007 (InventoryPanel.tscn)
**Validation**:

- Scene loads without errors
- Camera is active
- Background renders
- Can run scene independently

---

## Phase 3.3: Manual Test Scenarios (BEFORE Implementation)

### T010 [P] Manual test: Login flow validation

**Path**: `MANUAL_TESTING_GUIDE.md` (Scenario 1: Dual Player Join)
**Action**: Document expected behavior for login contract testing
**Test Steps**:

1. Open MainMenu scene (T008)
2. Try empty name → expect error "Name required"
3. Try short name "ab" → expect error "3-20 characters"
4. Try long name (21+ chars) → expect error "3-20 characters"
5. Try valid name "Player1" → expect transition to GameWorld
6. Open second tab, try "Player1" again → expect "Name already taken"
7. Try "player1" (lowercase) → expect "Name already taken" (case-insensitive)

**Expected Result**: TESTS FAIL (UI not implemented yet)
**Purpose**: Define acceptance criteria before implementation

---

### T011 [P] Manual test: Movement synchronization

**Path**: `MANUAL_TESTING_GUIDE.md` (Scenario 2: Movement Synchronization)
**Action**: Document expected behavior for movement contract testing
**Test Steps**:

1. Join as "Player1" in tab 1
2. Join as "Player2" in tab 2
3. Move Player1 with WASD → expect smooth 60 FPS movement
4. In tab 2, observe Player1's RemotePlayer → expect position updates <200ms
5. Move both players simultaneously → expect both visible, no collision
6. Measure latency (console timestamps) → expect <200ms p95

**Expected Result**: TESTS FAIL (scripts not implemented yet)
**Purpose**: Define performance and interpolation requirements

---

### T012 [P] Manual test: Item pickup/drop

**Path**: `MANUAL_TESTING_GUIDE.md` (Scenario 3: Item Interaction)
**Action**: Document expected behavior for item-interaction contract testing
**Test Steps**:

1. Spawn item via debug command (backend already supports)
2. Walk Player1 near item → expect highlight/glow effect
3. Press E key → expect item disappear, inventory opens with item
4. In tab 2 (Player2), confirm item vanished
5. Player1 clicks item in inventory → expect item appears at player position
6. Player2 walks to item, presses E → expect Player2 picks it up

**Expected Result**: TESTS FAIL (rendering and input handling not implemented)
**Purpose**: Define atomic pickup and visual feedback requirements

---

### T013 [P] Manual test: Inventory management

**Path**: `MANUAL_TESTING_GUIDE.md` (Scenario 4: Inventory Full)
**Action**: Document expected behavior for inventory contract testing
**Test Steps**:

1. Pick up 12 different item types (fill all slots)
2. Try to pick up 13th item → expect message "Inventory full"
3. Drop one item (click slot) → expect item appears on ground
4. Pick up item again → expect it fills empty slot
5. Pick up same type item → expect stacking (quantity increases)
6. Disconnect and rejoin → expect inventory persisted

**Expected Result**: TESTS FAIL (UI not connected to backend)
**Purpose**: Define full-inventory handling and persistence validation

---

## Phase 3.4: Client Scripts (Implementation - ONLY after manual test scenarios documented)

### T014 [P] Implement LocalPlayerController script

**Path**: `wanderlight-online/Scripts/Client/LocalPlayerController.cs`
**Action**: Create player input handling and movement controller
**Contract**: See `contracts/movement-contract.md`
**Key Methods**:

```csharp
public partial class LocalPlayerController : CharacterBody2D
{
    [Export] public float Speed = 200.0f;
    public ulong PlayerId { get; set; }
    public string DisplayName { get; set; }

    private float networkUpdateTimer = 0.0f;
    private const float NETWORK_UPDATE_INTERVAL = 0.05f; // 20Hz

    public override void _PhysicsProcess(double delta)
    {
        // Get input direction from Input.IsActionPressed
        // Calculate velocity
        // MoveAndSlide()
        // Throttled network update (20Hz)
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Handle pickup (E key) via PickupArea
        // Handle inventory toggle (TAB key)
    }
}
```

**Integration**:

- Call `PlayerController.TryMove()` for validation
- Call `NetworkManager.SendMovement()` for sync
- Subscribe to `Inventory.OnInventoryChanged` for UI updates

**Validation**:

- Attach script to Player.tscn (T004)
- Run GameWorld scene → player moves with WASD
- Console logs network updates at 20Hz

---

### T015 [P] Implement RemotePlayerRenderer script

**Path**: `wanderlight-online/Scripts/Client/RemotePlayerRenderer.cs`
**Action**: Create remote player position interpolation renderer
**Contract**: See `contracts/movement-contract.md`
**Key Methods**:

```csharp
public partial class RemotePlayerRenderer : CharacterBody2D
{
    public ulong PlayerId { get; set; }
    public string DisplayName { get; set; }

    private Vector2 targetPosition;
    private Vector2 previousPosition;
    private float interpolationTime = 0.0f;
    private const float INTERPOLATION_DURATION = 0.05f; // 50ms window

    public override void _Process(double delta)
    {
        // Linear interpolation from previous to target
        // Update position smoothly at 60 FPS
    }

    public void OnPositionUpdate(float x, float y)
    {
        // Called by GameManager from SpacetimeDB subscription
        // Store previous, set new target, reset interpolation timer
    }
}
```

**Integration**:

- Subscribe to `DatabaseManager.OnPlayerMoved` event
- Spawn/despawn on `OnPlayerAdded`/`OnPlayerRemoved`

**Validation**:

- Attach script to RemotePlayer.tscn (T005)
- Run two tabs → see smooth interpolation, no jitter

---

### T016 [P] Implement WorldItemRenderer script

**Path**: `wanderlight-online/Scripts/Client/WorldItemRenderer.cs`
**Action**: Create world item spawning and pickup interaction
**Contract**: See `contracts/item-interaction-contract.md`
**Key Methods**:

```csharp
public partial class WorldItemRenderer : Node2D
{
    public uint ItemId { get; set; }
    public ItemCategory ItemType { get; set; }
    public uint Quantity { get; set; }

    private Area2D pickupArea;
    private bool isPlayerNearby = false;

    public override void _Ready()
    {
        // Set sprite texture based on ItemType
        // Setup Area2D signals for proximity detection
    }

    public override void _Process(double delta)
    {
        // Highlight effect when player nearby
    }

    private void OnPickupAttempt()
    {
        // Called by LocalPlayerController (E key press)
        // Call WorldItem.TryPickup()
    }
}
```

**Integration**:

- Subscribe to `DatabaseManager.OnItemSpawned` for spawning
- Subscribe to `DatabaseManager.OnItemDespawned` for removal
- Call `WorldItem.TryPickup()` on player interaction

**Validation**:

- Run GameWorld → items spawn at correct positions
- Walk near item → see highlight
- Press E → item disappears, inventory updates

---

### T017 [P] Implement InventoryUI script

**Path**: `wanderlight-online/Scripts/Client/InventoryUI.cs`
**Action**: Create inventory display and click handling
**Contract**: See `contracts/inventory-contract.md`
**Key Methods**:

```csharp
public partial class InventoryUI : Panel
{
    private Button[] slotButtons = new Button[12];
    private Inventory inventory;

    public override void _Ready()
    {
        // Get references to all slot buttons
        // Subscribe to Inventory.OnInventoryChanged
        // Setup click handlers for each slot
        inventory = Inventory.Instance;
        inventory.OnInventoryChanged += RefreshUI;
    }

    private void RefreshUI()
    {
        // Get all slots from Inventory.GetSlots()
        // Update each button's icon and quantity label
    }

    private void OnSlotClicked(int slotIndex)
    {
        // Call Inventory.RemoveItem(slotIndex)
        // Drop item via WorldItem.Drop() at player position
    }

    public void Toggle()
    {
        // Show/hide panel
        Visible = !Visible;
    }
}
```

**Integration**:

- Subscribe to `Inventory.OnInventoryChanged` event
- Call `Inventory.RemoveItem()` on slot click
- Call `WorldItem.Drop()` to spawn dropped items

**Validation**:

- Press TAB → inventory opens
- Slots show correct items with quantities
- Click slot → item drops to world

---

### T018 [P] Implement MainMenuController script

**Path**: `wanderlight-online/Scripts/Client/MainMenuController.cs`
**Action**: Create login screen input validation and join logic
**Contract**: See `contracts/login-contract.md`
**Key Methods**:

```csharp
public partial class MainMenuController : Control
{
    private LineEdit nameInput;
    private Button joinButton;
    private Label errorLabel;

    public override void _Ready()
    {
        // Get node references
        // Connect button signal to OnJoinPressed
        joinButton.Pressed += OnJoinPressed;
    }

    private void OnJoinPressed()
    {
        string displayName = nameInput.Text.Trim();

        // Client-side validation (3-20 chars, alphanumeric + spaces)
        if (!ValidateName(displayName))
        {
            ShowError("Name must be 3-20 characters (letters, numbers, spaces)");
            return;
        }

        // Call backend PlayerManager.TryAddPlayer()
        var result = PlayerManager.Instance.TryAddPlayer(displayName);

        if (result.IsSuccess)
        {
            // Transition to GameWorld scene
            GetTree().ChangeSceneToFile("res://Scenes/GameWorld.tscn");
        }
        else
        {
            ShowError(result.Error);
        }
    }

    private bool ValidateName(string name)
    {
        // 3-20 characters, alphanumeric + spaces only
    }

    private void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.Visible = true;
    }
}
```

**Integration**:

- Call `PlayerManager.TryAddPlayer()` on join button
- Handle `Result<PlayerId>` success/error
- Change scene to GameWorld on success

**Validation**:

- Run MainMenu scene → enter name, click Join
- Invalid names show errors
- Valid name transitions to GameWorld
- Duplicate names rejected by backend

---

### T019 Implement GameManager script (Autoload)

**Path**: `wanderlight-online/Scripts/Client/GameManager.cs`
**Action**: Create coordinator singleton for managing game state and network subscriptions
**Key Responsibilities**:

- Manage SpacetimeDB subscriptions (player updates, item spawns)
- Spawn/despawn RemotePlayer instances based on network events
- Spawn/despawn WorldItem instances based on network events
- Coordinate camera following local player
- Handle disconnection/reconnection

**Key Methods**:

```csharp
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    private LocalPlayerController localPlayer;
    private Dictionary<ulong, RemotePlayerRenderer> remotePlayers = new();
    private Dictionary<uint, WorldItemRenderer> worldItems = new();

    public override void _Ready()
    {
        Instance = this;

        // Subscribe to backend events
        PlayerManager.Instance.OnPlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.OnPlayerRemoved += OnPlayerRemoved;
        DatabaseManager.Instance.OnPlayerMoved += OnPlayerMoved;
        DatabaseManager.Instance.OnItemSpawned += OnItemSpawned;
        DatabaseManager.Instance.OnItemDespawned += OnItemDespawned;
    }

    private void OnPlayerAdded(ulong playerId, string displayName, float x, float y)
    {
        if (playerId == localPlayer.PlayerId)
        {
            // This is us, already spawned
            return;
        }

        // Spawn RemotePlayer instance
        var remotePlayer = RemotePlayerScene.Instantiate<RemotePlayerRenderer>();
        remotePlayer.PlayerId = playerId;
        remotePlayer.DisplayName = displayName;
        remotePlayer.Position = new Vector2(x, y);
        GetTree().Root.AddChild(remotePlayer);
        remotePlayers[playerId] = remotePlayer;
    }

    private void OnPlayerMoved(ulong playerId, float x, float y)
    {
        if (remotePlayers.TryGetValue(playerId, out var remotePlayer))
        {
            remotePlayer.OnPositionUpdate(x, y);
        }
    }

    private void OnItemSpawned(uint itemId, ItemCategory itemType, float x, float y, uint quantity)
    {
        // Spawn WorldItem instance
    }

    // ... other event handlers
}
```

**Configuration**:

- Add to Project → Project Settings → Autoload
- Autoload name: "GameManager"
- Path: `res://Scripts/Client/GameManager.cs`

**Dependencies**: Requires all previous scripts (T014-T018)
**Validation**:

- GameManager exists as singleton
- Players spawn/despawn correctly
- Items spawn/despawn correctly
- No memory leaks (despawn removes instances)

---

## Phase 3.5: Integration & Wiring

### T020 Attach scripts to scenes

**Action**: Wire scripts to corresponding scene files
**Changes**:

1. `Scenes/Player.tscn` → Attach `LocalPlayerController.cs` to root
2. `Scenes/RemotePlayer.tscn` → Attach `RemotePlayerRenderer.cs` to root
3. `Scenes/WorldItem.tscn` → Attach `WorldItemRenderer.cs` to root
4. `Scenes/UI/InventoryPanel.tscn` → Attach `InventoryUI.cs` to root Panel
5. `Scenes/MainMenu.tscn` → Attach `MainMenuController.cs` to root Control

**Validation**:

- Open each scene → "Script" property shows attached script
- No "script not found" errors
- Exported properties appear in Inspector

---

### T021 Configure GameManager autoload

**Path**: `wanderlight-online/project.godot`
**Action**: Add GameManager to autoload singletons
**Configuration**:

```gdscript
[autoload]
GameManager="*res://Scripts/Client/GameManager.cs"
```

**Alternative**: Project → Project Settings → Autoload → Add → Select GameManager.cs

**Validation**:

- GameManager appears in Autoload list
- Can access `GameManager.Instance` from any script
- Instance exists before \_Ready() of other nodes

---

### T022 Setup camera follow behavior

**Path**: `wanderlight-online/Scripts/Client/GameManager.cs` (modify existing)
**Action**: Add camera tracking logic for local player
**Code Addition**:

```csharp
// In GameManager.cs
private Camera2D gameCamera;

public void SetLocalPlayer(LocalPlayerController player)
{
    localPlayer = player;

    // Make camera follow local player
    if (gameCamera != null)
    {
        gameCamera.Reparent(player);
        gameCamera.Position = Vector2.Zero; // Center on player
    }
}

public override void _Process(double delta)
{
    // Camera smooth follow (if not child of player)
    if (localPlayer != null && gameCamera != null && gameCamera.GetParent() != localPlayer)
    {
        gameCamera.Position = gameCamera.Position.Lerp(localPlayer.Position, 0.1f);
    }
}
```

**Integration**: Call `SetLocalPlayer()` after spawning local player in GameWorld

**Validation**:

- Camera follows player smoothly
- Player stays centered on screen
- Camera doesn't jitter at 60 FPS

---

### T023 Wire MainMenu to GameWorld transition

**Path**: `wanderlight-online/Scripts/Client/MainMenuController.cs` (modify existing)
**Action**: Ensure proper scene transition with player spawn
**Code Modification**:

```csharp
// In OnJoinPressed() after successful TryAddPlayer
if (result.IsSuccess)
{
    // Store player ID for GameWorld to retrieve
    GameManager.Instance.LocalPlayerId = result.Value;
    GameManager.Instance.LocalPlayerName = displayName;

    // Change scene
    GetTree().ChangeSceneToFile("res://Scenes/GameWorld.tscn");
}
```

**Integration**:

- GameWorld.cs (create if needed) reads LocalPlayerId from GameManager
- Spawns LocalPlayer at PlayerSpawn position
- Calls `GameManager.SetLocalPlayer()`

**Dependencies**: Requires T019 (GameManager), T018 (MainMenuController)
**Validation**:

- Login flow complete
- Player appears in GameWorld at spawn point
- Display name visible above character

---

### T024 Implement SpacetimeDB subscription polling

**Path**: `wanderlight-online/Scripts/Client/GameManager.cs` (modify existing)
**Action**: Add thread-safe polling of SpacetimeDB callbacks
**Code Addition**:

```csharp
// Based on research.md: SpacetimeDB callbacks run on background thread
// Must poll ConcurrentQueue in Godot main thread (_Process)

private ConcurrentQueue<Action> mainThreadQueue = new();

public override void _Process(double delta)
{
    // Poll all queued callbacks from SpacetimeDB background thread
    while (mainThreadQueue.TryDequeue(out var action))
    {
        action.Invoke();
    }

    // ... camera follow logic
}

// Subscribe to SpacetimeDB with thread-safe enqueueing
private void SubscribeToDatabase()
{
    DatabaseManager.Instance.OnPlayerAdded += (playerId, name, x, y) =>
    {
        mainThreadQueue.Enqueue(() => OnPlayerAdded(playerId, name, x, y));
    };

    // ... other subscriptions
}
```

**Rationale**: SpacetimeDB SDK uses background threads. Godot scene updates must happen on main thread. ConcurrentQueue provides thread-safe communication.

**Dependencies**: Requires understanding from `research.md` → "SpacetimeDB + Godot Integration"
**Validation**:

- No thread safety errors in console
- Network updates appear smoothly in UI
- No "called from wrong thread" exceptions

---

## Phase 3.6: HTML5 Export Configuration

### T025 [P] Configure HTML5 export preset

**Path**: `wanderlight-online/export_presets.cfg` (Godot export settings)
**Action**: Create optimized HTML5 export configuration
**Settings** (from `research.md`):

```
[preset.0]
name="HTML5"
platform="Web"
runnable=true
custom_template/release=""
variant/extensions_support=false
vram_texture_compression/for_desktop=true
html/export_icon=true
html/canvas_resize_policy=2
html/focus_canvas_on_start=true
progressive_web_app/enabled=false
```

**Optimization Settings**:

- Export Mode: **Release** (not Debug)
- Threading: SharedArrayBuffer **disabled**
- Compression: Gzip **enabled**
- Initial Memory: **256MB**
- Max Memory: **512MB**

**Validation**:

- Project → Export → HTML5 preset exists
- Test export to `/tmp/` directory
- Generated HTML file opens in browser
- No console errors about memory or CORS

---

### T026 [P] Create HTML5 loading screen

**Path**: `wanderlight-online/Assets/loading.html` (custom HTML shell)
**Action**: Add branded loading screen for browser launch
**Content**:

```html
<!DOCTYPE html>
<html>
  <head>
    <title>Wanderlight Online</title>
    <style>
      body {
        margin: 0;
        background: #1a1a2e;
      }
      #loading {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        color: #eee;
        font-family: Arial, sans-serif;
        text-align: center;
      }
    </style>
  </head>
  <body>
    <div id="loading">
      <h1>Wanderlight Online</h1>
      <p>Loading game...</p>
      <div id="progress"></div>
    </div>
    <canvas id="canvas"></canvas>
    <script src="index.js"></script>
  </body>
</html>
```

**Integration**: Set in Export → HTML5 → Custom HTML Shell

**Validation**:

- Loading screen appears before game loads
- Progress bar shows loading status
- Disappears when game ready

---

### T027 Test HTML5 build locally

**Action**: Export and test game in local browser
**Steps**:

1. Godot → Project → Export → HTML5 → Export Project
2. Save to `build/web/` directory
3. Start local HTTP server: `python -m http.server 8000` (in build/web/)
4. Open browser: `http://localhost:8000`
5. Test all 8 manual testing scenarios

**Expected Results**:

- Game loads without errors (check browser console)
- MainMenu appears with input field
- Can join game and move around
- Network sync works (test with second tab)
- FPS stable at 60 (check with Godot remote debugger)

**Validation Checklist**:

- [ ] HTML5 export completes without errors
- [ ] Game loads in Chrome, Firefox, Edge
- [ ] No console errors in browser DevTools
- [ ] Input handling works (WASD, E, TAB)
- [ ] WebSocket connection establishes to SpacetimeDB
- [ ] Player movement smooth at 60 FPS
- [ ] Remote players visible and interpolated
- [ ] Item pickup/drop works
- [ ] Inventory UI functional

---

## Phase 3.7: Polish & Optimization

### T028 [P] Add performance monitoring overlay

**Path**: `wanderlight-online/Scripts/Client/PerformanceMonitor.cs`
**Action**: Create debug overlay for FPS and latency
**Display Metrics**:

- FPS (Engine.GetFramesPerSecond())
- Network latency (timestamp diff between send/receive)
- Active RemotePlayers count
- Active WorldItems count
- Memory usage (OS.GetStaticMemoryUsage())

**UI**: Label in top-left corner (CanvasLayer, layer 100 for always on top)

**Validation**:

- Overlay shows in HTML5 build
- FPS consistently ~60
- Latency <200ms
- No memory leaks (usage stable over time)

---

### T029 [P] Optimize sprite atlases

**Path**: `wanderlight-online/Assets/Sprites/atlas.png`
**Action**: Combine small sprites into single texture atlas
**Rationale**: Reduce draw calls, faster loading in HTML5

**Process**:

1. Use Godot's built-in atlas packer or external tool (Aseprite, TexturePacker)
2. Combine: player_circle, item_wood, item_stone, item_metal
3. Update scene references to use atlas regions

**Validation**:

- All sprites render correctly
- HTML5 build has fewer network requests
- Faster initial load time

---

### T030 [P] Add connection status indicator

**Path**: `wanderlight-online/Scripts/Client/ConnectionStatusUI.cs`
**Action**: Create UI element showing SpacetimeDB connection state
**States**:

- 🟢 Connected (green dot)
- 🟡 Connecting (yellow dot, pulsing)
- 🔴 Disconnected (red dot + "Reconnecting..." message)

**Position**: Top-right corner, CanvasLayer

**Integration**:

- Subscribe to `NetworkManager.OnConnectionStateChanged` event
- Update UI based on connection state
- Auto-reconnect on disconnect (handled by SpacetimeDB SDK)

**Validation**:

- Indicator shows "Connected" when in-game
- Simulated disconnect (stop server) shows "Reconnecting"
- Successful reconnection shows "Connected" again

---

### T031 Implement error boundaries

**Path**: `wanderlight-online/Scripts/Client/ErrorHandler.cs`
**Action**: Add global error catching and user-friendly messages
**Approach**:

```csharp
public partial class ErrorHandler : Node
{
    public override void _EnterTree()
    {
        // Global exception handler
        GD.PushWarning("ErrorHandler initialized");
    }

    public static void HandleError(Exception ex, string context)
    {
        GD.PrintErr($"Error in {context}: {ex.Message}");
        GD.PrintErr(ex.StackTrace);

        // Show user-friendly message
        ShowErrorDialog($"An error occurred: {ex.Message}. Please try again.");
    }

    private static void ShowErrorDialog(string message)
    {
        // AcceptDialog popup with error message
    }
}
```

**Integration**: Wrap risky operations (network calls, file I/O) in try-catch, call ErrorHandler.HandleError

**Validation**:

- Simulated errors show user-friendly dialogs
- Game doesn't crash on network errors
- All errors logged to console for debugging

---

### T032 Add input validation for edge cases

**Path**: `wanderlight-online/Scripts/Client/MainMenuController.cs` (modify)
**Action**: Strengthen validation for display names
**Additional Checks**:

- No profanity filter (optional, post-MVP)
- Trim whitespace
- Block special characters (only alphanumeric + single spaces)
- Block SQL injection patterns (already safe with SpacetimeDB, but good practice)

**Code**:

```csharp
private bool ValidateName(string name)
{
    if (string.IsNullOrWhiteSpace(name)) return false;
    if (name.Length < 3 || name.Length > 20) return false;

    // Only letters, numbers, single spaces
    var allowedPattern = new Regex(@"^[a-zA-Z0-9]+( [a-zA-Z0-9]+)*$");
    return allowedPattern.IsMatch(name);
}
```

**Validation**:

- "Test Name" → valid
- "Test Name" (double space) → invalid
- "Test_Name" (underscore) → invalid
- "Test123" → valid

---

### T033 Create keyboard shortcut reference

**Path**: `wanderlight-online/Scenes/UI/HelpPanel.tscn`
**Action**: Build in-game help overlay showing controls
**Content**:

```
Controls:
- WASD / Arrow Keys: Move
- E: Pick up item
- TAB: Open/close inventory
- Click item: Drop to ground
- H: Toggle this help menu
```

**Implementation**: Panel (similar to InventoryPanel), toggle with H key

**Validation**:

- Press H → help panel appears
- Press H again → hides
- Panel doesn't block gameplay input

---

## Phase 3.8: Final Validation

### T034 Run all manual test scenarios

**Path**: `MANUAL_TESTING_GUIDE.md`
**Action**: Execute all 8 test scenarios in HTML5 build
**Scenarios** (see quickstart.md for details):

1. ✅ Dual Player Join (concurrent login, duplicate name rejection)
2. ✅ Movement Synchronization (60 FPS, <200ms latency)
3. ✅ Item Interaction (pickup/drop, atomic operations)
4. ✅ Inventory Full (12-slot limit, persistence)
5. ✅ Disconnection and Rejoin (state restoration)
6. ✅ Concurrent Item Pickup (race condition handling)
7. ✅ Player Disconnection (ghost cleanup)
8. ✅ Inventory Persistence (rejoin with items)

**Process**:

1. Start SpacetimeDB server
2. Export HTML5 build (T027)
3. Open 3+ browser tabs
4. Execute each scenario step-by-step
5. Record results (pass/fail, notes)

**Validation Gate**: ALL scenarios must pass before marking Phase 4.0 complete

---

### T035 Update project documentation

**Path**: Multiple files
**Action**: Finalize documentation for Phase 4.0 release
**Files to Update**:

1. `README.md` → Add "How to Play" section with browser link
2. `MANUAL_TESTING_GUIDE.md` → Update with HTML5 testing instructions
3. `specs/002-description-build-a/quickstart.md` → Add actual build output paths
4. `specs/002-description-build-a/spec.md` → Mark all requirements as implemented

**New Files**:

- `docs/DEPLOYMENT.md` → Instructions for hosting HTML5 build (GitHub Pages, itch.io, etc.)
- `docs/ARCHITECTURE.md` → System diagram showing client-backend integration

**Validation**:

- All docs accurate and up-to-date
- New user can follow README to play game
- Deployment guide works (test on clean machine)

---

## Dependencies Graph

```
Setup (T001-T003)
  ├─→ Scenes (T004-T009) [mostly parallel]
  │     └─→ Manual Tests (T010-T013) [parallel]
  │           └─→ Scripts (T014-T019) [mostly parallel]
  │                 └─→ Integration (T020-T024) [sequential]
  │                       └─→ HTML5 Export (T025-T027)
  │                             └─→ Polish (T028-T033) [parallel]
  │                                   └─→ Validation (T034-T035)
```

**Critical Path**:

1. Setup → Scenes → Scripts → Integration → Export → Validation
2. Parallelizable clusters: Scenes (T004-T007), Manual Tests (T010-T013), Scripts (T014-T018), Polish (T028-T033)

---

## Parallel Execution Examples

### Cluster 1: Scenes (after T003 complete)

```
Task: "Create Player scene (LocalPlayer)" → wanderlight-online/Scenes/Player.tscn
Task: "Create RemotePlayer scene" → wanderlight-online/Scenes/RemotePlayer.tscn
Task: "Create WorldItem scene" → wanderlight-online/Scenes/WorldItem.tscn
Task: "Create InventoryPanel scene" → wanderlight-online/Scenes/UI/InventoryPanel.tscn
```

**Rationale**: All create separate .tscn files, no dependencies

---

### Cluster 2: Manual Test Documentation (after T009 complete)

```
Task: "Manual test: Login flow validation" → MANUAL_TESTING_GUIDE.md#scenario1
Task: "Manual test: Movement synchronization" → MANUAL_TESTING_GUIDE.md#scenario2
Task: "Manual test: Item pickup/drop" → MANUAL_TESTING_GUIDE.md#scenario3
Task: "Manual test: Inventory management" → MANUAL_TESTING_GUIDE.md#scenario4
```

**Rationale**: All document expected behavior, no implementation

---

### Cluster 3: Client Scripts (after T013 complete)

```
Task: "Implement LocalPlayerController script" → Scripts/Client/LocalPlayerController.cs
Task: "Implement RemotePlayerRenderer script" → Scripts/Client/RemotePlayerRenderer.cs
Task: "Implement WorldItemRenderer script" → Scripts/Client/WorldItemRenderer.cs
Task: "Implement InventoryUI script" → Scripts/Client/InventoryUI.cs
Task: "Implement MainMenuController script" → Scripts/Client/MainMenuController.cs
```

**Rationale**: All create separate .cs files, minimal cross-dependencies
**Note**: T019 (GameManager) must be AFTER this cluster (depends on other scripts)

---

### Cluster 4: Polish (after T027 complete)

```
Task: "Add performance monitoring overlay" → Scripts/Client/PerformanceMonitor.cs
Task: "Optimize sprite atlases" → Assets/Sprites/atlas.png
Task: "Add connection status indicator" → Scripts/Client/ConnectionStatusUI.cs
```

**Rationale**: Independent enhancements, no shared files

---

## Task Completion Checklist

### Setup Phase (T001-T003)

- [ ] T001: Directory structure created
- [ ] T002: Input actions configured in project.godot
- [ ] T003: Placeholder sprites created and imported

### Scene Phase (T004-T009)

- [ ] T004: Player.tscn created with correct hierarchy
- [ ] T005: RemotePlayer.tscn created
- [ ] T006: WorldItem.tscn created
- [ ] T007: InventoryPanel.tscn created with 12-slot grid
- [ ] T008: MainMenu.tscn created with login UI
- [ ] T009: GameWorld.tscn created with camera and spawn

### Test Scenarios (T010-T013)

- [ ] T010: Login flow test documented
- [ ] T011: Movement sync test documented
- [ ] T012: Item interaction test documented
- [ ] T013: Inventory management test documented

### Implementation Phase (T014-T019)

- [ ] T014: LocalPlayerController.cs implemented
- [ ] T015: RemotePlayerRenderer.cs implemented
- [ ] T016: WorldItemRenderer.cs implemented
- [ ] T017: InventoryUI.cs implemented
- [ ] T018: MainMenuController.cs implemented
- [ ] T019: GameManager.cs implemented and autoloaded

### Integration Phase (T020-T024)

- [ ] T020: All scripts attached to scenes
- [ ] T021: GameManager configured as autoload
- [ ] T022: Camera follow behavior working
- [ ] T023: MainMenu→GameWorld transition working
- [ ] T024: SpacetimeDB polling thread-safe

### Export Phase (T025-T027)

- [ ] T025: HTML5 export preset configured
- [ ] T026: Loading screen created
- [ ] T027: Local HTML5 build tested successfully

### Polish Phase (T028-T033)

- [ ] T028: Performance monitoring overlay added
- [ ] T029: Sprite atlases optimized
- [ ] T030: Connection status indicator added
- [ ] T031: Error boundaries implemented
- [ ] T032: Input validation strengthened
- [ ] T033: Help panel created

### Validation Phase (T034-T035)

- [ ] T034: All 8 manual test scenarios pass
- [ ] T035: Documentation updated and accurate

---

## Notes

### Godot-Specific Considerations

- **No Traditional Unit Tests**: Godot game engine projects use manual testing guide approach (MANUAL_TESTING_GUIDE.md)
- **Scene-Based**: .tscn files are binary/text hybrid, edited primarily in Godot Editor
- **Autoload Pattern**: Singletons managed via project.godot autoload section
- **Export Presets**: Stored in export_presets.cfg, managed via Godot Editor

### Test-Driven Approach (Adapted for Game Engine)

1. Document expected behavior (T010-T013) - manual test scenarios
2. Implement functionality (T014-T019) - scripts and scenes
3. Validate with manual testing (T034) - run scenarios in browser

### Version Control

- Commit after completing each major phase (Setup, Scenes, Scripts, Integration, Export)
- Tag Phase 4.0 release after T035 complete
- Branch strategy: Work on `002-description-build-a`, merge to main when validated

### Post-MVP Enhancements (Out of Scope)

- Combat system, NPCs, quests
- Advanced animations and particle effects
- Sound effects and music
- Mobile controls (touch input)
- Account system with passwords
- Chat system
- Trading between players

---

## Success Criteria

Phase 4.0 is considered **COMPLETE** when:

1. ✅ All 35 tasks marked complete
2. ✅ HTML5 build exports without errors
3. ✅ All 8 manual test scenarios pass (T034)
4. ✅ Performance targets met: 60 FPS, <200ms latency
5. ✅ Multiple players can join and interact simultaneously
6. ✅ Documentation accurate and deployment guide works
7. ✅ Code committed and tagged `v4.0.0`

**Ready for**: Phase 5 (content creation - NPCs, quests, combat) or production deployment

---

**Generated**: October 8, 2025 | **Based on**: plan.md, data-model.md, contracts/, research.md, quickstart.md
