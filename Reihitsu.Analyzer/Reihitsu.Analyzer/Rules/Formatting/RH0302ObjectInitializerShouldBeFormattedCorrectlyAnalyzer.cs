using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0302: The object initializer should be formatted correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0302";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0302ObjectInitializerShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0302Title), nameof(AnalyzerResources.RH0302MessageFormat))
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

        var objectCreationExpression = objectInitializer.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

        if (objectCreationExpression?.Initializer != null)
        {
            var newKeywordPosition = objectCreationExpression.NewKeyword.GetLocation().GetLineSpan().StartLinePosition;

            if (CheckBraces(context, newKeywordPosition, objectCreationExpression))
            {
                CheckAssignments(context, newKeywordPosition, objectCreationExpression);
            }
        }
    }

    /// <summary>
    /// Checking the opening and closing braces
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="newKeywordPosition">New keyword position</param>
    /// <param name="objectCreationExpression">Object</param>
    /// <returns>Are all braces valid?</returns>
    private bool CheckBraces(SyntaxNodeAnalysisContext context, LinePosition newKeywordPosition, ObjectCreationExpressionSyntax objectCreationExpression)
    {
        var isValid = true;

        var openBracePosition = objectCreationExpression.Initializer
                                                        !.OpenBraceToken
                                                        .GetLocation()
                                                        .GetLineSpan()
                                                        .StartLinePosition;

        if (openBracePosition.Character != newKeywordPosition.Character)
        {
            context.ReportDiagnostic(CreateDiagnostic(objectCreationExpression.GetLocation()));

            isValid = false;
        }
        else
        {
            var closeBracePosition = objectCreationExpression.Initializer
                                                             .CloseBraceToken
                                                             .GetLocation()
                                                             .GetLineSpan()
                                                             .StartLinePosition;

            if (closeBracePosition.Character != newKeywordPosition.Character)
            {
                context.ReportDiagnostic(CreateDiagnostic(objectCreationExpression.GetLocation()));

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
    /// <param name="objectCreationExpression">Object</param>
    private void CheckAssignments(SyntaxNodeAnalysisContext context, LinePosition newKeywordPosition, ObjectCreationExpressionSyntax objectCreationExpression)
    {
        foreach (var memberInitializer in objectCreationExpression.Initializer!.Expressions.OfType<AssignmentExpressionSyntax>())
        {
            var memberNamePosition = memberInitializer.Left
                                                      .GetLocation()
                                                      .GetLineSpan()
                                                      .StartLinePosition;

            if (memberNamePosition.Character != newKeywordPosition.Character + 4)
            {
                context.ReportDiagnostic(CreateDiagnostic(objectCreationExpression.GetLocation()));
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