using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods that read the conditional directives of a type body and the accessors of its members, for
/// <see cref="OrderingDeclarationUtilities"/> and <see cref="OrderingMoveSafety"/>
/// </summary>
internal static class ConditionalGroupUnitUtilities
{
    #region Methods

    /// <summary>
    /// Gets the conditional directives (<see cref="SyntaxTriviaUtilities.IsConditionalDirective"/>) that start inside a
    /// span of the tree
    /// </summary>
    /// <param name="root">Root of the tree</param>
    /// <param name="span">Span</param>
    /// <returns>The conditional directives</returns>
    internal static List<DirectiveTriviaSyntax> GetConditionalDirectives(SyntaxNode root, TextSpan span)
    {
        return root.DescendantTrivia(span)
                   .Where(trivia => span.Contains(trivia.SpanStart)
                                    && SyntaxTriviaUtilities.IsConditionalDirective(trivia))
                   .Select(trivia => trivia.GetStructure())
                   .OfType<DirectiveTriviaSyntax>()
                   .ToList();
    }

    /// <summary>
    /// Determines whether the accessor has neither a body nor an expression body
    /// </summary>
    /// <param name="accessor">Accessor</param>
    /// <returns><see langword="true"/> if the accessor has no body</returns>
    internal static bool IsWithoutBody(AccessorDeclarationSyntax accessor)
    {
        return accessor.Body == null && accessor.ExpressionBody == null;
    }

    #endregion // Methods
}