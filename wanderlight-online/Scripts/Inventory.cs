// Copyright (c) 2025 Wanderlight Online
// Inventory.cs
namespace WanderlightOnline
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Inventory for player items. Fixed-slot array with atomic operations and serialization support.
    /// Integrated with DatabaseManager for persistent storage.
    /// </summary>
    public class Inventory
    {
        private const int DefaultCapacity = 12;

        private readonly ItemStack?[] slots;
        private readonly DatabaseManager? databaseManager;
        private SpacetimeDB.Identity? ownerId; // Player identity for SpacetimeDB persistence

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class with default capacity (12).
        /// </summary>
        public Inventory()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        /// <param name="capacity">The fixed number of slots (must be 12 to satisfy contract).</param>
        /// <param name="owner">Optional player identity for SpacetimeDB persistence.</param>
        public Inventory(int capacity, SpacetimeDB.Identity? owner = null)
        {
            if (capacity != DefaultCapacity)
            {
                // Enforce exact contract to avoid subtle mismatches
                throw new ArgumentOutOfRangeException(nameof(capacity), $"Inventory capacity must be {DefaultCapacity}.");
            }

            this.slots = new ItemStack?[capacity];
            this.Capacity = capacity;
            this.ownerId = owner;
            this.databaseManager = owner.HasValue ? DatabaseManager.Instance : null;
        }

        /// <summary>
        /// Gets the maximum number of items the inventory can hold (slots).
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Gets a read-only snapshot of the current slots.
        /// </summary>
        public ReadOnlyCollection<ItemStack?> Slots => Array.AsReadOnly(this.slots);

        /// <summary>
        /// Returns the maximum stack size for a given category.
        /// </summary>
        /// <param name="category">The item category.</param>
        /// <returns>The maximum stack size allowed for the category.</returns>
        public static int GetMaxStackFor(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Consumable => 20,
                ItemCategory.Equipment => 1,
                _ => 20, // Default stacking rule for Generic
            };
        }

        /// <summary>
        /// Deserializes inventory state from JSON.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An <see cref="Inventory"/> instance representing the saved state.</returns>
        public static Inventory FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var dto = JsonSerializer.Deserialize<InventoryDto>(json, options)
                ?? throw new ArgumentException("Invalid inventory JSON", nameof(json));

            var inv = new Inventory(dto.Capacity);
            if (dto.Slots.Length != inv.Capacity)
            {
                throw new InvalidOperationException("Deserialized slot count does not match capacity.");
            }

            for (int i = 0; i < dto.Slots.Length; i++)
            {
                var s = dto.Slots[i];
                inv.slots[i] = s == null ? null : new ItemStack(s.ItemType!, s.Category, s.Quantity);
            }

            return inv;
        }

        /// <summary>Gets the stack at a given index or null.</summary>
        /// <param name="index">The slot index to retrieve.</param>
        /// <returns>The <see cref="ItemStack"/> at the index, or null if empty.</returns>
        public ItemStack? GetSlot(int index)
        {
            this.ValidateIndex(index);
            return this.slots[index];
        }

        /// <summary>
        /// Attempts to add the entire quantity of an item atomically. No partial state on failure.
        /// Note: For persistence, caller should invoke DatabaseManager.SaveInventory() with the modified ItemStacks after successful add.
        /// </summary>
        /// <param name="itemType">Item identifier.</param>
        /// <param name="category">Item category.</param>
        /// <param name="quantity">Quantity to add (must be positive).</param>
        /// <returns>True if fully added; false if not enough space (no changes made).</returns>
        public bool TryAdd(string itemType, ItemCategory category, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemType))
            {
                return false;
            }

            if (quantity <= 0)
            {
                return false;
            }

            int maxPerStack = GetMaxStackFor(category);

            // Quick feasibility check: compute space available across slots (existing stacks + empties)
            int remaining = quantity;

            // Phase 1: simulate fill into existing compatible stacks
            foreach (var s in this.slots)
            {
                if (s is not null && s.ItemType == itemType && s.Category == category)
                {
                    int space = maxPerStack - s.Quantity;
                    if (space > 0)
                    {
                        int toAdd = Math.Min(space, remaining);
                        remaining -= toAdd;
                        if (remaining == 0)
                        {
                            break;
                        }
                    }
                }
            }

            // Phase 2: simulate creating new stacks in empty slots
            if (remaining > 0)
            {
                int emptySlots = this.slots.Count(s => s is null);
                int stacksNeeded = (int)Math.Ceiling(remaining / (double)maxPerStack);
                if (stacksNeeded > emptySlots)
                {
                    return false; // Not enough space to accommodate fully
                }
            }

            // Perform the actual mutation (guaranteed to succeed fully)
            remaining = quantity;

            // Fill existing stacks
            for (int i = 0; i < this.slots.Length && remaining > 0; i++)
            {
                var s = this.slots[i];
                if (s is not null && s.ItemType == itemType && s.Category == category)
                {
                    remaining = s.AddUpTo(remaining);
                }
            }

            // Create new stacks in empty slots
            for (int i = 0; i < this.slots.Length && remaining > 0; i++)
            {
                if (this.slots[i] is null)
                {
                    int toCreate = Math.Min(maxPerStack, remaining);
                    this.slots[i] = new ItemStack(itemType.Trim(), category, toCreate);
                    remaining -= toCreate;
                }
            }

            // Persist changes if database manager is available
            if (this.databaseManager != null && this.ownerId.HasValue && remaining == 0)
            {
                // Save each modified ItemStack to SpacetimeDB
                foreach (var stack in this.slots.Where(s => s != null))
                {
                    this.databaseManager.SaveInventory(stack!);
                }
            }

            return remaining == 0;
        }

        /// <summary>
        /// Attempts to remove the specified quantity of an item across stacks atomically.
        /// Note: For persistence, caller should invoke DatabaseManager.SaveInventory() after successful remove.
        /// </summary>
        /// <param name="itemType">Item identifier.</param>
        /// <param name="quantity">Quantity to remove (positive).</param>
        /// <returns>True if fully removed; false if not enough quantity (no changes made).</returns>
        public bool TryRemove(string itemType, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemType) || quantity <= 0)
            {
                return false;
            }

            int total = this.slots.Where(s => s is not null && s.ItemType == itemType).Sum(s => s!.Quantity);
            if (total < quantity)
            {
                return false;
            }

            // Perform removal
            int remaining = quantity;
            for (int i = 0; i < this.slots.Length && remaining > 0; i++)
            {
                var s = this.slots[i];
                if (s is not null && s.ItemType == itemType)
                {
                    int removed = s.RemoveUpTo(remaining);
                    remaining -= removed;
                    if (s.Quantity == 0)
                    {
                        this.slots[i] = null; // Clear empty stacks
                    }
                }
            }

            // Persist changes if database manager is available
            if (this.databaseManager != null && this.ownerId.HasValue)
            {
                // Save each modified ItemStack to SpacetimeDB
                foreach (var stack in this.slots.Where(s => s != null))
                {
                    this.databaseManager.SaveInventory(stack!);
                }
            }

            return true;
        }

        /// <summary>
        /// Moves a stack from one slot to another, merging when possible.
        /// </summary>
        /// <param name="from">Source slot index.</param>
        /// <param name="to">Destination slot index.</param>
        /// <returns>True if any change occurred.</returns>
        public bool Move(int from, int to)
        {
            this.ValidateIndex(from);
            this.ValidateIndex(to);
            if (from == to)
            {
                return false;
            }

            var src = this.slots[from];
            var dst = this.slots[to];
            if (src is null)
            {
                return false;
            }

            if (dst is null)
            {
                this.slots[to] = src;
                this.slots[from] = null;
                return true;
            }

            // Merge if same type/category and space available
            if (dst.ItemType == src.ItemType && dst.Category == src.Category && dst.Quantity < dst.MaxStack)
            {
                int remainder = dst.AddUpTo(src.Quantity);
                if (remainder == 0)
                {
                    this.slots[from] = null;
                }
                else
                {
                    // Some remained, update src to remainder
                    this.slots[from] = new ItemStack(src.ItemType, src.Category, remainder);
                }

                return true;
            }

            // Otherwise swap
            this.slots[to] = src;
            this.slots[from] = dst;
            return true;
        }

        /// <summary>
        /// Swaps two slots regardless of content.
        /// </summary>
        /// <param name="a">The first slot index.</param>
        /// <param name="b">The second slot index.</param>
        public void Swap(int a, int b)
        {
            this.ValidateIndex(a);
            this.ValidateIndex(b);
            if (a == b)
            {
                return;
            }

            (this.slots[a], this.slots[b]) = (this.slots[b], this.slots[a]);
        }

        /// <summary>
        /// Clears a slot.
        /// </summary>
        /// <param name="index">The slot index to clear.</param>
        public void ClearSlot(int index)
        {
            this.ValidateIndex(index);
            this.slots[index] = null;
        }

        /// <summary>
        /// Serializes inventory state to JSON for persistence.
        /// </summary>
        /// <returns>A compact JSON string representing the inventory state.</returns>
        public string ToJson()
        {
            var dto = new InventoryDto
            {
                Capacity = this.Capacity,
                Slots = this.slots.Select(s => s == null ? null : new ItemStackDto
                {
                    ItemType = s.ItemType,
                    Category = s.Category,
                    Quantity = s.Quantity,
                }).ToArray(),
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter() },
            };

            return JsonSerializer.Serialize(dto, options);
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= this.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {this.Capacity - 1}.");
            }
        }

        private sealed class InventoryDto
        {
            public int Capacity { get; set; }

            public ItemStackDto?[] Slots { get; set; } = Array.Empty<ItemStackDto?>();
        }

        private sealed class ItemStackDto
        {
            public string? ItemType { get; set; }

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ItemCategory Category { get; set; }

            public int Quantity { get; set; }
        }
    }
}
