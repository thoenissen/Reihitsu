using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5304: Nested collection initializer assignments should be formatted correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5304";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5304Title), nameof(AnalyzerResources.RH5304MessageFormat))
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
                AnalyzeObjectInitializer(context, objectCreationExpression, objectCreationExpression.Initializer);
                break;

            case ImplicitObjectCreationExpressionSyntax { Initializer: not null } implicitObjectCreationExpression:
                AnalyzeObjectInitializer(context, implicitObjectCreationExpression, implicitObjectCreationExpression.Initializer);
                break;
        }
    }

    /// <summary>
    /// Analyzing the nested collection initializer assignments within an object initializer
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="objectInitializer">Object initializer</param>
    private void AnalyzeObjectInitializer(SyntaxNodeAnalysisContext context, SyntaxNode diagnosticNode, InitializerExpressionSyntax objectInitializer)
    {
        foreach (var memberInitializer in objectInitializer.Expressions.OfType<AssignmentExpressionSyntax>())
        {
            // Only analyze assignments where the value is a collection initializer
            if (memberInitializer.Right is not InitializerExpressionSyntax collectionInitializer)
            {
                continue;
            }

            // Verify it's a collection initializer (has braces)
            if (collectionInitializer.Kind() != SyntaxKind.CollectionInitializerExpression)
            {
                continue;
            }

            CheckNestedCollectionInitializerAssignment(context, diagnosticNode, memberInitializer, collectionInitializer);
        }
    }

    /// <summary>
    /// Checks if a nested collection initializer assignment has proper formatting
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="diagnosticNode">Node used for diagnostics</param>
    /// <param name="assignment">The assignment expression</param>
    /// <param name="collectionInitializer">The collection initializer</param>
    private void CheckNestedCollectionInitializerAssignment(SyntaxNodeAnalysisContext context, SyntaxNode diagnosticNode, AssignmentExpressionSyntax assignment, InitializerExpressionSyntax collectionInitializer)
    {
        var assignmentTargetPosition = assignment.Left.GetLocation().GetLineSpan().StartLinePosition;
        var equalsTokenPosition = assignment.OperatorToken.GetLocation().GetLineSpan().StartLinePosition;
        var openBracePosition = collectionInitializer.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition;
        var closeBracePosition = collectionInitializer.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition;

        // Rule 1: Assignment target and equals must be on the same line
        if (assignmentTargetPosition.Line != equalsTokenPosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

            return;
        }

        // Rule 2: The opening brace must be on the same line as the equals sign
        if (equalsTokenPosition.Line != openBracePosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

            return;
        }

        var expressionLinePositions = collectionInitializer.Expressions.Select(expression => expression.GetLocation().GetLineSpan().StartLinePosition).ToArray();
        var isSingleLineCollection = openBracePosition.Line == closeBracePosition.Line;

        // Rule 3: Single-line nested collection initializers are allowed only when every item
        // is also on that same line.
        if (isSingleLineCollection)
        {
            if (expressionLinePositions.Any(position => position.Line != openBracePosition.Line))
            {
                context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

                return;
            }

            return;
        }

        // Rule 4: In multi-line form, opening and closing braces must be in the same column.
        if (openBracePosition.Character != closeBracePosition.Character)
        {
            context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));

            return;
        }

        // Rule 5: In multi-line form, every collection element must be on its own line between
        // the opening and closing braces.
        if (expressionLinePositions.Any(position => position.Line <= openBracePosition.Line || position.Line >= closeBracePosition.Line)
            || expressionLinePositions.Select(position => position.Line).Distinct().Count() != expressionLinePositions.Length)
        {
            context.ReportDiagnostic(CreateDiagnostic(diagnosticNode.GetLocation()));
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