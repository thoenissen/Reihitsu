using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Checks of a rename that the casing code fixes run before they rename a symbol: whether the rename would change the
/// meaning, the compile result, or other code than the renamed declaration and its references. None of them depends on
/// the node type of a casing rule
/// </summary>
internal static class CasingRenameSafetyUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a rename changes the projects it reaches unsafely: a document it changes holds code that
    /// another build configuration hides (SHARED-FIX-012), or a project it changes, or one that depends on such a
    /// project, gains a compile error or a change to the properties the compiler declares for positional record parameters
    /// </summary>
    /// <param name="solution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the rename changes the projects unsafely; otherwise, <see langword="false"/></returns>
    internal static async Task<bool> ChangesProjectsUnsafelyAsync(Solution solution, Solution renamedSolution, CancellationToken cancellationToken)
    {
        var projectChanges = renamedSolution.GetChanges(solution).GetProjectChanges().ToList();

        // No document the rename changes may hold code that another build configuration hides (SHARED-FIX-012)
        foreach (var changedDocumentId in projectChanges.SelectMany(static changes => changes.GetChangedDocuments()))
        {
            var document = solution.GetDocument(changedDocumentId);

            if (document != null
                && await ReihitsuCodeFixProviderBase.HoldsHidingCarrierAsync(document, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }
        }

        var affectedProjectIds = ExcludedCodeUtilities.GetDependentProjectIds(solution, projectChanges.Select(static changes => changes.ProjectId));

        return await HasAddedCompileErrorsAsync(solution, renamedSolution, affectedProjectIds, cancellationToken).ConfigureAwait(false)
               || await ChangesPositionalPropertiesAsync(solution, renamedSolution, affectedProjectIds, cancellationToken).ConfigureAwait(false);
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
    internal static async Task<ImmutableArray<ISymbol>> GetRenameCascadeAsync(Solution solution, ISymbol symbol, CancellationToken cancellationToken)
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
    internal static ImmutableHashSet<string> GetOldNames(ImmutableArray<ISymbol> cascade, ImmutableArray<ISymbol> renamedCascade)
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
    /// Gets the names of the native functions that the methods of a rename cascade bind to through an import attribute
    /// </summary>
    /// <param name="cascade">Symbols the rename renames</param>
    /// <returns>The entry point names in ordinal order</returns>
    /// <remarks>
    /// A rename changes no binding of a cascade member other than through its name, so the cascade binds to the same
    /// native functions after the rename exactly when the two ordered lists are equal
    /// </remarks>
    internal static ImmutableArray<string> GetNativeEntryPoints(ImmutableArray<ISymbol> cascade)
    {
        return cascade.Select(GetNativeEntryPoint)
                      .Where(static entryPoint => entryPoint != null)
                      .OrderBy(static entryPoint => entryPoint, StringComparer.Ordinal)
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
    internal static async Task<bool> HasAddedCompileErrorsAsync(Solution originalSolution,
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
    /// Determines whether a rename changes which positional parameters of a record get a compiler-declared property
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="projectIds">Projects the rename affects</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a public property would appear or disappear; otherwise, <see langword="false"/></returns>
    internal static async Task<bool> ChangesPositionalPropertiesAsync(Solution originalSolution,
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
    internal static bool HasNamespaceCollision(ISymbol declaredSymbol, string identifier, CancellationToken cancellationToken)
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
    /// Determines whether a rename would give a method that is not a local function a name the compiler looks up for a
    /// construct that does not write it in source
    /// </summary>
    /// <param name="declaredSymbol">Symbol to rename</param>
    /// <param name="identifier">Proposed identifier</param>
    /// <returns><see langword="true"/> when the rename is withheld for the name; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A method renamed to such a name can win the compiler's lookup over the method a construct called before, without a
    /// compile error and without a reference the renamer could see. The rename is withheld for every such name, whether
    /// or not a construct would call another method, because telling the two apart would need comparing every such
    /// construct before and after the rename. A local function is never found by such a lookup
    /// </remarks>
    internal static bool IsCompilerLookupMethodRename(ISymbol declaredSymbol, string identifier)
    {
        // The names that foreach, await foreach, query clauses, await, deconstruction, collection initializers, using,
        // fixed, and interpolated-string handlers look up
        return declaredSymbol is IMethodSymbol { MethodKind: not MethodKind.LocalFunction }
               && identifier is "Add"
                             or "AppendFormatted"
                             or "AppendLiteral"
                             or "Cast"
                             or "Deconstruct"
                             or "Dispose"
                             or "DisposeAsync"
                             or "GetAsyncEnumerator"
                             or "GetAwaiter"
                             or "GetEnumerator"
                             or "GetPinnableReference"
                             or "GroupBy"
                             or "GroupJoin"
                             or "Join"
                             or "OrderBy"
                             or "OrderByDescending"
                             or "Select"
                             or "SelectMany"
                             or "ThenBy"
                             or "ThenByDescending"
                             or "Where";
    }

    /// <summary>
    /// Determines whether a <c>CallerArgumentExpression</c> attribute in a parameter list names one of the specified
    /// names by an argument that is not exempt
    /// </summary>
    /// <param name="parameters">Parameter list</param>
    /// <param name="names">Old names of the cascade and the new name</param>
    /// <param name="exemptNames">Identifiers of a <see langword="nameof"/> argument that exempt it</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when such an attribute names one of the names; otherwise, <see langword="false"/></returns>
    internal static bool HasCallerArgumentExpressionNaming(ImmutableArray<IParameterSymbol> parameters, ImmutableHashSet<string> names, ImmutableHashSet<string> exemptNames, CancellationToken cancellationToken)
    {
        foreach (var parameter in parameters)
        {
            foreach (var attribute in parameter.GetAttributes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (attribute.AttributeClass is { Name: "CallerArgumentExpressionAttribute" } attributeClass
                    && string.Equals(attributeClass.ContainingNamespace?.ToDisplayString(), "System.Runtime.CompilerServices", StringComparison.Ordinal)
                    && attribute.ConstructorArguments.Length == 1
                    && attribute.ConstructorArguments[0].Value is string name
                    && names.Contains(name)
                    && IsExemptNameofArgument(attribute, exemptNames, cancellationToken) == false)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a <c>CallerArgumentExpression</c> attribute in a parameter list that the rename cascade
    /// reaches names one of the specified names by an argument that is not exempt
    /// </summary>
    /// <param name="cascade">Rename cascade before the rename</param>
    /// <param name="names">Old names of the cascade and the new name</param>
    /// <param name="identifier">New name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when such an attribute names one of the names; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The inspected lists are every parameter list that declares a parameter of the cascade, and the parameter lists of
    /// the members of an extension block whose receiver parameter is in the cascade. The attribute names a parameter by
    /// the value of its argument, which the renamer changes only for a <see langword="nameof"/> that refers to a renamed
    /// parameter, so every other argument naming an old or the new name would make the parameter that carries it receive
    /// another value at run time. Only a <see langword="nameof"/> of a single unqualified identifier that is the old name
    /// of a cascade parameter the same list declares, in C# 11 or later, is exempt; an extension block member's list
    /// declares no cascade parameter and so exempts nothing
    /// </remarks>
    internal static bool HasCallerArgumentExpressionNaming(ImmutableArray<ISymbol> cascade, ImmutableHashSet<string> names, string identifier, CancellationToken cancellationToken)
    {
        var cascadeParameters = cascade.OfType<IParameterSymbol>().ToList();

        foreach (var owner in cascadeParameters.Select(static parameter => parameter.ContainingSymbol).Distinct(SymbolEqualityComparer.Default))
        {
            var exemptNames = cascadeParameters.Where(parameter => SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, owner)
                                                                   && string.Equals(parameter.Name, identifier, StringComparison.Ordinal) == false)
                                               .Select(static parameter => parameter.Name)
                                               .ToImmutableHashSet(StringComparer.Ordinal);

            if (HasCallerArgumentExpressionNaming(GetParameters(owner), names, exemptNames, cancellationToken)
                || (owner is INamedTypeSymbol { IsExtension: true } extension
                    && extension.GetMembers().Any(member => HasCallerArgumentExpressionNaming(GetParameters(member), names, ImmutableHashSet<string>.Empty, cancellationToken))))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a rename changed code other than the references of the rename cascade
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="oldNames">Old names of the rename, as <see cref="GetOldNames"/> gives them</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a change reaches a token that is no reference of the cascade; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The renamer keeps the binding of a reference to another symbol that the new name would capture by rewriting
    /// that reference, such as qualifying it, turning an extension call into a static call, or casting an argument,
    /// without reporting a conflict. Renaming the cascade only replaces, or qualifies, identifiers that carry an old
    /// name, so every change has to lie on such identifiers: a replaced span may only overlap them, and an insertion
    /// has to touch one of them. A name the cascade still carries after the rename is no old name, so a reference to
    /// another symbol spelled that way, such as a field spelled like the parameter of the other part of a partial
    /// method, does not count as a reference of the cascade
    /// </remarks>
    internal static async Task<bool> RewritesOtherCodeAsync(Solution originalSolution,
                                                            Solution renamedSolution,
                                                            ImmutableHashSet<string> oldNames,
                                                            CancellationToken cancellationToken)
    {
        foreach (var documentId in renamedSolution.GetChanges(originalSolution)
                                                  .GetProjectChanges()
                                                  .SelectMany(static changes => changes.GetChangedDocuments()))
        {
            var originalTree = await originalSolution.GetDocument(documentId).GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var renamedTree = await renamedSolution.GetDocument(documentId).GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var originalRoot = await originalTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            foreach (var span in renamedTree.GetChanges(originalTree).Select(static change => change.Span))
            {
                var searchSpan = TextSpan.FromBounds(Math.Max(0, span.Start - 1), Math.Min(originalRoot.FullSpan.End, span.End + 1));
                var tokens = originalRoot.DescendantTokens(searchSpan, descendIntoTrivia: true)
                                         .Where(token => span.IsEmpty
                                                             ? token.Span.Start <= span.Start && span.Start <= token.Span.End
                                                             : token.Span.OverlapsWith(span))
                                         .ToList();
                var cascadeReferenceCount = tokens.Count(token => token.IsKind(SyntaxKind.IdentifierToken) && oldNames.Contains(token.ValueText));

                if (cascadeReferenceCount == 0
                    || (span.IsEmpty == false && cascadeReferenceCount != tokens.Count))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Counts the source locations that refer to a symbol or to another symbol of its rename cascade
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="symbol">Symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of distinct reference locations</returns>
    /// <remarks>
    /// A reference to another symbol that the renamer leaves as written can bind to the renamed symbol afterwards, for
    /// example a member access whose receiver has the renaming type when the renamed member now hides an inherited one.
    /// Such a reference adds a location, so a rename that keeps every other binding keeps the count
    /// </remarks>
    internal static async Task<int> CountReferenceLocationsAsync(Solution solution, ISymbol symbol, CancellationToken cancellationToken)
    {
        var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false);

        return referencedSymbols.SelectMany(static referencedSymbol => referencedSymbol.Locations)
                                .Select(static location => (location.Document.Id, location.Location.SourceSpan))
                                .Distinct()
                                .Count();
    }

    /// <summary>
    /// Determines whether a member of an interface in the type hierarchy of a property, event, or ordinary method of the
    /// rename cascade has the new name
    /// </summary>
    /// <param name="solution">Solution before the rename</param>
    /// <param name="cascade">Rename cascade before the rename</param>
    /// <param name="identifier">Proposed replacement identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when such an interface member has the new name; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Only the kinds that can implement an interface member by their name are checked: properties, events, and ordinary
    /// methods. Every other symbol of the cascade — a constructor, finalizer, operator, accessor, explicit interface
    /// implementation of a method, local function, field, parameter, type, or type parameter — adds no hierarchy, so a
    /// type rename whose cascade holds its constructors is not refused, while a positional record parameter is refused
    /// through the property in its cascade. For each checked member, the hierarchy is its containing type and every type
    /// of the solution derived from it; for an interface, every type that implements it and every interface that
    /// inherits from it. Which member implements an interface member depends on all their interface mappings, so the
    /// check refuses by name instead of comparing the mappings
    /// </remarks>
    internal static async Task<bool> HasInterfaceMemberOfNameInHierarchyAsync(Solution solution,
                                                                              ImmutableArray<ISymbol> cascade,
                                                                              string identifier,
                                                                              CancellationToken cancellationToken)
    {
        foreach (var symbol in cascade)
        {
            if (symbol is not (IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })
                || symbol.ContainingType is not { } containingType)
            {
                continue;
            }

            IEnumerable<INamedTypeSymbol> types = [containingType];

            if (containingType.TypeKind == TypeKind.Interface)
            {
                types = types.Concat(await SymbolFinder.FindImplementationsAsync(containingType, solution, cancellationToken: cancellationToken).ConfigureAwait(false))
                             .Concat(await SymbolFinder.FindDerivedInterfacesAsync(containingType, solution, cancellationToken: cancellationToken).ConfigureAwait(false));
            }
            else if (containingType.TypeKind == TypeKind.Class)
            {
                types = types.Concat(await SymbolFinder.FindDerivedClassesAsync(containingType, solution, cancellationToken: cancellationToken).ConfigureAwait(false));
            }

            if (types.SelectMany(static type => type.AllInterfaces)
                     .Any(interfaceType => interfaceType.GetMembers(identifier).IsEmpty == false))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the rename writes the new name where it is a keyword: with the verbatim prefix <c>@</c> at the
    /// declaration or at a reference, or without it where the written text would read it as a keyword
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="identifier">New name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a changed document holds more identifiers spelled <c>@</c> and the new name after the rename than before it, or its text reads such an identifier spelled as the new name as a keyword; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The renamer escapes the new name wherever it is a keyword at that position: a reserved keyword everywhere, and a
    /// contextual keyword only where it is a keyword, such as <c>await</c> inside an async method or a name inside a query
    /// expression. Where it leaves such a name unescaped, as for <c>await</c> inside an async local function, the renamed
    /// tree holds an identifier that its own text reads as a keyword when it is parsed again.
    /// Reading the renamer's result instead of a keyword list decides every such position alike (SHARED-CASING-013)
    /// </remarks>
    internal static async Task<bool> WritesNameAsKeywordAsync(Solution originalSolution,
                                                              Solution renamedSolution,
                                                              string identifier,
                                                              CancellationToken cancellationToken)
    {
        var verbatimText = "@" + identifier;

        foreach (var documentId in renamedSolution.GetChanges(originalSolution)
                                                  .GetProjectChanges()
                                                  .SelectMany(static changes => changes.GetChangedDocuments()))
        {
            var originalRoot = await originalSolution.GetDocument(documentId).GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var renamedTree = await renamedSolution.GetDocument(documentId).GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var renamedRoot = await renamedTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            if (CountVerbatimIdentifiers(renamedRoot, verbatimText) > CountVerbatimIdentifiers(originalRoot, verbatimText))
            {
                return true;
            }

            var renamedText = await renamedTree.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var reparsedRoot = await CSharpSyntaxTree.ParseText(renamedText, renamedTree.Options as CSharpParseOptions, renamedTree.FilePath, cancellationToken)
                                                     .GetRootAsync(cancellationToken)
                                                     .ConfigureAwait(false);

            // An identifier the renamer wrote as the bare new name has to stay an identifier when its text is read again
            if (renamedRoot.DescendantTokens()
                           .Where(token => token.IsKind(SyntaxKind.IdentifierToken)
                                           && string.Equals(token.Text, identifier, StringComparison.Ordinal))
                           .Any(token => reparsedRoot.FindToken(token.SpanStart).IsKind(SyntaxKind.IdentifierToken) == false))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether Roslyn attached a conflict annotation to any changed document
    /// </summary>
    /// <param name="originalSolution">Solution before the rename</param>
    /// <param name="renamedSolution">Solution after the rename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a changed document contains a conflict annotation; otherwise, <see langword="false"/></returns>
    internal static async Task<bool> HasConflictAnnotationsAsync(Solution originalSolution,
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
    /// Counts the identifier tokens of a syntax root that are spelled exactly as the given verbatim text
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="verbatimText">Identifier text with the verbatim prefix</param>
    /// <returns>The number of such identifier tokens</returns>
    private static int CountVerbatimIdentifiers(SyntaxNode root, string verbatimText)
    {
        return root == null
                   ? 0
                   : root.DescendantTokens().Count(token => token.IsKind(SyntaxKind.IdentifierToken)
                                                            && string.Equals(token.Text, verbatimText, StringComparison.Ordinal));
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
                    flags.Add(RecordPositionalParameterUtilities.GetDeclaredProperty(recordType, parameter) != null);
                }
            }
        }

        return flags.ToImmutable();
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
    /// Gets the parameter list of a method, a property, or an extension block
    /// </summary>
    /// <param name="member">Member</param>
    /// <returns>The member's parameters, the receiver parameter of an extension block, or an empty list for any other member</returns>
    private static ImmutableArray<IParameterSymbol> GetParameters(ISymbol member)
    {
        return member switch
               {
                   IMethodSymbol method => method.Parameters,
                   IPropertySymbol property => property.Parameters,
                   INamedTypeSymbol { IsExtension: true, ExtensionParameter: { } receiver } => ImmutableArray.Create(receiver),
                   _ => ImmutableArray<IParameterSymbol>.Empty
               };
    }

    /// <summary>
    /// Determines whether the single argument of an attribute is a <see langword="nameof"/> expression whose operand is
    /// a single unqualified identifier from the specified names, in a document compiled with C# 11 or later
    /// </summary>
    /// <param name="attribute">Attribute</param>
    /// <param name="exemptNames">Identifiers that exempt the argument</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the argument is exempt; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The check reads only the shape of the argument and the document's language version, never what the
    /// <see langword="nameof"/> operand binds to
    /// </remarks>
    private static bool IsExemptNameofArgument(AttributeData attribute, ImmutableHashSet<string> exemptNames, CancellationToken cancellationToken)
    {
        return exemptNames.IsEmpty == false
               && attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is AttributeSyntax { ArgumentList.Arguments.Count: 1 } attributeSyntax
               && attributeSyntax.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp11 }
               && attributeSyntax.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" }, ArgumentList.Arguments.Count: 1 } nameofExpression
               && nameofExpression.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax operand
               && exemptNames.Contains(operand.Identifier.ValueText);
    }

    #endregion // Methods
}