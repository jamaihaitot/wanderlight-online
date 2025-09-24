using Godot;
using GdUnit4;
using System.Linq;

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

            // Verify default capacity is 12
            AssertThat(inventory.Capacity).IsEqual(12);

            // Test consumable stacking (should stack up to 20)
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 20)).IsTrue();
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 1)).IsTrue(); // Should succeed - add to existing stack

            // Test equipment stacking (should not stack - max 1 per slot)
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue(); // Should create new stack in different slot

            // Fill remaining slots to test capacity limits - inventory currently has Potion (stacked) and 2 Swords
            // So we have 4 slots used: [Potion(20), Potion(1), Sword(1), Sword(1), empty, empty, ...]
            for (int i = 4; i <= 11; i++) // Fill remaining 8 slots to reach capacity
            {
                AssertThat(inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1)).IsTrue();
            }

            // Now inventory is full - should fail to add more
            AssertThat(inventory.TryAdd("NewItem", ItemCategory.Equipment, 1)).IsFalse(); // Should fail - no empty slots

            // Test generic stacking (default to 20 like consumables)
            var inventory2 = new Inventory();
            AssertThat(inventory2.TryAdd("Wood", ItemCategory.Generic, 20)).IsTrue();
            AssertThat(inventory2.TryAdd("Wood", ItemCategory.Generic, 1)).IsTrue(); // Should succeed - add to existing stack
        }

        [TestCase]
        public void InventoryActionsAreAtomic()
        {
            // Test that inventory operations are atomic
            var inventory = new Inventory();

            // Try to add more than can fit - should fail completely (atomic)
            AssertThat(inventory.TryAdd("Item", ItemCategory.Consumable, 250)).IsFalse(); // 250 > 12 slots * 20 max = 240
            AssertThat(inventory.Slots.All(s => s == null)).IsTrue(); // Inventory should be empty after failed operation

            // Try to remove items that don't exist - should fail atomically
            AssertThat(inventory.TryRemove("NonExistentItem", 1)).IsFalse();

            // Add some items successfully
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 15)).IsTrue();

            // Try to remove more than available - should fail atomically and leave original amount
            AssertThat(inventory.TryRemove("Potion", 20)).IsFalse();
            var potionStack = inventory.Slots.FirstOrDefault(s => s?.ItemType == "Potion");
            AssertThat(potionStack).IsNotNull();
            AssertThat(potionStack!.Quantity).IsEqual(15); // Original amount should remain
        }

        [TestCase]
        public void InventoryPersistsAcrossSessions()
        {
            // Test that inventory state is saved and restored across sessions
            var inventory = new Inventory();

            // Add some items
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();
            AssertThat(inventory.TryAdd("Potion", ItemCategory.Consumable, 15)).IsTrue();

            // Serialize to JSON
            string json = inventory.ToJson();
            AssertThat(json).IsNotEmpty();

            // Deserialize to new inventory
            var restoredInventory = Inventory.FromJson(json);

            // Verify state is preserved
            AssertThat(restoredInventory.Capacity).IsEqual(inventory.Capacity);
            var swordStack = restoredInventory.Slots.FirstOrDefault(s => s?.ItemType == "Sword");
            var potionStack = restoredInventory.Slots.FirstOrDefault(s => s?.ItemType == "Potion");

            AssertThat(swordStack).IsNotNull();
            AssertThat(swordStack!.Quantity).IsEqual(1);
            AssertThat(swordStack.Category).IsEqual(ItemCategory.Equipment);

            AssertThat(potionStack).IsNotNull();
            AssertThat(potionStack!.Quantity).IsEqual(15);
            AssertThat(potionStack.Category).IsEqual(ItemCategory.Consumable);
        }

        [TestCase]
        public void InventoryEnforcesServerSideValidation()
        {
            // Test that all inventory rules are enforced server-side
            var inventory = new Inventory();

            // Test capacity enforcement - cannot exceed 12 slots
            for (int i = 0; i < 12; i++)
            {
                AssertThat(inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1)).IsTrue();
            }
            // 13th item should fail
            AssertThat(inventory.TryAdd("Item13", ItemCategory.Equipment, 1)).IsFalse();

            // Test stacking rule enforcement
            var inventory2 = new Inventory();
            AssertThat(inventory2.TryAdd("Potion", ItemCategory.Consumable, 20)).IsTrue();
            AssertThat(inventory2.TryAdd("Potion", ItemCategory.Consumable, 1)).IsTrue(); // Should succeed - creates new stack since existing is at max

            // Test input validation
            AssertThat(inventory2.TryAdd("", ItemCategory.Generic, 1)).IsFalse(); // Empty item name
            AssertThat(inventory2.TryAdd("ValidItem", ItemCategory.Generic, 0)).IsFalse(); // Zero quantity
            AssertThat(inventory2.TryAdd("ValidItem", ItemCategory.Generic, -1)).IsFalse(); // Negative quantity
        }

        [TestCase]
        public void InventoryHandlesItemCategoriesCorrectly()
        {
            // Test category-specific behavior and stacking rules
            var inventory = new Inventory();

            // Test consumables stack to 20
            AssertThat(Inventory.GetMaxStackFor(ItemCategory.Consumable)).IsEqual(20);
            AssertThat(inventory.TryAdd("HealthPotion", ItemCategory.Consumable, 20)).IsTrue();

            // Test equipment doesn't stack (max 1)
            AssertThat(Inventory.GetMaxStackFor(ItemCategory.Equipment)).IsEqual(1);
            AssertThat(inventory.TryAdd("Sword", ItemCategory.Equipment, 1)).IsTrue();
            // Fill inventory to test equipment can't add beyond capacity
            // We have: Slot 0=HealthPotion(20), Slot 1=Sword(1), need to fill slots 2-11 with equipment
            for (int i = 2; i <= 11; i++) // Fill remaining 10 slots (2-11) 
            {
                AssertThat(inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1)).IsTrue();
            }
            AssertThat(inventory.TryAdd("NewSword", ItemCategory.Equipment, 1)).IsFalse(); // Should fail - inventory full

            // Test generic stacks to 20 (default behavior)
            var inventory2 = new Inventory(); // Fresh inventory for testing Generic category
            AssertThat(Inventory.GetMaxStackFor(ItemCategory.Generic)).IsEqual(20);
            AssertThat(inventory2.TryAdd("Wood", ItemCategory.Generic, 20)).IsTrue();

            // Verify categories don't mix when stacking
            AssertThat(inventory2.TryAdd("HealthPotion", ItemCategory.Consumable, 20)).IsTrue(); // Add consumable version first
            AssertThat(inventory2.TryAdd("HealthPotion", ItemCategory.Generic, 1)).IsTrue(); // Should create separate stack
            var healthPotionStacks = inventory2.Slots.Where(s => s?.ItemType == "HealthPotion").ToList();
            AssertThat(healthPotionStacks.Count).IsEqual(2); // One consumable, one generic
        }

        [TestCase]
        public void InventoryHandlesEdgeCases()
        {
            // Test edge cases and error conditions
            var inventory = new Inventory();

            // Test invalid input handling
            AssertThat(inventory.TryAdd(null!, ItemCategory.Generic, 1)).IsFalse();
            AssertThat(inventory.TryAdd("", ItemCategory.Generic, 1)).IsFalse();
            AssertThat(inventory.TryAdd("   ", ItemCategory.Generic, 1)).IsFalse(); // Whitespace only
            AssertThat(inventory.TryAdd("ValidItem", ItemCategory.Generic, 0)).IsFalse();
            AssertThat(inventory.TryAdd("ValidItem", ItemCategory.Generic, -5)).IsFalse();

            // Test remove edge cases
            AssertThat(inventory.TryRemove(null!, 1)).IsFalse();
            AssertThat(inventory.TryRemove("", 1)).IsFalse();
            AssertThat(inventory.TryRemove("NonExistent", 1)).IsFalse();
            AssertThat(inventory.TryRemove("ValidItem", 0)).IsFalse();
            AssertThat(inventory.TryRemove("ValidItem", -1)).IsFalse();

            // Test capacity edge case - fill to maximum
            for (int i = 0; i < 12; i++)
            {
                AssertThat(inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1)).IsTrue();
            }
            AssertThat(inventory.TryAdd("ExtraItem", ItemCategory.Equipment, 1)).IsFalse(); // Should fail when full
        }
    }
}
