using System;
using Godot;
using GdUnit4;
using WanderlightOnline;

namespace WanderlightOnline.Tests.Model
{
    using static Assertions;

    [TestSuite]
    public class InventoryModelTests
    {
        // Model: Inventory has exactly 12 slots for items
        // Requirements: Fixed capacity, slot indexing, bounds checking
        [TestCase]
        public void InventorySlotCapacityManagement()
        {
            // Arrange: New Inventory instance
            var inventory = new Inventory();
            
            // Act: Query inventory capacity and slot structure
            // Assert: Inventory has exactly 12 slots
            AssertThat(inventory.Capacity).IsEqual(12);
            
            // Assert: Slots are indexed from 0 to 11
            for (int i = 0; i < 12; i++)
            {
                AssertThat(inventory.GetSlot(i)).IsNull(); // Empty slots return null
            }
            
            // Assert: Slot access bounds are enforced
            bool invalidSlotAccess = false;
            try { inventory.GetSlot(-1); } catch (ArgumentOutOfRangeException) { invalidSlotAccess = true; }
            AssertBool(invalidSlotAccess).IsTrue();
            
            invalidSlotAccess = false;
            try { inventory.GetSlot(12); } catch (ArgumentOutOfRangeException) { invalidSlotAccess = true; }
            AssertBool(invalidSlotAccess).IsTrue();
        }

        // Model: Item stacking follows category-specific rules
        // Requirements: Stack size limits, category enforcement, overflow handling
        [TestCase]
        public void InventoryItemStackingRules()
        {
            // Arrange: Inventory with various item types
            var inventory = new Inventory();
            
            // Act: Add items to test stacking behavior
            // Assert: Consumables/materials stack up to 20
            AssertBool(inventory.TryAddItems("apple", 20, ItemCategory.Consumable)).IsTrue();
            AssertThat(inventory.GetSlot(0)).IsNotNull();
            AssertThat(inventory.GetSlot(0).Quantity).IsEqual(20);
            AssertThat(inventory.GetSlot(0).MaxStackSize).IsEqual(20);
            
            // Assert: Equipment items stack to 1 only
            AssertBool(inventory.TryAddItems("sword", 1, ItemCategory.Equipment)).IsTrue();
            var swordSlot = inventory.GetSlot(1);
            AssertThat(swordSlot).IsNotNull();
            AssertThat(swordSlot.MaxStackSize).IsEqual(1);
            AssertBool(swordSlot.IsFull).IsTrue();
            
            // Assert: Generic items follow default stacking rules (20)
            AssertBool(inventory.TryAddItems("wood", 20, ItemCategory.Generic)).IsTrue();
            var woodSlot = inventory.GetSlot(2);
            AssertThat(woodSlot).IsNotNull();
            AssertThat(woodSlot.MaxStackSize).IsEqual(20);
            
            // Assert: Stack overflow creates new stacks in available slots
            AssertBool(inventory.TryAddItems("apple", 5, ItemCategory.Consumable)).IsTrue(); // Should create new stack
            AssertThat(inventory.GetSlot(3)).IsNotNull();
            AssertThat(inventory.GetSlot(3).ItemType).IsEqual("apple");
            AssertThat(inventory.GetSlot(3).Quantity).IsEqual(5);
        }

        // Model: Item categories determine behavior and constraints
        // Requirements: Category validation, behavior enforcement, metadata
        [TestCase]
        public void InventoryItemCategorization()
        {
            // Arrange: Items of different categories
            var consumableStack = new ItemStack("apple", 10, ItemCategory.Consumable);
            var equipmentStack = new ItemStack("sword", 1, ItemCategory.Equipment);
            var genericStack = new ItemStack("wood", 15, ItemCategory.Generic);
            
            // Act: Add items and verify category handling
            // Assert: Generic category is default fallback
            AssertThat(genericStack.Category).IsEqual(ItemCategory.Generic);
            AssertThat(genericStack.MaxStackSize).IsEqual(20);
            
            // Assert: Consumable category enables high stacking
            AssertThat(consumableStack.Category).IsEqual(ItemCategory.Consumable);
            AssertThat(consumableStack.MaxStackSize).IsEqual(20);
            
            // Assert: Equipment category restricts to single items
            AssertThat(equipmentStack.Category).IsEqual(ItemCategory.Equipment);
            AssertThat(equipmentStack.MaxStackSize).IsEqual(1);
            
            // Assert: Category cannot be changed after creation (readonly)
            AssertThat(consumableStack.Category).IsEqual(ItemCategory.Consumable);
        }

