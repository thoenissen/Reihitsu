using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0437: Element return value must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0437ElementReturnValueMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0437ElementReturnValueMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0437";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0437ElementReturnValueMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0437Title), nameof(AnalyzerResources.RH0437MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration with a return value.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false
            || DocumentationAnalysisUtilities.TryGetNonVoidReturnType(declaration, out var returnType) == false)
        {
            return;
        }

        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var expandedDocumentation = DocumentationAnalysisUtilities.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);

        if (DocumentationAnalysisUtilities.HasInheritdoc(documentationComment, expandedDocumentation)
            || DocumentationAnalysisUtilities.HasTag(documentationComment, expandedDocumentation, "returns"))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(returnType.GetLocation()));
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