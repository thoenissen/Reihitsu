using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Analyzer that verifies multi-line raw string literals have their closing marker in the column of their opening quotes,
/// and written there behind whitespace without a tab (RH5202-009)
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5202";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing multi-line raw string literals for correct formatting
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStringLiteralExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LiteralExpressionSyntax literalExpression)
        {
            return;
        }

        var token = literalExpression.Token;

        if (token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken) == false
            && token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken) == false)
        {
            return;
        }

        CheckRawStringAlignment(context, literalExpression);
    }

    /// <summary>
    /// Analyzing interpolated multi-line raw string expressions for correct formatting
    /// </summary>
    /// <param name="context">Context</param>
    private void OnInterpolatedStringExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InterpolatedStringExpressionSyntax interpolatedString)
        {
            return;
        }

        if (interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == false)
        {
            return;
        }

        CheckRawStringAlignment(context, interpolatedString);
    }

    /// <summary>
    /// Checks the alignment of a multi-line raw string literal: it is reported when its closing marker does not start
    /// in the column of its opening quotes, measured as the code fix measures it
    /// (<see cref="RawStringLiteralUtilities.GetClosingLineShift"/>), or when it starts there behind whitespace that holds
    /// a tab (<see cref="RawStringLiteralUtilities.HasTabInLeadingWhitespace"/>, RH5202-009), whose stripped indentation
    /// the code fix and the formatter write as spaces. Its callers pass only literals of the multi-line raw kinds, whose
    /// closing marker always stands on a later line than their opening quotes
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="literal">The raw string literal expression or interpolated raw string expression</param>
    private void CheckRawStringAlignment(SyntaxNodeAnalysisContext context, SyntaxNode literal)
    {
        var sourceText = literal.SyntaxTree.GetText(context.CancellationToken);
        var closingLine = sourceText.Lines.GetLineFromPosition(literal.Span.End);

        if (RawStringLiteralUtilities.GetClosingLineShift(sourceText, literal) != 0
            || RawStringLiteralUtilities.HasTabInLeadingWhitespace(sourceText.ToString(closingLine.Span)))
        {
            context.ReportDiagnostic(CreateDiagnostic(literal.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnStringLiteralExpression, SyntaxKind.StringLiteralExpression, SyntaxKind.Utf8StringLiteralExpression);
        context.RegisterSyntaxNodeAction(OnInterpolatedStringExpression, SyntaxKind.InterpolatedStringExpression);
    }

    #endregion // DiagnosticAnalyzer
}