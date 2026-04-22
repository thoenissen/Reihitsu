using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0601: Constants must appear before fields.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0601ConstantsMustAppearBeforeFieldsAnalyzer : DiagnosticAnalyzerBase<RH0601ConstantsMustAppearBeforeFieldsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0601";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0601ConstantsMustAppearBeforeFieldsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0601Title), nameof(AnalyzerResources.RH0601MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the type declaration.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var seenNonConstantAccessibilityGroups = new HashSet<OrderingAccessibilityGroup>();

        foreach (var fieldDeclaration in typeDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            var accessibilityGroup = OrderingDeclarationUtilities.GetAccessibilityGroup(fieldDeclaration);

            if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
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
                                         SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}