using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0323: Local declarations should be followed by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<ExpressionStatementSyntax, RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0323";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0323LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0323Title), nameof(AnalyzerResources.RH0323MessageFormat), SyntaxKind.ExpressionStatement)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the previous statement in the same statement list
    /// </summary>
    /// <param name="statement">Current statement</param>
    /// <returns>The previous statement, if available</returns>
    private static StatementSyntax? GetPreviousStatement(ExpressionStatementSyntax statement)
    {
        return statement.Parent switch
               {
                   BlockSyntax block => GetPreviousStatement(block.Statements, statement),
                   SwitchSectionSyntax switchSection => GetPreviousStatement(switchSection.Statements, statement),
                   _ => null
               };
    }

    /// <summary>
    /// Gets the previous statement from the provided statement list
    /// </summary>
    /// <param name="statements">Statement list</param>
    /// <param name="currentStatement">Current statement</param>
    /// <returns>The previous statement, if available</returns>
    private static StatementSyntax? GetPreviousStatement(SyntaxList<StatementSyntax> statements, StatementSyntax currentStatement)
    {
        var statementIndex = statements.IndexOf(currentStatement);

        return statementIndex > 0 ? statements[statementIndex - 1] : null;
    }

    #endregion // Methods

    #region StatementShouldBePrecededByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override Location GetLocation(ExpressionStatementSyntax statement)
    {
        return statement.GetFirstToken().GetLocation();
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(ExpressionStatementSyntax statement)
    {
        return statement.GetFirstToken().GetPreviousToken();
    }

    /// <inheritdoc />
    protected override bool IsRelevant(ExpressionStatementSyntax statement)
    {
        return GetPreviousStatement(statement) is LocalDeclarationStatementSyntax
               && statement.Expression is AssignmentExpressionSyntax == false;
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}