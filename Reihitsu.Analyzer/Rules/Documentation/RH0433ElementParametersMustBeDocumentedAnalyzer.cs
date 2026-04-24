using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0433: Element parameters must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0433ElementParametersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0433ElementParametersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0433";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0433ElementParametersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0433Title), nameof(AnalyzerResources.RH0433MessageFormat))
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
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false)
        {
            return;
        }

        var parameters = DocumentationAnalysisUtilities.GetParameters(declaration);
        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (parameters.IsDefaultOrEmpty
            || documentationComment == null)
        {
            return;
        }

        var expandedDocumentation = DocumentationAnalysisUtilities.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);

        if (DocumentationAnalysisUtilities.HasInheritdoc(documentationComment, expandedDocumentation))
        {
            return;
        }

        var documentedParameterNames = DocumentationAnalysisUtilities.GetExpandedElements(expandedDocumentation, "param")
                                                                     .Select(obj => obj.Attribute("name")?.Value)
                                                                     .Where(obj => string.IsNullOrWhiteSpace(obj) == false)
                                                                     .ToImmutableHashSet(StringComparer.Ordinal);

        foreach (var parameter in parameters)
        {
            if (documentedParameterNames.Contains(parameter.Identifier.ValueText) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(parameter.Identifier.GetLocation()));
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