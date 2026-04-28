using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineCollapser"/>
/// </summary>
[TestClass]
public class BlankLineCollapserTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that two consecutive blank lines are preserved (not collapsed)
    /// </summary>
    [TestMethod]
    public void PreservesTwoConsecutiveBlankLines()
    {
        // Arrange
        const string input = """
                             class C
                             {


                                 void M() { }
                             }
                             """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that three consecutive blank lines are collapsed to a single blank line
    /// </summary>
    [TestMethod]
    public void CollapsesThreeConsecutiveBlankLinesToOne()
    {
        // Arrange
        const string input = """
                             class C
                             {



                                 void M() { }
                             }

                             """;

        const string expected = """
                                class C
                                {

                                    void M() { }
                                }

                                """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a single blank line is preserved without modification
    /// </summary>
    [TestMethod]
    public void PreservesSingleBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {

                                 void M() { }
                             }
                             """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that code without any blank lines is preserved without modification
    /// </summary>
    [TestMethod]
    public void PreservesNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() { }
                             }
                             """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines inside a method body are collapsed to one
    /// </summary>
    [TestMethod]
    public void CollapsesBlankLinesInMethodBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;



                                     var y = 2;
                                 }
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        var y = 2;
                                    }
                                }

                                """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines between member declarations are collapsed to one
    /// </summary>
    [TestMethod]
    public void CollapsesBlankLinesBetweenMembers()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void A() { }



                                 void B() { }
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    void A() { }

                                    void B() { }
                                }

                                """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines at the start of a file are collapsed to one
    /// </summary>
    [TestMethod]
    public void CollapsesBlankLinesAtStartOfFile()
    {
        // Arrange
        const string input = """



                             class C
                             {
                             }

                             """;

        const string expected = """

                                class C
                                {
                                }

                                """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that three or more consecutive blank lines are collapsed while preserving surrounding content
    /// </summary>
    [TestMethod]
    public void CollapsesBlankLinesWithMixedContent()
    {
        // Arrange
        const string input = """
                             using System;



                             namespace N
                             {
                                 class C
                                 {
                                     void A() { }



                                     void B() { }
                                 }
                             }

                             """;

        const string expected = """
                                using System;

                                namespace N
                                {
                                    class C
                                    {
                                        void A() { }

                                        void B() { }
                                    }
                                }

                                """;

        // Act
        var actual = ApplyCollapser(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Applies the <see cref="BlankLineCollapser"/> to the given source text
    /// </summary>
    /// <param name="source">The source text to process</param>
    /// <returns>The processed source text with excessive blank lines collapsed</returns>
    private static string ApplyCollapser(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var collapser = new BlankLineCollapser();
        var result = collapser.Visit(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Methods
}