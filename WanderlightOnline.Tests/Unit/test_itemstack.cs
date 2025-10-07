using System;
using GdUnit4;
using static GdUnit4.Assertions;
using WanderlightOnline;

namespace WanderlightOnline.Tests.Unit
{
    /// <summary>
    /// Unit tests for the ItemStack class.
    /// Tests validation, boundary conditions, and stack operations.
    /// </summary>
    [TestSuite]
    public class ItemStackUnitTests
    {
        // Test: Constructor validates item type
        [TestCase]
        public void ConstructorValidatesItemType()
        {
            // Valid item type should work
            var stack = new ItemStack("wood", ItemCategory.Generic, 5);
            AssertThat(stack.ItemType).IsEqual("wood");

            // Null item type should throw
            try
            {
                var invalid = new ItemStack(null, ItemCategory.Generic, 1);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentException)
            {
                // Expected
            }

            // Empty item type should throw
            try
            {
                var invalid = new ItemStack(string.Empty, ItemCategory.Generic, 1);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentException)
            {
                // Expected
            }

            // Whitespace-only item type should throw
            try
            {
                var invalid = new ItemStack("   ", ItemCategory.Generic, 1);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        // Test: Constructor trims whitespace from item type
        [TestCase]
        public void ConstructorTrimsItemType()
        {
            var stack = new ItemStack("  wood  ", ItemCategory.Generic, 1);
            AssertThat(stack.ItemType).IsEqual("wood");
        }

        // Test: Constructor validates quantity against MaxStack
        [TestCase]
        public void ConstructorValidatesQuantity()
        {
            // Valid quantities
            var generic = new ItemStack("item", ItemCategory.Generic, 15);
            AssertThat(generic.Quantity).IsEqual(15);

            var consumable = new ItemStack("potion", ItemCategory.Consumable, 20);
            AssertThat(consumable.Quantity).IsEqual(20);

            var equipment = new ItemStack("sword", ItemCategory.Equipment, 1);
            AssertThat(equipment.Quantity).IsEqual(1);

            // Quantity too low (0)
            try
            {
                var invalid = new ItemStack("item", ItemCategory.Generic, 0);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected
            }

            // Quantity too high for Generic (max 20)
            try
            {
                var invalid = new ItemStack("item", ItemCategory.Generic, 21);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected
            }

            // Quantity too high for Consumable (max 20)
            try
            {
                var invalid = new ItemStack("potion", ItemCategory.Consumable, 21);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected
            }

            // Quantity too high for Equipment (max 1)
            try
            {
                var invalid = new ItemStack("sword", ItemCategory.Equipment, 2);
                AssertThat(false).IsTrue(); // Should not reach here
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected
            }
        }

        // Test: MaxStack is set correctly based on category
        [TestCase]
        public void MaxStackSetCorrectlyByCategory()
        {
            var generic = new ItemStack("item", ItemCategory.Generic, 1);
            AssertThat(generic.MaxStack).IsEqual(20);

            var consumable = new ItemStack("potion", ItemCategory.Consumable, 1);
            AssertThat(consumable.MaxStack).IsEqual(20);

            var equipment = new ItemStack("sword", ItemCategory.Equipment, 1);
            AssertThat(equipment.MaxStack).IsEqual(1);
        }

        // Test: AddUpTo adds items correctly
        [TestCase]
        public void AddUpToAddsCorrectly()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 5);

            // Add within capacity
            var remainder = stack.AddUpTo(3);
            AssertThat(stack.Quantity).IsEqual(8);
            AssertThat(remainder).IsEqual(0);

            // Add exactly to capacity
            remainder = stack.AddUpTo(12);
            AssertThat(stack.Quantity).IsEqual(20);
            AssertThat(remainder).IsEqual(0);

            // Try to add more than capacity
            remainder = stack.AddUpTo(5);
            AssertThat(stack.Quantity).IsEqual(20); // Still at max
            AssertThat(remainder).IsEqual(5); // All returned as remainder
        }

        // Test: AddUpTo handles overflow correctly
        [TestCase]
        public void AddUpToHandlesOverflow()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 17);

