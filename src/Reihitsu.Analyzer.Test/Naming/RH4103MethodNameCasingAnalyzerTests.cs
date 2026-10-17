using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4103MethodNameCasingAnalyzer"/> and <see cref="RH4103MethodNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4103MethodNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4103MethodNameCasingAnalyzer, RH4103MethodNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
    [Requirement("RH4103-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase method name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
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
    [Requirement("RH4103-001")]
    [Requirement("RH4103-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an async method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
    [Requirement("RH4103-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase async method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
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
    [Requirement("RH4103-001")]
    [Requirement("RH4103-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an abstract method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
    [Requirement("RH4103-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a generic method with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
    [Requirement("RH4103-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase override method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
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

    /// <summary>
    /// Verifying no diagnostics for an explicit interface implementation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-002")]
    public async Task VerifyNoDiagnosticsForExplicitInterfaceImplementation()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    void {|#0:process|}();
                                }

                                public class Test : IProcessor
                                {
                                    void IProcessor.process()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat));
    }

    /// <summary>
    /// Verifies that interface, static, async, abstract, generic, and virtual methods, and overrides and implicit implementations of source methods are reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-001")]
    public async Task EveryKindOfMethodWhoseOriginalDefinitionIsInSourceIsChecked()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                public interface IProcessor
                                {
                                    void {|RH4103:execute|}();
                                }

                                public abstract class C
                                {
                                    public void ProcessData()
                                    {
                                    }

                                    public static void {|RH4103:calculateTotal|}()
                                    {
                                    }

                                    public async Task {|RH4103:loadDataAsync|}()
                                    {
                                        await Task.CompletedTask;
                                    }

                                    public abstract void {|RH4103:compute|}();

                                    public T {|RH4103:createInstance|}<T>()
                                        where T : new()
                                    {
                                        return new T();
                                    }

                                    public virtual void {|RH4103:run|}()
                                    {
                                    }
                                }

                                public class Derived : C, IProcessor
                                {
                                    public override void {|RH4103:compute|}()
                                    {
                                    }

                                    public override void {|RH4103:run|}()
                                    {
                                    }

                                    public void {|RH4103:execute|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an explicit interface method implementation is not reported, and that the fix for the interface method renames it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-002")]
    public async Task ExplicitInterfaceMethodImplementationIsNotReportedAndIsRenamedWithTheInterfaceMethod()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    void {|RH4103:process|}();
                                }

                                public class Processor : IProcessor
                                {
                                    void IProcessor.process()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public interface IProcessor
                                 {
                                     void Process();
                                 }

                                 public class Processor : IProcessor
                                 {
                                     void IProcessor.Process()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames each method to PascalCase together with its call, splits at the separator, and treats a name without separator or uppercase letter as one word
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-003")]
    public async Task FixRenamesMethodAndEveryCallTreatingUnseparatedLowercaseNameAsOneWord()
    {
        const string testCode = """
                                public class C
                                {
                                    public void {|RH4103:testmethod|}()
                                    {
                                    }

                                    public void {|RH4103:run_all|}()
                                    {
                                        testmethod();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public void Testmethod()
                                     {
                                     }

                                     public void RunAll()
                                     {
                                         Testmethod();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that renaming a source interface method or virtual method also renames its implicit implementation or override
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4103-003")]
    public async Task FixRenamesOverridesAndImplementationsWithTheirBase()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    void {|RH4103:execute|}();
                                }

                                public class Processor : IProcessor
                                {
                                    public void {|RH4103:execute|}()
                                    {
                                    }
                                }

                                public class Base
                                {
                                    public virtual void {|RH4103:run|}()
                                    {
                                    }
                                }

                                public class Derived : Base
                                {
                                    public override void {|RH4103:run|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public interface IProcessor
                                 {
                                     void Execute();
                                 }

                                 public class Processor : IProcessor
                                 {
                                     public void Execute()
                                     {
                                     }
                                 }

                                 public class Base
                                 {
                                     public virtual void Run()
                                     {
                                     }
                                 }

                                 public class Derived : Base
                                 {
                                     public override void Run()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames each method through the renamer, so the declarations and the call in the other method change together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("RH4103-003")]
    public async Task FixRenamesTheDeclarationAndEveryReference()
    {
        const string testCode = """
                                public class Calculator
                                {
                                    public int {|RH4103:computeTotal|}(int value)
                                    {
                                        return computeFactor(value) * 2;
                                    }

                                    private int {|RH4103:computeFactor|}(int value)
                                    {
                                        return value + 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Calculator
                                 {
                                     public int ComputeTotal(int value)
                                     {
                                         return ComputeFactor(value) * 2;
                                     }

                                     private int ComputeFactor(int value)
                                     {
                                         return value + 1;
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
                                    public class Calculator
                                    {
                                        public int {|#0:computeTotal|}(int value)
                                        {
                                            return computeFactor(value) * 2;
                                        }

                                        private int {|#1:computeFactor|}(int value)
                                        {
                                            return value + 1;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Calculator
                                     {
                                         public int ComputeTotal(int value)
                                         {
                                             return ComputeFactor(value) * 2;
                                         }

                                         private int ComputeFactor(int value)
                                         {
                                             return value + 1;
                                         }
                                     }
                                 }
                                 """;

        // The first reported method calls the second, so the batch has to rewrite the invocation together
        // with both declarations
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4103MethodNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4103MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}