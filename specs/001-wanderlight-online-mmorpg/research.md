# Phase 0 Research: Wanderlight Online — MMORPG foundation

## Research Questions & Clarifications

- What is the preferred unit test runner for Godot C#?
- Is a CLI interface required for game logic libraries in Godot?
- How should versioning be managed for the Godot project?
- How can RED-GREEN-Refactor cycle enforcement be automated in Godot workflow?
- What is the recommended approach for structured logging and telemetry in Godot (C#)?
- Are there any constraints for SpacetimeDB integration with Godot (C#)?

## Key Technical Decisions

- Use Godot 4.x with C# scripting for all game logic.
- SpacetimeDB for authoritative state, player/inventory/world persistence.
- WebSocket for real-time client-server communication.
- All core systems (PlayerManager, DatabaseManager, Inventory, NetworkManager) as testable C# classes.

## Risks & Mitigations

- **State divergence**: Use authoritative server, periodic snapshots, atomic actions.
- **Performance**: Optimize network loop, database queries, and Godot render loop.
- **Testing**: Enforce test-first development, integration tests with real DB.
- **Complexity**: Limit to 1 project, avoid unnecessary abstraction.

## References

- Godot C# documentation
- SpacetimeDB integration guides
- WebSocket protocol for games
- Wanderlight Online specification

---
