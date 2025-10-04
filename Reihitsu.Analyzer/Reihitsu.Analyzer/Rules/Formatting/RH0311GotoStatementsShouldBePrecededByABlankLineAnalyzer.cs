using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0311: The goto-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<GotoStatementSyntax, RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0311";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0311GotoStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0311Title), nameof(AnalyzerResources.RH0311MessageFormat), SyntaxKind.GotoStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(GotoStatementSyntax gotoStatement)
    {
        return gotoStatement.GotoKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(GotoStatementSyntax statement)
    {
        return statement.GotoKeyword.GetLocation();
    }
}