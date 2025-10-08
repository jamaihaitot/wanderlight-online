# Inventory Contract

**Integration Point**: InventoryUI ↔ Inventory + DatabaseManager  
**Purpose**: Define the interface for inventory display, item management, and UI interactions  
**Status**: Active  
**Version**: 1.0.0

---

## Overview

This contract specifies the interaction between the InventoryUI (client display) and the backend Inventory system for managing the 12-slot player inventory.

---

## Interface Definition

### Client Component

**Class**: `InventoryUI.cs`  
**Scene**: `InventoryPanel.tscn` (Panel with GridContainer)  
**Responsibility**: Display inventory contents, handle item clicks, toggle visibility

### Backend Component

**Class**: `Inventory.cs` (Phase 3 - already implemented)  
**Responsibility**: Manage 12 inventory slots, add/remove items, persist state

---

## Method Signatures

### Backend Methods

```csharp
// Inventory.cs (Phase 3 - already implemented)
public Result<Success> AddItem(ItemCategory itemType, uint quantity);
public Result<ItemStack> RemoveItem(int slotIndex);
public ItemStack[] GetSlots(); // Returns all 12 slots
public bool IsFull();
public int UsedSlots { get; }
```

**AddItem**:

- **Parameters**: `itemType` (ItemCategory enum), `quantity` (uint)
- **Returns**: Success or InventoryFull error
- **Behavior**: Stacks with existing items, finds empty slot if new type

**RemoveItem**:

- **Parameters**: `slotIndex` (0-11)
- **Returns**: ItemStack (type + quantity) or EmptySlot error
- **Behavior**: Clears slot, returns item data for dropping

**GetSlots**:

- **Returns**: Array of 12 ItemStack (null if empty)
- **Use**: Refresh entire UI after inventory changes

---

## Events

### OnInventoryChanged

```csharp
// Backend event (fired after any inventory modification)
public event Action OnInventoryChanged;

// Fired when:
//   - Item added (pickup, trade, etc.)
//   - Item removed (drop, use, etc.)
//   - Inventory loaded (rejoin)
```

**Client Handler**:

```csharp
// InventoryUI.cs
public override void _Ready()
{
    Inventory.Instance.OnInventoryChanged += RefreshUI;
}

private void RefreshUI()
{
    var slots = Inventory.Instance.GetSlots();
    for (int i = 0; i < 12; i++)
    {
        slotButtons[i].UpdateDisplay(slots[i]);
    }
}
```

---

## Data Flow

### Open/Close Inventory

```
1. Player presses TAB key
2. InventoryUI._UnhandledInput() detects "toggle_inventory" action
3. Toggle isOpen boolean
4. Set Panel.Visible = isOpen
5. If opening: Call RefreshUI() to sync with backend state
```

### Drop Item from Inventory

```
1. Player clicks slot 5 (contains "Health Potion x3")
2. InventorySlotUI.Pressed event fires
3. InventoryUI.OnSlotClicked(5):
   - Check if slot is empty → return if true
   - Call Inventory.RemoveItem(5) → Returns ItemStack{HealthPotion, 3}
   - Call WorldItem.Drop(HealthPotion, 3, LocalPlayer.Position)
4. Backend spawns item in world near player
5. OnInventoryChanged event fires
6. RefreshUI() updates slot 5 to empty
7. OnItemSpawned event fires (handled by WorldItemRenderer)
8. All players see item appear on ground
```

### Pickup Item to Inventory (Reverse Flow)

```
1. Player picks up item (see item-interaction-contract.md)
2. Inventory.AddItem(ItemCategory.HealthPotion, 1)
3. Backend finds empty slot or stacks with existing
4. OnInventoryChanged event fires
5. InventoryUI.RefreshUI() updates display
6. Player sees item appear in inventory (if open) or count increase
```

---

## Validation Rules

### Slot Index Validation

- Must be 0-11 (12 slots total)
- Validated by backend (client passes index from UI)

### Item Quantity Validation

- Minimum: 1 (no zero-quantity items)
- Maximum: Per item type (e.g., potions stack to 99)
- Validated by backend

### Inventory Full Handling

```csharp
// Client checks before pickup
if (Inventory.Instance.IsFull())
{
    ShowTooltip("Inventory Full (12/12)");
    return; // Don't attempt pickup
}
```

