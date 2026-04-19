using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.RawStringAlignment;

namespace Reihitsu.Formatter.Test.Unit.RawStringAlignment;

/// <summary>
/// Tests for <see cref="RawStringAlignmentPhase"/>
/// </summary>
[TestClass]
public class RawStringAlignmentPhaseTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Non-interpolated raw strings

    /// <summary>
    /// Verifies that an already aligned non-interpolated raw string is not modified.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(input, result.ToFullString());
    }

    /// <summary>
    /// Verifies that a misaligned non-interpolated raw string is corrected.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    /// <summary>
    /// Verifies that a single-line raw string is not modified.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(input, result.ToFullString());
    }

    /// <summary>
    /// Verifies that a four-quote raw string is aligned correctly.
    /// </summary>
    [TestMethod]
    public void FourQuoteRawStringIsAlignedCorrectly()
    {
        var input = "class C\r\n{\r\n    void M()\r\n    {\r\n        var a = \"\"\"\"\r\n    Hello \"\"\"\r\n    \"\"\"\";\r\n    }\r\n}";
        var expected = "class C\r\n{\r\n    void M()\r\n    {\r\n        var a = \"\"\"\"\r\n                Hello \"\"\"\r\n                \"\"\"\";\r\n    }\r\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    /// <summary>
    /// Verifies that multiple content lines are all adjusted uniformly.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    /// <summary>
    /// Verifies that a raw string needing leftward shift is handled correctly.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    #endregion // Non-interpolated raw strings

    #region Interpolated raw strings

    /// <summary>
    /// Verifies that an already aligned interpolated raw string is not modified.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(input, result.ToFullString());
    }

    /// <summary>
    /// Verifies that a misaligned interpolated raw string is corrected.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    /// <summary>
    /// Verifies that a double-dollar interpolated raw string is aligned correctly.
    /// </summary>
    [TestMethod]
    public void DoubleDollarInterpolatedRawStringIsAlignedCorrectly()
    {
        var input = "class C\r\n{\r\n    void M()\r\n    {\r\n        var x = 1;\r\n\r\n        var a = $$\"\"\"\r\n    {{x}} Test\r\n    \"\"\";\r\n    }\r\n}";
        var expected = "class C\r\n{\r\n    void M()\r\n    {\r\n        var x = 1;\r\n\r\n        var a = $$\"\"\"\r\n                  {{x}} Test\r\n                  \"\"\";\r\n    }\r\n}";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    /// <summary>
    /// Verifies that an interpolated raw string with multiple interpolation holes is aligned correctly.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    #endregion // Interpolated raw strings

    #region Multiple raw strings

    /// <summary>
    /// Verifies that multiple raw strings in the same file are all aligned independently.
    /// </summary>
    [TestMethod]
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var result = RawStringAlignmentPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(expected, result.ToFullString());
    }

    #endregion // Multiple raw strings
}