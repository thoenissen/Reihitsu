using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0307: The using-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<UsingStatementSyntax, RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0307";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0307UsingStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0307Title), nameof(AnalyzerResources.RH0307MessageFormat), SyntaxKind.UsingStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(UsingStatementSyntax usingStatement)
    {
        return usingStatement.AwaitKeyword == default
                   ? usingStatement.UsingKeyword.GetPreviousToken()
                   : usingStatement.AwaitKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(UsingStatementSyntax statement)
    {
        return statement.UsingKeyword.GetLocation();
    }
}