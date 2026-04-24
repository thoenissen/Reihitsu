using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0381: Parameters must be on same line or separate lines.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer : DiagnosticAnalyzerBase<RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0381";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0381ParametersMustBeOnSameLineOrSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0381Title), nameof(AnalyzerResources.RH0381MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var parameterList in root.DescendantNodes().OfType<ParameterListSyntax>())
        {
            if (parameterList.Parameters.Count <= 1)
            {
                continue;
            }

            var lineGroups = parameterList.Parameters.GroupBy(parameter => parameter.GetLocation().GetLineSpan().StartLinePosition.Line).ToList();

            if (lineGroups.Count > 1
                && lineGroups.Count < parameterList.Parameters.Count)
            {
                context.ReportDiagnostic(CreateDiagnostic(parameterList.OpenParenToken.GetLocation()));
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