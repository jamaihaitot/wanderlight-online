// <copyright file="WorldItem.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using System;

    /// <summary>
    /// Represents an item existing in the game world with position, stacking, ownership, and persistence.
    /// </summary>
    public sealed class WorldItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldItem"/> class.
        /// </summary>
        /// <param name="itemType">The item type identifier (non-empty).</param>
        /// <param name="category">The item category (determines max stack).</param>
        /// <param name="quantity">The stack quantity (1..MaxStack).</param>
        /// <param name="position">The world position.</param>
        /// <param name="isPersistent">Whether the item should persist across world resets.</param>
        public WorldItem(string itemType, ItemCategory category, int quantity, Vector2 position, bool isPersistent = false)
        {
            if (string.IsNullOrWhiteSpace(itemType))
            {
                throw new ArgumentException("Item type must be a non-empty string.", nameof(itemType));
            }

            this.ItemType = itemType.Trim();
            this.Category = category;
            this.MaxStack = Inventory.GetMaxStackFor(category);

            if (quantity < 1 || quantity > this.MaxStack)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), $"Quantity must be between 1 and {this.MaxStack} for category {category}.");
            }

            this.Quantity = quantity;
            this.Position = position ?? throw new ArgumentNullException(nameof(position));
            this.IsPersistent = isPersistent;
            this.InWorld = true; // Newly created world items are present in world by default
            this.Owner = null; // Available for pickup
        }

        /// <summary>Gets the item type identifier.</summary>
        public string ItemType { get; }

        /// <summary>Gets the item category.</summary>
        public ItemCategory Category { get; }

        /// <summary>Gets the maximum allowed stack size for this item.</summary>
        public int MaxStack { get; }

        /// <summary>Gets the quantity (stack size) of the world item.</summary>
        public int Quantity { get; private set; }

        /// <summary>Gets the current world position.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Gets the current owner (reserved or picked up) player display name; null means available.</summary>
        public string? Owner { get; private set; }

        /// <summary>Gets a value indicating whether this item should persist across world resets.</summary>
        public bool IsPersistent { get; private set; }

        /// <summary>Gets a value indicating whether this item currently exists in the world (true) or is picked up (false).</summary>
        public bool InWorld { get; private set; }

        /// <summary>
        /// Attempts to update the item position.
        /// </summary>
        /// <param name="position">The new position.</param>
        /// <returns>True if updated.</returns>
        public bool TrySetPosition(Vector2 position)
        {
            if (position == null)
            {
                return false;
            }

            this.Position = position;
            return true;
        }

        /// <summary>
        /// Attempts to reserve the item for a player.
        /// </summary>
        /// <param name="displayName">The player's display name.</param>
        /// <returns>True if reservation succeeded or already reserved by same player; otherwise false.</returns>
        public bool TryReserve(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return false;
            }

            displayName = displayName.Trim();

            if (this.Owner is null)
            {
                this.Owner = displayName;
                return true;
            }

            // Idempotent reservation by the same player
            return string.Equals(this.Owner, displayName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Cancels an existing reservation.
        /// </summary>
        /// <param name="displayName">The player's display name who owns the reservation.</param>
        /// <returns>True if the reservation was cleared; otherwise false.</returns>
        public bool CancelReservation(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return false;
            }

            if (string.Equals(this.Owner, displayName.Trim(), StringComparison.Ordinal))
            {
                this.Owner = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to pickup the item by a player. Item leaves the world on success.
        /// </summary>
        /// <param name="displayName">The player's display name.</param>
        /// <returns>True if picked up successfully.</returns>
        public bool TryPickup(string displayName)
        {
            if (!this.TryReserve(displayName))
            {
                return false;
            }

            this.InWorld = false;
            return true;
        }

        /// <summary>
        /// Drops the item into the world at a specific position.
        /// </summary>
        /// <param name="position">The target drop position.</param>
        public void Drop(Vector2 position)
        {
            this.Position = position ?? throw new ArgumentNullException(nameof(position));
            this.Owner = null;
            this.InWorld = true;
        }

        /// <summary>
        /// Sets the persistence flag.
        /// </summary>
        /// <param name="persistent">The desired persistence status.</param>
        public void SetPersistence(bool persistent) => this.IsPersistent = persistent;

        /// <summary>
        /// Attempts to split this world item stack into a new world item with the requested amount.
        /// The new item is spawned at the same position and inherits persistence.
        /// </summary>
        /// <param name="amount">Amount to split (1..Quantity-1).</param>
        /// <param name="splitItem">The resulting new world item if successful.</param>
        /// <returns>True if split succeeded; otherwise false.</returns>
        public bool TrySplit(int amount, out WorldItem? splitItem)
        {
            splitItem = null;

            if (amount <= 0 || amount >= this.Quantity)
            {
                return false;
            }

            // Respect max stack limits
            if (amount > this.MaxStack)
            {
                return false;
            }

            // Reduce current and create new item with the split amount
            this.Quantity -= amount;
            splitItem = new WorldItem(this.ItemType, this.Category, amount, new Vector2(this.Position.X, this.Position.Y), this.IsPersistent);
            return true;
        }

        /// <summary>
        /// Attempts to add to this item's quantity up to MaxStack.
        /// </summary>
        /// <param name="amount">Amount to add (positive).</param>
        /// <returns>The remainder that could not be added (0 if fully added).</returns>
        public int AddUpTo(int amount)
        {
            if (amount <= 0)
            {
                return amount;
            }

            int space = this.MaxStack - this.Quantity;
            int toAdd = Math.Min(space, amount);
            this.Quantity += toAdd;
            return amount - toAdd;
        }
    }
}
