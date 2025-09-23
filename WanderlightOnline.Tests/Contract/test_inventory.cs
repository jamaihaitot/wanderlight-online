using System;
using Godot;
using GdUnit4;
using WanderlightOnline;

namespace WanderlightOnline.Tests.Contract
{
    using static Assertions;

    [TestSuite]
    public class InventoryContractTests
    {
        [TestCase]
        public void InventoryEnforcesCapacityAndStacking()
        {
            // Test 12 slot capacity limit and stacking rules
            var inventory = new Inventory();
            
            // Test capacity
            AssertThat(inventory.Capacity).IsEqual(12);
            
            // Test consumable stacking (max 20 per stack, multiple stacks allowed)
            AssertBool(inventory.TryAddItems("apple", 20, ItemCategory.Consumable)).IsTrue();
            AssertBool(inventory.TryAddItems("apple", 5, ItemCategory.Consumable)).IsTrue(); // Should succeed - new stack
            AssertThat(inventory.GetItemCount("apple")).IsEqual(25);
            
            // Test equipment stacking (max 1 per stack, multiple stacks allowed)
            AssertBool(inventory.TryAddItems("sword", 1, ItemCategory.Equipment)).IsTrue();
            AssertBool(inventory.TryAddItems("sword", 1, ItemCategory.Equipment)).IsTrue(); // Should succeed - new stack
            AssertThat(inventory.GetItemCount("sword")).IsEqual(2);
        }

        [TestCase]
        public void InventoryActionsAreAtomic()
        {
            // Test that inventory operations are atomic
            var inventory = new Inventory();
            
            // Fill most slots
            for (int i = 0; i < 11; i++)
            {
                AssertBool(inventory.TryAddItems($"item{i}", 1, ItemCategory.Equipment)).IsTrue();
            }
            
            // This should fail because we don't have enough slots for 2 more equipment items
            AssertBool(inventory.TryAddItems("bigitem", 2, ItemCategory.Equipment)).IsFalse();
            
            // Inventory should be unchanged (atomic failure)
            AssertThat(inventory.GetItemCount("bigitem")).IsEqual(0);
        }

        [TestCase]
        public void InventoryPersistsAcrossSessions()
        {
            // Test that inventory state is saved and restored across sessions
            var inventory = new Inventory();
            AssertBool(inventory.TryAddItems("apple", 5, ItemCategory.Consumable)).IsTrue();
            AssertBool(inventory.TryAddItems("sword", 1, ItemCategory.Equipment)).IsTrue();
            
            // Serialize to JSON
            string json = inventory.ToJson();
            AssertThat(json).IsNotNull();
            
            // Deserialize from JSON
            var restoredInventory = Inventory.FromJson(json);
            AssertThat(restoredInventory.GetItemCount("apple")).IsEqual(5);
            AssertThat(restoredInventory.GetItemCount("sword")).IsEqual(1);
        }

        [TestCase]
        public void InventoryEnforcesServerSideValidation()
        {
            // Test that all inventory rules are enforced server-side
            var inventory = new Inventory();
            
            // Test invalid inputs are rejected
            AssertBool(inventory.TryAddItems("", 5, ItemCategory.Consumable)).IsFalse(); // Empty item name
            AssertBool(inventory.TryAddItems("apple", 0, ItemCategory.Consumable)).IsFalse(); // Zero quantity
            AssertBool(inventory.TryAddItems("apple", -1, ItemCategory.Consumable)).IsFalse(); // Negative quantity
        }

        [TestCase]
        public void InventoryHandlesItemCategoriesCorrectly()
        {
            // Test that different item categories behave according to their rules
            var inventory = new Inventory();
            
            // Generic items (max 20 per stack, multiple stacks allowed)
            AssertBool(inventory.TryAddItems("wood", 20, ItemCategory.Generic)).IsTrue();
            AssertBool(inventory.TryAddItems("wood", 10, ItemCategory.Generic)).IsTrue(); // Should succeed - new stack
            
            // Consumable items (max 20 per stack, multiple stacks allowed)
            AssertBool(inventory.TryAddItems("potion", 20, ItemCategory.Consumable)).IsTrue();
            AssertBool(inventory.TryAddItems("potion", 5, ItemCategory.Consumable)).IsTrue(); // Should succeed - new stack
            
            // Equipment items (max 1 per stack)
            AssertBool(inventory.TryAddItems("armor", 1, ItemCategory.Equipment)).IsTrue();
            AssertBool(inventory.TryAddItems("armor", 1, ItemCategory.Equipment)).IsTrue(); // Should succeed - new stack in different slot
        }

        [TestCase]
        public void InventoryHandlesEdgeCases()
        {
            // Test edge cases and error conditions
            var inventory = new Inventory();
            
            // Test slot bounds - validate negative case by testing valid operations
            var validSlot = inventory.GetSlot(0);
            AssertThat(validSlot).IsNull(); // Empty slot returns null
            
            // Test edge operations
            bool invalidSlotAccess = false;
            try { inventory.GetSlot(-1); } catch (ArgumentOutOfRangeException) { invalidSlotAccess = true; }
            AssertBool(invalidSlotAccess).IsTrue();
            
            invalidSlotAccess = false;
            try { inventory.GetSlot(12); } catch (ArgumentOutOfRangeException) { invalidSlotAccess = true; }
            AssertBool(invalidSlotAccess).IsTrue();
            
            // Test empty inventory operations
            AssertBool(inventory.TryRemoveItems("nonexistent", 1)).IsFalse();
            AssertThat(inventory.GetItemCount("nonexistent")).IsEqual(0);
            AssertBool(inventory.IsEmpty()).IsTrue();
            AssertBool(inventory.IsFull()).IsFalse();
        }
    }
}
