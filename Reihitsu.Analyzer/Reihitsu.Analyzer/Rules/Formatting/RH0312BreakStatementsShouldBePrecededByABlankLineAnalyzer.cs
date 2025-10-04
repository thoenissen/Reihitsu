using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0312: The break-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<BreakStatementSyntax, RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0312";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0312BreakStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0312Title), nameof(AnalyzerResources.RH0312MessageFormat), SyntaxKind.BreakStatement)
    {
    }

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
        return statement.BreakKeyword.GetPreviousToken().IsKind(SyntaxKind.CloseBraceToken) == false;
    }
}