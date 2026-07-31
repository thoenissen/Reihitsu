using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter lambda and anonymous method parameter names
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4124";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4124Title), nameof(AnalyzerResources.RH4124MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a parameter
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParameter(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ParameterSyntax parameter
            && context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is IParameterSymbol
                                                                                             {
                                                                                                 ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.AnonymousFunction },
                                                                                                 Name: var parameterName
                                                                                             }
            && SingleLetterIdentifierUtilities.HasSingleLetterName(parameterName))
        {
            context.ReportDiagnostic(CreateDiagnostic(parameter.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnParameter, SyntaxKind.Parameter);
    }

    #endregion // DiagnosticAnalyzer
}