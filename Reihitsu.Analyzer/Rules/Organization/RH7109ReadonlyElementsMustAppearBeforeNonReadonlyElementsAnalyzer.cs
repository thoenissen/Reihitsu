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
/// RH7109: Readonly elements must appear before non-readonly elements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer : DiagnosticAnalyzerBase<RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7109";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7109Title), nameof(AnalyzerResources.RH7109MessageFormat))
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

        var seenNonReadonlyGroups = new HashSet<(OrderingAccessibilityGroup AccessibilityGroup, bool IsStaticField)>();

        foreach (var fieldDeclaration in typeDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                continue;
            }

            var group = (OrderingDeclarationUtilities.GetAccessibilityGroup(fieldDeclaration), fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));

            if (fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                if (seenNonReadonlyGroups.Contains(group))
                {
                    context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(fieldDeclaration)));
                }

                continue;
            }

            seenNonReadonlyGroups.Add(group);
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