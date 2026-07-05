using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8203: &lt;inheritdoc&gt; must be used with inheriting class
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8203";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8203Title), nameof(AnalyzerResources.RH8203MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration
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

        var inheritdocNode = DirectDocumentationSyntaxChecker.GetFirstDirectTag(documentationComment, "inheritdoc");
        var expandedDocumentation = XmlDocumentationExpander.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);
        var expandedInheritdocElement = XmlDocumentationExpander.GetFirstExpandedElement(expandedDocumentation, "inheritdoc");

        if (inheritdocNode == null && expandedInheritdocElement == null)
        {
            return;
        }

        if ((inheritdocNode != null && DocumentationAnalysisUtilities.HasCrefAttribute(inheritdocNode))
            || (expandedInheritdocElement != null && DocumentationAnalysisUtilities.HasCrefAttribute(expandedInheritdocElement)))
        {
            return;
        }

        if (DocumentationAnalysisUtilities.CanInheritDocumentation(declaration, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        var diagnosticLocation = inheritdocNode?.GetLocation() ?? DocumentationAnalysisUtilities.GetDiagnosticLocation(declaration);

        context.ReportDiagnostic(CreateDiagnostic(diagnosticLocation));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.DocumentableDeclarationKinds);
    }

    #endregion // DiagnosticAnalyzer
}