using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0382: Field declarations should not combine multiple variables.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0382DoNotCombineFieldsAnalyzer : DiagnosticAnalyzerBase<RH0382DoNotCombineFieldsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0382";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0382DoNotCombineFieldsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0382Title), nameof(AnalyzerResources.RH0382MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes field declarations.
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