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
        : base(DiagnosticId, DiagnosticCategory.Naming)
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
        var configuration = ConfigurationManager.GetConfiguration(context.Options.AdditionalFiles).Configuration;

        if (configuration?.Naming?.AllowedNamespaceDeclarations?.Count > 0)
        {
            context.RegisterSyntaxNodeAction(analysisContext => OnNamespaceDeclarationSyntax(configuration, analysisContext), SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
        }
    }

    /// <summary>
    /// Analyzing all namespace declarations. The declared full name is compared by its value: the identifiers of the enclosing namespace
    /// declarations and of the declaration itself, without a verbatim <c>@</c> prefix and with every Unicode escape sequence replaced,
    /// joined by dots without the whitespace, line breaks, or comments written between them. The diagnostic stays on the declaration's own name
    /// </summary>
    /// <param name="configuration">Configuration</param>
    /// <param name="context">Context</param>
    private void OnNamespaceDeclarationSyntax(Configuration configuration, SyntaxNodeAnalysisContext context)
    {
        if (context.Node is BaseNamespaceDeclarationSyntax namespaceSyntax
            && configuration.Naming.AllowedNamespaceDeclarations.Contains(string.Join(".", NamespaceCasingHelper.GetFullNameParts(namespaceSyntax))) == false)
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