# Network Contract

## Overview

Defines the requirements and expected behaviors for networking and real-time synchronization in Wanderlight Online.

## Requirements

- WebSocket connection for real-time updates
- Message handling for player actions (join, move, pick up/drop)
- State deltas at 20 Hz; join snapshot on connect
- Mini-snapshot every 2s for correction
- Atomicity for concurrent actions
- ≤150ms p95 update latency

## Acceptance Criteria

- Real-time updates delivered within latency targets
- No duplicate or out-of-order updates
- Atomic handling of concurrent actions
- Reliable reconnection and state restoration

---
