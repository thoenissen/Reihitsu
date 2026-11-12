using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for code fixes that insert blank lines. This is the one owner of the blank line a code fix puts in front
/// of a statement (SHARED-STMT-004, SHARED-AFTERBRACE-001, SHARED-AFTERBRACE-002)
/// </summary>
internal static class BlankLineCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Inserts a blank line in front of the given token, the first token of a statement. When a line break outside any
    /// comment already separates the token from the token before it, one line break is inserted into the token's leading
    /// trivia, so every comment in front of the token stays with it below the new blank line. Otherwise the token shares
    /// a line with the token before it — directly, or behind a comment that holds every line break of the gap — and the
    /// gap is rebuilt: the whitespace at the end of the previous line is removed, and the token moves onto a new line
    /// below one blank line, indented like the statements of its block or switch section
    /// </summary>
    /// <param name="root">Syntax root of a document that contains <paramref name="token"/></param>
    /// <param name="token">First token of the statement that should be preceded by a blank line</param>
    /// <param name="behindLeadingDirectives">
    /// Whether the inserted line break goes directly after the last preprocessor directive in the leading trivia
    /// (SHARED-AFTERBRACE-001) rather than at its start
    /// </param>
    /// <returns>The updated syntax root</returns>
    public static SyntaxNode InsertBlankLineBefore(SyntaxNode root, SyntaxToken token, bool behindLeadingDirectives)
    {
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None)
            || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token))
        {
            var leadingTrivia = token.LeadingTrivia;
            var insertIndex = behindLeadingDirectives
                                  ? SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(leadingTrivia)
                                  : 0;
            var newLeadingTrivia = leadingTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(endOfLine));

            return root.ReplaceToken(token, token.WithLeadingTrivia(newLeadingTrivia));
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(TrimTrailingWhitespace(previousToken.TrailingTrivia));
        var updatedToken = token.WithLeadingTrivia(CreateBlankLineLeadingTrivia(root, token, previousToken, endOfLine));

        return root.ReplaceTokens([previousToken, token],
                                  (originalToken, _) => originalToken == previousToken
                                                            ? newPreviousToken
                                                            : updatedToken);
    }

    /// <summary>
    /// Builds the target token's leading trivia after moving it onto a new line below a blank line
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="token">Target token</param>
    /// <param name="previousToken">Token before the target</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList CreateBlankLineLeadingTrivia(SyntaxNode root, SyntaxToken token, SyntaxToken previousToken, string endOfLine)
    {
        var targetLeadingTrivia = token.LeadingTrivia;
        var suffixStart = 0;

        while (suffixStart < targetLeadingTrivia.Count && targetLeadingTrivia[suffixStart].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            suffixStart++;
        }

        var newLeadingTriviaItems = new List<SyntaxTrivia>(targetLeadingTrivia.Count - suffixStart + 3)
                                    {
                                        SyntaxFactory.EndOfLine(endOfLine),
                                        SyntaxFactory.EndOfLine(endOfLine)
                                    };
        var indentation = GetIndentation(root, token, previousToken);

        if (indentation.Length > 0)
        {
            newLeadingTriviaItems.Add(SyntaxFactory.Whitespace(indentation));
        }

        newLeadingTriviaItems.AddRange(targetLeadingTrivia.Skip(suffixStart));

        return SyntaxFactory.TriviaList(newLeadingTriviaItems);
    }

    /// <summary>
    /// Removes horizontal whitespace from the end of a trivia list
    /// </summary>
    /// <param name="trivia">Trivia to trim</param>
    /// <returns>The trimmed trivia</returns>
    private static SyntaxTriviaList TrimTrailingWhitespace(SyntaxTriviaList trivia)
    {
        while (trivia.Count > 0 && trivia[trivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            trivia = trivia.RemoveAt(trivia.Count - 1);
        }

        return trivia;
    }

    /// <summary>
    /// Gets the indentation to apply after moving the target token onto its own line. Normally this is the target
    /// statement's syntactic nesting depth, which is canonical: it self-corrects a stray extra space or tab on the previous
    /// statement's line instead of propagating it. That canonical value does not exist when an object initializer or
    /// anonymous object sits between the statement and its nearest brace scope, because neither is a level -
    /// <see cref="SyntaxIndentationUtilities.ComputeStatementIndentLevel"/> has no way to turn "one more initializer" into
    /// the anchor-derived column the formatter's own alignment contributors would place it at. In that situation the column
    /// is read from the text as written (SHARED-STMT-004), from the line on which the target's own preceding sibling in its
    /// statement list starts, since that line has not moved. The anchor has to be the target's actual preceding sibling
    /// rather than merely the innermost statement enclosing <paramref name="previousToken"/>: when the preceding sibling
    /// itself has an unbraced embedded body ("if (Check()) Baz();"), <paramref name="previousToken"/> sits inside that
    /// embedded statement, one level deeper than the sibling whose column the target must actually match. When that line is
    /// led by a label of the target's own switch statement, its whitespace is the label's column rather than the section's
    /// (see <see cref="GetSwitchSectionIndentation"/>)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="token">Target token</param>
    /// <param name="previousToken">Token before the target</param>
    /// <returns>Indentation to apply to the target after moving it to its own line</returns>
    private static string GetIndentation(SyntaxNode root, SyntaxToken token, SyntaxToken previousToken)
    {
        var targetStatement = token.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        if (targetStatement == null)
        {
            return string.Empty;
        }

        if (SyntaxIndentationUtilities.HasAnchorScopeAncestor(targetStatement))
        {
            var sourceText = root.SyntaxTree.GetText();
            var previousStatement = GetPrecedingSiblingStatement(targetStatement);
            var anchorPosition = previousStatement?.SpanStart ?? previousToken.SpanStart;
            var previousLine = sourceText.Lines.GetLineFromPosition(anchorPosition);
            var indentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, previousLine));

            if (targetStatement.Parent is SwitchSectionSyntax switchSection)
            {
                return GetSwitchSectionIndentation(sourceText, targetStatement, switchSection, previousLine, indentation);
            }

            return indentation;
        }

        return new string(' ', SyntaxIndentationUtilities.ComputeStatementIndentLevel(targetStatement) * SyntaxIndentationUtilities.IndentSize);
    }

    /// <summary>
    /// Gets the column, as written, of the statements of a switch section inside an object initializer or anonymous object.
    /// When the anchor line starts with a label of the enclosing switch statement - the target's own section's label, an
    /// earlier sibling section's label sharing the physical line, or trivia attached ahead of either - that line's
    /// whitespace is the label's column, one level shallower than the section's statements
    /// (<see cref="SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation"/>). When the anchor line is the continuation
    /// line of a label that spans several lines, its whitespace is neither the label's nor the section's column, so the
    /// column is read from another statement of the section that starts its own line, such as the section's
    /// <c>break;</c>. Without such a statement, the continuation line's whitespace is used unchanged. A label that is not
    /// itself first on its own line - the whole <c>switch</c> statement written on one physical line, for example - has no
    /// whitespace run that equals its column, so that shape is left unchanged
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="targetStatement">Statement being moved</param>
    /// <param name="switchSection">Switch section that owns <paramref name="targetStatement"/></param>
    /// <param name="anchorLine">Line on which the target's preceding sibling starts</param>
    /// <param name="anchorLineIndentation">The anchor line's own leading whitespace</param>
    /// <returns>Indentation to apply to the target</returns>
    private static string GetSwitchSectionIndentation(SourceText sourceText, StatementSyntax targetStatement, SwitchSectionSyntax switchSection, TextLine anchorLine, string anchorLineIndentation)
    {
        var contentStart = anchorLine.Start + anchorLineIndentation.Length;

        if (IsWithinLabelContinuation(switchSection, contentStart) == false)
        {
            return SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation(targetStatement, anchorLine, anchorLineIndentation);
        }

        var lineStartingStatement = switchSection.Statements.FirstOrDefault(statement => statement != targetStatement
                                                                                         && SyntaxTokenPositionUtilities.StartsLine(statement.GetFirstToken()));

        if (lineStartingStatement == null)
        {
            return anchorLineIndentation;
        }

        var statementLine = sourceText.Lines.GetLineFromPosition(lineStartingStatement.SpanStart);

        return FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, statementLine));
    }

    /// <summary>
    /// Determines whether a position lies inside a label of the enclosing switch statement behind the label's first
    /// token, on a continuation line of a label that spans several lines
    /// </summary>
    /// <param name="switchSection">Switch section whose parent is the enclosing switch statement</param>
    /// <param name="contentStart">Position of the first content of the line</param>
    /// <returns><see langword="true"/> if the position lies behind the first token of a label and inside its span</returns>
    private static bool IsWithinLabelContinuation(SwitchSectionSyntax switchSection, int contentStart)
    {
        return switchSection.Parent is SwitchStatementSyntax switchStatement
               && switchStatement.Sections.SelectMany(section => section.Labels)
                                          .Any(label => contentStart > label.SpanStart && contentStart < label.Span.End);
    }

    /// <summary>
    /// Gets the statement immediately preceding <paramref name="statement"/> in its own statement list
    /// </summary>
    /// <param name="statement">Statement whose preceding sibling should be found</param>
    /// <returns>The preceding sibling statement, or <see langword="null"/> when there is none</returns>
    private static StatementSyntax GetPrecedingSiblingStatement(StatementSyntax statement)
    {
        if (statement.Parent is BlockSyntax block)
        {
            return block.Statements.TakeWhile(currentStatement => currentStatement != statement)
                                   .LastOrDefault();
        }

        if (statement.Parent is SwitchSectionSyntax switchSection)
        {
            return switchSection.Statements.TakeWhile(currentStatement => currentStatement != statement)
                                           .LastOrDefault();
        }

        return null;
    }

    #endregion // Methods
}