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
    /// Gets the declared symbol that the rename should target
    /// </summary>
    /// <param name="model">Semantic model</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// The declared symbol when a comprehensive rename of the declaration and all references is supported;
    /// otherwise, <see langword="null"/>
    /// </returns>
    private ISymbol GetDeclaredSymbol(SemanticModel model, T node, CancellationToken cancellationToken)
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
    private async Task<Solution> ApplyCodeFixAsync(Document document, T node, string identifier, CancellationToken cancellationToken)
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
    /// Gets the diagnostics covered by the requested Document, Project, or Solution Fix All scope
    /// </summary>
    /// <param name="context">Fix All context</param>
    /// <returns>Diagnostics in deterministic project and document order</returns>
    private async Task<ImmutableArray<Diagnostic>> GetFixAllDiagnosticsAsync(FixAllContext context)
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

    /// <summary>
    /// Gets a stable project, path, and document key for diagnostic ordering
    /// </summary>
    /// <param name="solution">Solution containing the diagnostic</param>
    /// <param name="sourceTree">Diagnostic source tree</param>
    /// <returns>Stable diagnostic document key</returns>
    private string GetDocumentSortKey(Solution solution, SyntaxTree sourceTree)
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
    private async Task<Solution> AnnotateFixAllCandidatesAsync(Solution solution,
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
    /// Finds candidates whose shared normalized target cannot coexist in the same declaration or binding domain
    /// </summary>
    /// <param name="solution">Annotated solution</param>
    /// <param name="candidates">Aggregate rename candidates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Candidates belonging to conflicting duplicate-target groups</returns>
    private async Task<ImmutableArray<FixAllCandidate>> GetDuplicateTargetCandidatesAsync(Solution solution,
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
    private async Task AddDuplicateTargetsForCandidateAsync(Solution solution,
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
    private void AddIfMissing(ImmutableArray<FixAllCandidate>.Builder candidates, FixAllCandidate candidate)
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
    private async Task<bool> CandidatesConflictAsync(Solution solution,
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
    private async Task<(T Node, ISymbol Symbol)?> ResolveFixAllCandidateAsync(Solution solution,
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
    /// Annotates every source declaration that owns a non-namespace target's binding domain
    /// </summary>
    /// <param name="solution">Solution containing the declaration</param>
    /// <param name="documentId">Target declaration document</param>
    /// <param name="declaredSymbol">Target declaration symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The annotated solution and binding-domain document/annotation pairs</returns>
    private async Task<(Solution Solution, ImmutableArray<(DocumentId DocumentId, SyntaxAnnotation Annotation)> Domains)> AnnotateBindingDomainsAsync(Solution solution,
                                                                                                                                                      DocumentId documentId,
                                                                                                                                                      ISymbol declaredSymbol,
                                                                                                                                                      CancellationToken cancellationToken)
    {
        var domains = ImmutableArray.CreateBuilder<(DocumentId DocumentId, SyntaxAnnotation Annotation)>();
        var containingDeclarations = declaredSymbol.ContainingSymbol == null
                                         ? []
                                         : declaredSymbol.ContainingSymbol.DeclaringSyntaxReferences
                                                         .OrderBy(reference => GetDocumentSortKey(solution, reference.SyntaxTree), StringComparer.Ordinal)
                                                         .ThenBy(static reference => reference.Span.Start)
                                                         .Select(reference => (DocumentId: solution.GetDocument(reference.SyntaxTree)?.Id, reference.Span))
                                                         .Where(static reference => reference.DocumentId != null)
                                                         .ToImmutableArray();

        foreach (var containingDeclaration in containingDeclarations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var domainDocument = solution.GetDocument(containingDeclaration.DocumentId);
            var domainRoot = domainDocument == null
                                 ? null
                                 : await domainDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var domainNode = domainRoot?.FindNode(containingDeclaration.Span, getInnermostNodeForTie: true);

            if (domainDocument == null
                || domainRoot == null
                || domainNode == null)
            {
                continue;
            }

            var annotation = new SyntaxAnnotation(nameof(AnnotateBindingDomainsAsync), domains.Count.ToString(CultureInfo.InvariantCulture));
            solution = domainDocument.WithSyntaxRoot(domainRoot.ReplaceNode(domainNode, domainNode.WithAdditionalAnnotations(annotation))).Project.Solution;
            domains.Add((domainDocument.Id, annotation));
        }

        if (domains.Count == 0)
        {
            var declarationDocument = solution.GetDocument(documentId);
            var declarationRoot = declarationDocument == null
                                      ? null
                                      : await declarationDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (declarationDocument != null
                && declarationRoot != null)
            {
                var annotation = new SyntaxAnnotation(nameof(AnnotateBindingDomainsAsync), "fallback");
                solution = declarationDocument.WithSyntaxRoot(declarationRoot.WithAdditionalAnnotations(annotation)).Project.Solution;
                domains.Add((declarationDocument.Id, annotation));
            }
        }

        return (solution, domains.ToImmutable());
    }

    /// <summary>
    /// Gets compiler errors from the tracked source declarations that own a non-namespace rename's binding domain
    /// </summary>
    /// <param name="solution">Current solution</param>
    /// <param name="domains">Binding-domain document/annotation pairs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Binding-domain compiler errors, or a default array when a tracked domain cannot be resolved</returns>
    private async Task<ImmutableArray<Diagnostic>> GetBindingDomainErrorsAsync(Solution solution,
                                                                               ImmutableArray<(DocumentId DocumentId, SyntaxAnnotation Annotation)> domains,
                                                                               CancellationToken cancellationToken)
    {
        var errors = ImmutableArray.CreateBuilder<Diagnostic>();

        foreach (var domain in domains)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var document = solution.GetDocument(domain.DocumentId);
            var root = document == null
                           ? null
                           : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var model = document == null
                            ? null
                            : await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var node = root?.GetAnnotatedNodes(domain.Annotation).SingleOrDefault();

            if (model == null
                || node == null)
            {
                return default;
            }

            errors.AddRange(model.GetDiagnostics(node.Span, cancellationToken)
                                 .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        }

        return errors.ToImmutable();
    }

    /// <summary>
    /// Determines whether a namespace-owned declaration's proposed name collides with an existing source member
    /// </summary>
    /// <param name="declaredSymbol">Declaration symbol</param>
    /// <param name="identifier">Proposed identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the proposed name collides; otherwise, <see langword="false"/></returns>
    private bool HasNamespaceCollision(ISymbol declaredSymbol, string identifier, CancellationToken cancellationToken)
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
    private bool IsNamespaceCollision(ISymbol member, ISymbol declaredSymbol, ImmutableHashSet<SyntaxTree> declaringTrees)
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
    /// Determines whether a comprehensive rename produces Roslyn conflict annotations or added declaration errors
    /// </summary>
    /// <param name="document">Document containing the declaration</param>
    /// <param name="node">Declaration node to rename</param>
    /// <param name="identifier">Proposed replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the rename would conflict; otherwise, <see langword="false"/></returns>
    private async Task<bool> HasRenameConflictAsync(Document document, T node, string identifier, CancellationToken cancellationToken)
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
    /// Applies a rename only when Roslyn produces no namespace member collision, conflict annotations, or added declaration errors
    /// </summary>
    /// <param name="solution">Solution containing the declaration</param>
    /// <param name="documentId">Declaration document</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="identifier">Proposed replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The renamed solution when conflict-free; otherwise, <see langword="null"/></returns>
    private async Task<Solution> RenameWithoutConflictAsync(Solution solution,
                                                            DocumentId documentId,
                                                            SyntaxAnnotation annotation,
                                                            string identifier,
                                                            CancellationToken cancellationToken)
    {
        var preparation = await PrepareRenameAsync(solution,
                                                   documentId,
                                                   annotation,
                                                   identifier,
                                                   cancellationToken).ConfigureAwait(false);

        if (preparation == null)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var prepared = preparation.Value;
        var renamedSolution = await Renamer.RenameSymbolAsync(prepared.Solution,
                                                              prepared.Symbol,
                                                              default,
                                                              identifier,
                                                              cancellationToken)
                                           .ConfigureAwait(false);

        if (await HasConflictAnnotationsAsync(prepared.Solution, renamedSolution, cancellationToken).ConfigureAwait(false)
            || await ResolveRenameDeclarationAsync(renamedSolution, documentId, annotation, cancellationToken).ConfigureAwait(false) == null)
        {
            return null;
        }

        if (prepared.HasNamespaceOwner == false
            && await HasAddedBindingErrorsAsync(renamedSolution,
                                                prepared.Domains,
                                                prepared.OriginalErrors,
                                                cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return renamedSolution;
    }

    /// <summary>
    /// Resolves and validates the declaration plus binding domains required before a rename
    /// </summary>
    /// <param name="solution">Annotated solution</param>
    /// <param name="documentId">Document containing the declaration</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="identifier">Proposed identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The prepared rename inputs, or <see langword="null"/> when the rename is unsafe</returns>
    private async Task<(Solution Solution,
    ISymbol Symbol,
    ImmutableArray<(DocumentId DocumentId, SyntaxAnnotation Annotation)> Domains,
    ImmutableArray<Diagnostic> OriginalErrors,
    bool HasNamespaceOwner)?> PrepareRenameAsync(Solution solution,
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

        if (declaredSymbol.ContainingSymbol is INamespaceSymbol)
        {
            return HasNamespaceCollision(declaredSymbol, identifier, cancellationToken)
                       ? null
                       : (solution,
                          declaredSymbol,
                          ImmutableArray<(DocumentId, SyntaxAnnotation)>.Empty,
                          ImmutableArray<Diagnostic>.Empty,
                          true);
        }

        var domainResult = await AnnotateBindingDomainsAsync(solution,
                                                             documentId,
                                                             declaredSymbol,
                                                             cancellationToken).ConfigureAwait(false);
        var updatedResolved = await ResolveRenameDeclarationAsync(domainResult.Solution,
                                                                  documentId,
                                                                  annotation,
                                                                  cancellationToken).ConfigureAwait(false);

        if (updatedResolved == null)
        {
            return null;
        }

        var originalErrors = await GetBindingDomainErrorsAsync(domainResult.Solution,
                                                               domainResult.Domains,
                                                               cancellationToken).ConfigureAwait(false);

        return originalErrors.IsDefault
                   ? null
                   : (domainResult.Solution, updatedResolved.Value.Symbol, domainResult.Domains, originalErrors, false);
    }

    /// <summary>
    /// Resolves an annotated declaration and its symbol in a solution
    /// </summary>
    /// <param name="solution">Solution containing the declaration</param>
    /// <param name="documentId">Document containing the declaration</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The declaration and symbol, or <see langword="null"/> when either cannot be resolved</returns>
    private async Task<(T Node, ISymbol Symbol)?> ResolveRenameDeclarationAsync(Solution solution,
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
    private async Task<bool> HasConflictAnnotationsAsync(Solution originalSolution,
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
    /// Determines whether a rename added errors within the declaration's binding domains
    /// </summary>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="domains">Annotated binding domains</param>
    /// <param name="originalErrors">Errors present before the rename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the rename added binding errors; otherwise, <see langword="false"/></returns>
    private async Task<bool> HasAddedBindingErrorsAsync(Solution renamedSolution,
                                                        ImmutableArray<(DocumentId DocumentId, SyntaxAnnotation Annotation)> domains,
                                                        ImmutableArray<Diagnostic> originalErrors,
                                                        CancellationToken cancellationToken)
    {
        var renamedErrors = await GetBindingDomainErrorsAsync(renamedSolution, domains, cancellationToken).ConfigureAwait(false);

        return renamedErrors.IsDefault
               || renamedErrors.GroupBy(static diagnostic => diagnostic.Id)
                               .Any(errorGroup => errorGroup.Count() > originalErrors.Count(diagnostic => string.Equals(diagnostic.Id, errorGroup.Key, StringComparison.Ordinal)));
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
        /// <param name="containingSymbol">Declaration or binding domain owner</param>
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
        /// Declaration or binding domain owner
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