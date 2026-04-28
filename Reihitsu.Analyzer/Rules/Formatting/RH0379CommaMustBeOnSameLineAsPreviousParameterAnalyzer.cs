using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0379: Comma must be on same line as previous parameter
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0379CommaMustBeOnSameLineAsPreviousParameterAnalyzer : DiagnosticAnalyzerBase<RH0379CommaMustBeOnSameLineAsPreviousParameterAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0379";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0379CommaMustBeOnSameLineAsPreviousParameterAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0379Title), nameof(AnalyzerResources.RH0379MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var parameters in root.DescendantNodes().OfType<ParameterListSyntax>().Select(parameterList => parameterList.Parameters))
        {
            for (var index = 0; index < parameters.SeparatorCount; index++)
            {
                var comma = parameters.GetSeparator(index);
                var previousParameter = parameters[index];

                if (comma.GetLocation().GetLineSpan().StartLinePosition.Line != previousParameter.GetLocation().GetLineSpan().EndLinePosition.Line)
                {
                    context.ReportDiagnostic(CreateDiagnostic(comma.GetLocation()));
                }
            }
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