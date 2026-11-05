using System;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests that <see cref="BlankLinePhase"/>, run on its own, inserts no line break in front of a documentation comment
/// written behind code, single-line or delimited, and gives a documentation comment that starts its line the blank line
/// of a comment the author wrote on its own line
/// </summary>
[TestClass]
public class BlankLinePhaseDocumentationCommentTests
{
    #region Fields

    /// <summary>
    /// The line endings every fixture is exercised against
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that the phase leaves a delimited documentation comment behind a field, with the documented field behind
    /// it on its line, where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindFieldWithCodeBehindIt()
    {
        AssertUnchanged("class C\n{\n    int A; /** doc */ int B;\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a delimited documentation comment that ends the line of a field where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfDelimitedDocumentationEndingFieldLine()
    {
        AssertUnchanged("class C\n{\n    int A; /** doc */\n    int B;\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a delimited documentation comment behind an enum member where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindEnumMember()
    {
        AssertUnchanged("enum E\n{\n    First, /** doc */\n    Second\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a delimited documentation comment behind a block comment that trails a field where it
    /// was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindTrailingBlockComment()
    {
        AssertUnchanged("class C\n{\n    int A; /* c */ /** doc */ int B;\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a delimited documentation comment behind a statement where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindStatement()
    {
        AssertUnchanged("class C\n{\n    void M(int a)\n    {\n        a = 1; /** d */ a = 2;\n    }\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a delimited documentation comment behind a using directive where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfDelimitedDocumentationBehindUsingDirective()
    {
        AssertUnchanged("using System; /** doc */\nusing System.IO;\n\nclass C\n{\n    int A;\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a single-line documentation comment behind a field where it was written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void InsertsNoLineBreakInFrontOfSingleLineDocumentationBehindField()
    {
        AssertUnchanged("class C\n{\n    int A; /// <inheritdoc/>\n    int B;\n}");
    }

    /// <summary>
    /// Verifies that the phase leaves a stranded delimited documentation comment where it was written and gives it no
    /// blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void KeepsStrandedDelimitedDocumentationWithoutBlankLine()
    {
        AssertUnchanged("class C\n{\n    void M()\n    {\n        var x = 1; /** stray */\n    }\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that starts the line below a field, with the documented field
    /// behind it, gets one blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void SeparatesDelimitedDocumentationStartingItsLineFromField()
    {
        AssertResult("class C\n{\n    int A;\n    /** doc */ int B;\n}",
                     "class C\n{\n    int A;\n\n    /** doc */ int B;\n}");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment that starts the line below an enum member gets one blank line above
    /// it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void SeparatesDelimitedDocumentationStartingItsLineFromEnumMember()
    {
        AssertResult("enum E\n{\n    First,\n    /** doc */\n    Second\n}",
                     "enum E\n{\n    First,\n\n    /** doc */\n    Second\n}");
    }

    /// <summary>
    /// Verifies that a single-line documentation comment that starts the line below a field gets one blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void SeparatesSingleLineDocumentationStartingItsLineFromField()
    {
        AssertResult("class C\n{\n    int A;\n    /// <inheritdoc/>\n    int B;\n}",
                     "class C\n{\n    int A;\n\n    /// <inheritdoc/>\n    int B;\n}");
    }

    /// <summary>
    /// Verifies that a documentation comment that starts the first line of a type body gets no blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-012")]
    public void KeepsDocumentationStartingFirstLineOfTypeBodyWithoutBlankLine()
    {
        AssertUnchanged("class C\n{\n    /// <inheritdoc/>\n    void M()\n    {\n    }\n}");
        AssertUnchanged("class C\n{\n    /** doc */ void M()\n    {\n    }\n}");
    }

    /// <summary>
    /// Runs the phase under every line ending and asserts the expected output
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    /// <param name="expectedWithLf">The expected output with LF line endings</param>
    private static void AssertResult(string inputWithLf, string expectedWithLf)
    {
        foreach (var endOfLine in _lineEndings)
        {
            Assert.AreEqual(Normalize(expectedWithLf, endOfLine), Apply(Normalize(inputWithLf, endOfLine), endOfLine), $"Unexpected phase output under {Describe(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Runs the phase under every line ending and asserts that it changes nothing
    /// </summary>
    /// <param name="inputWithLf">The input with LF line endings</param>
    private static void AssertUnchanged(string inputWithLf)
    {
        AssertResult(inputWithLf, inputWithLf);
    }

    /// <summary>
    /// Runs the blank-line phase over the given source
    /// </summary>
    /// <param name="input">The source text</param>
    /// <param name="endOfLine">The document's line ending</param>
    /// <returns>The phase output</returns>
    private static string Apply(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(endOfLine);

        return new BlankLinePhase().Execute(tree.GetRoot(), context, CancellationToken.None).ToFullString();
    }

    /// <summary>
    /// Rewrites every line break of the text to the given line ending
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="endOfLine">The line ending</param>
    /// <returns>The normalized text</returns>
    private static string Normalize(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Names the given line ending
    /// </summary>
    /// <param name="endOfLine">The line ending</param>
    /// <returns><c>LF</c> or <c>CRLF</c></returns>
    private static string Describe(string endOfLine)
    {
        return endOfLine == "\n" ? "LF" : "CRLF";
    }

    #endregion // Methods
}