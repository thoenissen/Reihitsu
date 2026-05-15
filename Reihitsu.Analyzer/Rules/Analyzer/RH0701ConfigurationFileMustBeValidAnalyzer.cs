using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Analyzer;

/// <summary>
/// RH0701: reihitsu.json configuration must be valid
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0701ConfigurationFileMustBeValidAnalyzer : DiagnosticAnalyzerBase<RH0701ConfigurationFileMustBeValidAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0701";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0701ConfigurationFileMustBeValidAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Analyzer, nameof(AnalyzerResources.RH0701Title), nameof(AnalyzerResources.RH0701MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Create location
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="error">Error</param>
    /// <returns>Location</returns>
    private static Location CreateLocation(ConfigurationLoadResult configuration, ConfigurationValidationError error)
    {
        var span = ClampSpan(configuration.Text, error.Span);
        var lineSpan = configuration.Text == null
                           ? new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0))
                           : configuration.Text.Lines.GetLinePositionSpan(span);

        return Location.Create(configuration.File.Path, span, lineSpan);
    }

    /// <summary>
    /// Clamp span
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="span">Span</param>
    /// <returns>Clamped span</returns>
    private static TextSpan ClampSpan(SourceText text, TextSpan span)
    {
        if (text == null)
        {
            return new TextSpan(0, 0);
        }

        var start = Math.Min(span.Start, text.Length);
        var end = Math.Min(span.End, text.Length);

        return TextSpan.FromBounds(start, end);
    }

    /// <summary>
    /// Analyze compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilation(CompilationAnalysisContext context)
    {
        var configuration = ConfigurationManager.GetConfiguration(context.Options.AdditionalFiles);

        if (configuration.File == null
            || configuration.Errors.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var error in configuration.Errors)
        {
            context.ReportDiagnostic(CreateDiagnostic(CreateLocation(configuration, error), error.Message));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationAction(OnCompilation);
    }

    #endregion // DiagnosticAnalyzer
}