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

                                          public int this[int index]
                                          {
                                              get
                                              {
                                                  return _items[index];
                                              }
                                          }
                                      }
                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied indexer is converted to a block body and fully laid out
    /// by a single formatting pass
    /// </summary>
    [TestMethod]
    public void ConvertsExpressionBodiedIndexerToBlockBody()
    {
        AssertRuleResult(TestData, ResultData);
    }

    /// <summary>
    /// Verifies that several expression-bodied indexer overloads inside a nested type are converted
    /// at the correct indentation in a single pass
    /// </summary>
    [TestMethod]
    public void ConvertsOverloadedIndexersInsideNestedType()
    {
        // Arrange
        const string input = """
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
        const string expected = """
                                internal sealed class Outer
                                {
                                    internal sealed class Inner
                                    {
                                        private readonly int[] _items = [1, 2, 3];

                                        public int this[int index]
                                        {
                                            get
                                            {
                                                return _items[index];
                                            }
                                        }

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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}