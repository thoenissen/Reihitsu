using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// The one owner of which query syntax declares a range variable and of the identifier it declares it with
/// </summary>
internal static class RangeVariableDeclarationUtilities
{
    #region Properties

    /// <summary>
    /// Kinds of the nodes that declare a range variable: the <c>from</c>, <c>let</c>, and <c>join</c> clauses, the
    /// <c>into</c> of a <c>join</c> clause, and a query continuation
    /// </summary>
    internal static ImmutableArray<SyntaxKind> DeclarationKinds { get; } = [
                                                                               SyntaxKind.FromClause,
                                                                               SyntaxKind.LetClause,
                                                                               SyntaxKind.JoinClause,
                                                                               SyntaxKind.JoinIntoClause,
                                                                               SyntaxKind.QueryContinuation
                                                                           ];

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Gets the identifier of the range variable a node declares
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>The identifier, or <see langword="default"/> when the node declares no range variable</returns>
    internal static SyntaxToken GetIdentifier(SyntaxNode node)
    {
        return node switch
               {
                   FromClauseSyntax fromClause => fromClause.Identifier,
                   LetClauseSyntax letClause => letClause.Identifier,
                   JoinClauseSyntax joinClause => joinClause.Identifier,
                   JoinIntoClauseSyntax joinIntoClause => joinIntoClause.Identifier,
                   QueryContinuationSyntax queryContinuation => queryContinuation.Identifier,
                   _ => default
               };
    }

    #endregion // Methods
}