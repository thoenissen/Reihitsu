using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5029: Local declarations should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<LocalDeclarationStatementSyntax, RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5029";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5029Title), nameof(AnalyzerResources.RH5029MessageFormat), SyntaxKind.LocalDeclarationStatement)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the previous statement in the same statement list
    /// </summary>
    /// <param name="statement">Current statement</param>
    /// <returns>The previous statement, if available</returns>
    private static StatementSyntax GetPreviousStatement(LocalDeclarationStatementSyntax statement)
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
    protected override Location GetLocation(LocalDeclarationStatementSyntax statement)
    {
        return statement.GetFirstToken().GetLocation();
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(LocalDeclarationStatementSyntax statement)
    {
        return statement.GetFirstToken().GetPreviousToken();
    }

    /// <inheritdoc />
    protected override bool IsRelevant(LocalDeclarationStatementSyntax statement)
    {
        return GetPreviousStatement(statement) is not null and not LocalDeclarationStatementSyntax;
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}