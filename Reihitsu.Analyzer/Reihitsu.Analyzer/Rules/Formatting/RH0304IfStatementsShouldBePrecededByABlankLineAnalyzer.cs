using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0304: The if-Statement should be preceded by a blank line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer : StatementShouldBePrecededByABlankLineAnalyzerBase<IfStatementSyntax, RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer>
{
    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0304";

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0304IfStatementsShouldBePrecededByABlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH0304Title), nameof(AnalyzerResources.RH0304MessageFormat), SyntaxKind.IfStatement)
    {
    }

    /// <inheritdoc />
    protected override SyntaxToken GetPreviousToken(IfStatementSyntax ifStatement)
    {
        var previousToken = ifStatement.IfKeyword.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.ElseKeyword))
        {
            return default;
        }

        return previousToken;
    }

    /// <inheritdoc />
    protected override Location GetLocation(IfStatementSyntax statement)
    {
        return statement.IfKeyword.GetLocation();
    }
}