using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.UsingDirectives;

/// <summary>
/// Safety checks for using directive reordering
/// </summary>
public static class UsingDirectiveOrderingSafety
{
    #region Methods

    /// <summary>
    /// Determines whether the given using block can be reordered safely
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns><see langword="true"/> if the block can be reordered safely</returns>
    public static bool CanSafelyReorder(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return usingDirectives.Any(HasUnsafeTrivia) == false
               && ContainsPreprocessorDirective(usingDirectives) == false;
    }

    /// <summary>
    /// Determines whether a using directive contains trivia that must not be moved
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if the directive has unsafe trivia</returns>
    private static bool HasUnsafeTrivia(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.GetLeadingTrivia()
                             .Concat(usingDirective.GetTrailingTrivia())
                             .Any(trivia => trivia.GetStructure() is DirectiveTriviaSyntax);
    }

    /// <summary>
    /// Determines whether the using block contains a preprocessor directive line
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns><see langword="true"/> if the block contains a preprocessor directive</returns>
    private static bool ContainsPreprocessorDirective(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return Regex.IsMatch(usingDirectives.ToFullString(), @"(^|\r?\n)\s*#", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    }

    #endregion // Methods
}