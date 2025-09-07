using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0317: The checked-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<CheckedStatementSyntax, RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0317";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0317CheckedStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0317Title), nameof(AnalyzerResources.RH0317MessageFormat), SyntaxKind.CheckedStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(CheckedStatementSyntax checkedStatement)
    {
        return checkedStatement.Keyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(CheckedStatementSyntax statement)
    {
        return statement.Keyword.GetLocation();
    }
}