// <copyright file="DatabaseManager.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>
// DatabaseManager.cs
namespace WanderlightOnline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// DatabaseManager: Handles persistence and retrieval of player, inventory, and world state using SpacetimeDB.
    /// All actions are atomic and consistent. Integrates with SpacetimeDB (stubbed for now).
    /// </summary>
    public class DatabaseManager
    {
        // Simulated in-memory storage for demonstration/testing
        private readonly Dictionary<string, object> playerStore = new();
        private readonly Dictionary<string, object> inventoryStore = new();
        private readonly Dictionary<string, object> worldItemStore = new();

        /// <summary>
        /// Saves or updates a player entity atomically.
        /// </summary>
        public bool SavePlayer(string playerId, object playerData)
        {
            if (string.IsNullOrEmpty(playerId) || playerData == null)
                return false;
            lock (playerStore)
            {
                playerStore[playerId] = playerData;
                return true;
            }
        }

        /// <summary>
        /// Loads a player entity by ID.
        /// </summary>
        public object? LoadPlayer(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return null;
            lock (playerStore)
            {
                playerStore.TryGetValue(playerId, out var data);
                return data;
            }
        }

        /// <summary>
        /// Saves or updates an inventory entity atomically.
        /// </summary>
        public bool SaveInventory(string playerId, object inventoryData)
        {
            if (string.IsNullOrEmpty(playerId) || inventoryData == null)
                return false;
            lock (inventoryStore)
            {
                inventoryStore[playerId] = inventoryData;
                return true;
            }
        }

        /// <summary>
        /// Loads an inventory entity by player ID.
        /// </summary>
        public object? LoadInventory(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return null;
            lock (inventoryStore)
            {
                inventoryStore.TryGetValue(playerId, out var data);
                return data;
            }
        }

        /// <summary>
        /// Saves or updates a world item atomically.
        /// </summary>
        public bool SaveWorldItem(string itemId, object worldItemData)
        {
            if (string.IsNullOrEmpty(itemId) || worldItemData == null)
                return false;
            lock (worldItemStore)
            {
                worldItemStore[itemId] = worldItemData;
                return true;
            }
        }

        /// <summary>
        /// Loads a world item by ID.
        /// </summary>
        public object? LoadWorldItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;
            lock (worldItemStore)
            {
                worldItemStore.TryGetValue(itemId, out var data);
                return data;
            }
        }

        /// <summary>
        /// Atomically persists all state for a player (player, inventory, world items).
        /// </summary>
        public bool SavePlayerState(string playerId, object playerData, object inventoryData, IEnumerable<object> worldItems)
        {
            if (string.IsNullOrEmpty(playerId) || playerData == null || inventoryData == null || worldItems == null)
                return false;
            lock (playerStore)
                lock (inventoryStore)
                    lock (worldItemStore)
                    {
                        playerStore[playerId] = playerData;
                        inventoryStore[playerId] = inventoryData;
                        foreach (var item in worldItems)
                        {
                            // Assume item has a string ID property (stub)
                            var id = Guid.NewGuid().ToString();
                            worldItemStore[id] = item;
                        }
                        return true;
                    }
        }

        /// <summary>
        /// Loads all state for a player (player, inventory, world items).
        /// </summary>
        public (object? player, object? inventory, List<object> worldItems) LoadPlayerState(string playerId)
        {
            var worldItems = new List<object>();
            lock (playerStore)
                lock (inventoryStore)
                    lock (worldItemStore)
                    {
                        playerStore.TryGetValue(playerId, out var player);
                        inventoryStore.TryGetValue(playerId, out var inventory);
                        foreach (var item in worldItemStore.Values)
                        {
                            worldItems.Add(item);
                        }
                        return (player, inventory, worldItems);
                    }
        }

        /// <summary>
        /// Simulates a persistence flush (for test/atomicity).
        /// </summary>
        public void Flush() { /* No-op for stub */ }
    }
}
