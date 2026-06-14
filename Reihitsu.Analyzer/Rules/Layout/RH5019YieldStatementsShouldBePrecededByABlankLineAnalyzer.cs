using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5019: The yield-Statement should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<YieldStatementSyntax, RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5019";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5019Title), nameof(AnalyzerResources.RH5019MessageFormat), SyntaxKind.YieldReturnStatement, SyntaxKind.YieldBreakStatement)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the previous statement in the same statement list
    /// </summary>
    /// <param name="statement">Current statement</param>
    /// <returns>The previous statement, if available</returns>
    private static StatementSyntax GetPreviousStatement(YieldStatementSyntax statement)
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
    private static StatementSyntax GetPreviousStatement(SyntaxList<StatementSyntax> statements, StatementSyntax currentStatement)
    {
        var statementIndex = statements.IndexOf(currentStatement);

        return statementIndex > 0 ? statements[statementIndex - 1] : null;
    }

    #endregion // Methods

    #region StatementShouldBePrecededByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override bool IsRelevant(YieldStatementSyntax statement)
    {
        return GetPreviousStatement(statement) is not YieldStatementSyntax;
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(YieldStatementSyntax yieldStatement)
    {
        return yieldStatement.YieldKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(YieldStatementSyntax statement)
    {
        return statement.YieldKeyword.GetLocation();
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}