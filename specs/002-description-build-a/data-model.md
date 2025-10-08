# Data Model: Browser Playable Multiplayer Prototype (Phase 4.0)

**Feature**: Client UI Layer for Wanderlight Online MVP  
**Date**: October 8, 2025  
**Phase**: Phase 1 - Design & Contracts

---

## Overview

This document defines the client-side data model for the Godot UI layer. The backend data model (Player, Inventory, WorldItem entities) is already implemented and tested in Phase 3. This model focuses on **visual representations** and **UI components** that bind to backend entities.

### Scope

- **Client-Side Only**: These entities exist in Godot scenes/scripts
- **Display Layer**: Visual representations of backend state
- **No Business Logic**: All logic remains in backend classes (Phase 3)
- **Binding Pattern**: UI components subscribe to backend state changes

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT LAYER (Godot)                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐         ┌──────────────────┐            │
│  │ MainMenuUI   │────────▶│ GameManager      │            │
│  │ (Login)      │         │ (Coordinator)    │            │
│  └──────────────┘         └────────┬─────────┘            │
│                                     │                       │
│                        ┌────────────┼────────────┐         │
│                        │            │            │         │
│           ┌────────────▼───┐  ┌────▼──────┐  ┌──▼────────┐│
│           │ LocalPlayer    │  │ Remote    │  │ WorldItem ││
│           │ (Controlled)   │  │ Player    │  │ Instance  ││
│           │                │  │ Renderer  │  │ Renderer  ││
│           └────────┬───────┘  └───────────┘  └───────────┘│
│                    │                                        │
│                    │                                        │
│              ┌─────▼──────┐                                │
│              │ InventoryUI│                                │
│              │ (12 slots) │                                │
│              └────────────┘                                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
                              │
                     Binds to │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              BACKEND LAYER (Phase 3 - Complete)             │
├─────────────────────────────────────────────────────────────┤
│  PlayerManager │ Inventory │ NetworkManager │ WorldItem    │
│  DatabaseManager │ PlayerController │ Operator            │
└─────────────────────────────────────────────────────────────┘
```

---

## Client Entities

### 1. LocalPlayer (Scene Component)

**Purpose**: Represents the player's own character with input handling and rendering.

**Godot Type**: `CharacterBody2D` (scene: `Player.tscn`)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `PlayerId` | `ulong` | Unique player ID from backend | SpacetimeDB |
| `DisplayName` | `string` | Player's chosen name (3-20 chars) | User input → PlayerManager |
| `Position` | `Vector2` | Current world position (pixels) | CharacterBody2D.Position |
| `Velocity` | `Vector2` | Movement velocity (pixels/sec) | Input + Speed |
| `Speed` | `float` | Movement speed (default: 200 px/s) | Exported property |
| `Inventory` | `Inventory` | Reference to backend inventory | Backend class |

**Children** (Godot scene hierarchy):

- `Sprite2D`: Visual representation (placeholder sprite)
- `CollisionShape2D`: Physics collision (circle/rectangle)
- `Label`: Display name above character
- `Area2D`: Pickup detection range (50px radius)

**State Transitions**:

```
Connecting → Login Screen → Spawned → Idle
                                        ↓
                         ┌─────────────┴──────────────┐
                         │                            │
                    Moving ←──────────────→ Picking Up Item
                         │                            │
                         └────────→ Inventory Open ←──┘
                                        ↓
                                   Disconnected
```

**Validation Rules**:

- Position must be within world bounds (validated by backend)
- Velocity magnitude ≤ Speed
- DisplayName validated before spawn (3-20 chars, alphanumeric + spaces)

**Script**: `LocalPlayerController.cs`

```csharp
public partial class LocalPlayerController : CharacterBody2D
{
    [Export] public float Speed = 200.0f;

    public ulong PlayerId { get; private set; }
    public string DisplayName { get; private set; }
    public Inventory Inventory { get; private set; }

    private Area2D pickupArea;
    private Label displayNameLabel;

