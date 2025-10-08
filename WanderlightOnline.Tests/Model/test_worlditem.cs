using Godot;
using GdUnit4;

namespace WanderlightOnline.Tests.Model
{
    using static Assertions;

    [TestSuite]
    public class WorldItemModelTests
    {
        // Model: WorldItem has all required properties for game mechanics
        // Requirements: item_type, position, stack_size, owner, persistence_status
        [TestCase]
        public static void WorldItemRequiredProperties()
        {
            // Arrange
            var position = new WanderlightOnline.Vector2(10.5f, -2.25f);
            var item = new WorldItem("Potion", ItemCategory.Consumable, 5, position, isPersistent: true);

            // Assert
            AssertThat(item.ItemType).IsEqual("Potion");
            AssertThat(item.Category).IsEqual(ItemCategory.Consumable);
            AssertThat(item.Quantity).IsEqual(5);
            AssertThat(item.MaxStack).IsEqual(Inventory.GetMaxStackFor(ItemCategory.Consumable));
            AssertThat(item.Position).IsNotNull();
            AssertThat(item.Position.X).IsEqual(10.5f);
            AssertThat(item.Position.Y).IsEqual(-2.25f);
            AssertThat(item.Owner).IsNull();
            AssertThat(item.IsPersistent).IsTrue();
            AssertThat(item.InWorld).IsTrue();
        }

        // Model: Position determines world coordinates for rendering and collision
        // Requirements: Vector2 precision, coordinate validation, bounds
        [TestCase]
        public static void WorldItemPositionManagement()
        {
            // Arrange
            var item = new WorldItem("Wood", ItemCategory.Generic, 10, new WanderlightOnline.Vector2());

            // Act
            var ok = item.TrySetPosition(new WanderlightOnline.Vector2(123.456f, 789.123f));

            // Assert
            AssertThat(ok).IsTrue();
            AssertThat(item.Position.X).IsEqual(123.456f);
            AssertThat(item.Position.Y).IsEqual(789.123f);

            // Reposition and verify
            ok = item.TrySetPosition(new WanderlightOnline.Vector2(-50f, 0f));
            AssertThat(ok).IsTrue();
            AssertThat(item.Position.X).IsEqual(-50f);
            AssertThat(item.Position.Y).IsEqual(0f);
        }

        // Model: Stack size follows same rules as inventory stacking
        // Requirements: Category-based limits, validation, overflow handling
        [TestCase]
        public static void WorldItemStackSizeValidation()
        {
            // Arrange & Act
            var consumable = new WorldItem("Potion", ItemCategory.Consumable, 20, new WanderlightOnline.Vector2());
            var equipment = new WorldItem("Sword", ItemCategory.Equipment, 1, new WanderlightOnline.Vector2());
            var generic = new WorldItem("Wood", ItemCategory.Generic, 10, new WanderlightOnline.Vector2());

            // Assert: Max stack rules
            AssertThat(consumable.MaxStack).IsEqual(20);
            AssertThat(equipment.MaxStack).IsEqual(1);
            AssertThat(generic.MaxStack).IsEqual(20);

            // Assert: AddUpTo respects max
            int rem = generic.AddUpTo(15); // 10 + 10 (cap 20), remainder 5
            AssertThat(generic.Quantity).IsEqual(20);
            AssertThat(rem).IsEqual(5);

            rem = equipment.AddUpTo(1);
            AssertThat(equipment.Quantity).IsEqual(1);
            AssertThat(rem).IsEqual(1);

            // Assert: invalid creations
            try { var _ = new WorldItem("Potion", ItemCategory.Consumable, 0, new WanderlightOnline.Vector2()); AssertThat(false).IsTrue(); }
            catch (System.ArgumentOutOfRangeException) { /* expected */ }

            try { var _ = new WorldItem("", ItemCategory.Generic, 1, new WanderlightOnline.Vector2()); AssertThat(false).IsTrue(); }
            catch (System.ArgumentException) { /* expected */ }

            // Split
            var splitOk = consumable.TrySplit(5, out var splitItem);
            AssertThat(splitOk).IsTrue();
            AssertThat(splitItem).IsNotNull();
            AssertThat(consumable.Quantity).IsEqual(15);
            AssertThat(splitItem!.Quantity).IsEqual(5);
            AssertThat(splitItem.ItemType).IsEqual("Potion");
            AssertThat(splitItem.Category).IsEqual(ItemCategory.Consumable);
        }

