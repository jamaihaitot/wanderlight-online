# Player Contract

## Overview

Defines the requirements and expected behaviors for player authentication, connection, and state management in Wanderlight Online.

## Requirements

- Unique display name (case-insensitive)
- Join at defined spawn location
- Real-time movement and visibility
- Inventory management (add/remove/view)
- Atomic pick up/drop actions
- State restoration on reconnect
- Reservation of display name for 2 minutes after disconnect
- Profanity filtering and format enforcement

## Acceptance Criteria

- Player cannot join with duplicate or invalid display name
- Player state (position, inventory) restored on reconnect
- Inventory actions are atomic and consistent
- Movement updates are ordered and visible to nearby players

---
