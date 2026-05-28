using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5603: File must not end with a newline
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5603FileMustNotEndWithANewlineAnalyzer : DiagnosticAnalyzerBase<RH5603FileMustNotEndWithANewlineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5603";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5603FileMustNotEndWithANewlineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5603Title), nameof(AnalyzerResources.RH5603MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree for trailing newline characters
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var sourceText = context.Tree.GetText(context.CancellationToken);

        if (sourceText.Length == 0)
        {
            return;
        }

        var content = sourceText.ToString();
        var trailingNewlineStart = content.Length;

        while (trailingNewlineStart > 0
               && (content[trailingNewlineStart - 1] == '\r'
                   || content[trailingNewlineStart - 1] == '\n'))
        {
            trailingNewlineStart--;
        }

        if (trailingNewlineStart == content.Length)
        {
            return;
        }

        var diagnosticSpan = TextSpan.FromBounds(trailingNewlineStart, content.Length);

        context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, diagnosticSpan)));
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