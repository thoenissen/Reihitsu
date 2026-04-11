using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class HorizontalSpacingFullPipelineTests
{
    #region Constants

    private const string TestData = """
        internal class HorizontalSpacingTestData
        {
            public void Method()
            {
                var x=1;
                var y = x+2;
                var z = x  +  y;
                var list = new int[] { 1,2,3 };

                if(x == 1)
                {
                    System.Console.WriteLine( x );
                }

                for (var i=0; i<10; i++)
                {
                }
            }
        }
        """;

    private const string ResultData = """
        internal class HorizontalSpacingTestData
        {
            public void Method()
            {
                var x = 1;
                var y = x + 2;
                var z = x + y;
                var list = new int[] { 1, 2, 3 };

                if (x == 1)
                {
                    System.Console.WriteLine(x);
                }

                for (var i = 0; i < 10; i++)
                {
                }
            }
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
    /// Verifies that horizontal spacing is normalized correctly.
    /// </summary>
    [TestMethod]
    public void NormalizesHorizontalSpacing()
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