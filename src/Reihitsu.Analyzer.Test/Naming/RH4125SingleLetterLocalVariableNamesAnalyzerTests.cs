using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4125SingleLetterLocalVariableNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4125SingleLetterLocalVariableNamesAnalyzerTests : AnalyzerTestsBase<RH4125SingleLetterLocalVariableNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for local variable declarators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4125-001")]
    public async Task VerifyDiagnosticsForLocalVariableDeclarators()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var {|#0:x|} = 1;

                                        for (var {|#1:i|} = 0; i < x; i++)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4125SingleLetterLocalVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4125MessageFormat, 2));
    }

    /// <summary>
    /// Verifies diagnostics are reported for out, pattern, and deconstruction variable designations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4125-001")]
    public async Task VerifyDiagnosticsForVariableDesignations()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object value)
                                    {
                                        _ = int.TryParse("1", out var {|#0:x|});

                                        if (value is int {|#1:y|})
                                        {
                                        }

                                        var ({|#2:l|}, {|#3:t|}) = GetCoordinates();
                                    }

                                    private static (int Left, int Top) GetCoordinates()
                                    {
                                        return (1, 2);
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4125SingleLetterLocalVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4125MessageFormat, 4));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive local names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4125-001")]
    public async Task VerifyNoDiagnosticsForDescriptiveLocalNames()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object value)
                                    {
                                        var count = 1;
                                        _ = int.TryParse("1", out var result);

                                        if (value is int number)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for discard forms
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-001")]
    [Requirement("RH4125-001")]
    public async Task VerifyNoDiagnosticsForDiscards()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var _ = 1;
                                        int.TryParse("1", out _);
                                        var (_, value) = GetCoordinates();
                                    }

                                    private static (int Left, int Top) GetCoordinates()
                                    {
                                        return (1, 2);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4125-002")]
    public async Task VerifyNoDiagnosticsForFields()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    private int x;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies deconstructed foreach variables are left to the foreach-specific rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4125-002")]
    public async Task VerifyNoDiagnosticsForDeconstructedForeachVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process((int Left, int Top)[] items)
                                    {
                                        foreach (var (x, y) in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies local designations in a foreach body remain covered by the local-variable rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4125-001")]
    public async Task VerifyDiagnosticForDesignationInForeachBody()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object[] items)
                                    {
                                        foreach (var item in items)
                                        {
                                            if (item is int {|#0:x|})
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4125SingleLetterLocalVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4125MessageFormat));
    }

    /// <summary>
    /// Verifies single-letter locals declared by declarators, including const, using, for, using statement, and fixed declarations, and by out, pattern, and deconstruction designations are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4125-001")]
    public async Task SingleLetterLocalsOfEveryDeclaratorAndDesignationAreReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    unsafe void M(object input, int[] values, (int, int) pair)
                                    {
                                        int {|RH4125:t|} = 0, total = 1;
                                        const int {|RH4125:K|} = 2;
                                        using var {|RH4125:s|} = (IDisposable)null;

                                        for (int {|RH4125:i|} = 0; i < 1; i++)
                                        {
                                        }

                                        using (var {|RH4125:r|} = (IDisposable)null)
                                        {
                                        }

                                        fixed (int* {|RH4125:p|} = values)
                                        {
                                        }

                                        int.TryParse("1", out var {|RH4125:n|});

                                        if (input is int {|RH4125:x|})
                                        {
                                        }

                                        var ({|RH4125:a|}, {|RH4125:b|}) = pair;
                                        (var {|RH4125:c|}, var {|RH4125:d|}) = pair;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies foreach iteration variables, including deconstructed ones, catch variables, and fields are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4125-002")]
    [Requirement("SHARED-SINGLE-002")]
    public async Task IterationVariablesCatchVariablesAndFieldsAreNotChecked()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    private int f;

                                    void M(int[] values, List<(int, int)> pairs)
                                    {
                                        foreach (var x in values)
                                        {
                                        }

                                        foreach (var (k, v) in pairs)
                                        {
                                        }

                                        try
                                        {
                                        }
                                        catch (Exception e)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies variables declared in the collection expression or the body of a foreach statement are reported as ordinary locals
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-002")]
    [Requirement("RH4125-001")]
    public async Task LocalsDeclaredInForeachCollectionOrBodyAreNotIterationVariables()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        foreach (var item in Get(out var {|RH4125:z|}))
                                        {
                                            var {|RH4125:y|} = item;
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