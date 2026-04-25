using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0614: Using static directives must be ordered alphabetically.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer : DiagnosticAnalyzerBase<RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0614";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0614Title), nameof(AnalyzerResources.RH0614MessageFormat))
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
                                                                          .Where(obj => UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj) == UsingDirectiveOrderingGroup.Static))
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