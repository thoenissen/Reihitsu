using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/> and <see cref="RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests : AnalyzerTestsBase<RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer, RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider>
{
    /// <summary>
    /// Verifies that correctly formatted generic constraints do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
    /// Verifies that generic constraints on the wrong line or with the wrong indentation are detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example<T> {|#0:where|} T : class
                                {
                                }

                                internal struct ExampleStruct<T>
                                {|#1:where|} T : struct
                                {
                                }

                                internal interface IExample<T> {|#2:where|} T : class
                                {
                                }

                                internal delegate void ExampleDelegate<T>()
                                {|#3:where|} T : class;

                                internal record ExampleRecord<T>(T Value) {|#4:where|} T : class;

                                internal class Container
                                {
                                    internal void Method<TKey, TValue>() {|#5:where|} TKey : notnull
                                    {|#6:where|} TValue : class
                                    {
                                        void Local<TLocal>()
                                        {|#7:where|} TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
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

        await Verify(testData, fixedData, Diagnostics(RH0384GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH0384MessageFormat, 8));
    }
}
