using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0306: The do-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<DoStatementSyntax, RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0306";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0306DoStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0306Title), nameof(AnalyzerResources.RH0306MessageFormat), SyntaxKind.DoStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(DoStatementSyntax doStatement)
    {
        return doStatement.DoKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(DoStatementSyntax statement)
    {
        return statement.DoKeyword.GetLocation();
    }
}