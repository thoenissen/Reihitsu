using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4005InterfaceNameCasingAnalyzer"/> and <see cref="RH4005InterfaceNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4005InterfaceNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4005InterfaceNameCasingAnalyzer, RH4005InterfaceNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
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
                                    /// Test interface
                                    /// </summary>
                                    public interface {|#0:iTestInterface|}
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
                                     /// Test interface
                                     /// </summary>
                                     public interface ITestInterface
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a correctly cased interface name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    public async Task VerifyNoDiagnosticsForCorrectInterfaceName()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface IDocumentReader
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an interface whose name has a lowercase 'i' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForLowercaseIPrefixInterface()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:iDocumentReader|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IDocumentReader
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an interface whose name is PascalCase but missing the required 'I' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForMissingIPrefixInterface()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:DocumentReader|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IDocumentReader
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a generic interface whose name is PascalCase but missing the required 'I' prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForGenericInterfaceMissingIPrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:Repository|}<T>
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IRepository<T>
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase generic interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    public async Task VerifyNoDiagnosticsForGenericInterface()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface IRepository<T>
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a generic interface with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForGenericInterfaceWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:iRepository|}<T>
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IRepository<T>
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a lowercase word that merely starts with the letter 'i' (no real 'I' prefix)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForLowercaseWordStartingWithI()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:index|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IIndex
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for another lowercase word that merely starts with the letter 'i' (no real 'I' prefix)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForLowercaseWordStartingWithIWithMultipleLetters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:important|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IImportant
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a PascalCase word that starts with 'I' followed by a lowercase letter (no real 'I' prefix)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForPascalCaseWordStartingWithI()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:Index|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IIndex
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for the PascalCase word 'Item' that starts with 'I' followed by a lowercase letter (no real 'I' prefix)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    [Requirement("RH4005-002")]
    public async Task VerifyDiagnosticsForPascalCaseWordItem()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface {|#0:Item|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IItem
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat));
    }

    /// <summary>
    /// Verifies that an I followed by an uppercase letter and a PascalCase name is accepted, and that a lowercase prefix, a missing prefix, a word starting with I, a separator, and a bare I are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-001")]
    public async Task InterfaceNamesAreAcceptedOnlyAsIFollowedByPascalCase()
    {
        const string testCode = """
                                public interface IDocumentReader
                                {
                                }

                                public interface IHTTPClient
                                {
                                }

                                public interface IRepository<T>
                                {
                                }

                                public interface {|RH4005:iDocumentWriter|}
                                {
                                }

                                public interface {|RH4005:DocumentStore|}
                                {
                                }

                                public interface {|RH4005:Index|}
                                {
                                }

                                public interface {|RH4005:IOrder_Line|}
                                {
                                }

                                public interface {|RH4005:I|}
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix corrects the case of an existing prefix, adds a missing prefix in front of the whole name, keeps a rest that is already PascalCase, and converts any other rest
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4005-002")]
    public async Task FixKeepsExistingPrefixAddsMissingPrefixAndKeepsPascalCaseRest()
    {
        const string testCode = """
                                public interface {|RH4005:iDocumentReader|}
                                {
                                }

                                public interface {|RH4005:Repository|}<T>
                                {
                                }

                                public interface {|RH4005:index|}
                                {
                                }

                                public interface {|RH4005:Item|}
                                {
                                }

                                public interface {|RH4005:IOrder_Line|}
                                {
                                }

                                public interface {|RH4005:URL|}
                                {
                                }

                                public interface {|RH4005:Document_STORE|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public interface IDocumentReader
                                 {
                                 }

                                 public interface IRepository<T>
                                 {
                                 }

                                 public interface IIndex
                                 {
                                 }

                                 public interface IItem
                                 {
                                 }

                                 public interface IOrderLine
                                 {
                                 }

                                 public interface IURL
                                 {
                                 }

                                 public interface IDocumentStore
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
                                    public interface {|#0:orderService|}
                                    {
                                        void Execute();
                                    }

                                    public interface {|#1:iPaymentGateway|} : orderService
                                    {
                                        int Amount { get; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IOrderService
                                     {
                                         void Execute();
                                     }

                                     public interface IPaymentGateway : IOrderService
                                     {
                                         int Amount { get; }
                                     }
                                 }
                                 """;

        // The second interface inherits the first, so the base list reference is renamed together with both
        // declarations. The two names take the different arms of the prefix transform: "orderService" gains
        // the "I" prefix, "iPaymentGateway" only has its existing prefix corrected
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4005InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4005MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}