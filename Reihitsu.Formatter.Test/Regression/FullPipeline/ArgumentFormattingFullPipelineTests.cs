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
    /// Verifies that outer arguments fully split when a nested anonymous object becomes multi-line
    /// </summary>
    [TestMethod]
    public void FormatsArgumentsWhenNestedAnonymousObjectBecomesMultiLine()
    {
        const string input = """
                             internal class NestedAnonymousObjectArgumentTestData
                             {
                                 void Method()
                                 {
                                     Call("first", Project(new { Alpha = 1, Beta = 2 }), "third");
                                 }

                                 string Project(object value) => value.ToString();

                                 void Call(string first, string second, string third) { }
                             }
                             """;

        const string expected = """
                                internal class NestedAnonymousObjectArgumentTestData
                                {
                                    void Method()
                                    {
                                        Call("first",
                                             Project(new
                                                     {
                                                         Alpha = 1,
                                                         Beta = 2
                                                     }),
                                             "third");
                                    }

                                    string Project(object value)
                                    {
                                        return value.ToString();
                                    }

                                    void Call(string first, string second, string third)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that outer arguments fully split when a nested invocation becomes multi-line after child rewrites
    /// </summary>
    [TestMethod]
    public void FormatsOuterArgumentsWhenNestedInvocationBecomesMultiLine()
    {
        const string input = """
                             internal class NestedInvocationArgumentTestData
                             {
                                 void Method()
                                 {
                                     Outer("first", Inner("prefix", Wrap(new { Alpha = 1, Beta = 2 }), "suffix"), "third");
                                 }

                                 string Wrap(object value) => value.ToString();

                                 string Inner(string prefix, string value, string suffix) => prefix + value + suffix;

                                 void Outer(string first, string second, string third) { }
                             }
                             """;

        const string expected = """
                                internal class NestedInvocationArgumentTestData
                                {
                                    void Method()
                                    {
                                        Outer("first",
                                              Inner("prefix",
                                                    Wrap(new
                                                         {
                                                             Alpha = 1,
                                                             Beta = 2
                                                         }),
                                                    "suffix"),
                                              "third");
                                    }

                                    string Wrap(object value)
                                    {
                                        return value.ToString();
                                    }

                                    string Inner(string prefix, string value, string suffix)
                                    {
                                        return prefix + value + suffix;
                                    }

                                    void Outer(string first, string second, string third)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that outer arguments fully split when an expression-lambda body becomes
    /// multi-line because it wraps a multi-member anonymous object
    /// </summary>
    [TestMethod]
    public void FormatsArgumentsWhenLambdaContainsMultiMemberAnonymousObject()
    {
        const string input = """
                             internal class LambdaAnonymousObjectArgumentTestData
                             {
                                 void Method()
                                 {
                                     PrimaryKey("first", value => new { value.Alpha, value.Beta }, "third");
                                 }

                                 void PrimaryKey(string first, object second, string third) { }
                             }
                             """;

        const string expected = """
                                internal class LambdaAnonymousObjectArgumentTestData
                                {
                                    void Method()
                                    {
                                        PrimaryKey("first",
                                                   value => new
                                                            {
                                                                value.Alpha,
                                                                value.Beta
                                                            },
                                                   "third");
                                    }

                                    void PrimaryKey(string first, object second, string third)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that outer arguments fully split when an expression-lambda body becomes
    /// multi-line because it wraps an object initializer
    /// </summary>
    [TestMethod]
    public void FormatsArgumentsWhenLambdaContainsMultiMemberObjectInitializer()
    {
        const string input = """
                             using System;

                             internal class LambdaObjectInitializerArgumentTestData
                             {
                                 void Method()
                                 {
                                     Create("first", value => new Settings { Alpha = 1, Beta = 2 }, "third");
                                 }

                                 void Create(string first, Func<int, Settings> second, string third) { }
                             }

                             internal class Settings
                             {
                                 internal int Alpha { get; set; }

                                 internal int Beta { get; set; }
                             }
                             """;

        const string expected = """
                                using System;

                                internal class LambdaObjectInitializerArgumentTestData
                                {
                                    void Method()
                                    {
                                        Create("first",
                                               value => new Settings
                                                        {
                                                            Alpha = 1,
                                                            Beta = 2
                                                        },
                                               "third");
                                    }

                                    void Create(string first, Func<int, Settings> second, string third)
                                    {
                                    }
                                }

                                internal class Settings
                                {
                                    internal int Alpha { get; set; }

                                    internal int Beta { get; set; }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that outer arguments fully split when an expression-lambda body invocation
    /// becomes multi-line after its nested anonymous object is rewritten
    /// </summary>
    [TestMethod]
    public void FormatsArgumentsWhenLambdaBodyInvocationBecomesMultiLine()
    {
        const string input = """
                             internal class LambdaInvocationArgumentTestData
                             {
                                 void Method()
                                 {
                                     Call("first", value => Project(new { Alpha = 1, Beta = 2 }), "third");
                                 }

                                 string Project(object value) => value.ToString();

                                 void Call(string first, object second, string third) { }
                             }
                             """;

        const string expected = """
                                internal class LambdaInvocationArgumentTestData
                                {
                                    void Method()
                                    {
                                        Call("first",
                                             value => Project(new
                                                              {
                                                                  Alpha = 1,
                                                                  Beta = 2
                                                              }),
                                             "third");
                                    }

                                    string Project(object value)
                                    {
                                        return value.ToString();
                                    }

                                    void Call(string first, object second, string third)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a statement-lambda used as the second argument makes the outer
    /// argument list multi-line and formats the lambda block body correctly
    /// </summary>
    [TestMethod]
    public void FormatsStatementLambdaAsSecondArgumentAndSplitsOuterArguments()
    {
        const string input = """
                             internal class StatementLambdaSecondArgumentTestData
                             {
                                 void Method()
                                 {
                                     var changed = Apply(entry => entry.Key == currentKey, entry => { entry.State = nextState; entry.Payload = nextPayload; });
                                 }

                                 bool Apply(System.Func<Entry, bool> predicate, System.Action<Entry> action)
                                 {
                                     return true;
                                 }

                                 string currentKey;
                                 string nextPayload;
                                 int nextState;

                                 internal class Entry
                                 {
                                     internal string Key { get; set; }

                                     internal int State { get; set; }

                                     internal string Payload { get; set; }
                                 }
                             }
                             """;

        const string expected = """
                                internal class StatementLambdaSecondArgumentTestData
                                {
                                    void Method()
                                    {
                                        var changed = Apply(entry => entry.Key == currentKey,
                                                            entry =>
                                                            {
                                                                entry.State = nextState;
                                                                entry.Payload = nextPayload;
                                                            });
                                    }

                                    bool Apply(System.Func<Entry, bool> predicate, System.Action<Entry> action)
                                    {
                                        return true;
                                    }

                                    string currentKey;
                                    string nextPayload;
                                    int nextState;

                                    internal class Entry
                                    {
                                        internal string Key { get; set; }

                                        internal int State { get; set; }

                                        internal string Payload { get; set; }
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