    // Input handling (_PhysicsProcess for movement)
    // Pickup detection (Area2D.BodyEntered)
    // Network sync (NetworkManager.SendMovement)
}
```

---

### 2. RemotePlayer (Scene Component)

**Purpose**: Visual representation of other players with position interpolation.

**Godot Type**: `CharacterBody2D` (scene: `RemotePlayer.tscn`)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `PlayerId` | `ulong` | Unique player ID | SpacetimeDB subscription |
| `DisplayName` | `string` | Player's name | SpacetimeDB subscription |
| `TargetPosition` | `Vector2` | Latest position from network (20Hz) | SpacetimeDB update |
| `CurrentPosition` | `Vector2` | Interpolated position (60 FPS) | Lerp calculation |
| `PreviousPosition` | `Vector2` | Last position before update | Stored on update |
| `InterpolationTime` | `float` | Timer for lerp (0.0 to 0.05s) | \_Process delta |

**Children** (Godot scene hierarchy):

- `Sprite2D`: Visual representation (same as LocalPlayer)
- `CollisionShape2D`: Physics collision (for obstruction)
- `Label`: Display name above character

**State Transitions**:

```
PlayerAdded (SpacetimeDB) → Spawned → Interpolating Position
                                            ↓
                                  PlayerRemoved (SpacetimeDB) → Despawned
```

**Interpolation Logic**:

```
Every _Process(delta):
  if interpolationTime < 0.05s:
    interpolationTime += delta
    t = interpolationTime / 0.05
    CurrentPosition = PreviousPosition.Lerp(TargetPosition, t)
  else:
    CurrentPosition = TargetPosition
```

**Script**: `RemotePlayer.cs`

```csharp
public partial class RemotePlayer : CharacterBody2D
{
    public ulong PlayerId { get; set; }
    public string DisplayName { get; set; }

    private Vector2 previousPosition;
    private Vector2 targetPosition;
    private float interpolationTime = 0.0f;
    private const float InterpolationDuration = 0.05f;

    public void SetTargetPosition(Vector2 newPosition)
    {
        previousPosition = Position;
        targetPosition = newPosition;
        interpolationTime = 0.0f;
    }

    // _Process for interpolation
}
```

**Managed By**: `RemotePlayerRenderer.cs` (spawns/despawns instances)

---

### 3. WorldItemInstance (Scene Component)

**Purpose**: Visual representation of items on the ground with pickup detection.

**Godot Type**: `Node2D` (scene: `WorldItem.tscn`)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `ItemId` | `uint` | Unique item ID | SpacetimeDB (WorldItem table) |
| `ItemType` | `ItemCategory` | Type of item (enum) | SpacetimeDB |
| `Position` | `Vector2` | World position (pixels) | SpacetimeDB |
| `Quantity` | `uint` | Stack quantity | SpacetimeDB |
| `IsNearPlayer` | `bool` | Player within pickup range | Area2D overlap |

**Children** (Godot scene hierarchy):

- `Sprite2D`: Item icon sprite
- `Area2D`: Pickup detection zone (50px radius)
- `Label`: Quantity display (if stackable)

**State Transitions**:

```
ItemSpawned (SpacetimeDB) → Visible → Near Player → Picked Up (SpacetimeDB) → Despawned
```

**Pickup Logic**:

```
If IsNearPlayer AND Player presses E:
  LocalPlayerController.TryPickupNearbyItem()
    → WorldItem.TryPickup(playerId, itemId)
      → If Success: ItemRemoved event → Despawn
      → If Failure: Already taken by another player
```

**Script**: `WorldItemInstance.cs`

```csharp
public partial class WorldItemInstance : Node2D
{
    public uint ItemId { get; set; }
    public ItemCategory ItemType { get; set; }
    public uint Quantity { get; set; }

    private Sprite2D sprite;
    private Label quantityLabel;
    private Area2D pickupArea;

    public bool IsNearPlayer { get; private set; }

    // Area2D signals for pickup detection
}
```

**Managed By**: `WorldItemRenderer.cs` (spawns/despawns instances)

---

### 4. InventorySlotUI (UI Component)

**Purpose**: Single slot in the 12-slot inventory grid.

**Godot Type**: `Button` (parent: `GridContainer` in `InventoryPanel.tscn`)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `SlotIndex` | `int` | Position in grid (0-11) | Grid position |
| `ItemType` | `ItemCategory?` | Item in slot (nullable) | Inventory.Slots[SlotIndex] |
| `Quantity` | `uint` | Stack quantity | Inventory.Slots[SlotIndex] |
| `IsEmpty` | `bool` | No item in slot | ItemType == null |

**Children** (Button internal structure):

- `TextureRect`: Item icon texture
- `Label`: Quantity text (e.g., "x5")

**State Transitions**:

```
Empty ←────────────────→ Occupied (ItemType + Quantity)
  ↑                           │
  └───────────────────────────┘
       (Item dropped)    (Item added)
