using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0445: &lt;inheritdoc&gt; must be used with inheriting class
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0445InheritdocMustBeUsedWithInheritingClassAnalyzer : DiagnosticAnalyzerBase<RH0445InheritdocMustBeUsedWithInheritingClassAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0445";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0445InheritdocMustBeUsedWithInheritingClassAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0445Title), nameof(AnalyzerResources.RH0445MessageFormat))
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

        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var inheritdocNode = DocumentationAnalysisUtilities.GetFirstDirectTag(documentationComment, "inheritdoc");
        var expandedDocumentation = DocumentationAnalysisUtilities.GetExpandedDocumentation(declaration, context.SemanticModel, context.CancellationToken);
        var expandedInheritdocElement = DocumentationAnalysisUtilities.GetFirstExpandedElement(expandedDocumentation, "inheritdoc");

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

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.DocumentableDeclarationKinds);
    }

    #endregion // DiagnosticAnalyzer
}