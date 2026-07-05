using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7003: File-scoped namespaces should be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7003FileScopedNamespacesShouldBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7003";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7003FileScopedNamespacesShouldBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7003Title), nameof(AnalyzerResources.RH7003MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given namespace declaration can be converted to a file-scoped namespace without
    /// changing the file's scope
    /// </summary>
    /// <param name="namespaceDeclaration">Namespace declaration</param>
    /// <returns><see langword="true"/> if the namespace should be diagnosed; otherwise, <see langword="false"/></returns>
    private static bool IsEligibleNamespace(NamespaceDeclarationSyntax namespaceDeclaration)
    {
        if (namespaceDeclaration.Parent is not CompilationUnitSyntax compilationUnit)
        {
            return false;
        }

        if (compilationUnit.Members.Count != 1 || compilationUnit.Members[0] != namespaceDeclaration)
        {
            return false;
        }

        return namespaceDeclaration.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp10 };
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.NamespaceDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnNamespaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is NamespaceDeclarationSyntax namespaceDeclaration
            && IsEligibleNamespace(namespaceDeclaration))
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

        context.RegisterSyntaxNodeAction(OnNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}