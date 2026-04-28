using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0606: System using directives must be placed before other using directives
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer : DiagnosticAnalyzerBase<RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0606";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0606SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0606Title), nameof(AnalyzerResources.RH0606MessageFormat))
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