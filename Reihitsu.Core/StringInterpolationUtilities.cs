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
        if (interpolatedString.Contents.Count == 0)
        {
            return false;
        }

        return interpolatedString.Contents.OfType<InterpolationSyntax>().Any();
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

        if (modifiedContent == stringContent)
        {
            return interpolatedString;
        }

        var replacementNode = SyntaxFactory.ParseExpression(modifiedContent, 0, interpolatedString.SyntaxTree.Options);

        if (replacementNode.IsMissing)
        {
            return interpolatedString;
        }

        return replacementNode.WithTriviaFrom(interpolatedString);
    }

    /// <summary>
    /// Removes leading $ characters while preserving @ character if present
    /// </summary>
    /// <param name="text">Start token text with potential @ and $ prefixes</param>
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

    #endregion // Methods
}