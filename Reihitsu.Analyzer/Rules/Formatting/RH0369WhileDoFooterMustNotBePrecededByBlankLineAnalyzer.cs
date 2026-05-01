using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0369: While-do footer must not be preceded by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase<RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0369";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0369WhileDoFooterMustNotBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0369Title), nameof(AnalyzerResources.RH0369MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDoStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DoStatementSyntax doStatement)
        {
            return;
        }

        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var whileLineIndex = sourceText.Lines.GetLineFromPosition(doStatement.WhileKeyword.SpanStart).LineNumber;

        if (whileLineIndex == 0
            || FormattingTextAnalysisUtilities.IsBlankLine(sourceText, whileLineIndex - 1) == false)
        {
            return;
        }

        var blankLine = sourceText.Lines[whileLineIndex - 1];

        context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(blankLine.Start, blankLine.EndIncludingLineBreak))));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDoStatement, SyntaxKind.DoStatement);
    }

    #endregion // DiagnosticAnalyzer
}