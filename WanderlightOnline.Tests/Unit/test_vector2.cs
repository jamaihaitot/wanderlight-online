using System;
using GdUnit4;
using static GdUnit4.Assertions;
using WanderlightOnline;

namespace WanderlightOnline.Tests.Unit
{
    /// <summary>
    /// Unit tests for the Vector2 utility class.
    /// Tests mathematical operations, edge cases, and precision.
    /// </summary>
    [TestSuite]
    public class Vector2UnitTests
    {
        // Test: Vector2 constructor initializes values correctly
        [TestCase]
        public static void ConstructorInitializesValues()
        {
            var v1 = new Vector2(3.5f, 7.2f);
            AssertThat(v1.X).IsEqual(3.5f);
            AssertThat(v1.Y).IsEqual(7.2f);

            var v2 = new Vector2();
            AssertThat(v2.X).IsEqual(0f);
            AssertThat(v2.Y).IsEqual(0f);
        }

        // Test: Length calculation is accurate
        [TestCase]
        public static void LengthCalculatesCorrectly()
        {
            var v1 = new Vector2(3f, 4f);
            AssertThat(v1.Length).IsEqual(5f);

            var v2 = new Vector2(0f, 0f);
            AssertThat(v2.Length).IsEqual(0f);

            var v3 = new Vector2(-3f, -4f);
            AssertThat(v3.Length).IsEqual(5f);
        }

        // Test: Normalized vector has length 1
        [TestCase]
        public static void NormalizedProducesUnitVector()
        {
            var v1 = new Vector2(3f, 4f);
            var normalized = v1.Normalized();

            // Length should be 1 (within floating point precision)
            var lengthDiff = Math.Abs(normalized.Length - 1f);
            AssertThat(lengthDiff < 0.0001f).IsTrue();

            // Direction should be preserved
            AssertThat(normalized.X).IsEqual(0.6f);
            AssertThat(normalized.Y).IsEqual(0.8f);
        }

        // Test: Normalizing zero vector returns zero
        [TestCase]
        public static void NormalizedZeroVectorReturnsZero()
        {
            var v = new Vector2(0f, 0f);
            var normalized = v.Normalized();

            AssertThat(normalized.X).IsEqual(0f);
            AssertThat(normalized.Y).IsEqual(0f);
        }

        // Test: Vector addition operator
        [TestCase]
        public static void AdditionOperatorWorks()
        {
            var v1 = new Vector2(1f, 2f);
            var v2 = new Vector2(3f, 4f);
            var result = v1 + v2;

            AssertThat(result.X).IsEqual(4f);
            AssertThat(result.Y).IsEqual(6f);
        }

        // Test: Vector subtraction operator
        [TestCase]
        public static void SubtractionOperatorWorks()
        {
            var v1 = new Vector2(5f, 7f);
            var v2 = new Vector2(2f, 3f);
            var result = v1 - v2;

            AssertThat(result.X).IsEqual(3f);
            AssertThat(result.Y).IsEqual(4f);
        }

        // Test: Vector scalar multiplication (both orders)
        [TestCase]
        public static void MultiplicationOperatorWorks()
        {
            var v = new Vector2(2f, 3f);

            var result1 = v * 2f;
            AssertThat(result1.X).IsEqual(4f);
            AssertThat(result1.Y).IsEqual(6f);

            var result2 = 3f * v;
            AssertThat(result2.X).IsEqual(6f);
            AssertThat(result2.Y).IsEqual(9f);
        }

        // Test: Vector scalar division
        [TestCase]
        public static void DivisionOperatorWorks()
        {
            var v = new Vector2(10f, 20f);
            var result = v / 2f;

            AssertThat(result.X).IsEqual(5f);
            AssertThat(result.Y).IsEqual(10f);
        }

        // Test: Negative values work correctly
        [TestCase]
        public static void NegativeValuesWorkCorrectly()
        {
            var v1 = new Vector2(-5f, -10f);
            var v2 = new Vector2(3f, 4f);

            var sum = v1 + v2;
            AssertThat(sum.X).IsEqual(-2f);
            AssertThat(sum.Y).IsEqual(-6f);

            var product = v1 * -1f;
            AssertThat(product.X).IsEqual(5f);
            AssertThat(product.Y).IsEqual(10f);
        }

        // Test: Very small values (floating point precision)
        [TestCase]
        public static void SmallValuesMaintainPrecision()
        {
            var v = new Vector2(0.0001f, 0.0002f);
            var doubled = v * 2f;

            AssertThat(doubled.X).IsEqual(0.0002f);
            AssertThat(doubled.Y).IsEqual(0.0004f);
        }

        // Test: Very large values
        [TestCase]
        public static void LargeValuesWorkCorrectly()
        {
            var v = new Vector2(1000000f, 2000000f);
            var halved = v / 2f;

            AssertThat(halved.X).IsEqual(500000f);
            AssertThat(halved.Y).IsEqual(1000000f);
        }

        // Test: Operations preserve original vector immutability
        [TestCase]
        public static void OperationsDoNotModifyOriginal()
        {
            var v1 = new Vector2(5f, 10f);
            var originalX = v1.X;
            var originalY = v1.Y;

            var v2 = v1 + new Vector2(1f, 1f);
            var v3 = v1 * 2f;
            var v4 = v1.Normalized();

            // Original should be unchanged
            AssertThat(v1.X).IsEqual(originalX);
            AssertThat(v1.Y).IsEqual(originalY);
        }
    }
}
