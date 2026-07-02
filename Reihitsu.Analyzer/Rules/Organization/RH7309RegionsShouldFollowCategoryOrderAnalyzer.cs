using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7309: Regions should follow a fixed category order
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7309RegionsShouldFollowCategoryOrderAnalyzer : DiagnosticAnalyzerBase<RH7309RegionsShouldFollowCategoryOrderAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7309";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7309RegionsShouldFollowCategoryOrderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7309Title), nameof(AnalyzerResources.RH7309MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

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

        if (regions.Count < 2)
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var highestCategory = RegionCategory.Custom;

        foreach (var region in regions)
        {
            if (region.Region.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective)
            {
                continue;
            }

            var description = RegionDirectiveUtilities.GetRegionDescription(regionDirective);
            var category = RegionCategoryUtilities.GetRegionCategory(typeSymbol, description);

            if (category < highestCategory)
            {
                context.ReportDiagnostic(CreateDiagnostic(regionDirective.GetLocation(), description));
            }
            else
            {
                highestCategory = category;
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
                                         SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}