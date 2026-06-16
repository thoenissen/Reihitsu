using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6021: Colons must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6021ColonsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH6021ColonsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6021";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6021ColonsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6021Title), nameof(AnalyzerResources.RH6021MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the colon token for base lists and constructor initializers
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <returns>The colon token, or <see langword="default"/> if the node does not carry a spaced colon</returns>
    private static SyntaxToken GetColonToken(SyntaxNode node)
    {
        return node switch
               {
                   BaseListSyntax baseList => baseList.ColonToken,
                   ConstructorInitializerSyntax constructorInitializer => constructorInitializer.ColonToken,
                   _ => default
               };
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var node in root.DescendantNodes())
        {
            var token = GetColonToken(node);

            if (token.IsKind(SyntaxKind.ColonToken) == false)
            {
                continue;
            }

            // The formatter only normalizes the space when the colon and its neighbour share a line. When the
            // colon starts a continuation line (or its neighbour is on the next line), the indentation handling
            // owns the layout, so the analyzer must not flag the missing space at the line boundary.
            var hasLeadingLineBreak = token.GetPreviousToken().TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                                      || token.LeadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
            var hasTrailingLineBreak = token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                                       || token.GetNextToken().LeadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            var hasLeadingSpace = hasLeadingLineBreak || (token.SpanStart > 0 && sourceText[token.SpanStart - 1] == ' ');
            var hasTrailingSpace = hasTrailingLineBreak || (token.Span.End < sourceText.Length && sourceText[token.Span.End] == ' ');

            if (hasLeadingSpace == false || hasTrailingSpace == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}