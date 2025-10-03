using System;
using GdUnit4;
using static GdUnit4.Assertions;
using WanderlightOnline;
using System.Collections.Generic;

namespace WanderlightOnline.Tests.Model
{
    [TestSuite]
    public class PlayerControllerModelTests
    {
        [TestCase]
        public static void ProcessInputMovesPlayerCorrectly()
        {
            var player = new Player("TestPlayer");
            player.Position = new Vector2(0, 0);
            var inventory = new Inventory();
            var controller = new PlayerController(player, inventory);
            var input = new Vector2(1, 0); // Move right
            controller.ProcessInput(input, 1.0f); // 1 second delta
            AssertThat(player.Position.X).IsGreater(0);
            AssertThat(player.Position.Y).IsEqual(0);
        }

        [TestCase]
        public static void TryPickUpAddsItemToInventory()
        {
            var player = new Player("TestPlayer");
            player.Position = new Vector2(0, 0);
            var inventory = new Inventory();
            var controller = new PlayerController(player, inventory);
            var item = new WorldItem("Potion", ItemCategory.Consumable, 1, new Vector2(0, 0), true);
            AssertThat(controller.TryPickUp(item)).IsTrue();
            AssertThat(inventory.GetSlot(0)).IsNotNull();
            AssertThat(inventory.GetSlot(0)!.ItemType).IsEqual("Potion");
        }

        [TestCase]
        public static void TryDropRemovesItemFromInventory()
        {
            var player = new Player("TestPlayer");
            player.Position = new Vector2(0, 0);
            var inventory = new Inventory();
            var controller = new PlayerController(player, inventory);
            // Add item first
            inventory.TryAdd("Potion", ItemCategory.Consumable, 1);
            AssertThat(controller.TryDrop("Potion")).IsTrue();
            AssertThat(inventory.GetSlot(0)).IsNull();
        }
    }
}
