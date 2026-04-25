using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0209MethodNameCasingAnalyzer"/> and <see cref="RH0209MethodNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0209MethodNameCasingAnalyzerTests : AnalyzerTestsBase<RH0209MethodNameCasingAnalyzer, RH0209MethodNameCasingCodeFixProvider>
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

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class TestClass
                                    {
                                        /// <summary>
                                        /// Test method
                                        /// </summary>
                                        public void {|#0:testmethod|}()
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestClass
                                     {
                                         /// <summary>
                                         /// Test method
                                         /// </summary>
                                         public void Testmethod()
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0209MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0209MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase method name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseMethod()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void ProcessData()
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a static method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForStaticMethodWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public static void {|#0:calculateTotal|}()
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public static void CalculateTotal()
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0209MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0209MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an async method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForAsyncMethodWrongCasing()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public async Task {|#0:loadDataAsync|}()
                                        {
                                            await Task.CompletedTask;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public async Task LoadDataAsync()
                                         {
                                             await Task.CompletedTask;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0209MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0209MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase async method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForAsyncMethod()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public async Task LoadDataAsync()
                                        {
                                            await Task.CompletedTask;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an interface method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForInterfaceMethodWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface IProcessor
                                    {
                                        void {|#0:execute|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IProcessor
                                     {
                                         void Execute();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0209MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0209MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an abstract method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForAbstractMethodWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public abstract class AbstractBase
                                    {
                                        public abstract void {|#0:compute|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public abstract class AbstractBase
                                     {
                                         public abstract void Compute();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0209MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0209MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a generic method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForGenericMethodWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public T {|#0:createInstance|}<T>()
                                            where T : new()
                                        {
                                            return new T();
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public T CreateInstance<T>()
                                             where T : new()
                                         {
                                             return new T();
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0209MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0209MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase override method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverrideMethod()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class BaseClass
                                    {
                                        public virtual void ProcessData()
                                        {
                                        }
                                    }

                                    public class DerivedClass : BaseClass
                                    {
                                        public override void ProcessData()
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}