using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Utilities;

/// <summary>
/// Writes a target that a node-level entry point of <see cref="ReihitsuFormatter"/> formatted back into its document: the
/// end of the target's last line, and the fields a field split makes of the target, in place of the target among the
/// members of its type
/// </summary>
internal static class TargetWriteBackUtilities
{
    #region Methods

    /// <summary>
    /// Replaces the target in the members of its type by the fields the split makes of it, with the target's trivia written
    /// back on them. A member that shares the target's first or last line is moved to a line of its own, and a later field
    /// stands in the column the phases give it (FMT-PIPE-010, FMT-PIPE-018, FMT-PIPE-029)
    /// </summary>
    /// <param name="members">The members of the type that declares the target</param>
    /// <param name="targetIndex">The index of the target among the members</param>
    /// <param name="formattedFields">The fields the split makes of the target, as the phases lay them out</param>
    /// <param name="sharesLineWithPrecedingMember">Whether the target shares its first line with the member in front</param>
    /// <param name="sharesLineWithFollowingMember">Whether the target shares its last line with the member behind</param>
    /// <param name="endOfLine">The line ending detected for the document</param>
    /// <param name="targetTokenAnnotation">The annotation that marks the target's first token</param>
    /// <param name="targetFieldAnnotation">The annotation that marks every field the split makes of the target</param>
    /// <returns>The members of the type with the fields in place of the target</returns>
    internal static List<MemberDeclarationSyntax> ReplaceTargetBySplitFields(SyntaxList<MemberDeclarationSyntax> members,
                                                                             int targetIndex,
                                                                             List<FieldDeclarationSyntax> formattedFields,
                                                                             bool sharesLineWithPrecedingMember,
                                                                             bool sharesLineWithFollowingMember,
                                                                             string endOfLine,
                                                                             SyntaxAnnotation targetTokenAnnotation,
                                                                             SyntaxAnnotation targetFieldAnnotation)
    {
        var target = members[targetIndex];
        var memberIndentation = SyntaxFactory.Whitespace(new string(' ', SyntaxTokenPositionUtilities.GetColumn(formattedFields[1].GetFirstToken())));
        var lineBreak = SyntaxFactory.EndOfLine(endOfLine);
        var fields = WriteBackSplitFields(formattedFields,
                                          target.GetFirstToken(),
                                          target.GetLastToken(),
                                          sharesLineWithPrecedingMember,
                                          sharesLineWithFollowingMember,
                                          memberIndentation,
                                          lineBreak,
                                          targetTokenAnnotation,
                                          targetFieldAnnotation);

        return ReplaceTargetByFields(members, targetIndex, fields, sharesLineWithPrecedingMember, sharesLineWithFollowingMember, memberIndentation, lineBreak);
    }

    /// <summary>
    /// Gets the fields the split makes of the target in a formatted copy of the context, in source order
    /// </summary>
    /// <param name="formattedContext">The formatted copy of the context</param>
    /// <param name="targetField">The field declaration the split splits</param>
    /// <param name="targetFieldAnnotation">The annotation that marks every field the split makes of the target</param>
    /// <param name="formattedFields">One field per declarator of the target</param>
    /// <returns><see langword="true"/> if the copy holds one single-declarator field per declarator of the target; otherwise, <see langword="false"/></returns>
    internal static bool TryGetSplitFields(SyntaxNode formattedContext, FieldDeclarationSyntax targetField, SyntaxAnnotation targetFieldAnnotation, out List<FieldDeclarationSyntax> formattedFields)
    {
        formattedFields = formattedContext.GetAnnotatedNodes(targetFieldAnnotation)
                                          .OfType<FieldDeclarationSyntax>()
                                          .ToList();

        return formattedFields.Count == targetField.Declaration.Variables.Count
               && formattedFields.All(field => field.Declaration.Variables.Count == 1);
    }

