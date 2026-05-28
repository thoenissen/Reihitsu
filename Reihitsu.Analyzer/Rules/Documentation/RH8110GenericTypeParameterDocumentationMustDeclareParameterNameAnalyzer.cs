using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8110: Generic type parameter documentation must declare parameter name
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer : DiagnosticAnalyzerBase<RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8110";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8110Title), nameof(AnalyzerResources.RH8110MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration with generic type parameters
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

        var unnamedTypeParameterNodes = DirectDocumentationSyntaxChecker.GetDirectTags(documentationComment, "typeparam").Where(typeParameterNode => string.IsNullOrWhiteSpace(DocumentationAnalysisUtilities.GetNameAttributeValue(typeParameterNode)));

        foreach (var typeParameterNode in unnamedTypeParameterNodes)
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.TypeParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}