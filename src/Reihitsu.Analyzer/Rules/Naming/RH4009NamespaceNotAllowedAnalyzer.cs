using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4009: The given namespace is not allowed (see reihitsu.json)
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4009NamespaceNotAllowedAnalyzer : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4009";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4009NamespaceNotAllowedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4009Title), nameof(AnalyzerResources.RH4009MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the value of a declared namespace name
    /// </summary>
    /// <param name="nameSyntax">Namespace name syntax</param>
    /// <returns>The identifier values joined by dots</returns>
    private static string GetNamespaceName(NameSyntax nameSyntax)
    {
        return string.Join(".", NamespaceCasingHelper.GetLocations(nameSyntax).Select(part => part.Name));
    }

    /// <summary>
    /// Start of the compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var configuration = ConfigurationManager.GetConfiguration(context.Options.AdditionalFiles).Configuration;

        if (configuration?.Naming?.AllowedNamespaceDeclarations?.Count > 0)
        {
            context.RegisterSyntaxNodeAction(analysisContext => OnNamespaceDeclarationSyntax(configuration, analysisContext), SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
        }
    }

    /// <summary>
    /// Analyzing all namespace declarations. The declared name is compared by its value: its identifiers without a verbatim <c>@</c> prefix
    /// and with every Unicode escape sequence replaced, joined by dots without the whitespace, line breaks, or comments written between them
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="context">Context</param>
    private void OnNamespaceDeclarationSyntax(Configuration configuration, SyntaxNodeAnalysisContext context)
    {
        if (context.Node is BaseNamespaceDeclarationSyntax namespaceSyntax
            && configuration.Naming.AllowedNamespaceDeclarations.Contains(GetNamespaceName(namespaceSyntax.Name)) == false)
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