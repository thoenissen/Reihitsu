using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Joins the opening parenthesis of an argument list onto the line of the token in front of it when a line break
/// separates the two (<c>Foo⏎(4)</c>, <c>new List&lt;int&gt;⏎(4)</c>, <c>: base⏎(1)</c>, <c>[with⏎(4)]</c>), wherever
/// the argument list stands
/// </summary>
/// <remarks>
/// <para>
/// This rewriter is the one owner of that gap: the chain rewriter and the list-element join leave it alone. It covers
/// the argument list of an invocation, of an object creation and an implicit object creation, of a constructor
/// initializer, of a primary-constructor base type, and of a collection expression's <c>with(…)</c> element; the
/// bracketed argument list of an element access and an attribute's argument list are not its gaps.
/// </para>
/// <para>
/// It runs after the initializer rewriter, so a collection expression or a complex element initializer that was written
/// over several lines is decided as written, and before the list, binary-operator, chain, and conditional rewriters, so
/// a list or a conditional expression is decided as the line it becomes. A gap that holds a comment, a directive, or
/// disabled text is not joined; blank lines in the gap go with the join. A join writes the minimal separator
/// (<see cref="LineBreakTriviaUtilities.CreateJoinSeparator"/>). The opening parenthesis of an argument list that is
/// the formatting root's first token has no token in front of it inside the root, so it keeps its line.
/// </para>
/// </remarks>
internal sealed class ArgumentListLineStartRewriter : CSharpSyntaxRewriter
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
    public ArgumentListLineStartRewriter(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a token is the opening parenthesis of an argument list whose gap in front this rewriter owns
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token opens such an argument list; otherwise, <see langword="false"/></returns>
    internal static bool IsArgumentListOpenParenthesis(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.OpenParenToken)
               && token.Parent is ArgumentListSyntax
                               {
                                   Parent: InvocationExpressionSyntax
                                           or ObjectCreationExpressionSyntax
                                           or ImplicitObjectCreationExpressionSyntax
                                           or ConstructorInitializerSyntax
                                           or PrimaryConstructorBaseTypeSyntax
                                           or WithElementSyntax
                               };
    }

    /// <summary>
    /// Determines whether the opening parenthesis of an argument list may be joined onto the line of the token in front of
    /// it: a line break separates the two, the token in front lies inside the root, and no comment, directive, or
    /// disabled text sits in the gap
    /// </summary>
    /// <param name="root">The root being formatted</param>
    /// <param name="token">The opening parenthesis</param>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <returns><see langword="true"/> if the gap may be joined; otherwise, <see langword="false"/></returns>
    private static bool CanJoin(SyntaxNode root, SyntaxToken token, out SyntaxToken previousToken)
    {
        previousToken = default;

        if (token.IsMissing
            || IsArgumentListOpenParenthesis(token) == false)
        {
            return false;
        }

        previousToken = token.GetPreviousToken();

        return previousToken.IsKind(SyntaxKind.None) == false
               && previousToken.Span.IsEmpty == false
               && root.FullSpan.Contains(previousToken.Span)
               && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token)
               && LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false;
    }

    /// <summary>
    /// Selects the gaps in front of an argument list's opening parenthesis that may be joined (<see cref="CanJoin"/>), in
    /// text order
    /// </summary>
    /// <param name="root">The node to rewrite</param>
    /// <returns>The gaps to join, each given by the token in front of it and the opening parenthesis behind it</returns>
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