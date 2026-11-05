using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7101: Field declarations should not combine multiple variables
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7101DoNotCombineFieldsAnalyzer : DiagnosticAnalyzerBase
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
    /// <param name="errorVerdicts">Syntax error verdict per tree</param>
    private void OnFieldDeclaration(SyntaxNodeAnalysisContext context, SyntaxTreeErrorVerdictCache errorVerdicts)
    {
        // A field cannot be declared in an extension block (CS9282), so such a declaration is no state of a type, and the
        // formatter, which the fix splits through, does not split it (RH7101-002)
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration
            || fieldDeclaration.Declaration.Variables.Count <= 1
            || fieldDeclaration.Parent.IsKind(SyntaxKind.ExtensionBlockDeclaration))
        {
            return;
        }

        // The rule withholds itself for a whole file with malformed syntax, where the formatter leaves the file untouched and
        // the fix could not split the declaration (RH7101-001, SHARED-JOIN-002)
        if (errorVerdicts.ContainsError(context.Node.SyntaxTree, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(fieldDeclaration.Declaration.Variables[1].Identifier.GetLocation()));
    }

    /// <summary>
    /// Starts the analysis of a compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var errorVerdicts = new SyntaxTreeErrorVerdictCache();

        context.RegisterSyntaxNodeAction(nodeContext => OnFieldDeclaration(nodeContext, errorVerdicts), SyntaxKind.FieldDeclaration);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion // DiagnosticAnalyzer
}