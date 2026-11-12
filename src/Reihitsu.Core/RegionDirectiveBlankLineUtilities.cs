using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    /// Determines whether a region or end region directive requires the line below it to be blank (SHARED-BLANK-006). This is
    /// the one statement of the closing-brace exemption: a <c>#region</c> always requires the blank line, an
    /// <c>#endregion</c> requires it unless the line below begins with a closing brace <c>}</c>. Whether the directive stands
    /// on the last line is the caller's fact
    /// </summary>
    /// <param name="isEndRegion">Whether the directive is an <c>#endregion</c></param>
    /// <param name="doesLineBelowBeginWithCloseBrace">Whether the first token of the line below the directive is a closing brace</param>
    /// <returns><see langword="true"/> when the line below the directive has to be blank</returns>
    public static bool RequiresBlankLineBelow(bool isEndRegion, bool doesLineBelowBeginWithCloseBrace)
    {
        return isEndRegion == false
               || doesLineBelowBeginWithCloseBrace == false;
    }

    /// <summary>
    /// Determines whether the directive on the specified line is missing the blank line that must follow it, applying
    /// <see cref="RequiresBlankLineBelow(bool, bool)"/> to the line directly below the directive
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="directiveLineIndex">Zero-based line index of the directive</param>
    /// <param name="isEndRegion">Whether the directive is an <c>#endregion</c></param>
    /// <param name="closeBraceStartLineIndices">Line indices whose first token is a closing brace</param>
    /// <returns><see langword="true"/> when a blank line must be inserted after the directive</returns>
    public static bool IsMissingRequiredBlankLineAfter(SourceText sourceText, int directiveLineIndex, bool isEndRegion, ISet<int> closeBraceStartLineIndices)
    {
        return directiveLineIndex < sourceText.Lines.Count - 1
               && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, directiveLineIndex + 1) == false
               && RequiresBlankLineBelow(isEndRegion, closeBraceStartLineIndices.Contains(directiveLineIndex + 1));
    }

    /// <summary>
    /// Determines whether the blank lines directly above a target line follow a region directive that requires the blank
    /// line below it, so a rule that deletes blank lines in front of the target must not report them. The nearest line above
    /// the target that is not blank has to start with an active <c>#region</c> or <c>#endregion</c>, and
    /// <see cref="RequiresBlankLineBelow(bool, bool)"/> decides the closing-brace exemption on the target line
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

        var directive = root.FindTrivia(GetContentStart(sourceText, directiveLineIndex)).GetStructure();

        if (directive is not (RegionDirectiveTriviaSyntax { IsActive: true } or EndRegionDirectiveTriviaSyntax { IsActive: true }))
        {
            return false;
        }

        var targetContentStart = GetContentStart(sourceText, targetLineIndex);
        var targetFirstToken = root.FindToken(targetContentStart);
        var doesTargetBeginWithCloseBrace = targetFirstToken.IsKind(SyntaxKind.CloseBraceToken)
                                            && targetFirstToken.SpanStart == targetContentStart;

        return RequiresBlankLineBelow(directive is EndRegionDirectiveTriviaSyntax, doesTargetBeginWithCloseBrace);
    }

    /// <summary>
    /// Gets the position of the first character on the specified line that is not whitespace
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="lineIndex">Zero-based line index</param>
    /// <returns>The position behind the line's leading whitespace</returns>
    private static int GetContentStart(SourceText sourceText, int lineIndex)
    {
        var line = sourceText.Lines[lineIndex];

        return line.Start + FormattingTextAnalysisUtilities.GetLeadingWhitespace(FormattingTextAnalysisUtilities.GetLineText(sourceText, line)).Length;
    }

    #endregion // Methods
}