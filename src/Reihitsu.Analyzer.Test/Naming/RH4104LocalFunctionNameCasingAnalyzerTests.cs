using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4104LocalFunctionNameCasingAnalyzer"/> and <see cref="RH4104LocalFunctionNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4104LocalFunctionNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4104LocalFunctionNameCasingAnalyzer, RH4104LocalFunctionNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-001")]
    [Requirement("RH4104-002")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4104LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4104MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase local function name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-001")]
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
    [Requirement("RH4104-001")]
    [Requirement("RH4104-002")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4104LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4104MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase async local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-001")]
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
    [Requirement("RH4104-001")]
    [Requirement("RH4104-002")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4104LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4104MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an inner local function with wrong casing when nested inside another local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-001")]
    [Requirement("RH4104-002")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4104LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4104MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a generic local function with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-001")]
    [Requirement("RH4104-002")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4104LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4104MessageFormat));
    }

    /// <summary>
    /// Verifies that async, static, generic, and nested local functions are reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-001")]
    public async Task LocalFunctionsAreCheckedIncludingAsyncStaticGenericAndNestedOnes()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public class C
                                {
                                    public void M()
                                    {
                                        void ProcessItem()
                                        {
                                        }

                                        async Task {|RH4104:loadItem|}()
                                        {
                                            await Task.CompletedTask;
                                        }

                                        static void {|RH4104:processAll|}()
                                        {
                                            void {|RH4104:innerLocal|}()
                                            {
                                            }
                                        }

                                        T {|RH4104:createValue|}<T>()
                                            where T : new()
                                        {
                                            return new T();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames an outer and a nested local function to PascalCase together with their calls
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4104-002")]
    public async Task FixRenamesNestedLocalFunctionsAndEveryCall()
    {
        const string testCode = """
                                public class Runner
                                {
                                    public int Run(int value)
                                    {
                                        int {|RH4104:outerHelper|}(int input)
                                        {
                                            int {|RH4104:innerHelper|}(int nested)
                                            {
                                                return nested + 1;
                                            }

                                            return innerHelper(input);
                                        }

                                        return outerHelper(value);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Runner
                                 {
                                     public int Run(int value)
                                     {
                                         int OuterHelper(int input)
                                         {
                                             int InnerHelper(int nested)
                                             {
                                                 return nested + 1;
                                             }

                                             return InnerHelper(input);
                                         }

                                         return OuterHelper(value);
                                     }
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
                                    public class Runner
                                    {
                                        public int Run(int value)
                                        {
                                            int {|#0:outerHelper|}(int input)
                                            {
                                                int {|#1:innerHelper|}(int nested)
                                                {
                                                    return nested + 1;
                                                }

                                                return innerHelper(input);
                                            }

                                            return outerHelper(value);
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Runner
                                     {
                                         public int Run(int value)
                                         {
                                             int OuterHelper(int input)
                                             {
                                                 int InnerHelper(int nested)
                                                 {
                                                     return nested + 1;
                                                 }

                                                 return InnerHelper(input);
                                             }

                                             return OuterHelper(value);
                                         }
                                     }
                                 }
                                 """;

        // The second local function is declared inside the first, so the two renames happen in nested scopes
        // and each has to reach its own invocation
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4104LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4104MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}