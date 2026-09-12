using System;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Safety checks for using directive reordering
/// </summary>
public static class UsingDirectiveOrderingSafety
{
    #region Fields

    /// <summary>
    /// Matches a preprocessor directive line within a using block. This pattern carries no nested quantifier and
    /// cannot backtrack pathologically, and it only ever matches source Roslyn has already parsed, so it uses
    /// <see cref="Regex.InfiniteMatchTimeout"/> rather than a wall-clock budget: a thread merely descheduled on a
    /// loaded machine would otherwise throw and surface as AD0001, silently disabling the consuming analyzer for
    /// the whole compilation
    /// </summary>
    private static readonly Regex _preprocessorDirectiveRegex = new(@"(^|\r?\n)\s*#", RegexOptions.CultureInvariant | RegexOptions.Compiled, Regex.InfiniteMatchTimeout);

    #endregion // Fields

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
    /// Determines whether the using block contains a preprocessor directive line
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns><see langword="true"/> if the block contains a preprocessor directive</returns>
    private static bool ContainsPreprocessorDirective(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return _preprocessorDirectiveRegex.IsMatch(usingDirectives.ToFullString());
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

    #endregion // Methods
}