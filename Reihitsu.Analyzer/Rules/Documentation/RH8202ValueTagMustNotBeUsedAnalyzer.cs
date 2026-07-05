using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8202: The &lt;value&gt; tag must not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8202ValueTagMustNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8202";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RH8202ValueTagMustNotBeUsedAnalyzer"/> class
    /// </summary>
    public RH8202ValueTagMustNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8202Title), nameof(AnalyzerResources.RH8202MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a declaration
    /// </summary>
    /// <param name="context">Analysis context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false)
        {
            return;
        }

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var valueNode = DirectDocumentationSyntaxChecker.GetFirstDirectTag(documentationComment, "value");

        if (valueNode == null)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(valueNode.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}