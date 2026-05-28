using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8030: Element documentation must have summary text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8030ElementDocumentationMustHaveSummaryTextAnalyzer : DiagnosticAnalyzerBase<RH8030ElementDocumentationMustHaveSummaryTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8030";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8030ElementDocumentationMustHaveSummaryTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8030Title), nameof(AnalyzerResources.RH8030MessageFormat))
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
        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var summaryNode = DirectDocumentationSyntaxChecker.GetFirstDirectTag(documentationComment, "summary");

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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.SummaryDocumentationKinds);
    }

    #endregion // DiagnosticAnalyzer
}