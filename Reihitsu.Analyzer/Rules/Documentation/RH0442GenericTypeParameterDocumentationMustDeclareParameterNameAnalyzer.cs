using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0442: Generic type parameter documentation must declare parameter name.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer : DiagnosticAnalyzerBase<RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0442";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0442GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0442Title), nameof(AnalyzerResources.RH0442MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration with generic type parameters.
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

        foreach (var typeParameterNode in DocumentationAnalysisUtilities.GetDirectTags(documentationComment, "typeparam"))
        {
            if (string.IsNullOrWhiteSpace(DocumentationAnalysisUtilities.GetNameAttributeValue(typeParameterNode)))
            {
                context.ReportDiagnostic(CreateDiagnostic(typeParameterNode.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.TypeParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}