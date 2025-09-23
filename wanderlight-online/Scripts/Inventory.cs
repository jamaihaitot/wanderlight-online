using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WanderlightOnline
{
    /// <summary>
    /// Represents a player's inventory with 12 fixed slots and item management operations.
    /// </summary>
    public class Inventory
    {
        private const int MaxSlots = 12;
        private readonly ItemStack[] slots;

        /// <summary>
        /// Gets the maximum number of slots in the inventory.
        /// </summary>
        [JsonIgnore]
        public int Capacity => MaxSlots;

        /// <summary>
        /// Gets the inventory slots for serialization.
        /// </summary>
        [JsonPropertyName("slots")]
        public ItemStack[] Slots => slots;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class.
        /// </summary>
        public Inventory()
        {
            slots = new ItemStack[MaxSlots];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inventory"/> class from serialized data.
        /// </summary>
        /// <param name="slots">The serialized slots data.</param>
        [JsonConstructor]
        public Inventory(ItemStack[] slots)
        {
            this.slots = new ItemStack[MaxSlots];
            if (slots != null)
            {
                for (int i = 0; i < Math.Min(slots.Length, MaxSlots); i++)
                {
                    this.slots[i] = slots[i];
                }
            }
        }

        /// <summary>
        /// Gets the item stack at the specified slot index.
        /// </summary>
        /// <param name="index">The slot index (0-11).</param>
        /// <returns>The ItemStack at the slot, or null if empty.</returns>
        public ItemStack GetSlot(int index)
        {
            ValidateSlotIndex(index);
            return slots[index];
        }

        /// <summary>
        /// Sets the item stack at the specified slot index.
        /// </summary>
        /// <param name="index">The slot index (0-11).</param>
        /// <param name="stack">The ItemStack to set, or null to clear the slot.</param>
        public void SetSlot(int index, ItemStack stack)
        {
            ValidateSlotIndex(index);
            slots[index] = stack;
        }

        /// <summary>
        /// Attempts to add items to the inventory atomically.
        /// </summary>
        /// <param name="itemType">The type of item to add.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <param name="category">The category of the item.</param>
        /// <returns>True if all items were added successfully, false otherwise.</returns>
        public bool TryAddItems(string itemType, int quantity, ItemCategory category)
        {
            if (string.IsNullOrWhiteSpace(itemType) || quantity <= 0)
                return false;

            // Find existing stacks of the same item type
            var existingStacks = new List<int>();
            for (int i = 0; i < MaxSlots; i++)
            {
                if (slots[i] != null && slots[i].ItemType == itemType && slots[i].Category == category)
                {
                    existingStacks.Add(i);
                }
            }

            // Try to merge with existing stacks first
            int remainingQuantity = quantity;
            foreach (int slotIndex in existingStacks)
            {
                var slot = slots[slotIndex];
                if (!slot.IsFull)
                {
                    int spaceAvailable = slot.MaxStackSize - slot.Quantity;
                    int amountToAdd = Math.Min(remainingQuantity, spaceAvailable);
                    remainingQuantity -= amountToAdd;
                }
            }

            // If we still have items to add, check if we can create new stacks
            if (remainingQuantity > 0)
            {
                int emptySlots = GetEmptySlotCount();
                int maxStackSize = new ItemStack("temp", 1, category).MaxStackSize;
                int stacksNeeded = (remainingQuantity + maxStackSize - 1) / maxStackSize; // Ceiling division
                
                if (stacksNeeded > emptySlots)
                    return false; // Not enough space
            }

            // If we reach here, we can fit everything - now actually add the items
            remainingQuantity = quantity;
            
            // First pass: fill existing stacks
            foreach (int slotIndex in existingStacks)
            {
                var slot = slots[slotIndex];
                if (!slot.IsFull && remainingQuantity > 0)
                {
                    int amountAdded = slot.AddItems(remainingQuantity);
                    remainingQuantity -= amountAdded;
                }
            }

            // Second pass: create new stacks in empty slots
            for (int i = 0; i < MaxSlots && remainingQuantity > 0; i++)
            {
                if (slots[i] == null)
                {
                    int maxStackSize = new ItemStack("temp", 1, category).MaxStackSize;
                    int amountForThisSlot = Math.Min(remainingQuantity, maxStackSize);
                    slots[i] = new ItemStack(itemType, amountForThisSlot, category);
                    remainingQuantity -= amountForThisSlot;
                }
            }

            return remainingQuantity == 0;
        }

        /// <summary>
        /// Attempts to remove items from the inventory atomically.
        /// </summary>
        /// <param name="itemType">The type of item to remove.</param>
        /// <param name="quantity">The quantity to remove.</param>
        /// <returns>True if all items were removed successfully, false otherwise.</returns>
        public bool TryRemoveItems(string itemType, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemType) || quantity <= 0)
                return false;

            // First pass: Check if we have enough items
            var removePlan = new List<(int slotIndex, int amount)>();
            int remainingToRemove = quantity;

            for (int i = 0; i < MaxSlots && remainingToRemove > 0; i++)
            {
                var slot = slots[i];
                if (slot != null && slot.ItemType == itemType)
                {
                    int amountToRemove = Math.Min(remainingToRemove, slot.Quantity);
                    removePlan.Add((i, amountToRemove));
                    remainingToRemove -= amountToRemove;
                }
            }

            // If we don't have enough items, fail atomically
            if (remainingToRemove > 0)
                return false;

            // Second pass: Actually remove the items
            foreach ((int slotIndex, int amount) in removePlan)
            {
                slots[slotIndex].RemoveItems(amount);
                if (slots[slotIndex].IsEmpty)
                {
                    slots[slotIndex] = null;
                }
            }

            return true;
        }

        /// <summary>
        /// Moves items from one slot to another, merging or swapping as appropriate.
        /// </summary>
        /// <param name="fromIndex">The source slot index.</param>
        /// <param name="toIndex">The destination slot index.</param>
        /// <returns>True if the move was successful, false otherwise.</returns>
        public bool MoveItems(int fromIndex, int toIndex)
        {
            ValidateSlotIndex(fromIndex);
            ValidateSlotIndex(toIndex);

            if (fromIndex == toIndex)
                return true;

            var fromStack = slots[fromIndex];
            var toStack = slots[toIndex];

            if (fromStack == null)
                return false;

            // If destination is empty, just move
            if (toStack == null)
            {
                slots[toIndex] = fromStack;
                slots[fromIndex] = null;
                return true;
            }

            // If stacks can be merged, merge them
            if (fromStack.CanMergeWith(toStack))
            {
                int spaceAvailable = toStack.MaxStackSize - toStack.Quantity;
                int amountToMove = Math.Min(fromStack.Quantity, spaceAvailable);
                
                toStack.AddItems(amountToMove);
                fromStack.RemoveItems(amountToMove);

                if (fromStack.IsEmpty)
                {
                    slots[fromIndex] = null;
                }

                return true;
            }

            // Otherwise, swap the stacks
            slots[fromIndex] = toStack;
            slots[toIndex] = fromStack;
            return true;
        }

        /// <summary>
        /// Swaps items between two slots.
        /// </summary>
        /// <param name="slotA">The first slot index.</param>
        /// <param name="slotB">The second slot index.</param>
        /// <returns>True if the swap was successful, false otherwise.</returns>
        public bool SwapSlots(int slotA, int slotB)
        {
            ValidateSlotIndex(slotA);
            ValidateSlotIndex(slotB);

            var stackA = slots[slotA];
            var stackB = slots[slotB];

            slots[slotA] = stackB;
            slots[slotB] = stackA;

            return true;
        }

        /// <summary>
        /// Clears the specified slot.
        /// </summary>
        /// <param name="index">The slot index to clear.</param>
        public void ClearSlot(int index)
        {
            ValidateSlotIndex(index);
            slots[index] = null;
        }

        /// <summary>
        /// Gets the total quantity of a specific item type in the inventory.
        /// </summary>
        /// <param name="itemType">The item type to count.</param>
        /// <returns>The total quantity of the item.</returns>
        public int GetItemCount(string itemType)
        {
            if (string.IsNullOrWhiteSpace(itemType))
                return 0;

            return slots
                .Where(slot => slot != null && slot.ItemType == itemType)
                .Sum(slot => slot.Quantity);
        }

        /// <summary>
        /// Gets the number of empty slots in the inventory.
        /// </summary>
        /// <returns>The number of empty slots.</returns>
        public int GetEmptySlotCount()
        {
            return slots.Count(slot => slot == null);
        }

        /// <summary>
        /// Checks if the inventory is empty.
        /// </summary>
        /// <returns>True if all slots are empty, false otherwise.</returns>
        public bool IsEmpty()
        {
            return slots.All(slot => slot == null);
        }

        /// <summary>
        /// Checks if the inventory is full.
        /// </summary>
        /// <returns>True if all slots are occupied, false otherwise.</returns>
        public bool IsFull()
        {
            return slots.All(slot => slot != null);
        }

        /// <summary>
        /// Serializes the inventory to JSON.
        /// </summary>
        /// <returns>JSON representation of the inventory.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Deserializes an inventory from JSON.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized inventory.</returns>
        public static Inventory FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Inventory();

            return JsonSerializer.Deserialize<Inventory>(json) ?? new Inventory();
        }

        /// <summary>
        /// Validates that a slot index is within the valid range.
        /// </summary>
        /// <param name="index">The slot index to validate.</param>
        private void ValidateSlotIndex(int index)
        {
            if (index < 0 || index >= MaxSlots)
                throw new ArgumentOutOfRangeException(nameof(index), $"Slot index must be between 0 and {MaxSlots - 1}");
        }
    }
}