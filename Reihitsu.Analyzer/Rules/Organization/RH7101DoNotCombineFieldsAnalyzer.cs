using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7101: Field declarations should not combine multiple variables
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7101DoNotCombineFieldsAnalyzer : DiagnosticAnalyzerBase<RH7101DoNotCombineFieldsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7101";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7101DoNotCombineFieldsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7101Title), nameof(AnalyzerResources.RH7101MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes field declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration
            || fieldDeclaration.Declaration.Variables.Count <= 1)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(fieldDeclaration.Declaration.Variables[1].Identifier.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}