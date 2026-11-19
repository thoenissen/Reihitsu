using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Shared base for analyzers that prevent non-override members from being placed in override regions
/// </summary>
public abstract class NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    private protected NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase(string diagnosticId)
        : base(diagnosticId, DiagnosticCategory.Organization)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the member is a non-override member checked by this analyzer
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="symbol">Symbol the declaration declares; for a field-like event, the event of one of its declarators</param>
    /// <returns><see langword="true"/> if the member should be checked</returns>
    protected abstract bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, ISymbol symbol);

    /// <summary>
    /// Tries to get the name of the region that judges the member: the innermost region that encloses it, when the member
    /// lies in a top-level region (SHARED-REGGROUP-001)
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Regions of the member's type declaration with their nesting depth</param>
    /// <param name="regionName">Name of the innermost enclosing region</param>
    /// <returns><see langword="true"/> if the member lies in a top-level region and its innermost region has a name</returns>
    private static bool TryGetContainingRegionName(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion, int NestingDepth)> regions, out string regionName)
    {
        regionName = string.Empty;

        if (RegionDirectiveUtilities.TryFindInnermostContainingRegion(memberDeclaration, regions, out var region) == false
            || region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        regionName = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

        return string.IsNullOrEmpty(regionName) == false;
    }

    /// <summary>
    /// Gets the names of the override regions of a type (SHARED-REGGROUP-006): the names of the types that introduced a
    /// member that any declaration of the type overrides, whatever the member's kind
    /// </summary>
    /// <param name="typeSymbol">Type symbol</param>
    /// <returns>The region names</returns>
    private static HashSet<string> GetOverrideRegionNames(INamedTypeSymbol typeSymbol)
    {
        var overrideRegionNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member.IsImplicitlyDeclared
                || member is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
            {
                continue;
            }

            overrideRegionNames.UnionWith(OverrideMemberUtilities.GetOverrideRegionNames(member));
        }

        return overrideRegionNames;
    }

    /// <summary>
    /// Determines whether a part of a partial member lies in an override region of the type that holds it
    /// </summary>
    /// <param name="part">Part of the partial member</param>
    /// <param name="overrideRegionNames">Override region names of the type</param>
    /// <returns><see langword="true"/> when the part lies in an override region</returns>
    private static bool LiesInOverrideRegion(MemberDeclarationSyntax part, HashSet<string> overrideRegionNames)
    {
        return part.Parent is TypeDeclarationSyntax partTypeDeclaration
               && TryGetContainingRegionName(part, RegionDirectiveUtilities.GetTypeRegions(partTypeDeclaration), out var regionName)
               && overrideRegionNames.Contains(regionName);
    }

    /// <summary>
    /// Analyzes the type declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var regions = RegionDirectiveUtilities.GetTypeRegions(typeDeclaration);

        if (regions.Count == 0
            || context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var overrideRegionNames = GetOverrideRegionNames(typeSymbol);

        if (overrideRegionNames.Count == 0)
        {
            return;
        }

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            if (TryGetContainingRegionName(memberDeclaration, regions, out var regionName) == false
                || overrideRegionNames.Contains(regionName) == false)
            {
                continue;
            }

            // Each declarator of a field-like event is judged on its own (SHARED-REGGROUP-005)
            foreach (var (symbol, location) in PartialMemberUtilities.GetDeclaredSymbols(memberDeclaration, context.SemanticModel, context.CancellationToken))
            {
                if (IsRelevantNonOverrideMember(memberDeclaration, symbol) == false)
                {
                    continue;
                }

                // A partial member is one member and is reported once, on the first part in an override region (SHARED-DIAG-012)
                if (memberDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                    && PartialMemberUtilities.GetReportedPart(memberDeclaration,
                                                              symbol,
                                                              part => LiesInOverrideRegion(part, overrideRegionNames),
                                                              context.SemanticModel.Compilation,
                                                              context.CancellationToken) != memberDeclaration)
                {
                    continue;
                }

                context.ReportDiagnostic(CreateDiagnostic(location, regionName));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnTypeDeclaration,
                                         SyntaxKind.ClassDeclaration,
                                         SyntaxKind.StructDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration,
                                         SyntaxKind.UnionDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}