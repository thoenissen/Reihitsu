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

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for code fixes that give the code they change lines of its own, such as a block of a control-flow
/// statement they wrap in braces or break up, or a member that shared its line with another member, and leave the code
/// around it as written (SHARED-FIX-006, SHARED-FIX-007)
/// </summary>
internal static class LineLayoutCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Replaces a statement with a block whose braces start their own lines in the column of the line on which the
    /// statement's owner starts, and lets the formatter lay out the block's content (SHARED-FIX-006, SHARED-FIX-007). The
    /// gap in front of the opening brace loses only its whitespace at the end of the line; a clause of the same statement
    /// that followed the block on its line, such as <c>else</c> or the <c>while</c> of a <c>do</c> statement, moves to the
    /// next line in the same column. Nothing else outside the block changes
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="statement">The statement to replace; its parent owns it as a body</param>
    /// <param name="block">The block that replaces it</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    internal static async Task<Document> ReplaceWithBlockOnOwnLinesAsync(Document document, StatementSyntax statement, BlockSyntax block, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || statement.Parent == null)
        {
            return document;
        }

        var endOfLine = SyntaxFactory.EndOfLine(LineEndingUtilities.DetectEndOfLine(root));
        var indentation = SyntaxFactory.Whitespace(GetLineIndentation(sourceText, statement.Parent.SpanStart));
        var previousToken = statement.GetFirstToken().GetPreviousToken();
        var nextToken = statement.GetLastToken().GetNextToken();
        var movesNextToken = IsFollowingClause(nextToken, statement.Parent)
                             && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(statement.GetLastToken(), nextToken) == false;
        var annotation = new SyntaxAnnotation();
        var closeBraceTrailingTrivia = movesNextToken
                                           ? SyntaxFactory.TriviaList(endOfLine)
                                           : block.CloseBraceToken.TrailingTrivia;
        var newBlock = block.WithOpenBraceToken(block.OpenBraceToken.WithLeadingTrivia(ReplaceWhitespace(block.OpenBraceToken.LeadingTrivia, indentation)))
                            .WithCloseBraceToken(block.CloseBraceToken.WithLeadingTrivia(ReplaceWhitespace(block.CloseBraceToken.LeadingTrivia, indentation))
                                                                      .WithTrailingTrivia(closeBraceTrailingTrivia))
                            .WithAdditionalAnnotations(annotation);

        var newRoot = root.ReplaceSyntax([statement],
                                         (_, _) => newBlock,
                                         movesNextToken ? [previousToken, nextToken] : [previousToken],
                                         (original, _) => original == previousToken
                                                              ? original.WithTrailingTrivia(EndWithLineBreak(original.TrailingTrivia, endOfLine))
                                                              : original.WithLeadingTrivia(original.LeadingTrivia.Insert(0, indentation)),
                                         [],
                                         (_, rewritten) => rewritten);

        return await FormatAnnotatedNodeAsync(document, newRoot, annotation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Formats a member declaration through the formatter after separating it from a sibling member that shares its first
    /// or its last line, so that the member and that sibling stand on lines of their own (SHARED-FIX-007, shared line)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">Member</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    internal static async Task<Document> FormatMemberOnLinesOfItsOwnAsync(Document document, MemberDeclarationSyntax member, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var annotation = new SyntaxAnnotation();

        return await FormatAnnotatedNodeAsync(document, SeparateFromSiblingMembers(root, sourceText, member, annotation), annotation, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether a member starts on the line on which a sibling member in front of it ends, with nothing but
    /// whitespace between them
    /// </summary>
    /// <param name="member">Member</param>
    /// <returns><see langword="true"/> if the member shares its first line with the sibling in front of it</returns>
    internal static bool SharesLineWithPreviousSiblingMember(MemberDeclarationSyntax member)
    {
        var firstToken = member.GetFirstToken();
        var previousToken = firstToken.GetPreviousToken();

        return IsSiblingMemberBoundary(previousToken, member, isLastTokenOfSibling: true)
               && IsWhitespaceGap(previousToken, firstToken);
    }

    /// <summary>
    /// Removes the whitespace at the end of a trivia list and ends it with a line break, unless it already ends with one
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <param name="endOfLine">Line break to append</param>
    /// <returns>The trivia list ending with a line break</returns>
    internal static SyntaxTriviaList EndWithLineBreak(SyntaxTriviaList trivia, SyntaxTrivia endOfLine)
    {
        var count = trivia.Count;

        while (count > 0
               && trivia[count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            count--;
        }

        var result = SyntaxFactory.TriviaList(trivia.Take(count));

        return count > 0 && trivia[count - 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   ? result
                   : result.Add(endOfLine);
    }

    /// <summary>
    /// Replaces the document's root and formats the node that carries the annotation in it
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="newRoot">New syntax root</param>
    /// <param name="annotation">Annotation of the node to format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document, or the original one when the node is not found</returns>
    private static async Task<Document> FormatAnnotatedNodeAsync(Document document, SyntaxNode newRoot, SyntaxAnnotation annotation, CancellationToken cancellationToken)
    {
        var newDocument = document.WithSyntaxRoot(newRoot);
        var node = (await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))?.GetAnnotatedNodes(annotation).FirstOrDefault();

        if (node == null)
        {
            return document;
        }

        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(newDocument, node, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Separates a member from a sibling member that shares its first or its last line, so that the member starts and ends
    /// its own lines (SHARED-FIX-007, shared line): a gap to the sibling that holds nothing but whitespace becomes a line break,
    /// and the code behind the line break starts in the column of the line on which the member starts. The member carries
    /// the annotation in the returned root
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text of the root</param>
    /// <param name="member">Member</param>
    /// <param name="annotation">Annotation to put on the member</param>
    /// <returns>The updated root</returns>
    private static SyntaxNode SeparateFromSiblingMembers(SyntaxNode root, SourceText sourceText, MemberDeclarationSyntax member, SyntaxAnnotation annotation)
    {
        var endOfLine = SyntaxFactory.EndOfLine(LineEndingUtilities.DetectEndOfLine(root));
        var indentation = SyntaxFactory.Whitespace(GetLineIndentation(sourceText, member.SpanStart));
        var firstToken = member.GetFirstToken();
        var lastToken = member.GetLastToken();
        var previousToken = firstToken.GetPreviousToken();
        var nextToken = lastToken.GetNextToken();
        var breaksBefore = SharesLineWithPreviousSiblingMember(member);
        var breaksAfter = IsSiblingMemberBoundary(nextToken, member, isLastTokenOfSibling: false)
                          && IsWhitespaceGap(lastToken, nextToken);
        var newMember = member;

        if (breaksBefore || breaksAfter)
        {
            newMember = member.ReplaceTokens([firstToken, lastToken],
                                             (original, rewritten) =>
                                             {
                                                 if (breaksBefore && original == firstToken)
                                                 {
                                                     rewritten = rewritten.WithLeadingTrivia(ReplaceWhitespace(rewritten.LeadingTrivia, indentation));
                                                 }

                                                 if (breaksAfter && original == lastToken)
                                                 {
                                                     rewritten = rewritten.WithTrailingTrivia(EndWithLineBreak(rewritten.TrailingTrivia, endOfLine));
                                                 }

                                                 return rewritten;
                                             });
        }

        var tokens = new List<SyntaxToken>();

        if (breaksBefore)
        {
            tokens.Add(previousToken);
        }

        if (breaksAfter)
        {
            tokens.Add(nextToken);
        }

        return root.ReplaceSyntax([member],
                                  (_, _) => newMember.WithAdditionalAnnotations(annotation),
                                  tokens,
                                  (original, _) => original == previousToken
                                                       ? original.WithTrailingTrivia(EndWithLineBreak(original.TrailingTrivia, endOfLine))
                                                       : original.WithLeadingTrivia(ReplaceWhitespace(original.LeadingTrivia, indentation)),
                                  [],
                                  (_, rewritten) => rewritten);
    }

    /// <summary>
    /// Determines whether the gap between two tokens on one line holds nothing but whitespace
    /// </summary>
    /// <param name="left">Token in front of the gap</param>
    /// <param name="right">Token behind the gap</param>
    /// <returns><see langword="true"/> if the gap holds only whitespace</returns>
    private static bool IsWhitespaceGap(SyntaxToken left, SyntaxToken right)
    {
        return left.TrailingTrivia.Concat(right.LeadingTrivia).All(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
    }

    /// <summary>
    /// Determines whether a token is the last or the first token of another member declared in the same list as a member
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="member">Member</param>
    /// <param name="isLastTokenOfSibling">Whether the token has to be the sibling's last token; otherwise its first token</param>
    /// <returns><see langword="true"/> if the token is that boundary of a sibling member</returns>
    private static bool IsSiblingMemberBoundary(SyntaxToken token, MemberDeclarationSyntax member, bool isLastTokenOfSibling)
    {
        var sibling = token.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>(obj => obj.Parent == member.Parent);

        return sibling != null
               && sibling != member
               && (isLastTokenOfSibling ? sibling.GetLastToken() : sibling.GetFirstToken()) == token;
    }

    /// <summary>
    /// Determines whether a token starts a clause of the statement that owns a block and follows that block, such as the
    /// <c>else</c> of an <c>if</c> statement, the <c>while</c> of a <c>do</c> statement, or a <c>catch</c> or
    /// <c>finally</c> clause
    /// </summary>
    /// <param name="token">The token behind the block</param>
    /// <param name="owner">The node that owns the block</param>
    /// <returns><see langword="true"/> if the token starts such a clause</returns>
    private static bool IsFollowingClause(SyntaxToken token, SyntaxNode owner)
    {
        return token.Parent switch
               {
                   ElseClauseSyntax elseClause => elseClause.Parent == owner,
                   DoStatementSyntax doStatement => doStatement == owner && token.IsKind(SyntaxKind.WhileKeyword),
                   CatchClauseSyntax or FinallyClauseSyntax => token.Parent.Parent == owner || token.Parent.Parent == owner.Parent,
                   _ => false,
               };
    }

    /// <summary>
    /// Replaces a trivia list that holds nothing but whitespace with the given indentation; a list that holds anything else
    /// is kept
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <param name="indentation">Indentation</param>
    /// <returns>The resulting trivia list</returns>
    private static SyntaxTriviaList ReplaceWhitespace(SyntaxTriviaList trivia, SyntaxTrivia indentation)
    {
        return trivia.All(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia))
                   ? SyntaxFactory.TriviaList(indentation)
                   : trivia;
    }

    /// <summary>
    /// Gets the whitespace in front of the first character of the line that holds a position
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="position">Position</param>
    /// <returns>The line's indentation</returns>
    private static string GetLineIndentation(SourceText sourceText, int position)
    {
        return FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines.GetLineFromPosition(position)));
    }

    #endregion // Methods
}