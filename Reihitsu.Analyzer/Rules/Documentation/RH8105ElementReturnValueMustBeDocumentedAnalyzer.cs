using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8105: Element return value must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8105ElementReturnValueMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH8105ElementReturnValueMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8105";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8105ElementReturnValueMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8105Title), nameof(AnalyzerResources.RH8105MessageFormat))
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
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false
            || DocumentationAnalysisUtilities.TryGetNonVoidReturnType(declaration, out var returnType) == false)
        {
            return;
        }

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var expandedDocumentation = XmlDocumentationExpander.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);

        if (XmlDocumentationExpander.HasInheritdoc(documentationComment, expandedDocumentation)
            || XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation, "returns"))
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.ReturnValueOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}