using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Provides helpers for working with region directives.
/// </summary>
internal static class RegionDirectiveUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the directive is located within an element body.
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive is inside an element body</returns>
    internal static bool IsWithinElementBody(SyntaxTrivia directiveTrivia)
    {
        var currentNode = directiveTrivia.Token.Parent;

        while (currentNode != null)
        {
            switch (currentNode)
            {
                case BlockSyntax { Parent: not TypeDeclarationSyntax and not NamespaceDeclarationSyntax and not FileScopedNamespaceDeclarationSyntax and not CompilationUnitSyntax }:
                case AccessorListSyntax:
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                case StatementSyntax:
                    return true;

                case TypeDeclarationSyntax:
                case NamespaceDeclarationSyntax:
                case FileScopedNamespaceDeclarationSyntax:
                case CompilationUnitSyntax:
                    return false;

                default:
                    currentNode = currentNode.Parent;

                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to find the matching region directive.
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="matchingDirectiveTrivia">Matching directive trivia</param>
    /// <returns><see langword="true"/> if a matching directive was found</returns>
    internal static bool TryFindMatchingDirective(SyntaxNode syntaxRoot, SyntaxTrivia directiveTrivia, out SyntaxTrivia matchingDirectiveTrivia)
    {
        matchingDirectiveTrivia = default;

        if (syntaxRoot == null
            || (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
                && directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false))
        {
            return false;
        }

        var directives = syntaxRoot.DescendantTrivia(descendIntoTrivia: true)
                                   .Where(trivia => trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                                                    || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                                   .ToList();
        var directiveIndex = directives.FindIndex(currentDirective => currentDirective == directiveTrivia);

        if (directiveIndex < 0)
        {
            return false;
        }

        if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
        {
            var nestedRegionCount = 0;

            for (var directivePosition = directiveIndex + 1; directivePosition < directives.Count; directivePosition++)
            {
                var currentDirective = directives[directivePosition];

                if (currentDirective.IsKind(SyntaxKind.RegionDirectiveTrivia))
                {
                    nestedRegionCount++;
                }
                else if (nestedRegionCount == 0)
                {
                    matchingDirectiveTrivia = currentDirective;

                    return true;
                }
                else
                {
                    nestedRegionCount--;
                }
            }
        }
        else
        {
            var nestedEndRegionCount = 0;

            for (var directivePosition = directiveIndex - 1; directivePosition >= 0; directivePosition--)
            {
                var currentDirective = directives[directivePosition];

                if (currentDirective.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                {
                    nestedEndRegionCount++;
                }
                else if (nestedEndRegionCount == 0)
                {
                    matchingDirectiveTrivia = currentDirective;

                    return true;
                }
                else
                {
                    nestedEndRegionCount--;
                }
            }
        }

        return false;
    }

    #endregion // Methods
}