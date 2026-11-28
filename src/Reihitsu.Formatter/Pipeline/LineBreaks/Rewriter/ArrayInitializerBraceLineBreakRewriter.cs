using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Puts the braces of every array initializer that spans several lines on lines of their own (FMT-BREAK-046): the first element
/// starts the line behind the opening brace, the closing brace starts its line, the opening brace of an array creation, a
/// <c>stackalloc</c> array creation, and a row starts its own line, and the element behind a multi-line row starts the next
/// line. The opening brace of a declaration's initializer stays on the line of the <c>=</c>. An initializer on one line is left
/// as it is, and so is every other line break between the elements
/// </summary>
/// <remarks>
/// The rewriter runs after the rewriters that join or break lines inside an element and before
/// <see cref="LineBreakListRewriter"/>, so each initializer is judged on the lines the joins and the layouts of its elements
/// leave, and a list whose element it makes multi-line is decided as the multi-line list it becomes. A comment keeps its
/// line: a brace or an element that has to start its line moves onto the line below the comment in front of it
/// </remarks>
internal sealed class ArrayInitializerBraceLineBreakRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The token gap normalizer
    /// </summary>
    private readonly TokenGapNormalizer _gapNormalizer;

    /// <summary>
    /// The brace placer
    /// </summary>
    private readonly BracePlacer _bracePlacer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public ArrayInitializerBraceLineBreakRewriter(FormattingContext context,
                                                  TokenGapNormalizer gapNormalizer,
                                                  BracePlacer bracePlacer,
                                                  CancellationToken cancellationToken)
    {
        _context = context;
        _gapNormalizer = gapNormalizer;
        _bracePlacer = bracePlacer;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether an initializer is an array initializer that spans several lines: a line break stands in the text from
    /// the end of its opening brace to the start of its closing brace, in the trivia between its tokens or inside one of them,
    /// such as a verbatim string
    /// </summary>
    /// <param name="initializer">The initializer, or <see langword="null"/></param>
    /// <returns><see langword="true"/> if the initializer is a multi-line array initializer</returns>
    private static bool IsMultiLineArrayInitializer(InitializerExpressionSyntax initializer)
    {
        if (initializer == null
            || initializer.IsKind(SyntaxKind.ArrayInitializerExpression) == false
            || initializer.OpenBraceToken.IsMissing
            || initializer.CloseBraceToken.IsMissing)
        {
            return false;
        }

        var text = initializer.ToFullString();
        var start = initializer.OpenBraceToken.Span.End - initializer.FullSpan.Start;
        var end = initializer.CloseBraceToken.SpanStart - initializer.FullSpan.Start;

        return text.AsSpan(start, end - start).IndexOfAny('\r', '\n') >= 0;
    }

    /// <summary>
    /// Moves a token onto a line of its own when anything but whitespace stands in front of it on its line: a line break goes
    /// directly in front of the token, so a comment in front of it, in its leading trivia or behind the previous token, keeps
    /// its line and the token starts the line below it. The whitespace in front of the token on the comment's line is removed;
    /// the indentation phase gives the token its column
    /// </summary>
    /// <typeparam name="TNode">The syntax node type containing the token</typeparam>
    /// <param name="node">The node containing the token</param>
    /// <param name="token">The token that has to start its line</param>
    /// <returns>The node with the token starting its line</returns>
    private TNode MoveTokenBelowItsLine<TNode>(TNode node, SyntaxToken token)
        where TNode : SyntaxNode
    {
        if (token.IsMissing
            || SyntaxTokenPositionUtilities.StartsLine(token))
        {
            return node;
        }

        var leadingTrivia = TriviaListUtilities.RemoveTrailingWhitespace(token.LeadingTrivia);

        return node.ReplaceToken(token, token.WithLeadingTrivia(leadingTrivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine))));
    }

    /// <summary>
    /// Moves the opening brace of the multi-line initializer of an array creation or a <c>stackalloc</c> array creation onto
    /// a line of its own
    /// </summary>
    /// <typeparam name="TNode">The creation's syntax node type</typeparam>
    /// <param name="node">The creation</param>
    /// <param name="initializer">The creation's initializer, or <see langword="null"/></param>
    /// <returns>The creation with its opening brace starting its line</returns>
    private TNode PlaceCreationBrace<TNode>(TNode node, InitializerExpressionSyntax initializer)
        where TNode : SyntaxNode
    {
        return IsMultiLineArrayInitializer(initializer)
                   ? MoveTokenBelowItsLine(node, initializer.OpenBraceToken)
                   : node;
    }

    /// <summary>
    /// Lays out the rows of a multi-line array initializer: a multi-line row's opening brace starts its line, and the element
    /// behind the comma that follows it starts the next line
    /// </summary>
    /// <param name="node">The multi-line array initializer</param>
    /// <returns>The initializer with its multi-line rows starting and ending their lines</returns>
    private InitializerExpressionSyntax PlaceMultiLineRows(InitializerExpressionSyntax node)
    {
        for (var elementIndex = 0; elementIndex < node.Expressions.Count; elementIndex++)
        {
            if (node.Expressions[elementIndex] is not InitializerExpressionSyntax row
                || IsMultiLineArrayInitializer(row) == false)
            {
                continue;
            }

            node = MoveTokenBelowItsLine(node, row.OpenBraceToken);

            if (elementIndex + 1 < node.Expressions.Count)
            {
                node = MoveTokenBelowItsLine(node, node.Expressions[elementIndex + 1].GetFirstToken());
            }
        }

        return node;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InitializerExpressionSyntax)base.VisitInitializerExpression(node);

        if (IsMultiLineArrayInitializer(node) == false)
        {
            return node;
        }

        node = _bracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = PlaceMultiLineRows(node);

        return _gapNormalizer.NormalizeGapBeforeOwnedToken(node, node.CloseBraceToken, static (owner, token) => owner.WithCloseBraceToken(token));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ArrayCreationExpressionSyntax)base.VisitArrayCreationExpression(node);

        return PlaceCreationBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ImplicitArrayCreationExpressionSyntax)base.VisitImplicitArrayCreationExpression(node);

        return PlaceCreationBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (StackAllocArrayCreationExpressionSyntax)base.VisitStackAllocArrayCreationExpression(node);

        return PlaceCreationBrace(node, node.Initializer);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ImplicitStackAllocArrayCreationExpressionSyntax)base.VisitImplicitStackAllocArrayCreationExpression(node);

        return PlaceCreationBrace(node, node.Initializer);
    }

    #endregion // CSharpSyntaxVisitor
}