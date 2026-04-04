using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0315: The throw-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<ThrowStatementSyntax, RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0315";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0315ThrowStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0315Title), nameof(AnalyzerResources.RH0315MessageFormat), SyntaxKind.ThrowStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(ThrowStatementSyntax throwStatement)
    {
        return throwStatement.ThrowKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(ThrowStatementSyntax statement)
    {
        return statement.ThrowKeyword.GetLocation();
    }
}