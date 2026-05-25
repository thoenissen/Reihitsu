using System;
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
/// <typeparam name="TAnalyzer">Concrete analyzer type</typeparam>
public abstract class NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    private protected NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName)
        : base(diagnosticId, DiagnosticCategory.Formatting, titleResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the type name that introduced the original method in the override chain
    /// </summary>
    /// <param name="methodSymbol">Method symbol</param>
    /// <returns>Containing type name of the first declaration in the chain</returns>
    protected static string GetOriginalDeclaringTypeName(IMethodSymbol methodSymbol)
    {
        while (methodSymbol.OverriddenMethod != null)
        {
            methodSymbol = methodSymbol.OverriddenMethod;
        }

        return methodSymbol.ContainingType.Name;
    }

    /// <summary>
    /// Gets the type name that introduced the original property or indexer in the override chain
    /// </summary>
    /// <param name="propertySymbol">Property symbol</param>
    /// <returns>Containing type name of the first declaration in the chain</returns>
    protected static string GetOriginalDeclaringTypeName(IPropertySymbol propertySymbol)
    {
        while (propertySymbol.OverriddenProperty != null)
        {
            propertySymbol = propertySymbol.OverriddenProperty;
        }

        return propertySymbol.ContainingType.Name;
    }

    /// <summary>
    /// Gets the type name that introduced the original event in the override chain
    /// </summary>
    /// <param name="eventSymbol">Event symbol</param>
    /// <returns>Containing type name of the first declaration in the chain</returns>
    protected static string GetOriginalDeclaringTypeName(IEventSymbol eventSymbol)
    {
        while (eventSymbol.OverriddenEvent != null)
        {
            eventSymbol = eventSymbol.OverriddenEvent;
        }

        return eventSymbol.ContainingType.Name;
    }

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

        var overrideRegionNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            if (TryGetOverrideRegionName(memberDeclaration, context.SemanticModel, context.CancellationToken, out var overrideRegionName))
            {
                overrideRegionNames.Add(overrideRegionName);
            }
        }

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