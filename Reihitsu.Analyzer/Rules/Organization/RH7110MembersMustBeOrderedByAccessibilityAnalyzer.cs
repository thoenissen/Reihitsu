using System.Collections.Generic;

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
/// RH7110: Members must be ordered by accessibility
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7110MembersMustBeOrderedByAccessibilityAnalyzer : DiagnosticAnalyzerBase<RH7110MembersMustBeOrderedByAccessibilityAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7110";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7110MembersMustBeOrderedByAccessibilityAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7110Title), nameof(AnalyzerResources.RH7110MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the type declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);
        var narrowestSeenByGroup = new Dictionary<(int RegionIndex, OrderingMemberKindGroup MemberKind), OrderingAccessibilityGroup>();

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            var accessibilityGroup = OrderingDeclarationUtilities.GetAccessibilityGroup(memberDeclaration);

            if (accessibilityGroup == OrderingAccessibilityGroup.None)
            {
                continue;
            }

            var group = (RegionDirectiveUtilities.GetContainingRegionIndex(memberDeclaration, regions),
                         OrderingDeclarationUtilities.GetMemberKind(memberDeclaration));

            if (narrowestSeenByGroup.TryGetValue(group, out var narrowestSeen)
                && accessibilityGroup < narrowestSeen)
            {
                context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(memberDeclaration)));

                continue;
            }

            narrowestSeenByGroup[group] = accessibilityGroup;
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
                                         SyntaxKind.InterfaceDeclaration,
                                         SyntaxKind.RecordDeclaration,
                                         SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}