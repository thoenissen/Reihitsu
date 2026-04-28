using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full pipeline regression tests for raw string literal alignment.
/// Ensures the formatter correctly aligns raw string markers without damaging content
/// </summary>
[TestClass]
public class RawStringAlignmentFullPipelineTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Aligned strings - no change expected

    /// <summary>
    /// Verifies that an already correctly aligned raw string is not modified by the pipeline
    /// </summary>
    [TestMethod]
    public void AlreadyAlignedRawStringIsNotModified()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                             Hello World
                                             """;
                                 }
                             }
                             """";

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an already correctly aligned interpolated raw string is not modified
    /// </summary>
    [TestMethod]
    public void AlreadyAlignedInterpolatedRawStringIsNotModified()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "World";

                                     var a = $"""
                                              Hello {name}
                                              """;
                                 }
                             }
                             """";

        AssertRuleResult(input);
    }

    #endregion // Aligned strings - no change expected

    #region Misaligned strings - correction expected

    /// <summary>
    /// Verifies that a misaligned raw string in a method body is corrected by the full pipeline
    /// </summary>
    [TestMethod]
    public void MisalignedRawStringInMethodBodyIsCorrected()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 Hello
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Hello
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a misaligned interpolated raw string is corrected by the full pipeline
    /// </summary>
    [TestMethod]
    public void MisalignedInterpolatedRawStringIsCorrected()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var name = "World";

                                     var a = $"""
                                 Hello {name}
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var name = "World";

                                        var a = $"""
                                                 Hello {name}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    #endregion // Misaligned strings - correction expected

    #region Complex scenarios

    /// <summary>
    /// Verifies that raw strings in nested blocks are aligned correctly
    /// </summary>
    [TestMethod]
    public void RawStringInNestedBlockIsAlignedCorrectly()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     if (true)
                                     {
                                         var a = """
                                     Nested content
                                     """;
                                     }
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        if (true)
                                        {
                                            var a = """
                                                    Nested content
                                                    """;
                                        }
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that raw strings used as method arguments are aligned correctly
    /// </summary>
    [TestMethod]
    public void RawStringAsMethodArgumentIsAlignedCorrectly()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     Console.WriteLine("""
                                 Hello from method argument
                                 """);
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        Console.WriteLine("""
                                                          Hello from method argument
                                                          """);
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string with varied content indentation preserves relative indentation
    /// </summary>
    [TestMethod]
    public void RelativeIndentationWithinRawStringIsPreserved()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 {
                                     "name": "test",
                                     "value": 42
                                 }
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                {
                                                    "name": "test",
                                                    "value": 42
                                                }
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string with empty lines preserves them correctly
    /// </summary>
    [TestMethod]
    public void EmptyLinesInRawStringArePreserved()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 First

                                 Third
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                First

                                                Third
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiple raw strings in the same method are all aligned correctly
    /// </summary>
    [TestMethod]
    public void MultipleRawStringsInSameMethodAreAligned()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                 First string
                                 """;

                                     var b = """
                                 Second string
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                First string
                                                """;

                                        var b = """
                                                Second string
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string in a field initializer is aligned correctly
    /// </summary>
    [TestMethod]
    public void RawStringInFieldInitializerIsAlignedCorrectly()
    {
        const string input = """"
                             class C
                             {
                                 private readonly string _data = """
                                 Hello
                                 """;
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    private readonly string _data = """
                                                                    Hello
                                                                    """;
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a raw string with badly indented code alongside badly indented surrounding code
    /// is corrected by the full pipeline
    /// </summary>
    [TestMethod]
    public void BadlyIndentedCodeAndRawStringAreBothCorrected()
    {
        const string input = """"
                                 class C
                                 {
                               void M()
                               {
                               var a = """
                             Content
                             """;
                               }
                                 }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var a = """
                                                Content
                                                """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an interpolated raw string with complex expressions in interpolation holes is handled correctly
    /// </summary>
    [TestMethod]
    public void InterpolatedRawStringWithComplexExpressionsIsHandled()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var items = new[] { 1, 2, 3 };

                                     var a = $"""
                                 Count: {items.Length}
                                 First: {items[0]}
                                 """;
                                 }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var items = new[] { 1, 2, 3 };

                                        var a = $"""
                                                 Count: {items.Length}
                                                 First: {items[0]}
                                                 """;
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the formatter does not damage the semantic value of a raw string.
    /// Content that begins with the closing marker column should remain intact
    /// </summary>
    [TestMethod]
    public void ContentSemanticValueIsPreserved()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var a = """
                                             Hello World
                                             """;

                                     var b = $"""
                                              Test {42}
                                              """;
                                 }
                             }
                             """";

        // Already aligned — content must remain exactly the same
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a raw string used as an argument with misaligned content is corrected without damaging the content
    /// </summary>
    [TestMethod]
    public void RawStringAsArgumentWithMisalignedContentIsCorrectedWithoutDamage()
    {
        const string input = """"
                             class C
                             {
                                 void L(string a)
                                 {
                                 }
                             
                                 void M() {  L("""
                                 Test
                                 """); }
                             }
                             """";

        const string expected = """"
                                class C
                                {
                                    void L(string a)
                                    {
                                    }
                                
                                    void M()
                                    {
                                        L("""
                                          Test
                                          """);
                                    }
                                }
                                """";

        AssertRuleResult(input, expected);
    }

    #endregion // Complex scenarios
}