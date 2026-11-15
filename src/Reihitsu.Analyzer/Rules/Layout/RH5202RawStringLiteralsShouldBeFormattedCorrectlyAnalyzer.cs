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
    /// (<see cref="RawStringLiteralUtilities.GetClosingLineShift"/>)
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="literal">The raw string literal expression or interpolated raw string expression</param>
    private void CheckRawStringAlignment(SyntaxNodeAnalysisContext context, SyntaxNode literal)
    {
        var sourceText = literal.SyntaxTree.GetText(context.CancellationToken);

        if (RawStringLiteralUtilities.GetClosingLineShift(sourceText, literal) != 0)
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