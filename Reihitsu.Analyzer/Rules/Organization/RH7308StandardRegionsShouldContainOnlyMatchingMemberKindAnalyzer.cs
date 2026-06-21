using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7308: Standard regions should contain only their matching member kind
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer : DiagnosticAnalyzerBase<RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7308";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Maps a standard region label to the member kind it is expected to contain
    /// </summary>
    private static readonly Dictionary<string, string> _standardRegionKinds = new(StringComparer.OrdinalIgnoreCase)
                                                                              {
                                                                                  ["Fields"] = "field declarations",
                                                                                  ["Properties"] = "property declarations",
                                                                                  ["Methods"] = "method declarations",
                                                                                  ["Events"] = "event declarations",
                                                                                  ["Constructors"] = "constructor declarations"
                                                                              };

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7308Title), nameof(AnalyzerResources.RH7308MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the standard region label that matches the member kind, if any
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The matching standard region label, or <see cref="string.Empty"/> if the member is not a tracked kind</returns>
    private static string GetMemberRegionLabel(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   ConstructorDeclarationSyntax => "Constructors",
                   FieldDeclarationSyntax => "Fields",
                   PropertyDeclarationSyntax => "Properties",
                   MethodDeclarationSyntax => "Methods",
                   EventDeclarationSyntax or EventFieldDeclarationSyntax => "Events",
                   _ => string.Empty
               };
    }

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

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            var memberLabel = GetMemberRegionLabel(memberDeclaration);

            if (string.IsNullOrEmpty(memberLabel)
                || TryGetContainingRegionName(memberDeclaration, regions, out var regionName) == false
                || _standardRegionKinds.TryGetValue(regionName, out var expectedKind) == false
                || string.Equals(memberLabel, regionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(memberDeclaration), regionName, expectedKind));
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