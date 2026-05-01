using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0391: Assignments must have proper line breaks
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0391AssignmentsMustHaveProperLineBreaksAnalyzer : DiagnosticAnalyzerBase<RH0391AssignmentsMustHaveProperLineBreaksAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0391";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0391AssignmentsMustHaveProperLineBreaksAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0391Title), nameof(AnalyzerResources.RH0391MessageFormat))
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

        CheckAssignment(context, assignment.Left, assignment.OperatorToken, assignment.Right, assignment.GetLocation());
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

                CheckAssignment(context, identifier, variable.Initializer.EqualsToken, variable.Initializer.Value, variable.GetLocation());
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

            CheckAssignment(context, identifier, property.Initializer.EqualsToken, property.Initializer.Value, property.GetLocation());
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
    private void CheckAssignment(SyntaxNodeAnalysisContext context, SyntaxNodeOrToken target, SyntaxToken equalsToken, ExpressionSyntax value, Location location)
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