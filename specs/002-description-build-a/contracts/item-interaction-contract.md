# Item Interaction Contract

**Integration Point**: WorldItemRenderer + LocalPlayerController ↔ WorldItem + DatabaseManager  
**Purpose**: Define the interface for world item spawning, pickup detection, and drop mechanics  
**Status**: Active  
**Version**: 1.0.0

---

## Overview

This contract specifies the interaction between client-side item rendering/pickup and backend item management for items on the ground in the game world.

---

## Interface Definition

### Client Components

**Classes**:

- `WorldItemRenderer.cs`: Spawns/despawns WorldItemInstance scenes
- `LocalPlayerController.cs`: Detects nearby items, handles E key pickup
- `WorldItemInstance.cs`: Visual representation with Area2D for proximity

### Backend Component

**Class**: `WorldItem.cs` (Phase 3 - already implemented)  
**Responsibility**: Spawn items, handle atomic pickup, persist state

---

## Method Signatures

### Backend Methods

```csharp
// WorldItem.cs (Phase 3 - already implemented)
public static Result<uint> Drop(ItemCategory itemType, uint quantity, Vector2 position);
public static Result<Success> TryPickup(ulong playerId, uint itemId);
public static WorldItem[] GetAllItems();
```

**Drop**:

- **Parameters**: `itemType`, `quantity`, `position` (where to spawn)
- **Returns**: `itemId` (unique ID for the spawned item) or error
- **Behavior**: Creates item in SpacetimeDB world_item table

**TryPickup**:

- **Parameters**: `playerId` (who picks up), `itemId` (which item)
- **Returns**: Success or error (already taken, inventory full, etc.)
- **Behavior**: Atomic operation - only ONE player succeeds if multiple try simultaneously

**GetAllItems**:

- **Returns**: Array of all items in world
- **Use**: Initial sync when player joins

---

## Events

### OnItemSpawned

```csharp
// Backend event (fired when item added to world_item table)
public event Action<uint, ItemCategory, float, float, uint> OnItemSpawned;

// Parameters:
//   - itemId: Unique identifier
//   - itemType: Category (enum)
//   - x, y: World position
//   - quantity: Stack size
```

**Client Handler**:

```csharp
// WorldItemRenderer.cs
DatabaseManager.Instance.OnItemSpawned += (itemId, itemType, x, y, quantity) =>
{
    SpawnItemVisual(itemId, itemType, new Vector2(x, y), quantity);
};
```

### OnItemRemoved

```csharp
// Backend event (fired when item removed from world_item table)
public event Action<uint> OnItemRemoved;

// Parameters:
//   - itemId: ID of item to remove
```

**Client Handler**:

```csharp
// WorldItemRenderer.cs
DatabaseManager.Instance.OnItemRemoved += (itemId) =>
{
    DespawnItemVisual(itemId);
};
```

---

## Data Flow

### Drop Item to World

```
1. Player clicks inventory slot with "Health Potion x3"
2. InventoryUI calls Inventory.RemoveItem(slotIndex)
3. InventoryUI calls WorldItem.Drop(HealthPotion, 3, playerPosition)
4. Backend:
   - Generates unique itemId
   - Inserts into SpacetimeDB world_item table
   - Returns itemId
5. OnItemSpawned event fires (ALL clients)
6. WorldItemRenderer.SpawnItemVisual():
   - Instantiate WorldItem.tscn scene
   - Set position, icon, quantity label
   - Add to scene tree
7. All players see item appear
```

### Pickup Item from World

```
1. Player walks near item (within 50px)
2. WorldItemInstance.Area2D detects overlap with player
3. IsNearPlayer = true (visual feedback: item glows)
4. Player presses E key
5. LocalPlayerController._UnhandledInput() detects "interact"
6. LocalPlayerController.TryPickupNearbyItem():
   - Get nearest item in range
   - Call WorldItem.TryPickup(localPlayerId, itemId)
7. Backend:
   - Check player inventory not full
   - Remove from world_item table (atomic)
   - Call Inventory.AddItem(itemType, quantity)
   - Return Success
8. OnItemRemoved event fires (ALL clients)
9. WorldItemRenderer.DespawnItemVisual(itemId):
   - Find WorldItemInstance by itemId
   - Remove from scene tree (QueueFree)
10. OnInventoryChanged event fires
11. InventoryUI updates to show new item
12. All players see item disappear
```

