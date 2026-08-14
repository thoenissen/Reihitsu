using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5303: The collection initializer should be formatted correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5303";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5303Title), nameof(AnalyzerResources.RH5303MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.CollectionInitializerExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCollectionInitializerExpression(SyntaxNodeAnalysisContext context)
    {
        var collectionInitializer = (InitializerExpressionSyntax)context.Node;

        switch (collectionInitializer.Parent)
        {
            case ObjectCreationExpressionSyntax { Initializer: not null } objectCreationExpression:
                AnalyzeCollectionInitializer(context, objectCreationExpression, objectCreationExpression.NewKeyword, objectCreationExpression.Initializer);
                break;

            case ImplicitObjectCreationExpressionSyntax { Initializer: not null } implicitObjectCreationExpression:
                AnalyzeCollectionInitializer(context, implicitObjectCreationExpression, implicitObjectCreationExpression.NewKeyword, implicitObjectCreationExpression.Initializer);
                break;
        }
    }

    /// <summary>
    /// Analyzing the braces and elements of a collection initializer
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="newKeyword">The <c>new</c> keyword</param>
    /// <param name="collectionInitializer">Collection initializer</param>
    private void AnalyzeCollectionInitializer(SyntaxNodeAnalysisContext context, SyntaxNode diagnosticNode, SyntaxToken newKeyword, InitializerExpressionSyntax collectionInitializer)
    {
        var newKeywordPosition = newKeyword.GetLocation().GetLineSpan().StartLinePosition;

        if (CheckBraces(context, newKeywordPosition, diagnosticNode, collectionInitializer))
        {
            CheckElements(context, newKeywordPosition, collectionInitializer);
        }
    }

    /// <summary>
    /// Checking the opening and closing braces
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="collectionInitializer">Collection initializer</param>
    /// <returns>Are all braces valid?</returns>
    private bool CheckBraces(SyntaxNodeAnalysisContext context, LinePosition newKeywordPosition, SyntaxNode diagnosticNode, InitializerExpressionSyntax collectionInitializer)
    {
        if (InitializerLayoutAnalysisUtilities.IsAlignedAt(collectionInitializer.OpenBraceToken, newKeywordPosition.Character)
            && InitializerLayoutAnalysisUtilities.IsAlignedAt(collectionInitializer.CloseBraceToken, newKeywordPosition.Character))
        {
            return true;
        }

        context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

        return false;
    }

    /// <summary>
    /// Checking the collection initializer elements
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="anchorPosition">Position that owns the initializer's indentation level</param>
    /// <param name="collectionInitializer">Collection initializer</param>
    private void CheckElements(SyntaxNodeAnalysisContext context, LinePosition anchorPosition, InitializerExpressionSyntax collectionInitializer)
    {
        var openBraceLine = collectionInitializer.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeBraceLine = collectionInitializer.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var expressionLines = new HashSet<int>();

        foreach (var expression in collectionInitializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();
            var expressionLine = firstToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (expressionLine <= openBraceLine
                || expressionLine >= closeBraceLine
                || expressionLines.Add(expressionLine) == false
                || InitializerLayoutAnalysisUtilities.IsAlignedAt(firstToken, anchorPosition.Character + SyntaxIndentationUtilities.IndentSize) == false)
            {
                // Report at the offending element so multiple misaligned elements do not produce duplicate
                // diagnostics that all share the whole creation expression's span
                context.ReportDiagnostic(CreateDiagnostic(expression.GetLocation()));

                continue;
            }

            if (expression is InitializerExpressionSyntax complexElement
                && complexElement.IsKind(SyntaxKind.ComplexElementInitializerExpression))
            {
                var misalignment = InitializerLayoutAnalysisUtilities.FindFirstComplexElementMisalignment(complexElement, anchorPosition.Character + SyntaxIndentationUtilities.IndentSize);

                if (misalignment != null)
                {
                    context.ReportDiagnostic(CreateDiagnostic(misalignment));
                }
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnCollectionInitializerExpression, SyntaxKind.CollectionInitializerExpression);
    }

    #endregion // DiagnosticAnalyzer
}