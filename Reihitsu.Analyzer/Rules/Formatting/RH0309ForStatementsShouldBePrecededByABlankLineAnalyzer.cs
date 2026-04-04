using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0309: The for-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<ForStatementSyntax, RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0309";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0309ForStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0309Title), nameof(AnalyzerResources.RH0309MessageFormat), SyntaxKind.ForStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(ForStatementSyntax forStatement)
    {
        return forStatement.ForKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(ForStatementSyntax statement)
    {
        return statement.ForKeyword.GetLocation();
    }
}