            // Try to add 5, but only room for 3
            var remainder = stack.AddUpTo(5);
            AssertThat(stack.Quantity).IsEqual(20); // At max
            AssertThat(remainder).IsEqual(2); // 2 couldn't fit
        }

        // Test: AddUpTo handles zero and negative amounts
        [TestCase]
        public void AddUpToHandlesInvalidAmounts()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 5);

            // Zero amount
            var remainder = stack.AddUpTo(0);
            AssertThat(stack.Quantity).IsEqual(5); // Unchanged
            AssertThat(remainder).IsEqual(0);

            // Negative amount
            remainder = stack.AddUpTo(-3);
            AssertThat(stack.Quantity).IsEqual(5); // Unchanged
            AssertThat(remainder).IsEqual(-3); // Returned as-is
        }

        // Test: RemoveUpTo removes items correctly
        [TestCase]
        public void RemoveUpToRemovesCorrectly()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 10);

            // Remove some items
            var removed = stack.RemoveUpTo(3);
            AssertThat(stack.Quantity).IsEqual(7);
            AssertThat(removed).IsEqual(3);

            // Remove exactly remaining
            removed = stack.RemoveUpTo(7);
            AssertThat(stack.Quantity).IsEqual(0);
            AssertThat(removed).IsEqual(7);
        }

        // Test: RemoveUpTo handles insufficient quantity
        [TestCase]
        public void RemoveUpToHandlesInsufficient()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 5);

            // Try to remove more than available
            var removed = stack.RemoveUpTo(10);
            AssertThat(stack.Quantity).IsEqual(0); // All removed
            AssertThat(removed).IsEqual(5); // Only 5 were available
        }

        // Test: RemoveUpTo handles zero and negative amounts
        [TestCase]
        public void RemoveUpToHandlesInvalidAmounts()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 5);

            // Zero amount
            var removed = stack.RemoveUpTo(0);
            AssertThat(stack.Quantity).IsEqual(5); // Unchanged
            AssertThat(removed).IsEqual(0);

            // Negative amount
            removed = stack.RemoveUpTo(-3);
            AssertThat(stack.Quantity).IsEqual(5); // Unchanged
            AssertThat(removed).IsEqual(0); // Nothing removed
        }

        // Test: Clone creates an independent copy
        [TestCase]
        public void CloneCreatesIndependentCopy()
        {
            var original = new ItemStack("wood", ItemCategory.Generic, 5);
            var clone = original.Clone();

            // Same values
            AssertThat(clone.ItemType).IsEqual(original.ItemType);
            AssertThat(clone.Category).IsEqual(original.Category);
            AssertThat(clone.Quantity).IsEqual(original.Quantity);
            AssertThat(clone.MaxStack).IsEqual(original.MaxStack);

            // Independent - modifying clone doesn't affect original
            clone.AddUpTo(3);
            AssertThat(clone.Quantity).IsEqual(8);
            AssertThat(original.Quantity).IsEqual(5);
        }

        // Test: Properties are read-only (except Quantity via operations)
        [TestCase]
        public void PropertiesAreReadOnly()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 5);

            // ItemType, Category, and MaxStack should be readonly
            // (this is enforced by the compiler, but we verify behavior)
            var itemType = stack.ItemType;
            var category = stack.Category;
            var maxStack = stack.MaxStack;

            // These values should remain constant
            stack.AddUpTo(1);
            AssertThat(stack.ItemType).IsEqual(itemType);
            AssertThat(stack.Category).IsEqual(category);
            AssertThat(stack.MaxStack).IsEqual(maxStack);
        }

        // Test: Full stack can't add more
        [TestCase]
        public void FullStackCannotAddMore()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 20); // Max capacity

            var remainder = stack.AddUpTo(1);
            AssertThat(stack.Quantity).IsEqual(20);
            AssertThat(remainder).IsEqual(1);
        }

        // Test: Empty stack operations
        [TestCase]
        public void EmptyStackOperations()
        {
            var stack = new ItemStack("wood", ItemCategory.Generic, 1);

            // Empty the stack
            stack.RemoveUpTo(1);
            AssertThat(stack.Quantity).IsEqual(0);

            // Can't remove from empty
            var removed = stack.RemoveUpTo(5);
            AssertThat(removed).IsEqual(0);
            AssertThat(stack.Quantity).IsEqual(0);

            // Can add to empty
            var remainder = stack.AddUpTo(5);
            AssertThat(stack.Quantity).IsEqual(5);
            AssertThat(remainder).IsEqual(0);
        }

        // Test: Equipment category stacking behavior
        [TestCase]
        public void EquipmentStackingBehavior()
        {
            var equipment = new ItemStack("sword", ItemCategory.Equipment, 1);

            // Equipment maxes at 1
            AssertThat(equipment.MaxStack).IsEqual(1);
            AssertThat(equipment.Quantity).IsEqual(1);

            // Can't add more
            var remainder = equipment.AddUpTo(1);
            AssertThat(equipment.Quantity).IsEqual(1);
            AssertThat(remainder).IsEqual(1);
        }

        // Test: Consumable category stacking behavior
        [TestCase]
        public void ConsumableStackingBehavior()
        {
            var consumable = new ItemStack("potion", ItemCategory.Consumable, 15);

            // Consumable maxes at 20
            AssertThat(consumable.MaxStack).IsEqual(20);

            // Can add up to 20
            var remainder = consumable.AddUpTo(10);
            AssertThat(consumable.Quantity).IsEqual(20);
            AssertThat(remainder).IsEqual(5); // 5 couldn't fit
        }
    }
}