---

## UI Layout

### InventoryPanel Structure

```
Panel (480x320 pixels, centered)
├── Label: "Inventory (5/12)" [title bar]
├── GridContainer (3 columns x 4 rows, 120x120 per slot)
│   ├── InventorySlotUI [0] (Button with icon + quantity)
│   ├── InventorySlotUI [1]
│   ├── InventorySlotUI [2]
│   ├── InventorySlotUI [3]
│   ├── ... (8 more slots)
│   └── InventorySlotUI [11]
└── CloseButton (optional, TAB to close)
```

### Slot Display States

- **Empty**: Gray background, no icon, no text
- **Occupied**: Item icon texture, quantity label if >1
- **Highlighted**: Hover effect (lighter background)

---

## Performance Requirements

| Metric     | Target | Notes                               |
| ---------- | ------ | ----------------------------------- |
| Open/Close | <16ms  | Instant (single frame)              |
| UI Refresh | <10ms  | Update 12 slots on inventory change |
| Slot Click | <50ms  | Click → Backend → UI update         |

---

## Testing Scenarios

### Manual Test 1: Open/Close

```
GIVEN player in game
WHEN player presses TAB
THEN inventory panel becomes visible
  AND displays current inventory state (items + quantities)
WHEN player presses TAB again
THEN inventory panel closes
```

### Manual Test 2: Drop Item

```
GIVEN player has "Health Potion x3" in slot 5
  AND inventory is open
WHEN player clicks slot 5
THEN item disappears from inventory
  AND "Health Potion x3" appears on ground near player
  AND all other players see the item
```

### Manual Test 3: Full Inventory

```
GIVEN player has 12/12 inventory slots filled
WHEN player attempts to pick up new item
THEN tooltip displays "Inventory Full (12/12)"
  AND item remains on ground
  AND pickup action does not occur
```

### Manual Test 4: Stack Merge

```
GIVEN player has "Health Potion x5" in slot 2
WHEN player picks up "Health Potion x3"
THEN slot 2 updates to "Health Potion x8"
  AND no new slot is used
```

---

## Example Implementation

### Client (InventoryUI.cs)

```csharp
public partial class InventoryUI : Panel
{
    private bool isOpen = false;
    private InventorySlotUI[] slots = new InventorySlotUI[12];
    private Label titleLabel;

    public override void _Ready()
    {
        // Get slot references from GridContainer
        var grid = GetNode<GridContainer>("GridContainer");
        for (int i = 0; i < 12; i++)
        {
            slots[i] = grid.GetChild<InventorySlotUI>(i);
            slots[i].SlotIndex = i;
            int index = i; // Capture for lambda
            slots[i].Pressed += () => OnSlotClicked(index);
        }

        titleLabel = GetNode<Label>("TitleLabel");

        // Subscribe to inventory changes
        Inventory.Instance.OnInventoryChanged += RefreshUI;

        Visible = false; // Start closed
        RefreshUI(); // Sync initial state
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_inventory"))
        {
            isOpen = !isOpen;
            Visible = isOpen;
            if (isOpen) RefreshUI(); // Sync on open
            GetViewport().SetInputAsHandled();
        }
    }

    private void RefreshUI()
    {
        var inventorySlots = Inventory.Instance.GetSlots();
        int usedSlots = Inventory.Instance.UsedSlots;

        titleLabel.Text = $"Inventory ({usedSlots}/12)";

        for (int i = 0; i < 12; i++)
        {
            slots[i].UpdateDisplay(inventorySlots[i]);
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (slots[slotIndex].IsEmpty) return;

        var result = Inventory.Instance.RemoveItem(slotIndex);
        if (result.IsSuccess)
        {
            WorldItem.Drop(
                result.Value.ItemType,
                result.Value.Quantity,
                LocalPlayer.Instance.Position
            );
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

- Drag-and-drop slot reordering
- Item tooltips with descriptions
- Quick-use items (right-click)
- Item filtering/sorting

---

## Status

- [x] Contract defined
- [x] Backend implementation exists (Phase 3)
- [ ] Client implementation pending (Phase 4.0)
- [ ] Manual testing pending (Phase 4.0)

---

**Last Updated**: October 8, 2025  
**Reviewed By**: Phase 4.0 Planning Process
