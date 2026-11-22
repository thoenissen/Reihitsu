using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Provides utility methods for working with string interpolations
/// </summary>
public static class StringInterpolationUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether an interpolated string contains any interpolation holes
    /// </summary>
    /// <param name="interpolatedString">Interpolated string expression</param>
    /// <returns><see langword="true"/> if the string has interpolation holes</returns>
    public static bool HasInterpolations(InterpolatedStringExpressionSyntax interpolatedString)
    {
        return interpolatedString.Contents.OfType<InterpolationSyntax>().Any();
    }

    /// <summary>
    /// Determines whether every interpolation, that of a non-verbatim interpolated string included, can contain a line break
    /// in the language version: from C# 11 on. Before it, an interpolation of a non-verbatim interpolated string cannot (CS8967)
    /// </summary>
    /// <param name="languageVersion">Effective language version the code is compiled with</param>
    /// <returns><see langword="true"/> when every interpolation can contain a line break; otherwise, <see langword="false"/></returns>
    public static bool CanInterpolationsHoldLineBreaks(LanguageVersion languageVersion)
    {
        return languageVersion >= LanguageVersion.CSharp11;
    }

    /// <summary>
    /// Determines whether the node is, or stands at any depth inside, an interpolation of an interpolated string that is
    /// not verbatim (one that starts with <c>$"</c>) while the code is compiled with C# 10 or earlier. Such an
    /// interpolation cannot contain a line break before C# 11 (CS8967), so nothing inside it may be broken onto another line
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <param name="languageVersion">Effective language version the code is compiled with</param>
    /// <returns><see langword="true"/> when no line break may be written inside the node; otherwise, <see langword="false"/></returns>
    public static bool IsInsideSingleLineInterpolation(SyntaxNode node, LanguageVersion languageVersion)
    {
        if (CanInterpolationsHoldLineBreaks(languageVersion))
        {
            return false;
        }

        return node.AncestorsAndSelf()
                   .OfType<InterpolationSyntax>()
                   .Any(interpolation => interpolation.Parent is InterpolatedStringExpressionSyntax interpolatedString
                                         && interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedStringStartToken));
    }

    /// <summary>
    /// Reconstructs the interpolated string expression without $ markers by removing them from the start token
    /// </summary>
    /// <param name="interpolatedString">The interpolated string expression</param>
    /// <returns>A new expression without $ markers, or the original if no changes needed</returns>
    public static SyntaxNode RemoveInterpolationMarkers(InterpolatedStringExpressionSyntax interpolatedString)
    {
        var stringContent = interpolatedString.ToString();
        var modifiedContent = RemoveLeadingDollarMarkers(stringContent);

        // Standard and verbatim interpolated strings escape literal braces by doubling them ({{ and }}). After the
        // dollar marker is removed the result is no longer interpolated, so the doubled braces must be collapsed back
        // to single braces to preserve the runtime value. Raw string literals do not use brace doubling, so they are
        // left untouched.
        if (interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedStringStartToken)
            || interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedVerbatimStringStartToken))
        {
            modifiedContent = UnescapeBraces(modifiedContent);
        }

        if (modifiedContent == stringContent)
        {
            return interpolatedString;
        }

        var replacementNode = SyntaxFactory.ParseExpression(modifiedContent, 0, interpolatedString.SyntaxTree.Options);

        if (replacementNode.ContainsDiagnostics)
        {
            return interpolatedString;
        }

        return replacementNode.WithTriviaFrom(interpolatedString);
    }

    /// <summary>
    /// Removes leading $ characters from the start token text while preserving a leading @ character if present
    /// </summary>
    /// <param name="text">Interpolated string text with potential @ and $ prefixes</param>
    /// <returns>Text with $ characters removed but @ preserved</returns>
    private static string RemoveLeadingDollarMarkers(string text)
    {
        var result = new StringBuilder();
        var hasAt = false;
        var index = 0;

        while (index < text.Length)
        {
            var character = text[index];

            if (character == '$')
            {
                index++;
            }
            else if (character == '@' && hasAt == false)
            {
                result.Append(character);
                hasAt = true;
                index++;
            }
            else
            {
                break;
            }
        }

        while (index < text.Length)
        {
            result.Append(text[index]);
            index++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Collapses doubled braces ({{ and }}) into single braces to undo interpolated string brace escaping
    /// </summary>
    /// <param name="text">Text that may contain doubled braces</param>
    /// <returns>Text with doubled braces collapsed into single braces</returns>
    private static string UnescapeBraces(string text)
    {
        return text.Replace("{{", "{")
                   .Replace("}}", "}");
    }

    #endregion // Methods
}