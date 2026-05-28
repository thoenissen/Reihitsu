using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8402: Files must start with the configured copyright header
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer : DiagnosticAnalyzerBase<RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8402";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8402Title), nameof(AnalyzerResources.RH8402MessageFormat))
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
        var configurationLoadResult = ConfigurationManager.GetConfiguration(context.Options.AdditionalFiles);
        var configuration = configurationLoadResult.Configuration?.Copyright;

        if (configurationLoadResult.Errors.IsDefaultOrEmpty == false
            || configuration == null
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
        if (context.Tree.Options is CSharpParseOptions parseOptions
            && parseOptions.DocumentationMode == DocumentationMode.None)
        {
            return;
        }

        var sourceText = context.Tree.GetText(context.CancellationToken);
        var expectedHeader = CopyrightHeaderTemplateResolver.ResolveHeader(configuration, context.Tree.FilePath);

        if (string.IsNullOrEmpty(expectedHeader))
        {
            return;
        }

        expectedHeader = CopyrightHeaderTemplateResolver.NormalizeLineEndings(expectedHeader, sourceText);

        var text = sourceText.ToString();

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