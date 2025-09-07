using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0313: The break-Statement should be followed by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer : StatementShouldBeFollowedByABlankLineAnalyzerBase<BreakStatementSyntax, RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0313";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0313BreakStatementsShouldBeFollowedByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0313Title), nameof(AnalyzerResources.RH0313MessageFormat), SyntaxKind.BreakStatement)
    {
    }

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
}