using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0210LocalFunctionNameCasingAnalyzer"/> and <see cref="RH0210LocalFunctionNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0210LocalFunctionNameCasingAnalyzerTests : AnalyzerTestsBase<RH0210LocalFunctionNameCasingAnalyzer, RH0210LocalFunctionNameCasingCodeFixProvider>
{
    #region Members

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
                                        public void Method()
                                        {
                                            void {|#0:testLocalFunction|}()
                                            {
                                            }
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
                                         public void Method()
                                         {
                                             void TestLocalFunction()
                                             {
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0210LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0210MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase local function name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseLocalFunction()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            void ProcessItem()
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an async local function with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForAsyncLocalFunctionWrongCasing()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            async Task {|#0:loadItem|}()
                                            {
                                                await Task.CompletedTask;
                                            }
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
                                         public void Method()
                                         {
                                             async Task LoadItem()
                                             {
                                                 await Task.CompletedTask;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0210LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0210MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase async local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForAsyncLocalFunction()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            async Task LoadItem()
                                            {
                                                await Task.CompletedTask;
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a static local function with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForStaticLocalFunctionWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            static void {|#0:processItem|}()
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public void Method()
                                         {
                                             static void ProcessItem()
                                             {
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0210LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0210MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an inner local function with wrong casing when nested inside another local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForNestedLocalFunctionWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            void OuterLocal()
                                            {
                                                void {|#0:innerLocal|}()
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public void Method()
                                         {
                                             void OuterLocal()
                                             {
                                                 void InnerLocal()
                                                 {
                                                 }
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0210LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0210MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a generic local function with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForGenericLocalFunctionWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public void Method()
                                        {
                                            T {|#0:createValue|}<T>()
                                                where T : new()
                                            {
                                                return new T();
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public void Method()
                                         {
                                             T CreateValue<T>()
                                                 where T : new()
                                             {
                                                 return new T();
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0210LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0210MessageFormat));
    }

    #endregion // Members
}