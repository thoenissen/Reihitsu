using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH0106: File may only contain a single namespace
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0106FileMayOnlyContainASingleNamespaceAnalyzer : DiagnosticAnalyzerBase<RH0106FileMayOnlyContainASingleNamespaceAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0106FileMayOnlyContainASingleNamespaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0106Title), nameof(AnalyzerResources.RH0106MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all syntax trees
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
        {
            return;
        }

        var namespaceDeclarations = root.Members.OfType<BaseNamespaceDeclarationSyntax>().ToList();

        if (namespaceDeclarations.Count <= 1)
        {
            return;
        }

        foreach (var namespaceDeclaration in namespaceDeclarations.Skip(1))
        {
            context.ReportDiagnostic(CreateDiagnostic(namespaceDeclaration.Name.GetLocation()));
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