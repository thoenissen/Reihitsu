using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// Shared helpers for code fixes that insert blank lines
/// </summary>
internal static class BlankLineCodeFixUtilities
{
    #region Methods

    /// <summary>
    /// Inserts a blank line in front of the given token
    /// </summary>
    /// <param name="root">Syntax root that contains <paramref name="token"/></param>
    /// <param name="token">Token that should be preceded by a blank line</param>
    /// <returns>The updated syntax root</returns>
    /// <remarks>
    /// The insertion point is placed after any leading directive rather than at trivia index 0, which
    /// would otherwise land the blank line inside the conditional/region block the directive opens or
    /// closes
    /// </remarks>
    public static SyntaxNode InsertBlankLineBefore(SyntaxNode root, SyntaxToken token)
    {
        var endOfLine = ReihitsuFormatterHelpers.DetectEndOfLine(root);
        var leadingTrivia = token.LeadingTrivia;
        var insertIndex = SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(leadingTrivia);
        var newLeadingTrivia = leadingTrivia.Insert(insertIndex, SyntaxFactory.EndOfLine(endOfLine));

        return root.ReplaceToken(token, token.WithLeadingTrivia(newLeadingTrivia));
    }

    #endregion // Methods
}