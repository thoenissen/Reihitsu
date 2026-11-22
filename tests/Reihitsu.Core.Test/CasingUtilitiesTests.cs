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
    [Requirement("SHARED-CASING-006")]
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
    [Requirement("SHARED-CASING-006")]
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
    [DataRow("T1", true)]
    [DataRow("T2", true)]
    [DataRow("T1a", false)]
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
    [DataRow("T1", "T1")]
    [DataRow("T2", "T2")]
    [DataRow("t1", "T1")]
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
    [Requirement("SHARED-CASING-006")]
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

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToCamelCase(string)"/> writes the leading run of uppercase letters of the first
    /// word in lowercase, and keeps the case of the run's last letter when a lowercase letter follows it directly
    /// </summary>
    /// <param name="input">The string to convert to camelCase</param>
    /// <param name="expected">The expected camelCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("HTTPClient", "httpClient")]
    [DataRow("XMLParser", "xmlParser")]
    [DataRow("IOStream", "ioStream")]
    [DataRow("HTTP2Client", "http2Client")]
    [DataRow("ABc", "aBc")]
    [DataRow("URLs", "urLs")]
    [DataRow("XML_Parser", "xmlParser")]
    [DataRow("HTTPClient_timeout", "httpClientTimeout")]
    public void ToCamelCaseLowercasesTheLeadingUppercaseRunOfTheFirstWord(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToCamelCase(input));
    }

    /// <summary>
    /// Tests that the leading run of uppercase letters that <see cref="CasingUtilities.ToCamelCase(string)"/> lowercases ends
    /// with the first word, so a word boundary marked with <c>_</c> is never merged into it
    /// </summary>
    /// <param name="input">The string to convert to camelCase</param>
    /// <param name="expected">The expected camelCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("A_B", "aB")]
    [DataRow("A_B_Cd", "aBCd")]
    [DataRow("m_HTTPClient", "mHTTPClient")]
    [DataRow("X_Y_Z", "xYZ")]
    [DataRow("X_YZ", "xYz")]
    [DataRow("I_O_Stream", "iOStream")]
    public void ToCamelCaseTakesTheLeadingUppercaseRunFromTheFirstWordOnly(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToCamelCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToUnderlineCamelCase(string)"/> lowercases the leading run of uppercase letters
    /// of the first word as the camelCase step does, and puts <c>_</c> in front
    /// </summary>
    /// <param name="input">The string to convert to _camelCase</param>
    /// <param name="expected">The expected _camelCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("HTTPClient", "_httpClient")]
    [DataRow("HTTP2Client", "_http2Client")]
    [DataRow("URLs", "_urLs")]
    [DataRow("HTTPClient_timeout", "_httpClientTimeout")]
    [DataRow("A_B", "_aB")]
    [DataRow("m_HTTPClient", "_mHTTPClient")]
    public void ToUnderlineCamelCaseLowercasesTheLeadingUppercaseRunOfTheFirstWord(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToUnderlineCamelCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToPascalCase(string)"/> keeps a leading acronym, which only the camelCase
    /// targets lowercase
    /// </summary>
    /// <param name="input">The string to convert to PascalCase</param>
    /// <param name="expected">The expected PascalCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("HTTPClient", "HTTPClient")]
    [DataRow("HTTPClient_timeout", "HTTPClientTimeout")]
    [DataRow("XMLParser", "XMLParser")]
    public void ToPascalCaseKeepsALeadingAcronym(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToPascalCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.IsPascalCase(string)"/> accepts a combining mark, a letter number, and
    /// connector punctuation other than <c>_</c> wherever a letter or a digit is accepted
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("SHARED-CASING-001")]
    [DataRow("Cafe\u0301", true)]
    [DataRow("Class\u216B", true)]
    [DataRow("Order\u203Fline", true)]
    [DataRow("First\uFF3FName", true)]
    [DataRow("Ma\u0903ra", true)]
    [DataRow("cafe\u0301", false)]
    [DataRow("Order_Line", false)]
    [DataRow("\u216BClass", false)]
    public void IsPascalCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation(string input, bool expected)
    {
        Assert.AreEqual(expected, CasingUtilities.IsPascalCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.IsCamelCase(string)"/> accepts a combining mark, a letter number, and connector
    /// punctuation other than <c>_</c> wherever a letter or a digit is accepted
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("SHARED-CASING-002")]
    [DataRow("cafe\u0301", true)]
    [DataRow("class\u216B", true)]
    [DataRow("order\u203Fline", true)]
    [DataRow("Cafe\u0301", false)]
    [DataRow("order_line", false)]
    [DataRow("\u0301cafe", false)]
    public void IsCamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation(string input, bool expected)
    {
        Assert.AreEqual(expected, CasingUtilities.IsCamelCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.IsUnderlineCamelCase(string)"/> accepts a combining mark, a letter number, and
    /// connector punctuation other than <c>_</c> behind the prefix wherever a letter or a digit is accepted
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("SHARED-CASING-003")]
    [DataRow("_cafe\u0301", true)]
    [DataRow("_class\u216B", true)]
    [DataRow("_order\u203Fline", true)]
    [DataRow("_Cafe\u0301", false)]
    [DataRow("_\u0301cafe", false)]
    public void IsUnderlineCamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation(string input, bool expected)
    {
        Assert.AreEqual(expected, CasingUtilities.IsUnderlineCamelCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToPascalCase(string)"/> keeps every combining mark, letter number, and connector
    /// punctuation other than <c>_</c> in its word: a combining mark counts as part of the character in front of it, a letter
    /// number and connector punctuation count like a digit, and only <c>_</c> splits words
    /// </summary>
    /// <param name="input">The string to convert to PascalCase</param>
    /// <param name="expected">The expected PascalCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("cafe\u0301_bar", "Cafe\u0301Bar")]
    [DataRow("order\u203Fline", "Order\u203Fline")]
    [DataRow("CAFE\u0301_NAME", "Cafe\u0301Name")]
    [DataRow("CLASS\u216B_NAME", "CLASS\u216BName")]
    [DataRow("ORDER\u203FLINE_NAME", "ORDER\u203FLINEName")]
    [DataRow("a_\u0301b", "A\u0301b")]
    public void ToPascalCaseKeepsEveryIdentifierCharacterInItsWord(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToPascalCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToCamelCase(string)"/> counts a combining mark as part of the uppercase letter in
    /// front of it, so it neither ends the leading run nor is the lowercase letter behind it, while a letter number ends the run
    /// </summary>
    /// <param name="input">The string to convert to camelCase</param>
    /// <param name="expected">The expected camelCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("CAFE\u0301_NAME", "cafe\u0301Name")]
    [DataRow("E\u0301TATName", "e\u0301tatName")]
    [DataRow("XE\u0301", "xe\u0301")]
    [DataRow("HTTP\u216BClient", "http\u216BClient")]
    public void ToCamelCaseCountsACombiningMarkAsPartOfTheLetterInFrontOfIt(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToCamelCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToTypeParameterName(string)"/> treats a <c>t</c> or <c>T</c> followed by <c>_</c>
    /// as the prefix, drops the underscores behind it, and leaves <c>T</c> alone for an empty body
    /// </summary>
    /// <param name="input">The string to convert to a type parameter name</param>
    /// <param name="expected">The expected type parameter name</param>
    [TestMethod]
    [Requirement("RH4121-003")]
    [DataRow("T_Key", "TKey")]
    [DataRow("t_value", "TValue")]
    [DataRow("t_key", "TKey")]
    [DataRow("T_1", "T1")]
    [DataRow("T_", "T")]
    [DataRow("t_", "T")]
    [DataRow("T__Key", "TKey")]
    [DataRow("T_T", "TT")]
    public void ToTypeParameterNameDropsTheUnderscoresBehindAnExistingPrefix(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToTypeParameterName(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToPascalCase(string)"/> keeps a letter number or a combining mark that starts a
    /// word after the first as written, also when the character has an uppercase mapping
    /// </summary>
    /// <param name="input">The string to convert to PascalCase</param>
    /// <param name="expected">The expected PascalCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("class_ⅻ", "Classⅻ")]
    [DataRow("order_ⅻx", "Orderⅻx")]
    [DataRow("order_ⅰ_line", "OrderⅰLine")]
    [DataRow("a_ͅb", "Aͅb")]
    [DataRow("CLASS_ⅿ", "Classⅿ")]
    public void ToPascalCaseKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToPascalCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToCamelCase(string)"/> keeps a letter number or a combining mark that starts a
    /// word after the first as written, also when the character has an uppercase mapping
    /// </summary>
    /// <param name="input">The string to convert to camelCase</param>
    /// <param name="expected">The expected camelCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("class_ⅻ", "classⅻ")]
    [DataRow("order_ⅻx", "orderⅻx")]
    [DataRow("a_ͅb", "aͅb")]
    public void ToCamelCaseKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToCamelCase(input));
    }

    /// <summary>
    /// Tests that <see cref="CasingUtilities.ToUnderlineCamelCase(string)"/> keeps a letter number or a combining mark that
    /// starts a word after the first as written, also when the character has an uppercase mapping
    /// </summary>
    /// <param name="input">The string to convert to _camelCase</param>
    /// <param name="expected">The expected _camelCase string</param>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [DataRow("class_ⅻ", "_classⅻ")]
    [DataRow("order_ⅻx", "_orderⅻx")]
    [DataRow("a_ͅb", "_aͅb")]
    public void ToUnderlineCamelCaseKeepsALetterNumberOrCombiningMarkThatStartsALaterWordAsWritten(string input, string expected)
    {
        Assert.AreEqual(expected, CasingUtilities.ToUnderlineCamelCase(input));
    }

    /// <summary>
    /// Tests that every PascalCase, camelCase, and _camelCase derivation keeps each letter number, combining mark, and connector
    /// punctuation other than <c>_</c> as written when it starts a word after the first, for every such character
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    public void EveryDerivationKeepsEachCharacterThatIsKeptAsWrittenWhenItStartsALaterWord()
    {
        var failures = new System.Collections.Generic.List<string>();

        for (var code = 0; code <= char.MaxValue; code++)
        {
            var character = (char)code;

            if (character == '_'
                || char.IsSurrogate(character))
            {
                continue;
            }

            var category = char.GetUnicodeCategory(character);

            if (category != System.Globalization.UnicodeCategory.LetterNumber
                && category != System.Globalization.UnicodeCategory.NonSpacingMark
                && category != System.Globalization.UnicodeCategory.SpacingCombiningMark
                && category != System.Globalization.UnicodeCategory.ConnectorPunctuation)
            {
                continue;
            }

            var input = $"a_{character}b";

            if (CasingUtilities.ToPascalCase(input) != $"A{character}b"
                || CasingUtilities.ToCamelCase(input) != $"a{character}b"
                || CasingUtilities.ToUnderlineCamelCase(input) != $"_a{character}b")
            {
                failures.Add($"U+{code:X4}");
            }
        }

        Assert.IsEmpty(failures, $"Characters changed at the start of a later word: {string.Join(", ", failures)}");
    }

    #endregion // Tests
}