# Inventory Contract

## Overview

Defines the requirements and expected behaviors for inventory management in Wanderlight Online.

## Requirements

- 12 slots per player
- Stack sizes: 20 for consumables/materials, 1 for equipment
- Categories: generic, consumable, equipment
- Server-side enforcement of capacity and stacking
- Persistence of inventory on change and disconnect

## Acceptance Criteria

- Cannot add items beyond capacity
- Stacking rules enforced
- Inventory persists across sessions
- Atomic add/remove actions

---
