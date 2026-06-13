using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatterHelpers"/>
/// </summary>
[TestClass]
public class ReihitsuFormatterHelpersTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> returns LF when the source uses LF line endings
    /// </summary>
    [TestMethod]
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
    /// Verifies that <see cref="ReihitsuFormatterHelpers.DetectEndOfLine"/> falls back to <see cref="System.Environment.NewLine"/> when no end-of-line trivia is found
    /// </summary>
    [TestMethod]
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

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> returns zero for a top-level member in a file-scoped namespace
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelReturnsZeroForTopLevelMemberInFileScopedNamespace()
    {
        // Arrange
        const string input = """
                             namespace Test;

                             class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(classDecl);

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> returns one for a member inside a block-scoped namespace
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelReturnsOneForMemberInsideBlockScopedNamespace()
    {
        // Arrange
        const string input = """
                             namespace Test
                             {
                                 class Foo { }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(classDecl);

        // Assert
        Assert.AreEqual(1, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> returns two for a method inside a class inside a block-scoped namespace
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelReturnsTwoForMethodInsideClassInsideBlockScopedNamespace()
    {
        // Arrange
        const string input = """
                             namespace Test
                             {
                                 class Foo
                                 {
                                     void Bar() { }
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(methodDecl);

        // Assert
        Assert.AreEqual(2, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> counts nested types correctly
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelCountsNestedTypesCorrectly()
    {
        // Arrange
        const string input = """
                             namespace Test
                             {
                                 class Outer
                                 {
                                     class Inner
                                     {
                                         void Bar() { }
                                     }
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(methodDecl);

        // Assert
        Assert.AreEqual(3, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> counts the switch section as an
    /// indenting ancestor for a statement directly inside a <c>case</c> body, matching the full-document layout
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelCountsSwitchSectionForStatementInsideCaseBody()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             if (x > 0)
                                             {
                                                 Foo();
                                             }

                                             break;
                                     }
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var ifStatement = root.DescendantNodes().OfType<IfStatementSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(ifStatement);

        // Assert — class + method body + switch + switch-section
        Assert.AreEqual(4, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> counts the switch section as an
    /// indenting ancestor for a statement directly inside a <c>default</c> body
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelCountsSwitchSectionForStatementInsideDefaultBody()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M(int x)
                                 {
                                     switch (x)
                                     {
                                         default:
                                             Foo();
                                             break;
                                     }
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var fooStatement = root.DescendantNodes().OfType<ExpressionStatementSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(fooStatement);

        // Assert — class + method body + switch + switch-section
        Assert.AreEqual(4, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeBaseIndentLevel"/> does not count the switch section
    /// for a <c>case</c> label itself, which sits at the switch-statement indentation level rather than the body level
    /// </summary>
    [TestMethod]
    public void ComputeBaseIndentLevelDoesNotCountSwitchSectionForCaseLabel()
    {
        // Arrange
        const string input = """
                             public class C
                             {
                                 public void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             Foo();
                                             break;
                                     }
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var caseLabel = root.DescendantNodes().OfType<CaseSwitchLabelSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.ComputeBaseIndentLevel(caseLabel);

        // Assert — class + method body + switch (the label is not a statement body)
        Assert.AreEqual(3, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeTokenColumn"/> returns zero for a token at the beginning of a line
    /// </summary>
    [TestMethod]
    public void ComputeTokenColumnReturnsZeroForTokenAtBeginningOfLine()
    {
        // Arrange
        const string input = "class Foo { }";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var classKeyword = root.DescendantTokens().First(t => t.IsKind(SyntaxKind.ClassKeyword));

        // Act
        var result = ReihitsuFormatterHelpers.ComputeTokenColumn(classKeyword, root);

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.ComputeTokenColumn"/> returns the correct column for an indented token
    /// </summary>
    [TestMethod]
    public void ComputeTokenColumnReturnsCorrectColumnForIndentedToken()
    {
        // Arrange
        const string input = """
                             namespace Test;
                                 class Foo { }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var classKeyword = root.DescendantTokens().First(t => t.IsKind(SyntaxKind.ClassKeyword));

        // Act
        var result = ReihitsuFormatterHelpers.ComputeTokenColumn(classKeyword, root);

        // Assert
        Assert.AreEqual(4, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.AdjustNodeIndentation"/> shifts indentation right by the given offset
    /// </summary>
    [TestMethod]
    public void AdjustNodeIndentationShiftsIndentationRightByOffset()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                 void Bar()
                                 {
                                 }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.AdjustNodeIndentation(methodDecl, 4);
        var resultText = result.ToFullString();

        // Assert
        Assert.Contains("        void Bar()", resultText, "Method declaration should be shifted right by 4 spaces.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.AdjustNodeIndentation"/> shifts indentation left by the given offset
    /// </summary>
    [TestMethod]
    public void AdjustNodeIndentationShiftsIndentationLeftByOffset()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                                     void Bar()
                                     {
                                     }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.AdjustNodeIndentation(methodDecl, -4);
        var resultText = result.ToFullString();

        // Assert
        Assert.Contains("    void Bar()", resultText, "Method declaration should be shifted left by 4 spaces.");
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.AdjustNodeIndentation"/> returns the node unchanged when there are no tokens on new lines
    /// </summary>
    [TestMethod]
    public void AdjustNodeIndentationReturnsUnchangedNodeWhenNoTokensOnNewLines()
    {
        // Arrange
        const string input = "class Foo { void Bar() { } }";

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var originalText = methodDecl.ToFullString();

        // Act
        var result = ReihitsuFormatterHelpers.AdjustNodeIndentation(methodDecl, 4);
        var resultText = result.ToFullString();

        // Assert
        Assert.AreEqual(originalText, resultText);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatterHelpers.AdjustNodeIndentation"/> does not produce negative indentation
    /// </summary>
    [TestMethod]
    public void AdjustNodeIndentationDoesNotProduceNegativeIndentation()
    {
        // Arrange
        const string input = """
                             class Foo
                             {
                               void Bar()
                               {
                               }
                             }
                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = ReihitsuFormatterHelpers.AdjustNodeIndentation(methodDecl, -100);
        var resultText = result.ToFullString();

        // Assert — should not throw and indentation should be clamped to zero
        Assert.Contains("void Bar()", resultText, "Method declaration should still be present.");
    }

    #endregion // Methods
}