using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/> and <see cref="RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests : AnalyzerTestsBase<RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer, RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider>
{
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
                                }
                                """;
        const string fixedData = """
                                 internal class Example<T>
                                     where T : class
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
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
                                }
                                """;
        const string fixedData = """
                                 internal struct ExampleStruct<T>
                                     where T : struct
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
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
                                }
                                """;
        const string fixedData = """
                                 internal interface IExample<T>
                                     where T : class
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
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
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
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
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
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
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
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
                     Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat));
    }
}