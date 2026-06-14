using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.StructuralTransforms;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests for <see cref="ControlFlowBraceTransform"/> comment preservation when braces are inserted
/// </summary>
[TestClass]
public class ControlFlowBraceTransformTests : FormatterPhaseTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that a comment above an unbraced if-body statement is kept when the body is wrapped in a block
    /// </summary>
    [TestMethod]
    public void LeadingCommentAboveIfBodyIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool condition)
                                 {
                                     if (condition)
                                         // important
                                         Foo();
                                 }
                             }
                             """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.Contains("// important", actual, "The leading comment must not be deleted.");
        Assert.IsInstanceOfType<BlockSyntax>(GetIfStatement(actual).Statement, "The if-body should be wrapped in a block.");
    }

    /// <summary>
    /// Verifies that a trailing comment on an unbraced if-body statement is kept when the body is wrapped in a block
    /// </summary>
    [TestMethod]
    public void TrailingCommentOnIfBodyIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool condition)
                                 {
                                     if (condition)
                                         Foo(); // note
                                 }
                             }
                             """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.Contains("// note", actual, "The trailing comment must not be deleted.");
        Assert.IsInstanceOfType<BlockSyntax>(GetIfStatement(actual).Statement, "The if-body should be wrapped in a block.");
    }

    /// <summary>
    /// Verifies that a comment above an unbraced else-body statement is kept when the body is wrapped in a block
    /// </summary>
    [TestMethod]
    public void LeadingCommentAboveElseBodyIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool condition)
                                 {
                                     if (condition)
                                         Foo();
                                     else
                                         // else comment
                                         Bar();
                                 }
                             }
                             """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.Contains("// else comment", actual, "The else-body comment must not be deleted.");
        Assert.IsInstanceOfType<BlockSyntax>(GetIfStatement(actual).Else.Statement, "The else-body should be wrapped in a block.");
    }

    /// <summary>
    /// Verifies that an unbraced if-body without comments is still wrapped in a block (existing behavior)
    /// </summary>
    [TestMethod]
    public void UnbracedIfBodyWithoutCommentsIsWrappedInBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool condition)
                                 {
                                     if (condition)
                                         Foo();
                                 }
                             }
                             """;

        // Act
        var actual = ApplyPhase(input);

        // Assert
        Assert.IsInstanceOfType<BlockSyntax>(GetIfStatement(actual).Statement, "The if-body should be wrapped in a block.");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the given source and returns its single if-statement
    /// </summary>
    /// <param name="source">The source to parse</param>
    /// <returns>The if-statement contained in the source</returns>
    private IfStatementSyntax GetIfStatement(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);

        return tree.GetRoot(TestContext.CancellationToken)
                   .DescendantNodes()
                   .OfType<IfStatementSyntax>()
                   .Single();
    }

    #endregion // Methods

    #region FormatterPhaseTestsBase

    /// <inheritdoc/>
    protected override SyntaxNode ExecutePhase(SyntaxNode root, CancellationToken cancellationToken)
    {
        var context = new FormattingContext(Environment.NewLine);

        return new StructuralTransformPhase().Execute(root, context, cancellationToken);
    }

    #endregion // FormatterPhaseTestsBase
}