using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7207: Using directives should be organized into groups
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7207";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7207Title), nameof(AnalyzerResources.RH7207MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the using directive has a blank line before it (based on its leading trivia)
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns><see langword="true"/> if there is a blank line before the directive</returns>
    private static bool HasBlankLineBefore(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
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

        var canonical = UsingDirectiveOrderingUtilities.ComputeCanonicalOrder(usings);

        for (var index = 0; index < usings.Count; index++)
        {
            if (ReferenceEquals(usings[index], canonical[index]) == false)
            {
                return false;
            }
        }

        for (var index = 1; index < usings.Count; index++)
        {
            var sameGroup = UsingDirectiveOrderingUtilities.AreInSameGroup(usings[index - 1], usings[index]);
            var hasBlankLine = HasBlankLineBefore(usings[index]);

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