using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Blank line management — inserts required blank lines before and after
/// statements and comments, removes blank lines after opening braces, and
/// collapses excessive consecutive blank lines
/// </summary>
internal sealed class BlankLinePhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies blank line formatting rules to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var current = root;

        foreach (var rewriter in CreateRewriters(context, cancellationToken))
        {
            current = rewriter.Visit(current);

            cancellationToken.ThrowIfCancellationRequested();
        }

        return current;
    }

    /// <summary>
    /// Creates the ordered blank-line subphase rewriters
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ordered list of rewriters to execute</returns>
    private static IReadOnlyList<CSharpSyntaxRewriter> CreateRewriters(FormattingContext context,
                                                                       CancellationToken cancellationToken)
    {
        var editor = new BlankLineEditor(context);

        return [
                   new BlankLineTokenCleanupRewriter(cancellationToken),
                   new BlankLineTriviaBoundaryRewriter(context, editor, cancellationToken),
                   new BlankLineStatementSpacingRewriter(editor, cancellationToken),
                   new BlankLineBreakSpacingRewriter(context, editor, cancellationToken),
                   new BlankLineCollapser(cancellationToken),
               ];
    }

    #endregion // Methods
}