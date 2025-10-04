using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0310: The return-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<ReturnStatementSyntax, RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0310";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0310ReturnStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0310Title), nameof(AnalyzerResources.RH0310MessageFormat), SyntaxKind.ReturnStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(ReturnStatementSyntax returnStatement)
    {
        return returnStatement.ReturnKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(ReturnStatementSyntax statement)
    {
        return statement.ReturnKeyword.GetLocation();
    }
}