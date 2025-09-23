using System;
using System.Text.Json.Serialization;

namespace WanderlightOnline
{
    /// <summary>
    /// Represents a stack of items with quantity and category-based limits.
    /// </summary>
    public class ItemStack
    {
        /// <summary>
        /// Gets the type/name of the item.
        /// </summary>
        [JsonPropertyName("item_type")]
        public string ItemType { get; private set; }

        /// <summary>
        /// Gets the current quantity in the stack.
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; private set; }

        /// <summary>
        /// Gets the category of the item.
        /// </summary>
        [JsonPropertyName("category")]
        public ItemCategory Category { get; private set; }

        /// <summary>
        /// Gets the maximum stack size for this item's category.
        /// </summary>
        [JsonIgnore]
        public int MaxStackSize => Category switch
        {
            ItemCategory.Consumable => 20,
            ItemCategory.Equipment => 1,
            ItemCategory.Generic => 20,
            _ => 20
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemStack"/> class.
        /// </summary>
        /// <param name="itemType">The type/name of the item.</param>
        /// <param name="quantity">The initial quantity.</param>
        /// <param name="category">The category of the item.</param>
        [JsonConstructor]
        public ItemStack(string itemType, int quantity, ItemCategory category)
        {
            if (string.IsNullOrWhiteSpace(itemType))
                throw new ArgumentException("Item type cannot be null or empty", nameof(itemType));
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            ItemType = itemType;
            Category = category;
            Quantity = Math.Min(quantity, MaxStackSize);
        }

        /// <summary>
        /// Attempts to add items to this stack.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <returns>The amount that was actually added.</returns>
        public int AddItems(int amount)
        {
            if (amount <= 0)
                return 0;

            int spaceAvailable = MaxStackSize - Quantity;
            int amountToAdd = Math.Min(amount, spaceAvailable);
            Quantity += amountToAdd;
            return amountToAdd;
        }

        /// <summary>
        /// Attempts to remove items from this stack.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>The amount that was actually removed.</returns>
        public int RemoveItems(int amount)
        {
            if (amount <= 0)
                return 0;

            int amountToRemove = Math.Min(amount, Quantity);
            Quantity -= amountToRemove;
            return amountToRemove;
        }

        /// <summary>
        /// Checks if this stack can be merged with another stack.
        /// </summary>
        /// <param name="other">The other stack to check.</param>
        /// <returns>True if the stacks can be merged.</returns>
        public bool CanMergeWith(ItemStack other)
        {
            return other != null && 
                   ItemType == other.ItemType && 
                   Category == other.Category &&
                   Quantity < MaxStackSize;
        }

        /// <summary>
        /// Gets whether this stack is empty.
        /// </summary>
        [JsonIgnore]
        public bool IsEmpty => Quantity <= 0;

        /// <summary>
        /// Gets whether this stack is full.
        /// </summary>
        [JsonIgnore]
        public bool IsFull => Quantity >= MaxStackSize;

        /// <summary>
        /// Creates a copy of this ItemStack.
        /// </summary>
        /// <returns>A new ItemStack with the same properties.</returns>
        public ItemStack Clone()
        {
            return new ItemStack(ItemType, Quantity, Category);
        }
    }
}