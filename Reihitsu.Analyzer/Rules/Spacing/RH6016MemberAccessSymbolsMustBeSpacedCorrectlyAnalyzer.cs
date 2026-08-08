using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6016: Member access symbols must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6016";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6016Title), nameof(AnalyzerResources.RH6016MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the token is a member-access symbol whose spacing this rule owns: the dot of a
    /// plain, bound or qualified access, the question mark that opens a conditional access, or the arrow
    /// of a pointer access. The parent check on the question mark is what separates the conditional access
    /// from a conditional expression and from a nullable type, which keep their spacing
    /// </summary>
    /// <param name="token">Token to inspect</param>
    /// <returns><see langword="true"/> if the token is a member-access symbol</returns>
    private static bool IsMemberAccessSymbol(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.DotToken)
               || (token.IsKind(SyntaxKind.QuestionToken) && token.Parent is ConditionalAccessExpressionSyntax)
               || (token.IsKind(SyntaxKind.MinusGreaterThanToken) && token.Parent is MemberAccessExpressionSyntax);
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var token in root.DescendantTokens().Where(IsMemberAccessSymbol))
        {
            var (hasLeadingSpace, hasTrailingSpace) = AdjacentTokenSpacingUtilities.DetermineSameLineAdjacentSpacing(token, sourceText);

            if (hasLeadingSpace || hasTrailingSpace)
            {
                context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
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