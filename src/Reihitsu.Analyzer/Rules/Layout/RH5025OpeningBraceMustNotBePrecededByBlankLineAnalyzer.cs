using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5025: Opening brace must not be preceded by blank line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5025";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5025Title), nameof(AnalyzerResources.RH5025MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether an opening brace opens a block statement that follows another statement of its block or switch
    /// section. The brace starts a statement, so the blank line above it is not reported (RH5025-005)
    /// </summary>
    /// <param name="token">The opening brace</param>
    /// <returns><see langword="true"/> if the brace opens such a block statement</returns>
    private static bool IsOpeningBraceOfBlockStatementAfterStatement(SyntaxToken token)
    {
        return token.Parent is BlockSyntax block
               && block.OpenBraceToken == token
               && BlankLineSpacingPolicy.IsBlockStatementAfterStatement(block);
    }

    /// <summary>
    /// Determines whether an opening brace opens an interpolation hole. The line above it belongs to the literal's text, so
    /// it is not reported (RH5025-006)
    /// </summary>
    /// <param name="token">The opening brace</param>
    /// <returns><see langword="true"/> if the brace opens an interpolation hole</returns>
    private static bool IsOpeningBraceOfInterpolation(SyntaxToken token)
    {
        return token.Parent is InterpolationSyntax interpolation
               && interpolation.OpenBraceToken == token;
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var openBraceLineIndices = FormattingTextAnalysisUtilities.GetLineIndicesBeginningWithToken(root,
                                                                                                    sourceText,
                                                                                                    static token => token.IsKind(SyntaxKind.OpenBraceToken)
                                                                                                                    && IsOpeningBraceOfInterpolation(token) == false
                                                                                                                    && IsOpeningBraceOfBlockStatementAfterStatement(token) == false);

        foreach (var blankLine in FormattingTextAnalysisUtilities.EnumeratePrecedingBlankLines(sourceText, openBraceLineIndices))
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(blankLine.Start, blankLine.EndIncludingLineBreak))));
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