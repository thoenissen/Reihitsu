using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.StructuralTransforms;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Reproduction gate scratch tests for issue #712 (trailing comma removal near a preprocessor conditional block).
/// Not intended to be committed by this gate.
/// </summary>
[TestClass]
public class Issue712ReproTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the trailing comma after the last element before an "#if" conditional block is preserved,
    /// using the issue's exact reported input (LF line endings, SYMBOL undefined, array initializer, top-level statement).
    /// </summary>
    [TestMethod]
    public void PreservesTrailingCommaBeforePreprocessorConditionalBlockArrayTopLevelLF()
    {
        // Arrange
        const string input = """
                             var array = new[]
                             {
                                 1,
                             #if SYMBOL
                                 2,
                             #endif
                             };

                             """;

        // Act
        var actual = ExecutePhase(input, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Executes the <see cref="StructuralTransformPhase"/> on the given input
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The transformed source text</returns>
    private static string ExecutePhase(string input, CancellationToken cancellationToken)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: cancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = new StructuralTransformPhase().Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    #endregion // Methods
}