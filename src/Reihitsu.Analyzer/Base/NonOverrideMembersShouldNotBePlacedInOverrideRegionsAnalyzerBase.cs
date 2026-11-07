using System.Collections.Generic;
using System.Threading;

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
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    private protected NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName)
        : base(diagnosticId, DiagnosticCategory.Organization, titleResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Tries to determine the expected override region name for the member
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="overrideRegionName">Expected override region name</param>
    /// <returns><see langword="true"/> if the member is an override of the analyzer's target kind</returns>
    protected abstract bool TryGetOverrideRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string overrideRegionName);

    /// <summary>
    /// Determines whether the member is a non-override member checked by this analyzer
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the member should be checked</returns>
    protected abstract bool IsRelevantNonOverrideMember(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken);

    /// <summary>
    /// Tries to get the containing top-level region name for the member
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="regions">Top-level regions</param>
    /// <param name="regionName">Containing region name</param>
    /// <returns><see langword="true"/> if a containing top-level region exists</returns>
    private static bool TryGetContainingRegionName(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions, out string regionName)
    {
        regionName = string.Empty;

        if (RegionDirectiveUtilities.TryFindContainingRegion(memberDeclaration, regions, out var region) == false
            || region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        regionName = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

        return string.IsNullOrEmpty(regionName) == false;
    }

    /// <summary>
    /// Gets the names of the override regions of a type declaration: the regions named after the type that introduced
    /// a member the declaration overrides
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="semanticModel">Semantic model of the declaration's tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The region names</returns>
    private HashSet<string> GetOverrideRegionNames(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var overrideRegionNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            if (TryGetOverrideRegionName(memberDeclaration, semanticModel, cancellationToken, out var overrideRegionName))
            {
                overrideRegionNames.Add(overrideRegionName);
            }
        }

        return overrideRegionNames;
    }

    /// <summary>
    /// Determines whether a part of a partial member lies in an override region of the type declaration that holds it
    /// </summary>
    /// <param name="part">Part of the partial member</param>
    /// <param name="analyzedTypeDeclaration">Type declaration being analyzed</param>
    /// <param name="analyzedOverrideRegionNames">Override region names of the analyzed type declaration</param>
    /// <param name="semanticModel">Semantic model of the analyzed type declaration's tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// <see langword="true"/> when the part lies in an override region, and for a part in another syntax tree, whose
    /// override regions this analysis cannot bind, so that the part counts as reported as every part of a covered member is
    /// </returns>
    private bool LiesInOverrideRegion(MemberDeclarationSyntax part,
                                      TypeDeclarationSyntax analyzedTypeDeclaration,
                                      HashSet<string> analyzedOverrideRegionNames,
                                      SemanticModel semanticModel,
                                      CancellationToken cancellationToken)
    {
        if (part.Parent is not TypeDeclarationSyntax partTypeDeclaration)
        {
            return false;
        }

        if (partTypeDeclaration.SyntaxTree != semanticModel.SyntaxTree)
        {
            return true;
        }

        var overrideRegionNames = partTypeDeclaration == analyzedTypeDeclaration
                                      ? analyzedOverrideRegionNames
                                      : GetOverrideRegionNames(partTypeDeclaration, semanticModel, cancellationToken);

        return TryGetContainingRegionName(part, RegionDirectiveUtilities.GetTopLevelRegions(partTypeDeclaration), out var regionName)
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

        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        if (regions.Count == 0)
        {
            return;
        }

        var overrideRegionNames = GetOverrideRegionNames(typeDeclaration, context.SemanticModel, context.CancellationToken);

        if (overrideRegionNames.Count == 0)
        {
            return;
        }

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            if (IsRelevantNonOverrideMember(memberDeclaration, context.SemanticModel, context.CancellationToken) == false
                || TryGetContainingRegionName(memberDeclaration, regions, out var regionName) == false
                || overrideRegionNames.Contains(regionName) == false)
            {
                continue;
            }

            // A partial member is one member and is reported once, on the first part in an override region (SHARED-DIAG-012)
            if (memberDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                && PartialMemberUtilities.GetReportedPart(memberDeclaration,
                                                          PartialMemberUtilities.GetDeclaredSymbol(memberDeclaration, context.SemanticModel, context.CancellationToken),
                                                          part => LiesInOverrideRegion(part, typeDeclaration, overrideRegionNames, context.SemanticModel, context.CancellationToken),
                                                          context.SemanticModel.Compilation,
                                                          context.CancellationToken) != memberDeclaration)
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(memberDeclaration), regionName));
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
                                         SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}