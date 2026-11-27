using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzerTests : AnalyzerTestsBase<RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a simple lambda parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task VerifyDiagnosticForSimpleLambdaParameter()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int> increment = {|#0:n|} => n + 1;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for parenthesized lambda and anonymous method parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task VerifyDiagnosticsForParenthesizedLambdaAndAnonymousMethodParameters()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int, int> add = ({|#0:x|}, {|#1:y|}) => x + y;
                                        Func<int, int> increment = delegate(int {|#2:n|}) { return n + 1; };
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive names or the discard identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task VerifyNoDiagnosticsForDescriptiveNamesAndDiscardIdentifier()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int> increment = value => value + 1;
                                        Func<int, int> ignore = _ => 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for method and local function parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-002")]
    public async Task VerifyNoDiagnosticsForOtherParameterKinds()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int p)
                                    {
                                        static int Double(int n) => n * 2;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter parameters of simple lambdas, parenthesized lambdas, and anonymous methods are reported, while discards and descriptive names are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task SingleLetterLambdaAndAnonymousMethodParametersAreReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = {|RH4124:x|} => x;
                                        Func<int, int, int> g = ({|RH4124:a|}, {|RH4124:B|}) => a + B;
                                        Action<int> h = delegate (int {|RH4124:v|}) { };
                                        Func<int, int, int> i = (_, _) => 0;
                                        Func<int, int> j = value => value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter parameters of a method and a local function are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-002")]
    public async Task MethodAndLocalFunctionParametersAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        int Local(int b) => b;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter range variables declared by <c>from</c>, <c>let</c>, <c>join</c>, a join's <c>into</c>, and a query continuation are reported, while a discard is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task SingleLetterQueryRangeVariablesAreReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values, int[] others)
                                    {
                                        var doubled = from {|RH4124:x|} in values
                                                      let {|RH4124:y|} = x * 2
                                                      select y;

                                        var joined = from int value in values
                                                     join {|RH4124:o|} in others on value equals o
                                                     select value + o;

                                        var grouped = from value in values
                                                      join other in others on value equals other into {|RH4124:g|}
                                                      select g.Count();

                                        var continued = from value in values
                                                        group value by value % 2 into {|RH4124:p|}
                                                        select p.Key into {|RH4124:k|}
                                                        select k;

                                        var nested = from {|RH4124:@a|} in values
                                                     from _ in others
                                                     select @a;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies range variables with longer names are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task QueryRangeVariablesWithLongerNamesAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values)
                                    {
                                        var names = from value in values
                                                    let doubledValue = value * 2
                                                    select doubledValue;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the range variable from the reported shape is reported once on its declaration, and its uses in <c>let</c>, <c>where</c>, <c>orderby</c>, and <c>select</c> are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task UsesOfQueryRangeVariablesInLaterClausesAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values)
                                    {
                                        var doubled = from {|RH4124:x|} in values
                                                      where x > 0
                                                      orderby x descending
                                                      let {|RH4124:y|} = x * 2
                                                      select y;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a single-letter range variable of the first <c>from</c> clause with an explicit type is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task SingleLetterRangeVariableOfExplicitlyTypedFromClauseIsReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(object[] objects)
                                    {
                                        var numbers = from int {|RH4124:t|} in objects
                                                      select t;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a single-letter range variable of a second <c>from</c> clause is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task SingleLetterRangeVariableOfSecondFromClauseIsReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values, int[] others)
                                    {
                                        var sums = from value in values
                                                   from {|RH4124:u|} in others
                                                   select value + u;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a single-letter range variable declared by the continuation of a <c>group</c> clause is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task SingleLetterRangeVariableOfGroupContinuationIsReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values)
                                    {
                                        var keys = from value in values
                                                   group value by value into {|RH4124:h|}
                                                   select h.Key;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a discard range variable is not reported in any clause that declares a range variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-001")]
    [Requirement("RH4124-003")]
    public async Task DiscardRangeVariablesAreNotReported()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values, int[] others)
                                    {
                                        var fromDiscard = from _ in values select 0;
                                        var letDiscard = from value in values let _ = value select value;
                                        var joinDiscard = from value in values join _ in others on value equals _ select value;
                                        var joinIntoDiscard = from value in values join other in others on value equals other into _ select value;
                                        var continuationDiscard = from value in values select value into _ select 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies uppercase, verbatim, and non-ASCII single-letter range variables are reported, while a two-letter one is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-001")]
    [Requirement("RH4124-003")]
    public async Task SingleLetterRangeVariableIsOneCharacterOtherThanUnderscore()
    {
        const string testCode = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] values)
                                    {
                                        var upper = from {|RH4124:X|} in values select X;
                                        var verbatim = from {|RH4124:@b|} in values select @b;
                                        var nonAscii = from {|RH4124:é|} in values select é;
                                        var twoLetters = from ab in values select ab;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter range variables are reported in a field initializer, an expression-bodied property, a nested query, a query inside a lambda, and a query inside a local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-003")]
    public async Task SingleLetterRangeVariablesAreReportedInEveryEnclosingScope()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    static IEnumerable<int> Field = from {|RH4124:f|} in new[] { 1 } select f;

                                    IEnumerable<int> Property => from {|RH4124:p|} in new[] { 1 } select p;

                                    void M(int[] values, int[] others)
                                    {
                                        var counts = from value in values
                                                     select (from {|RH4124:z|} in others where z == value select z).Count();

                                        Func<int[], IEnumerable<int>> project = array => from {|RH4124:q|} in array select q;

                                        int Sum(int[] array) => (from {|RH4124:r|} in array select r).Sum();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}