```

**Interaction Logic**:

```
On Button.Pressed():
  if IsEmpty:
    return (do nothing)
  else:
    ItemStack item = Inventory.RemoveItem(SlotIndex)
    WorldItem.Drop(item.ItemType, item.Quantity, LocalPlayer.Position)
    Update UI (slot becomes empty)
```

**Script**: `InventorySlotUI.cs`

```csharp
public partial class InventorySlotUI : Button
{
    [Export] public int SlotIndex { get; set; }

    public ItemCategory? ItemType { get; private set; }
    public uint Quantity { get; private set; }
    public bool IsEmpty => !ItemType.HasValue;

    private TextureRect iconTexture;
    private Label quantityLabel;

    public void UpdateSlot(ItemCategory? itemType, uint quantity)
    {
        ItemType = itemType;
        Quantity = quantity;

        if (IsEmpty)
        {
            iconTexture.Texture = null;
            quantityLabel.Visible = false;
        }
        else
        {
            iconTexture.Texture = GetItemIcon(itemType.Value);
            quantityLabel.Text = $"x{quantity}";
            quantityLabel.Visible = quantity > 1;
        }
    }
}
```

---

### 5. InventoryPanel (UI Container)

**Purpose**: 12-slot inventory UI with open/close toggle.

**Godot Type**: `Panel` (Control node, scene: `InventoryPanel.tscn`)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `IsOpen` | `bool` | Panel visibility state | TAB key toggle |
| `Slots` | `InventorySlotUI[12]` | Array of slot UI components | Child nodes |

**Children** (Panel hierarchy):

- `GridContainer` (3 columns x 4 rows)
  - `InventorySlotUI` x12 (slots 0-11)

**State Transitions**:

```
Closed (Visible=false) ←──TAB key──→ Open (Visible=true)
```

**Update Logic**:

```
On Inventory.OnInventoryChanged event:
  for (int i = 0; i < 12; i++):
    ItemStack slot = Inventory.GetSlots()[i]
    Slots[i].UpdateSlot(slot.ItemType, slot.Quantity)
```

**Script**: `InventoryUI.cs`

```csharp
public partial class InventoryUI : Panel
{
    private bool isOpen = false;
    private InventorySlotUI[] slots = new InventorySlotUI[12];

    public override void _Ready()
    {
        // Get slot references
        var grid = GetNode<GridContainer>("GridContainer");
        for (int i = 0; i < 12; i++)
        {
            slots[i] = grid.GetChild<InventorySlotUI>(i);
            slots[i].SlotIndex = i;
            slots[i].Pressed += () => OnSlotClicked(slots[i].SlotIndex);
        }

        // Subscribe to inventory changes
        Inventory.Instance.OnInventoryChanged += RefreshUI;

        Visible = false; // Start closed
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_inventory"))
        {
            isOpen = !isOpen;
            Visible = isOpen;
            GetViewport().SetInputAsHandled();
        }
    }

    private void RefreshUI()
    {
        var inventorySlots = Inventory.Instance.GetSlots();
        for (int i = 0; i < 12; i++)
        {
            slots[i].UpdateSlot(inventorySlots[i].ItemType, inventorySlots[i].Quantity);
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (slots[slotIndex].IsEmpty) return;

        var item = Inventory.Instance.RemoveItem(slotIndex);
        WorldItem.Drop(item.ItemType, item.Quantity, LocalPlayer.Instance.Position);
    }
}
```

---

### 6. GameManager (Coordinator)

**Purpose**: Coordinates scene transitions, player spawning, and network synchronization.

**Godot Type**: `Node` (autoload singleton)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `LocalPlayer` | `LocalPlayer` | Reference to local player instance | Spawned on join |
| `RemotePlayers` | `Dictionary<ulong, RemotePlayer>` | All other players | SpacetimeDB subscription |
| `WorldItems` | `Dictionary<uint, WorldItemInstance>` | All ground items | SpacetimeDB subscription |
| `CurrentScene` | `Node` | Active scene (MainMenu or GameWorld) | Scene tree |

**Responsibilities**:

- Scene management (MainMenu → GameWorld transition)
- Player spawning (local and remote)
- Network update queue processing (see research.md threading pattern)
- Connection state monitoring

**State Machine**:

```
Initializing → MainMenu → Connecting → InGame → Disconnected
                  ↑                       │
                  └───────────────────────┘
```

**Script**: `GameManager.cs`

```csharp
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public LocalPlayer LocalPlayer { get; private set; }
    private Dictionary<ulong, RemotePlayer> remotePlayers = new();
    private Dictionary<uint, WorldItemInstance> worldItems = new();

