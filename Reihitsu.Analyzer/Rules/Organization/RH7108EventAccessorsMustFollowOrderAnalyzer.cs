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
public class RH7108EventAccessorsMustFollowOrderAnalyzer : DiagnosticAnalyzerBase<RH7108EventAccessorsMustFollowOrderAnalyzer>
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
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7108Title), nameof(AnalyzerResources.RH7108MessageFormat))
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