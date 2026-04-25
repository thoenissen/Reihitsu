using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0609: Using alias directives must be ordered alphabetically by alias name.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer : DiagnosticAnalyzerBase<RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0609";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0609Title), nameof(AnalyzerResources.RH0609MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the using directive scope.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUsingScope(SyntaxNodeAnalysisContext context)
    {
        foreach (var isGlobalSet in new[] { false, true })
        {
            string previousSortKey = null;

            foreach (var usingDirective in UsingDirectiveOrderingUtilities.GetUsings(context.Node)
                                                                          .Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == isGlobalSet)
                                                                          .Where(obj => UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj) == UsingDirectiveOrderingGroup.Alias))
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