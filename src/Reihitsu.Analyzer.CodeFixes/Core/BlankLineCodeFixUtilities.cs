using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for code fixes that insert blank lines or give a member lines of its own. It owns the blank line that the
/// statement blank-line fixes, the RH3208 fix, and the RH5030 fix put in front of a statement (SHARED-STMT-004,
/// SHARED-AFTERBRACE-001, SHARED-AFTERBRACE-002); the RH5011 fix still inserts its blank line below <c>break</c> with its
/// own code
/// </summary>
internal static class BlankLineCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether <see cref="TryInsertBlankLineBefore"/> can give the statement that starts with the given token a
    /// column. It cannot when the statement shares its line with the token before it, so it would be moved, it sits inside
    /// an object initializer or anonymous object, and the line on which its preceding sibling starts is started by neither
    /// a statement nor a label of its list, or is a label's continuation line with no other statement of its own section
    /// starting its line: the text then holds no column of the list (SHARED-STMT-004, SHARED-AFTERBRACE-002), and a caller
    /// withholds its fix when it registers
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
    /// below one blank line, indented like the statements of its block or switch section. Where the text holds no such
    /// column (<see cref="CanInsertBlankLineBefore"/>), nothing is inserted and no column is guessed
    /// </summary>
    /// <param name="root">Syntax root of a document that contains <paramref name="token"/></param>
    /// <param name="token">First token of the statement that should be preceded by a blank line</param>
    /// <param name="behindLeadingDirectives">
    /// Whether the inserted line break goes directly after the last preprocessor directive in the leading trivia
    /// (SHARED-AFTERBRACE-001) rather than at its start
    /// </param>
    /// <param name="updatedRoot">The updated syntax root, or <paramref name="root"/> unchanged when nothing is inserted</param>
    /// <returns><see langword="false"/> if the statement would be moved and the text holds no column for it</returns>
    public static bool TryInsertBlankLineBefore(SyntaxNode root, SyntaxToken token, bool behindLeadingDirectives, out SyntaxNode updatedRoot)
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

            updatedRoot = root.ReplaceToken(token, token.WithLeadingTrivia(newLeadingTrivia));

            return true;
        }

        if (TryGetIndentation(root, token, previousToken, out var indentation) == false)
        {
            updatedRoot = root;

            return false;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(TrimTrailingWhitespace(previousToken.TrailingTrivia));
        var updatedToken = token.WithLeadingTrivia(CreateBlankLineLeadingTrivia(token, indentation, endOfLine));

        updatedRoot = root.ReplaceTokens([previousToken, token],
                                         (originalToken, _) => originalToken == previousToken
                                                                   ? newPreviousToken
                                                                   : updatedToken);

        return true;
    }

    /// <summary>
    /// Determines whether a member shares a line with a sibling member: one of two adjacent tokens on the member's first or
    /// last line belongs to another member of the same list, and the gap between them holds nothing but whitespace
    /// </summary>
    /// <param name="member">Member</param>
    /// <param name="left">Token in front of the gap</param>
    /// <param name="right">Token behind the gap</param>
    /// <returns><see langword="true"/> if the gap separates the member from a sibling on one line</returns>
    public static bool SharesLineWithSiblingMember(MemberDeclarationSyntax member, SyntaxToken left, SyntaxToken right)
    {
        var siblingToken = left.SpanStart < member.SpanStart ? left : right;

        return siblingToken.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>(obj => obj.Parent == member.Parent) is { } sibling
               && sibling != member
               && left.TrailingTrivia.Concat(right.LeadingTrivia).All(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
    }

    /// <summary>
    /// Formats a member through the formatter after giving it lines of its own (SHARED-FIX-007, shared line): a whitespace
    /// gap to a sibling member on the member's first or last line becomes a line break, and the code behind that line
    /// break starts in the column of the line on which the member starts. That column is a member column only when the
    /// line begins with a member of the same list, the member itself or one in front of it: the line's first token starts
    /// the line behind nothing but whitespace and comments whose text starts on that line, and it is the first token of
    /// such a member. On any other line, such as one that begins with the header of a type written on one line, the
    /// continuation of a sibling's initializer, a statement of a sibling's body, the tail of a multi-line literal, or the
    /// tail of a comment that starts on an earlier line, no gap is broken and only the member is formatted (RH5417-003,
    /// RH3202-002)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    public static async Task<Document> FormatMemberOnLinesOfItsOwnAsync(Document document, MemberDeclarationSyntax member, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var endOfLine = SyntaxFactory.EndOfLine(ReihitsuFormatterHelpers.DetectEndOfLine(root));
        var memberLine = sourceText.Lines.GetLineFromPosition(member.SpanStart);
        var indentation = SyntaxFactory.Whitespace(GetLineIndentation(sourceText, memberLine));
        var lineStartToken = GetLineStartToken(root, memberLine);
        var lineBeginsWithMember = StartsLineBehindComments(sourceText, lineStartToken)
                                   && lineStartToken.Parent?.AncestorsAndSelf()
                                                           .OfType<MemberDeclarationSyntax>()
                                                           .TakeWhile(obj => obj.GetFirstToken() == lineStartToken)
                                                           .Any(obj => obj.Parent == member.Parent) == true;
        var firstToken = member.GetFirstToken();
        var lastToken = member.GetLastToken();
        var previousToken = firstToken.GetPreviousToken();
        var nextToken = lastToken.GetNextToken();
        var breaksBefore = lineBeginsWithMember && SharesLineWithSiblingMember(member, previousToken, firstToken);
        var breaksAfter = lineBeginsWithMember && SharesLineWithSiblingMember(member, lastToken, nextToken);
        var newMember = member.ReplaceTokens([firstToken, lastToken],
                                             (original, rewritten) =>
                                             {
                                                 if (original == firstToken && breaksBefore)
                                                 {
                                                     rewritten = rewritten.WithLeadingTrivia(indentation);
                                                 }

                                                 return original == lastToken && breaksAfter ? rewritten.WithTrailingTrivia(endOfLine) : rewritten;
                                             });
        var annotation = new SyntaxAnnotation();
        var newDocument = document.WithSyntaxRoot(root.ReplaceSyntax([member],
                                                                     (_, _) => newMember.WithAdditionalAnnotations(annotation),
                                                                     new[] { breaksBefore ? previousToken : default, breaksAfter ? nextToken : default }.Where(obj => obj.RawKind != 0),
                                                                     (original, _) => original == previousToken ? original.WithTrailingTrivia(endOfLine) : original.WithLeadingTrivia(indentation),
                                                                     [],
                                                                     (_, rewritten) => rewritten));
        var node = (await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))?.GetAnnotatedNodes(annotation).First();

        return node == null
                   ? document
                   : (await ReihitsuFormatter.FormatNodeInDocumentAsync(newDocument, node, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Builds the target token's leading trivia after moving it onto a new line below a blank line
    /// </summary>
    /// <param name="token">Target token</param>
    /// <param name="indentation">Indentation of the target's new line</param>
    /// <param name="endOfLine">Line-ending sequence</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList CreateBlankLineLeadingTrivia(SyntaxToken token, string indentation, string endOfLine)
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
    /// embedded statement, one level deeper than the sibling whose column the target must actually match. Only a line that
    /// a statement or a label of the target's list starts holds that column (see <see cref="TryGetListIndentation"/>)
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="token">Target token</param>
    /// <param name="previousToken">Token before the target</param>
    /// <param name="indentation">
    /// Indentation to apply to the target after moving it to its own line; <see langword="null"/> when the text holds no
    /// column of the target's statement list
    /// </param>
    /// <returns><see langword="false"/> if the text holds no column of the target's statement list</returns>
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
            var anchorLine = sourceText.Lines.GetLineFromPosition(previousStatement?.SpanStart ?? previousToken.SpanStart);
            var lineStartToken = GetLineStartToken(root, anchorLine);

            return TryGetListIndentation(sourceText, targetStatement, lineStartToken, anchorLine, out indentation);
        }

        indentation = new string(' ', SyntaxIndentationUtilities.ComputeStatementIndentLevel(targetStatement) * SyntaxIndentationUtilities.IndentSize);

        return true;
    }

    /// <summary>
    /// Gets the column, as written, of the statements of the target's block or switch section inside an object initializer
    /// or anonymous object, from what starts the anchor line (SHARED-STMT-004). When the first token of a statement of the
    /// target's block, or of any section of the same switch, starts the line, that line's column is used; when the first
    /// token of a label of the same switch starts it, the column one level deeper
    /// (<see cref="SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation"/>). When the line is a continuation line of
    /// such a label, the column is read from another statement of the target's own section that starts its line, such as
    /// the section's <c>break;</c>. Anything else - the tail of a comment that starts on an earlier line, a wrapped
    /// continuation, a nested statement, a closing brace - is that construct's layout: the text then holds no column of
    /// the list, and the method gives none
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="targetStatement">Statement being moved</param>
    /// <param name="lineStartToken">First token that starts on <paramref name="anchorLine"/> or behind it</param>
    /// <param name="anchorLine">Line on which the target's preceding sibling starts</param>
    /// <param name="indentation">Indentation to apply to the target; <see langword="null"/> when the text holds no column of the list</param>
    /// <returns><see langword="false"/> if the text holds no column of the list</returns>
    private static bool TryGetListIndentation(SourceText sourceText, StatementSyntax targetStatement, SyntaxToken lineStartToken, TextLine anchorLine, out string indentation)
    {
        var switchStatement = (targetStatement.Parent as SwitchSectionSyntax)?.Parent as SwitchStatementSyntax;
        var label = lineStartToken.Parent?.FirstAncestorOrSelf<SwitchLabelSyntax>();
        var isLabelOfSwitch = switchStatement != null && label?.Parent?.Parent == switchStatement;

        indentation = null;

        if (StartsLineBehindComments(sourceText, lineStartToken) == false)
        {
            return false;
        }

        if (isLabelOfSwitch
            && label?.GetFirstToken() != lineStartToken)
        {
            var lineStartingStatement = ((SwitchSectionSyntax)targetStatement.Parent).Statements.FirstOrDefault(statement => statement != targetStatement
                                                                                                                             && StartsLineBehindComments(sourceText, statement.GetFirstToken()));

            if (lineStartingStatement != null)
            {
                indentation = GetLineIndentation(sourceText, sourceText.Lines.GetLineFromPosition(lineStartingStatement.SpanStart));
            }

            return indentation != null;
        }

        var startsListStatement = lineStartToken.Parent?.AncestorsAndSelf()
                                                       .OfType<StatementSyntax>()
                                                       .TakeWhile(statement => statement.GetFirstToken() == lineStartToken)
                                                       .Any(statement => statement.Parent == targetStatement.Parent
                                                                         || (switchStatement != null && statement.Parent?.Parent == switchStatement)) == true;

        if (startsListStatement || isLabelOfSwitch)
        {
            indentation = SyntaxIndentationUtilities.ComputeAnchorDerivedIndentation(targetStatement, anchorLine, GetLineIndentation(sourceText, anchorLine));
        }

        return indentation != null;
    }

    /// <summary>
    /// Determines whether a token starts its line in the sense of SHARED-STMT-004: no part of an earlier token reaches into
    /// the line, and nothing stands in front of the token on that line but whitespace and comments whose text starts on that
    /// same line. The tail of a comment that starts on an earlier line, or any other trivia, means it does not
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="token">Token to check</param>
    /// <returns><see langword="true"/> if the token starts its line</returns>
    private static bool StartsLineBehindComments(SourceText sourceText, SyntaxToken token)
    {
        var lineStart = sourceText.Lines.GetLineFromPosition(token.SpanStart).Start;
        var previousToken = token.GetPreviousToken();

        return previousToken.Span.End <= lineStart
               && previousToken.TrailingTrivia.Concat(token.LeadingTrivia)
                                              .Where(trivia => trivia.Span.End > lineStart && trivia.SpanStart < token.SpanStart)
                                              .All(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                                                             || (SyntaxTriviaUtilities.IsCommentTrivia(trivia) && trivia.SpanStart >= lineStart));
    }

    /// <summary>
    /// Gets the first token that starts on a line or behind it; a token that starts on an earlier line and reaches into the
    /// line, such as a multi-line literal, is skipped
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="line">Line</param>
    /// <returns>The first token that starts at or behind the start of <paramref name="line"/></returns>
    private static SyntaxToken GetLineStartToken(SyntaxNode root, TextLine line)
    {
        var lineStartToken = root.FindToken(line.Start);

        return lineStartToken.SpanStart < line.Start
                   ? lineStartToken.GetNextToken()
                   : lineStartToken;
    }

    /// <summary>
    /// Gets the leading whitespace of a line
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>The whitespace in front of the line's first character that is not whitespace</returns>
    private static string GetLineIndentation(SourceText sourceText, TextLine line)
    {
        return FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, line));
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