using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Blank line management — inserts required blank lines before and after
/// statements and comments, removes blank lines after opening braces, and
/// collapses excessive consecutive blank lines. Before C# 11, the phase leaves every interpolation of a
/// non-verbatim interpolated string as written, because such an interpolation cannot hold a line break there (CS8967)
/// </summary>
internal sealed class BlankLinePhase : IFormattingPhase
{
    #region Methods

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
                   new BlankLineTokenCleanupRewriter(context, cancellationToken),
                   new BlankLineTriviaBoundaryRewriter(context, editor, cancellationToken),
                   new BlankLineRegionDirectiveRewriter(context, editor, cancellationToken),
                   new BlankLineStatementSpacingRewriter(editor, cancellationToken),
                   new BlankLineBreakSpacingRewriter(editor, cancellationToken),
                   new BlankLineCollapser(cancellationToken),
               ];
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return SingleLineInterpolationUtilities.RunKeepingSingleLineInterpolations(root, context.LanguageVersion, CreateRewriters(context, cancellationToken), cancellationToken);
    }

    #endregion // IFormattingPhase
}