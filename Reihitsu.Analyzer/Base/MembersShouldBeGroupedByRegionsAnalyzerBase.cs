using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Shared base for analyzers that require members to be grouped into a top-level region named after a related type
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
    /// Tries to determine the expected region name for the member
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="expectedRegionName">Expected region name</param>
    /// <returns><see langword="true"/> if the member is in scope for the analyzer</returns>
    protected abstract bool TryGetExpectedRegionName(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out string expectedRegionName);

    /// <summary>
    /// Determines whether the member is inside a matching top-level region
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="expectedRegionName">Expected region name</param>
    /// <param name="regions">Top-level regions</param>
    /// <returns><see langword="true"/> if the member is organized correctly</returns>
    private static bool HasMatchingRegion(MemberDeclarationSyntax memberDeclaration, string expectedRegionName, IReadOnlyList<(SyntaxTrivia Region, SyntaxTrivia EndRegion)> regions)
    {
        if (RegionDirectiveUtilities.TryFindContainingRegion(memberDeclaration, regions, out var region) == false
            || region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
        {
            return false;
        }

        return RegionDirectiveUtilities.GetRegionDescription(regionDirective).Equals(expectedRegionName, StringComparison.Ordinal);
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

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            if (TryGetExpectedRegionName(memberDeclaration, context.SemanticModel, context.CancellationToken, out var expectedRegionName) == false
                || HasMatchingRegion(memberDeclaration, expectedRegionName, regions))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(memberDeclaration), expectedRegionName));
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