using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5604Title), nameof(AnalyzerResources.RH5604MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

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

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true).Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
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

        var predominantLineEnding = LineEndingUtilities.DetectEndOfLine(root);

        foreach (var trivia in endOfLineTrivia.Where(trivia => trivia.ToString() != predominantLineEnding))
        {
            var line = sourceText.Lines.GetLineFromPosition(trivia.SpanStart);

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