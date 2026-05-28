using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7204: Using alias directives must be ordered alphabetically by alias name
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer : DiagnosticAnalyzerBase<RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7204";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7204Title), nameof(AnalyzerResources.RH7204MessageFormat))
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