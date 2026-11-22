using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for casing analyzers
/// </summary>
public abstract class CasingAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// The syntax kinds to analyze
    /// </summary>
    private readonly SyntaxKind[] _types;

    /// <summary>
    /// The function to validate the casing
    /// </summary>
    private readonly Func<string, bool> _casingValidation;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">The diagnostic ID</param>
    /// <param name="category">The diagnostic category</param>
    /// <param name="type">The syntax kind to analyze</param>
    /// <param name="casingValidation">The function to validate the casing</param>
    private protected CasingAnalyzerBase(string diagnosticId, DiagnosticCategory category, SyntaxKind type, Func<string, bool> casingValidation)
        : base(diagnosticId, category)
    {
        _types = [type];
        _casingValidation = casingValidation;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">The diagnostic ID</param>
    /// <param name="category">The diagnostic category</param>
    /// <param name="types">The syntax kinds to analyze</param>
    /// <param name="casingValidation">The function to validate the casing</param>
    private protected CasingAnalyzerBase(string diagnosticId, DiagnosticCategory category, SyntaxKind[] types, Func<string, bool> casingValidation)
        : base(diagnosticId, category)
    {
        _types = types;
        _casingValidation = casingValidation;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the rule accepts a name, that is, whether it reports no diagnostic for a declaration it checks
    /// that carries this name
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <returns><see langword="true"/> if the rule accepts the name; otherwise, <see langword="false"/></returns>
    internal bool IsValidName(string name)
    {
        return _casingValidation(name);
    }

    /// <summary>
    /// Gets the locations of the names to check
    /// </summary>
    /// <param name="node">Syntax node</param>
    /// <returns>The locations of the names to check</returns>
    protected abstract IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node);

    /// <summary>
    /// Determines whether a method, property, or event is dictated: it, or a member it overrides through any number of
    /// levels, overrides or implements a member declared outside its compilation (SHARED-CASING-014)
    /// </summary>
    /// <param name="member">The member</param>
    /// <returns><see langword="true"/> if the member is dictated; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A member implements an interface member only when its own type implements that interface, so only the interfaces
    /// of the member's containing type are searched; a member that implements an interface member only on behalf of a
    /// derived type is not dictated by it
    /// </remarks>
    private protected static bool IsDictatedMember(ISymbol member)
    {
        if (member is not (IMethodSymbol or IPropertySymbol or IEventSymbol))
        {
            return false;
        }

        var assembly = member.ContainingAssembly;

        for (var current = member; current != null; current = GetOverriddenMember(current))
        {
            var overridden = GetOverriddenMember(current);

            if ((overridden != null
                 && IsDeclaredOutside(overridden, assembly))
                || GetImplementedInterfaceMembers(current).Any(implemented => IsDeclaredOutside(implemented, assembly)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a method or local function binds to a native function whose name the runtime takes from the
    /// method's name: it is declared with <c>[DllImport]</c> or <c>[LibraryImport]</c> without an <c>EntryPoint</c>
    /// argument (SHARED-CASING-014)
    /// </summary>
    /// <param name="attributeLists">The attribute lists of the method or local function declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the native entry point is taken from the name; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The attribute is read from the declaration, because <c>[DllImport]</c> is a pseudo-attribute that the symbol does
    /// not list among its attributes, and whether an <c>EntryPoint</c> argument is written is a fact of the syntax, whatever
    /// its value
    /// </remarks>
    private protected static bool HasNativeEntryPointFromName(SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (var attribute in attributeLists.SelectMany(static attributeList => attributeList.Attributes))
        {
            if (semanticModel.GetSymbolInfo(attribute, cancellationToken).Symbol is IMethodSymbol { ContainingType: { Name: "DllImportAttribute" or "LibraryImportAttribute" } attributeType }
                && string.Equals(attributeType.ContainingNamespace?.ToDisplayString(), "System.Runtime.InteropServices", StringComparison.Ordinal))
            {
                return attribute.ArgumentList?.Arguments.Any(static argument => argument.NameEquals?.Name.Identifier.ValueText == "EntryPoint") != true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the name at a location is dictated by another compilation or by a native entry point, so the
    /// rule does not report it (SHARED-CASING-014)
    /// </summary>
    /// <param name="node">The analyzed syntax node</param>
    /// <param name="location">The location of the name, as <see cref="GetLocations"/> gives it</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the name is dictated; otherwise, <see langword="false"/></returns>
    private protected virtual bool IsDictatedName(SyntaxNode node, Location location, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return false;
    }

    /// <summary>
    /// Gets the member that a method, property, or event overrides
    /// </summary>
    /// <param name="member">The member</param>
    /// <returns>The overridden member, or <see langword="null"/> when the member overrides none</returns>
    private static ISymbol GetOverriddenMember(ISymbol member)
    {
        return member switch
               {
                   IMethodSymbol method => method.OverriddenMethod,
                   IPropertySymbol property => property.OverriddenProperty,
                   IEventSymbol @event => @event.OverriddenEvent,
                   _ => null
               };
    }

    /// <summary>
    /// Gets the interface members that a member implements, explicitly or implicitly, for its own containing type
    /// </summary>
    /// <param name="member">The member</param>
    /// <returns>The implemented interface members</returns>
    private static IEnumerable<ISymbol> GetImplementedInterfaceMembers(ISymbol member)
    {
        var explicitImplementations = member switch
                                      {
                                          IMethodSymbol method => method.ExplicitInterfaceImplementations.Cast<ISymbol>(),
                                          IPropertySymbol property => property.ExplicitInterfaceImplementations.Cast<ISymbol>(),
                                          IEventSymbol @event => @event.ExplicitInterfaceImplementations.Cast<ISymbol>(),
                                          _ => []
                                      };

        foreach (var implemented in explicitImplementations)
        {
            yield return implemented;
        }

        if (member.ContainingType is not { } containingType)
        {
            yield break;
        }

        var definition = PartialMemberUtilities.GetDefiningPartSymbol(member);

        foreach (var interfaceMember in containingType.AllInterfaces.SelectMany(interfaceType => interfaceType.GetMembers(member.Name)))
        {
            var implementation = containingType.FindImplementationForInterfaceMember(interfaceMember);

            if (implementation != null
                && SymbolEqualityComparer.Default.Equals(PartialMemberUtilities.GetDefiningPartSymbol(implementation), definition))
            {
                yield return interfaceMember;
            }
        }
    }

    /// <summary>
    /// Determines whether a symbol is declared outside the compilation of the given assembly
    /// </summary>
    /// <param name="symbol">The symbol</param>
    /// <param name="assembly">The assembly of the compilation</param>
    /// <returns><see langword="true"/> if the symbol is declared in another assembly; otherwise, <see langword="false"/></returns>
    private static bool IsDeclaredOutside(ISymbol symbol, IAssemblySymbol assembly)
    {
        return SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition.ContainingAssembly, assembly) == false;
    }

    /// <summary>
    /// Analyzes the node
    /// </summary>
    /// <param name="context">The syntax node analysis context</param>
    private void OnAnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        foreach (var (name, location) in GetLocations(context.Node))
        {
            CheckCasing(name, location, context);
        }
    }

    /// <summary>
    /// Checks the casing of the given name and reports a diagnostic if it's not valid and not dictated by another
    /// compilation or a native entry point
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="location">The location of the name in the source code</param>
    /// <param name="context">The syntax node analysis context</param>
    private void CheckCasing(string name, Location location, SyntaxNodeAnalysisContext context)
    {
        if (IsValidName(name) == false
            && IsDictatedName(context.Node, location, context.SemanticModel, context.CancellationToken) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAnalyzeNode, _types);
    }

    #endregion // DiagnosticAnalyzer
}