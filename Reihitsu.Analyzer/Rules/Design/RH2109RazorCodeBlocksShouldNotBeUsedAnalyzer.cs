using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2109: Razor @code blocks should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2109";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Pattern for Razor code blocks
    /// </summary>
    private static readonly Regex _codeBlockPattern = new(@"@code\s*\{", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2109Title), nameof(AnalyzerResources.RH2109MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilation(CompilationAnalysisContext context)
    {
        foreach (var file in context.Options.AdditionalFiles)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) is false)
            {
                continue;
            }

            var text = file.GetText(context.CancellationToken);

            if (text == null)
            {
                continue;
            }

            foreach (Match match in _codeBlockPattern.Matches(text.ToString()))
            {
                var span = new TextSpan(match.Index, "@code".Length);
                var lineSpan = text.Lines.GetLinePositionSpan(span);
                var location = Location.Create(file.Path, span, lineSpan);

                context.ReportDiagnostic(CreateDiagnostic(location));
            }
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