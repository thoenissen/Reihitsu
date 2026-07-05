using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8103: Element parameter documentation must declare parameter name
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8103";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8103Title), nameof(AnalyzerResources.RH8103MessageFormat))
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

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        foreach (var paramNode in DirectDocumentationSyntaxChecker.GetDirectTags(documentationComment, "param")
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.ParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}