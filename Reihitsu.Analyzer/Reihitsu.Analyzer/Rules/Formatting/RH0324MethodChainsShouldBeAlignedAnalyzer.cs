using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0324: Method chains should be aligned.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0324MethodChainsShouldBeAlignedAnalyzer : DiagnosticAnalyzerBase<RH0324MethodChainsShouldBeAlignedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0324";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0324MethodChainsShouldBeAlignedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0324Title), nameof(AnalyzerResources.RH0324MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing member access expressions for correct method chain alignment
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
    /// Analyzing conditional access expressions for correct method chain alignment
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

        if (chainLinks.Count < 2)
        {
            return;
        }

        var firstLine = GetLine(chainLinks[0]);

        if (chainLinks.All(link => GetLine(link) == firstLine))
        {
            return;
        }

        var referenceColumn = GetColumn(chainLinks[0]);

        for (var i = 1; i < chainLinks.Count; i++)
        {
            var linkLine = GetLine(chainLinks[i]);
            var linkColumn = GetColumn(chainLinks[i]);

            if (linkLine == firstLine)
            {
                if (chainLinks.Skip(i + 1).Any(subsequentLink => GetLine(subsequentLink) != firstLine))
                {
                    context.ReportDiagnostic(CreateDiagnostic(chainLinks[i].GetLocation()));
                }
            }
            else
            {
                if (linkColumn != referenceColumn)
                {
                    context.ReportDiagnostic(CreateDiagnostic(chainLinks[i].GetLocation()));
                }
            }
        }
    }

    /// <summary>
    /// Determines whether the given node is an inner member of a larger chain
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns><c>true</c> if the node is part of a larger chain; otherwise <c>false</c></returns>
    private bool IsInnerChainMember(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            switch (current)
            {
                case InvocationExpressionSyntax:
                case ElementAccessExpressionSyntax:
                case PostfixUnaryExpressionSyntax:
                    current = current.Parent;

                    continue;
                case MemberAccessExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                    return true;
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
    private List<SyntaxToken> CollectChainLinks(SyntaxNode node)
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
                if (isInvoked)
                {
                    if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
                    {
                        links.Add(postfixUnary.OperatorToken);
                        current = postfixUnary.Operand;
                    }
                    else
                    {
                        links.Add(memberAccess.OperatorToken);
                        current = memberAccess.Expression;
                    }
                }
                else
                {
                    current = memberAccess.Expression;
                }

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
    /// Gets the line number of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Line number</returns>
    private int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Gets the column of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Column</returns>
    private int GetColumn(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Character;
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
