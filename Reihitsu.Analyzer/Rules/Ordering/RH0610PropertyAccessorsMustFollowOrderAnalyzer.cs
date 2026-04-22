using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0610: Property accessors must follow order.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0610PropertyAccessorsMustFollowOrderAnalyzer : DiagnosticAnalyzerBase<RH0610PropertyAccessorsMustFollowOrderAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0610";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0610PropertyAccessorsMustFollowOrderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0610Title), nameof(AnalyzerResources.RH0610MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the accessor list.
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