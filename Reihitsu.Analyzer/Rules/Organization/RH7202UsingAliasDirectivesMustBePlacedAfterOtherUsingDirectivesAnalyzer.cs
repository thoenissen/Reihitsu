using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7202: Using alias directives must be placed after other using directives
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer : DiagnosticAnalyzerBase<RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7202";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7202Title), nameof(AnalyzerResources.RH7202MessageFormat))
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
            var seenRegularDirective = false;
            var directives = UsingDirectiveOrderingUtilities.GetUsings(context.Node)
                                                            .Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == isGlobalSet)
                                                            .ToList();

            for (var directiveIndex = directives.Count - 1; directiveIndex >= 0; directiveIndex--)
            {
                var usingDirective = directives[directiveIndex];
                var usingDirectiveGroup = UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective);

                if (usingDirectiveGroup is UsingDirectiveOrderingGroup.SystemNamespace
                                        or UsingDirectiveOrderingGroup.OtherNamespace)
                {
                    seenRegularDirective = true;

                    continue;
                }

                if (usingDirectiveGroup == UsingDirectiveOrderingGroup.Alias
                    && seenRegularDirective)
                {
                    context.ReportDiagnostic(CreateDiagnostic(UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective)));
                }
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