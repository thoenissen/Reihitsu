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
/// RH8033: Extension declaration type parameters must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8033";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8033Title), nameof(AnalyzerResources.RH8033MessageFormat))
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
        if (context.Node is not ExtensionBlockDeclarationSyntax declaration
            || declaration.TypeParameterList == null)
        {
            return;
        }

        var typeParameters = declaration.TypeParameterList.Parameters.ToImmutableArray();
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, SyntaxKind.ExtensionBlockDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}