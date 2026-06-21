using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Provides helpers for analyzing blank lines around <c>#region</c> and <c>#endregion</c> directives
/// </summary>
public static class RegionDirectiveBlankLineUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the specified trivia is a <c>#region</c> or <c>#endregion</c> directive
    /// </summary>
    /// <param name="trivia">Trivia to inspect</param>
    /// <returns><see langword="true"/> when the trivia is a region or end-region directive</returns>
    public static bool IsRegionDirective(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
               || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
    }

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
    /// Determines whether the directive on the specified line is missing the blank line that must precede it
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="directiveLineIndex">Zero-based line index of the directive</param>
    /// <param name="openBraceEndLineIndices">Line indices whose last token is an opening brace</param>
    /// <returns><see langword="true"/> when a blank line must be inserted before the directive</returns>
    public static bool IsMissingRequiredBlankLineBefore(SourceText sourceText, int directiveLineIndex, ISet<int> openBraceEndLineIndices)
    {
        return directiveLineIndex > 0
               && FormattingTextAnalysisUtilities.IsBlankLine(sourceText, directiveLineIndex - 1) == false
               && openBraceEndLineIndices.Contains(directiveLineIndex - 1) == false;
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

    #endregion // Methods
}