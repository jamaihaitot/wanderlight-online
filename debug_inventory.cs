using WanderlightOnline;
using System;

class DebugInventory {
    static void Main() {
        var inventory = new Inventory();
        
        Console.WriteLine("Testing consumable stacking:");
        bool result1 = inventory.TryAddItems("apple", 20, ItemCategory.Consumable);
        Console.WriteLine($"Added 20 apples: {result1}");
        
        bool result2 = inventory.TryAddItems("apple", 1, ItemCategory.Consumable);
        Console.WriteLine($"Added 1 more apple: {result2}");
        
        Console.WriteLine("\nTesting equipment stacking:");
        bool result3 = inventory.TryAddItems("sword", 1, ItemCategory.Equipment);
        Console.WriteLine($"Added 1 sword: {result3}");
        
        bool result4 = inventory.TryAddItems("sword", 1, ItemCategory.Equipment);
        Console.WriteLine($"Added another sword: {result4}");
        
        Console.WriteLine($"\nApple count: {inventory.GetItemCount("apple")}");
        Console.WriteLine($"Sword count: {inventory.GetItemCount("sword")}");
        
        for (int i = 0; i < 12; i++) {
            var slot = inventory.GetSlot(i);
            if (slot != null) {
                Console.WriteLine($"Slot {i}: {slot.ItemType} x{slot.Quantity} ({slot.Category})");
            }
        }
    }
}
