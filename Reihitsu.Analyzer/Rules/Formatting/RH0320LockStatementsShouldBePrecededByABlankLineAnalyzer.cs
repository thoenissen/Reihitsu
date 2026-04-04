using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0320: The lock-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<LockStatementSyntax, RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0320";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0320LockStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0320Title), nameof(AnalyzerResources.RH0320MessageFormat), SyntaxKind.LockStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(LockStatementSyntax lockStatement)
    {
        return lockStatement.LockKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(LockStatementSyntax statement)
    {
        return statement.LockKeyword.GetLocation();
    }
}