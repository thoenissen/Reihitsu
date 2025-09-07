using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0321: The yield-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<YieldStatementSyntax, RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0321";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0321YieldStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0321Title), nameof(AnalyzerResources.RH0321MessageFormat), SyntaxKind.YieldReturnStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(YieldStatementSyntax yieldStatement)
    {
        return yieldStatement.YieldKeyword.GetPreviousToken();
    }

    /// <inheritdoc />
    protected override Location GetLocation(YieldStatementSyntax statement)
    {
        return statement.YieldKeyword.GetLocation();
    }
}