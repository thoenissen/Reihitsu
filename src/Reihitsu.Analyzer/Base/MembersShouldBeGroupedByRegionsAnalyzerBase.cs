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
/// Shared base for analyzers that require members to be grouped into a region named after a related type
/// </summary>
public abstract class MembersShouldBeGroupedByRegionsAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    private protected MembersShouldBeGroupedByRegionsAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName)
        : base(diagnosticId, DiagnosticCategory.Organization, titleResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the names of the regions the member belongs in
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="symbol">Symbol the declaration declares; for a field-like event, the event of one of its declarators</param>
    /// <returns>The names, any one of which a matching region may carry, or no name when the member is not in scope for the analyzer</returns>
    protected abstract IReadOnlyList<string> GetExpectedRegionNames(MemberDeclarationSyntax memberDeclaration, ISymbol symbol);

    /// <summary>
    /// Determines whether the region that judges the member — the innermost region that encloses it, when it lies in a
    /// top-level region (SHARED-REGGROUP-001) — carries one of the expected names
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="expectedRegionNames">Expected region names</param>
    /// <param name="regions">Regions of the member's type declaration with their nesting depth</param>
    /// <returns><see langword="true"/> if the member is organized correctly</returns>
    private static bool HasMatchingRegion(MemberDeclarationSyntax memberDeclaration, IReadOnlyList<string> expectedRegionNames, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion, int NestingDepth)> regions)
    {
        if (RegionDirectiveUtilities.TryFindInnermostContainingRegion(memberDeclaration, regions, out var region) == false
            || region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        var regionName = RegionDirectiveUtilities.GetRegionDescription(regionDirective);

        return expectedRegionNames.Any(expectedRegionName => regionName.Equals(expectedRegionName, StringComparison.Ordinal));
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

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            // Each declarator of a field-like event is judged on its own (SHARED-REGGROUP-005)
            foreach (var (symbol, location) in PartialMemberUtilities.GetDeclaredSymbols(memberDeclaration, context.SemanticModel, context.CancellationToken))
            {
                var expectedRegionNames = GetExpectedRegionNames(memberDeclaration, symbol);

                if (expectedRegionNames.Count == 0
                    || HasMatchingRegion(memberDeclaration, expectedRegionNames, regions))
                {
                    continue;
                }

                // A partial member is one member and is reported once, on the first part outside a matching region (SHARED-DIAG-012)
                if (memberDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                    && PartialMemberUtilities.GetReportedPart(memberDeclaration,
                                                              symbol,
                                                              part => part.Parent is not TypeDeclarationSyntax partTypeDeclaration
                                                                      || HasMatchingRegion(part, expectedRegionNames, RegionDirectiveUtilities.GetTypeRegions(partTypeDeclaration)) == false,
                                                              context.SemanticModel.Compilation,
                                                              context.CancellationToken) != memberDeclaration)
                {
                    continue;
                }

                context.ReportDiagnostic(CreateDiagnostic(location, expectedRegionNames[0]));
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