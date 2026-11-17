using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Enumerations;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// Builds the block body that replaces an expression body, centralizing the brace
/// construction shared by every expression-bodied member transform. The member-specific
/// decisions (statement form, which trivia carriers to pass) remain in the individual transforms
/// </summary>
internal static class ExpressionBodyToBlockConverter
{
    #region Methods

    /// <summary>
    /// Creates a block body that wraps the given expression. Comments on lines of their own in front of the arrow lead the
    /// created statement on lines of their own, in source order. When the arrow's line holds a comment in front of the arrow,
    /// no comment is moved, and the arrow's leading trivia stays with the open brace. The gap between the arrow and the
    /// expression holds no comment, directive, or disabled text, because the precheck refuses an input that holds one there
    /// (FMT-UNSUP-012)
    /// </summary>
    /// <param name="expression">The arrow expression to wrap</param>
    /// <param name="statementForm">The statement form the expression should take</param>
    /// <param name="arrowToken">The arrow token being replaced; its leading trivia moves to the open brace unless it consists of comments on lines of their own, which lead the created statement instead</param>
    /// <param name="semicolonToken">The semicolon token being replaced; its trailing trivia moves to the close brace and any comment in its leading trivia moves in front of the created statement's semicolon</param>
    /// <returns>The block body</returns>
    internal static BlockSyntax CreateBlock(ExpressionSyntax expression,
                                            ExpressionBodyStatementForm statementForm,
                                            SyntaxToken arrowToken,
                                            SyntaxToken semicolonToken)
    {
        var openBraceLeadingTrivia = arrowToken.LeadingTrivia;
        var statement = CreateStatement(expression, statementForm, semicolonToken);

        if (ArrowLineHoldsComment(arrowToken) == false
            && TrySplitOwnLineComments(arrowToken.LeadingTrivia, out var commentLinesTrivia, out var arrowLineTrivia)
            && commentLinesTrivia.Count > 0
            && arrowLineTrivia.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            openBraceLeadingTrivia = default;

            // The comment lines start behind a line break that ends the open brace's line
            commentLinesTrivia.Insert(0, commentLinesTrivia.First(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)));

            statement = statement.WithLeadingTrivia(SyntaxFactory.TriviaList(commentLinesTrivia).AddRange(statement.GetLeadingTrivia()));
        }

        // The created tokens are built with explicit trivia, so they carry no elastic trivia (FMT-STRUCT-026)
        var openBrace = SyntaxFactory.Token(openBraceLeadingTrivia, SyntaxKind.OpenBraceToken, default);

