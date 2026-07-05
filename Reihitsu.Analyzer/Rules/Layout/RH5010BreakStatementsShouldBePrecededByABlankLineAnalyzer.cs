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
        return statement.BreakKeyword.GetPreviousToken().IsKind(SyntaxKind.CloseBraceToken) == false
               && statement.Parent?.IsKind(SyntaxKind.SwitchSection) != true;
    }

    #endregion // StatementShouldBePrecededByABlankLineAnalyzerBase
}