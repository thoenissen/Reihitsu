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
public class RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer : DiagnosticAnalyzerBase
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
    /// Analyze the using directive scope: every alias directive whose sort key comes before the key of an earlier
    /// directive of its group is reported, whatever lies between them
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUsingScope(SyntaxNodeAnalysisContext context)
    {
        var usingDirectives = UsingDirectiveOrderingUtilities.GetUsings(context.Node);

        if (UsingDirectiveOrderingSafety.CanSafelyReorder(usingDirectives) == false)
        {
            return;
        }

        foreach (var usingDirective in UsingDirectiveOrderingUtilities.GetDirectivesOutOfOrderWithinTheirGroup(usingDirectives)
                                                                      .Where(obj => UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj) == UsingDirectiveOrderingGroup.Alias))
        {
            context.ReportDiagnostic(CreateDiagnostic(UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective)));
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