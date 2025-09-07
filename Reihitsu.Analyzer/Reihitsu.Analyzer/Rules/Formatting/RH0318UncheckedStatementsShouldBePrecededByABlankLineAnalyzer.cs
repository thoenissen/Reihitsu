using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0318: The unchecked-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<CheckedStatementSyntax, RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0318";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0318UncheckedStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0318Title), nameof(AnalyzerResources.RH0318MessageFormat), SyntaxKind.UncheckedStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(CheckedStatementSyntax uncheckedStatement)
    {
        return uncheckedStatement.Keyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(CheckedStatementSyntax statement)
    {
        return statement.Keyword.GetLocation();
    }
}