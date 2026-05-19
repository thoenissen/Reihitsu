using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0398: Wrapped fluent calls should keep the first call on the original line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer : DiagnosticAnalyzerBase<RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0398";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0398WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0398Title), nameof(AnalyzerResources.RH0398MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the given node is an inner member of a larger chain
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns><c>true</c> if the node is part of a larger chain; otherwise <c>false</c></returns>
    private static bool IsInnerChainMember(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            switch (current)
            {
                case InvocationExpressionSyntax:
                case ElementAccessExpressionSyntax:
                case PostfixUnaryExpressionSyntax:
                    {
                        current = current.Parent;

                        continue;
                    }
                case MemberAccessExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                    {
                        return true;
                    }
            }

            break;
        }

        return false;
    }

    /// <summary>
    /// Collects all chain link tokens from the outermost node down to the root expression
    /// </summary>
    /// <param name="node">The outermost node of the chain</param>
    /// <returns>List of alignment tokens in chain order (first link closest to root)</returns>
    private static List<SyntaxToken> CollectChainLinks(SyntaxNode node)
    {
        var links = new List<SyntaxToken>();
        var current = node;
        var isInvoked = node.Parent is InvocationExpressionSyntax;

        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                current = invocation.Expression;
                isInvoked = true;
            }
            else if (current is MemberAccessExpressionSyntax memberAccess)
            {
                current = ProcessMemberAccess(memberAccess, isInvoked, links);
                isInvoked = false;
            }
            else if (current is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                links.Add(conditionalAccess.OperatorToken);
                current = conditionalAccess.Expression;
                isInvoked = false;
            }
            else if (current is ElementAccessExpressionSyntax elementAccess)
            {
                current = elementAccess.Expression;
            }
            else if (current is PostfixUnaryExpressionSyntax postfix)
            {
                current = postfix.Operand;
            }
            else
            {
                break;
            }
        }

        links.Reverse();

        return links;
    }

    /// <summary>
    /// Processes a member access expression within a chain, adding the appropriate token if invoked
    /// </summary>
    /// <param name="memberAccess">The member access expression</param>
    /// <param name="isInvoked">Whether the member access is invoked</param>
    /// <param name="links">The list of chain link tokens to add to</param>
    /// <returns>The next node to process in the chain</returns>
    private static ExpressionSyntax ProcessMemberAccess(MemberAccessExpressionSyntax memberAccess, bool isInvoked, List<SyntaxToken> links)
    {
        if (isInvoked == false)
        {
            return memberAccess.Expression;
        }

        if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
        {
            links.Add(postfixUnary.OperatorToken);

            return postfixUnary.Operand;
        }

        links.Add(memberAccess.OperatorToken);

        return memberAccess.Expression;
    }

    /// <summary>
    /// Gets the line number of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Line number</returns>
    private static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Determines whether the first invoked chain link is preceded by intermediate member access
    /// </summary>
    /// <param name="token">The chain link token</param>
    /// <returns><c>true</c> if intermediate member access precedes the invocation; otherwise <c>false</c></returns>
    private static bool HasIntermediateMemberAccess(SyntaxToken token)
    {
        return token.Parent switch
               {
                   MemberAccessExpressionSyntax memberAccess => memberAccess.Expression is MemberAccessExpressionSyntax
                                                                                        or ConditionalAccessExpressionSyntax,
                   ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression is MemberAccessExpressionSyntax
                                                                                                       or ConditionalAccessExpressionSyntax,
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether the token has a comment directly above its line
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><c>true</c> if a comment is directly above the token; otherwise <c>false</c></returns>
    private static bool HasCommentDirectlyAbove(SyntaxToken token)
    {
        if (token.LeadingTrivia.Any(IsCommentTrivia) == false)
        {
            return false;
        }

        if (token.SyntaxTree == null)
        {
            return true;
        }

        var line = GetLine(token);

        if (line <= 0)
        {
            return false;
        }

        var previousLine = token.SyntaxTree.GetText().Lines[line - 1].ToString().Trim();

        return previousLine.StartsWith("//", StringComparison.Ordinal)
               || previousLine.StartsWith("/*", StringComparison.Ordinal)
               || previousLine.StartsWith("*", StringComparison.Ordinal)
               || previousLine.EndsWith("*/", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether a trivia is a comment
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><c>true</c> if the trivia is a comment; otherwise <c>false</c></returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Analyzing member access expressions for correct first-call placement in wrapped fluent calls
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (IsInnerChainMember(memberAccess))
        {
            return;
        }

        AnalyzeChain(context, memberAccess);
    }

    /// <summary>
    /// Analyzing conditional access expressions for correct first-call placement in wrapped fluent calls
    /// </summary>
    /// <param name="context">Context</param>
    private void OnConditionalAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConditionalAccessExpressionSyntax conditionalAccess)
        {
            return;
        }

        if (IsInnerChainMember(conditionalAccess))
        {
            return;
        }

        AnalyzeChain(context, conditionalAccess);
    }

    /// <summary>
    /// Analyzes a chain starting from the outermost node
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="outermostNode">The outermost node of the chain</param>
    private void AnalyzeChain(SyntaxNodeAnalysisContext context, SyntaxNode outermostNode)
    {
        var chainLinks = CollectChainLinks(outermostNode);

        if (chainLinks.Count == 0)
        {
            return;
        }

        var firstLink = chainLinks[0];
        var previousToken = firstLink.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None))
        {
            return;
        }

        if (GetLine(firstLink) == GetLine(previousToken))
        {
            return;
        }

        if (HasIntermediateMemberAccess(firstLink))
        {
            return;
        }

        if (HasCommentDirectlyAbove(firstLink))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(firstLink.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(OnConditionalAccessExpression, SyntaxKind.ConditionalAccessExpression);
    }

    #endregion // DiagnosticAnalyzer
}