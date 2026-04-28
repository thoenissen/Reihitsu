using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0435: Element parameter documentation must declare parameter name
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzer : DiagnosticAnalyzerBase<RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0435";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0435ElementParameterDocumentationMustDeclareParameterNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0435Title), nameof(AnalyzerResources.RH0435MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a parameter-bearing declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration)
        {
            return;
        }

        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        foreach (var paramNode in DocumentationAnalysisUtilities.GetDirectTags(documentationComment, "param")
                                                                .Where(paramNode => string.IsNullOrWhiteSpace(DocumentationAnalysisUtilities.GetNameAttributeValue(paramNode))))
        {
            context.ReportDiagnostic(CreateDiagnostic(paramNode.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.ParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}