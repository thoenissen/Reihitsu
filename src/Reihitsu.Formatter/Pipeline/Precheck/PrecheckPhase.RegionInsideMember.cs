using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Precheck;

/// <summary>
/// The region-directive shape of the precheck (FMT-UNSUP-023)
/// </summary>
internal sealed partial class PrecheckPhase
{
    #region Methods

    /// <summary>
    /// Finds the first active <c>#region</c> or <c>#endregion</c> directive of a node that stands in the leading trivia of a
    /// token other than the positions between members (FMT-UNSUP-023): the node's first token, the first token of a member
    /// declaration, a using directive, an extern alias, or a file's attribute list, the closing brace of a type, an extension
    /// block, an enum, or a namespace, and the end of the file. A whole file that holds a top-level statement excepts none of
    /// them. A region directive in an inactive branch is no directive of the build being formatted and is not found. A node
    /// without a directive is left at once, and a token's position is only decided when its leading trivia holds a region
    /// directive
    /// </summary>
    /// <param name="node">The node the precheck decides on</param>
    /// <returns>The first such directive, or <see langword="null"/></returns>
    private static SyntaxTrivia? FindRegionDirectiveInsideMember(SyntaxNode node)
    {
        if (node.ContainsDirectives == false)
        {
            return null;
        }

        var exceptsMemberPositions = node is not CompilationUnitSyntax compilationUnit
                                     || compilationUnit.Members.Any(static member => member is GlobalStatementSyntax) == false;
        var firstToken = node.GetFirstToken();

        foreach (var token in node.DescendantTokens())
        {
            if (token.HasLeadingTrivia == false)
            {
                continue;
            }

            foreach (var trivia in token.LeadingTrivia)
            {
                if (SyntaxTriviaUtilities.IsRegionDirective(trivia)
                    && SyntaxTriviaUtilities.IsInactiveDirective(trivia) == false)
                {
                    if (exceptsMemberPositions
                        && (token == firstToken || StandsBetweenMembers(token)))
                    {
                        break;
                    }

                    return trivia;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a region directive in the leading trivia of a token stands between members (FMT-UNSUP-023): the token
    /// is the end of the file, the closing brace of a type, an extension block, an enum, or a namespace, or the first token of a
    /// member declaration other than a top-level statement, of a using directive, of an extern alias, or of a file's attribute
    /// list. The node a token begins or closes contains the token, so the decision reads only the token's ancestors
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> when the token's leading trivia stands between members</returns>
    private static bool StandsBetweenMembers(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.EndOfFileToken))
        {
            return true;
        }

        if (token.IsKind(SyntaxKind.CloseBraceToken))
        {
            return token.Parent is BaseTypeDeclarationSyntax or NamespaceDeclarationSyntax;
        }

        for (var ancestor = token.Parent; ancestor != null && ancestor.GetFirstToken() == token; ancestor = ancestor.Parent)
        {
            switch (ancestor)
            {
                case GlobalStatementSyntax:
                    return false;

                case MemberDeclarationSyntax:
                case UsingDirectiveSyntax:
                case ExternAliasDirectiveSyntax:
                case AttributeListSyntax { Parent: CompilationUnitSyntax }:
                    return true;
            }
        }

        return false;
    }

    #endregion // Methods
}