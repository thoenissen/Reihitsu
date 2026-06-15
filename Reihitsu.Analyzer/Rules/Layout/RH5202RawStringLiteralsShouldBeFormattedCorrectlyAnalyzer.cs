using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Analyzer that verifies raw string literals have their opening and closing quote markers aligned at the same column
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer>
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
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5202Title), nameof(AnalyzerResources.RH5202MessageFormat))
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

        CheckRawStringAlignment(context, token.GetLocation(), token.Text);
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

        var startToken = interpolatedString.StringStartToken;

        if (startToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken) == false)
        {
            return;
        }

        var endToken = interpolatedString.StringEndToken;

        if (endToken.SyntaxTree == null || startToken.SyntaxTree == null)
        {
            return;
        }

        var startLineSpan = startToken.SyntaxTree.GetLineSpan(startToken.Span);
        var endLineSpan = endToken.SyntaxTree.GetLineSpan(endToken.Span);

        if (startLineSpan.StartLinePosition.Line == endLineSpan.StartLinePosition.Line)
        {
            return;
        }

        var startText = startToken.Text;
        var openingQuoteColumn = startLineSpan.StartLinePosition.Character
                                 + RawStringLiteralUtilities.GetQuoteOffset(startText);
        var endText = endToken.Text;
        var lastNewlineIndex = endText.LastIndexOf('\n');
        var closingLine = lastNewlineIndex >= 0
                              ? endText.Substring(lastNewlineIndex + 1)
                              : endText;
        var closingQuoteColumn = closingLine.Length - closingLine.TrimStart().Length;

        if (openingQuoteColumn != closingQuoteColumn)
        {
            context.ReportDiagnostic(CreateDiagnostic(interpolatedString.GetLocation()));
        }
    }

    /// <summary>
    /// Checks the alignment of a raw string literal token
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="location">The location for the diagnostic</param>
    /// <param name="tokenText">The full text of the raw string literal token</param>
    private void CheckRawStringAlignment(SyntaxNodeAnalysisContext context, Location location, string tokenText)
    {
        var lineSpan = location.GetLineSpan();
        var startLine = lineSpan.StartLinePosition.Line;
        var endLine = lineSpan.EndLinePosition.Line;

        if (startLine == endLine)
        {
            return;
        }

        var openingQuoteColumn = lineSpan.StartLinePosition.Character;

        var lines = tokenText.Split(["\r\n", "\n"], StringSplitOptions.None);
        var lastLine = lines[lines.Length - 1];
        var closingQuoteColumn = lastLine.Length - lastLine.TrimStart().Length;

        if (openingQuoteColumn != closingQuoteColumn)
        {
            context.ReportDiagnostic(CreateDiagnostic(location));
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