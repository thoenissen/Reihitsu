using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5301: The object initializer should be formatted correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5301";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5301Title), nameof(AnalyzerResources.RH5301MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ObjectInitializerExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnObjectInitializerExpression(SyntaxNodeAnalysisContext context)
    {
        var objectInitializer = (InitializerExpressionSyntax)context.Node;

        switch (objectInitializer.Parent)
        {
            case ObjectCreationExpressionSyntax { Initializer: not null } objectCreationExpression:
                AnalyzeObjectInitializer(context, objectCreationExpression, objectCreationExpression.NewKeyword, objectCreationExpression.Initializer);
                break;

            case ImplicitObjectCreationExpressionSyntax { Initializer: not null } implicitObjectCreationExpression:
                AnalyzeObjectInitializer(context, implicitObjectCreationExpression, implicitObjectCreationExpression.NewKeyword, implicitObjectCreationExpression.Initializer);
                break;
        }
    }

    /// <summary>
    /// Analyzing the braces and assignments of an object initializer
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="newKeyword">The <c>new</c> keyword</param>
    /// <param name="objectInitializer">Object initializer</param>
    private void AnalyzeObjectInitializer(SyntaxNodeAnalysisContext context, SyntaxNode diagnosticNode, SyntaxToken newKeyword, InitializerExpressionSyntax objectInitializer)
    {
        var newKeywordPosition = newKeyword.GetLocation().GetLineSpan().StartLinePosition;

        if (CheckBraces(context, newKeywordPosition, diagnosticNode, objectInitializer))
        {
            CheckAssignments(context, newKeywordPosition, diagnosticNode, objectInitializer);
        }
    }

    /// <summary>
    /// Checking the opening and closing braces
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="objectInitializer">Object initializer</param>
    /// <returns>Are all braces valid?</returns>
    private bool CheckBraces(SyntaxNodeAnalysisContext context, LinePosition newKeywordPosition, SyntaxNode diagnosticNode, InitializerExpressionSyntax objectInitializer)
    {
        var isValid = true;

        var openBracePosition = objectInitializer.OpenBraceToken
                                                 .GetLocation()
                                                 .GetLineSpan()
                                                 .StartLinePosition;

        if (openBracePosition.Character != newKeywordPosition.Character)
        {
            context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

            isValid = false;
        }
        else
        {
            var closeBracePosition = objectInitializer.CloseBraceToken
                                                      .GetLocation()
                                                      .GetLineSpan()
                                                      .StartLinePosition;

            if (closeBracePosition.Character != newKeywordPosition.Character)
            {
                context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

                isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Checking assignments
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="objectInitializer">Object initializer</param>
    private void CheckAssignments(SyntaxNodeAnalysisContext context, LinePosition newKeywordPosition, SyntaxNode diagnosticNode, InitializerExpressionSyntax objectInitializer)
    {
        foreach (var memberInitializer in objectInitializer.Expressions.OfType<AssignmentExpressionSyntax>())
        {
            var memberNamePosition = memberInitializer.Left
                                                      .GetLocation()
                                                      .GetLineSpan()
                                                      .StartLinePosition;

            if (memberNamePosition.Character != newKeywordPosition.Character + 4)
            {
                context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnObjectInitializerExpression, SyntaxKind.ObjectInitializerExpression);
    }

    #endregion // DiagnosticAnalyzer
}