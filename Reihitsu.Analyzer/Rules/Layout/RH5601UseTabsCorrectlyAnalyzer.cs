using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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

                if (SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, index))
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