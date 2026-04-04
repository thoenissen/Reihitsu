using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Test.Core
{
    /// <summary>
    /// Contains unit tests for the <see cref="CasingUtilities"/> class.
    /// </summary>
    [TestClass]
    public class CasingUtilitiesTests
    {
        /// <summary>
        /// Tests the <see cref="CasingUtilities.ToPascalCase(string)"/> method with various inputs.
        /// </summary>
        /// <param name="input">The string to convert to PascalCase.</param>
        /// <param name="expected">The expected PascalCase string.</param>
        [TestMethod]
        [DataRow("FirstName", "FirstName")]
        [DataRow("first_name", "FirstName")]
        [DataRow("FIRST_NAME", "FirstName")]
        [DataRow("First_Name", "FirstName")]
        [DataRow("first_Name", "FirstName")]
        [DataRow("_first_name", "FirstName")]
        [DataRow("_firstName", "FirstName")]
        [DataRow("firstName", "FirstName")]
        [DataRow("first_SName", "FirstSName")]
        [DataRow("FName", "FName")]
        [DataRow("", "")]
        [DataRow(null, null)]
        public void ToPascalCaseTest(string input, string expected)
        {
            Assert.AreEqual(expected, CasingUtilities.ToPascalCase(input));
        }

        /// <summary>
        /// Tests the <see cref="CasingUtilities.ToCamelCase(string)"/> method with various inputs
        /// </summary>
        /// <param name="input">The string to convert to camelCase.</param>
        /// <param name="expected">The expected camelCase string</param>
        [TestMethod]
        [DataRow("FirstName", "firstName")]
        [DataRow("first_name", "firstName")]
        [DataRow("FIRST_NAME", "firstName")]
        [DataRow("First_Name", "firstName")]
        [DataRow("first_Name", "firstName")]
        [DataRow("_first_name", "firstName")]
        [DataRow("_firstName", "firstName")]
        [DataRow("firstName", "firstName")]
        [DataRow("first_SName", "firstSName")]
        [DataRow("FName", "fName")]
        [DataRow("", "")]
        [DataRow(null, null)]
        public void ToCamelCaseTest(string input, string expected)
        {
            Assert.AreEqual(expected, CasingUtilities.ToCamelCase(input));
        }
    }
}