using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5011: The break-Statement should be followed by a blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer : StatementShouldBeFollowedByABlankLineAnalyzerBase<BreakStatementSyntax>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5011";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5011Title), nameof(AnalyzerResources.RH5011MessageFormat), SyntaxKind.BreakStatement)
    {
    }

    #endregion // Constructor

    #region StatementShouldBeFollowedByABlankLineAnalyzerBase

    /// <inheritdoc />
    protected override SyntaxToken GetNextToken(BreakStatementSyntax breakStatement)
    {
        var nextToken = breakStatement.BreakKeyword.GetNextToken();

        if (nextToken.IsKind(SyntaxKind.SemicolonToken))
        {
            nextToken = nextToken.GetNextToken();
        }

        return nextToken;
    }

    /// <inheritdoc />
    protected override Location GetLocation(BreakStatementSyntax statement)
    {
        return statement.BreakKeyword.GetLocation();
    }

    #endregion // StatementShouldBeFollowedByABlankLineAnalyzerBase
}