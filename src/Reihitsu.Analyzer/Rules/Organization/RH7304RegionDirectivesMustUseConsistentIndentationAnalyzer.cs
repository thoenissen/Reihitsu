using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Organization;

/// <summary>
/// RH7304: Region directives must use consistent indentation with containing code
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH7304";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Organization, nameof(AnalyzerResources.RH7304Title), nameof(AnalyzerResources.RH7304MessageFormat))
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
        // FindToken performs an O(log n) tree descent to the token at or after the region directive, avoiding a
        // full DescendantTokens() scan for every region pair.
        var regionEnd = regionTrivia.Span.End;

        if (regionEnd >= syntaxRoot.FullSpan.End)
        {
            return null;
        }

        var firstTokenInRegion = syntaxRoot.FindToken(regionEnd);

        if (firstTokenInRegion.IsKind(SyntaxKind.None)
            || firstTokenInRegion.SpanStart < regionEnd
            || firstTokenInRegion.SpanStart >= endRegionTrivia.SpanStart)
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
    /// Analyzes region pairs after excluding directives within element bodies from matching
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var (regionTrivia, endRegionTrivia) in RegionDirectiveUtilities.GetRegionPairs(syntaxRoot,
                                                                                                static directiveTrivia => RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia) == false))
        {
            AnalyzeRegionPair(context, syntaxRoot, sourceText, regionTrivia, endRegionTrivia);
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