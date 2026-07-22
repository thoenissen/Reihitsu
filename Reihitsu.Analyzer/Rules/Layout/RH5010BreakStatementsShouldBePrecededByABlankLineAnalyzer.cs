using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5010: The break-Statement should be preceded by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<BreakStatementSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5010";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5010Title), nameof(AnalyzerResources.RH5010MessageFormat), SyntaxKind.BreakStatement)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the break statement's containing statement list is a switch section, either directly
    /// or through a block that is itself the switch section's braced body. This mirrors the formatter's
    /// <c>BlankLineStatementSpacingRewriter</c>, which never requires a blank line before a break in either
    /// shape, regardless of what precedes it (issue #440)
    /// </summary>
    /// <param name="statement">Break statement</param>
    /// <returns><see langword="true"/> if the break statement is in a switch section</returns>
    private static bool IsInSwitchSection(BreakStatementSyntax statement)
    {
        return statement.Parent is SwitchSectionSyntax
               || (statement.Parent is BlockSyntax block && block.Parent is SwitchSectionSyntax);
    }

    #endregion // Methods

    #region StatementShouldBePrecededByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(BreakStatementSyntax breakStatement)
    {
        return breakStatement.BreakKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(BreakStatementSyntax statement)
    {
        return statement.BreakKeyword.GetLocation();
    }

    /// <inheritdoc />
    protected override bool IsRelevant(BreakStatementSyntax statement)
    {
        return IsInSwitchSection(statement) == false;
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}