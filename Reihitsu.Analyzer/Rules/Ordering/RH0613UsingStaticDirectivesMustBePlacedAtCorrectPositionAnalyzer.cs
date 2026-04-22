using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// RH0613: Using static directives must be placed at correct position.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer : DiagnosticAnalyzerBase<RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0613";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0613UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Ordering, nameof(AnalyzerResources.RH0613Title), nameof(AnalyzerResources.RH0613MessageFormat))
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
            var directives = UsingDirectiveOrderingUtilities.GetUsings(context.Node)
                                                            .Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == isGlobalSet)
                                                            .ToList();
            var violatingDirectives = new HashSet<SyntaxNode>();
            var seenAliasDirective = false;

            foreach (var usingDirective in directives)
            {
                var usingDirectiveGroup = UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective);

                if (usingDirectiveGroup == UsingDirectiveOrderingGroup.Alias)
                {
                    seenAliasDirective = true;
                }
                else if (usingDirectiveGroup == UsingDirectiveOrderingGroup.Static
                         && seenAliasDirective)
                {
                    violatingDirectives.Add(usingDirective);
                }
            }

            var seenRegularDirective = false;

            for (var directiveIndex = directives.Count - 1; directiveIndex >= 0; directiveIndex--)
            {
                var usingDirective = directives[directiveIndex];
                var usingDirectiveGroup = UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective);

                if (usingDirectiveGroup is UsingDirectiveOrderingGroup.SystemNamespace
                                        or UsingDirectiveOrderingGroup.OtherNamespace)
                {
                    seenRegularDirective = true;
                }
                else if (usingDirectiveGroup == UsingDirectiveOrderingGroup.Static
                         && seenRegularDirective)
                {
                    violatingDirectives.Add(usingDirective);
                }
            }

            foreach (var usingDirective in violatingDirectives.OfType<UsingDirectiveSyntax>())
            {
                context.ReportDiagnostic(CreateDiagnostic(UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective)));
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