using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5111: Assignments must have proper line breaks
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5111AssignmentsMustHaveProperLineBreaksAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5111";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5111AssignmentsMustHaveProperLineBreaksAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5111Title), nameof(AnalyzerResources.RH5111MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.SimpleAssignmentExpression"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSimpleAssignmentExpression(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (assignment.Parent is EqualsValueClauseSyntax)
        {
            return;
        }

        // The formatter's LineBreakAssignmentRewriter only collapses collection initializers onto the
        // operator line; every other initializer-expression right-hand side (object/array/complex element
        // initializers used as object-initializer member values) is left on the next line. Exempt those
        // shapes from the value-placement check so formatted code stays diagnostic-free.
        var skipValuePlacementCheck = assignment.Right is InitializerExpressionSyntax initializer
                                      && initializer.IsKind(SyntaxKind.CollectionInitializerExpression) == false;

        CheckAssignment(context, assignment.Left, assignment.OperatorToken, assignment.Right, assignment.GetLocation(), skipValuePlacementCheck);
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.VariableDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (VariableDeclarationSyntax)context.Node;

        foreach (var variable in declaration.Variables)
        {
            if (variable.Initializer != null)
            {
                var identifier = variable.Identifier;

                CheckAssignment(context, identifier, variable.Initializer.EqualsToken, variable.Initializer.Value, variable.GetLocation(), skipValuePlacementCheck: false);
            }
        }
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.PropertyDeclaration"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnPropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        if (property.Initializer != null)
        {
            var identifier = property.Identifier;

            CheckAssignment(context, identifier, property.Initializer.EqualsToken, property.Initializer.Value, property.GetLocation(), skipValuePlacementCheck: false);
        }
    }

    /// <summary>
    /// Checks if an assignment has proper line breaks
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="target">The assignment target (left-hand side) - can be a token or node</param>
    /// <param name="equalsToken">The equals token</param>
    /// <param name="value">The value expression (right-hand side)</param>
    /// <param name="location">Location for diagnostic reporting</param>
    /// <param name="skipValuePlacementCheck">Whether the value-placement rule should be skipped because the formatter never collapses the value onto the operator line</param>
    private void CheckAssignment(SyntaxNodeAnalysisContext context, SyntaxNodeOrToken target, SyntaxToken equalsToken, ExpressionSyntax value, Location location, bool skipValuePlacementCheck)
    {
        var targetEndLine = target.GetLocation()?.GetLineSpan().EndLinePosition.Line;
        var equalsLine = equalsToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var valueStartLine = value.GetLocation().GetLineSpan().StartLinePosition.Line;

        // Rule 1: The assignment target and equals operator must be on the same line
        if (targetEndLine != equalsLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(location));

            return;
        }

        if (skipValuePlacementCheck)
        {
            return;
        }

        // Rule 2: The equals operator and the start of the value must be on the same line
        if (equalsLine != valueStartLine)
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

        context.RegisterSyntaxNodeAction(OnSimpleAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(OnVariableDeclaration, SyntaxKind.VariableDeclaration);
        context.RegisterSyntaxNodeAction(OnPropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}