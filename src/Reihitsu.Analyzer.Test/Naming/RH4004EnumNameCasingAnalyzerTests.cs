using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4004EnumNameCasingAnalyzer"/> and <see cref="RH4004EnumNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4004EnumNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4004EnumNameCasingAnalyzer, RH4004EnumNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    [Requirement("RH4004-002")]
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
                                    /// Test enum
                                    /// </summary>
                                    public enum {|#0:testEnum|}
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
                                     /// Test enum
                                     /// </summary>
                                     public enum TestEnum
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4004EnumNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4004MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase enum name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseEnum()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public enum Direction
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an internal enum with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    [Requirement("RH4004-002")]
    public async Task VerifyDiagnosticsForInternalEnumWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    internal enum {|#0:statusCode|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     internal enum StatusCode
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4004EnumNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4004MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for an internal PascalCase enum
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    public async Task VerifyNoDiagnosticsForInternalEnum()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    internal enum StatusCode
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a Flags enum with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    [Requirement("RH4004-002")]
    public async Task VerifyDiagnosticsForFlagsEnumWrongCasing()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    [Flags]
                                    public enum {|#0:accessMode|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     [Flags]
                                     public enum AccessMode
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4004EnumNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4004MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an enum nested in a class with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    [Requirement("RH4004-002")]
    public async Task VerifyDiagnosticsForNestedEnumWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class OuterClass
                                    {
                                        public enum {|#0:errorKind|}
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class OuterClass
                                     {
                                         public enum ErrorKind
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4004EnumNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4004MessageFormat));
    }

    /// <summary>
    /// Verifies that a flags enum and a nested internal enum are checked and reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-001")]
    public async Task EnumsAreCheckedWhateverTheirAccessibilityAttributesOrNesting()
    {
        const string testCode = """
                                using System;

                                public enum Direction
                                {
                                }

                                [Flags]
                                public enum {|RH4004:accessMode|}
                                {
                                }

                                public class C
                                {
                                    internal enum {|RH4004:errorKind|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames the enum to PascalCase together with its use as a field type and in a member access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4004-002")]
    public async Task FixRenamesEnumAndEveryReference()
    {
        const string testCode = """
                                public enum {|RH4004:orderState|}
                                {
                                    Open
                                }

                                public class C
                                {
                                    public orderState State = orderState.Open;
                                }
                                """;

        const string fixedCode = """
                                 public enum OrderState
                                 {
                                     Open
                                 }

                                 public class C
                                 {
                                     public OrderState State = OrderState.Open;
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
                                    public enum {|#0:orderState|}
                                    {
                                        Created
                                    }

                                    public enum {|#1:paymentState|}
                                    {
                                        Pending
                                    }

                                    public class StateHolder
                                    {
                                        public orderState Order { get; set; }

                                        public paymentState Payment { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public enum OrderState
                                     {
                                         Created
                                     }

                                     public enum PaymentState
                                     {
                                         Pending
                                     }

                                     public class StateHolder
                                     {
                                         public OrderState Order { get; set; }

                                         public PaymentState Payment { get; set; }
                                     }
                                 }
                                 """;

        // Both enums are referenced by the same class, so the batch rewrites two declarations and two
        // property types
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4004EnumNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4004MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}