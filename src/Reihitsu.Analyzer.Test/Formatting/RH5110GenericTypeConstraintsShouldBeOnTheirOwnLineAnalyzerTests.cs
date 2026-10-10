using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/> and <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests : BatchCodeFixTestsBase<RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer, RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that correctly formatted generic constraints do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenGenericConstraintsAreFormattedCorrectly()
    {
        const string testData = """
                                internal class Example<T>
                                    where T : class
                                {
                                }

                                internal struct ExampleStruct<T>
                                    where T : struct
                                {
                                }

                                internal interface IExample<T>
                                    where T : class
                                {
                                }

                                internal delegate void ExampleDelegate<T>()
                                    where T : class;

                                internal record ExampleRecord<T>(T Value)
                                    where T : class;

                                internal class Container
                                {
                                    internal void Method<TKey, TValue>()
                                        where TKey : notnull
                                        where TValue : class
                                    {
                                        void Local<TLocal>()
                                            where TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that class generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyClassConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example<T> {|#0:where|} T : class
                                {
                                    public int Bar { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example<T>
                                     where T : class
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    /// <summary>
    /// Verifies that struct generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal struct ExampleStruct<T>
                                {|#0:where|} T : struct
                                {
                                    public int Bar { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal struct ExampleStruct<T>
                                     where T : struct
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    /// <summary>
    /// Verifies that interface generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterfaceConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal interface IExample<T> {|#0:where|} T : class
                                {
                                    public int Bar { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal interface IExample<T>
                                     where T : class
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    /// <summary>
    /// Verifies that delegate generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDelegateConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal delegate void ExampleDelegate<T>()
                                {|#0:where|} T : class;
                                """;
        const string fixedData = """
                                 internal delegate void ExampleDelegate<T>()
                                     where T : class;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    /// <summary>
    /// Verifies that record generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal record ExampleRecord<T>(T Value) {|#0:where|} T : class;
                                """;
        const string fixedData = """
                                 internal record ExampleRecord<T>(T Value)
                                     where T : class;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    /// <summary>
    /// Verifies that method generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMethodConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Container
                                {
                                    internal void Method<TKey, TValue>() {|#0:where|} TKey : notnull
                                    where TValue : class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Container
                                 {
                                     internal void Method<TKey, TValue>()
                                         where TKey : notnull
                                         where TValue : class
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    /// <summary>
    /// Verifies that local function generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLocalFunctionConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Container
                                {
                                    internal void Method()
                                    {
                                        void Local<TLocal>()
                                        {|#0:where|} TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Container
                                 {
                                     internal void Method()
                                     {
                                         void Local<TLocal>()
                                             where TLocal : class
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Container
                                {
                                    internal void Outer<TOuter>() {|#0:where|} TOuter : class
                                    {
                                        void Local<TLocal>()
                                        {|#1:where|} TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Container
                                 {
                                     internal void Outer<TOuter>()
                                         where TOuter : class
                                     {
                                         void Local<TLocal>()
                                             where TLocal : class
                                         {
                                         }
                                     }
                                 }
                                 """;

        // The outer diagnostic's fix reformats the whole outer method declaration, whose span fully contains the
        // local function rewritten by the inner diagnostic's fix, so the batch fixer discards the overlapping
        // inner change in its first pass; the outer rewrite already carries the correctly formatted nested
        // constraint clause, but the batch fixer still needs a second pass to converge because its first pass
        // re-analyzes and re-applies the (now redundant) inner fix before recognizing it is a no-op
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5110MessageFormat, 2),
                                  test => test.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}