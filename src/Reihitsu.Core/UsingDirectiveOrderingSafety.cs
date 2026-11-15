using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Safety checks for using directive reordering
/// </summary>
public static class UsingDirectiveOrderingSafety
{
    #region Methods

    /// <summary>
    /// Determines whether the given using block can be reordered safely. A block is unsafe when a preprocessor
    /// directive stands anywhere in the trivia of its directives: in the leading or trailing trivia of a directive,
    /// or between its tokens. The decision reads trivia kinds, not text, so a line of a comment that starts with
    /// <c>#</c> is comment text and does not make the block unsafe
    /// </summary>
    /// <param name="usingDirectives">Using directives</param>
    /// <returns><see langword="true"/> if the block can be reordered safely</returns>
    public static bool CanSafelyReorder(SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return usingDirectives.Any(usingDirective => usingDirective.ContainsDirectives) == false;
    }

    #endregion // Methods
}