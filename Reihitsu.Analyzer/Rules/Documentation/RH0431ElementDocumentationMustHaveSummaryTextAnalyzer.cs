using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0431: Element documentation must have summary text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0431ElementDocumentationMustHaveSummaryTextAnalyzer : DiagnosticAnalyzerBase<RH0431ElementDocumentationMustHaveSummaryTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0431";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0431ElementDocumentationMustHaveSummaryTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0431Title), nameof(AnalyzerResources.RH0431MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MemberDeclarationSyntax declaration)
        {
            if (DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false)
            {
                return;
            }

            AnalyzeDeclaration(context, declaration);

            return;
        }

        if (context.Node is EnumMemberDeclarationSyntax enumMemberDeclaration)
        {
            AnalyzeDeclaration(context, enumMemberDeclaration);
        }
    }

    /// <summary>
    /// Analyzes a declaration with XML documentation support
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="declaration">Declaration node</param>
    private void AnalyzeDeclaration(SyntaxNodeAnalysisContext context, SyntaxNode declaration)
    {
        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var summaryNode = DocumentationAnalysisUtilities.GetFirstDirectTag(documentationComment, "summary");

        if (summaryNode == null)
        {
            return;
        }

        if (DocumentationAnalysisUtilities.IsEmpty(summaryNode) == false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(summaryNode.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.SummaryDocumentationKinds);
    }

    #endregion // DiagnosticAnalyzer
}