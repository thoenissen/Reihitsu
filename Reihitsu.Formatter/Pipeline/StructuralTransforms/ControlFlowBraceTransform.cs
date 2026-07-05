using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Inserts missing braces for control-flow statements handled by the formatter
/// </summary>
internal sealed class ControlFlowBraceTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public ControlFlowBraceTransform(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Wraps a statement in a block when it is not already braced
    /// </summary>
    /// <param name="statement">The statement to inspect</param>
    /// <returns>The braced statement</returns>
    private static StatementSyntax WrapWithBraces(StatementSyntax statement)
    {
        if (statement is null or BlockSyntax)
        {
            return statement;
        }

        var leadingComments = CollectComments(statement.GetLeadingTrivia());

        var normalizedStatement = statement.WithLeadingTrivia(NormalizeLeadingTrivia(leadingComments))
                                           .WithTrailingTrivia(NormalizeTrailingTrivia(statement.GetTrailingTrivia()));

        var block = SyntaxFactory.Block(SyntaxFactory.SingletonList(normalizedStatement));

        if (leadingComments.Count > 0)
        {
            // The end-of-line after the open brace must live on the brace token (mirroring
            // normally-parsed code); otherwise the blank-line phase treats a leading end-of-line
            // on the first statement as a blank line and strips it, gluing the comment to the brace.
            block = block.WithOpenBraceToken(block.OpenBraceToken.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
        }

        return block;
    }

    /// <summary>
    /// Builds the leading trivia for a statement that is being wrapped in braces.
    /// Whitespace and end-of-line trivia are dropped because the pipeline re-applies layout,
    /// but comments are preserved so they are not silently deleted
    /// </summary>
    /// <param name="leadingComments">The comments collected from the original leading trivia of the statement</param>
    /// <returns>The normalized leading trivia</returns>
    private static SyntaxTriviaList NormalizeLeadingTrivia(List<SyntaxTrivia> leadingComments)
    {
        if (leadingComments.Count == 0)
        {
            return SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker);
        }

        var result = new List<SyntaxTrivia>();

        foreach (var comment in leadingComments)
        {
            result.Add(comment);
            result.Add(SyntaxFactory.ElasticCarriageReturnLineFeed);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Builds the trailing trivia for a statement that is being wrapped in braces.
    /// A trailing comment (for example <c>Foo(); // note</c>) is preserved on the same line as the statement
    /// </summary>
    /// <param name="trailingTrivia">The original trailing trivia of the statement</param>
    /// <returns>The normalized trailing trivia</returns>
    private static SyntaxTriviaList NormalizeTrailingTrivia(SyntaxTriviaList trailingTrivia)
    {
        var comments = CollectComments(trailingTrivia);

        if (comments.Count == 0)
        {
            return SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker);
        }

        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.Space
                     };

        foreach (var comment in comments)
        {
            result.Add(comment);
        }

        result.Add(SyntaxFactory.ElasticMarker);

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Collects the comment trivia from a trivia list, preserving their order
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The comment trivia contained in the list</returns>
    private static List<SyntaxTrivia> CollectComments(SyntaxTriviaList triviaList)
    {
        return triviaList.Where(ReihitsuFormatterHelpers.IsCommentTrivia)
                         .ToList();
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (IfStatementSyntax)base.VisitIfStatement(node);

        if (node == null)
        {
            return null;
        }

        node = node.WithStatement(WrapWithBraces(node.Statement));

        if (node.Else is { Statement: not null and not BlockSyntax and not IfStatementSyntax } elseClause)
        {
            node = node.WithElse(elseClause.WithStatement(WrapWithBraces(elseClause.Statement)));
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}