    private ConcurrentQueue<PlayerUpdate> playerUpdates = new();
    private ConcurrentQueue<ItemUpdate> itemUpdates = new();

    public override void _Ready()
    {
        Instance = this;
        SetupSpacetimeDBSubscriptions();
    }

    public override void _Process(double delta)
    {
        // Process network update queues (20Hz → 60FPS)
        ProcessPlayerUpdates();
        ProcessItemUpdates();
    }

    private void SetupSpacetimeDBSubscriptions()
    {
        DatabaseManager.Instance.OnPlayerAdded += (id, name, x, y) =>
        {
            playerUpdates.Enqueue(new PlayerUpdate { ... });
        };

        // More subscriptions...
    }
}
```

---

### 7. MainMenuUI (Scene)

**Purpose**: Login screen with display name input and join button.

**Godot Type**: `Control` (scene: `MainMenu.tscn`)

**Attributes**:
| Attribute | Type | Description | Source |
|-----------|------|-------------|--------|
| `DisplayNameInput` | `LineEdit` | Text input for name | User input |
| `JoinButton` | `Button` | Triggers join action | Click event |
| `ErrorLabel` | `Label` | Error message display | Validation failure |

**Children** (UI hierarchy):

- `VBoxContainer`
  - `Label`: "Enter Display Name"
  - `LineEdit`: Display name input (max 20 chars)
  - `Button`: "Join Game"
  - `Label`: Error message (initially hidden)

**State Transitions**:

```
Initial → Name Entered → Validating → Success (→ GameWorld)
                            ↓
                       Error (show message)
```

**Validation Flow**:

```
On JoinButton.Pressed():
  string name = DisplayNameInput.Text

  ValidationResult validation = DisplayNameValidator.Validate(name)
  if (!validation.IsSuccess):
    ErrorLabel.Text = validation.ErrorMessage
    ErrorLabel.Visible = true
    return

  Result<PlayerId> result = PlayerManager.TryAddPlayer(name)
  if (!result.IsSuccess):
    ErrorLabel.Text = result.ErrorMessage (e.g., "Name already taken")
    ErrorLabel.Visible = true
    return

  GameManager.Instance.TransitionToGameWorld(result.Value)
```

**Script**: `MainMenuController.cs`

```csharp
public partial class MainMenuController : Control
{
    private LineEdit displayNameInput;
    private Button joinButton;
    private Label errorLabel;

    public override void _Ready()
    {
        displayNameInput = GetNode<LineEdit>("VBoxContainer/DisplayNameInput");
        joinButton = GetNode<Button>("VBoxContainer/JoinButton");
        errorLabel = GetNode<Label>("VBoxContainer/ErrorLabel");

        joinButton.Pressed += OnJoinButtonPressed;
        errorLabel.Visible = false;
    }

    private void OnJoinButtonPressed()
    {
        string name = displayNameInput.Text;

        // Validate (see research.md for validation rules)
        var validation = DisplayNameValidator.Validate(name);
        if (!validation.IsSuccess)
        {
            ShowError(validation.ErrorMessage);
            return;
        }

        // Try to add player
        var result = PlayerManager.Instance.TryAddPlayer(name);
        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
            return;
        }

