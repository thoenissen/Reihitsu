using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8031: Extension declarations must have summary text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8031";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8031Title), nameof(AnalyzerResources.RH8031MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze an extension declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExtensionBlockDeclarationSyntax declaration)
        {
            return;
        }

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            context.ReportDiagnostic(CreateDiagnostic(declaration.Keyword.GetLocation()));

            return;
        }

        var summaryNode = DirectDocumentationSyntaxChecker.GetFirstDirectTag(documentationComment, "summary");

        if (summaryNode == null
            || DocumentationAnalysisUtilities.IsEmpty(summaryNode))
        {
            context.ReportDiagnostic(CreateDiagnostic(summaryNode?.GetLocation() ?? declaration.Keyword.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, SyntaxKind.ExtensionBlockDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}