### Race Condition (Two Players Pickup Same Item)

```
Timeline:
  T=0ms: Alice walks to item, IsNearPlayer=true
  T=0ms: Bob walks to item, IsNearPlayer=true
  T=100ms: Alice presses E → TryPickup(AliceId, item123)
  T=102ms: Bob presses E → TryPickup(BobId, item123)

Backend Handling:
  - Alice's request arrives first
  - SpacetimeDB removes item123 (atomic DELETE)
  - Returns Success to Alice
  - Bob's request arrives 2ms later
  - SpacetimeDB: item123 not found (already deleted)
  - Returns Error("Item no longer available") to Bob

Client Results:
  - Alice: Item added to inventory, item disappears
  - Bob: Item disappears (OnItemRemoved event), no inventory change
  - Bob sees item vanish "in front of him" (another player got it)
  - No error message for Bob (expected behavior in multiplayer)
```

---

## Proximity Detection

### Area2D Setup (WorldItemInstance)

```csharp
// WorldItemInstance.tscn hierarchy:
Node2D (WorldItemInstance)
├── Sprite2D (item icon)
├── Label (quantity, e.g., "x5")
└── Area2D (pickup detection)
    └── CollisionShape2D (CircleShape2D, radius=50px)
```

### Overlap Detection

```csharp
// WorldItemInstance.cs
public partial class WorldItemInstance : Node2D
{
    public uint ItemId { get; set; }
    public ItemCategory ItemType { get; set; }
    public uint Quantity { get; set; }
    public bool IsNearPlayer { get; private set; }

    private Area2D pickupArea;
    private Sprite2D sprite;

    public override void _Ready()
    {
        pickupArea = GetNode<Area2D>("Area2D");
        sprite = GetNode<Sprite2D>("Sprite2D");

        pickupArea.BodyEntered += OnBodyEntered;
        pickupArea.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is LocalPlayerController)
        {
            IsNearPlayer = true;
            sprite.Modulate = Colors.Yellow; // Visual feedback: glow
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is LocalPlayerController)
        {
            IsNearPlayer = false;
            sprite.Modulate = Colors.White; // Remove glow
        }
    }
}
```

---

## Validation Rules

### Pickup Validation (Backend)

1. **Item Exists**: itemId must be in world_item table
2. **Inventory Space**: Player inventory must have room
3. **Atomic Operation**: First request wins, subsequent fail

### Drop Validation (Backend)

1. **Valid Position**: Within world bounds
2. **Valid Quantity**: > 0
3. **Item Type**: Must be valid ItemCategory enum value

---

## Performance Requirements

| Metric          | Target               | Notes                     |
| --------------- | -------------------- | ------------------------- |
| Pickup Latency  | <200ms               | E key → Item disappears   |
| Drop Latency    | <200ms               | Click slot → Item appears |
| Proximity Check | Every frame (60 FPS) | Area2D overlap detection  |
| Max Items       | 100                  | Performance limit         |

---

## Testing Scenarios

### Manual Test 1: Single Player Pickup

```
GIVEN item "Health Potion x3" at position (200, 200)
  AND player at position (250, 200) [50px away]
WHEN player presses E key
THEN item disappears from world
  AND "Health Potion x3" appears in player's inventory
  AND happens within 200ms
```

### Manual Test 2: Two Players Same Item

```
GIVEN item "Health Potion" at position (300, 300)
  AND Alice at (310, 300) [within range]
  AND Bob at (290, 300) [within range]
WHEN Alice and Bob both press E simultaneously
THEN one player gets item (atomic operation)
  AND other player sees item disappear
  AND no duplicate items created
```

### Manual Test 3: Drop and Pickup

