using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0438: Element return value documentation must have text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0438ElementReturnValueDocumentationMustHaveTextAnalyzer : DiagnosticAnalyzerBase<RH0438ElementReturnValueDocumentationMustHaveTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0438";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0438ElementReturnValueDocumentationMustHaveTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0438Title), nameof(AnalyzerResources.RH0438MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration with a return value
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

        var returnsNode = DocumentationAnalysisUtilities.GetFirstDirectTag(documentationComment, "returns");

        if (returnsNode == null
            || DocumentationAnalysisUtilities.IsEmpty(returnsNode) == false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(returnsNode.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.ReturnValueOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}