using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5022: Opening brace must not be followed by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5022";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5022Title), nameof(AnalyzerResources.RH5022MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var openBraceLineIndices = FormattingTextAnalysisUtilities.GetLineIndicesEndingWithToken(root,
                                                                                                 sourceText,
                                                                                                 static token => token.IsKind(SyntaxKind.OpenBraceToken));

        foreach (var blankLine in FormattingTextAnalysisUtilities.EnumerateFollowingBlankLines(sourceText, openBraceLineIndices))
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(blankLine.Start, blankLine.EndIncludingLineBreak))));
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