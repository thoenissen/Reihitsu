using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8108: Generic type parameters must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8108GenericTypeParametersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8108";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8108GenericTypeParametersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8108Title), nameof(AnalyzerResources.RH8108MessageFormat))
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
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false)
        {
            return;
        }

        var typeParameters = DocumentationAnalysisUtilities.GetTypeParameters(declaration);
        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (typeParameters.IsDefaultOrEmpty
            || documentationComment == null)
        {
            return;
        }

        var expandedDocumentation = XmlDocumentationExpander.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);

        if (XmlDocumentationExpander.HasInheritdoc(documentationComment, expandedDocumentation))
        {
            return;
        }

        var documentedTypeParameterNames = XmlDocumentationExpander.GetExpandedElements(expandedDocumentation, "typeparam")
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.TypeParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}