using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7108: Event accessors must follow order
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7108EventAccessorsMustFollowOrderAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7108";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7108EventAccessorsMustFollowOrderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the event declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        // The accessors are judged as the analyzed build compiles them; a conditional directive in the list does not keep
        // it from being reported, and the ordering requirements decide whether the fix may move the accessor
        // (RH7108-001, RH7108-005, SHARED-DIAG-008)
        if (context.Node is not EventDeclarationSyntax { AccessorList: not null } eventDeclaration)
        {
            return;
        }

        if (AccessorOrderingUtilities.TryGetAccessorMove(eventDeclaration.AccessorList,
                                                         SyntaxKind.AddAccessorDeclaration,
                                                         [SyntaxKind.RemoveAccessorDeclaration],
                                                         out var accessorToMove,
                                                         out _))
        {
            context.ReportDiagnostic(CreateDiagnostic(accessorToMove.Keyword.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, SyntaxKind.EventDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}