using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7107: Property accessors must follow order
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7107PropertyAccessorsMustFollowOrderAnalyzer : DiagnosticAnalyzerBase<RH7107PropertyAccessorsMustFollowOrderAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7107";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7107PropertyAccessorsMustFollowOrderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7107Title), nameof(AnalyzerResources.RH7107MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the accessor list
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        var accessorList = context.Node switch
                           {
                               PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.AccessorList,
                               IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.AccessorList,
                               _ => null,
                           };

        if (accessorList == null)
        {
            return;
        }

        if (AccessorOrderingUtilities.TryGetAccessorMove(accessorList,
                                                         SyntaxKind.GetAccessorDeclaration,
                                                         [SyntaxKind.SetAccessorDeclaration, SyntaxKind.InitAccessorDeclaration],
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

        context.RegisterSyntaxNodeAction(OnDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}