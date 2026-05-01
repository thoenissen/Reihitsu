using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline regression tests for argument formatting rules:
/// first-argument collapse, mixed-line splitting, and argument alignment
/// </summary>
[TestClass]
public class ArgumentFormattingFullPipelineTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that simple method call arguments are formatted correctly
    /// </summary>
    [TestMethod]
    public void FormatsSimpleMethodCallArguments()
    {
        const string input = """
                             using System;

                             internal class SimpleMethodCallTestData
                             {
                                 void MixedLineArguments()
                                 {
                                     Console.WriteLine("test1", "test2",
                                                       "test3");
                                 }

                                 void FirstArgumentOnNewLine()
                                 {
                                     Console.WriteLine(
                                         "test1",
                                         "test2");
                                 }

                                 void MisalignedArguments()
                                 {
                                     Console.WriteLine("test1",
                                       "test2",
                                             "test3");
                                 }

                                 void SingleLineArguments()
                                 {
                                     Console.WriteLine("test1", "test2", "test3");
                                 }

                                 void AlreadyCorrectMultiLine()
                                 {
                                     Console.WriteLine("test1",
                                                       "test2",
                                                       "test3");
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                internal class SimpleMethodCallTestData
                                {
                                    void MixedLineArguments()
                                    {
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");
                                    }

                                    void FirstArgumentOnNewLine()
                                    {
                                        Console.WriteLine("test1",
                                                          "test2");
                                    }

                                    void MisalignedArguments()
                                    {
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");
                                    }

                                    void SingleLineArguments()
                                    {
                                        Console.WriteLine("test1", "test2", "test3");
                                    }

                                    void AlreadyCorrectMultiLine()
                                    {
                                        Console.WriteLine("test1",
                                                          "test2",
                                                          "test3");
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested call arguments are formatted correctly
    /// </summary>
    [TestMethod]
    public void FormatsNestedCallArguments()
    {
        const string input = """
                             using System;

                             internal class NestedCallsTestData
                             {
                                 void NestedCalls()
                                 {
                                     Outer(Inner("a", "b",
                                                 "c"),
                                           "d",
                                           "e");
                                 }

                                 void NestedCallsAllMixed()
                                 {
                                     Outer(Inner("a",
                                                 "b"), "d",
                                           "e");
                                 }

                                 string Inner(string a, string b, string c = null) => null;

                                 void Outer(string a, string b, string c) { }
                             }
                             """;

        const string expected = """
                                using System;

                                internal class NestedCallsTestData
                                {
                                    void NestedCalls()
                                    {
                                        Outer(Inner("a",
                                                    "b",
                                                    "c"),
                                              "d",
                                              "e");
                                    }

                                    void NestedCallsAllMixed()
                                    {
                                        Outer(Inner("a",
                                                    "b"),
                                              "d",
                                              "e");
                                    }

                                    string Inner(string a, string b, string c = null)
                                    {
                                        return null;
                                    }

                                    void Outer(string a, string b, string c)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that constructor call arguments are formatted correctly
    /// </summary>
    [TestMethod]
    public void FormatsConstructorCallArguments()
    {
        const string input = """
                             internal class ConstructorCallTestData
                             {
                                 void Method()
                                 {
                                     var obj = new MyClass("a", "b",
                                                          "c");
                                 }
                             }

                             internal class MyClass
                             {
                                 internal MyClass(string a, string b, string c) { }
                             }
                             """;

        const string expected = """
                                internal class ConstructorCallTestData
                                {
                                    void Method()
                                    {
                                        var obj = new MyClass("a",
                                                              "b",
                                                              "c");
                                    }
                                }

                                internal class MyClass
                                {
                                    internal MyClass(string a, string b, string c)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multi-line arguments (e.g. method chains as arguments) are handled correctly
    /// </summary>
    [TestMethod]
    public void FormatsMultiLineArguments()
    {
        const string input = """
                             using System.Linq;

                             internal class MultiLineArgumentTestData
                             {
                                 void Method()
                                 {
                                     Call("test1",
                                          items.Where(i => i > 0)
                                               .Select(i => i.ToString())
                                               .ToArray(),
                                          "test3");
                                 }

                                 int[] items;

                                 void Call(string a, string[] b, string c) { }
                             }
                             """;

        const string expected = """
                                using System.Linq;

                                internal class MultiLineArgumentTestData
                                {
                                    void Method()
                                    {
                                        Call("test1",
                                             items.Where(i => i > 0)
                                                  .Select(i => i.ToString())
                                                  .ToArray(),
                                             "test3");
                                    }

                                    int[] items;

                                    void Call(string a, string[] b, string c)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that method chains with mixed-line arguments are formatted correctly
    /// </summary>
    [TestMethod]
    public void FormatsMethodChainWithArguments()
    {
        const string input = """
                             using System.Linq;

                             internal class MethodChainWithArgumentsTestData
                             {
                                 void Method()
                                 {
                                     var result = items.Where(i => i > 0)
                                                      .Select(i => i.ToString())
                                                      .Aggregate(
                                                      "seed", (a, b) =>
                                                                         { return a + b; },
                                                                 r => r.ToUpper());
                                 }

                                 int[] items;
                             }
                             """;

        const string expected = """
                                using System.Linq;

                                internal class MethodChainWithArgumentsTestData
                                {
                                    void Method()
                                    {
                                        var result = items.Where(i => i > 0)
                                                          .Select(i => i.ToString())
                                                          .Aggregate("seed",
                                                                     (a, b) =>
                                                                     {
                                                                         return a + b;
                                                                     },
                                                                     r => r.ToUpper());
                                    }

                                    int[] items;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}