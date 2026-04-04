using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Extensions;

/// <summary>
/// Extension methods for <see cref="SyntaxToken"/>
/// </summary>
internal static class SyntaxTokenExtensions
{
    /// <summary>
    /// Checking, if the node is any of the given kinds
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="kinds">Kinds</param>
    /// <returns>Is the node any of the given kinds?</returns>
    public static bool IsAnyKindOf(this SyntaxToken node, params Span<SyntaxKind> kinds)
    {
        foreach (var kind in kinds)
        {
            if (node.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }
}