// <copyright file="ItemStack.cs" company="Wanderlight Online">
// Copyright (c) 2025 Wanderlight Online
// </copyright>

namespace WanderlightOnline
{
    using System;

    /// <summary>
    /// Represents a stack of items of the same type and category.
    /// </summary>
    public sealed class ItemStack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemStack"/> class.
        /// </summary>
        /// <param name="itemType">The item type identifier (non-empty).</param>
        /// <param name="category">The item category.</param>
        /// <param name="quantity">The initial quantity (1..MaxStack).</param>
        public ItemStack(string itemType, ItemCategory category, int quantity)
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
        }

        /// <summary>Gets the item type identifier.</summary>
        public string ItemType { get; }

        /// <summary>Gets the item category.</summary>
        public ItemCategory Category { get; }

        /// <summary>Gets the maximum allowed stack size for this item.</summary>
        public int MaxStack { get; }

        /// <summary>Gets the quantity of items in the stack.</summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// Attempts to add items to the stack up to MaxStack.
        /// </summary>
        /// <param name="amount">Amount to add (must be positive).</param>
        /// <returns>The amount that could not be added (remainder). Zero means fully added.</returns>
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

        /// <summary>
        /// Removes up to the specified amount from the stack.
        /// </summary>
        /// <param name="amount">The amount to remove (must be positive).</param>
        /// <returns>The amount actually removed.</returns>
        public int RemoveUpTo(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int removed = Math.Min(this.Quantity, amount);
            this.Quantity -= removed;
            return removed;
        }

        /// <summary>
        /// Creates a deep copy of this stack.
        /// </summary>
        /// <returns>A new <see cref="ItemStack"/> with identical values.</returns>
        public ItemStack Clone() => new ItemStack(this.ItemType, this.Category, this.Quantity);
    }
}