```
GIVEN player has "Mana Potion x5" in inventory
WHEN player opens inventory and clicks the potion
THEN potion appears on ground near player
  AND inventory shows potion removed
WHEN player walks to dropped potion and presses E
THEN potion returns to inventory (same slot or new slot)
```

### Manual Test 4: Proximity Visual Feedback

```
GIVEN item on ground
WHEN player walks within 50px of item
THEN item sprite glows yellow (highlight)
WHEN player walks away (>50px)
THEN item sprite returns to normal color
```

### Manual Test 5: Inventory Full

```
GIVEN player has 12/12 inventory slots filled
  AND item "Health Potion" on ground
WHEN player walks to item and presses E
THEN tooltip displays "Inventory Full (12/12)"
  AND item remains on ground
  AND pickup does not occur
```

---

## Example Implementation

### Client (LocalPlayerController.cs - Pickup)

```csharp
public override void _UnhandledInput(InputEvent @event)
{
    if (@event.IsActionPressed("interact"))
    {
        TryPickupNearbyItem();
        GetViewport().SetInputAsHandled();
    }
}

private void TryPickupNearbyItem()
{
    // Check if inventory full
    if (Inventory.Instance.IsFull())
    {
        ShowTooltip($"Inventory Full ({Inventory.Instance.UsedSlots}/{Inventory.Instance.MaxSlots})");
        return;
    }

    // Find nearest item in pickup range
    var nearbyItem = FindNearestItem(50.0f); // 50px radius
    if (nearbyItem == null) return;

    // Attempt pickup (backend handles atomicity)
    var result = WorldItem.TryPickup(PlayerId, nearbyItem.ItemId);

    // Success handled by OnItemRemoved + OnInventoryChanged events
    // Failure is silent (another player got it first)
}

private WorldItemInstance FindNearestItem(float maxDistance)
{
    var items = GetTree().GetNodesInGroup("WorldItems");
    WorldItemInstance nearest = null;
    float nearestDist = maxDistance;

    foreach (WorldItemInstance item in items)
    {
        float dist = Position.DistanceTo(item.Position);
        if (dist < nearestDist && item.IsNearPlayer)
        {
            nearest = item;
            nearestDist = dist;
        }
    }

    return nearest;
}
```

### Client (WorldItemRenderer.cs - Spawn/Despawn)

```csharp
public partial class WorldItemRenderer : Node
{
    [Export] public PackedScene WorldItemScene;

    private Dictionary<uint, WorldItemInstance> activeItems = new();

    public override void _Ready()
    {
        DatabaseManager.Instance.OnItemSpawned += SpawnItemVisual;
        DatabaseManager.Instance.OnItemRemoved += DespawnItemVisual;
    }

    private void SpawnItemVisual(uint itemId, ItemCategory itemType, float x, float y, uint quantity)
    {
        var instance = WorldItemScene.Instantiate<WorldItemInstance>();
        instance.ItemId = itemId;
        instance.ItemType = itemType;
        instance.Quantity = quantity;
        instance.Position = new Vector2(x, y);
        instance.AddToGroup("WorldItems");

        AddChild(instance);
        activeItems[itemId] = instance;
    }

    private void DespawnItemVisual(uint itemId)
    {
        if (activeItems.TryGetValue(itemId, out var instance))
        {
            instance.QueueFree();
            activeItems.Remove(itemId);
        }
    }
}
```

---

## Versioning

**Version**: 1.0.0  
**Breaking Changes**: None  
**Migration**: N/A

**Future Enhancements** (out of scope):

- Item durability/expiration timers
- Item quality/rarity tiers
- Auto-pickup on proximity (no E key)
- Item ownership (drops visible only to dropper for 30s)

---

## Status

- [x] Contract defined
- [x] Backend implementation exists (Phase 3)
- [ ] Client implementation pending (Phase 4.0)
- [ ] Manual testing pending (Phase 4.0)

---

**Last Updated**: October 8, 2025  
**Reviewed By**: Phase 4.0 Planning Process
