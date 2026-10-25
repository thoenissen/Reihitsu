using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.RawStringAlignment;

/// <summary>
/// Tests for the move of a multi-line raw string literal's lines under its opening quotes, which <see cref="IndentationPhase"/>
/// applies in the same phase that places the opening quotes
/// </summary>
[TestClass]
public class IndentationPhaseRawStringMoveTests : FormatterPhaseTestsBase
{
    #region Non-interpolated raw strings

    /// <summary>
    /// Verifies that an already aligned non-interpolated raw string is not modified
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-006")]
    public void AlreadyAlignedNonInterpolatedRawStringIsNotModified()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                             Hello
                                             """;
                                 }
                             }
                             """";

        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a misaligned non-interpolated raw string is corrected
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void MisalignedNonInterpolatedRawStringIsCorrected()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 Hello
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a single-line raw string is not modified
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-006")]
    public void SingleLineRawStringIsNotModified()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """Hello""";
                                 }
                             }
                             """";

        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a four-quote raw string is aligned correctly
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void FourQuoteRawStringIsAlignedCorrectly()
    {
        var input = "class C\r\n{\r\n    void M()\r\n    {\r\n        var a = \"\"\"\"\r\n    Hello \"\"\"\r\n    \"\"\"\";\r\n    }\r\n}";
        var expected = "class C\r\n{\r\n    void M()\r\n    {\r\n        var a = \"\"\"\"\r\n                Hello \"\"\"\r\n                \"\"\"\";\r\n    }\r\n}";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that multiple content lines are all adjusted uniformly
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public void MultipleContentLinesAreAdjustedUniformly()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 Line1
                                     Line2
                                 Line3
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Line1
                                                    Line2
                                                Line3
                                                """;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a raw string needing leftward shift is handled correctly
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-002")]
    public void LeftwardShiftIsHandledCorrectly()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                                     Hello
                                                     """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a misaligned UTF-8 (<c>u8</c>) raw string is corrected
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-004")]
    public void MisalignedUtf8RawStringIsCorrected()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 Hello
                                 """u8;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """u8;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that an already aligned UTF-8 (<c>u8</c>) raw string is not modified
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-004")]
    [Requirement("FMT-RAW-006")]
    public void AlreadyAlignedUtf8RawStringIsNotModified()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                             Hello
                                             """u8;
                                 }
                             }
                             """";

        Assert.AreEqual(input, ApplyPhase(input));
    }

    #endregion // Non-interpolated raw strings

    #region Interpolated raw strings

    /// <summary>
    /// Verifies that an already aligned interpolated raw string is not modified
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-006")]
    public void AlreadyAlignedInterpolatedRawStringIsNotModified()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "World";

                                     var a = $"""
                                              Hello {name}
                                              """;
                                 }
                             }
                             """";

        Assert.AreEqual(input, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a misaligned interpolated raw string is corrected
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void MisalignedInterpolatedRawStringIsCorrected()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "World";

                                     var a = $"""
                                 Hello {name}
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "World";

                                        var a = $"""
                                                 Hello {name}
                                                 """;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that a double-dollar interpolated raw string is aligned correctly
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-003")]
    public void DoubleDollarInterpolatedRawStringIsAlignedCorrectly()
    {
        var input = "class C\r\n{\r\n    void M()\r\n    {\r\n        var x = 1;\r\n\r\n        var a = $$\"\"\"\r\n    {{x}} Test\r\n    \"\"\";\r\n    }\r\n}";
        var expected = "class C\r\n{\r\n    void M()\r\n    {\r\n        var x = 1;\r\n\r\n        var a = $$\"\"\"\r\n                  {{x}} Test\r\n                  \"\"\";\r\n    }\r\n}";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    /// <summary>
    /// Verifies that an interpolated raw string with multiple interpolation holes is aligned correctly
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void InterpolatedRawStringWithMultipleHolesIsAligned()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var first = "Hello";
                                     var last = "World";

                                     var a = $"""
                                 {first}, {last}!
                                 Welcome
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var first = "Hello";
                                        var last = "World";

                                        var a = $"""
                                                 {first}, {last}!
                                                 Welcome
                                                 """;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    #endregion // Interpolated raw strings

    #region Multiple raw strings

    /// <summary>
    /// Verifies that multiple raw strings in the same file are all aligned independently
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    public void MultipleRawStringsAreAlignedIndependently()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 First
                                 """;

                                     var b = """
                                 Second
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                First
                                                """;

                                        var b = """
                                                Second
                                                """;
                                    }
                                }
                                """";

        Assert.AreEqual(expected, ApplyPhase(input));
    }

    #endregion // Multiple raw strings

    #region FormatterPhaseTestsBase

    /// <inheritdoc/>
    protected override SyntaxNode ExecutePhase(SyntaxNode root, CancellationToken cancellationToken)
    {
        var endOfLine = root.ToFullString().Contains("\r\n") ? "\r\n" : "\n";

        return new IndentationPhase().Execute(root, new FormattingContext(endOfLine), cancellationToken);
    }

    #endregion // FormatterPhaseTestsBase
}