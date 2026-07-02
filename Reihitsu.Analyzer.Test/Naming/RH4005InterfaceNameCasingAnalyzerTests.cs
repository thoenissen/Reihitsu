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
public class RH4005InterfaceNameCasingAnalyzerTests : AnalyzerTestsBase<RH4005InterfaceNameCasingAnalyzer, RH4005InterfaceNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}