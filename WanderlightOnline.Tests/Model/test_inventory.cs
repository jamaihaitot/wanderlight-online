using Godot;
using GdUnit4;
using System;
using System.Linq;

namespace WanderlightOnline.Tests.Model
{
    using static Assertions;

    [TestSuite]
    public class InventoryModelTests
    {
        // Model: Inventory has exactly 12 slots for items
        // Requirements: Fixed capacity, slot indexing, bounds checking
        [TestCase]
        public static void InventorySlotCapacityManagement()
        {
            // Arrange: New Inventory instance
            var inventory = new Inventory();

            // Act & Assert: Inventory has exactly 12 slots
            AssertThat(inventory.Capacity).IsEqual(12);
            AssertThat(inventory.Slots.Count).IsEqual(12);

            // Assert: Slots are indexed from 0 to 11
            for (int i = 0; i < 12; i++)
            {
                AssertThat(inventory.GetSlot(i)).IsNull(); // Empty slots return null
            }

            // Assert: Slot access bounds are enforced
            // Test invalid slot access throws exceptions
            try { inventory.GetSlot(-1); AssertThat(false).IsTrue(); }
            catch (ArgumentOutOfRangeException) { /* Expected */ }

            try { inventory.GetSlot(12); AssertThat(false).IsTrue(); }
            catch (ArgumentOutOfRangeException) { /* Expected */ }

            try { inventory.ClearSlot(-1); AssertThat(false).IsTrue(); }
            catch (ArgumentOutOfRangeException) { /* Expected */ }

            try { inventory.ClearSlot(12); AssertThat(false).IsTrue(); }
            catch (ArgumentOutOfRangeException) { /* Expected */ }
        }