    /// <summary>
    /// Determines whether the phases started the line of a member's first token in a formatted copy of the context: the token
    /// starts its line, or a documentation comment in its leading trivia does (FMT-PIPE-029). The documentation-comment phase
    /// starts the line of a documentation comment written behind code, and the line-break phase starts a member's line where
    /// the gap in front of it holds no line break, so reading the result covers both without restating either
    /// </summary>
    /// <param name="firstToken">The member's first token in the formatted copy of the context</param>
    /// <returns><see langword="true"/> if the token or a documentation comment in front of it starts a line; otherwise, <see langword="false"/></returns>
    internal static bool StartsLineInLayout(SyntaxToken firstToken)
    {
        if (SyntaxTokenPositionUtilities.StartsLine(firstToken))
        {
            return true;
        }

        var leadingTrivia = firstToken.LeadingTrivia;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (SyntaxTriviaUtilities.IsDocumentationCommentTrivia(leadingTrivia[triviaIndex])
                && TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(firstToken, triviaIndex))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Ends the target's last line, when its trailing trivia ends in a line break, as the phases end every other line of the
    /// target: the line break is written in the line ending detected for the document, and the whitespace directly in front of
    /// it is removed, behind the last token or behind a delimited comment alike (FMT-PIPE-010, SHARED-FIX-008). Whitespace in
    /// front of a comment and the text of a comment stay; a trailing trivia without a line break, because code follows the
    /// target on its line, is returned as it stands
    /// </summary>
    /// <param name="trailingTrivia">The trailing trivia written back behind the target</param>
    /// <param name="endOfLine">The line ending detected for the document</param>
    /// <returns>The trailing trivia with the target's last line ended as formatted</returns>
    internal static SyntaxTriviaList EndTargetLineAsFormatted(SyntaxTriviaList trailingTrivia, string endOfLine)
    {
        if (trailingTrivia.Count == 0
            || trailingTrivia[trailingTrivia.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return trailingTrivia;
        }

        var lineBreak = trailingTrivia[trailingTrivia.Count - 1];

        if (lineBreak.ToFullString() != endOfLine)
        {
            lineBreak = SyntaxFactory.EndOfLine(endOfLine);
        }

        return TriviaListUtilities.RemoveTrailingWhitespace(trailingTrivia.RemoveAt(trailingTrivia.Count - 1)).Add(lineBreak);
    }

    /// <summary>
    /// Writes back the target's trivia on the fields the split makes of it: the first field gets the target's leading trivia
    /// as it stands in the document, behind the indentation of a member when it shares its line with the member in front;
    /// the last field gets the target's trailing trivia, ending its line with the line break when it shares its line with
    /// the member behind, and as the phases end every other line otherwise (FMT-PIPE-010, FMT-PIPE-018)
    /// </summary>
    /// <param name="formattedFields">The fields the split makes of the target, as the phases lay them out</param>
    /// <param name="originalFirstToken">The target's first token in the document</param>
    /// <param name="originalLastToken">The target's last token in the document</param>
    /// <param name="sharesLineWithPrecedingMember">Whether the target shares its first line with the member in front</param>
    /// <param name="sharesLineWithFollowingMember">Whether the target shares its last line with the member behind</param>
    /// <param name="memberIndentation">The indentation the phases give a later field</param>
    /// <param name="lineBreak">The line break of the document</param>
    /// <param name="targetTokenAnnotation">The annotation that marks the target's first token</param>
    /// <param name="targetFieldAnnotation">The annotation that marks every field the split makes of the target</param>
    /// <returns>The fields to write back, in source order</returns>
    private static List<MemberDeclarationSyntax> WriteBackSplitFields(List<FieldDeclarationSyntax> formattedFields,
                                                                      SyntaxToken originalFirstToken,
                                                                      SyntaxToken originalLastToken,
                                                                      bool sharesLineWithPrecedingMember,
                                                                      bool sharesLineWithFollowingMember,
                                                                      SyntaxTrivia memberIndentation,
                                                                      SyntaxTrivia lineBreak,
                                                                      SyntaxAnnotation targetTokenAnnotation,
                                                                      SyntaxAnnotation targetFieldAnnotation)
    {
        var fields = new List<MemberDeclarationSyntax>(formattedFields.Count);

        for (var fieldIndex = 0; fieldIndex < formattedFields.Count; fieldIndex++)
        {
            var field = formattedFields[fieldIndex].WithoutAnnotations(targetFieldAnnotation);

            if (fieldIndex == 0)
            {
                // The target's leading trivia is written back as it stands in the document (FMT-PIPE-018); behind another
                // member, the line break in front of it puts it where the phases place a member
                var leadingTrivia = sharesLineWithPrecedingMember
                                        ? SyntaxFactory.TriviaList(memberIndentation).AddRange(TrimLeadingWhitespace(originalFirstToken.LeadingTrivia))
                                        : originalFirstToken.LeadingTrivia;
                var firstToken = field.GetFirstToken();

                field = field.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(leadingTrivia).WithoutAnnotations(targetTokenAnnotation));
            }

            if (fieldIndex == formattedFields.Count - 1)
            {
                var trailingTrivia = sharesLineWithFollowingMember
                                         ? TriviaListUtilities.RemoveTrailingWhitespace(originalLastToken.TrailingTrivia).Add(lineBreak)
                                         : EndTargetLineAsFormatted(originalLastToken.TrailingTrivia, lineBreak.ToFullString());
                var lastToken = field.GetLastToken();

                field = field.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(trailingTrivia));
            }

            fields.Add(field);
        }

        return fields;
    }