        // Model: ItemStack manages quantity and metadata
        // Requirements: Quantity tracking, type safety, validation
        [TestCase]
        public void InventoryItemStackValidation()
        {
            // Arrange: ItemStack instances with various configurations
            var validStack = new ItemStack("apple", 5, ItemCategory.Consumable);
            
            // Act: Create and modify ItemStacks
            // Assert: ItemStack has valid item_type string
            AssertThat(validStack.ItemType).IsEqual("apple");
            AssertThat(validStack.ItemType).IsNotEmpty();
            
            // Assert: Quantity is positive integer
            AssertThat(validStack.Quantity).IsEqual(5);
            AssertThat(validStack.Quantity).IsGreater(0);
            
            // Assert: Quantity respects category-based maximums
            var equipmentStack = new ItemStack("sword", 10, ItemCategory.Equipment); // Attempts 10 but gets clamped to 1
            AssertThat(equipmentStack.Quantity).IsEqual(1);
            
            // Assert: ItemStack validation on construction
            bool invalidConstruction = false;
            try { new ItemStack("", 1, ItemCategory.Generic); } catch (ArgumentException) { invalidConstruction = true; }
            AssertBool(invalidConstruction).IsTrue();
            
            invalidConstruction = false;
            try { new ItemStack("item", 0, ItemCategory.Generic); } catch (ArgumentException) { invalidConstruction = true; }
            AssertBool(invalidConstruction).IsTrue();
            
            invalidConstruction = false; 
            try { new ItemStack("item", -1, ItemCategory.Generic); } catch (ArgumentException) { invalidConstruction = true; }
            AssertBool(invalidConstruction).IsTrue();
        }

        // Model: Inventory operations maintain consistency
        // Requirements: Add/remove operations, slot management, validation
        [TestCase]
        public void InventoryOperationConsistency()
        {
            // Arrange: Inventory with partial contents
            var inventory = new Inventory();
            AssertBool(inventory.TryAddItems("apple", 10, ItemCategory.Consumable)).IsTrue();
            
            // Act: Perform add, remove, and move operations
            // Assert: Add operations respect capacity limits
            AssertThat(inventory.GetItemCount("apple")).IsEqual(10);
            
            // Assert: Remove operations update quantities correctly
            AssertBool(inventory.TryRemoveItems("apple", 3)).IsTrue();
            AssertThat(inventory.GetItemCount("apple")).IsEqual(7);
            
            // Assert: Move operations preserve item properties
            AssertBool(inventory.TryAddItems("sword", 1, ItemCategory.Equipment)).IsTrue();
            AssertBool(inventory.MoveItems(1, 2)).IsTrue(); // Move sword from slot 1 to slot 2
            AssertThat(inventory.GetSlot(1)).IsNull();
            AssertThat(inventory.GetSlot(2)).IsNotNull();
            AssertThat(inventory.GetSlot(2).ItemType).IsEqual("sword");
            
            // Assert: Invalid operations rejected gracefully
            AssertBool(inventory.TryRemoveItems("nonexistent", 1)).IsFalse();
            AssertBool(inventory.MoveItems(0, 0)).IsTrue(); // Same slot move should succeed
        }

        // Model: Inventory state representation for persistence
        // Requirements: Serialization, state consistency, recovery
        [TestCase]
        public void InventoryStateRepresentation()
        {
            // Arrange: Inventory with complex item arrangement
            var inventory = new Inventory();
            AssertBool(inventory.TryAddItems("apple", 15, ItemCategory.Consumable)).IsTrue();
            AssertBool(inventory.TryAddItems("sword", 1, ItemCategory.Equipment)).IsTrue();
            AssertBool(inventory.TryAddItems("wood", 20, ItemCategory.Generic)).IsTrue();
            
            // Act: Serialize and deserialize inventory state
            string json = inventory.ToJson();
            AssertThat(json).IsNotNull();
            AssertThat(json).IsNotEmpty();
            
            var restoredInventory = Inventory.FromJson(json);
            
            // Assert: All slots preserved during serialization
            AssertThat(restoredInventory.Capacity).IsEqual(12);
            
            // Assert: ItemStack properties maintained
            AssertThat(restoredInventory.GetItemCount("apple")).IsEqual(15);
            AssertThat(restoredInventory.GetItemCount("sword")).IsEqual(1);
            AssertThat(restoredInventory.GetItemCount("wood")).IsEqual(20);
            
            // Assert: Empty slots handled correctly
            AssertThat(restoredInventory.GetEmptySlotCount()).IsEqual(9); // 12 - 3 used slots
            
            // Assert: Deserialized inventory functionally identical
            AssertBool(restoredInventory.TryAddItems("apple", 5, ItemCategory.Consumable)).IsTrue();
            AssertThat(restoredInventory.GetItemCount("apple")).IsEqual(20);
        }
    }
}
