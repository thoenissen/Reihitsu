using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5027: While-do footer must not be preceded by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5027";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5027Title), nameof(AnalyzerResources.RH5027MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the <c>while</c> keyword's line is the footer's line in the sense of RH5027-001: nothing but
    /// whitespace, or whitespace and the closing brace of the statement's block body, stands in front of the keyword on its
    /// line. Anything else in front of it — code, a comment, or the end of a literal or a comment that started on an earlier
    /// line — makes the line above part of that code or text rather than the footer's gap
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="doStatement">The <c>do</c> statement</param>
    /// <param name="whileLine">The line that holds the <c>while</c> keyword</param>
    /// <returns><see langword="true"/> if the line starts with the footer</returns>
    private static bool StartsWithFooter(SourceText sourceText, DoStatementSyntax doStatement, TextLine whileLine)
    {
        var whileStart = doStatement.WhileKeyword.SpanStart;

        if (FormattingTextAnalysisUtilities.IsWhitespaceOnly(sourceText, TextSpan.FromBounds(whileLine.Start, whileStart)))
        {
            return true;
        }

        return doStatement.Statement is BlockSyntax block
               && block.CloseBraceToken.SpanStart >= whileLine.Start
               && FormattingTextAnalysisUtilities.IsWhitespaceOnly(sourceText, TextSpan.FromBounds(whileLine.Start, block.CloseBraceToken.SpanStart))
               && FormattingTextAnalysisUtilities.IsWhitespaceOnly(sourceText, TextSpan.FromBounds(block.CloseBraceToken.Span.End, whileStart));
    }

    /// <summary>
    /// Analyzes a <c>do</c> statement and reports the blank line directly above its footer's line
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDoStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DoStatementSyntax doStatement)
        {
            return;
        }

        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var whileLine = sourceText.Lines.GetLineFromPosition(doStatement.WhileKeyword.SpanStart);
        var whileLineIndex = whileLine.LineNumber;

        if (whileLineIndex == 0
            || StartsWithFooter(sourceText, doStatement, whileLine) == false
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