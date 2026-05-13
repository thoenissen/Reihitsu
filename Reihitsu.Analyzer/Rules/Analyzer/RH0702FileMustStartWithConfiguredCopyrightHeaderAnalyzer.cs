using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Analyzer;

/// <summary>
/// RH0702: Files must start with the configured copyright header
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer : DiagnosticAnalyzerBase<RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0702";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Analyzer, nameof(AnalyzerResources.RH0702Title), nameof(AnalyzerResources.RH0702MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze compilation start
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var configuration = ConfigurationManager.GetConfiguration(context.Options.AdditionalFiles).Configuration?.Copyright;

        if (configuration == null
            || string.IsNullOrWhiteSpace(configuration.CopyrightText))
        {
            return;
        }

        context.RegisterSyntaxTreeAction(analysisContext => OnSyntaxTree(configuration, analysisContext));
    }

    /// <summary>
    /// Analyze syntax tree
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(ConfigurationCategoryCopyright configuration, SyntaxTreeAnalysisContext context)
    {
        var expectedHeader = CopyrightHeaderTemplateResolver.ResolveHeader(configuration, context.Tree.FilePath);

        if (string.IsNullOrEmpty(expectedHeader))
        {
            return;
        }

        var text = context.Tree.GetText(context.CancellationToken).ToString();

        if (text.StartsWith(expectedHeader, StringComparison.Ordinal) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, new TextSpan(0, 0))));
        }
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