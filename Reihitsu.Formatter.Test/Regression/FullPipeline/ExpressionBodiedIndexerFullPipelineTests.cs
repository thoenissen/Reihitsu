using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class ExpressionBodiedIndexerFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for expression-bodied-indexer formatting scenarios
    /// </summary>
    private const string TestData = """
                                    internal sealed class Values
                                    {
                                        private readonly int[] _items = [1, 2, 3];

                                        public int this[int index] => _items[index];
                                    }
                                    """;

    /// <summary>
    /// Expected formatter output for expression-bodied-indexer scenarios
    /// </summary>
    private const string ResultData = """
                                      internal sealed class Values
                                      {
                                          private readonly int[] _items = [1, 2, 3];

                                          public int this[int index] => _items[index];
                                      }
                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied indexer stays expression-bodied through the full pipeline
    /// </summary>
    [TestMethod]
    public void KeepsExpressionBodiedIndexer()
    {
        AssertRuleResult(TestData, ResultData);
    }

    /// <summary>
    /// Verifies that several get-only indexer overloads inside a nested type become expression-bodied indexers at
    /// the correct indentation in a single pass, whichever body form they were written in
    /// </summary>
    [TestMethod]
    public void ConvertsOverloadedGetOnlyIndexersInsideNestedType()
    {
        // Arrange
        const string input = """
                             internal sealed class Outer
                             {
                                 internal sealed class Inner
                                 {
                                     private readonly int[] _items = [1, 2, 3];

                                     public int this[int index] => _items[index];

                                     public int this[string key]
                                     {
                                         get
                                         {
                                             return _items[key.Length];
                                         }
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                internal sealed class Outer
                                {
                                    internal sealed class Inner
                                    {
                                        private readonly int[] _items = [1, 2, 3];

                                        public int this[int index] => _items[index];

                                        public int this[string key] => _items[key.Length];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}