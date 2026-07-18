using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLinePhase"/> — blank lines required before <c>#region</c> and <c>#endregion</c>
/// directives that directly follow a switch-label colon. Exercised at the phase level rather than through the
/// full pipeline because <see cref="Reihitsu.Formatter.Pipeline.RegionFormatting.NestedRegionRemovalStep"/>
/// unconditionally strips region directives nested inside a switch statement, which would otherwise remove the
/// directives under test before blank-line formatting ever runs
/// </summary>
[TestClass]
public class BlankLineRegionDirectivePhaseTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>#region</c> directive that directly follows a
    /// switch-label colon. RH5031's Core policy exempts only lines ending with an opening brace, unlike the
    /// general first-in-block exemption that also treats switch labels as exempt (issue #428)
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeRegionAfterSwitchLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             #region R
                                             Bar();
                                             #endregion
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:

                                                #region R

                                                Bar();

                                                #endregion

                                                break;
                                        }
                                    }
                                }
                                """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before an <c>#endregion</c> directive that directly follows a
    /// switch-label colon, the symmetric counterpart of the <c>#region</c> case (issue #428)
    /// </summary>
    [TestMethod]
    public void InsertsBlankLineBeforeEndRegionAfterSwitchLabel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int value)
                                 {
                                     #region R
                                     switch (value)
                                     {
                                         case 1:
                                         #endregion
                                             Bar();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int value)
                                    {
                                        #region R

                                        switch (value)
                                        {
                                            case 1:

                                            #endregion

                                                Bar();
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Applies the <see cref="BlankLinePhase"/> to the given source text
    /// </summary>
    /// <param name="source">The source text to rewrite</param>
    /// <returns>The rewritten source text</returns>
    private static string ApplyRewriter(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(Environment.NewLine);
        var result = new BlankLinePhase().Execute(tree.GetRoot(), context, CancellationToken.None);

        return result.ToFullString();
    }

    #endregion // Methods
}