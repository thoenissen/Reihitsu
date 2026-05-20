using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0454: Extension declaration parameters must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0454";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0454ExtensionDeclarationParametersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0454Title), nameof(AnalyzerResources.RH0454MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze an extension declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExtensionBlockDeclarationSyntax declaration)
        {
            return;
        }

        var parameters = declaration.ParameterList.Parameters;
        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (parameters.Count == 0
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, SyntaxKind.ExtensionBlockDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}