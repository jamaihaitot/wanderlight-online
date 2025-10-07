// <copyright file="test_latency.cs" company="Wanderlight Online">
// Copyright (c) Wanderlight Online. All rights reserved.
// </copyright>

using GdUnit4;
using static GdUnit4.Assertions;
using System.Diagnostics;
using System.Collections.Generic;

namespace WanderlightOnline.Tests.Performance
{
    /// <summary>
    /// Performance tests ensuring operations meet <200ms latency requirements.
    /// These tests verify that core game operations complete quickly enough for real-time gameplay.
    /// </summary>
    [TestSuite]
    public class LatencyPerformanceTests
    {
        [TestCase]
        public void MovementSynchronizationUnder200Ms()
        {
            // Arrange: Mock movement update
            var stopwatch = new Stopwatch();
            var position1 = new Vector2(0.0f, 0.0f);
            var position2 = new Vector2(100.0f, 100.0f);

            // Act: Measure calculation time
            stopwatch.Start();
            var distance = (position2 - position1).Length;
            var normalized = (position2 - position1).Normalized();
            stopwatch.Stop();

            // Assert: Movement calculations complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
        }

        [TestCase]
        public void MultiplePlayerUpdatesUnder200Ms()
        {
            // Arrange: Simulate multiple player position updates
            var stopwatch = new Stopwatch();
            var positions = new List<Vector2>();
            for (int i = 0; i < 50; i++)
            {
                positions.Add(new Vector2(i * 10.0f, i * 10.0f));
            }

            // Act: Measure batch position processing time
            stopwatch.Start();
            float totalDistance = 0;
            for (int i = 0; i < positions.Count - 1; i++)
            {
                totalDistance += (positions[i + 1] - positions[i]).Length;
            }

            stopwatch.Stop();

            // Assert: Batch updates complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
        }

        [TestCase]
        public void ItemStackOperationsUnder200Ms()
        {
            // Arrange: Create item stack operations
            var stopwatch = new Stopwatch();
            var stacks = new List<ItemStack>();

            // Act: Measure item stack manipulation time
            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                var stack = new ItemStack("item", ItemCategory.Generic, 10);
                stack.AddUpTo(5);
                stack.RemoveUpTo(3);
                var clone = stack.Clone();
                stacks.Add(clone);
            }

            stopwatch.Stop();

            // Assert: Item operations complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
            AssertThat(stacks.Count).IsEqual(100);
        }

        [TestCase]
        public void VectorMathBenchmarkUnder10Ms()
        {
            // Arrange: Vector math operations
            var stopwatch = new Stopwatch();
            var v1 = new Vector2(10.0f, 20.0f);
            var v2 = new Vector2(30.0f, 40.0f);

            // Act: Measure 1000 vector operations
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                var sum = v1 + v2;
                var diff = v1 - v2;
                var scaled = v1 * 2.0f;
                var normalized = v1.Normalized();
                var length = v1.Length;
            }

            stopwatch.Stop();

