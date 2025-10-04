using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0319: The fixed-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<FixedStatementSyntax, RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0319";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0319FixedStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0319Title), nameof(AnalyzerResources.RH0319MessageFormat), SyntaxKind.FixedStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(FixedStatementSyntax fixedStatement)
    {
        return fixedStatement.FixedKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(FixedStatementSyntax statement)
    {
        return statement.FixedKeyword.GetLocation();
    }
}