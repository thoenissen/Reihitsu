using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0440: Generic type parameters must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0440GenericTypeParametersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0440GenericTypeParametersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0440";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0440GenericTypeParametersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0440Title), nameof(AnalyzerResources.RH0440MessageFormat))
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
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false)
        {
            return;
        }

        var typeParameters = DocumentationAnalysisUtilities.GetTypeParameters(declaration);
        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (typeParameters.IsDefaultOrEmpty
            || documentationComment == null)
        {
            return;
        }

        var expandedDocumentation = DocumentationAnalysisUtilities.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);

        if (DocumentationAnalysisUtilities.HasInheritdoc(documentationComment, expandedDocumentation))
        {
            return;
        }

        var documentedTypeParameterNames = DocumentationAnalysisUtilities.GetExpandedElements(expandedDocumentation, "typeparam")
                                                                         .Select(obj => obj.Attribute("name")?.Value)
                                                                         .Where(obj => string.IsNullOrWhiteSpace(obj) == false)
                                                                         .ToImmutableHashSet(StringComparer.Ordinal);

        foreach (var typeParameter in typeParameters.Where(typeParameter => documentedTypeParameterNames.Contains(typeParameter.Identifier.ValueText) == false))
        {
            context.ReportDiagnostic(CreateDiagnostic(typeParameter.Identifier.GetLocation()));
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