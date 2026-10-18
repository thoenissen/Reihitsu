using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4126SingleLetterForeachIterationVariableNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4126SingleLetterForeachIterationVariableNamesAnalyzerTests : AnalyzerTestsBase<RH4126SingleLetterForeachIterationVariableNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a single-letter foreach iteration variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4126-001")]
    public async Task VerifyDiagnosticForForeachIterationVariable()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int[] items)
                                    {
                                        foreach (var {|#0:i|} in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4126SingleLetterForeachIterationVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4126MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for deconstructed foreach iteration variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4126-001")]
    public async Task VerifyDiagnosticsForDeconstructedForeachIterationVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process((int Left, int Top)[] items)
                                    {
                                        foreach (var ({|#0:x|}, {|#1:y|}) in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4126SingleLetterForeachIterationVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4126MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a descriptive name or the discard identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-001")]
    [Requirement("RH4126-001")]
    public async Task VerifyNoDiagnosticsForDescriptiveNameAndDiscardIdentifier()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int[] items)
                                    {
                                        foreach (var item in items)
                                        {
                                        }

                                        foreach (var _ in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for regular local variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4126-002")]
    public async Task VerifyNoDiagnosticsForRegularLocalVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var i = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies local designations in a foreach body are left to the local-variable rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4126-002")]
    public async Task VerifyNoDiagnosticsForDesignationsInForeachBody()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object[] items)
                                    {
                                        foreach (var item in items)
                                        {
                                            if (item is int x)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter iteration variables, including each variable of a deconstructing foreach statement, are reported, while descriptive names and discards are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4126-001")]
    public async Task SingleLetterIterationVariablesAreReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int[] values, List<(int, int)> pairs)
                                    {
                                        foreach (var {|RH4126:x|} in values)
                                        {
                                        }

                                        foreach (var ({|RH4126:k|}, {|RH4126:v|}) in pairs)
                                        {
                                        }

                                        foreach (var (key, _) in pairs)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the single variable, the variables of a var deconstruction, of a parenthesized deconstruction, and of a nested deconstruction of a foreach statement all count as iteration variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4126-001")]
    public async Task IterationVariablesOfEveryForeachFormAtAnyNestingDepthAreReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(List<(int, int)> pairs, List<(int, (int, int))> nested, int[] values)
                                    {
                                        foreach (var {|RH4126:x|} in values)
                                        {
                                        }

                                        foreach (var ({|RH4126:k|}, {|RH4126:v|}) in pairs)
                                        {
                                        }

                                        foreach ((var {|RH4126:a|}, var {|RH4126:b|}) in pairs)
                                        {
                                        }

                                        foreach ((int {|RH4126:c|}, var ({|RH4126:d|}, {|RH4126:e|})) in nested)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies ordinary locals and variables declared in the collection expression or the body of a foreach statement are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4126-002")]
    [Requirement("SHARED-SINGLE-002")]
    public async Task LocalsOutsideTheIterationVariablesAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = 0;

                                        foreach (var item in Get(out var z))
                                        {
                                            var y = item;
                                        }
                                    }

                                    int[] Get(out int count)
                                    {
                                        count = 0;

                                        return [];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}