# Copilot Implementation Instructions

## Overview

This file provides agent-specific instructions for GitHub Copilot to guide implementation of Wanderlight Online — MMORPG foundation.

## Key Principles

- Follow specification-driven, test-first, and simplicity mandates from the constitution.
- All game logic must be implemented as testable C# classes in Godot.
- Use SpacetimeDB for persistence and authoritative state.
- WebSocket for real-time networking.
- No unnecessary abstraction or speculative features.

## Implementation Steps

1. Implement PlayerManager, DatabaseManager, Inventory, NetworkManager, PlayerController as C# classes.
2. Write unit and integration tests before implementation (RED-GREEN-Refactor).
3. Integrate SpacetimeDB and WebSocket for state sync and persistence.
4. Ensure all contracts and data models are followed.
5. Maintain structured logging and telemetry for operator observability.
6. Document all code and systems clearly.

## Constraints

- 1 project (Godot game)
- 12 inventory slots, 50+ CCU, ≤2% missed update frames
- ≤150ms p95 update latency, ≤2s p95 join snapshot
- Atomic item actions, unique display names

---
