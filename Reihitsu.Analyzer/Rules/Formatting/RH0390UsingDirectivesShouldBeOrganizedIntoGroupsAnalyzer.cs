using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0390: Using directives should be organized into groups
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer : DiagnosticAnalyzerBase<RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0390";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0390Title), nameof(AnalyzerResources.RH0390MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Computes the canonical (sorted and grouped) order of the given using directives
    /// </summary>
    /// <param name="usings">Using directives</param>
    /// <returns>Canonically ordered list</returns>
    internal static List<UsingDirectiveSyntax> ComputeCanonicalOrder(SyntaxList<UsingDirectiveSyntax> usings)
    {
        return usings.Select((usingDirective, directiveIndex) => new
                                                                 {
                                                                     UsingDirective = usingDirective,
                                                                     DirectiveIndex = directiveIndex,
                                                                 })
                     .OrderBy(obj => GetUsingTypeOrder(obj.UsingDirective))
                     .ThenBy(obj => GetNamespaceGroupOrderKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                     .ThenBy(obj => UsingDirectiveOrderingUtilities.GetSortKey(obj.UsingDirective), StringComparer.OrdinalIgnoreCase)
                     .ThenBy(obj => obj.DirectiveIndex)
                     .Select(obj => obj.UsingDirective)
                     .ToList();
    }

    /// <summary>
    /// Determines whether two using directives belong to the same formatting group
    /// </summary>
    /// <param name="left">Left using directive</param>
    /// <param name="right">Right using directive</param>
    /// <returns><see langword="true"/> if both directives belong to the same group</returns>
    internal static bool AreInSameGroup(UsingDirectiveSyntax left, UsingDirectiveSyntax right)
    {
        return GetUsingTypeOrder(left) == GetUsingTypeOrder(right)
               && string.Equals(GetRootNamespace(left), GetRootNamespace(right), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the root namespace (first segment before the first dot) for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>Root namespace segment</returns>
    internal static string GetRootNamespace(UsingDirectiveSyntax usingDirective)
    {
        var name = usingDirective.Name?.ToString() ?? string.Empty;
        var dotIndex = name.IndexOf('.');

        return dotIndex >= 0 ? name.Substring(0, dotIndex) : name;
    }

    /// <summary>
    /// Gets the namespace ordering key for a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The namespace ordering key</returns>
    internal static string GetNamespaceGroupOrderKey(UsingDirectiveSyntax usingDirective)
    {
        var rootNamespace = GetRootNamespace(usingDirective);

        return string.Equals(rootNamespace, "System", StringComparison.OrdinalIgnoreCase)
                   ? string.Empty
                   : rootNamespace;
    }

    /// <summary>
    /// Gets the using-type ordering slot
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The using-type ordering slot</returns>
    internal static int GetUsingTypeOrder(UsingDirectiveSyntax usingDirective)
    {
        return UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective) switch
               {
                   UsingDirectiveOrderingGroup.Static => 1,
                   UsingDirectiveOrderingGroup.Alias => 2,
                   _ => 0,
               };
    }

    /// <summary>
    /// Determines whether the using directive has a blank line before it (based on its leading trivia)
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if there is a blank line before the directive</returns>
    private static bool HasBlankLineBefore(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Determines whether the given using directives are properly organized into groups
    /// </summary>
    /// <param name="usings">Using directives</param>
    /// <returns><see langword="true"/> if the usings are already organized correctly</returns>
    private static bool IsOrganized(SyntaxList<UsingDirectiveSyntax> usings)
    {
        if (usings.Count < 2)
        {
            return true;
        }

        var canonical = ComputeCanonicalOrder(usings);

        for (var i = 0; i < usings.Count; i++)
        {
            if (ReferenceEquals(usings[i], canonical[i]) == false)
            {
                return false;
            }
        }

        for (var i = 1; i < usings.Count; i++)
        {
            var sameGroup = AreInSameGroup(usings[i - 1], usings[i]);
            var hasBlankLine = HasBlankLineBefore(usings[i]);

            if (sameGroup == hasBlankLine)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the using directives from the given scope node
    /// </summary>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <returns>Using directives</returns>
    private static SyntaxList<UsingDirectiveSyntax> GetUsings(SyntaxNode scope)
    {
        return scope switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.Usings,
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Usings,
                   _ => default,
               };
    }

    /// <summary>
    /// Analyzes the using directive scope
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUsingScope(SyntaxNodeAnalysisContext context)
    {
        var usings = GetUsings(context.Node);

        if (usings.Count < 2)
        {
            return;
        }

        if (IsOrganized(usings) == false)
        {
            var firstUsing = usings[0];
            var location = firstUsing.Name?.GetLocation() ?? firstUsing.GetLocation();

            context.ReportDiagnostic(CreateDiagnostic(location));
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