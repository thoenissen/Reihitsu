using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5114: Using directives must be placed on separate lines
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5114";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5114Title), nameof(AnalyzerResources.RH5114MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a using directive starts on the line on which its predecessor ends. The line of
    /// the predecessor's terminating semicolon is compared with the line of the directive's first token,
    /// so a line break inside a comment between the two — a single-line documentation comment or a block
    /// comment that spans lines — separates them just like an ordinary line break
    /// </summary>
    /// <param name="previousDirective">Preceding using directive in the same list</param>
    /// <param name="currentDirective">Using directive to inspect</param>
    /// <returns><see langword="true"/> if both directives share a line; otherwise, <see langword="false"/></returns>
    internal static bool StartsOnLineOfPreviousDirective(UsingDirectiveSyntax previousDirective, UsingDirectiveSyntax currentDirective)
    {
        var previousEndLine = previousDirective.GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;
        var currentStartLine = currentDirective.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;

        return previousEndLine == currentStartLine;
    }

    /// <summary>
    /// Reports every using directive that starts on the line of the preceding directive in the same list
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="usingDirectives">Using directives of one scope</param>
    private void AnalyzeUsingDirectives(SyntaxNodeAnalysisContext context, SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        for (var directiveIndex = 1; directiveIndex < usingDirectives.Count; directiveIndex++)
        {
            if (StartsOnLineOfPreviousDirective(usingDirectives[directiveIndex - 1], usingDirectives[directiveIndex]))
            {
                context.ReportDiagnostic(CreateDiagnostic(usingDirectives[directiveIndex].GetLocation()));
            }
        }
    }

    /// <summary>
    /// Analyzes the using directives of a compilation unit
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is CompilationUnitSyntax compilationUnit)
        {
            AnalyzeUsingDirectives(context, compilationUnit.Usings);
        }
    }

    /// <summary>
    /// Analyzes the using directives of a block-scoped or file-scoped namespace
    /// </summary>
    /// <param name="context">Context</param>
    private void OnNamespaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is BaseNamespaceDeclarationSyntax namespaceDeclaration)
        {
            AnalyzeUsingDirectives(context, namespaceDeclaration.Usings);
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnCompilationUnit, SyntaxKind.CompilationUnit);
        context.RegisterSyntaxNodeAction(OnNamespaceDeclaration, SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}