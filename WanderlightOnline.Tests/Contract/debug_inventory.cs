using GdUnit4;

using WanderlightOnline;

using static GdUnit4.Assertions;

[TestSuite]
public partial class DebugInventoryTests
{
    [TestCase]
    public void DebugInventoryCapacity()
    {
        var inventory = new Inventory();

        // Add 21 potions - should take 2 slots (20+1)
        bool result1 = inventory.TryAdd("Potion", ItemCategory.Consumable, 21);
        AssertThat(result1).IsTrue();

        // Add 2 swords - should take 2 slots (1+1)
        bool result2 = inventory.TryAdd("Sword", ItemCategory.Equipment, 1);
        bool result3 = inventory.TryAdd("Sword", ItemCategory.Equipment, 1);
        AssertThat(result2).IsTrue();
        AssertThat(result3).IsTrue();

        // Count used slots
        int usedSlots = 0;
        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            if (inventory.Slots[i] != null)
            {
                usedSlots++;
                System.Console.WriteLine($"Slot {i}: {inventory.Slots[i]!.ItemType} x{inventory.Slots[i]!.Quantity} ({inventory.Slots[i]!.Category})");
            }
        }
        System.Console.WriteLine($"Used slots so far: {usedSlots}");
        System.Console.WriteLine($"Available slots: {12 - usedSlots}");

        // Try to fill remaining slots
        int addCount = 0;
        for (int i = 4; i <= 12; i++)
        {
            bool canAdd = inventory.TryAdd($"Item{i}", ItemCategory.Equipment, 1);
            System.Console.WriteLine($"Add Item{i}: {canAdd}");
            if (canAdd) addCount++;
            if (!canAdd) break;
        }

        System.Console.WriteLine($"Successfully added {addCount} more items");

        // Check final state
        usedSlots = 0;
        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            if (inventory.Slots[i] != null)
            {
                usedSlots++;
            }
        }
        System.Console.WriteLine($"Final used slots: {usedSlots}");

        // Try to add one more - should fail
        bool finalAdd = inventory.TryAdd("NewItem", ItemCategory.Equipment, 1);
        System.Console.WriteLine($"Add NewItem (should fail): {finalAdd}");

        // The test expects this to be false, but we're checking what actually happens
        AssertThat(finalAdd).IsFalse();
    }
}