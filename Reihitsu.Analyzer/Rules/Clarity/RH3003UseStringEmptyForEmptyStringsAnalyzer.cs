using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3003: Use string.Empty for empty strings
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3003UseStringEmptyForEmptyStringsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3003";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3003UseStringEmptyForEmptyStringsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3003Title), nameof(AnalyzerResources.RH3003MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the context should be skipped
    /// </summary>
    /// <param name="literalExpression">Literal expression</param>
    /// <returns><see langword="true"/> if the literal should be skipped</returns>
    private static bool ShouldSkip(LiteralExpressionSyntax literalExpression)
    {
        if (literalExpression.Ancestors().Any(ancestor => ancestor is AttributeArgumentSyntax
                                                                   or ConstantPatternSyntax
                                                                   or CaseSwitchLabelSyntax
                                                                   or CasePatternSwitchLabelSyntax
                                                                   or GotoStatementSyntax))
        {
            return true;
        }

        if (literalExpression.Ancestors().Any(ancestor => (ancestor is FieldDeclarationSyntax fieldDeclaration && fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                                                          || (ancestor is LocalDeclarationStatementSyntax localDeclaration && localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))))
        {
            return true;
        }

        if (literalExpression.Ancestors().Any(ancestor => ancestor is EqualsValueClauseSyntax { Parent: ParameterSyntax }))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Analyze all string literal expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnStringLiteralExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LiteralExpressionSyntax literalExpression
            || literalExpression.Token.ValueText != string.Empty
            || ShouldSkip(literalExpression))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(literalExpression.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnStringLiteralExpression, SyntaxKind.StringLiteralExpression);
    }

    #endregion // DiagnosticAnalyzer
}