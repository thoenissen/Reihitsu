using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Enumerations;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Joins the token behind a gap onto the line of the token in front of it, removing the line breaks and blank lines of the
/// gap, for the gaps behind a keyword, <c>is</c>, <c>not</c>, a modifier, a cast, an arm, a base list, a filter, a loop header,
/// a lambda, or in front of <c>with</c> (FMT-BREAK-041), wherever the gap stands
/// </summary>
/// <remarks>
/// <para>
/// <see cref="JoinedGapUtilities.GetGap"/> states the gaps; this rewriter joins those of FMT-BREAK-041. The gap behind a pattern
/// combinator <c>and</c> or <c>or</c> is the pattern chain's (FMT-BREAK-017), the gaps around the <c>=</c> of an
/// anonymous-object member and of a <c>let</c> clause are the assignment rewriter's (FMT-BREAK-016), and the gaps behind the
/// modifiers of a parameter are the list-element join's (FMT-BREAK-015). The pattern behind <c>is</c> or <c>not</c> keeps its own
/// line where FMT-BREAK-022 starts its first token on a line of its own
/// (<see cref="PatternLineStartUtilities.StartsOwnLine"/>).
/// </para>
/// <para>
/// It runs after the initializer rewriter, so a collection expression or an initializer that was written over several lines
/// is decided as written, and before the list, binary-operator, chain, and conditional rewriters, so they decide on the
/// joined lines. No gap it joins holds a comment, a directive, or disabled text: the precheck refuses an input that holds one
/// there (FMT-UNSUP-011, FMT-UNSUP-017). A join writes the minimal separator
/// (<see cref="LineBreakTriviaUtilities.CreateJoinSeparator"/>). A gap whose token in front lies outside the root is not
/// joined.
/// </para>
/// </remarks>
internal sealed class ContinuationJoinRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public ContinuationJoinRewriter(FormattingContext context,
                                    CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a gap is one this rewriter joins (FMT-BREAK-041). The token behind the gap belongs to the node this
    /// rewriter visits, whose parents are intact, so FMT-BREAK-022's case-label condition is read from them and, at the
    /// formatting root, from the root's facts
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="token">The token behind the gap</param>
    /// <returns><see langword="true"/> if the gap is joined; otherwise, <see langword="false"/></returns>
    private bool IsJoinedGap(JoinedGap gap, SyntaxToken previousToken, SyntaxToken token)
    {
        return gap switch
               {
                   JoinedGap.IsPattern => PatternLineStartUtilities.StartsOwnLine(token, _context.RootConstruct) == false,
                   JoinedGap.PatternCombinator => previousToken.IsKind(SyntaxKind.NotKeyword)
                                                  && PatternLineStartUtilities.StartsOwnLine(token, _context.RootConstruct) == false,
                   JoinedGap.KeywordOperand
                   or JoinedGap.TypeTest
                   or JoinedGap.SwitchArmWhen
                   or JoinedGap.SwitchArmArrow
                   or JoinedGap.BaseList
                   or JoinedGap.Modifier
                   or JoinedGap.CatchFilter
                   or JoinedGap.ForHeader
                   or JoinedGap.ForEachExpression
                   or JoinedGap.Cast
                   or JoinedGap.LambdaArrow
                   or JoinedGap.With => true,
                   _ => false
               };
    }

    /// <summary>
    /// Selects the gaps that are joined, in text order: a line break separates the two tokens, the token in front lies inside
    /// the root, and the gap is one of FMT-BREAK-041
    /// </summary>
    /// <param name="root">The node to rewrite</param>
    /// <returns>The gaps to join, each given by the token in front of it and the token behind it</returns>
    private IEnumerable<(SyntaxToken PreviousToken, SyntaxToken Token)> SelectJoinableGaps(SyntaxNode root)
    {
        SyntaxToken previousToken = default;

        foreach (var token in root.DescendantTokens())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (token.Span.IsEmpty)
            {
                continue;
            }

            if (previousToken != default
                && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token)
                && IsJoinedGap(JoinedGapUtilities.GetGap(previousToken, token), previousToken, token))
            {
                yield return (previousToken, token);
            }

            previousToken = token;
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