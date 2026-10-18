using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4003StructNameCasingAnalyzer"/> and <see cref="RH4003StructNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4003StructNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4003StructNameCasingAnalyzer, RH4003StructNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    [Requirement("RH4003-002")]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test struct
                                    /// </summary>
                                    public struct {|#0:testStruct|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 using System.Text;
                                 using System.Threading.Tasks;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test struct
                                     /// </summary>
                                     public struct TestStruct
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4003StructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4003MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase struct name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseStruct()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public struct Coordinate
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a readonly struct with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    [Requirement("RH4003-002")]
    public async Task VerifyDiagnosticsForReadonlyStructWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public readonly struct {|#0:immutablePoint|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public readonly struct ImmutablePoint
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4003StructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4003MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase readonly struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    public async Task VerifyNoDiagnosticsForReadonlyStruct()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public readonly struct ImmutablePoint
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a generic struct with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    [Requirement("RH4003-002")]
    public async Task VerifyDiagnosticsForGenericStructWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public struct {|#0:valuePair|}<T1, T2>
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public struct ValuePair<T1, T2>
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4003StructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4003MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase generic struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    public async Task VerifyNoDiagnosticsForGenericStruct()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public struct ValuePair<T1, T2>
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a ref struct with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    [Requirement("RH4003-002")]
    public async Task VerifyDiagnosticsForRefStructWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public ref struct {|#0:bufferView|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public ref struct BufferView
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4003StructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4003MessageFormat));
    }

    /// <summary>
    /// Verifies that readonly, ref, and generic structs are checked and reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-001")]
    public async Task StructsAreCheckedIncludingReadonlyRefAndGenericStructs()
    {
        const string testCode = """
                                public struct Coordinate
                                {
                                }

                                public readonly struct {|RH4003:immutablePoint|}
                                {
                                }

                                public ref struct {|RH4003:bufferView|}
                                {
                                }

                                public struct {|RH4003:valuePair|}<T1, T2>
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames each struct to PascalCase together with its use as a field type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4003-002")]
    public async Task FixRenamesStructAndEveryReference()
    {
        const string testCode = """
                                public struct {|RH4003:pointValue|}
                                {
                                    public rangeValue Range;
                                }

                                public struct {|RH4003:rangeValue|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public struct PointValue
                                 {
                                     public RangeValue Range;
                                 }

                                 public struct RangeValue
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public struct {|#0:pointValue|}
                                    {
                                        public rangeValue Range;
                                    }

                                    public struct {|#1:rangeValue|}
                                    {
                                        public int Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public struct PointValue
                                     {
                                         public RangeValue Range;
                                     }

                                     public struct RangeValue
                                     {
                                         public int Length;
                                     }
                                 }
                                 """;

        // The first struct declares a field of the second, so one rename has to reach the other declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4003StructNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4003MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}