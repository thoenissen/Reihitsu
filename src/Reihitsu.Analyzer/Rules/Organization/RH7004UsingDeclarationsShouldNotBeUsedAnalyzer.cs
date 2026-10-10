using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7004: Using declarations should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7004UsingDeclarationsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7004";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7004UsingDeclarationsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7004Title), nameof(AnalyzerResources.RH7004MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes local declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax { IsConst: false, UsingKeyword.RawKind: not 0 } usingDeclaration)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(usingDeclaration.UsingKeyword.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnLocalDeclarationStatement, SyntaxKind.LocalDeclarationStatement);
    }

    #endregion // DiagnosticAnalyzer
}