        // Success - transition to game
        GameManager.Instance.TransitionToGameWorld(result.Value);
    }

    private void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.Visible = true;
    }
}
```

---

## Backend Entity References

These entities exist in Phase 3 backend (already implemented). Client entities bind to them:

### Player (Backend)

- **Table**: SpacetimeDB `player` table
- **Attributes**: `PlayerId` (ulong), `DisplayName` (string), `PositionX` (float), `PositionY` (float)
- **Managed By**: `PlayerManager.cs`

### Inventory (Backend)

- **Class**: `Inventory.cs`
- **Attributes**: `Slots` (ItemStack[12]), `UsedSlots` (int)
- **Methods**: `AddItem()`, `RemoveItem()`, `IsFull()`, `GetSlots()`

### WorldItem (Backend)

- **Table**: SpacetimeDB `world_item` table
- **Attributes**: `ItemId` (uint), `ItemType` (ItemCategory), `X` (float), `Y` (float), `Quantity` (uint)
- **Methods**: `TryPickup()`, `Drop()`

---

## Data Flow Diagrams

### Login Flow

```
User Input → LineEdit → MainMenuController → DisplayNameValidator
                                                    ↓
                                          PlayerManager.TryAddPlayer()
                                                    ↓
                                          SpacetimeDB (player table)
                                                    ↓
                                          OnPlayerAdded event
                                                    ↓
                                          GameManager → Spawn LocalPlayer
```

### Movement Flow

```
Keyboard Input → LocalPlayerController._PhysicsProcess()
                        ↓
                  CharacterBody2D.MoveAndSlide()
                        ↓
                  NetworkManager.SendMovement(Position)
                        ↓
                  SpacetimeDB (player table update)
                        ↓
                  OnPlayerMoved event (all clients)
                        ↓
                  RemotePlayer.SetTargetPosition()
                        ↓
                  Interpolation in _Process() → Smooth 60 FPS
```

### Item Pickup Flow

```
LocalPlayer near Item → Area2D overlap → IsNearPlayer = true
                              ↓
                        Player presses E key
                              ↓
                  WorldItem.TryPickup(playerId, itemId)
                              ↓
                  SpacetimeDB (atomic operation)
                              ↓
            ┌─────────────────┴─────────────────┐
            ↓                                   ↓
  OnItemRemoved event               Inventory.AddItem()
            ↓                                   ↓
  WorldItemRenderer.Despawn()       InventoryUI.RefreshUI()
```

---

## Validation Summary

| Entity          | Validation Rules                                | Enforced By                   |
| --------------- | ----------------------------------------------- | ----------------------------- |
| LocalPlayer     | Display name: 3-20 chars, alphanumeric + spaces | DisplayNameValidator          |
| LocalPlayer     | Position within world bounds                    | Backend (PlayerController)    |
| RemotePlayer    | PlayerId unique                                 | SpacetimeDB (primary key)     |
| WorldItem       | Atomic pickup (only one player)                 | Backend (WorldItem.TryPickup) |
| InventorySlotUI | SlotIndex 0-11                                  | GridContainer position        |
| Inventory       | Max 12 slots                                    | Backend (Inventory.IsFull())  |

---

## Performance Considerations

### Entity Limits

- **Max Remote Players**: 100 (per constitution performance target)
- **Max World Items**: 100 (per constitution performance target)
- **Inventory Slots**: Fixed at 12 (backend constraint)

### Object Pooling Strategy

- **RemotePlayer Instances**: Pool and reuse (don't create/destroy on every join/leave)
- **WorldItem Instances**: Pool and reuse (spawn/despawn from pool)
- **Reason**: Reduces GC pressure, maintains 60 FPS

### Memory Estimates

- LocalPlayer: ~10 KB (sprite + script)
- RemotePlayer: ~8 KB each (100 players = 800 KB)
- WorldItem: ~5 KB each (100 items = 500 KB)
- **Total Client Memory**: ~2-3 MB for entities (well within 256MB HTML5 budget)

---

## Next Steps

This data model serves as the foundation for:

1. **Contract Generation** (`/contracts/`) - Define integration interfaces
2. **Scene Creation** - Build .tscn files matching entity structures
3. **Script Implementation** - Write C# scripts implementing entity behaviors
4. **Testing** - Validate entities against manual testing scenarios

All entities designed to maintain constitution principles:

- ✅ Simplicity (no abstraction layers)
- ✅ Framework Direct Usage (pure Godot nodes)
- ✅ Backend Integration (direct class calls, no DTOs)
- ✅ Performance (60 FPS, <200ms latency targets met)
