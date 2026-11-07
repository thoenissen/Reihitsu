using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="RawStringLiteralUtilities"/>
/// </summary>
[TestClass]
public class RawStringLiteralUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that the leading whitespace length counts every leading whitespace character as one and stops at the first other character
    /// </summary>
    /// <param name="lineText">The line text</param>
    /// <param name="expectedLength">The expected length</param>
    [TestMethod]
    [Requirement("RH5202-001")]
    [DataRow("", 0)]
    [DataRow("x", 0)]
    [DataRow("    \"\"\"", 4)]
    [DataRow("\t\t\"\"\"", 2)]
    [DataRow(" \t \"\"\"", 3)]
    [DataRow("\u00A0   \"\"\"", 4)]
    [DataRow("    \u00A0\"\"\"", 5)]
    [DataRow("\v\f\u2003x", 3)]
    [DataRow("  x  ", 2)]
    [DataRow("    ", 4)]
    [DataRow("\u00A0\t ", 3)]
    public void GetLeadingWhitespaceLengthCountsEveryLeadingWhitespaceCharacter(string lineText, int expectedLength)
    {
        Assert.AreEqual(expectedLength, RawStringLiteralUtilities.GetLeadingWhitespaceLength(lineText));
    }

    #endregion // Tests
}