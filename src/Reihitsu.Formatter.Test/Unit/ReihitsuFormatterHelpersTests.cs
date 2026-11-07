using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatterHelpers"/>
/// </summary>
[TestClass]
public class ReihitsuFormatterHelpersTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns LF when the source uses LF line endings
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineReturnsLineFeedWhenSourceUsesLineFeed()
    {
        // Arrange
        const string input = "namespace Test;\nclass Foo { }\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns CRLF when the source uses CRLF line endings
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineReturnsCarriageReturnLineFeedWhenSourceUsesCarriageReturnLineFeed()
    {
        // Arrange
        const string input = "namespace Test;\r\nclass Foo { }\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\r\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns the predominant line ending when the source mixes styles
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineReturnsPredominantLineFeedWhenSourceHasMixedLineEndings()
    {
        // Arrange
        const string input = "namespace Test;\nclass Foo { }\r\nclass Bar { }\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns the predominant CRLF line ending when the source mixes styles
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineReturnsPredominantCarriageReturnLineFeedWhenSourceHasMixedLineEndings()
    {
        // Arrange
        const string input = "namespace Test;\r\nclass Foo { }\nclass Bar { }\r\n";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\r\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns LF when every line break of the source is a lone carriage return
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineReturnsLineFeedWhenEveryLineBreakIsALoneCarriageReturn()
    {
        // Arrange
        const string input = "namespace Test;\rclass Foo { }\rclass Bar { }\r";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> counts a lone carriage return as LF, so two lone carriage returns outnumber one CRLF
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineCountsALoneCarriageReturnAsLineFeed()
    {
        // Arrange
        const string input = "namespace Test;\rclass Foo { }\r\nclass Bar { }\r";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns CRLF when one lone carriage return, counted as LF, ties with one CRLF
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineReturnsCarriageReturnLineFeedWhenALoneCarriageReturnTiesWithIt()
    {
        // Arrange
        const string input = "namespace Test;\rclass Foo { }\r\nclass Bar { }";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual("\r\n", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> falls back to <see cref="System.Environment.NewLine"/> when no end-of-line trivia is found
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    public void DetectEndOfLineFallsBackToEnvironmentNewLineWhenNoEndOfLineTriviaFound()
    {
        // Arrange
        const string input = "class Foo { }";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.DetectEndOfLine(root);

        // Assert
        Assert.AreEqual(System.Environment.NewLine, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.HasSyntaxErrors"/> returns true when the syntax tree contains errors
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-005")]
    public void HasSyntaxErrorsReturnsTrueWhenTreeContainsErrors()
    {
        // Arrange
        const string input = """
                             namespace Test;

                             public class Foo
                             {
                                 public void Bar(
                                 {
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.HasSyntaxErrors(tree);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.HasSyntaxErrors"/> returns false when the syntax tree is valid
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-005")]
    public void HasSyntaxErrorsReturnsFalseWhenTreeIsValid()
    {
        // Arrange
        const string input = """
                             namespace Test;

                             public class Foo
                             {
                                 public void Bar()
                                 {
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.HasSyntaxErrors(tree);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns true for a single-line auto-generated marker
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsTrueForSingleLineMarker()
    {
        // Arrange
        const string input = """
                             // <auto-generated />
                             namespace Test;

                             public class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns true for a multi-line auto-generated marker
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsTrueForMultiLineMarker()
    {
        // Arrange
        const string input = """
                             /*
                              * <auto-generated>
                              *     This code was generated by a tool.
                              * </auto-generated>
                              */
                             namespace Test;

                             public class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns true for the autogenerated marker without hyphen
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsTrueForAutogeneratedMarkerWithoutHyphen()
    {
        // Arrange
        const string input = """
                             // <autogenerated />
                             namespace Test;

                             public class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns false when no auto-generated marker is present
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsFalseWhenNoMarkerIsPresent()
    {
        // Arrange
        const string input = """
                             namespace Test;

                             public class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> is case-insensitive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceIsCaseInsensitive()
    {
        // Arrange
        const string input = """
                             // <AUTO-GENERATED />
                             namespace Test;

                             public class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns false when the marker only appears in a comment inside the file body rather than the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsFalseWhenMarkerIsOnlyInBodyComment()
    {
        // Arrange
        const string input = """
                             namespace Test;

                             public class Foo
                             {
                                 // Files containing an <auto-generated> header are skipped by the formatter.
                                 public void M() { }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns false when the marker appears in a multi-line comment in the file body rather than the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsFalseWhenMarkerIsInBodyBlockComment()
    {
        // Arrange
        const string input = """
                             namespace Test;

                             public class Foo
                             {
                                 /* Detects an <auto-generated> header marker on a member. */
                                 public void M() { }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.IsAutoGeneratedSource"/> returns true when the marker appears in the header preceded by other leading comments
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-006")]
    public void IsAutoGeneratedSourceReturnsTrueWhenMarkerIsInHeaderAfterOtherComments()
    {
        // Arrange
        const string input = """
                             // Copyright header.
                             // <auto-generated />
                             namespace Test;

                             public class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // Act
        var result = ReihitsuFormatterHelpers.IsAutoGeneratedSource(root);

        // Assert
        Assert.IsTrue(result);
    }

    #endregion // Methods
}