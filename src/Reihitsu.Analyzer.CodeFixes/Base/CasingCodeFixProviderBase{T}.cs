using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider for casing rules
/// </summary>
/// <typeparam name="T">Node type</typeparam>
public abstract class CasingCodeFixProviderBase<T> : CodeFixProvider
    where T : SyntaxNode
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Title
    /// </summary>
    private readonly string _title;

    /// <summary>
    /// Casing conversion
    /// </summary>
    private readonly Func<string, string> _casingConversion;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="casingConversion">Casing conversion</param>
    protected CasingCodeFixProviderBase(string diagnosticId, string title, Func<string, string> casingConversion)
    {
        _diagnosticId = diagnosticId;
        _title = title;
        _casingConversion = casingConversion;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Get identifier value text
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Identifier value text</returns>
    protected abstract string GetIdentifier(T node);

    /// <summary>
    /// Determines whether a code fix can be safely registered for the specified node
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the code fix can be safely registered; otherwise, <see langword="false"/></returns>
    protected virtual bool CanRegisterCodeFix(T node)
    {
        return true;
    }

    /// <summary>
    /// Gets the declared symbol that the rename should target
    /// </summary>
    /// <param name="model">Semantic model</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// The declared symbol when a comprehensive rename of the declaration and all references is supported;
    /// otherwise, <see langword="null"/>
    /// </returns>
    private static ISymbol GetDeclaredSymbol(SemanticModel model, T node, CancellationToken cancellationToken)
    {
        // Tuple elements are intentionally excluded because the renamer does not support them, which means only the
        // declaration could be changed
        return node switch
               {
                   VariableDeclaratorSyntax variableDeclarator => model.GetDeclaredSymbol(variableDeclarator, cancellationToken),
                   MemberDeclarationSyntax memberDeclaration => model.GetDeclaredSymbol(memberDeclaration, cancellationToken),
                   ParameterSyntax parameter => model.GetDeclaredSymbol(parameter, cancellationToken),
                   SingleVariableDesignationSyntax singleVariableDesignation => model.GetDeclaredSymbol(singleVariableDesignation, cancellationToken),
                   ForEachStatementSyntax forEachStatement => model.GetDeclaredSymbol(forEachStatement, cancellationToken),
                   CatchDeclarationSyntax catchDeclaration => model.GetDeclaredSymbol(catchDeclaration, cancellationToken),
                   LocalFunctionStatementSyntax localFunctionStatement => model.GetDeclaredSymbol(localFunctionStatement, cancellationToken),
                   TypeParameterSyntax typeParameter => model.GetDeclaredSymbol(typeParameter, cancellationToken),
                   _ => null
               };
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node</param>
    /// <param name="identifier">The replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Solution> ApplyCodeFixAsync(Document document, T node, string identifier, CancellationToken cancellationToken)
    {
        var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        var declaredSymbol = model == null
                                 ? null
                                 : GetDeclaredSymbol(model, node, cancellationToken);

        // The fix is only registered when a declared symbol is available, so a comprehensive rename is always
        // possible here. Without a symbol the solution is left unchanged instead of producing a declaration-only
        // rename that would leave references stale
        if (declaredSymbol == null)
        {
            return document.Project.Solution;
        }

        return await Renamer.RenameSymbolAsync(document.Project.Solution, declaredSymbol, default, identifier, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the diagnostics covered by the requested Document, Project, or Solution Fix All scope
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <returns>Diagnostics in deterministic project and document order</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetFixAllDiagnosticsAsync(FixAllContext context)
    {
        switch (context.Scope)
        {
            case FixAllScope.Document when context.Document != null:
                {
                    return await context.GetDocumentDiagnosticsAsync(context.Document).ConfigureAwait(false);
                }

            case FixAllScope.Project when context.Project != null:
                {
                    return await context.GetAllDiagnosticsAsync(context.Project).ConfigureAwait(false);
                }

            case FixAllScope.Solution:
                {
                    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

                    foreach (var project in context.Solution.Projects)
                    {
                        diagnostics.AddRange(await context.GetAllDiagnosticsAsync(project).ConfigureAwait(false));
                    }

                    return diagnostics.ToImmutable();
                }

            default:
                {
                    return [];
                }
        }
    }

    /// <summary>
    /// Gets a stable project, path, and document key for diagnostic ordering
    /// </summary>
    /// <param name="solution">Solution containing the diagnostic</param>
    /// <param name="sourceTree">Diagnostic source tree</param>
    /// <returns>Stable diagnostic document key</returns>
    private static string GetDocumentSortKey(Solution solution, SyntaxTree sourceTree)
    {
        var document = sourceTree == null
                           ? null
                           : solution.GetDocument(sourceTree);

        return document == null
                   ? string.Empty
                   : $"{document.Project.Name}\0{document.FilePath ?? document.Name}\0{document.Name}";
    }

    /// <summary>
    /// Adds tracking annotations so every declaration can be re-resolved after earlier solution-wide renames
    /// </summary>
    /// <param name="solution">Solution containing the candidates</param>
    /// <param name="candidates">Candidates to annotate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The annotated solution</returns>
    private static async Task<Solution> AnnotateFixAllCandidatesAsync(Solution solution,
                                                                      ImmutableArray<FixAllCandidate> candidates,
                                                                      CancellationToken cancellationToken)
    {
        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = solution.GetDocument(candidate.DocumentId);
            var root = document == null
                           ? null
                           : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (document == null
                || root == null
                || root.FindNode(candidate.OriginalSpan) is not T node)
            {
                continue;
            }

            solution = document.WithSyntaxRoot(root.ReplaceNode(node, node.WithAdditionalAnnotations(candidate.Annotation))).Project.Solution;
        }

        return solution;
    }

    /// <summary>
    /// Finds candidates with one containing symbol and one normalized target whose renames cannot both be applied
    /// </summary>
    /// <param name="solution">Annotated solution</param>
    /// <param name="candidates">Aggregate rename candidates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Candidates belonging to conflicting duplicate-target groups</returns>
    private static async Task<ImmutableArray<FixAllCandidate>> GetDuplicateTargetCandidatesAsync(Solution solution,
                                                                                                 ImmutableArray<FixAllCandidate> candidates,
                                                                                                 CancellationToken cancellationToken)
    {
        var duplicateTargetCandidates = ImmutableArray.CreateBuilder<FixAllCandidate>();

        for (var firstIndex = 0; firstIndex < candidates.Length; firstIndex++)
        {
            await AddDuplicateTargetsForCandidateAsync(solution,
                                                       candidates,
                                                       firstIndex,
                                                       duplicateTargetCandidates,
                                                       cancellationToken).ConfigureAwait(false);
        }

        return duplicateTargetCandidates.ToImmutable();
    }

    /// <summary>
    /// Adds every later candidate that conflicts with one candidate's normalized target
    /// </summary>
    /// <param name="solution">Annotated solution</param>
    /// <param name="candidates">Ordered fix-all candidates</param>
    /// <param name="firstIndex">Index of the candidate to compare</param>
    /// <param name="duplicates">Duplicate target accumulator</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes after all later candidates have been compared</returns>
    private static async Task AddDuplicateTargetsForCandidateAsync(Solution solution,
                                                                   ImmutableArray<FixAllCandidate> candidates,
                                                                   int firstIndex,
                                                                   ImmutableArray<FixAllCandidate>.Builder duplicates,
                                                                   CancellationToken cancellationToken)
    {
        var first = candidates[firstIndex];

        for (var secondIndex = firstIndex + 1; secondIndex < candidates.Length; secondIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var second = candidates[secondIndex];

            if (string.Equals(first.Identifier, second.Identifier, StringComparison.Ordinal)
                && SymbolEqualityComparer.Default.Equals(first.ContainingSymbol, second.ContainingSymbol)
                && await CandidatesConflictAsync(solution, first, second, cancellationToken).ConfigureAwait(false))
            {
                AddIfMissing(duplicates, first);
                AddIfMissing(duplicates, second);
            }
        }
    }

    /// <summary>
    /// Adds a candidate when its tracking annotation is not already represented
    /// </summary>
    /// <param name="candidates">Candidate accumulator</param>
    /// <param name="candidate">Candidate to add</param>
    private static void AddIfMissing(ImmutableArray<FixAllCandidate>.Builder candidates, FixAllCandidate candidate)
    {
        if (candidates.Any(existing => existing.Annotation == candidate.Annotation) == false)
        {
            candidates.Add(candidate);
        }
    }

    /// <summary>
    /// Determines whether two candidates with one normalized target conflict when composed
    /// </summary>
    /// <param name="solution">Annotated solution</param>
    /// <param name="first">First candidate</param>
    /// <param name="second">Second candidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when applying both candidates would create a rename conflict; otherwise, <see langword="false"/></returns>
    private static async Task<bool> CandidatesConflictAsync(Solution solution,
                                                            FixAllCandidate first,
                                                            FixAllCandidate second,
                                                            CancellationToken cancellationToken)
    {
        var firstRenamedSolution = await RenameWithoutConflictAsync(solution,
                                                                    first.DocumentId,
                                                                    first.Annotation,
                                                                    first.Identifier,
                                                                    cancellationToken).ConfigureAwait(false);

        return firstRenamedSolution != null
               && await RenameWithoutConflictAsync(firstRenamedSolution,
                                                   second.DocumentId,
                                                   second.Annotation,
                                                   second.Identifier,
                                                   cancellationToken).ConfigureAwait(false) == null;
    }

    /// <summary>
    /// Re-resolves an aggregate rename candidate in the current evolving solution
    /// </summary>
    /// <param name="solution">Current solution</param>
    /// <param name="candidate">Candidate to resolve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The current declaration node and symbol, or <see langword="null"/> when either is unavailable</returns>
    private static async Task<(T Node, ISymbol Symbol)?> ResolveFixAllCandidateAsync(Solution solution,
                                                                                     FixAllCandidate candidate,
                                                                                     CancellationToken cancellationToken)
    {
        var document = solution.GetDocument(candidate.DocumentId);
        var root = document == null
                       ? null
                       : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var model = document == null
                        ? null
                        : await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var node = root?.GetAnnotatedNodes(candidate.Annotation).OfType<T>().SingleOrDefault();
        var symbol = model == null || node == null
                         ? null
                         : GetDeclaredSymbol(model, node, cancellationToken);

        return node == null || symbol == null
                   ? null
                   : (node, symbol);
    }

    /// <summary>
    /// Gets the symbol whose declarations bound the code that can refer to a renamed symbol
    /// </summary>
    /// <param name="symbol">Renamed symbol</param>
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

    /// <summary>
    /// Gets every document in which code can refer to a renamed symbol
    /// </summary>
    /// <param name="solution">Solution containing the symbol</param>
    /// <param name="symbol">Renamed symbol</param>
    /// <returns>The documents, in solution order</returns>
    private static ImmutableArray<Document> GetReferringDocuments(Solution solution, ISymbol symbol)
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
    private static ImmutableArray<ProjectId> GetDependentProjectIds(Solution solution, IEnumerable<ProjectId> projectIds)
    {
        var dependencyGraph = solution.GetProjectDependencyGraph();

        return projectIds.SelectMany(projectId => dependencyGraph.GetProjectsThatTransitivelyDependOnThisProject(projectId).Prepend(projectId))
                         .Distinct()
                         .ToImmutableArray();
    }

    /// <summary>
    /// Gets the rename cascade of a declared symbol: the symbol itself and at least every symbol declared in source that a
    /// rename of it renames with it, as Roslyn determines it
    /// </summary>
    /// <param name="solution">Solution containing the symbol</param>
    /// <param name="symbol">Declared symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The symbols, starting with the declared symbol, without duplicates</returns>
    /// <remarks>
    /// The set is read from Roslyn's reference engine rather than modeled by relation kind: the definitions that
    /// <see cref="SymbolFinder.FindReferencesAsync(ISymbol, Solution, CancellationToken)"/> reports are the symbols it
    /// cascades to, the same search the renamer uses to find what it renames. This covers, for example, overridden and
    /// overriding members, implemented interface members and their implementations, and the other part of a partial
    /// method together with its parameters. The search can also reach a symbol the renamer leaves as it is, such as a
    /// type parameter of the other part of a partial method; that only widens the region a check inspects, and
    /// <see cref="GetOldNames"/> takes no old name from such a symbol. Every safety check of a rename takes its region
    /// from these symbols rather than from the declared symbol alone. A symbol declared only in a referenced assembly
    /// is not renamed, so it is not part of the cascade
    /// </remarks>
    private static async Task<ImmutableArray<ISymbol>> GetRenameCascadeAsync(Solution solution, ISymbol symbol, CancellationToken cancellationToken)
    {
        var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false);

        return referencedSymbols.Select(static referencedSymbol => referencedSymbol.Definition)
                                .Where(static definition => definition.Locations.Any(static location => location.IsInSource))
                                .Prepend(symbol)
                                .Distinct<ISymbol>(SymbolEqualityComparer.Default)
                                .ToImmutableArray();
    }

    /// <summary>
    /// Gets the old names of a rename: the names that the symbols of the cascade carried before the rename and no
    /// longer carry after it
    /// </summary>
    /// <param name="cascade">Rename cascade before the rename</param>
    /// <param name="renamedCascade">Rename cascade of the renamed symbol after the rename</param>
    /// <returns>The old names</returns>
    /// <remarks>
    /// Only symbols that source can refer to by their name are counted, so an accessor or a constructor adds no old
    /// name. A symbol of the cascade spelled differently from the declared one, such as the parameter of the other part
    /// of a partial method, adds its own spelling when the renamer renames it. The names are compared as counts rather
    /// than by symbol, because the reference search the cascade is read from also reaches symbols the renamer leaves
    /// as they are, such as a type parameter of the other part of a partial method: such a symbol keeps its name in
    /// both cascades and adds no old name
    /// </remarks>
    private static ImmutableHashSet<string> GetOldNames(ImmutableArray<ISymbol> cascade, ImmutableArray<ISymbol> renamedCascade)
    {
        var remainingNames = renamedCascade.Where(static symbol => symbol.CanBeReferencedByName)
                                           .Select(static symbol => symbol.Name)
                                           .ToList();
        var oldNames = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var name in cascade.Where(static symbol => symbol.CanBeReferencedByName).Select(static symbol => symbol.Name))
        {
            var isRetained = remainingNames.Remove(name);

            if (isRetained == false)
            {
                oldNames.Add(name);
            }
        }

        return oldNames.ToImmutable();
    }

    /// <summary>
    /// Determines whether an old name or the new name occurs as an identifier in code that a conditional directive
    /// excludes
    /// </summary>
    /// <param name="solution">Solution containing the symbols</param>
    /// <param name="cascade">Rename cascade before the rename</param>
    /// <param name="oldNames">Old names of the cascade, as <see cref="GetOldNames"/> reads them</param>
    /// <param name="identifier">Proposed identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when excluded code names an old name or the new name; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Excluded code is not bound, so the renamer cannot rename a reference in it, and an occurrence there cannot be
    /// told apart from an unrelated name. Every occurrence therefore withholds the rename, in every document whose code
    /// can refer to one of the symbols of the cascade
    /// </remarks>
    private static async Task<bool> HasExcludedCodeOccurrenceAsync(Solution solution,
                                                                   ImmutableArray<ISymbol> cascade,
                                                                   ImmutableHashSet<string> oldNames,
                                                                   string identifier,
                                                                   CancellationToken cancellationToken)
    {
        foreach (var document in cascade.SelectMany(symbol => GetReferringDocuments(solution, symbol)).Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root?.ContainsDirectives != true)
            {
                continue;
            }

            if (root.DescendantTrivia(descendIntoTrivia: true)
                    .Where(static trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia))
                    .SelectMany(static trivia => SyntaxFactory.ParseTokens(trivia.ToString()))
                    .Any(token => token.IsKind(SyntaxKind.IdentifierToken)
                                  && (oldNames.Contains(token.ValueText)
                                      || string.Equals(token.ValueText, identifier, StringComparison.Ordinal))))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the name of the native function that a method binds to through an import attribute
    /// </summary>
    /// <param name="symbol">Symbol</param>
    /// <returns>The entry point name, or <see langword="null"/> when the symbol binds to no native function</returns>
    private static string GetNativeEntryPoint(ISymbol symbol)
    {
        if (symbol is not IMethodSymbol method)
        {
            return null;
        }

        var dllImportData = method.GetDllImportData();

        if (dllImportData != null)
        {
            return dllImportData.EntryPointName ?? method.Name;
        }

        foreach (var attribute in method.GetAttributes())
        {
            if (attribute.AttributeClass is { Name: "DllImportAttribute" or "LibraryImportAttribute" } attributeClass
                && string.Equals(attributeClass.ContainingNamespace?.ToDisplayString(), "System.Runtime.InteropServices", StringComparison.Ordinal))
            {
                var entryPoint = attribute.NamedArguments.FirstOrDefault(static argument => string.Equals(argument.Key, "EntryPoint", StringComparison.Ordinal));

                return entryPoint.Value.Value as string ?? method.Name;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the names of the native functions that the methods of a rename cascade bind to through an import attribute
    /// </summary>
    /// <param name="cascade">Symbols the rename renames</param>
    /// <returns>The entry point names in ordinal order</returns>
    /// <remarks>
    /// A rename changes no binding of a cascade member other than through its name, so the cascade binds to the same
    /// native functions after the rename exactly when the two ordered lists are equal
    /// </remarks>
    private static ImmutableArray<string> GetNativeEntryPoints(ImmutableArray<ISymbol> cascade)
    {
        return cascade.Select(GetNativeEntryPoint)
                      .Where(static entryPoint => entryPoint != null)
                      .OrderBy(static entryPoint => entryPoint, StringComparer.Ordinal)
                      .ToImmutableArray();
    }

    /// <summary>
    /// Gets the compile errors of a project: the parse, declaration, and method body errors of every document
    /// </summary>
    /// <param name="project">Project</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The errors, or a default array when the compilation is unavailable</returns>
    /// <remarks>
    /// A rename can put an error into a document it does not change, for example when the new name makes a query clause,
    /// a deconstruction, <see langword="foreach"/>, or <see langword="await"/> bind ambiguously, so every document counts
    /// </remarks>
    private static async Task<ImmutableArray<Diagnostic>> GetCompileErrorsAsync(Project project, CancellationToken cancellationToken)
    {
        var compilation = project == null
                              ? null
                              : await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

        return compilation == null
                   ? default
                   : compilation.GetDiagnostics(cancellationToken)
                                .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                                .ToImmutableArray();
    }

    /// <summary>
    /// Determines whether a rename added a compile error to one of the projects it affects
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="projectIds">Projects the rename affects</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the rename added a compile error; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The renamer renames every source declaration of the symbol's rename cascade but no member declared in a
    /// referenced assembly, so an added error can stand in any type and any document of an affected project, not only
    /// in the one declaring the renamed symbol. Errors are compared by ID because the rename moves their locations
    /// </remarks>
    private static async Task<bool> HasAddedCompileErrorsAsync(Solution originalSolution,
                                                               Solution renamedSolution,
                                                               ImmutableArray<ProjectId> projectIds,
                                                               CancellationToken cancellationToken)
    {
        foreach (var projectId in projectIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var originalErrors = await GetCompileErrorsAsync(originalSolution.GetProject(projectId), cancellationToken).ConfigureAwait(false);
            var renamedErrors = await GetCompileErrorsAsync(renamedSolution.GetProject(projectId), cancellationToken).ConfigureAwait(false);

            if (originalErrors.IsDefault
                || renamedErrors.IsDefault
                || renamedErrors.GroupBy(static diagnostic => diagnostic.Id)
                                .Any(errorGroup => errorGroup.Count() > originalErrors.Count(diagnostic => string.Equals(diagnostic.Id, errorGroup.Key, StringComparison.Ordinal))))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets, for every positional parameter of every record in the specified projects, whether the compiler declares a
    /// property for it
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectIds">Projects to inspect</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The flags in project, document, and declaration order, or a default array when a model is unavailable</returns>
    /// <remarks>
    /// The compiler declares a property for a positional parameter only when the record neither declares nor inherits a
    /// member of the same name. A rename does not add, remove, or reorder declarations, so the flags of two solutions
    /// compare position by position
    /// </remarks>
    private static async Task<ImmutableArray<bool>> GetPositionalPropertyFlagsAsync(Solution solution,
                                                                                    ImmutableArray<ProjectId> projectIds,
                                                                                    CancellationToken cancellationToken)
    {
        var flags = ImmutableArray.CreateBuilder<bool>();

        foreach (var document in projectIds.SelectMany(projectId => solution.GetProject(projectId)?.Documents ?? []))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var records = root?.DescendantNodes(static node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax)
                              .OfType<RecordDeclarationSyntax>()
                              .Where(static record => record.ParameterList != null)
                              .ToImmutableArray() ?? [];

            if (records.IsEmpty)
            {
                continue;
            }

            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (model == null)
            {
                return default;
            }

            foreach (var record in records)
            {
                var recordType = model.GetDeclaredSymbol(record, cancellationToken);

                foreach (var parameter in record.ParameterList.Parameters)
                {
                    flags.Add(recordType != null
                              && recordType.GetMembers(parameter.Identifier.ValueText)
                                           .OfType<IPropertySymbol>()
                                           .Any(property => property.DeclaringSyntaxReferences.Any(reference => reference.SyntaxTree == parameter.SyntaxTree
                                                                                                                && parameter.Span.Contains(reference.Span))));
                }
            }
        }

        return flags.ToImmutable();
    }

    /// <summary>
    /// Determines whether a rename changes which positional parameters of a record get a compiler-declared property
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="projectIds">Projects the rename affects</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a public property would appear or disappear; otherwise, <see langword="false"/></returns>
    private static async Task<bool> ChangesPositionalPropertiesAsync(Solution originalSolution,
                                                                     Solution renamedSolution,
                                                                     ImmutableArray<ProjectId> projectIds,
                                                                     CancellationToken cancellationToken)
    {
        var originalFlags = await GetPositionalPropertyFlagsAsync(originalSolution, projectIds, cancellationToken).ConfigureAwait(false);
        var renamedFlags = await GetPositionalPropertyFlagsAsync(renamedSolution, projectIds, cancellationToken).ConfigureAwait(false);

        return originalFlags.IsDefault
               || renamedFlags.IsDefault
               || originalFlags.SequenceEqual(renamedFlags) == false;
    }

    /// <summary>
    /// Determines whether a namespace-owned declaration's proposed name collides with an existing source member
    /// </summary>
    /// <param name="declaredSymbol">Declaration symbol</param>
    /// <param name="identifier">Proposed identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the proposed name collides; otherwise, <see langword="false"/></returns>
    private static bool HasNamespaceCollision(ISymbol declaredSymbol, string identifier, CancellationToken cancellationToken)
    {
        if (declaredSymbol.ContainingSymbol is not INamespaceSymbol containingNamespace)
        {
            return false;
        }

        var declaringTrees = declaredSymbol.DeclaringSyntaxReferences.Select(static reference => reference.SyntaxTree)
                                                                     .ToImmutableHashSet();

        foreach (var member in containingNamespace.GetMembers(identifier))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsNamespaceCollision(member, declaredSymbol, declaringTrees))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether one existing namespace member conflicts with the proposed declaration name
    /// </summary>
    /// <param name="member">Existing namespace member</param>
    /// <param name="declaredSymbol">Declaration being renamed</param>
    /// <param name="declaringTrees">Syntax trees containing the declaration</param>
    /// <returns><see langword="true"/> when the existing member conflicts; otherwise, <see langword="false"/></returns>
    private static bool IsNamespaceCollision(ISymbol member, ISymbol declaredSymbol, ImmutableHashSet<SyntaxTree> declaringTrees)
    {
        if (SymbolEqualityComparer.Default.Equals(member, declaredSymbol)
            || member.DeclaringSyntaxReferences.IsEmpty)
        {
            return false;
        }

        if (declaredSymbol is not INamedTypeSymbol declaredType
            || member is not INamedTypeSymbol existingType)
        {
            return true;
        }

        return declaredType.Arity == existingType.Arity
               && ((declaredType.IsFileLocal || existingType.IsFileLocal) == false
                   || existingType.DeclaringSyntaxReferences.Any(reference => declaringTrees.Contains(reference.SyntaxTree)));
    }

    /// <summary>
    /// Determines whether a comprehensive rename is unsafe, as <see cref="RenameWithoutConflictAsync"/> decides it
    /// </summary>
    /// <param name="document">Document containing the declaration</param>
    /// <param name="node">Declaration node to rename</param>
    /// <param name="identifier">Proposed replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the rename would conflict; otherwise, <see langword="false"/></returns>
    private static async Task<bool> HasRenameConflictAsync(Document document, T node, string identifier, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return true;
        }

        var annotation = new SyntaxAnnotation(nameof(HasRenameConflictAsync));
        var annotatedSolution = document.WithSyntaxRoot(root.ReplaceNode(node, node.WithAdditionalAnnotations(annotation))).Project.Solution;

        return await RenameWithoutConflictAsync(annotatedSolution,
                                                document.Id,
                                                annotation,
                                                identifier,
                                                cancellationToken).ConfigureAwait(false) == null;
    }

    /// <summary>
    /// Applies a rename only when it is safe: no namespace member collision, no name in excluded code, no Roslyn
    /// conflict annotation, no changed native entry point, no added compile error, and no change to the properties the
    /// compiler declares for positional record parameters
    /// </summary>
    /// <param name="solution">Solution containing the declaration</param>
    /// <param name="documentId">Declaration document</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="identifier">Proposed replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The renamed solution when it is safe; otherwise, <see langword="null"/></returns>
    /// <remarks>
    /// The checks that depend on what the rename reaches take it from <see cref="GetRenameCascadeAsync"/>: the native
    /// entry points of every cascade member, and the excluded code of every document that can refer to one, searched
    /// for the new name and for the old names that the cascade before and after the rename gives. The compile
    /// error and positional property checks inspect every document of every project the rename changes or that depends
    /// on one of them
    /// </remarks>
    private static async Task<Solution> RenameWithoutConflictAsync(Solution solution,
                                                                   DocumentId documentId,
                                                                   SyntaxAnnotation annotation,
                                                                   string identifier,
                                                                   CancellationToken cancellationToken)
    {
        var prepared = await PrepareRenameAsync(solution,
                                                documentId,
                                                annotation,
                                                identifier,
                                                cancellationToken).ConfigureAwait(false);

        if (prepared == null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var renamedSolution = await Renamer.RenameSymbolAsync(solution,
                                                              prepared.Value.Symbol,
                                                              default,
                                                              identifier,
                                                              cancellationToken)
                                           .ConfigureAwait(false);

        if (await HasConflictAnnotationsAsync(solution, renamedSolution, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var renamedDeclaration = await ResolveRenameDeclarationAsync(renamedSolution, documentId, annotation, cancellationToken).ConfigureAwait(false);

        if (renamedDeclaration == null)
        {
            return null;
        }

        var renamedCascade = await GetRenameCascadeAsync(renamedSolution, renamedDeclaration.Value.Symbol, cancellationToken).ConfigureAwait(false);

        if (GetNativeEntryPoints(prepared.Value.Cascade).SequenceEqual(GetNativeEntryPoints(renamedCascade), StringComparer.Ordinal) == false)
        {
            return null;
        }

        if (await HasExcludedCodeOccurrenceAsync(solution,
                                                 prepared.Value.Cascade,
                                                 GetOldNames(prepared.Value.Cascade, renamedCascade),
                                                 identifier,
                                                 cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var projectChanges = renamedSolution.GetChanges(solution).GetProjectChanges();
        var affectedProjectIds = GetDependentProjectIds(solution, projectChanges.Select(static changes => changes.ProjectId));

        if (await HasAddedCompileErrorsAsync(solution, renamedSolution, affectedProjectIds, cancellationToken).ConfigureAwait(false)
            || await ChangesPositionalPropertiesAsync(solution, renamedSolution, affectedProjectIds, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return renamedSolution;
    }

    /// <summary>
    /// Resolves the declaration's symbol and its rename cascade and rejects the renames that are unsafe before the
    /// renamer runs
    /// </summary>
    /// <param name="solution">Annotated solution</param>
    /// <param name="documentId">Document containing the declaration</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="identifier">Proposed identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The symbol to rename and its rename cascade, or <see langword="null"/> when the rename is unsafe</returns>
    private static async Task<(ISymbol Symbol, ImmutableArray<ISymbol> Cascade)?> PrepareRenameAsync(Solution solution,
                                                                                                     DocumentId documentId,
                                                                                                     SyntaxAnnotation annotation,
                                                                                                     string identifier,
                                                                                                     CancellationToken cancellationToken)
    {
        var resolved = await ResolveRenameDeclarationAsync(solution, documentId, annotation, cancellationToken).ConfigureAwait(false);

        if (resolved == null)
        {
            return null;
        }

        var declaredSymbol = resolved.Value.Symbol;

        if (HasNamespaceCollision(declaredSymbol, identifier, cancellationToken))
        {
            return null;
        }

        var cascade = await GetRenameCascadeAsync(solution, declaredSymbol, cancellationToken).ConfigureAwait(false);

        return (declaredSymbol, cascade);
    }

    /// <summary>
    /// Resolves an annotated declaration and its symbol in a solution
    /// </summary>
    /// <param name="solution">Solution containing the declaration</param>
    /// <param name="documentId">Document containing the declaration</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The declaration and symbol, or <see langword="null"/> when either cannot be resolved</returns>
    private static async Task<(T Node, ISymbol Symbol)?> ResolveRenameDeclarationAsync(Solution solution,
                                                                                       DocumentId documentId,
                                                                                       SyntaxAnnotation annotation,
                                                                                       CancellationToken cancellationToken)
    {
        var document = solution.GetDocument(documentId);
        var root = document == null
                       ? null
                       : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var model = document == null
                        ? null
                        : await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var node = root?.GetAnnotatedNodes(annotation).OfType<T>().SingleOrDefault();
        var symbol = model == null || node == null
                         ? null
                         : GetDeclaredSymbol(model, node, cancellationToken);

        return node == null || symbol == null
                   ? null
                   : (node, symbol);
    }

    /// <summary>
    /// Determines whether Roslyn attached a conflict annotation to any changed document
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a changed document contains a conflict annotation; otherwise, <see langword="false"/></returns>
    private static async Task<bool> HasConflictAnnotationsAsync(Solution originalSolution,
                                                                Solution renamedSolution,
                                                                CancellationToken cancellationToken)
    {
        foreach (var documentId in renamedSolution.GetChanges(originalSolution)
                                                  .GetProjectChanges()
                                                  .SelectMany(static changes => changes.GetChangedDocuments()))
        {
            var document = renamedSolution.GetDocument(documentId);
            var root = document == null
                           ? null
                           : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root?.GetAnnotatedNodesAndTokens(ConflictAnnotation.Kind).Any() == true)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to compute a valid replacement identifier for the specified node
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="identifier">The computed replacement identifier when the conversion succeeds</param>
    /// <returns>
    /// <see langword="true"/> if the conversion produced a valid identifier that differs from the original;
    /// otherwise, <see langword="false"/>
    /// </returns>
    private bool TryGetFixedIdentifier(T node, out string identifier)
    {
        identifier = null;

        string original;

        try
        {
            original = GetIdentifier(node);
            identifier = _casingConversion(original);
        }
        catch (Exception)
        {
            // A defective conversion must never surface as an unhandled exception inside the code action
            return false;
        }

        // The conversion has to yield a valid, non-empty identifier that actually changes the original name; otherwise
        // there is nothing to fix (for example letterless names such as "_" or "__")
        return string.IsNullOrEmpty(identifier) == false
               && string.Equals(identifier, original, StringComparison.Ordinal) == false
               && SyntaxFacts.IsValidIdentifier(identifier);
    }

    /// <summary>
    /// Applies every safe aggregate rename while refusing candidates that share a conflicting normalized target
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution containing every safe aggregate rename</returns>
    private async Task<Solution> ApplyFixAllAsync(FixAllContext context, CancellationToken cancellationToken)
    {
        var solution = context.Solution;
        var diagnostics = await GetFixAllDiagnosticsAsync(context).ConfigureAwait(false);
        var candidates = await GetFixAllCandidatesAsync(solution, diagnostics, cancellationToken).ConfigureAwait(false);

        if (candidates.IsEmpty)
        {
            return solution;
        }

        solution = await AnnotateFixAllCandidatesAsync(solution, candidates, cancellationToken).ConfigureAwait(false);

        var duplicateTargetCandidates = await GetDuplicateTargetCandidatesAsync(solution, candidates, cancellationToken).ConfigureAwait(false);

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (duplicateTargetCandidates.Any(duplicate => duplicate.Annotation == candidate.Annotation))
            {
                continue;
            }

            var resolvedCandidate = await ResolveFixAllCandidateAsync(solution, candidate, cancellationToken).ConfigureAwait(false);

            if (resolvedCandidate == null
                || CanRegisterCodeFix(resolvedCandidate.Value.Node) == false
                || TryGetFixedIdentifier(resolvedCandidate.Value.Node, out var identifier) == false
                || string.Equals(identifier, candidate.Identifier, StringComparison.Ordinal) == false)
            {
                continue;
            }

            var renamedSolution = await RenameWithoutConflictAsync(solution,
                                                                   candidate.DocumentId,
                                                                   candidate.Annotation,
                                                                   identifier,
                                                                   cancellationToken).ConfigureAwait(false);

            if (renamedSolution != null)
            {
                solution = renamedSolution;
            }
        }

        return solution;
    }

    /// <summary>
    /// Resolves aggregate diagnostics to supported declarations and their proposed identifiers
    /// </summary>
    /// <param name="solution">Solution containing the diagnostics</param>
    /// <param name="diagnostics">Diagnostics to resolve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Supported aggregate rename candidates in deterministic order</returns>
    private async Task<ImmutableArray<FixAllCandidate>> GetFixAllCandidatesAsync(Solution solution,
                                                                                 ImmutableArray<Diagnostic> diagnostics,
                                                                                 CancellationToken cancellationToken)
    {
        var candidates = ImmutableArray.CreateBuilder<FixAllCandidate>();
        var orderedLocations = diagnostics.Where(static diagnostic => diagnostic.Location.IsInSource)
                                          .Select(static diagnostic => diagnostic.Location)
                                          .OrderBy(location => GetDocumentSortKey(solution, location.SourceTree), StringComparer.Ordinal)
                                          .ThenBy(static location => location.SourceSpan.Start)
                                          .ThenBy(static location => location.SourceSpan.Length);

        foreach (var location in orderedLocations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sourceTree = location.SourceTree;
            var document = sourceTree == null
                               ? null
                               : solution.GetDocument(sourceTree);
            var root = document == null
                           ? null
                           : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = document == null
                            ? null
                            : await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (document == null
                || root == null
                || model == null
                || root.FindNode(location.SourceSpan) is not T node
                || CanRegisterCodeFix(node) == false
                || TryGetFixedIdentifier(node, out var identifier) == false)
            {
                continue;
            }

            var declaredSymbol = GetDeclaredSymbol(model, node, cancellationToken);

            if (declaredSymbol != null)
            {
                candidates.Add(new FixAllCandidate(document.Id,
                                                   location.SourceSpan,
                                                   new SyntaxAnnotation(nameof(FixAllCandidate), candidates.Count.ToString(CultureInfo.InvariantCulture)),
                                                   identifier,
                                                   declaredSymbol.ContainingSymbol));
            }
        }

        return candidates.ToImmutable();
    }

    #endregion // Methods

    #region Types

    /// <summary>
    /// Immutable aggregate rename candidate
    /// </summary>
#pragma warning disable RH2104 // The immutable candidate is private to the shared casing policy owner by design
    private readonly struct FixAllCandidate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="documentId">Declaration document</param>
        /// <param name="originalSpan">Original diagnostic span</param>
        /// <param name="annotation">Tracking annotation</param>
        /// <param name="identifier">Proposed identifier</param>
        /// <param name="containingSymbol">Symbol containing the declaration</param>
        public FixAllCandidate(DocumentId documentId,
                               TextSpan originalSpan,
                               SyntaxAnnotation annotation,
                               string identifier,
                               ISymbol containingSymbol)
        {
            DocumentId = documentId;
            OriginalSpan = originalSpan;
            Annotation = annotation;
            Identifier = identifier;
            ContainingSymbol = containingSymbol;
        }

        /// <summary>
        /// Declaration document
        /// </summary>
        public DocumentId DocumentId { get; }

        /// <summary>
        /// Original diagnostic span
        /// </summary>
        public TextSpan OriginalSpan { get; }

        /// <summary>
        /// Tracking annotation
        /// </summary>
        public SyntaxAnnotation Annotation { get; }

        /// <summary>
        /// Proposed identifier
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Symbol containing the declaration; candidates with one target are checked against each other only when they
        /// share it
        /// </summary>
        public ISymbol ContainingSymbol { get; }
    }
#pragma warning restore RH2104

#pragma warning disable RH2101 // The provider is private to the shared casing policy owner by design
    /// <summary>
    /// Fix All provider that composes casing renames against an evolving solution
    /// </summary>
    private sealed class CasingFixAllProvider : FixAllProvider
    {
        #region FixAllProvider

        /// <inheritdoc/>
        public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
        {
            return [FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution];
        }

        /// <inheritdoc/>
        public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
        {
            if (fixAllContext.CodeFixProvider is not CasingCodeFixProviderBase<T> provider)
            {
                return Task.FromResult<CodeAction>(null);
            }

            var equivalenceKey = fixAllContext.CodeActionEquivalenceKey ?? provider.GetType().Name;
            var action = CodeAction.Create(provider._title,
                                           cancellationToken => provider.ApplyFixAllAsync(fixAllContext.WithCancellationToken(cancellationToken), cancellationToken),
                                           equivalenceKey);

            return Task.FromResult(action);
        }

        #endregion // FixAllProvider
    }
#pragma warning restore RH2101

    #endregion // Types

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return new CasingFixAllProvider();
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                // The fix is only offered when the declared symbol resolves, so the rename covers the declaration and
                // every reference. A declaration-only rename would leave references stale and produce non-compiling code
                if (model == null
                    || root.FindNode(diagnosticSpan) is not T node
                    || CanRegisterCodeFix(node) == false
                    || TryGetFixedIdentifier(node, out var identifier) == false)
                {
                    continue;
                }

                var declaredSymbol = GetDeclaredSymbol(model, node, context.CancellationToken);

                if (declaredSymbol != null
                    && await HasRenameConflictAsync(context.Document, node, identifier, context.CancellationToken).ConfigureAwait(false) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, node, identifier, cancellationToken),
                                                              GetType().Name),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}