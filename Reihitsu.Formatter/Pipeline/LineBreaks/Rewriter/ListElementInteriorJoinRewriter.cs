using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
/// stays with the element's first token.
/// </para>
/// <para>
/// A gap that another owner already lays out is never joined: list boundaries (separators, element starts, closers),
/// brace and pattern constructs, operator wraps of binary, conditional, member-access, is-pattern, and assignment
/// expressions, multi-line literals, interpolation holes, attribute-list placement, and query clauses. When a new
/// rewriter takes ownership of a gap inside a list element, that gap has to be excluded here as well, or this rewriter
/// and the new owner would disagree about the same line break.
/// </para>
/// </remarks>
internal sealed class ListElementInteriorJoinRewriter : CSharpSyntaxRewriter
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
    public ListElementInteriorJoinRewriter(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a token starts a line inside a list element at a position this rewriter may join onto the
    /// previous token's line
    /// </summary>
    /// <param name="root">The root being formatted</param>
    /// <param name="token">The token that would be pulled onto the previous line</param>
    /// <param name="previousToken">The previous token, whose line the token would join</param>
    /// <returns><see langword="true"/> if the token may be joined; otherwise, <see langword="false"/></returns>
    private static bool CanJoin(SyntaxNode root, SyntaxToken token, out SyntaxToken previousToken)
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

        if (LineBreakTriviaUtilities.HasTrailingEndOfLine(previousToken) == false
            && token.LeadingTrivia.Any(SyntaxKind.EndOfLineTrivia) == false)
        {
            return false;
        }

        var element = FindInnermostListElement(token);

        if (element == null || element.GetFirstToken() == token)
        {
            return false;
        }

        return IsOwnedGap(previousToken, token, element) == false
               && LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, token) == false;
    }

    /// <summary>
    /// Finds the innermost ancestor of a token that is an element of an argument-, parameter-, attribute-, tuple-, or
    /// type-argument-like list
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The innermost list element containing the token, or <see langword="null"/> if there is none</returns>
    private static SyntaxNode FindInnermostListElement(SyntaxToken token)
    {
        for (var node = token.Parent; node != null; node = node.Parent)
        {
            if (IsListElement(node))
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a node is an element of a list whose element interiors this rewriter joins
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns><see langword="true"/> if the node is such a list element; otherwise, <see langword="false"/></returns>
    private static bool IsListElement(SyntaxNode node)
    {
        return node switch
               {
                   ArgumentSyntax => true,
                   AttributeArgumentSyntax => true,
                   ParameterSyntax { Parent: BaseParameterListSyntax } => true,
                   TypeParameterSyntax => true,
                   FunctionPointerParameterSyntax => true,
                   TupleElementSyntax => true,
                   AttributeSyntax => true,
                   TypeSyntax { Parent: TypeArgumentListSyntax } => true,
                   _ => false
               };
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

        for (var node = token.Parent; node != null && node != element; node = node.Parent)
        {
            if (IsOwningConstruct(node, token))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a node between a token and its list element owns the line breaks inside it
    /// </summary>
    /// <param name="node">The node on the path from the token to its list element</param>
    /// <param name="token">The token after the gap</param>
    /// <returns><see langword="true"/> if the node owns the token's line break; otherwise, <see langword="false"/></returns>
    private static bool IsOwningConstruct(SyntaxNode node, SyntaxToken token)
    {
        return node switch
               {
                   CollectionExpressionSyntax collection => token != collection.OpenBracketToken,
                   BlockSyntax => true,
                   InitializerExpressionSyntax => true,
                   AnonymousObjectCreationExpressionSyntax => true,
                   SwitchExpressionSyntax => true,
                   RecursivePatternSyntax => true,
                   ListPatternSyntax => true,
                   ParenthesizedPatternSyntax => true,
                   AccessorListSyntax => true,
                   InterpolationSyntax => true,
                   QueryBodySyntax => true,
                   _ => false
               };
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

        return text.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0;
    }

    /// <summary>
    /// Determines whether two joined tokens are written without a space between them. Delimiters, member, nullable, and
    /// argument-name punctuation, and prefix operators are written tight; every other pair gets one space, and the horizontal spacing
    /// phase normalizes the pairs its policy has a rule for. A pair that would merge into different tokens when written
    /// tight - <c>- -x</c>, <c>out var</c> - always keeps its space
    /// </summary>
    /// <param name="previousToken">The token before the gap</param>
    /// <param name="token">The token after the gap</param>
    /// <returns><see langword="true"/> if the tokens are joined without a space; otherwise, <see langword="false"/></returns>
    private static bool IsJoinedTight(SyntaxToken previousToken, SyntaxToken token)
    {
        var isTightPair = IsTightBefore(token) || IsTightAfter(previousToken);

        return isTightPair && LexesUnchanged(previousToken, token);
    }

    /// <summary>
    /// Determines whether a token is written directly after the previous token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if no space precedes the token; otherwise, <see langword="false"/></returns>
    private static bool IsTightBefore(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenParenToken => IsTightOpenParenthesis(token),
                   SyntaxKind.OpenBracketToken => token.Parent is BracketedArgumentListSyntax or ArrayRankSpecifierSyntax,
                   SyntaxKind.DotToken or SyntaxKind.ColonColonToken or SyntaxKind.DotDotToken => true,
                   SyntaxKind.QuestionToken => token.Parent is NullableTypeSyntax,
                   SyntaxKind.ColonToken => token.Parent is NameColonSyntax or ExpressionColonSyntax,
                   SyntaxKind.AsteriskToken => token.Parent is PointerTypeSyntax,
                   SyntaxKind.LessThanToken => token.Parent is TypeArgumentListSyntax or TypeParameterListSyntax or FunctionPointerParameterListSyntax,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether an opening parenthesis is written directly after the previous token: the argument list of an
    /// invocation, object creation, or attribute, the parameter list of a declaration that is not an anonymous
    /// function, and the operand of a keyword operator such as <c>typeof</c>. A parenthesized expression, a tuple, a
    /// cast, or a lambda parameter list keeps a space after a keyword or an argument name
    /// </summary>
    /// <param name="openParenthesis">The opening parenthesis</param>
    /// <returns><see langword="true"/> if no space precedes the parenthesis; otherwise, <see langword="false"/></returns>
    private static bool IsTightOpenParenthesis(SyntaxToken openParenthesis)
    {
        return openParenthesis.Parent switch
               {
                   ArgumentListSyntax or AttributeArgumentListSyntax => true,
                   ParameterListSyntax parameterList => parameterList.Parent is not AnonymousFunctionExpressionSyntax,
                   TypeOfExpressionSyntax or DefaultExpressionSyntax or SizeOfExpressionSyntax or CheckedExpressionSyntax or RefTypeExpressionSyntax or RefValueExpressionSyntax or MakeRefExpressionSyntax => true,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether the token after a token is written directly after it
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if no space follows the token; otherwise, <see langword="false"/></returns>
    private static bool IsTightAfter(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenParenToken or SyntaxKind.OpenBracketToken or SyntaxKind.DotToken or SyntaxKind.ColonColonToken or SyntaxKind.DotDotToken => true,
                   SyntaxKind.LessThanToken => token.Parent is TypeArgumentListSyntax or TypeParameterListSyntax or FunctionPointerParameterListSyntax,
                   SyntaxKind.CloseParenToken => token.Parent is CastExpressionSyntax,
                   _ => token.Parent is PrefixUnaryExpressionSyntax prefixUnary && token == prefixUnary.OperatorToken
               };
    }

    /// <summary>
    /// Determines whether two tokens written without a space between them still lex as the same two tokens
    /// </summary>
    /// <param name="previousToken">The first token</param>
    /// <param name="token">The second token</param>
    /// <returns><see langword="true"/> if the concatenated text lexes as the same two tokens; otherwise, <see langword="false"/></returns>
    private static bool LexesUnchanged(SyntaxToken previousToken, SyntaxToken token)
    {
        var lexed = SyntaxFactory.ParseTokens(previousToken.Text + token.Text).ToList();

        return lexed.Count == 3
               && lexed[0].RawKind == previousToken.RawKind
               && lexed[0].Text == previousToken.Text
               && lexed[0].HasTrailingTrivia == false
               && lexed[1].RawKind == token.RawKind
               && lexed[1].Text == token.Text
               && lexed[1].HasLeadingTrivia == false
               && lexed[2].IsKind(SyntaxKind.EndOfFileToken);
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

            if (CanJoin(root, token, out var previousToken) == false)
            {
                continue;
            }

            var separator = IsJoinedTight(previousToken, token)
                                ? SyntaxFactory.TriviaList()
                                : SyntaxFactory.TriviaList(SyntaxFactory.Space);
            var currentPrevious = replacements.TryGetValue(previousToken, out var replacedPrevious) ? replacedPrevious : previousToken;
            var currentToken = replacements.TryGetValue(token, out var replacedToken) ? replacedToken : token;
            var previousTrailing = LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(currentPrevious.TrailingTrivia);

            replacements[previousToken] = currentPrevious.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(previousTrailing).AddRange(separator));
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