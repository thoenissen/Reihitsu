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

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.CodeFixes.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Code fix provider for casing rules
/// </summary>
/// <typeparam name="T">Node type</typeparam>
public abstract class CasingCodeFixProviderBase<T> : ReihitsuCodeFixProviderBase
    where T : SyntaxNode
{
    #region Fields

    /// <summary>
    /// Analyzer of the rule whose diagnostics the provider fixes
    /// </summary>
    private readonly CasingAnalyzerBase _analyzer;

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
    /// <param name="analyzer">Analyzer of the rule whose diagnostics the provider fixes; it decides whether the rule accepts a derived name</param>
    /// <param name="title">Title</param>
    /// <param name="casingConversion">Casing conversion</param>
    protected CasingCodeFixProviderBase(CasingAnalyzerBase analyzer, string title, Func<string, string> casingConversion)
    {
        _analyzer = analyzer;
        _diagnosticId = analyzer.SupportedDiagnostics[0].Id;
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
    /// Applies a rename only when it is safe: no method name the compiler looks up implicitly, no namespace member
    /// collision, no name in excluded code or in a caller-argument expression attribute, no Roslyn conflict annotation,
    /// no new name written where it is a keyword, no rewritten or rebound reference to a symbol outside the rename
    /// cascade, no reference of the cascade rewritten beyond its name, no new name that a member of an interface in the
    /// type hierarchy of a cascade property, event, or ordinary method has, no changed native entry point, no changed
    /// document that holds code another build configuration hides, no added compile error, and no change to the
    /// properties the compiler declares for positional record parameters
    /// </summary>
    /// <param name="solution">Solution containing the declaration</param>
    /// <param name="documentId">Declaration document</param>
    /// <param name="annotation">Declaration tracking annotation</param>
    /// <param name="identifier">Proposed replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The renamed solution when it is safe; otherwise, <see langword="null"/></returns>
    /// <remarks>
    /// The checks that depend on what the rename reaches take it from <see cref="CasingRenameSafetyUtilities.GetRenameCascadeAsync"/>: the interface
    /// members in the type hierarchy of every cascade property, event, and ordinary method, the native entry points of every
    /// cascade member, the caller-argument expression attributes of every parameter list that
    /// declares a cascade parameter, the changes the renamer makes, which may lie only on identifiers that carry an
    /// old name, and the excluded code of every document that can refer to one, each searched for the new name and for
    /// the old names that the cascade before and after the rename gives. The compile
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

        // A new name that a member of an interface in the type hierarchy of a cascade property, event, or ordinary method
        // has may change which member implements it, which a rename may not do (SHARED-CASING-009)
        if (await CasingRenameSafetyUtilities.HasInterfaceMemberOfNameInHierarchyAsync(solution, prepared.Value.Cascade, identifier, cancellationToken).ConfigureAwait(false))
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

        // A new name that the rename writes where it is a keyword, with the verbatim prefix or without it, only looks
        // corrected or does not compile (SHARED-CASING-013)
        if (await CasingRenameSafetyUtilities.HasConflictAnnotationsAsync(solution, renamedSolution, cancellationToken).ConfigureAwait(false)
            || await CasingRenameSafetyUtilities.WritesNameAsKeywordAsync(solution, renamedSolution, identifier, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var renamedDeclaration = await ResolveRenameDeclarationAsync(renamedSolution, documentId, annotation, cancellationToken).ConfigureAwait(false);

        if (renamedDeclaration == null)
        {
            return null;
        }

        var renamedCascade = await CasingRenameSafetyUtilities.GetRenameCascadeAsync(renamedSolution, renamedDeclaration.Value.Symbol, cancellationToken).ConfigureAwait(false);
        var oldNames = CasingRenameSafetyUtilities.GetOldNames(prepared.Value.Cascade, renamedCascade);

        // The rename may change no binding of a reference to a symbol outside the cascade, whether the renamer rewrites
        // that reference or leaves it as written (SHARED-CASING-009)
        if (await CasingRenameSafetyUtilities.RewritesOtherCodeAsync(solution, renamedSolution, oldNames, cancellationToken).ConfigureAwait(false)
            || await CasingRenameSafetyUtilities.CountReferenceLocationsAsync(renamedSolution, renamedDeclaration.Value.Symbol, cancellationToken).ConfigureAwait(false)
               != await CasingRenameSafetyUtilities.CountReferenceLocationsAsync(solution, prepared.Value.Symbol, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        if (CasingRenameSafetyUtilities.GetNativeEntryPoints(prepared.Value.Cascade).SequenceEqual(CasingRenameSafetyUtilities.GetNativeEntryPoints(renamedCascade), StringComparer.Ordinal) == false)
        {
            return null;
        }

        var renamedNames = oldNames.Add(identifier);

        if (CasingRenameSafetyUtilities.HasCallerArgumentExpressionNaming(prepared.Value.Cascade, renamedNames, identifier, cancellationToken)
            || await ExcludedCodeUtilities.HasExcludedCodeOccurrenceAsync(solution, prepared.Value.Cascade, renamedNames, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        if (await CasingRenameSafetyUtilities.ChangesProjectsUnsafelyAsync(solution, renamedSolution, cancellationToken).ConfigureAwait(false))
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

        if (CasingRenameSafetyUtilities.IsCompilerLookupMethodRename(declaredSymbol, identifier)
            || CasingRenameSafetyUtilities.HasNamespaceCollision(declaredSymbol, identifier, cancellationToken))
        {
            return null;
        }

        var cascade = await CasingRenameSafetyUtilities.GetRenameCascadeAsync(solution, declaredSymbol, cancellationToken).ConfigureAwait(false);

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
    /// Tries to compute a valid replacement identifier for the specified node
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="identifier">The computed replacement identifier when the conversion succeeds</param>
    /// <returns>
    /// <see langword="true"/> if the conversion produced a valid identifier that differs from the original and that the
    /// rule accepts; otherwise, <see langword="false"/>
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

        // The conversion has to yield a valid, non-empty identifier that actually changes the original name, and the rule
        // must accept it. Otherwise there is nothing to fix (for example letterless names such as "_" or "__"), or the
        // fix would leave a name the same rule still reports (for example "_1st" for "__1st", or "I_" for an interface
        // named "_"), which one application does not correct
        return string.IsNullOrEmpty(identifier) == false
               && string.Equals(identifier, original, StringComparison.Ordinal) == false
               && SyntaxFacts.IsValidIdentifier(identifier)
               && _analyzer.IsValidName(identifier);
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

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
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

    #endregion // ReihitsuCodeFixProviderBase
}