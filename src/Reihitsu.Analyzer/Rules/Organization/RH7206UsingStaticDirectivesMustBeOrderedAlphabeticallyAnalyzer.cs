using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7206: Using static directives must be ordered alphabetically
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7206";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the using directive scope: every <c>using static</c> directive whose sort key comes before the key of an earlier
    /// directive of its group is reported, whatever lies between them
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUsingScope(SyntaxNodeAnalysisContext context)
    {
        var usingDirectives = UsingDirectiveOrderingUtilities.GetUsings(context.Node);

        foreach (var usingDirective in UsingDirectiveOrderingUtilities.GetDirectivesOutOfOrderWithinTheirGroup(usingDirectives)
                                                                      .Where(obj => UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj) == UsingDirectiveOrderingGroup.Static))
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