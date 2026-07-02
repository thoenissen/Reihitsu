using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Analyzer;

/// <summary>
/// RH0002: DocumentationMode must not be None for documentation rules
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0002DocumentationModeMustNotBeNoneAnalyzer : DiagnosticAnalyzerBase<RH0002DocumentationModeMustNotBeNoneAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0002";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0002DocumentationModeMustNotBeNoneAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Analyzer, nameof(AnalyzerResources.RH0002Title), nameof(AnalyzerResources.RH0002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Create location
    /// </summary>
    /// <param name="syntaxTree">Syntax tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Location</returns>
    private static Location CreateLocation(SyntaxTree syntaxTree, CancellationToken cancellationToken)
    {
        var firstToken = syntaxTree.GetRoot(cancellationToken).GetFirstToken(includeZeroWidth: true);

        return firstToken != default
                   ? firstToken.GetLocation()
                   : Location.Create(syntaxTree, new TextSpan(0, 0));
    }

    /// <summary>
    /// Analyze compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilation(CompilationAnalysisContext context)
    {
        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            if (syntaxTree.Options is CSharpParseOptions parseOptions
                && parseOptions.DocumentationMode == DocumentationMode.None)
            {
                context.ReportDiagnostic(CreateDiagnostic(CreateLocation(syntaxTree, context.CancellationToken)));
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