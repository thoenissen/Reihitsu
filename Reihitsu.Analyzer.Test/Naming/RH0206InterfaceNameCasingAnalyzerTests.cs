using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0206InterfaceNameCasingAnalyzer"/> and <see cref="RH0206InterfaceNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0206InterfaceNameCasingAnalyzerTests : AnalyzerTestsBase<RH0206InterfaceNameCasingAnalyzer, RH0206InterfaceNameCasingCodeFixProvider>
{
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
                                    public interface {|#0:itestInterface|}
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

        await Verify(testCode, fixedCode, Diagnostics(RH0206InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0206MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0206InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0206MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0206InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0206MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0206InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0206MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0206InterfaceNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0206MessageFormat));
    }
}