using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6002: Commas must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6002CommasMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6002";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6002CommasMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6002Title), nameof(AnalyzerResources.RH6002MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the formatter-owned trivia gaps directly adjacent to a comma
    /// </summary>
    /// <param name="token">Comma token</param>
    /// <returns>The previous token together with the normalized previous and comma tokens</returns>
    internal static (SyntaxToken PreviousToken, SyntaxToken NormalizedPreviousToken, SyntaxToken NormalizedCommaToken) AnalyzeSpacing(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();
        var normalizedPreviousToken = previousToken;

        if (previousToken.RawKind != 0
            && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token) == false)
        {
            normalizedPreviousToken = SyntaxTriviaUtilities.SetTrailingWhitespace(previousToken, 0);
        }

        var nextToken = token.GetNextToken();
        var normalizedCommaToken = token;

        if (nextToken.RawKind != 0
            && SyntaxTriviaUtilities.AreSeparatedByEndOfLine(token, nextToken) == false)
        {
            normalizedCommaToken = SyntaxTriviaUtilities.SetTrailingWhitespace(token, 1);
        }

        return (previousToken, normalizedPreviousToken, normalizedCommaToken);
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var token in root.DescendantTokens().Where(currentToken => currentToken.IsKind(SyntaxKind.CommaToken)))
        {
            if (CommaSpacingUtilities.IsSpacingExempt(token))
            {
                continue;
            }

            var (previousToken, normalizedPreviousToken, normalizedCommaToken) = AnalyzeSpacing(token);

            if (previousToken == normalizedPreviousToken
                && token == normalizedCommaToken)
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}