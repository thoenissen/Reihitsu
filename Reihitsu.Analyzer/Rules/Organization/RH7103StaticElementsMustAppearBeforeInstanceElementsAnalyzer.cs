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
/// RH7103: Static elements must appear before instance elements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer : DiagnosticAnalyzerBase<RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7103";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7103Title), nameof(AnalyzerResources.RH7103MessageFormat))
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

        var seenInstanceGroups = new HashSet<(OrderingMemberKindGroup MemberKind, OrderingAccessibilityGroup AccessibilityGroup)>();

        foreach (var memberDeclaration in typeDeclaration.Members)
        {
            if (OrderingDeclarationUtilities.IsConst(memberDeclaration))
            {
                continue;
            }

            var group = (OrderingDeclarationUtilities.GetMemberKind(memberDeclaration), OrderingDeclarationUtilities.GetAccessibilityGroup(memberDeclaration));

            if (OrderingDeclarationUtilities.IsStatic(memberDeclaration))
            {
                if (seenInstanceGroups.Contains(group))
                {
                    context.ReportDiagnostic(CreateDiagnostic(OrderingDeclarationUtilities.GetDiagnosticLocation(memberDeclaration)));
                }

                continue;
            }

            seenInstanceGroups.Add(group);
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