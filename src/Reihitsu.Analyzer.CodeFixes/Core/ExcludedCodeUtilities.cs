using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.CodeFixes.Core;

/// <summary>
/// The check shared by the code fixes that change a name or a declaration other code can refer to: whether code that a
/// conditional directive excludes names it (SHARED-FIX-003)
/// </summary>
internal static class ExcludedCodeUtilities
{
    #region Methods

    /// <summary>
    /// Gets every document in which code can refer to a symbol
    /// </summary>
    /// <param name="solution">Solution containing the symbol</param>
    /// <param name="symbol">Symbol</param>
    /// <returns>The documents, in solution order</returns>
    internal static ImmutableArray<Document> GetReferringDocuments(Solution solution, ISymbol symbol)
    {
        var scope = GetReferenceScope(symbol);

        if (scope != null)
        {
            return scope.DeclaringSyntaxReferences
                        .Select(reference => solution.GetDocument(reference.SyntaxTree))
                        .Where(static document => document != null)
                        .Distinct()
                        .ToImmutableArray();
        }

        var declaringProject = symbol.DeclaringSyntaxReferences
                                     .Select(reference => solution.GetDocument(reference.SyntaxTree)?.Project)
                                     .FirstOrDefault(static project => project != null);

        return declaringProject == null
                   ? []
                   : GetDependentProjectIds(solution, [declaringProject.Id]).SelectMany(projectId => solution.GetProject(projectId)?.Documents ?? [])
                                                                            .ToImmutableArray();
    }

    /// <summary>
    /// Gets the specified projects and every project that depends on one of them
    /// </summary>
    /// <param name="solution">Solution containing the projects</param>
    /// <param name="projectIds">Projects whose dependents are added</param>
    /// <returns>The projects, without duplicates</returns>
    internal static ImmutableArray<ProjectId> GetDependentProjectIds(Solution solution, IEnumerable<ProjectId> projectIds)
    {
        var dependencyGraph = solution.GetProjectDependencyGraph();

        return projectIds.SelectMany(projectId => dependencyGraph.GetProjectsThatTransitivelyDependOnThisProject(projectId).Prepend(projectId))
                         .Distinct()
                         .ToImmutableArray();
    }

    /// <summary>
    /// Determines whether one of the names occurs as an identifier in code that a conditional directive excludes, in a
    /// document whose code can refer to one of the symbols
    /// </summary>
    /// <param name="solution">Solution containing the symbols</param>
    /// <param name="symbols">Symbols whose referring documents are inspected</param>
    /// <param name="names">Names to look for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when excluded code names one of the names; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Excluded code is not bound, so no reference search or rename reaches a reference in it, and an occurrence there
    /// cannot be told apart from an unrelated name. Every occurrence therefore counts
    /// </remarks>
    internal static Task<bool> HasExcludedCodeOccurrenceAsync(Solution solution,
                                                              IEnumerable<ISymbol> symbols,
                                                              ImmutableHashSet<string> names,
                                                              CancellationToken cancellationToken)
    {
        return HasExcludedCodeOccurrenceAsync(solution, symbols, names, null, cancellationToken);
    }

    /// <summary>
    /// Determines whether one of the names occurs as an identifier in code that a conditional directive excludes, in a
    /// document whose code can refer to one of the symbols, counting only the excluded code a filter accepts
    /// </summary>
    /// <param name="solution">Solution containing the symbols</param>
    /// <param name="symbols">Symbols whose referring documents are inspected</param>
    /// <param name="names">Names to look for</param>
    /// <param name="isCounted">Filter that accepts the disabled text whose occurrences count, or <see langword="null"/> to count every one</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when counted excluded code names one of the names; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Excluded code is not bound, so no reference search or rename reaches a reference in it, and an occurrence there
    /// cannot be told apart from an unrelated name. Every occurrence therefore counts
    /// </remarks>
    internal static async Task<bool> HasExcludedCodeOccurrenceAsync(Solution solution,
                                                                    IEnumerable<ISymbol> symbols,
                                                                    ImmutableHashSet<string> names,
                                                                    Func<SyntaxTrivia, bool> isCounted,
                                                                    CancellationToken cancellationToken)
    {
        foreach (var document in symbols.SelectMany(symbol => GetReferringDocuments(solution, symbol)).Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root?.ContainsDirectives != true)
            {
                continue;
            }

            if (root.DescendantTrivia(descendIntoTrivia: true)
                    .Where(trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia)
                                     && (isCounted == null
                                         || isCounted(trivia)))
                    .SelectMany(static trivia => SyntaxFactory.ParseTokens(trivia.ToString()))
                    .Any(token => token.IsKind(SyntaxKind.IdentifierToken)
                                  && names.Contains(token.ValueText)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the symbol whose declarations bound the code that can refer to a symbol
    /// </summary>
    /// <param name="symbol">Symbol</param>
    /// <returns>
    /// The bounding symbol, or <see langword="null"/> when code of the declaring project and of every project that
    /// depends on it can refer to the symbol
    /// </returns>
    private static ISymbol GetReferenceScope(ISymbol symbol)
    {
        if (symbol is ITypeParameterSymbol)
        {
            return symbol.ContainingSymbol;
        }

        // A parameter can be named by a named argument wherever its method can be called, so its scope is the method's
        var current = symbol is IParameterSymbol
                          ? symbol.ContainingSymbol
                          : symbol;

        while (current != null
               && current is not INamespaceSymbol)
        {
            switch (current)
            {
                case ILocalSymbol or IRangeVariableSymbol or ILabelSymbol:
                case IMethodSymbol { MethodKind: MethodKind.LocalFunction or MethodKind.AnonymousFunction }:
                case INamedTypeSymbol { IsFileLocal: true }:
                    {
                        return current;
                    }

                case { DeclaredAccessibility: Accessibility.Private }:
                    {
                        return current.ContainingType ?? current;
                    }
            }

            current = current.ContainingSymbol;
        }

        return null;
    }

    #endregion // Methods
}