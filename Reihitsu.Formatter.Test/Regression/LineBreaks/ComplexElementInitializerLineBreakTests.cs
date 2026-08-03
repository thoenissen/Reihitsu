using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #425: <see cref="LineBreakInitializerRewriter"/> must not explode
/// already-single-line complex element initializers (dictionary-style key/value pairs)
/// one-token-per-line. Pairs whose contents become multi-line are recursively expanded
/// </summary>
[TestClass]
public class ComplexElementInitializerLineBreakTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Executes the <see cref="LineBreakPhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = new LineBreakPhase().Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Asserts that the line-break phase leaves an already-single-line complex element initializer
    /// unchanged
    /// </summary>
    /// <param name="input">The C# source text</param>
    private void AssertUnchanged(string input)
    {
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        Assert.AreEqual(input, actual, "Already-single-line complex element initializer pairs must remain unchanged.");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the issue #425 reproduction: dictionary-style pairs inside a collection initializer
    /// keep their single-line layout instead of being exploded one-token-per-line
    /// </summary>
    [TestMethod]
    public void DictionaryStylePairsStaySingleLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             public class C
                             {
                                 private readonly Dictionary<string, int> _map = new Dictionary<string, int>
                                 {
                                     { "a", 1 },
                                     { "b", 2 },
                                 };
                             }
                             """;

        AssertUnchanged(input);
    }

    /// <summary>
    /// Verifies that a single dictionary-style pair also keeps its single-line layout. The outer
    /// collection initializer has only one element here, so this isolates the recursive rewrite of
    /// the pair itself from the outer one-element-per-line policy, which never triggers when there is
    /// only one element
    /// </summary>
    [TestMethod]
    public void SingleDictionaryStylePairStaysSingleLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             public class C
                             {
                                 private readonly Dictionary<string, int> _map = new Dictionary<string, int>
                                 {
                                     { "a", 1 },
                                 };
                             }
                             """;

        AssertUnchanged(input);
    }

    #endregion // Tests
}