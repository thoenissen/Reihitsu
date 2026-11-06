using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
    #region Constants

    /// <summary>
    /// Source of the referenced assembly that declares members with names that are not in PascalCase
    /// </summary>
    private const string MetaAssemblySource = """
                                              namespace Meta
                                              {
                                                  public abstract class MetaBase
                                                  {
                                                      public abstract void process();
                                                  }

                                                  public interface IMeta
                                                  {
                                                      void process();
                                                  }

                                                  public class MetaImpl
                                                  {
                                                      public void process()
                                                      {
                                                      }
                                                  }
                                              }
                                              """;

    /// <summary>
    /// Source of a base type whose virtual method an excluded <c>#if</c> branch references
    /// </summary>
    private const string BaseWithExcludedReferenceSource = """
                                                           public class Base
                                                           {
                                                               public virtual void {|RH4103:process|}()
                                                               {
                                                               }

                                                               public void Run()
                                                               {
                                                           #if NEVER
                                                                   process();
                                                           #endif
                                                               }
                                                           }
                                                           """;

    /// <summary>
    /// Source of a file-local type that overrides the virtual method of <see cref="BaseWithExcludedReferenceSource"/>
    /// </summary>
    private const string FileLocalLeafSource = """
                                               file class Leaf : Base
                                               {
                                                   public override void {|RH4103:process|}()
                                                   {
                                                   }
                                               }
                                               """;

    /// <summary>
    /// Source of an extension method that a query expression would use as its <c>Select</c> clause after the rename
    /// </summary>
    private const string SelectExtensionSource = """
                                                 using System;
                                                 using System.Collections.Generic;

                                                 namespace Ext;

                                                 public static class MyLinq
                                                 {
                                                     public static IEnumerable<TResult> {|RH4103:select|}<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
                                                     {
                                                         foreach (var item in source)
                                                         {
                                                             yield return selector(item);
                                                         }
                                                     }
                                                 }
                                                 """;

    #endregion // Constants

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

    /// <summary>
    /// Verifies that a method bound to a native entry point by its own name keeps its diagnostic but gets no fix, while a method whose literal entry point stays is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsWithheldForANativeEntryPointTakenFromTheMethodName()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                public static class C
                                {
                                    [DllImport("libc")]
                                    public static extern int {|RH4103:getpid|}();

                                    [DllImport("libc", EntryPoint = "getppid")]
                                    public static extern int {|RH4103:getppid|}();
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 public static class C
                                 {
                                     [DllImport("libc")]
                                     public static extern int {|RH4103:getpid|}();

                                     [DllImport("libc", EntryPoint = "getppid")]
                                     public static extern int Getppid();
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a method declared with <c>DllImport</c> and no entry point keeps its diagnostic but gets no fix, also when it is called
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedForADllImportMethodWithoutEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                internal static class Native
                                {
                                    [DllImport("libc")]
                                    internal static extern int {|RH4103:getpid|}();

                                    internal static int Current()
                                    {
                                        return getpid();
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method whose entry point is <c>nameof</c> of the method itself keeps its diagnostic but gets no fix, because the rename would change the entry point
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedForADllImportMethodWhoseEntryPointIsNameofTheMethod()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                internal static class Native
                                {
                                    [DllImport("libc", EntryPoint = nameof(getpid))]
                                    internal static extern int {|RH4103:getpid|}();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method declared with <c>DllImport</c> and a literal entry point is renamed, because the native function it calls stays the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixRenamesADllImportMethodWithALiteralEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                internal static class Native
                                {
                                    [DllImport("libc", EntryPoint = "getpid")]
                                    internal static extern int {|RH4103:getpid|}();

                                    internal static int Current()
                                    {
                                        return getpid();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 internal static class Native
                                 {
                                     [DllImport("libc", EntryPoint = "getpid")]
                                     internal static extern int Getpid();

                                     internal static int Current()
                                     {
                                         return Getpid();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a <c>partial</c> method declared with <c>LibraryImport</c> and no entry point keeps its diagnostic but gets no fix. No source generator runs in the test, so the missing implementation is reported before and after
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedForALibraryImportMethodWithoutEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                internal static partial class Native
                                {
                                    [LibraryImport("libc")]
                                    internal static partial int {|CS8795:{|RH4103:getpid|}|}();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a <c>partial</c> method declared with <c>LibraryImport</c> and a literal entry point is renamed, because the native function it calls stays the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixRenamesALibraryImportMethodWithALiteralEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                internal static partial class Native
                                {
                                    [LibraryImport("libc", EntryPoint = "getpid")]
                                    internal static partial int {|CS8795:{|RH4103:getpid|}|}();
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 internal static partial class Native
                                 {
                                     [LibraryImport("libc", EntryPoint = "getpid")]
                                     internal static partial int {|CS8795:Getpid|}();
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an override whose rename would cascade to an override in another source type of a member declared in a referenced assembly keeps its diagnostic but gets no fix, because that other type would no longer compile
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToAnOverrideOfAReferencedAssemblyMember()
    {
        const string testCode = """
                                using Meta;

                                public class Middle : MetaBase
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }

                                public class Leaf : Middle
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", MetaAssemblySource);

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a direct override of a member declared in a referenced assembly keeps its diagnostic but gets no fix, because the renamed override would override nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedForADirectOverrideOfAReferencedAssemblyMember()
    {
        const string testCode = """
                                using Meta;

                                public class Middle : MetaBase
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", MetaAssemblySource);

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that an override chain declared entirely in source is renamed together, so the fix stays offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAnOverrideChainDeclaredInSourceTogether()
    {
        const string testCode = """
                                public abstract class SourceBase
                                {
                                    public abstract void {|RH4103:process|}();
                                }

                                public class Middle : SourceBase
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }

                                public class Leaf : Middle
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public abstract class SourceBase
                                 {
                                     public abstract void Process();
                                 }

                                 public class Middle : SourceBase
                                 {
                                     public override void Process()
                                     {
                                     }
                                 }

                                 public class Leaf : Middle
                                 {
                                     public override void Process()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an override whose rename would cascade to a source base type's implementation of an interface declared in a referenced assembly keeps its diagnostic but gets no fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToAnImplementationOfAReferencedAssemblyInterface()
    {
        const string testCode = """
                                using Meta;

                                public class Middle : IMeta
                                {
                                    public virtual void {|RH4103:process|}()
                                    {
                                    }
                                }

                                public class Leaf : Middle
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", MetaAssemblySource);

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a method that a derived source type uses to implement an interface declared in a referenced assembly keeps its diagnostic but gets no fix, because the derived type would no longer implement the interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenADerivedTypeImplementsAReferencedAssemblyInterfaceWithTheMethod()
    {
        const string testCode = """
                                using Meta;

                                public class A
                                {
                                    public void {|RH4103:process|}()
                                    {
                                    }
                                }

                                public class B : A, IMeta
                                {
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", MetaAssemblySource);

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a method that a derived type uses to implement an interface declared in source is renamed together with the interface member, so the fix stays offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAMethodThatADerivedTypeUsesToImplementASourceInterface()
    {
        const string testCode = """
                                public interface ISource
                                {
                                    void {|RH4103:process|}();
                                }

                                public class A
                                {
                                    public void {|RH4103:process|}()
                                    {
                                    }
                                }

                                public class B : A, ISource
                                {
                                }
                                """;

        const string fixedCode = """
                                 public interface ISource
                                 {
                                     void Process();
                                 }

                                 public class A
                                 {
                                     public void Process()
                                     {
                                     }
                                 }

                                 public class B : A, ISource
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an interface method declared in source, which a type implements with a method inherited from a referenced assembly, keeps its diagnostic but gets no fix, because the implementing type would no longer implement the interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenASourceInterfaceMethodIsImplementedByAReferencedAssemblyMember()
    {
        const string testCode = """
                                using Meta;

                                public interface ISource
                                {
                                    void {|RH4103:process|}();
                                }

                                public class Impl : MetaImpl, ISource
                                {
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", MetaAssemblySource);

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that a static abstract interface method keeps its diagnostic but gets no fix when the rename would reach a <c>DllImport</c> method without entry point that implements it, because that method would then call another native function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationWithoutEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                public interface INative
                                {
                                    static abstract int {|RH4103:getpid|}();
                                }

                                public class Native : INative
                                {
                                    [DllImport("libc")]
                                    public static extern int {|RH4103:getpid|}();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a static abstract interface method keeps its diagnostic but gets no fix when the rename would reach a <c>DllImport</c> method whose entry point is <c>nameof</c> of itself, because the entry point would change
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationWhoseEntryPointIsNameofTheMethod()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                public interface INative
                                {
                                    static abstract int {|RH4103:getpid|}();
                                }

                                public class Native : INative
                                {
                                    [DllImport("libc", EntryPoint = nameof(getpid))]
                                    public static extern int {|RH4103:getpid|}();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a static abstract interface method keeps its diagnostic but gets no fix when the rename would reach a <c>LibraryImport</c> method without entry point that implements it. No source generator runs in the test, so the missing implementation is reported before and after
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToALibraryImportImplementationWithoutEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                public interface INative
                                {
                                    static abstract int {|RH4103:getpid|}();
                                }

                                public partial class Native : INative
                                {
                                    [LibraryImport("libc")]
                                    public static partial int {|CS8795:{|RH4103:getpid|}|}();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a static virtual interface method keeps its diagnostic but gets no fix when the rename would reach a <c>DllImport</c> method without entry point that implements it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameOfAStaticVirtualMethodCascadesToADllImportImplementation()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                public interface INative
                                {
                                    static virtual int {|RH4103:getpid|}()
                                    {
                                        return 0;
                                    }
                                }

                                public class Native : INative
                                {
                                    [DllImport("libc")]
                                    public static extern int {|RH4103:getpid|}();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a static abstract interface method keeps its diagnostic but gets no fix when the rename would reach a <c>DllImport</c> method without entry point that implements it in another project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToADllImportImplementationInADependentProject()
    {
        const string contractCode = """
                                    public interface INative
                                    {
                                        static abstract int {|RH4103:getpid|}();
                                    }
                                    """;

        const string testCode = """
                                using System.Runtime.InteropServices;

                                public class Native : INative
                                {
                                    [DllImport("libc")]
                                    public static extern int {|RH4103:getpid|}();
                                }
                                """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Contracts"].Sources.Add(("/Contracts/INative.cs", contractCode));
                         test.TestState.AdditionalProjectReferences.Add("Contracts");
                     });
    }

    /// <summary>
    /// Verifies that a static abstract interface method is renamed together with a <c>DllImport</c> method that implements it with a literal entry point, because the native function it calls stays the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixRenamesAStaticAbstractMethodTogetherWithADllImportImplementationWithALiteralEntryPoint()
    {
        const string testCode = """
                                using System.Runtime.InteropServices;

                                public interface INative
                                {
                                    static abstract int {|RH4103:getpid|}();
                                }

                                public class Native : INative
                                {
                                    [DllImport("libc", EntryPoint = "getpid")]
                                    public static extern int {|RH4103:getpid|}();
                                }
                                """;

        const string fixedCode = """
                                 using System.Runtime.InteropServices;

                                 public interface INative
                                 {
                                     static abstract int Getpid();
                                 }

                                 public class Native : INative
                                 {
                                     [DllImport("libc", EntryPoint = "getpid")]
                                     public static extern int Getpid();
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an override in a file-local type keeps its diagnostic but gets no fix when the rename would reach the overridden method, whose document references it in an excluded <c>#if</c> branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToAMethodReferencedInExcludedCode()
    {
        await Verify(BaseWithExcludedReferenceSource,
                     BaseWithExcludedReferenceSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", FileLocalLeafSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", FileLocalLeafSource));
                     });
    }

    /// <summary>
    /// Verifies that an override in a file-local type keeps its diagnostic but gets no fix when the rename would reach the overridden method referenced in excluded code, also in documents with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToAMethodReferencedInExcludedCodeWithCarriageReturnLineFeed()
    {
        var baseCode = NormalizeToCarriageReturnLineFeed(BaseWithExcludedReferenceSource);
        var leafCode = NormalizeToCarriageReturnLineFeed(FileLocalLeafSource);

        await Verify(baseCode,
                     baseCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", leafCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", leafCode));
                     });
    }

    /// <summary>
    /// Verifies that an override in a private nested type keeps its diagnostic but gets no fix when the rename would reach the overridden method, whose document references it in an excluded <c>#if</c> branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesFromAPrivateNestedOverrideToAMethodReferencedInExcludedCode()
    {
        const string leafCode = """
                                public class Outer
                                {
                                    private class Leaf : Base
                                    {
                                        public override void {|RH4103:process|}()
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(BaseWithExcludedReferenceSource,
                     BaseWithExcludedReferenceSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", leafCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", leafCode));
                     });
    }

    /// <summary>
    /// Verifies that a method of a private nested type keeps its diagnostic but gets no fix when the rename would reach the interface method it implements, which another document references in an excluded <c>#if</c> branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesFromAPrivateNestedImplementationToAnInterfaceMethodReferencedInExcludedCode()
    {
        const string interfaceCode = """
                                     public interface IRunner
                                     {
                                         void {|RH4103:process|}();
                                     }

                                     public static class Use
                                     {
                                         public static void Run(IRunner runner)
                                         {
                                     #if NEVER
                                             runner.process();
                                     #endif
                                         }
                                     }
                                     """;

        const string implementationCode = """
                                          public class Outer
                                          {
                                              private class Impl : IRunner
                                              {
                                                  public void {|RH4103:process|}()
                                                  {
                                                  }
                                              }
                                          }
                                          """;

        await Verify(interfaceCode,
                     interfaceCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", implementationCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", implementationCode));
                     });
    }

    /// <summary>
    /// Verifies that an override in a file-local type keeps its diagnostic but gets no fix when an excluded <c>#if</c> branch of the overridden method's document declares the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesFromAFileLocalOverrideToATypeWhoseExcludedCodeDeclaresTheNewName()
    {
        const string baseCode = """
                                public class Base
                                {
                                    public virtual void {|RH4103:process|}()
                                    {
                                    }

                                #if NEVER
                                    public void Process()
                                    {
                                    }
                                #endif
                                }
                                """;

        await Verify(baseCode,
                     baseCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", FileLocalLeafSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", FileLocalLeafSource));
                     });
    }

    /// <summary>
    /// Verifies that an override keeps its diagnostic but gets no fix when the rename would reach the overridden method of a referenced project, whose document references it in an excluded <c>#if</c> branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesFromAnOverrideToAMethodOfAReferencedProjectReferencedInExcludedCode()
    {
        const string leafCode = """
                                public class Leaf : Base
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }
                                """;

        await Verify(leafCode,
                     leafCode,
                     test =>
                     {
                         test.TestState.AdditionalProjects["BaseProject"].Sources.Add(("/BaseProject/Base.cs", BaseWithExcludedReferenceSource));
                         test.TestState.AdditionalProjectReferences.Add("BaseProject");
                     });
    }

    /// <summary>
    /// Verifies that an override in a file-local type is renamed together with the overridden method when no excluded code names either name, so the fix stays offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    public async Task FixRenamesAFileLocalOverrideTogetherWithTheOverriddenMethodWhenNoExcludedCodeNamesIt()
    {
        const string baseCode = """
                                public class Base
                                {
                                    public virtual void {|RH4103:process|}()
                                    {
                                    }

                                    public void Run()
                                    {
                                #if NEVER
                                        Stop();
                                #else
                                        process();
                                #endif
                                    }

                                    public void Stop()
                                    {
                                    }
                                }
                                """;

        const string fixedBaseCode = """
                                     public class Base
                                     {
                                         public virtual void Process()
                                         {
                                         }

                                         public void Run()
                                         {
                                     #if NEVER
                                             Stop();
                                     #else
                                             Process();
                                     #endif
                                         }

                                         public void Stop()
                                         {
                                         }
                                     }
                                     """;

        const string fixedLeafCode = """
                                     file class Leaf : Base
                                     {
                                         public override void Process()
                                         {
                                         }
                                     }
                                     """;

        await Verify(baseCode,
                     fixedBaseCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", FileLocalLeafSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", fixedLeafCode));
                     });
    }

    /// <summary>
    /// Verifies that an override is renamed together with the overridden method of a referenced project when no excluded code names either name, so the fix stays offered across projects
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    public async Task FixRenamesAnOverrideTogetherWithTheOverriddenMethodOfAReferencedProject()
    {
        const string baseCode = """
                                public class Base
                                {
                                    public virtual void {|RH4103:process|}()
                                    {
                                    }

                                    public void Run()
                                    {
                                        process();
                                    }
                                }
                                """;

        const string fixedBaseCode = """
                                     public class Base
                                     {
                                         public virtual void Process()
                                         {
                                         }

                                         public void Run()
                                         {
                                             Process();
                                         }
                                     }
                                     """;

        const string leafCode = """
                                public class Leaf : Base
                                {
                                    public override void {|RH4103:process|}()
                                    {
                                    }
                                }
                                """;

        const string fixedLeafCode = """
                                     public class Leaf : Base
                                     {
                                         public override void Process()
                                         {
                                         }
                                     }
                                     """;

        await Verify(leafCode,
                     fixedLeafCode,
                     test =>
                     {
                         test.TestState.AdditionalProjects["BaseProject"].Sources.Add(("/BaseProject/Base.cs", baseCode));
                         test.TestState.AdditionalProjectReferences.Add("BaseProject");
                         test.FixedState.AdditionalProjects["BaseProject"].Sources.Add(("/BaseProject/Base.cs", fixedBaseCode));
                         test.FixedState.AdditionalProjectReferences.Add("BaseProject");
                     });
    }

    /// <summary>
    /// Verifies that a query method keeps its diagnostic but gets no fix when its new name would make a query expression of another, unchanged document ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameMakesAQueryOfAnotherDocumentAmbiguous()
    {
        const string queryCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 using Ext;

                                 public static class Use
                                 {
                                     public static IEnumerable<int> Run(List<int> values)
                                     {
                                         return from value in values select value * 2;
                                     }
                                 }
                                 """;

        await Verify(SelectExtensionSource,
                     SelectExtensionSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", queryCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", queryCode));
                     });
    }

    /// <summary>
    /// Verifies that a query method keeps its diagnostic but gets no fix when its new name would make a query expression of the same document ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameMakesAQueryOfTheSameDocumentAmbiguous()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                using Ext;

                                namespace Ext
                                {
                                    public static class MyLinq
                                    {
                                        public static IEnumerable<TResult> {|RH4103:select|}<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
                                        {
                                            foreach (var item in source)
                                            {
                                                yield return selector(item);
                                            }
                                        }
                                    }
                                }

                                public static class Use
                                {
                                    public static IEnumerable<int> Run(List<int> values)
                                    {
                                        return from value in values select value * 2;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a query method is renamed when a query expression of another document does not see it, so the new name makes nothing ambiguous and the fix stays offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAQueryMethodThatNoQueryOfAnotherDocumentSees()
    {
        const string queryCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public static class Use
                                 {
                                     public static IEnumerable<int> Run(List<int> values)
                                     {
                                         return from value in values select value * 2;
                                     }
                                 }
                                 """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 namespace Ext;

                                 public static class MyLinq
                                 {
                                     public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
                                     {
                                         foreach (var item in source)
                                         {
                                             yield return selector(item);
                                         }
                                     }
                                 }
                                 """;

        await Verify(SelectExtensionSource,
                     fixedCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", queryCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", queryCode));
                     });
    }

    /// <summary>
    /// Verifies that a query method keeps its diagnostic but gets no fix when its new name would make a query expression of a dependent project ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameMakesAQueryOfADependentProjectAmbiguous()
    {
        const string queryCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 using Ext;

                                 public static class Use
                                 {
                                     public static IEnumerable<int> Run(List<int> values)
                                     {
                                         return from value in values select value * 2;
                                     }
                                 }
                                 """;

        await Verify(SelectExtensionSource,
                     SelectExtensionSource,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Consumer"].Sources.Add(("/Consumer/Use.cs", queryCode));
                         test.TestState.AdditionalProjects["Consumer"].AdditionalProjectReferences.Add("TestProject");
                     });
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would make a deconstruction of another, unchanged document ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameMakesADeconstructionOfAnotherDocumentAmbiguous()
    {
        const string extensionCode = """
                                     namespace Shapes
                                     {
                                         public class Point
                                         {
                                             public int X { get; set; }

                                             public int Y { get; set; }
                                         }
                                     }

                                     namespace Ext
                                     {
                                         public static class PointExtensions
                                         {
                                             public static void {|RH4103:deconstruct|}(this Shapes.Point point, out int x, out int y)
                                             {
                                                 x = point.X;
                                                 y = point.Y;
                                             }
                                         }
                                     }

                                     namespace Ext2
                                     {
                                         public static class PointExtensions
                                         {
                                             public static void Deconstruct(this Shapes.Point point, out int x, out int y)
                                             {
                                                 x = point.X;
                                                 y = point.Y;
                                             }
                                         }
                                     }
                                     """;

        const string useCode = """
                               using Ext;
                               using Ext2;

                               using Shapes;

                               public static class Use
                               {
                                   public static int Sum(Point point)
                                   {
                                       var (x, y) = point;

                                       return x + y;
                                   }
                               }
                               """;

        await Verify(extensionCode,
                     extensionCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", useCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", useCode));
                     });
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would make a <c>foreach</c> of another, unchanged document ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameMakesAForeachOfAnotherDocumentAmbiguous()
    {
        const string extensionCode = """
                                     using System.Collections.Generic;

                                     namespace Shapes
                                     {
                                         public class Bag
                                         {
                                         }
                                     }

                                     namespace Ext
                                     {
                                         public static class BagExtensions
                                         {
                                             public static IEnumerator<int> {|RH4103:getEnumerator|}(this Shapes.Bag bag)
                                             {
                                                 yield return 1;
                                             }
                                         }
                                     }

                                     namespace Ext2
                                     {
                                         public static class BagExtensions
                                         {
                                             public static IEnumerator<int> GetEnumerator(this Shapes.Bag bag)
                                             {
                                                 yield return 2;
                                             }
                                         }
                                     }
                                     """;

        const string useCode = """
                               using Ext;
                               using Ext2;

                               using Shapes;

                               public static class Use
                               {
                                   public static int Sum(Bag bag)
                                   {
                                       var sum = 0;

                                       foreach (var value in bag)
                                       {
                                           sum += value;
                                       }

                                       return sum;
                                   }
                               }
                               """;

        await Verify(extensionCode,
                     extensionCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", useCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", useCode));
                     });
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would make an <c>await</c> of another, unchanged document ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameMakesAnAwaitOfAnotherDocumentAmbiguous()
    {
        const string extensionCode = """
                                     using System.Runtime.CompilerServices;
                                     using System.Threading.Tasks;

                                     namespace Shapes
                                     {
                                         public class Job
                                         {
                                         }
                                     }

                                     namespace Ext
                                     {
                                         public static class JobExtensions
                                         {
                                             public static TaskAwaiter {|RH4103:getAwaiter|}(this Shapes.Job job)
                                             {
                                                 return Task.CompletedTask.GetAwaiter();
                                             }
                                         }
                                     }

                                     namespace Ext2
                                     {
                                         public static class JobExtensions
                                         {
                                             public static TaskAwaiter GetAwaiter(this Shapes.Job job)
                                             {
                                                 return Task.CompletedTask.GetAwaiter();
                                             }
                                         }
                                     }
                                     """;

        const string useCode = """
                               using System.Threading.Tasks;

                               using Ext;
                               using Ext2;

                               using Shapes;

                               public static class Use
                               {
                                   public static async Task Run(Job job)
                                   {
                                       await job;
                                   }
                               }
                               """;

        await Verify(extensionCode,
                     extensionCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", useCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", useCode));
                     });
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