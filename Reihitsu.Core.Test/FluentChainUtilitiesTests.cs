using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="FluentChainUtilities"/>
/// </summary>
[TestClass]
public class FluentChainUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a null-forgiving operator represents an invoked link when its member-access dot shares the line
    /// </summary>
    [TestMethod]
    public void GetInvokedLinkOperatorReturnsNullForgivingOperatorForSameLineAccess()
    {
        var memberAccess = GetOutermostInvokedMemberAccess("value?.B()!.C()");

        var operatorToken = FluentChainUtilities.GetInvokedLinkOperator(memberAccess);

        Assert.AreEqual(SyntaxKind.ExclamationToken, operatorToken.Kind());
    }

    /// <summary>
    /// Verifies that a null-forgiving operator still represents the invoked link when its member-access dot is wrapped
    /// </summary>
    [TestMethod]
    public void GetInvokedLinkOperatorReturnsNullForgivingOperatorForWrappedAccess()
    {
        var memberAccess = GetOutermostInvokedMemberAccess("value?.B()!\n.C()");

        var operatorToken = FluentChainUtilities.GetInvokedLinkOperator(memberAccess);

        Assert.AreEqual(SyntaxKind.ExclamationToken, operatorToken.Kind());
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses an invocation and returns its outermost member access
    /// </summary>
    /// <param name="source">The expression source</param>
    /// <returns>The outermost invoked member access</returns>
    private static MemberAccessExpressionSyntax GetOutermostInvokedMemberAccess(string source)
    {
        return SyntaxFactory.ParseExpression(source)
                            .DescendantNodesAndSelf()
                            .OfType<MemberAccessExpressionSyntax>()
                            .Single(memberAccess => memberAccess.Expression is PostfixUnaryExpressionSyntax);
    }

    #endregion // Methods
}