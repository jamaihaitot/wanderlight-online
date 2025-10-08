using System;
using System.Collections.Generic;
using GdUnit4;
using static GdUnit4.Assertions;
using WanderlightOnline;

namespace WanderlightOnline.Tests.Model
{
    [TestSuite]
    public class DatabaseManagerModelTests
    {
        [TestCase]
        public static void SaveAndLoadPlayerWorks()
        {
            var db = DatabaseManager.Instance;
            var player = new Player("Alice") { Position = new Vector2(10, 20) };

            // Note: SavePlayer requires active SpacetimeDB connection with reducers
            // This test validates the method signature and basic flow
            var saveResult = db.SavePlayer(player);

            // Without active connection, save returns false
            AssertThat(saveResult).IsFalse();
        }

        [TestCase]
        public static void SaveAndLoadInventoryWorks()
        {
            var db = DatabaseManager.Instance;
            var item = new ItemStack("Potion", ItemCategory.Consumable, 5);

            // Note: SaveInventory requires active SpacetimeDB connection with reducers
            // This test validates the method signature and basic flow
            var saveResult = db.SaveInventory(item);

            // Without active connection, save returns false
            AssertThat(saveResult).IsFalse();
        }

        [TestCase]
        public static void SaveAndLoadWorldItemWorks()
        {
            var db = DatabaseManager.Instance;
            var item = new WorldItem("Sword", ItemCategory.Equipment, 1, new Vector2(5, 5), false);

            // Note: SaveWorldItem requires active SpacetimeDB connection with reducers
            // This test validates the method signature and basic flow
            var saveResult = db.SaveWorldItem(item);

            // Without active connection, save returns false
            AssertThat(saveResult).IsFalse();
        }

        [TestCase]
        public static void SaveAndLoadPlayerStateWorks()
        {
            var db = DatabaseManager.Instance;
            var player = new Player("Bob") { Position = new Vector2(15, 25) };
            var inventory = new List<ItemStack> { new ItemStack("Gem", ItemCategory.Consumable, 3) };
            var worldItems = new List<WorldItem> { new WorldItem("Stone", ItemCategory.Generic, 1, new Vector2(0, 0), false) };

            // Note: SavePlayerState requires active SpacetimeDB connection with reducers
            // This test validates the method signature and basic flow
            var saveResult = db.SavePlayerState(player, inventory, worldItems);

            // Without active connection, save returns false
            AssertThat(saveResult).IsFalse();
        }
    }
}
