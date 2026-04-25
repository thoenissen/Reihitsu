using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0443: Generic type parameter documentation must have text.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzer : DiagnosticAnalyzerBase<RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0443";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0443GenericTypeParameterDocumentationMustHaveTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0443Title), nameof(AnalyzerResources.RH0443MessageFormat))
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

        var emptyTypeParameterNodes = DocumentationAnalysisUtilities.GetDirectTags(documentationComment, "typeparam").Where(typeParameterNode => DocumentationAnalysisUtilities.IsEmpty(typeParameterNode));

        foreach (var typeParameterNode in emptyTypeParameterNodes)
        {
            context.ReportDiagnostic(CreateDiagnostic(typeParameterNode.GetLocation()));
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