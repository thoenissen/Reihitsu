using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Inserts missing braces for control-flow statements handled by the formatter
/// </summary>
internal sealed class ControlFlowBraceTransform : CSharpSyntaxRewriter
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

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public ControlFlowBraceTransform(FormattingContext context,
                                     CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Wraps a statement in a block when it is not already braced. The block's braces carry only the separators the text
    /// needs to parse back to the block (FMT-STRUCT-026): a line break behind the opening brace when a comment leads the
    /// statement, a line break behind a single-line comment that ends the statement's trailing trivia without one, and a line
    /// break behind the closing brace when a comment or a directive leads the token that follows it. The statement keeps the
    /// comments around it together with the whitespace and line breaks the author wrote in front of and between them
    /// </summary>
    /// <param name="statement">The statement to inspect</param>
    /// <param name="nextToken">The token that follows the inserted closing brace, read from the original tree, whose leading trivia decides whether the closing brace needs a line break behind it; <see langword="default"/> when another inserted closing brace follows it</param>
    /// <param name="endOfLine">The end-of-line sequence of a separator line break</param>
    /// <returns>The braced statement</returns>
    private static StatementSyntax WrapWithBraces(StatementSyntax statement,
                                                  SyntaxToken nextToken,
                                                  string endOfLine)
    {
        if (CanWrap(statement) == false)
        {
            return statement;
        }

        // The statement carries its comments as written; the later phases lay out the line breaks in front of the first
        // comment's line, and the closing brace starts a new line exactly when the author ended the statement's line
        var leadingTrivia = BlockWrapTriviaUtilities.GetCarriedLeadingTrivia(statement.GetLeadingTrivia(), keepBlankLinesAboveFirstComment: false);
        var normalizedStatement = statement.WithLeadingTrivia(leadingTrivia)
                                           .WithTrailingTrivia(BlockWrapTriviaUtilities.GetCarriedTrailingTrivia(statement.GetTrailingTrivia(), SyntaxFactory.EndOfLine(endOfLine), endsLine: false));

        // A comment behind the opening brace on its line would trail the brace once the text is parsed again, so the
        // brace's line ends in front of the statement's first comment
        var openBraceTrailingTrivia = leadingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia)
                                          ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                                          : default;

        // The comment or directive in front of the next token starts its line in the original text; behind the closing
        // brace on its line, a comment would trail the brace and a directive would not start a line
        var closeBraceTrailingTrivia = StartsWithCommentOrDirective(nextToken.LeadingTrivia)
                                           ? SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(endOfLine))
                                           : default;

        // The braces are built with explicit trivia, so they carry no elastic trivia (FMT-STRUCT-026)
        return SyntaxFactory.Block(SyntaxFactory.Token(default, SyntaxKind.OpenBraceToken, openBraceTrailingTrivia),
                                   SyntaxFactory.SingletonList(normalizedStatement),
                                   SyntaxFactory.Token(default, SyntaxKind.CloseBraceToken, closeBraceTrailingTrivia));
    }

    /// <summary>
    /// Determines whether a statement gets braces. A block needs none. A preprocessor directive or disabled text in the
    /// statement's trivia would end up between the statement and a brace on a line the directive does not own, silently
    /// changing which code compiles under which configuration, so such a statement stays unbraced
    /// </summary>
    /// <param name="statement">The statement</param>
    /// <returns><see langword="true"/> if the statement is wrapped in braces; otherwise, <see langword="false"/></returns>
    private static bool CanWrap(StatementSyntax statement)
    {
        return statement is not null and not BlockSyntax
               && statement.GetLeadingTrivia().Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia) == false
               && statement.GetTrailingTrivia().Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia) == false;
    }

    /// <summary>
    /// Gets the token that follows the closing brace inserted behind an <c>if</c> or <c>else</c> body, read from the
    /// original tree: the <c>else</c> keyword behind an <c>if</c> body, or the token behind the whole <c>if</c> statement.
    /// When that statement is itself a body this transform braces, its own inserted closing brace follows, so no token of
    /// the original tree does
    /// </summary>
    /// <param name="body">The body of the original tree</param>
    /// <returns>The token that follows the inserted closing brace, or <see langword="default"/> when another inserted closing brace follows it</returns>
    private static SyntaxToken GetTokenBehindInsertedCloseBrace(StatementSyntax body)
    {
        return body.Parent switch
               {
                   IfStatementSyntax { Else: not null } ifStatement when ifStatement.Statement == body => ifStatement.Else.ElseKeyword,
                   IfStatementSyntax ifStatement => GetTokenBehindIfStatement(ifStatement),
                   ElseClauseSyntax { Parent: IfStatementSyntax ifStatement } => GetTokenBehindIfStatement(ifStatement),
                   _ => body.GetLastToken().GetNextToken(includeZeroWidth: true)
               };
    }

    /// <summary>
    /// Gets the token that follows an <c>if</c> statement once the transform has braced the bodies around it, read from the
    /// original tree. An <c>else if</c> stays unbraced, so what follows it is what follows the <c>if</c> statement it
    /// belongs to; an <c>if</c> that is the body of another <c>if</c> or <c>else</c> is followed by that body's inserted
    /// closing brace when the body gets braces
    /// </summary>
    /// <param name="ifStatement">The <c>if</c> statement of the original tree</param>
    /// <returns>The token that follows the statement, or <see langword="default"/> when an inserted closing brace follows it</returns>
    private static SyntaxToken GetTokenBehindIfStatement(IfStatementSyntax ifStatement)
    {
        return ifStatement.Parent switch
               {
                   ElseClauseSyntax { Parent: IfStatementSyntax outerIfStatement } => GetTokenBehindIfStatement(outerIfStatement),
                   IfStatementSyntax outerIfStatement when outerIfStatement.Statement == ifStatement => CanWrap(ifStatement)
                                                                                                            ? default
                                                                                                            : GetTokenBehindInsertedCloseBrace(ifStatement),
                   _ => ifStatement.GetLastToken().GetNextToken(includeZeroWidth: true)
               };
    }

    /// <summary>
    /// Determines whether a comment, a directive, or disabled text is the first trivia on the line a leading trivia list
    /// starts on, in front of any line break
    /// </summary>
    /// <param name="leadingTrivia">The leading trivia of the token that follows the braced statement</param>
    /// <returns><see langword="true"/> if the first trivia other than whitespace, in front of any line break, is a comment, a directive, or disabled text; otherwise, <see langword="false"/></returns>
    private static bool StartsWithCommentOrDirective(SyntaxTriviaList leadingTrivia)
    {
        foreach (var trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            return SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                   || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia);
        }

        return false;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        // The token behind each inserted closing brace is read from the original tree, because the visited node is
        // detached from it
        var statementNextToken = GetTokenBehindInsertedCloseBrace(node.Statement);
        var elseNextToken = node.Else != null
                                ? GetTokenBehindInsertedCloseBrace(node.Else.Statement)
                                : default;

        node = (IfStatementSyntax)base.VisitIfStatement(node);

        if (node == null)
        {
            return null;
        }

        node = node.WithStatement(WrapWithBraces(node.Statement, statementNextToken, _context.EndOfLine));

        if (node.Else is { Statement: not null and not BlockSyntax and not IfStatementSyntax } elseClause)
        {
            node = node.WithElse(elseClause.WithStatement(WrapWithBraces(elseClause.Statement, elseNextToken, _context.EndOfLine)));
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}