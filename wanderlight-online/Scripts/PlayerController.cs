// <copyright file="PlayerController.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>
// PlayerController.cs

namespace WanderlightOnline
{
    using System;
    using Godot;

    /// <summary>
    /// PlayerController: Handles player input, movement, and interaction logic.
    /// Responsible for processing input, updating player position, and triggering actions (pick up, drop, interact).
    /// </summary>
    public class PlayerController
    {
        private readonly Player player;
        private readonly Inventory inventory;
        private Vector2 velocity = new Vector2(0, 0);
        private const float MoveSpeed = 3.5f;
        private const float Acceleration = 25f;
        private const float Friction = 10f;

        public PlayerController(Player player, Inventory inventory)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <summary>
        /// Processes input and updates player movement.
        /// </summary>
        public void ProcessInput(Vector2 inputDirection, float delta)
        {
            if (inputDirection.Length > 0)
            {
                // Accelerate in input direction
                this.velocity += inputDirection.Normalized() * Acceleration * delta;
                if (this.velocity.Length > MoveSpeed)
                    this.velocity = this.velocity.Normalized() * MoveSpeed;
            }
            else
            {
                // Apply friction
                if (this.velocity.Length > 0)
                {
                    var frictionAmount = Math.Min(Friction * delta, this.velocity.Length);
                    this.velocity -= this.velocity.Normalized() * frictionAmount;
                }
            }

            // Update player position
            player.Position += this.velocity * delta;
        }

        /// <summary>
        /// Attempts to pick up a world item.
        /// </summary>
        public bool TryPickUp(WorldItem item)
        {
            if (item == null || !item.IsPersistent || !item.InWorld)
                return false;
            // Try to add the item to inventory atomically
            return this.inventory.TryAdd(item.ItemType, item.Category, item.Quantity);
        }

        /// <summary>
        /// Attempts to drop an item from inventory.
        /// </summary>
        public bool TryDrop(string itemType)
        {
            if (string.IsNullOrEmpty(itemType))
                return false;
            // Try to remove one of the item type from inventory atomically
            return this.inventory.TryRemove(itemType, 1);
        }
    }
}
