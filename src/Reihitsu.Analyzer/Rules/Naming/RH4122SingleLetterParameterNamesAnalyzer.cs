using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Analyzer for single-letter method, constructor, and indexer parameter names
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4122SingleLetterParameterNamesAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4122";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4122SingleLetterParameterNamesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4122Title), nameof(AnalyzerResources.RH4122MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Checks whether a parameter belongs to a supported declaration
    /// </summary>
    /// <param name="parameterSymbol">Parameter symbol</param>
    /// <returns>True if the parameter is handled by this rule, otherwise false</returns>
    private static bool IsSupportedParameter(IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.ContainingSymbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind is not MethodKind.LocalFunction
                                           and not MethodKind.AnonymousFunction;
        }

        return parameterSymbol.ContainingSymbol is IPropertySymbol { IsIndexer: true };
    }

    /// <summary>
    /// Analyzes a parameter
    /// </summary>
    /// <param name="context">Context</param>
    private void OnParameter(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ParameterSyntax parameter
            && context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is IParameterSymbol parameterSymbol
            && IsSupportedParameter(parameterSymbol)
            && SingleLetterIdentifierUtilities.HasSingleLetterName(parameterSymbol.Name)
            && SingleLetterIdentifierUtilities.IsFrameworkMandatedSignature(parameterSymbol) == false
            && SingleLetterIdentifierUtilities.IsRecordPrimaryConstructorParameter(parameterSymbol) == false)
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