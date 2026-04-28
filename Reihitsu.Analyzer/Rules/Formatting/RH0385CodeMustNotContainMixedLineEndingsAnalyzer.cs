using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0385: Code must not contain mixed line endings
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0385CodeMustNotContainMixedLineEndingsAnalyzer : DiagnosticAnalyzerBase<RH0385CodeMustNotContainMixedLineEndingsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0385";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0385CodeMustNotContainMixedLineEndingsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0385Title), nameof(AnalyzerResources.RH0385MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the predominant line ending used by the file
    /// </summary>
    /// <param name="endOfLineTrivia">End-of-line trivia in source order</param>
    /// <param name="counts">Line-ending counts</param>
    /// <returns>The predominant line ending</returns>
    private static string GetPredominantLineEnding(IReadOnlyList<SyntaxTrivia> endOfLineTrivia, IReadOnlyDictionary<string, int> counts)
    {
        var predominantCount = 0;

        foreach (var currentCount in counts.Values)
        {
            if (currentCount > predominantCount)
            {
                predominantCount = currentCount;
            }
        }

        foreach (var trivia in endOfLineTrivia)
        {
            var lineEnding = trivia.ToString();

            if (counts[lineEnding] == predominantCount)
            {
                return lineEnding;
            }
        }

        return endOfLineTrivia[0].ToString();
    }

    /// <summary>
    /// Analyzes the syntax tree for mixed line endings
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var endOfLineTrivia = new List<SyntaxTrivia>();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                continue;
            }

            var lineEnding = trivia.ToString();

            endOfLineTrivia.Add(trivia);
            counts[lineEnding] = counts.TryGetValue(lineEnding, out var count)
                                     ? count + 1
                                     : 1;
        }

        if (counts.Count <= 1)
        {
            return;
        }

        var predominantLineEnding = GetPredominantLineEnding(endOfLineTrivia, counts);

        foreach (var trivia in endOfLineTrivia)
        {
            if (trivia.ToString() != predominantLineEnding)
            {
                var line = sourceText.Lines.GetLineFromPosition(trivia.SpanStart);

                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(line.Start, line.EndIncludingLineBreak))));
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