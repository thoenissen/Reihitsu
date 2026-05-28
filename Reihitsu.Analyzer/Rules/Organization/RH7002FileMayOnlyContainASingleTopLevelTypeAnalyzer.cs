using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7002: File may only contain a single top-level type
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer : DiagnosticAnalyzerBase<RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7002";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7002Title), nameof(AnalyzerResources.RH7002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all syntax trees
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
        {
            return;
        }

        var topLevelTypeDeclarations = root.DescendantNodes(static node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax)
                                           .OfType<BaseTypeDeclarationSyntax>()
                                           .ToList();

        if (topLevelTypeDeclarations.Count <= 1)
        {
            return;
        }

        foreach (var topLevelTypeDeclaration in topLevelTypeDeclarations.Skip(1))
        {
            context.ReportDiagnostic(CreateDiagnostic(topLevelTypeDeclaration.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}