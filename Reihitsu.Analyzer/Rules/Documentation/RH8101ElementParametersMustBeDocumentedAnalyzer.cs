using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8101: Element parameters must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8101ElementParametersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8101";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8101ElementParametersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8101Title), nameof(AnalyzerResources.RH8101MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a parameter-bearing declaration
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
        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (parameters.IsDefaultOrEmpty
            || documentationComment == null)
        {
            return;
        }

        var expandedDocumentation = XmlDocumentationExpander.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);

        if (XmlDocumentationExpander.HasInheritdoc(documentationComment, expandedDocumentation))
        {
            return;
        }

        var documentedParameterNames = XmlDocumentationExpander.GetExpandedElements(expandedDocumentation, "param")
                                                               .Select(obj => obj.Attribute("name")?.Value)
                                                               .Where(obj => string.IsNullOrWhiteSpace(obj) == false)
                                                               .ToImmutableHashSet(StringComparer.Ordinal);

        foreach (var parameter in parameters.Where(parameter => documentedParameterNames.Contains(parameter.Identifier.ValueText) == false))
        {
            context.ReportDiagnostic(CreateDiagnostic(parameter.Identifier.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.ParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}