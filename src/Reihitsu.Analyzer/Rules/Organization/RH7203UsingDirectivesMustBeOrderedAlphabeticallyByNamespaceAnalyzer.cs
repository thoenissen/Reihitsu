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
        : base(DiagnosticId, DiagnosticCategory.Organization)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze the using directive scope: every regular namespace directive whose sort key comes before the key of an earlier
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
                                                                      .Where(obj => UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj) is UsingDirectiveOrderingGroup.SystemNamespace or UsingDirectiveOrderingGroup.OtherNamespace))
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