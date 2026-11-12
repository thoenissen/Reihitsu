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
/// Shared helpers for code fixes that insert blank lines. It owns the blank line that the statement blank-line fixes, the
/// RH3208 fix, and the RH5030 fix put in front of a statement (SHARED-STMT-004, SHARED-AFTERBRACE-001,
/// SHARED-AFTERBRACE-002); the RH5011 fix still inserts its blank line below <c>break</c> with its own code
/// </summary>
internal static class BlankLineCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether <see cref="InsertBlankLineBefore"/> can give the statement that starts with the given token a
    /// column. It cannot when the statement shares its line with the token before it, so it would be moved, and it sits in a
    /// switch section inside an object initializer or anonymous object whose statements stand only behind a <c>case</c>
    /// label's continuation line: the text then holds no column of the section (SHARED-STMT-004), and the fix is withheld
    /// </summary>
    /// <param name="root">Syntax root of a document that contains <paramref name="token"/></param>
    /// <param name="token">First token of the statement that should be preceded by a blank line</param>
    /// <returns><see langword="true"/> if the blank line can be inserted</returns>
    public static bool CanInsertBlankLineBefore(SyntaxNode root, SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        return previousToken.IsKind(SyntaxKind.None)
               || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token)
               || TryGetIndentation(root, token, previousToken, out _);
    }

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

        // A caller that withholds its fix where the text holds no column of the section checks CanInsertBlankLineBefore
        // first; any other caller gets the continuation line's whitespace in that case
        TryGetIndentation(root, token, previousToken, out var indentation);

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
    /// (see <see cref="TryGetSwitchSectionIndentation"/>)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="token">Target token</param>
    /// <param name="previousToken">Token before the target</param>
    /// <param name="indentation">
    /// Indentation to apply to the target after moving it to its own line; when the text holds no column of the target's
    /// switch section, the leading whitespace of the label's continuation line
    /// </param>
    /// <returns><see langword="false"/> if the text holds no column of the target's switch section</returns>
    private static bool TryGetIndentation(SyntaxNode root, SyntaxToken token, SyntaxToken previousToken, out string indentation)
    {
        var targetStatement = token.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        if (targetStatement == null)
        {
            indentation = string.Empty;

            return true;
        }

        if (SyntaxIndentationUtilities.HasAnchorScopeAncestor(targetStatement))
        {
            var sourceText = root.SyntaxTree.GetText();
            var previousStatement = GetPrecedingSiblingStatement(targetStatement);
            var anchorPosition = previousStatement?.SpanStart ?? previousToken.SpanStart;
            var previousLine = sourceText.Lines.GetLineFromPosition(anchorPosition);
            var anchorLineIndentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, previousLine));

            if (targetStatement.Parent is SwitchSectionSyntax switchSection)
            {
                return TryGetSwitchSectionIndentation(sourceText, targetStatement, switchSection, previousLine, anchorLineIndentation, out indentation);
            }

            indentation = anchorLineIndentation;

            return true;
        }

        indentation = new string(' ', SyntaxIndentationUtilities.ComputeStatementIndentLevel(targetStatement) * SyntaxIndentationUtilities.IndentSize);

        return true;
    }

    /// <summary>
    /// Gets the column, as written, of the statements of a switch section inside an object initializer or anonymous object.
    /// When the anchor line starts with a label of the enclosing switch statement - the target's own section's label, an
    /// earlier sibling section's label sharing the physical line, or trivia attached ahead of either - that line's
    /// whitespace is the label's column, one level shallower than the section's statements
    /// (<see cref="SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation"/>). When the anchor line is the continuation
    /// line of a label that spans several lines, its whitespace is neither the label's nor the section's column, so the
    /// column is read from another statement of the section that starts its own line, such as the section's
    /// <c>break;</c>. Without such a statement, the text holds no column of the section: the method returns
    /// <see langword="false"/> and gives the continuation line's whitespace unchanged. A label that is not
    /// itself first on its own line - the whole <c>switch</c> statement written on one physical line, for example - has no
    /// whitespace run that equals its column, so that shape is left unchanged
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="targetStatement">Statement being moved</param>
    /// <param name="switchSection">Switch section that owns <paramref name="targetStatement"/></param>
    /// <param name="anchorLine">Line on which the target's preceding sibling starts</param>
    /// <param name="anchorLineIndentation">The anchor line's own leading whitespace</param>
    /// <param name="indentation">Indentation to apply to the target</param>
    /// <returns><see langword="false"/> if the text holds no column of the section</returns>
    private static bool TryGetSwitchSectionIndentation(SourceText sourceText, StatementSyntax targetStatement, SwitchSectionSyntax switchSection, TextLine anchorLine, string anchorLineIndentation, out string indentation)
    {
        var contentStart = anchorLine.Start + anchorLineIndentation.Length;

        if (IsWithinLabelContinuation(switchSection, contentStart) == false)
        {
            indentation = SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation(targetStatement, anchorLine, anchorLineIndentation);

            return true;
        }

        var lineStartingStatement = switchSection.Statements.FirstOrDefault(statement => statement != targetStatement
                                                                                         && SyntaxTokenPositionUtilities.StartsLine(statement.GetFirstToken()));

        if (lineStartingStatement == null)
        {
            indentation = anchorLineIndentation;

            return false;
        }

        var statementLine = sourceText.Lines.GetLineFromPosition(lineStartingStatement.SpanStart);

        indentation = FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, statementLine));

        return true;
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