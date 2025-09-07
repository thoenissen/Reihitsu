using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0316: The switch-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<SwitchStatementSyntax, RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0316";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0316SwitchStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0316Title), nameof(AnalyzerResources.RH0316MessageFormat), SyntaxKind.SwitchStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(SwitchStatementSyntax switchStatement)
    {
        return switchStatement.SwitchKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(SwitchStatementSyntax statement)
    {
        return statement.SwitchKeyword.GetLocation();
    }
}