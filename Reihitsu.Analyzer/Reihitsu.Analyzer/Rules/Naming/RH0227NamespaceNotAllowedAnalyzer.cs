using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0227: The given namespace is not allowed (see reihitsu.json)
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0227NamespaceNotAllowedAnalyzer : DiagnosticAnalyzerBase<RH0227NamespaceNotAllowedAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0227";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0227NamespaceNotAllowedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0227Title), nameof(AnalyzerResources.RH0227MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Start of the compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        if (ConfigurationManager.TryGetConfiguration(context.Options.AdditionalFiles, out var configuration)
            && configuration?.Naming?.AllowedNamespaceDeclarations?.Count > 0)
        {
            context.RegisterSyntaxNodeAction(analysisContext => OnNamespaceDeclarationSyntax(configuration, analysisContext), SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
        }
    }

    /// <summary>
    /// Analyzing all namespace declarations
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="context">Context</param>
    private void OnNamespaceDeclarationSyntax(Configuration configuration, SyntaxNodeAnalysisContext context)
    {
        if (context.Node is BaseNamespaceDeclarationSyntax namespaceSyntax
            && configuration.Naming.AllowedNamespaceDeclarations.Contains(namespaceSyntax.Name.ToString()) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(namespaceSyntax.Name.GetLocation()));
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