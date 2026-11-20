using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Joins an attached part onto the line of the token in front of it when a line break separates the two, wherever the
/// part stands: the opening parenthesis of an argument list (<c>Foo⏎(4)</c>, <c>new List&lt;int&gt;⏎(4)</c>,
/// <c>: base⏎(1)</c>, <c>[with⏎(4)]</c>; FMT-BREAK-039), and the first token of an element access, a conditional element
/// access, or a null-forgiving operator (<c>arr⏎[0]</c>, <c>n⏎?[0]</c>, <c>Get()⏎!</c>; FMT-BREAK-040). A line break
/// between the <c>?</c> and the <c>[</c> of a conditional element access is removed as well
/// </summary>
/// <remarks>
/// <para>
/// This rewriter is the one owner of these gaps (<see cref="AttachedPartGapUtilities.IsAttachedPartGap"/>): the chain
/// rewriter and the list-element join leave them alone.
/// </para>
/// <para>
/// It runs after the initializer rewriter, so a collection expression or a complex element initializer that was written
/// over several lines is decided as written, and before the list, binary-operator, chain, and conditional rewriters, so
/// a list or a conditional expression is decided as the line it becomes. Blank lines in the gap go with the join. No gap
/// it joins holds a comment, a directive, or disabled text: the precheck refuses an input that holds one there
/// (FMT-UNSUP-014). A join writes the minimal separator (<see cref="LineBreakTriviaUtilities.CreateJoinSeparator"/>). The
/// first token of a part that is the formatting root's first token has no token in front of it inside the root, so it
/// keeps its line.
/// </para>
/// </remarks>
internal sealed class AttachedPartLineStartRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public AttachedPartLineStartRewriter(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the gap in front of a token of an attached part may be joined: the gap is one of an attached
    /// part, a line break separates the token from the token in front of it, and that token lies inside the root
    /// </summary>
    /// <param name="root">The root being formatted</param>
    /// <param name="token">The token behind the gap</param>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <returns><see langword="true"/> if the gap may be joined; otherwise, <see langword="false"/></returns>
    private static bool CanJoin(SyntaxNode root, SyntaxToken token, out SyntaxToken previousToken)
    {
        previousToken = default;

        if (token.IsMissing
            || AttachedPartGapUtilities.IsAttachedPartGap(token) == false)
        {
            return false;
        }

        previousToken = SyntaxTokenNavigationUtilities.GetPreviousToken(token);

        return previousToken.IsKind(SyntaxKind.None) == false
               && previousToken.Span.IsEmpty == false
               && root.FullSpan.Contains(previousToken.Span)
               && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token);
    }

    /// <summary>
    /// Selects the gaps of attached parts that may be joined (<see cref="CanJoin"/>), in text order
    /// </summary>
    /// <param name="root">The node to rewrite</param>
    /// <returns>The gaps to join, each given by the token in front of it and the token behind it</returns>
    private IEnumerable<(SyntaxToken PreviousToken, SyntaxToken Token)> SelectJoinableGaps(SyntaxNode root)
    {
        foreach (var token in root.DescendantTokens())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (CanJoin(root, token, out var previousToken))
            {
                yield return (previousToken, token);
            }
        }
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return node == null
                   ? null
                   : LineBreakTriviaUtilities.JoinGaps(node, SelectJoinableGaps(node));
    }

    #endregion // CSharpSyntaxVisitor
}