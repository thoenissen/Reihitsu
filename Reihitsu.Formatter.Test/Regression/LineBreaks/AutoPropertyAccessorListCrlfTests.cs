using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #306: a single-line auto-property accessor list must stay on the
/// property declaration line even when the source uses CRLF line endings and enough content
/// precedes the property
/// </summary>
[TestClass]
public class AutoPropertyAccessorListCrlfTests
{
    #region Constants

    /// <summary>
    /// Source whose auto-property is preceded by a long using directive and file-scoped namespace
    /// </summary>
    private const string TestData = """
                                    using System.ComponentModel.DataAnnotations.Schema;

                                    namespace Reihitsu.Sample.Entities.Tables.GameData.Items;

                                    public class C
                                    {
                                        #region Properties

                                        /// <summary>
                                        /// Id of the item
                                        /// </summary>
                                        public int ItemId { get; set; }

                                        #endregion // Properties
                                    }
                                    """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Formats the given source through the full pipeline using the requested end-of-line sequence
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="endOfLine">The end-of-line sequence to use</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input, string endOfLine, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(endOfLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Normalizes every line break in the given text to CRLF regardless of the source line endings
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <returns>The text using CRLF line endings</returns>
    private static string ToCrlf(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a CRLF auto-property preceded by a long using directive and file-scoped
    /// namespace keeps its accessor list on the property line
    /// </summary>
    [TestMethod]
    public void AutoPropertyAccessorListStaysOnPropertyLineWithCrlf()
    {
        var input = ToCrlf(TestData);

        var actual = Format(input, "\r\n", TestContext.CancellationToken);

        Assert.AreEqual(input, actual, "The single-line auto-property accessor list must not be wrapped onto its own line.");
    }

    #endregion // Tests
}