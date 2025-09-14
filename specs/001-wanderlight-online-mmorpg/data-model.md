# Data Model: Wanderlight Online — MMORPG foundation

## Entities

### Player

- display_name: string (unique, case-insensitive)
- position: Vector2
- inventory: Inventory
- connection_state: enum (connected, ghosted, disconnected)
- reservation_status: bool

### Inventory

- slots: int (max 12)
- items: list of ItemStack

### ItemStack

- item_type: string
- quantity: int (max 20 for consumables/materials, 1 for equipment)
- category: enum (generic, consumable, equipment)

### WorldItem

- item_type: string
- position: Vector2
- stack_size: int
- owner: string (display_name if picked up)
- persistence_status: bool

### Operator

- telemetry: metrics (connected players, join time, update latency, errors)
- logs: structured log entries
- world_reset_command: bool

## Relationships

- Player has Inventory
- Inventory contains ItemStacks
- WorldItem can be picked up/dropped by Player
- Operator can reset world state and view telemetry/logs

---
