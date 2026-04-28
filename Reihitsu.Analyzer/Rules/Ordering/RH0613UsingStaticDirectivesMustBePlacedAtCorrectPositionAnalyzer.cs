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
/// RH0613: Using static directives must be placed at correct position
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
    /// Analyze the using directive scope
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUsingScope(SyntaxNodeAnalysisContext context)
    {
        foreach (var isGlobalSet in new[] { false, true })
        {
            var directives = UsingDirectiveOrderingUtilities.GetUsings(context.Node)
                                                            .Where(obj => UsingDirectiveOrderingUtilities.IsGlobalUsing(obj) == isGlobalSet)
                                                            .ToList();

            foreach (var usingDirective in GetViolatingDirectives(directives))
            {
                context.ReportDiagnostic(CreateDiagnostic(UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective)));
            }
        }
    }

    /// <summary>
    /// Gets all static using directives that violate their required ordering
    /// </summary>
    /// <param name="directives">Using directives</param>
    /// <returns>Violating directives</returns>
    private IEnumerable<UsingDirectiveSyntax> GetViolatingDirectives(IReadOnlyList<UsingDirectiveSyntax> directives)
    {
        var violations = new HashSet<UsingDirectiveSyntax>();

        AddViolationsAfterAlias(directives, violations);
        AddViolationsBeforeRegularUsings(directives, violations);

        return violations;
    }

    /// <summary>
    /// Adds static-using violations that appear after alias usings
    /// </summary>
    /// <param name="directives">Using directives</param>
    /// <param name="violations">Violation set</param>
    private void AddViolationsAfterAlias(IReadOnlyList<UsingDirectiveSyntax> directives, ISet<UsingDirectiveSyntax> violations)
    {
        var seenAliasDirective = false;

        foreach (var usingDirective in directives)
        {
            var group = UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective);

            if (group == UsingDirectiveOrderingGroup.Alias)
            {
                seenAliasDirective = true;

                continue;
            }

            if (group == UsingDirectiveOrderingGroup.Static && seenAliasDirective)
            {
                violations.Add(usingDirective);
            }
        }
    }

    /// <summary>
    /// Adds static-using violations that appear before regular namespace usings
    /// </summary>
    /// <param name="directives">Using directives</param>
    /// <param name="violations">Violation set</param>
    private void AddViolationsBeforeRegularUsings(IReadOnlyList<UsingDirectiveSyntax> directives, ISet<UsingDirectiveSyntax> violations)
    {
        var seenRegularDirective = false;

        for (var directiveIndex = directives.Count - 1; directiveIndex >= 0; directiveIndex--)
        {
            var usingDirective = directives[directiveIndex];
            var group = UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective);

            if (group is UsingDirectiveOrderingGroup.SystemNamespace or UsingDirectiveOrderingGroup.OtherNamespace)
            {
                seenRegularDirective = true;

                continue;
            }

            if (group == UsingDirectiveOrderingGroup.Static && seenRegularDirective)
            {
                violations.Add(usingDirective);
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