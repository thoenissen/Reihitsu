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
/// RH7102: Constants must appear before fields
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7102ConstantsMustAppearBeforeFieldsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7102";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7102ConstantsMustAppearBeforeFieldsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization)
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
        var seenNonConstantAccessibilityGroups = new HashSet<(int RegionIndex, OrderingAccessibilityGroup AccessibilityGroup)>();

        foreach (var fieldDeclaration in typeDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            // A field is compared only within its own top-level region, or outside every region (RH7102-003)
            var accessibilityGroup = (RegionDirectiveUtilities.GetContainingRegionIndex(fieldDeclaration, regions),
                                      OrderingDeclarationUtilities.GetAccessibilityGroup(fieldDeclaration));

            if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                // The modifiers are judged as the analyzed build compiles them, also when a conditional directive
                // splits them; the fix withholds itself there (RH7102-001, SHARED-DIAG-008)
                if (seenNonConstantAccessibilityGroups.Contains(accessibilityGroup))
                {
                    context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(fieldDeclaration)));
                }

                continue;
            }

            seenNonConstantAccessibilityGroups.Add(accessibilityGroup);
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
                                         SyntaxKind.RecordStructDeclaration,
                                         SyntaxKind.UnionDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}