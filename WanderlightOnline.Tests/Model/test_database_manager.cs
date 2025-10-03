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
        public void SaveAndLoadPlayerWorks()
        {
            var db = new DatabaseManager();
            var playerId = "player1";
            var playerData = new { Name = "Alice", Level = 5 };
            AssertThat(db.SavePlayer(playerId, playerData)).IsTrue();
            var loaded = db.LoadPlayer(playerId);
            AssertThat(loaded).IsNotNull();
            AssertThat(loaded.GetType().GetProperty("Name")?.GetValue(loaded)).IsEqual("Alice");
        }

        [TestCase]
        public static void SaveAndLoadInventoryWorks()
        {
            var db = new DatabaseManager();
            var playerId = "player2";
            var inventoryData = new { Slots = 12, Items = new List<string> { "Potion" } };
            AssertThat(db.SaveInventory(playerId, inventoryData)).IsTrue();
            var loaded = db.LoadInventory(playerId);
            AssertThat(loaded).IsNotNull();
            var slotsProp = loaded.GetType().GetProperty("Slots");
            var slotsValue = slotsProp?.GetValue(loaded);
            AssertThat(slotsValue).IsNotNull();
            AssertThat(slotsValue).IsNotNull();
            AssertThat((int)slotsValue!).IsEqual(12);
        }

        [TestCase]
        public void SaveAndLoadWorldItemWorks()
        {
            var db = new DatabaseManager();
            var itemId = "item1";
            var itemData = new { Type = "Sword", Position = new { X = 1, Y = 2 } };
            AssertThat(db.SaveWorldItem(itemId, itemData)).IsTrue();
            var loaded = db.LoadWorldItem(itemId);
            AssertThat(loaded).IsNotNull();
            var typeProp = loaded.GetType().GetProperty("Type");
            var typeValue = typeProp?.GetValue(loaded);
            AssertThat(typeValue).IsNotNull();
            AssertThat(typeValue).IsEqual("Sword");
        }

        [TestCase]
        public void SaveAndLoadPlayerStateWorks()
        {
            var db = new DatabaseManager();
            var playerId = "player3";
            var playerData = new { Name = "Bob" };
            var inventoryData = new { Slots = 12 };
            var worldItems = new List<object> { new { Type = "Gem" } };
            AssertThat(db.SavePlayerState(playerId, playerData, inventoryData, worldItems)).IsTrue();
            var (player, inventory, items) = db.LoadPlayerState(playerId);
            AssertThat(player).IsNotNull();
            AssertThat(inventory).IsNotNull();
            AssertThat(items.Count > 0).IsTrue();
        }
    }
}
