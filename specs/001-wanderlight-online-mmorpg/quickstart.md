# Quickstart: Wanderlight Online — MMORPG foundation

## Prerequisites

- Godot 4.x (with C# support/Mono)
- SpacetimeDB account and database instance
- .NET SDK (for C# development)

## Setup Steps

1. Clone the repository and switch to branch `001-wanderlight-online-mmorpg`.
2. Open Godot and create a new project named `Wanderlight Online`.
3. Create folders: `Scripts`, `Scenes`, `Assets`.
4. Enable C# support in Godot (install Mono if needed).
5. Set up SpacetimeDB and obtain connection details.
6. Implement core systems:
   - `PlayerManager` (player join/auth)
   - `DatabaseManager` (SpacetimeDB integration)
   - `Inventory` (item management)
   - `NetworkManager` (WebSocket communication)
7. Run unit tests for all critical components.
8. Export project for HTML5 (browser deployment).

## Development Notes

- All game logic in C# scripts, organized by system.
- Use SpacetimeDB for persistence and authoritative state.
- WebSocket for real-time updates.
- Follow test-first workflow and constitution principles.

---