        return SyntaxFactory.Block(openBrace,
                                   SyntaxFactory.SingletonList(statement),
                                   SyntaxFactory.Token(default, SyntaxKind.CloseBraceToken, semicolonToken.TrailingTrivia));
    }

    /// <summary>
    /// Determines whether the arrow's line holds a comment in front of the arrow, behind the last line break of its leading
    /// trivia
    /// </summary>
    /// <param name="arrowToken">The arrow token</param>
    /// <returns><see langword="true"/> if the arrow's line holds a comment in front of the arrow; otherwise, <see langword="false"/></returns>
    private static bool ArrowLineHoldsComment(SyntaxToken arrowToken)
    {
        for (var triviaIndex = arrowToken.LeadingTrivia.Count - 1; triviaIndex >= 0; triviaIndex--)
        {
            var trivia = arrowToken.LeadingTrivia[triviaIndex];

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia)
                || SyntaxTriviaUtilities.EndsWithLineBreak(trivia))
            {
                return false;
            }

            if (SyntaxTriviaUtilities.IsCommentTrivia(trivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Splits a leading trivia list into the comments that stand on lines of their own and the trivia of the last line,
    /// the one that holds the token the list leads. A line counts as a comment line when it holds nothing but whitespace
    /// and ordinary comments and ends with a line break; a line holding only whitespace is a blank line. The comment
    /// lines come back with the whitespace the author wrote in front of and between their comments (FMT-STRUCT-026), each
    /// ended by its line break, with the blank lines between two of them; the whitespace behind a line's last comment, and
    /// blank lines in front of the first and behind the last comment line, are dropped
    /// </summary>
    /// <param name="leadingTrivia">The leading trivia to split</param>
    /// <param name="commentLinesTrivia">The comment lines, in source order</param>
    /// <param name="lastLineTrivia">The trivia of the last line, in front of the token</param>
    /// <returns><see langword="true"/> if the list holds nothing but whitespace, line breaks, and ordinary comments; otherwise, <see langword="false"/></returns>
    private static bool TrySplitOwnLineComments(SyntaxTriviaList leadingTrivia,
                                                out List<SyntaxTrivia> commentLinesTrivia,
                                                out SyntaxTriviaList lastLineTrivia)
    {
        commentLinesTrivia = [];
        lastLineTrivia = default;

        var lineTrivia = new List<SyntaxTrivia>();
        var pendingBlankLines = new List<SyntaxTrivia>();

        foreach (var trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                var firstContentIndex = lineTrivia.FindIndex(static lineTriviaItem => lineTriviaItem.IsKind(SyntaxKind.WhitespaceTrivia) == false);

                if (firstContentIndex < 0)
                {
                    pendingBlankLines.Add(trivia);
                }
                else
                {
                    var lastContentIndex = lineTrivia.FindLastIndex(static lineTriviaItem => lineTriviaItem.IsKind(SyntaxKind.WhitespaceTrivia) == false);

                    if (commentLinesTrivia.Count > 0)
                    {
                        commentLinesTrivia.AddRange(pendingBlankLines);
                    }

                    commentLinesTrivia.AddRange(lineTrivia.GetRange(0, lastContentIndex + 1));
                    commentLinesTrivia.Add(trivia);
                    pendingBlankLines.Clear();
                }

                lineTrivia.Clear();

                continue;
            }

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false)
            {
                commentLinesTrivia.Clear();

                return false;
            }

            lineTrivia.Add(trivia);
        }

        lastLineTrivia = SyntaxFactory.TriviaList(lineTrivia);

        return true;
    }

    /// <summary>
    /// Wraps the expression in the requested statement form. A <see cref="ThrowExpressionSyntax"/>
    /// always becomes a <see cref="ThrowStatementSyntax"/> regardless of the requested form, because
    /// <c>return throw ...;</c> does not compile (CS8115). Any comment or directive in the semicolon
    /// token's leading trivia that would otherwise be lost with the discarded token is carried in
    /// front of the newly created semicolon token instead. A <c>return</c> statement drops the
    /// expression's leading indentation when no line break is left in front of the expression, so an
    /// expression written on the line after the arrow joins the keyword exactly like one written on the
    /// arrow's line
    /// </summary>
    /// <param name="expression">The expression to wrap</param>
    /// <param name="statementForm">The statement form</param>
    /// <param name="semicolonToken">The semicolon token being replaced</param>
    /// <returns>The wrapping statement</returns>
    private static StatementSyntax CreateStatement(ExpressionSyntax expression,
                                                   ExpressionBodyStatementForm statementForm,
                                                   SyntaxToken semicolonToken)
    {
        var newSemicolonToken = SyntaxFactory.Token(ExtractSignificantTrivia(semicolonToken.LeadingTrivia), SyntaxKind.SemicolonToken, default);

        if (expression is ThrowExpressionSyntax throwExpression)
        {
            return SyntaxFactory.ThrowStatement(throwExpression.ThrowKeyword, throwExpression.Expression, newSemicolonToken);
        }

        if (statementForm == ExpressionBodyStatementForm.ExpressionStatement)
        {
            return SyntaxFactory.ExpressionStatement(expression, newSemicolonToken);
        }

        return CreateReturnStatement(expression, newSemicolonToken);
    }

    /// <summary>
    /// Wraps the expression in a <c>return</c> statement that the expression joins on the keyword's line. When the
    /// expression's leading trivia holds no line break, it is the indentation of an expression that started its own line
    /// after the arrow: the line break itself belonged to the discarded arrow token, and the indentation is dropped. Leading
    /// trivia that carries a line break is kept, because the layout it describes is then the author's. Otherwise the keyword
    /// gets one space behind it only when nothing separates it from the expression and the two would read as one token, as
    /// the lexer decides for <c>return42</c>
    /// </summary>
    /// <param name="expression">The expression to wrap</param>
    /// <param name="semicolonToken">The semicolon token of the created statement</param>
    /// <returns>The <c>return</c> statement</returns>
    private static ReturnStatementSyntax CreateReturnStatement(ExpressionSyntax expression, SyntaxToken semicolonToken)
    {
        var returnKeyword = SyntaxFactory.Token(default, SyntaxKind.ReturnKeyword, default);
        var leadingTrivia = expression.GetLeadingTrivia();

        if (leadingTrivia.Count > 0
            && leadingTrivia.All(static trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            expression = expression.WithLeadingTrivia(default(SyntaxTriviaList));
        }

        if (expression.GetLeadingTrivia().FullSpan.Length == 0
            && TokenSeparationUtilities.WouldMergeIntoOneToken(returnKeyword, expression.GetFirstToken()))
        {
            returnKeyword = returnKeyword.WithTrailingTrivia(SyntaxFactory.Space);
        }

        return SyntaxFactory.ReturnStatement(returnKeyword, expression, semicolonToken);
    }

    /// <summary>
    /// Extracts the comment or directive trivia worth preserving from a trivia list that would
    /// otherwise be discarded outright. A list carrying nothing but whitespace and line breaks
    /// collapses to an empty list, unchanged from today's behavior. A list carrying a comment or
    /// directive keeps that trivia together with whatever separates it from the far end (such as the
    /// end-of-line that terminates a single-line comment), with its own leading run of whitespace
    /// stripped so it does not leave a stray space when relocated
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The significant trivia to carry over, or an empty list when nothing but whitespace was present</returns>
    private static SyntaxTriviaList ExtractSignificantTrivia(SyntaxTriviaList triviaList)
    {
        var hasSignificantTrivia = triviaList.Any(static trivia => SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                                                                   || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));

        if (hasSignificantTrivia == false)
        {
            return default;
        }

        var significantTrivia = new List<SyntaxTrivia>();
        var skippingLeadingWhitespace = true;

        foreach (var trivia in triviaList)
        {
            if (skippingLeadingWhitespace && trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            skippingLeadingWhitespace = false;
            significantTrivia.Add(trivia);
        }

        return TriviaListUtilities.RemoveTrailingWhitespace(SyntaxFactory.TriviaList(significantTrivia));
    }

    #endregion // Methods
}