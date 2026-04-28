using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="CommentIndentationContributor"/>
/// </summary>
[TestClass]
public class CommentIndentationContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a single-line comment preceding a token is aligned to the token's layout column
    /// </summary>
    [TestMethod]
    public void AlignsCommentToFollowingTokenLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // comment
                                     var x = 1;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populate the model with layout for the var token line
        var varDecl = root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().First();
        var varToken = varDecl.GetFirstToken();
        var varLine = LayoutComputer.GetLine(varToken);
        model.Set(varLine, new TokenLayout(8, "Block"));

        var contributor = new CommentIndentationContributor();

        // Act
        contributor.Contribute(root, scope, model, context);

        // Assert — the comment on the line before var should have the same column
        var commentLine = varLine - 1;
        Assert.IsTrue(model.TryGetLayout(commentLine, out var commentLayout), "Expected layout for comment line");
        Assert.AreEqual(8, commentLayout.Column, "Comment should align to the following token's column");
    }

    /// <summary>
    /// Verifies that when no layout exists for the token, no comment alignment is performed
    /// </summary>
    [TestMethod]
    public void DoesNotAlignCommentWhenTokenHasNoLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // comment
                                     var x = 1;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CommentIndentationContributor();

        // Act — no pre-populated layout
        contributor.Contribute(root, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Without pre-populated layout, no comment alignment should occur");
    }

    /// <summary>
    /// Verifies that comments on the same line as the token are not re-aligned
    /// </summary>
    [TestMethod]
    public void DoesNotAlignInlineComment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1; // inline comment
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populate model for the var token
        var varDecl = root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().First();
        var varToken = varDecl.GetFirstToken();
        var varLine = LayoutComputer.GetLine(varToken);
        model.Set(varLine, new TokenLayout(8, "Block"));

        var contributor = new CommentIndentationContributor();
        var countBefore = model.Count;

        // Act
        contributor.Contribute(root, scope, model, context);

        // Assert — inline comment on same line as token should not add extra entries
        Assert.AreEqual(countBefore, model.Count, "Inline comments should not create additional layout entries");
    }

    /// <summary>
    /// Verifies that multiple comments before a token are all aligned to the token's layout
    /// </summary>
    [TestMethod]
    public void AlignsMultipleCommentsBeforeToken()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // first comment
                                     // second comment
                                     var x = 1;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        var varDecl = root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().First();
        var varToken = varDecl.GetFirstToken();
        var varLine = LayoutComputer.GetLine(varToken);
        model.Set(varLine, new TokenLayout(8, "Block"));

        var contributor = new CommentIndentationContributor();

        // Act
        contributor.Contribute(root, scope, model, context);

        // Assert — both comment lines should be aligned
        Assert.IsTrue(model.TryGetLayout(varLine - 1, out var comment2Layout), "Expected layout for second comment line");
        Assert.AreEqual(8, comment2Layout.Column);

        Assert.IsTrue(model.TryGetLayout(varLine - 2, out var comment1Layout), "Expected layout for first comment line");
        Assert.AreEqual(8, comment1Layout.Column);
    }

    /// <summary>
    /// Verifies that a comment as the only element in a scope gets correct indentation layout
    /// </summary>
    [TestMethod]
    public void AlignsCommentWhenOnlyElementInScope()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int kind)
                                 {
                                     if (kind == 1)
                                     {
                                     }
                                     else if (kind == 2)
                                     {
                                         // Only comment in scope
                                     }
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populate the model with layout for the closing brace that follows the comment
        var elseBlock = root.DescendantNodes().OfType<BlockSyntax>().Last();
        var closeBrace = elseBlock.CloseBraceToken;
        var closeBraceLine = LayoutComputer.GetLine(closeBrace);
        model.Set(closeBraceLine, new TokenLayout(8, "Block"));

        var contributor = new CommentIndentationContributor();
        var countBefore = model.Count;

        // Act
        contributor.Contribute(root, scope, model, context);

        // Assert — the comment trivia should get a layout entry
        Assert.IsGreaterThan(countBefore, model.Count, "Should produce layout for comment-only scope");
    }

    #endregion // Methods
}