using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0611: Event accessors must follow order
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0611EventAccessorsMustFollowOrderAnalyzer : DiagnosticAnalyzerBase<RH0611EventAccessorsMustFollowOrderAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0611";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0611EventAccessorsMustFollowOrderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0611Title), nameof(AnalyzerResources.RH0611MessageFormat))
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