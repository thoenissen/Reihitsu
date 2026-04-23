using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0436: Element parameter documentation must have text.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0436ElementParameterDocumentationMustHaveTextAnalyzer : DiagnosticAnalyzerBase<RH0436ElementParameterDocumentationMustHaveTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0436";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0436ElementParameterDocumentationMustHaveTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0436Title), nameof(AnalyzerResources.RH0436MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a parameter-bearing declaration.
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

        foreach (var paramNode in DocumentationAnalysisUtilities.GetDirectTags(documentationComment, "param"))
        {
            if (DocumentationAnalysisUtilities.IsEmpty(paramNode))
            {
                context.ReportDiagnostic(CreateDiagnostic(paramNode.GetLocation()));
            }
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