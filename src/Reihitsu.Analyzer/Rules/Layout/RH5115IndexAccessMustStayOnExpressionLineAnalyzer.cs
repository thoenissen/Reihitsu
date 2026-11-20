using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5115: Index access must stay on the line of its expression
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5115IndexAccessMustStayOnExpressionLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5115";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5115IndexAccessMustStayOnExpressionLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a token starts on a later line than the token in front of it ends on. Blank lines, comments,
    /// directives, and disabled text in the gap do not change the answer (RH5115-001, RH5115-002)
    /// </summary>
    /// <param name="previousToken">The token in front of the gap</param>
    /// <param name="token">The token behind the gap</param>
    /// <returns><see langword="true"/> if the token starts on a later line; otherwise, <see langword="false"/></returns>
    private static bool StartsLaterLine(SyntaxToken previousToken, SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line > previousToken.GetLocation().GetLineSpan().EndLinePosition.Line;
    }

    /// <summary>
    /// Gets the first token of the index access a node is, when it is one: the <c>[</c> of an element access, or the
    /// <c>?</c> of a conditional element access, as the attached-part predicate states them
    /// (<see cref="AttachedPartGapUtilities.StartsAttachedPart"/>)
    /// </summary>
    /// <param name="node">An element access or a conditional access</param>
    /// <param name="firstToken">The first token of the index access</param>
    /// <returns><see langword="true"/> if the node is an index access; otherwise, <see langword="false"/></returns>
    private static bool TryGetFirstToken(SyntaxNode node, out SyntaxToken firstToken)
    {
        firstToken = node switch
                     {
                         ElementAccessExpressionSyntax elementAccess => elementAccess.ArgumentList.OpenBracketToken,
                         ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.OperatorToken,
                         _ => default
                     };

        return firstToken.IsMissing == false
               && AttachedPartGapUtilities.StartsAttachedPart(firstToken);
    }

    /// <summary>
    /// Determines whether a line break separates an index access from the token in front of it, or, for a conditional
    /// element access, its <c>?</c> from its <c>[</c> (RH5115-001)
    /// </summary>
    /// <param name="firstToken">The first token of the index access</param>
    /// <returns><see langword="true"/> if a gap of the access holds a line break; otherwise, <see langword="false"/></returns>
    private static bool HasLineBreakInGap(SyntaxToken firstToken)
    {
        if (StartsLaterLine(SyntaxTokenNavigationUtilities.GetPreviousToken(firstToken), firstToken))
        {
            return true;
        }

        var bracketToken = SyntaxTokenNavigationUtilities.GetNextToken(firstToken);

        return AttachedPartGapUtilities.IsElementBindingOpenBracket(bracketToken)
               && StartsLaterLine(firstToken, bracketToken);
    }

    /// <summary>
    /// Analyzes an element access or a conditional access
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="errorVerdicts">Syntax error verdict per tree</param>
    private void OnAccess(SyntaxNodeAnalysisContext context, SyntaxTreeErrorVerdictCache errorVerdicts)
    {
        if (TryGetFirstToken(context.Node, out var firstToken) == false
            || HasLineBreakInGap(firstToken) == false)
        {
            return;
        }

        // The rule withholds itself for a whole file with malformed syntax (SHARED-JOIN-002), so the verdict covers the
        // entire tree rather than the analyzed access.
        if (errorVerdicts.ContainsError(context.Node.SyntaxTree, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(firstToken.GetLocation()));
    }

    /// <summary>
    /// Starts the analysis of a compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var errorVerdicts = new SyntaxTreeErrorVerdictCache();

        context.RegisterSyntaxNodeAction(nodeContext => OnAccess(nodeContext, errorVerdicts), SyntaxKind.ElementAccessExpression, SyntaxKind.ConditionalAccessExpression);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion // DiagnosticAnalyzer
}