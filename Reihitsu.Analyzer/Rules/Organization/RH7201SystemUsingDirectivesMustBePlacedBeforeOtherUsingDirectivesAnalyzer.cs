using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7201: System using directives must be placed before other using directives
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7201";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7201Title), nameof(AnalyzerResources.RH7201MessageFormat))
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
            var seenNonSystemDirective = false;
            var directives = UsingDirectiveOrderingUtilities.GetUsings(context.Node)
                                                            .Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == isGlobalSet)
                                                            .Where(obj =>
                                                                   {
                                                                       var usingDirectiveGroup = UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(obj);

                                                                       return usingDirectiveGroup is UsingDirectiveOrderingGroup.SystemNamespace
                                                                                                  or UsingDirectiveOrderingGroup.OtherNamespace;
                                                                   });

            foreach (var usingDirective in directives)
            {
                if (UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirective))
                {
                    if (seenNonSystemDirective)
                    {
                        context.ReportDiagnostic(CreateDiagnostic(UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective)));
                    }

                    continue;
                }

                seenNonSystemDirective = true;
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