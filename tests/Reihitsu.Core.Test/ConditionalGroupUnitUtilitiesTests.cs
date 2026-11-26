using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="ConditionalGroupUnitUtilities"/>
/// </summary>
[TestClass]
public class ConditionalGroupUnitUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that the conditional directives of a span are its <c>#if</c>, <c>#elif</c>, <c>#else</c>, and <c>#endif</c>
    /// directives, in the order of the text, and no directive of another kind
    /// </summary>
    [TestMethod]
    public void GetConditionalDirectivesReturnsExactlyTheConditionalDirectives()
    {
        const string source = """
                              #define A
                              class C
                              {
                              #region R
                              #if A
                              #pragma warning disable CS0168
                                  int _a;
                              #nullable enable
                              #elif B
                                  int _b;
                              #else
                                  int _c;
                              #endif
                              #endregion
                              }
                              """;

        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source);

        var kinds = ConditionalGroupUnitUtilities.GetConditionalDirectives(root, root.FullSpan)
                                                 .Select(directive => directive.Kind())
                                                 .ToArray();

        Assert.AreSequenceEqual([SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElifDirectiveTrivia, SyntaxKind.ElseDirectiveTrivia, SyntaxKind.EndIfDirectiveTrivia], kinds);
    }

    #endregion // Tests
}