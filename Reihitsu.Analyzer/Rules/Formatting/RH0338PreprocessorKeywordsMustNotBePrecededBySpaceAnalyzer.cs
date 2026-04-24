using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0338: Preprocessor keywords must not be preceded by space.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer : DiagnosticAnalyzerBase<RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0338";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0338PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0338Title), nameof(AnalyzerResources.RH0338MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var line in sourceText.Lines)
        {
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var trimmed = lineText.TrimStart();

            if (trimmed.StartsWith("#", StringComparison.Ordinal) == false)
            {
                continue;
            }

            var indentLength = lineText.Length - trimmed.Length;

            if (indentLength > 0)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(line.Start, line.Start + indentLength))));
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