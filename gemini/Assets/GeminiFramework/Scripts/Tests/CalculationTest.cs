using NUnit.Framework;

namespace Gemini.Tests
{
    /// <summary>
    /// Simple test class for testing of the calculation class
    /// </summary>
    public class CalculationTest
    {
        /// <summary>
        /// Simple testing method for assertion of success case
        /// </summary>
        [Test]
        public void CalculationTestSimplePasses()
        {
            Assert.AreEqual(3, Calculation.Sum(1,2));
        }
        /// <summary>
        /// Simple testing method for assertion of failed case
        /// </summary>
        [Test]
        public void CalculationTestSimpleFail()
        {
            Assert.AreEqual(2, Calculation.Multi(1,2));
        }
    }

    /// <summary>
    /// Simple calculation class
    /// </summary>
    public class Calculation
    {
        /// <summary>
        /// Sum of the numbers
        /// </summary>
        /// <param name="a">left operand</param>
        /// <param name="b">right operand</param>
        /// <returns>return the sum of the integer </returns>
        public static int Sum(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// Multiply the numbers
        /// </summary>
        /// <param name="a">left operand</param>
        /// <param name="b">right operand</param>
        /// <returns>return the multiply of the integer </returns>
        public static int Multi(int a, int b)
        {
            return a * b;
        }
    }
}
