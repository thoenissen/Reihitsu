using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Provides helpers for analyzing blank lines around <c>#region</c> and <c>#endregion</c> directives
/// </summary>
public static class RegionDirectiveBlankLineUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the directive on the specified line sits next to a line that the blank-line formatting never
    /// rewrites, such as disabled code, multi-line comments, or string literals
    /// </summary>
    /// <param name="directiveLineIndex">Zero-based line index of the directive</param>
    /// <param name="lineCount">Total number of lines in the document</param>
    /// <param name="nonFormattableLineIndices">Line indices that the blank-line formatting never rewrites</param>
    /// <returns><see langword="true"/> when the directive should be left untouched</returns>
    public static bool IsAdjacentToNonFormattableLine(int directiveLineIndex, int lineCount, ISet<int> nonFormattableLineIndices)
    {
        return (directiveLineIndex > 0 && nonFormattableLineIndices.Contains(directiveLineIndex - 1))
               || (directiveLineIndex < lineCount - 1 && nonFormattableLineIndices.Contains(directiveLineIndex + 1));
    }

    /// <summary>
    /// Determines whether a region or end region directive is missing the blank line that must precede it. This is the one
    /// statement of the rule: the line directly above the directive has to be blank, unless the directive stands on the
    /// first line or the line above ends with an opening brace. Nothing else on the line above, such as a conditional
    /// directive, and no other directive in the same gap exempts it. The analyzer and its code fix read the facts from the
    /// source text, the formatter reads them from the trivia around the directive
    /// </summary>
    /// <param name="hasLineAbove">Whether a line stands above the directive, that is whether it is not on the first line</param>
    /// <param name="isLineAboveBlank">Whether the line directly above the directive holds nothing but whitespace</param>
    /// <param name="doesLineAboveEndWithOpenBrace">
    /// Whether the last content of the line directly above the directive is an opening brace, with no comment behind it
    /// </param>
    /// <returns><see langword="true"/> when a blank line must be inserted before the directive</returns>
    public static bool IsMissingRequiredBlankLineBefore(bool hasLineAbove, bool isLineAboveBlank, bool doesLineAboveEndWithOpenBrace)
    {
        return hasLineAbove
               && isLineAboveBlank == false
               && doesLineAboveEndWithOpenBrace == false;
    }

    /// <summary>
    /// Determines whether the directive on the specified line is missing the blank line that must precede it, reading the
    /// facts of <see cref="IsMissingRequiredBlankLineBefore(bool, bool, bool)"/> from the source text
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="directiveLineIndex">Zero-based line index of the directive</param>
    /// <param name="openBraceEndLineIndices">Line indices whose last token is an opening brace</param>
    /// <returns><see langword="true"/> when a blank line must be inserted before the directive</returns>
    public static bool IsMissingRequiredBlankLineBefore(SourceText sourceText, int directiveLineIndex, ISet<int> openBraceEndLineIndices)
    {
        var hasLineAbove = directiveLineIndex > 0;

        return IsMissingRequiredBlankLineBefore(hasLineAbove,
                                                hasLineAbove && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, directiveLineIndex - 1),
                                                hasLineAbove && openBraceEndLineIndices.Contains(directiveLineIndex - 1));
    }

    /// <summary>
    /// Determines whether the directive on the specified line is missing the blank line that must follow it
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="directiveLineIndex">Zero-based line index of the directive</param>
    /// <param name="closeBraceStartLineIndices">Line indices whose first token is a closing brace</param>
    /// <returns><see langword="true"/> when a blank line must be inserted after the directive</returns>
    public static bool IsMissingRequiredBlankLineAfter(SourceText sourceText, int directiveLineIndex, ISet<int> closeBraceStartLineIndices)
    {
        return directiveLineIndex < sourceText.Lines.Count - 1
               && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, directiveLineIndex + 1) == false
               && closeBraceStartLineIndices.Contains(directiveLineIndex + 1) == false;
    }

    /// <summary>
    /// Determines whether the blank lines directly above a target line follow a region directive that requires the blank
    /// line below it (SHARED-BLANK-006), so a rule that deletes blank lines in front of the target must not report them. The
    /// nearest line above the target that is not blank has to start with an active <c>#region</c>, or with an active
    /// <c>#endregion</c> when the target line does not begin with a closing brace <c>}</c>, which exempts only
    /// <c>#endregion</c>
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="targetLineIndex">Zero-based index of the line below the blank lines</param>
    /// <returns><see langword="true"/> when the blank lines above the target follow a region directive that requires one</returns>
    public static bool FollowsRegionDirectiveRequiringBlankLine(SyntaxNode root, SourceText sourceText, int targetLineIndex)
    {
        var directiveLineIndex = FormattingTextAnalysisUtilities.FindPreviousNonBlankLineIndex(sourceText, targetLineIndex);

        if (directiveLineIndex < 0)
        {
            return false;
        }

        var directiveLine = sourceText.Lines[directiveLineIndex];
        var contentStart = directiveLine.Start + FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, directiveLine)).Length;

        switch (root.FindTrivia(contentStart).GetStructure())
        {
            case RegionDirectiveTriviaSyntax { IsActive: true }:
                {
                    return true;
                }

            case EndRegionDirectiveTriviaSyntax { IsActive: true }:
                {
                    var targetContent = FormattingTextAnalysisUtilities.GetLineText(sourceText, sourceText.Lines[targetLineIndex]).TrimStart();

                    return targetContent.Length == 0
                           || targetContent[0] != '}';
                }

            default:
                {
                    return false;
                }
        }
    }

    #endregion // Methods
}