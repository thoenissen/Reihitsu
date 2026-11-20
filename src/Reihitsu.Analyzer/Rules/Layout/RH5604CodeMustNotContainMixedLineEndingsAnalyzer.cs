using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5604: Code must not contain mixed line endings
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5604CodeMustNotContainMixedLineEndingsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5604";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5604CodeMustNotContainMixedLineEndingsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree for mixed line endings: the line breaks the tree carries
    /// (<see cref="LineEndingUtilities.GetLineBreakTrivia"/>, <see cref="LineEndingUtilities.GetLineBreakTokens"/>) are mixed
    /// when they have more than one distinct text, and then every line break that differs from the predominant line ending
    /// (<see cref="LineEndingUtilities.DetectEndOfLine"/>) is reported on its line
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var endOfLineTrivia = new List<SyntaxTrivia>();
        var documentationEndOfLineTokens = new List<SyntaxToken>();
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var trivia in LineEndingUtilities.GetLineBreakTrivia(root))
        {
            var lineEnding = trivia.ToString();

            endOfLineTrivia.Add(trivia);
            counts[lineEnding] = counts.TryGetValue(lineEnding, out var count)
                                     ? count + 1
                                     : 1;
        }

        foreach (var token in LineEndingUtilities.GetLineBreakTokens(root))
        {
            documentationEndOfLineTokens.Add(token);
            counts[token.Text] = counts.TryGetValue(token.Text, out var count)
                                     ? count + 1
                                     : 1;
        }

        if (counts.Count <= 1)
        {
            return;
        }

        var predominantLineEnding = LineEndingUtilities.DetectEndOfLine(root);

        foreach (var trivia in endOfLineTrivia.Where(trivia => trivia.ToString() != predominantLineEnding))
        {
            var line = sourceText.Lines.GetLineFromPosition(trivia.SpanStart);

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(line.Start, line.EndIncludingLineBreak))));
        }

        foreach (var token in documentationEndOfLineTokens.Where(token => token.Text != predominantLineEnding))
        {
            var line = sourceText.Lines.GetLineFromPosition(token.SpanStart);

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(line.Start, line.EndIncludingLineBreak))));
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