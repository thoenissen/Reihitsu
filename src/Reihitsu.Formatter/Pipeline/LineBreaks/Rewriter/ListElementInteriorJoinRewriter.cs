using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Data.Enumerations;
using Reihitsu.Formatter.Extensions;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Joins a line that starts inside an element of an argument-, parameter-, attribute-, tuple-, or type-argument-like
/// list at a token other than the element's first token onto the previous line, so a wrap the author placed inside an
/// element - <c>out⏎var value</c>, <c>message:⏎"x"</c>, <c>x =&gt;⏎x + 1</c> - does not survive formatting
/// </summary>
/// <remarks>
/// <para>
/// The rewriter runs before <see cref="LineBreakListRewriter"/>, so a list that was multi-line only because of such an
/// interior wrap is decided as the single-line list it becomes. A join is refused when a comment, a directive, or
/// disabled text sits in the gap, because each must keep its own line; the indentation phase then aligns the line that
/// stays with the element's first token. A join writes the minimal separator
/// (<see cref="LineBreakTriviaUtilities.CreateJoinSeparator"/>); every other space of the joined gap is the
/// horizontal-spacing phase's.
/// </para>
/// <para>
/// A gap that another owner already lays out is never joined: list boundaries (separators, element starts, closers),
/// the opening parenthesis of a parameter list other than a lambda's, operator wraps of binary, conditional, member-access, postfix,
/// is-pattern, and assignment expressions, multi-line literals, attribute-list placement, and every line inside a
/// construct <see cref="ListElementInteriorUtilities.FindOwningConstruct"/> names - brace scopes, initializers, switch
/// expressions, patterns, collection expression interiors, interpolation holes, and the clauses after a query's first
/// <c>from</c>. When a new rewriter takes ownership of a gap inside a list element, that gap has to be excluded here as
/// well, or this rewriter and the new owner would disagree about the same line break.
/// </para>
/// <para>
/// The join is a formatter-only layout decision: no analyzer rule reports such an interior wrap.
/// </para>
/// </remarks>
internal sealed class ListElementInteriorJoinRewriter : CSharpSyntaxRewriter
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
    public ListElementInteriorJoinRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a token starts a line inside a list element at a position this rewriter may join onto the
    /// previous token's line
    /// </summary>
    /// <param name="root">The root being formatted</param>
    /// <param name="rootConstruct">The facts about the construct the formatting root stands in</param>
    /// <param name="token">The token that would be pulled onto the previous line</param>
    /// <param name="previousToken">The previous token, whose line the token would join</param>
    /// <returns><see langword="true"/> if the token may be joined; otherwise, <see langword="false"/></returns>
    private static bool CanJoin(SyntaxNode root, RootConstruct rootConstruct, SyntaxToken token, out SyntaxToken previousToken)
    {
        previousToken = default;

        if (token.Span.IsEmpty || token.IsMissing)
        {
            return false;
        }

        previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None)
            || previousToken.Span.IsEmpty
            || root.FullSpan.Contains(previousToken.Span) == false)
        {
            return false;
        }

        if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token) == false)
        {
            return false;
        }

        var element = FindInnermostListElement(root, rootConstruct, token);

        if (element == null || element.GetFirstToken() == token)
        {
            return false;
        }

        return IsOwnedGap(previousToken, token, element) == false
               && LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false;
    }

    /// <summary>
    /// Finds the innermost ancestor of a token that is an element of an argument-, parameter-, attribute-, tuple-, or
    /// type-argument-like list. The formatting root has no parent, so when no element lies inside the root, the facts the
    /// entry point captured from the document say whether the root is, or lies inside, such an element with no owning
    /// construct between (<see cref="ListElementInteriorUtilities.LiesInListElement"/>); the root then stands for that
    /// element, and only the constructs inside the root are left to decide whether a gap is owned
    /// </summary>
    /// <param name="root">The root being formatted</param>
    /// <param name="rootConstruct">The facts about the construct the formatting root stands in</param>
    /// <param name="token">The token</param>
    /// <returns>The innermost list element containing the token, or <see langword="null"/> if there is none</returns>
    private static SyntaxNode FindInnermostListElement(SyntaxNode root, RootConstruct rootConstruct, SyntaxToken token)
    {
        for (var node = token.Parent; node != null; node = node.Parent)
        {
            if (ListElementInteriorUtilities.IsListElement(node))
            {
                return node;
            }
        }

        return rootConstruct.IsRootIn(root, RootConstruct.ListElement)
                   ? root
                   : null;
    }

    /// <summary>
    /// Determines whether the gap between two tokens of a list element is laid out by another owner and therefore
    /// must not be joined here
    /// </summary>
    /// <param name="previousToken">The token before the gap</param>
    /// <param name="token">The token after the gap</param>
    /// <param name="element">The innermost list element containing both tokens</param>
    /// <returns><see langword="true"/> if another owner decides this gap; otherwise, <see langword="false"/></returns>
    private static bool IsOwnedGap(SyntaxToken previousToken, SyntaxToken token, SyntaxNode element)
    {
        // List boundaries belong to the list rewriters and the alignment contributors
        if (token.IsKind(SyntaxKind.CommaToken)
            || previousToken.IsKind(SyntaxKind.CommaToken)
            || IsClosingToken(token))
        {
            return true;
        }

        // LineBreakListRewriter collapses the opening parenthesis of an anonymous method's parameter list onto the
        // delegate keyword's line, matching RH5105, which covers every parameter-list owner except a parenthesized
        // lambda; the lambda's opening gap has no other owner and is joined here
        if (token.IsKind(SyntaxKind.OpenParenToken) && token.Parent is ParameterListSyntax { Parent: not ParenthesizedLambdaExpressionSyntax })
        {
            return true;
        }

        // Multi-line literals keep the lines their text spans
        if (SpansLines(previousToken) || SpansLines(token))
        {
            return true;
        }

        if (IsOperatorWrap(previousToken) || IsOperatorWrap(token))
        {
            return true;
        }

        // Attribute-list placement belongs to the attribute target formatting
        if ((previousToken.IsKind(SyntaxKind.CloseBracketToken) && previousToken.Parent is AttributeListSyntax)
            || (token.IsKind(SyntaxKind.OpenBracketToken) && token.Parent is AttributeListSyntax))
        {
            return true;
        }

        if (token.Parent is InterpolatedStringExpressionSyntax interpolatedString)
        {
            return token != interpolatedString.StringStartToken || SpansLines(interpolatedString);
        }

        return ListElementInteriorUtilities.FindOwningConstruct(token, element) != null;
    }

    /// <summary>
    /// Determines whether a token closes a parenthesized, bracketed, braced, or angle-bracket list
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token is a closing delimiter; otherwise, <see langword="false"/></returns>
    private static bool IsClosingToken(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.CloseParenToken or SyntaxKind.CloseBracketToken or SyntaxKind.CloseBraceToken => true,
                   SyntaxKind.GreaterThanToken => token.Parent is TypeArgumentListSyntax or TypeParameterListSyntax or FunctionPointerParameterListSyntax,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a token is the operator of an expression whose operator wraps another rewriter lays out
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the token is such an operator; otherwise, <see langword="false"/></returns>
    private static bool IsOperatorWrap(SyntaxToken token)
    {
        return token.Parent switch
               {
                   BinaryExpressionSyntax binary => token == binary.OperatorToken,
                   BinaryPatternSyntax binaryPattern => token == binaryPattern.OperatorToken,
                   ConditionalExpressionSyntax conditional => token == conditional.QuestionToken || token == conditional.ColonToken,
                   MemberAccessExpressionSyntax memberAccess => token == memberAccess.OperatorToken,
                   ConditionalAccessExpressionSyntax conditionalAccess => token == conditionalAccess.OperatorToken,
                   MemberBindingExpressionSyntax memberBinding => token == memberBinding.OperatorToken,
                   PostfixUnaryExpressionSyntax postfixUnary => token == postfixUnary.OperatorToken,
                   IsPatternExpressionSyntax isPattern => token == isPattern.IsKeyword,
                   AssignmentExpressionSyntax assignment => token == assignment.OperatorToken,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether the text of a token or node spans more than one line
    /// </summary>
    /// <param name="nodeOrToken">The token or node</param>
    /// <returns><see langword="true"/> if its text contains a line break; otherwise, <see langword="false"/></returns>
    private static bool SpansLines(SyntaxNodeOrToken nodeOrToken)
    {
        var text = nodeOrToken.IsToken
                       ? nodeOrToken.AsToken().Text
                       : nodeOrToken.AsNode().ToString();

        return LineEndingUtilities.ContainsLineTerminator(text);
    }

    /// <summary>
    /// Joins every interior wrap of a list element inside the node onto the previous line
    /// </summary>
    /// <param name="root">The node to rewrite</param>
    /// <returns>The rewritten node, or the original node when nothing was joined</returns>
    private SyntaxNode JoinInteriorWraps(SyntaxNode root)
    {
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var token in root.DescendantTokens())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (CanJoin(root, _context.RootConstruct, token, out var previousToken) == false)
            {
                continue;
            }

            var currentPrevious = replacements.TryGetValue(previousToken, out var replacedPrevious) ? replacedPrevious : previousToken;
            var currentToken = replacements.TryGetValue(token, out var replacedToken) ? replacedToken : token;

            replacements[previousToken] = currentPrevious.WithTrailingTrivia(LineBreakTriviaUtilities.CreateJoinedTrailingTrivia(currentPrevious, token));
            replacements[token] = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(currentToken);
        }

        if (replacements.Count == 0)
        {
            return root;
        }

        return root.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        return node == null
                   ? null
                   : JoinInteriorWraps(node);
    }

    #endregion // CSharpSyntaxVisitor
}