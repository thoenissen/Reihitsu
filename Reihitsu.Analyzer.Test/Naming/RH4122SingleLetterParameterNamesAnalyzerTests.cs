using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4122SingleLetterParameterNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4122SingleLetterParameterNamesAnalyzerTests : AnalyzerTestsBase<RH4122SingleLetterParameterNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for method, constructor, and indexer parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForSupportedParameterKinds()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public TestClass(int {|#0:c|})
                                    {
                                    }

                                    public int this[int {|#1:i|}] => i;

                                    public void Process(int {|#2:p|})
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4122SingleLetterParameterNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4122MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class primary constructor parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForClassPrimaryConstructorParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass(int {|#0:c|})
                                {
                                    public int Count { get; } = c;
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4122SingleLetterParameterNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4122MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for delegate, operator, and conversion parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForAdditionalParameterKinds()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public delegate void TestDelegate(int {|#0:d|});

                                public class TestClass
                                {
                                    public static TestClass operator +(TestClass {|#1:l|}, TestClass {|#2:r|})
                                    {
                                        return l;
                                    }

                                    public static explicit operator int(TestClass {|#3:v|})
                                    {
                                        return 0;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4122SingleLetterParameterNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4122MessageFormat, 4));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive names or the discard identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDescriptiveNamesAndDiscardIdentifier()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int count, int _)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for override parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverrideParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public abstract class TestBase
                                {
                                    public abstract int this[int index] { get; }

                                    public abstract void Process(int item);
                                }

                                public class TestClass : TestBase
                                {
                                    public override int this[int i] => i;

                                    public override void Process(int p)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for explicit interface implementation parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForExplicitInterfaceImplementationParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public interface ITestHandler
                                {
                                    int this[int index] { get; }

                                    void Process(int item);
                                }

                                public class TestClass : ITestHandler
                                {
                                    int ITestHandler.this[int i] => i;

                                    void ITestHandler.Process(int p)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecordPrimaryConstructorParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record TestRecord(int x);
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for parameter kinds handled by other rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOtherParameterKinds()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        static int Local(int l) => l;
                                        Func<int, int> lambda = p => p;
                                        Func<int, int> anonymousMethod = delegate(int a) { return a; };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}