using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8111: Generic type parameter documentation must have text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer : DiagnosticAnalyzerBase<RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8111";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8111Title), nameof(AnalyzerResources.RH8111MessageFormat))
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

        var emptyTypeParameterNodes = DirectDocumentationSyntaxChecker.GetDirectTags(documentationComment, "typeparam").Where(typeParameterNode => DocumentationAnalysisUtilities.IsEmpty(typeParameterNode));

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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.TypeParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}