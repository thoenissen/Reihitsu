using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for the <see cref="CasingUtilities"/> class
/// </summary>
[TestClass]
public class CasingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Tests the <see cref="CasingUtilities.ToPascalCase(string)"/> method with various inputs
    /// </summary>
    /// <param name="input">The string to convert to PascalCase</param>
    /// <param name="expected">The expected PascalCase string</param>
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
    [DataRow("_", "_")]
    [DataRow("__", "__")]
    [DataRow("___", "___")]
    [DataRow("", "")]
    [DataRow(null, null)]
    public void ToPascalCaseTest(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToPascalCase(input));
    }

    /// <summary>
    /// Tests the <see cref="CasingUtilities.ToCamelCase(string)"/> method with various inputs
    /// </summary>
    /// <param name="input">The string to convert to camelCase</param>
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
    [DataRow("_", "_")]
    [DataRow("__", "__")]
    [DataRow("___", "___")]
    [DataRow("", "")]
    [DataRow(null, null)]
    public void ToCamelCaseTest(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToCamelCase(input));
    }

    /// <summary>
    /// Tests the <see cref="CasingUtilities.IsTypeParameterName(string)"/> method with various inputs
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [DataRow("T", true)]
    [DataRow("TKey", true)]
    [DataRow("TValue", true)]
    [DataRow("TFirstName", true)]
    [DataRow("Tkey", false)]
    [DataRow("tValue", false)]
    [DataRow("Key", false)]
    [DataRow("key", false)]
    [DataRow("T1", false)]
    [DataRow("T_Key", false)]
    [DataRow("_", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void IsTypeParameterNameTest(string input, bool expected)
    {
        Assert.AreEqual(expected, CasingUtilities.IsTypeParameterName(input));
    }

    /// <summary>
    /// Tests the <see cref="CasingUtilities.ToTypeParameterName(string)"/> method with various inputs
    /// </summary>
    /// <param name="input">The string to convert to a type parameter name</param>
    /// <param name="expected">The expected type parameter name</param>
    [TestMethod]
    [DataRow("T", "T")]
    [DataRow("TKey", "TKey")]
    [DataRow("Key", "TKey")]
    [DataRow("key", "TKey")]
    [DataRow("value", "TValue")]
    [DataRow("tValue", "TValue")]
    [DataRow("tKey", "TKey")]
    [DataRow("t", "T")]
    [DataRow("Type", "TType")]
    [DataRow("Tkey", "TTkey")]
    [DataRow("first_name", "TFirstName")]
    [DataRow("_", "_")]
    [DataRow("", "")]
    [DataRow(null, null)]
    public void ToTypeParameterNameTest(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToTypeParameterName(input));
    }

    /// <summary>
    /// Tests the <see cref="CasingUtilities.ToUnderlineCamelCase(string)"/> method with various inputs
    /// </summary>
    /// <param name="input">The string to convert to _camelCase</param>
    /// <param name="expected">The expected _camelCase string</param>
    [TestMethod]
    [DataRow("FirstName", "_firstName")]
    [DataRow("first_name", "_firstName")]
    [DataRow("FIRST_NAME", "_firstName")]
    [DataRow("_firstName", "_firstName")]
    [DataRow("firstName", "_firstName")]
    [DataRow("_", "_")]
    [DataRow("__", "__")]
    [DataRow("___", "___")]
    [DataRow("", "")]
    [DataRow(null, null)]
    public void ToUnderlineCamelCaseTest(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToUnderlineCamelCase(input));
    }

    #endregion // Tests
}