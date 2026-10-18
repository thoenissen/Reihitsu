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
    [Requirement("RH4122-001")]
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
    [Requirement("RH4122-001")]
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
    [Requirement("RH4122-001")]
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
    [Requirement("SHARED-SINGLE-001")]
    [Requirement("RH4122-001")]
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
    [Requirement("RH4122-002")]
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
    [Requirement("RH4122-002")]
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
    [Requirement("RH4122-003")]
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
    [Requirement("RH4122-003")]
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

    /// <summary>
    /// Verifies lowercase, uppercase, verbatim, and non-ASCII single letters count as single-letter names, while an underscore and a two-letter name do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-001")]
    [Requirement("RH4122-001")]
    public async Task SingleLetterNameIsOneCharacterOtherThanUnderscore()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int {|RH4122:a|}, int {|RH4122:X|}, int {|RH4122:@b|}, int {|RH4122:é|}, int _, int ab)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a name of one letter-number character counts as a single-letter name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-SINGLE-001")]
    public async Task SingleLetterNumberCountsAsSingleLetterName()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int {|RH4122:Ⅻ|})
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter parameters of delegates, interface members, class and struct primary constructors, constructors, indexers, implicit interface implementations, operators, and conversion operators are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4122-001")]
    public async Task ParametersOfEveryMemberKindAndImplicitImplementationsAreChecked()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                public delegate void D(int {|RH4122:a|});

                                public interface I
                                {
                                    void M(int {|RH4122:b|});
                                }

                                public class C(int {|RH4122:c|}) : IComparer<int>
                                {
                                    public C(int {|RH4122:d|}, int count)
                                        : this(d + count)
                                    {
                                    }

                                    public int this[int {|RH4122:i|}] => i;

                                    public int Compare(int {|RH4122:x|}, int {|RH4122:y|}) => 0;

                                    public static C operator +(C {|RH4122:l|}, C right) => l;

                                    public static implicit operator int(C {|RH4122:v|}) => 0;
                                }

                                public struct S(int {|RH4122:s|});
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial method reports its own single-letter parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4122-001")]
    public async Task PartialMethodParameterIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class P
                                {
                                    public partial void M(int {|RH4122:a|});
                                }

                                public partial class P
                                {
                                    public partial void M(int {|RH4122:a|})
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies parameters of an explicit interface implementation, an overriding method, and an overriding indexer are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4122-002")]
    public async Task OverridesAndExplicitInterfaceImplementationsAreNotChecked()
    {
        const string testCode = """
                                public interface I
                                {
                                    void M(int value);
                                }

                                public abstract class B
                                {
                                    public abstract void N(int value);

                                    public abstract int this[int index] { get; }
                                }

                                public class C : B, I
                                {
                                    void I.M(int v)
                                    {
                                    }

                                    public override void N(int v)
                                    {
                                    }

                                    public override int this[int i] => i;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies parameters of record primary constructors, local functions, lambdas, and anonymous methods are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4122-003")]
    public async Task RecordLocalFunctionLambdaAndAnonymousMethodParametersAreNotChecked()
    {
        const string testCode = """
                                using System;

                                public record Point(int X, int Y);

                                public record class Line(int A);

                                public record struct Size(int W, int H);

                                public class C
                                {
                                    public void M()
                                    {
                                        int Local(int a) => a;
                                        Func<int, int> f = b => b;
                                        Action<int> g = delegate (int c) { };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter primary constructor parameters of a readonly record struct are not reported, while those of a readonly struct are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4122-003")]
    public async Task ReadonlyRecordStructPrimaryConstructorParametersAreNotChecked()
    {
        const string testCode = """
                                public readonly record struct Point(int X, int y);

                                public readonly record struct Pair(int a, int b);

                                public readonly struct Size(int {|RH4122:w|});
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}