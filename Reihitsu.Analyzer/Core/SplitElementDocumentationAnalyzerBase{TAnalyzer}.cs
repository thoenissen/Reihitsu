using System.Collections.Immutable;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared base class for the split declaration documentation analyzers
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class SplitElementDocumentationAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// Accessibility group enforced by the derived analyzer
    /// </summary>
    private readonly DocumentationAccessibilityGroup _accessibilityGroup;

    /// <summary>
    /// Syntax kinds handled by the derived analyzer
    /// </summary>
    private readonly ImmutableArray<SyntaxKind> _syntaxKinds;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitElementDocumentationAnalyzerBase{TAnalyzer}"/> class
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    /// <param name="accessibilityGroup">Accessibility group</param>
    /// <param name="syntaxKinds">Syntax kinds to analyze</param>
    protected SplitElementDocumentationAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName, DocumentationAccessibilityGroup accessibilityGroup, params SyntaxKind[] syntaxKinds)
        : base(diagnosticId, DiagnosticCategory.Documentation, titleResourceName, messageFormatResourceName)
    {
        _accessibilityGroup = accessibilityGroup;
        _syntaxKinds = [.. syntaxKinds];
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a declaration
    /// </summary>
    /// <param name="context">Analysis context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.NeedsDocumentation(declaration) == false
            || DocumentationAnalysisUtilities.MatchesAccessibilityGroup(declaration, _accessibilityGroup) == false
            || DocumentationAnalysisUtilities.HasRequiredDocumentation(declaration, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(DocumentationAnalysisUtilities.GetDiagnosticLocation(declaration)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, _syntaxKinds);
    }

    #endregion // DiagnosticAnalyzer
}