    /// <summary>
    /// Replaces the target in the members of its type by the fields the split makes of it. A member that shares the
    /// target's first line ends its line behind any comment that trails it, and a member that shares the target's last
    /// line starts the next line with the indentation of a member, keeping its leading trivia
    /// </summary>
    /// <param name="members">The members of the type that declares the target</param>
    /// <param name="targetIndex">The index of the target among the members</param>
    /// <param name="fields">The fields to write back in place of the target</param>
    /// <param name="sharesLineWithPrecedingMember">Whether the target shares its first line with the member in front</param>
    /// <param name="sharesLineWithFollowingMember">Whether the target shares its last line with the member behind</param>
    /// <param name="memberIndentation">The indentation the phases give a later field</param>
    /// <param name="lineBreak">The line break of the document</param>
    /// <returns>The members of the type with the fields in place of the target</returns>
    private static List<MemberDeclarationSyntax> ReplaceTargetByFields(SyntaxList<MemberDeclarationSyntax> members,
                                                                       int targetIndex,
                                                                       List<MemberDeclarationSyntax> fields,
                                                                       bool sharesLineWithPrecedingMember,
                                                                       bool sharesLineWithFollowingMember,
                                                                       SyntaxTrivia memberIndentation,
                                                                       SyntaxTrivia lineBreak)
    {
        var updatedMembers = new List<MemberDeclarationSyntax>(members.Count + fields.Count);

        for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
        {
            var member = members[memberIndex];

            if (memberIndex == targetIndex)
            {
                updatedMembers.AddRange(fields);
            }
            else if (memberIndex == targetIndex - 1
                     && sharesLineWithPrecedingMember)
            {
                // A comment that trails the member in front stays behind it; only the whitespace behind it ends the line
                var lastToken = member.GetLastToken();

                updatedMembers.Add(member.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(TriviaListUtilities.RemoveTrailingWhitespace(lastToken.TrailingTrivia).Add(lineBreak))));
            }
            else if (memberIndex == targetIndex + 1
                     && sharesLineWithFollowingMember)
            {
                // The member behind keeps its leading trivia, a documentation comment included, and starts the next line
                var firstToken = member.GetFirstToken();

                updatedMembers.Add(member.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxFactory.TriviaList(memberIndentation).AddRange(TrimLeadingWhitespace(firstToken.LeadingTrivia)))));
            }
            else
            {
                updatedMembers.Add(member);
            }
        }

        return updatedMembers;
    }

    /// <summary>
    /// Removes the whitespace a trivia list starts with
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The trivia list without its leading whitespace</returns>
    private static SyntaxTriviaList TrimLeadingWhitespace(SyntaxTriviaList trivia)
    {
        return SyntaxFactory.TriviaList(trivia.SkipWhile(static item => item.IsKind(SyntaxKind.WhitespaceTrivia)));
    }

    #endregion // Methods
}