        // Model: Owner property tracks pickup state and reservation
        // Requirements: Null for dropped items, display_name for picked up items
        [TestCase]
        public static void WorldItemOwnershipTracking()
        {
            var item = new WorldItem("Potion", ItemCategory.Consumable, 3, new WanderlightOnline.Vector2());

            // Initially available
            AssertThat(item.Owner).IsNull();
            AssertThat(item.InWorld).IsTrue();

            // Reserve and idempotent reserve
            AssertThat(item.TryReserve("Alice")).IsTrue();
            AssertThat(item.Owner).IsEqual("Alice");
            AssertThat(item.TryReserve("Alice")).IsTrue();

            // Different player cannot override reservation
            AssertThat(item.TryReserve("Bob")).IsFalse();
            AssertThat(item.Owner).IsEqual("Alice");

            // Cancel reservation
            AssertThat(item.CancelReservation("Bob")).IsFalse();
            AssertThat(item.CancelReservation("Alice")).IsTrue();
            AssertThat(item.Owner).IsNull();

            // Pickup makes item leave world
            AssertThat(item.TryPickup("Alice")).IsTrue();
            AssertThat(item.InWorld).IsFalse();
            AssertThat(item.Owner).IsEqual("Alice");
        }

        // Model: Persistence status controls database storage behavior
        // Requirements: Persistence flag, cleanup rules, temporary items
        [TestCase]
        public static void WorldItemPersistenceManagement()
        {
            var persistentItem = new WorldItem("Relic", ItemCategory.Equipment, 1, new WanderlightOnline.Vector2(), isPersistent: true);
            var tempItem = new WorldItem("Leaf", ItemCategory.Generic, 1, new WanderlightOnline.Vector2(), isPersistent: false);

            AssertThat(persistentItem.IsPersistent).IsTrue();
            AssertThat(tempItem.IsPersistent).IsFalse();

            // Toggle
            tempItem.SetPersistence(true);
            AssertThat(tempItem.IsPersistent).IsTrue();
            persistentItem.SetPersistence(false);
            AssertThat(persistentItem.IsPersistent).IsFalse();
        }

        // Model: WorldItem lifecycle from drop to pickup
        // Requirements: State transitions, validation, cleanup
        [TestCase]
        public static void WorldItemLifecycleManagement()
        {
            var dropPos = new WanderlightOnline.Vector2(5f, 6f);
            var item = new WorldItem("Wood", ItemCategory.Generic, 2, dropPos);

            // Dropped in world
            AssertThat(item.InWorld).IsTrue();
            AssertThat(item.Position.X).IsEqual(5f);
            AssertThat(item.Position.Y).IsEqual(6f);

            // Reserve and attempt split -> pickup part
            AssertThat(item.TryReserve("Charlie")).IsTrue();
            AssertThat(item.Owner).IsEqual("Charlie");

            // Drop to new pos and ensure availability cleared
            item.Drop(new WanderlightOnline.Vector2(7f, 8f));
            AssertThat(item.InWorld).IsTrue();
            AssertThat(item.Owner).IsNull();
            AssertThat(item.Position.X).IsEqual(7f);
            AssertThat(item.Position.Y).IsEqual(8f);

            // Pickup
            AssertThat(item.TryPickup("Charlie")).IsTrue();
            AssertThat(item.InWorld).IsFalse();
        }
    }
}
