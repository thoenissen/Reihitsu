using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Analyzer.Extensions;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0334: Keywords must be spaced correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0334KeywordsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0334KeywordsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0334";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0334KeywordsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0334Title), nameof(AnalyzerResources.RH0334MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var token in root.DescendantTokens())
        {
            if (token.IsAnyKindOf(SyntaxKind.IfKeyword,
                                  SyntaxKind.WhileKeyword,
                                  SyntaxKind.ForKeyword,
                                  SyntaxKind.ForEachKeyword,
                                  SyntaxKind.SwitchKeyword,
                                  SyntaxKind.CatchKeyword,
                                  SyntaxKind.FixedKeyword,
                                  SyntaxKind.LockKeyword,
                                  SyntaxKind.UsingKeyword,
                                  SyntaxKind.ReturnKeyword,
                                  SyntaxKind.ThrowKeyword) == false)
            {
                continue;
            }

            var nextToken = token.GetNextToken();

            if (nextToken.IsKind(SyntaxKind.OpenParenToken) == false
                || token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
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