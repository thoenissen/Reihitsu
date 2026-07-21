using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5601: Use tabs correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5601UseTabsCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5601";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5601UseTabsCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5601Title), nameof(AnalyzerResources.RH5601MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token holds string or character content whose tabs are semantic and must not be altered
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token holds string or character content; otherwise, <see langword="false"/></returns>
    public static bool IsStringContentToken(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.StringLiteralToken)
               || token.IsKind(SyntaxKind.CharacterLiteralToken)
               || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
               || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
               || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
               || token.IsKind(SyntaxKind.Utf8StringLiteralToken)
               || token.IsKind(SyntaxKind.Utf8SingleLineRawStringLiteralToken)
               || token.IsKind(SyntaxKind.Utf8MultiLineRawStringLiteralToken);
    }

    /// <summary>
    /// Determines whether the specified position falls inside a comment or preprocessor-disabled text
    /// interior. The formatter never rewrites that content, and it may be semantically meaningful (for
    /// example, aligned example output or deliberately preserved inactive code), so tabs there are exempt
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="position">Position to inspect</param>
    /// <returns><see langword="true"/> if the position is inside a comment or disabled-text interior; otherwise, <see langword="false"/></returns>
    public static bool IsInsideCommentOrDisabledText(SyntaxNode root, int position)
    {
        var trivia = root.FindTrivia(position);

        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.DisabledTextTrivia);
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var line in sourceText.Lines)
        {
            for (var index = line.Start; index < line.End; index++)
            {
                if (sourceText[index] != '\t')
                {
                    continue;
                }

                var token = root.FindToken(index);

                if (IsStringContentToken(token))
                {
                    continue;
                }

                if (IsInsideCommentOrDisabledText(root, index))
                {
                    continue;
                }

                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, new TextSpan(index, 1))));
            }
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