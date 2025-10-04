using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0305: The while-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<WhileStatementSyntax, RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0305";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0305WhileStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0305Title), nameof(AnalyzerResources.RH0305MessageFormat), SyntaxKind.WhileStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(WhileStatementSyntax whileStatement)
    {
        return whileStatement.WhileKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(WhileStatementSyntax statement)
    {
        return statement.WhileKeyword.GetLocation();
    }
}