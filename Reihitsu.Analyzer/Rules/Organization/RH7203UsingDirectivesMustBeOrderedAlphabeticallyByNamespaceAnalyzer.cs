using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7203: Using directives must be ordered alphabetically by namespace
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7203";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7203Title), nameof(AnalyzerResources.RH7203MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the using directive scope
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUsingScope(SyntaxNodeAnalysisContext context)
    {
        foreach (var isGlobalSet in new[] { false, true })
        {
            AnalyzeGroup(context, isGlobalSet, UsingDirectiveOrderingGroup.SystemNamespace);
            AnalyzeGroup(context, isGlobalSet, UsingDirectiveOrderingGroup.OtherNamespace);
        }
    }

    /// <summary>
    /// Analyze a single using directive group
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="isGlobalSet">Whether the directives are global</param>
    /// <param name="usingDirectiveGroup">Group to analyze</param>
    private void AnalyzeGroup(SyntaxNodeAnalysisContext context, bool isGlobalSet, UsingDirectiveOrderingGroup usingDirectiveGroup)
    {
        string previousSortKey = null;

        foreach (var usingDirective in UsingDirectiveOrderingUtilities.GetUsings(context.Node)
                                                                      .Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == isGlobalSet)
                                                                      .Where(obj => UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj) == usingDirectiveGroup))
        {
            var currentSortKey = UsingDirectiveOrderingUtilities.GetSortKey(usingDirective);

            if (previousSortKey != null
                && UsingDirectiveOrderingUtilities.CompareSortKeys(currentSortKey, previousSortKey) < 0)
            {
                context.ReportDiagnostic(CreateDiagnostic(UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective)));
            }

            previousSortKey = currentSortKey;
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnUsingScope, SyntaxKind.CompilationUnit, SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}