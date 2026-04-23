using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0432: The &lt;value&gt; tag must not be used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0432ValueTagMustNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH0432ValueTagMustNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID.
    /// </summary>
    public const string DiagnosticId = "RH0432";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RH0432ValueTagMustNotBeUsedAnalyzer"/> class.
    /// </summary>
    public RH0432ValueTagMustNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0432Title), nameof(AnalyzerResources.RH0432MessageFormat))
    {
    }

    #endregion // Constructor

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration);
    }

    #endregion // DiagnosticAnalyzer

    #region Methods

    /// <summary>
    /// Analyzes a declaration.
    /// </summary>
    /// <param name="context">Analysis context.</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false)
        {
            return;
        }

        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var valueNode = DocumentationAnalysisUtilities.GetFirstDirectTag(documentationComment, "value");

        if (valueNode == null)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(valueNode.GetLocation()));
    }

    #endregion // Methods
}