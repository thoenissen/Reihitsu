using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class RegionFormattingFullPipelineTests
{
    #region Constants

    private const string TestData = """
                                    internal class RegionFormattingTestData
                                    {
                                        #region fields

                                        private int _value;

                                        #endregion

                                        #region Constructor

                                        public RegionFormattingTestData()
                                        {
                                            _value = 0;
                                        }

                                        #endregion // constructor

                                        #region methods

                                        public int GetValue()
                                        {
                                            return _value;
                                        }

                                        #endregion // Methods
                                    }
                                    """;

    private const string ResultData = """
                                      internal class RegionFormattingTestData
                                      {
                                          #region Fields

                                          private int _value;

                                          #endregion // Fields

                                          #region Constructor

                                          public RegionFormattingTestData()
                                          {
                                              _value = 0;
                                          }

                                          #endregion // Constructor

                                          #region Methods

                                          public int GetValue()
                                          {
                                              return _value;
                                          }

                                          #endregion // Methods
                                      }
                                      """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that region directives are formatted correctly.
    /// </summary>
    [TestMethod]
    public void FormatsRegionDirectives()
    {
        // Arrange
        var input = TestData;
        var expected = ResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}