        // Model: Item stacking follows category-specific rules
        // Requirements: Stack size limits, category enforcement, overflow handling
        [TestCase]
        public static void InventoryItemStackingRules()
        {
            // Arrange: Inventory with various item types
            var inventory = new Inventory();

            // Act & Assert: Consumables/materials stack up to 20
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 20)).IsTrue();
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 1)).IsTrue(); // Should succeed - create new stack

            // Assert: Generic items follow default stacking rules (20)
            AssertThat(inventory.TryAdd("Wood", ItemCategory.Generic, 20)).IsTrue();
            AssertThat(inventory.TryAdd("Wood", ItemCategory.Generic, 1)).IsTrue(); // Should succeed - creates new stack since existing is at max

            // Assert: Equipment items stack to 1 only
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();
            // Fill remaining slots to test capacity limit  
            // Current slots: 0=Potion(20), 1=Potion(1), 2=Wood(20), 3=Wood(1), 4=Sword(1)
            // Fill slots 5-11 with equipment (7 more slots)
            for (int i = 5; i <= 11; i++)
            {
                AssertThat(inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1)).IsTrue();
            }
            AssertThat(inventory.TryAdd("FailedItem", ItemCategory.Equipment, 1)).IsFalse(); // Should fail - full inventory

            // Assert: Verify inventory is at capacity (all 12 slots filled)
            var filledSlots = inventory.Slots.Count(s => s != null);
            AssertThat(filledSlots).IsEqual(12);
        }

        // Model: Item categories determine behavior and constraints
        // Requirements: Category validation, behavior enforcement, metadata
        [TestCase]
        public static void InventoryItemCategorization()
        {
            // Arrange: Items of different categories
            var inventory = new Inventory();

            // Act & Assert: Generic category is default fallback
            AssertThat(Inventory.GetMaxStackFor(ItemCategory.Generic)).IsEqual(20);
            AssertThat(inventory.TryAdd("Wood", ItemCategory.Generic, 20)).IsTrue();

            // Assert: Consumable category enables high stacking
            AssertThat(Inventory.GetMaxStackFor(ItemCategory.Consumable)).IsEqual(20);
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 20)).IsTrue();

            // Assert: Equipment category restricts to single items
            AssertThat(Inventory.GetMaxStackFor(ItemCategory.Equipment)).IsEqual(1);
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();

            // Assert: Category cannot be changed after creation (test via ItemStack)
            var itemStack = new ItemStack("TestItem", ItemCategory.Equipment, 1);
            AssertThat(itemStack.Category).IsEqual(ItemCategory.Equipment);
            // ItemStack is immutable - category cannot be changed post-creation
        }

        // Model: ItemStack manages quantity and metadata
        // Requirements: Quantity tracking, type safety, validation
        [TestCase]
        public static void InventoryItemStackValidation()
        {
            // Arrange & Act: Create ItemStack instances
            var consumableStack = new ItemStack("Potion", ItemCategory.Consumable, 15);
            var equipmentStack = new ItemStack("Sword", ItemCategory.Equipment, 1);
            var genericStack = new ItemStack("Wood", ItemCategory.Generic, 10);

            // Assert: ItemStack has valid item_type string
            AssertThat(consumableStack.ItemType).IsEqual("Potion");
            AssertThat(equipmentStack.ItemType).IsEqual("Sword");
            AssertThat(genericStack.ItemType).IsEqual("Wood");

            // Assert: Quantity is positive integer
            AssertThat(consumableStack.Quantity > 0).IsTrue();
            AssertThat(equipmentStack.Quantity > 0).IsTrue();
            AssertThat(genericStack.Quantity > 0).IsTrue();

            // Assert: Quantity respects category-based maximums
            AssertThat(consumableStack.Quantity <= 20).IsTrue();
            AssertThat(equipmentStack.Quantity <= 1).IsTrue();
            AssertThat(genericStack.Quantity <= 20).IsTrue();

            // Assert: Invalid ItemStack creation fails
            // Test invalid ItemStack creation throws exceptions
            try { var _ = new ItemStack("", ItemCategory.Generic, 1); AssertThat(false).IsTrue(); }
            catch (ArgumentException) { /* Expected */ }

            try { var _ = new ItemStack("Valid", ItemCategory.Generic, 0); AssertThat(false).IsTrue(); }
            catch (ArgumentException) { /* Expected */ }

            try { var _ = new ItemStack("Valid", ItemCategory.Generic, -1); AssertThat(false).IsTrue(); }
            catch (ArgumentException) { /* Expected */ }
        }

        // Model: Inventory operations maintain consistency
        // Requirements: Add/remove operations, slot management, validation
        [TestCase]
        public static void InventoryOperationConsistency()
        {
            // Arrange: Inventory with partial contents
            var inventory = new Inventory();
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 10)).IsTrue();
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();

            // Act & Assert: Add operations respect capacity limits
            for (int i = 0; i < 10; i++)
            {
                AssertThat(inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1)).IsTrue();
            }
            AssertThat(inventory.TryAdd("ExtraItem", ItemCategory.Equipment, 1)).IsFalse(); // Should fail when full

            // Assert: Remove operations update quantities correctly
            AssertThat(inventory.TryRemove("Potion", 5)).IsTrue();
            var remainingPotion = inventory.Slots.FirstOrDefault(s => s?.ItemType == "Potion");
            AssertThat(remainingPotion).IsNotNull();
            AssertThat(remainingPotion!.Quantity).IsEqual(5);

            // Assert: Move operations preserve item properties
            AssertThat(inventory.Move(0, 1)).IsTrue(); // Move first item to second slot

            // Assert: Invalid operations rejected gracefully
            AssertThat(inventory.TryRemove("NonExistent", 1)).IsFalse();
            AssertThat(inventory.TryAdd("", ItemCategory.Generic, 1)).IsFalse();
        }

        // Model: Inventory state representation for persistence
        // Requirements: Serialization, state consistency, recovery
        [TestCase]
        public static void InventoryStateRepresentation()
        {
            // Arrange: Inventory with complex item arrangement
            var inventory = new Inventory();
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 15)).IsTrue();
            AssertThat(inventory.TryAdd("Wood", ItemCategory.Generic, 20)).IsTrue();

            // Act: Serialize and deserialize inventory state
            string json = inventory.ToJson();
            var deserializedInventory = Inventory.FromJson(json);

            // Assert: All slots preserved during serialization
            AssertThat(deserializedInventory.Capacity).IsEqual(inventory.Capacity);

            // Assert: ItemStack properties maintained and empty slots handled correctly
            for (int i = 0; i < inventory.Capacity; i++)
            {
                var originalSlot = inventory.GetSlot(i);
                var deserializedSlot = deserializedInventory.GetSlot(i);

                if (originalSlot == null)
                {
                    AssertThat(deserializedSlot).IsNull();
                }
                else
                {
                    AssertThat(deserializedSlot).IsNotNull();
                    AssertThat(deserializedSlot!.ItemType).IsEqual(originalSlot.ItemType);
                    AssertThat(deserializedSlot.Category).IsEqual(originalSlot.Category);
                    AssertThat(deserializedSlot.Quantity).IsEqual(originalSlot.Quantity);
                }
            }

            // Assert: Deserialized inventory functionally identical
            AssertThat(deserializedInventory.TryAdd("NewItem", ItemCategory.Generic, 1)).IsTrue();
        }
    }
}
