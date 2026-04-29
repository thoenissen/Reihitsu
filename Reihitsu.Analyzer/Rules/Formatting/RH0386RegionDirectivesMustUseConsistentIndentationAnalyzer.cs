using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0386: Region directives must use consistent indentation with containing code
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer : DiagnosticAnalyzerBase<RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0386";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0386Title), nameof(AnalyzerResources.RH0386MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the indentation text used by the first code token inside a region pair
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="regionTrivia">Region trivia</param>
    /// <param name="endRegionTrivia">Endregion trivia</param>
    /// <returns>Indentation text, or <see langword="null"/> if no containing code can be determined</returns>
    internal static string GetExpectedIndentation(SyntaxNode syntaxRoot, SourceText sourceText, SyntaxTrivia regionTrivia, SyntaxTrivia endRegionTrivia)
    {
        var firstTokenInRegion = syntaxRoot.DescendantTokens()
                                           .FirstOrDefault(token => token.SpanStart >= regionTrivia.Span.End
                                                                    && token.SpanStart < endRegionTrivia.SpanStart);

        if (firstTokenInRegion == default || firstTokenInRegion.IsKind(SyntaxKind.None))
        {
            return null;
        }

        return GetIndentation(sourceText, firstTokenInRegion.SpanStart);
    }

    /// <summary>
    /// Gets the indentation text at the given position
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="position">Position</param>
    /// <returns>Indentation text</returns>
    internal static string GetIndentation(SourceText sourceText, int position)
    {
        var line = sourceText.Lines.GetLineFromPosition(position);

        return sourceText.ToString(TextSpan.FromBounds(line.Start, position));
    }

    /// <summary>
    /// Determines whether the directive should be ignored by this rule
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if ignored</returns>
    private static bool ShouldIgnoreDirective(SyntaxTrivia directiveTrivia)
    {
        return RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia);
    }

    /// <summary>
    /// Reports diagnostics for region directives whose indentation differs from the containing code
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="sourceText">Source text</param>
    /// <param name="regionTrivia">Region trivia</param>
    /// <param name="endRegionTrivia">Endregion trivia</param>
    private void AnalyzeRegionPair(SyntaxTreeAnalysisContext context, SyntaxNode syntaxRoot, SourceText sourceText, SyntaxTrivia regionTrivia, SyntaxTrivia endRegionTrivia)
    {
        var expectedIndentation = GetExpectedIndentation(syntaxRoot, sourceText, regionTrivia, endRegionTrivia);

        if (expectedIndentation == null)
        {
            return;
        }

        if (GetIndentation(sourceText, regionTrivia.SpanStart) != expectedIndentation)
        {
            context.ReportDiagnostic(CreateDiagnostic(regionTrivia.GetLocation()));
        }

        if (GetIndentation(sourceText, endRegionTrivia.SpanStart) != expectedIndentation)
        {
            context.ReportDiagnostic(CreateDiagnostic(endRegionTrivia.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var regionStack = new Stack<SyntaxTrivia>();

        foreach (var directiveTrivia in syntaxRoot.DescendantTrivia(descendIntoTrivia: true))
        {
            if (directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                if (ShouldIgnoreDirective(directiveTrivia) == false)
                {
                    regionStack.Push(directiveTrivia);
                }
            }
            else if (directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                if (ShouldIgnoreDirective(directiveTrivia) || regionStack.Count == 0)
                {
                    continue;
                }

                AnalyzeRegionPair(context, syntaxRoot, sourceText, regionStack.Pop(), directiveTrivia);
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