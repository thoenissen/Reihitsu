using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;

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
    /// <param name="nextToken">The token that follows the statement in the original tree, whose leading trivia decides whether the closing brace needs a line break behind it</param>
    /// <param name="endOfLine">The end-of-line sequence of a separator line break</param>
    /// <returns>The braced statement</returns>
    private static StatementSyntax WrapWithBraces(StatementSyntax statement,
                                                  SyntaxToken nextToken,
                                                  string endOfLine)
    {
        if (statement is null or BlockSyntax)
        {
            return statement;
        }

        // A preprocessor directive or disabled text in the statement's trivia would end up between the statement and a
        // brace on a line the directive does not own, silently changing which code compiles under which configuration, so
        // leave such a statement unbraced instead.
        if (statement.GetLeadingTrivia().Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia)
            || statement.GetTrailingTrivia().Any(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia))
        {
            return statement;
        }

        var leadingTrivia = GetCarriedLeadingTrivia(statement.GetLeadingTrivia());
        var normalizedStatement = statement.WithLeadingTrivia(leadingTrivia)
                                           .WithTrailingTrivia(GetCarriedTrailingTrivia(statement.GetTrailingTrivia(), endOfLine));

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
    /// Builds the leading trivia of a statement that is being wrapped in braces. Everything from the line the first comment
    /// stands on is kept as written; the whitespace and line breaks in front of that line, and the whole trivia when it holds
    /// no comment, are dropped, because the later phases lay out the statement
    /// </summary>
    /// <param name="leadingTrivia">The original leading trivia of the statement</param>
    /// <returns>The carried leading trivia</returns>
    private static SyntaxTriviaList GetCarriedLeadingTrivia(SyntaxTriviaList leadingTrivia)
    {
        var firstCommentIndex = -1;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (SyntaxTriviaUtilities.IsCommentTrivia(leadingTrivia[triviaIndex]))
            {
                firstCommentIndex = triviaIndex;

                break;
            }
        }

        if (firstCommentIndex < 0)
        {
            return default;
        }

        var lineStartIndex = firstCommentIndex;

        while (lineStartIndex > 0
               && SyntaxTriviaUtilities.EndsLine(leadingTrivia[lineStartIndex - 1]) == false)
        {
            lineStartIndex--;
        }

        return SyntaxFactory.TriviaList(leadingTrivia.Skip(lineStartIndex));
    }

    /// <summary>
    /// Builds the trailing trivia of a statement that is being wrapped in braces. The trivia is kept as written, its line
    /// break included, so the inserted closing brace starts a new line exactly when the author ended the statement's line.
    /// A single-line comment that ends the trivia without a line break, at the end of the file, gets one, because the
    /// closing brace behind it would otherwise be part of the comment
    /// </summary>
    /// <param name="trailingTrivia">The original trailing trivia of the statement</param>
    /// <param name="endOfLine">The end-of-line sequence of the separator line break</param>
    /// <returns>The carried trailing trivia</returns>
    private static SyntaxTriviaList GetCarriedTrailingTrivia(SyntaxTriviaList trailingTrivia, string endOfLine)
    {
        var lastTrivia = trailingTrivia.LastOrDefault(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false);

        return lastTrivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                   ? trailingTrivia.Add(SyntaxFactory.EndOfLine(endOfLine))
                   : trailingTrivia;
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

        // The token behind each body is read from the original tree, because the visited node is detached from it
        var statementNextToken = node.Statement.GetLastToken().GetNextToken(includeZeroWidth: true);
        var elseNextToken = node.Else?.Statement.GetLastToken().GetNextToken(includeZeroWidth: true) ?? default;

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