using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0822: Collection expressions should be formatted correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0822";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0822CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0822Title), nameof(AnalyzerResources.RH0822MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.CollectionExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCollectionExpression(SyntaxNodeAnalysisContext context)
    {
        var collectionExpression = (CollectionExpressionSyntax)context.Node;
        var openBracketPosition = collectionExpression.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition;
        var closeBracketPosition = collectionExpression.CloseBracketToken.GetLocation().GetLineSpan().StartLinePosition;
        var elementLinePositions = collectionExpression.Elements.Select(element => element.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition).ToArray();
        var isSingleLineCollection = openBracketPosition.Line == closeBracketPosition.Line;

        // Rule 1: Single-line collection expressions are allowed only when every element
        // is also on that same line.
        if (isSingleLineCollection)
        {
            if (elementLinePositions.Any(position => position.Line != openBracketPosition.Line))
            {
                context.ReportDiagnostic(CreateDiagnostic(collectionExpression.GetLocation()));
            }

            return;
        }

        // Rule 2: In multi-line form, opening and closing brackets must be in the same column.
        if (openBracketPosition.Character != closeBracketPosition.Character)
        {
            context.ReportDiagnostic(CreateDiagnostic(collectionExpression.GetLocation()));

            return;
        }

        // Rule 3: In multi-line form, every collection element must be on its own line between
        // the opening and closing brackets.
        if (elementLinePositions.Any(position => position.Line <= openBracketPosition.Line || position.Line >= closeBracketPosition.Line)
            || elementLinePositions.Select(position => position.Line).Distinct().Count() != elementLinePositions.Length)
        {
            context.ReportDiagnostic(CreateDiagnostic(collectionExpression.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnCollectionExpression, SyntaxKind.CollectionExpression);
    }

    #endregion // DiagnosticAnalyzer
}