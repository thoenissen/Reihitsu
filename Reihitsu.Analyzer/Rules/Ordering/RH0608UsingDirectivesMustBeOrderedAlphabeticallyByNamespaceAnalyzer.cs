using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0608: Using directives must be ordered alphabetically by namespace
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer : DiagnosticAnalyzerBase<RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0608";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0608Title), nameof(AnalyzerResources.RH0608MessageFormat))
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