            // Assert: 1000 vector operations complete in <10ms (should be very fast)
            AssertThat(stopwatch.ElapsedMilliseconds < 10).IsTrue();
        }

        [TestCase]
        public void BatchItemStackCreationUnder200Ms()
        {
            // Arrange: Simulate batch creation
            var stopwatch = new Stopwatch();
            var stacks = new List<ItemStack>();

            // Act: Create many item stacks (simulating world items or player inventories)
            stopwatch.Start();
            for (int i = 0; i < 500; i++)
            {
                var stack = new ItemStack($"item_{i % 10}", ItemCategory.Generic, 1);
                stacks.Add(stack);
            }

            stopwatch.Stop();

            // Assert: Batch operations complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
            AssertThat(stacks.Count).IsEqual(500);
        }

        [TestCase]
        public void ComplexVectorOperationsUnder200Ms()
        {
            // Arrange: Complex vector operations (movement prediction, pathfinding)
            var stopwatch = new Stopwatch();
            var waypoints = new List<Vector2>();

            // Act: Calculate complex movement paths
            stopwatch.Start();
            var current = new Vector2(0, 0);
            for (int i = 0; i < 100; i++)
            {
                var target = new Vector2(i * 15.0f, i * 15.0f);
                var direction = (target - current).Normalized();
                var distance = (target - current).Length;
                var steps = (int)(distance / 10.0f);

                for (int j = 0; j < steps; j++)
                {
                    current = current + (direction * 10.0f);
                    waypoints.Add(current);
                }
            }

            stopwatch.Stop();

            // Assert: Complex calculations complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
        }

        [TestCase]
        public void ItemCategoryOperationsUnder200Ms()
        {
            // Arrange: Test category-specific operations
            var stopwatch = new Stopwatch();
            var items = new List<ItemStack>();

            // Act: Create items with different categories and stack limits
            stopwatch.Start();
            for (int i = 0; i < 200; i++)
            {
                var category = (ItemCategory)(i % 3); // Cycle through categories
                var maxStack = Inventory.GetMaxStackFor(category);
                var stack = new ItemStack($"item_{i}", category, 1);

                // Simulate filling to max stack
                for (int j = 1; j < maxStack; j++)
                {
                    stack.AddUpTo(1);
                }

                items.Add(stack);
            }

            stopwatch.Stop();

            // Assert: Category operations complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
            AssertThat(items.Count).IsEqual(200);
        }

        [TestCase]
        public void InventorySerializationUnder200Ms()
        {
            // Arrange: Create and serialize inventory
            var stopwatch = new Stopwatch();
            var inventory = new Inventory();

            // Act: Serialize inventory state
            stopwatch.Start();
            var json = inventory.ToJson();
            var deserialized = Inventory.FromJson(json);
            stopwatch.Stop();

            // Assert: Serialization completes in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
            AssertThat(deserialized.Capacity).IsEqual(inventory.Capacity);
        }

        [TestCase]
        public void SimulatedGameTickUnder200Ms()
        {
            // Arrange: Simulate a full game tick
            var stopwatch = new Stopwatch();

            // Act: Execute operations that would happen in a single game tick
            stopwatch.Start();

            // Simulate 20 players moving
            for (int i = 0; i < 20; i++)
            {
                var pos = new Vector2(i * 10.0f, i * 10.0f);
                var velocity = new Vector2(1.0f, 1.0f);
                var newPos = pos + velocity;
                var distance = (newPos - pos).Length;
            }

            // Simulate item operations
            for (int i = 0; i < 50; i++)
            {
                var stack = new ItemStack("resource", ItemCategory.Generic, 10);
                stack.AddUpTo(5);
                stack.RemoveUpTo(3);
            }

            stopwatch.Stop();

            // Assert: Full game tick completes in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
        }

        [TestCase]
        public void HighFrequencyVectorUpdatesUnder200Ms()
        {
            // Arrange: Simulate high-frequency position updates (20Hz)
            var stopwatch = new Stopwatch();
            var positions = new List<Vector2>();

            // Act: Simulate 20Hz updates for 1 second (20 updates per second)
            stopwatch.Start();
            var currentPos = new Vector2(0, 0);
            for (int i = 0; i < 20; i++) // 20 updates
            {
                // Simulate velocity-based movement
                var velocity = new Vector2(5.0f, 3.0f);
                currentPos = currentPos + velocity;
                positions.Add(currentPos);

                // Simulate distance checks for nearby players
                for (int j = 0; j < 10; j++)
                {
                    var otherPos = new Vector2(j * 20.0f, j * 20.0f);
                    var distance = (currentPos - otherPos).Length;
                }
            }

            stopwatch.Stop();

            // Assert: High-frequency updates complete in <200ms
            AssertThat(stopwatch.ElapsedMilliseconds < 200).IsTrue();
            AssertThat(positions.Count).IsEqual(20);
        }
    }
}
