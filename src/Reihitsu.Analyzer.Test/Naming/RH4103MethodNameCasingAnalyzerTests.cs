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

    /// <summary>
    /// Source of an extension method <c>first</c> whose derived name <c>First</c> is not a name the compiler looks up
    /// </summary>
    private const string FirstExtensionSource = """
                                                using System.Collections.Generic;

                                                namespace Ext;

                                                public static class MyLinq
                                                {
                                                    public static TSource {|RH4103:first|}<TSource>(this IEnumerable<TSource> source)
                                                    {
                                                        foreach (var item in source)
                                                        {
                                                            return item;
                                                        }

                                                        return default;
                                                    }
                                                }
                                                """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the rename is not offered when a document it changes, besides the diagnostic's own, holds a comment that
    /// another build compiles as code: the reference to the method stands behind the carrier, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task NoRenameIsOfferedWhenAnotherDocumentItChangesHoldsAHidingCarrier()
    {
        const string source = """
                              namespace Reihitsu.Analyzer.Test.Naming.Resources
                              {
                                  public class Calculator
                                  {
                                      public int {|RH4103:computeTotal|}(int value)
                                      {
                                          return value * 2;
                                      }
                                  }
                              }
                              """;
        const string otherSource = """
                                   namespace Reihitsu.Analyzer.Test.Naming.Resources
                                   {
                                       public class Client
                                       {
                                           public int Use(Calculator calculator)
                                           {
                                   #if !X
                                               /*
                                   #else
                                               var s = @"
                                               */
                                               int c = 1; //";
                                   #endif
                                               return calculator.computeTotal(1);
                                           }
                                       }
                                   }
                                   """;

        await Verify(source,
                     source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));
                     });
    }

    /// <summary>
    /// Verifies that a Fix All in the solution, started from a document without a hiding carrier, renames there and leaves the
    /// method of a second document that holds a hiding carrier as it is, with its diagnostic still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllLeavesADocumentWithAHidingCarrierUnchanged()
    {
        const string source = """
                              namespace Reihitsu.Analyzer.Test.Naming.Resources
                              {
                                  public class Calculator
                                  {
                                      public int {|RH4103:computeTotal|}(int value)
                                      {
                                          return value * 2;
                                      }
                                  }
                              }
                              """;
        const string fixedSource = """
                                   namespace Reihitsu.Analyzer.Test.Naming.Resources
                                   {
                                       public class Calculator
                                       {
                                           public int ComputeTotal(int value)
                                           {
                                               return value * 2;
                                           }
                                       }
                                   }
                                   """;
        const string otherSource = """
                                   namespace Reihitsu.Analyzer.Test.Naming.Resources
                                   {
                                       public class Client
                                       {
                                           public int {|RH4103:useTotal|}(int value)
                                           {
                                   #if !X
                                               /*
                                   #else
                                               var s = @"
                                               */
                                               int c = 1; //";
                                   #endif
                                               return value;
                                           }
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

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
    /// Verifies that an extension method is renamed when a call of another document does not see it, so the new name makes nothing ambiguous and the fix stays offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAnExtensionMethodThatNoCallOfAnotherDocumentSees()
    {
        const string callCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public static class Use
                                {
                                    public static int Run(List<int> values)
                                    {
                                        return values.First();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 namespace Ext;

                                 public static class MyLinq
                                 {
                                     public static TSource First<TSource>(this IEnumerable<TSource> source)
                                     {
                                         foreach (var item in source)
                                         {
                                             return item;
                                         }

                                         return default;
                                     }
                                 }
                                 """;

        await Verify(FirstExtensionSource,
                     fixedCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", callCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", callCode));
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

    /// <summary>
    /// Verifies that a method that would be renamed to <c>GetEnumerator</c> is still reported but not fixed, because a
    /// <c>foreach</c> over the type would call the renamed method instead of the explicit interface implementation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldChangeTheMethodAForeachCalls()
    {
        const string testCode = """
                                using System.Collections;
                                using System.Collections.Generic;

                                public class Numbers : IEnumerable<int>
                                {
                                    public IEnumerator<int> {|RH4103:getEnumerator|}()
                                    {
                                        yield return 1;
                                    }

                                    IEnumerator<int> IEnumerable<int>.GetEnumerator()
                                    {
                                        yield return 2;
                                    }

                                    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();
                                }

                                public static class Consumer
                                {
                                    public static int First()
                                    {
                                        foreach (var number in new Numbers())
                                        {
                                            return number;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves a method whose rename would change the method a <c>foreach</c> calls, and renames
    /// the other method of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task FixAllSkipsTheRenameThatWouldChangeTheMethodAForeachCalls()
    {
        const string testCode = """
                                using System.Collections;
                                using System.Collections.Generic;

                                public class Numbers : IEnumerable<int>
                                {
                                    public IEnumerator<int> {|RH4103:getEnumerator|}()
                                    {
                                        yield return 1;
                                    }

                                    public int {|RH4103:count_all|}()
                                    {
                                        return 0;
                                    }

                                    IEnumerator<int> IEnumerable<int>.GetEnumerator()
                                    {
                                        yield return 2;
                                    }

                                    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();
                                }

                                public static class Consumer
                                {
                                    public static int First()
                                    {
                                        foreach (var number in new Numbers())
                                        {
                                            return number;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections;
                                 using System.Collections.Generic;

                                 public class Numbers : IEnumerable<int>
                                 {
                                     public IEnumerator<int> {|RH4103:getEnumerator|}()
                                     {
                                         yield return 1;
                                     }

                                     public int CountAll()
                                     {
                                         return 0;
                                     }

                                     IEnumerator<int> IEnumerable<int>.GetEnumerator()
                                     {
                                         yield return 2;
                                     }

                                     IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();
                                 }

                                 public static class Consumer
                                 {
                                     public static int First()
                                     {
                                         foreach (var number in new Numbers())
                                         {
                                             return number;
                                         }

                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that an extension method that would be renamed to <c>Select</c> is still reported but not fixed,
    /// because a query clause in its namespace would call it instead of <c>Enumerable.Select</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldChangeTheMethodAQueryClauseCalls()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static IEnumerable<int> {|RH4103:select|}(this IEnumerable<int> source, Func<int, int> selector)
                                        {
                                            return source;
                                        }
                                    }

                                    public class User
                                    {
                                        public IEnumerable<int> Get(List<int> values)
                                        {
                                            return from value in values select value * 2;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves an extension method whose rename would change the method a query clause calls,
    /// and renames the other method of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task FixAllSkipsTheRenameThatWouldChangeTheMethodAQueryClauseCalls()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static IEnumerable<int> {|RH4103:select|}(this IEnumerable<int> source, Func<int, int> selector)
                                        {
                                            return source;
                                        }

                                        public static int {|RH4103:count_all|}(this IEnumerable<int> source)
                                        {
                                            return 0;
                                        }
                                    }

                                    public class User
                                    {
                                        public IEnumerable<int> Get(List<int> values)
                                        {
                                            return from value in values select value * 2;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 namespace N
                                 {
                                     public static class Extensions
                                     {
                                         public static IEnumerable<int> {|RH4103:select|}(this IEnumerable<int> source, Func<int, int> selector)
                                         {
                                             return source;
                                         }

                                         public static int CountAll(this IEnumerable<int> source)
                                         {
                                             return 0;
                                         }
                                     }

                                     public class User
                                     {
                                         public IEnumerable<int> Get(List<int> values)
                                         {
                                             return from value in values select value * 2;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that a method that would be renamed to <c>Deconstruct</c> is still reported but not fixed, because a
    /// deconstruction would call it instead of the extension method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldChangeTheMethodADeconstructionCalls()
    {
        const string testCode = """
                                public class Point
                                {
                                    public void {|RH4103:deconstruct|}(out int x, out int y)
                                    {
                                        x = 3;
                                        y = 0;
                                    }
                                }

                                public static class PointExtensions
                                {
                                    public static void Deconstruct(this Point point, out int x, out int y)
                                    {
                                        x = 7;
                                        y = 0;
                                    }
                                }

                                public static class Consumer
                                {
                                    public static int GetX(Point point)
                                    {
                                        var (x, _) = point;

                                        return x;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves a method whose rename would change the method a deconstruction calls, and renames
    /// the other method of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task FixAllSkipsTheRenameThatWouldChangeTheMethodADeconstructionCalls()
    {
        const string testCode = """
                                public class Point
                                {
                                    public void {|RH4103:deconstruct|}(out int x, out int y)
                                    {
                                        x = 3;
                                        y = 0;
                                    }

                                    public int {|RH4103:get_length|}()
                                    {
                                        return 0;
                                    }
                                }

                                public static class PointExtensions
                                {
                                    public static void Deconstruct(this Point point, out int x, out int y)
                                    {
                                        x = 7;
                                        y = 0;
                                    }
                                }

                                public static class Consumer
                                {
                                    public static int GetX(Point point)
                                    {
                                        var (x, _) = point;

                                        return x;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Point
                                 {
                                     public void {|RH4103:deconstruct|}(out int x, out int y)
                                     {
                                         x = 3;
                                         y = 0;
                                     }

                                     public int GetLength()
                                     {
                                         return 0;
                                     }
                                 }

                                 public static class PointExtensions
                                 {
                                     public static void Deconstruct(this Point point, out int x, out int y)
                                     {
                                         x = 7;
                                         y = 0;
                                     }
                                 }

                                 public static class Consumer
                                 {
                                     public static int GetX(Point point)
                                     {
                                         var (x, _) = point;

                                         return x;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that a method that would be renamed to <c>Add</c> is still reported but not fixed, because a collection
    /// initializer would call it instead of the extension method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task NoFixIsOfferedWhenTheRenameWouldChangeTheMethodACollectionInitializerCalls()
    {
        const string testCode = """
                                using System.Collections;
                                using System.Collections.Generic;

                                public class Box : IEnumerable<object>
                                {
                                    private readonly List<object> _items = new List<object>();

                                    public void {|RH4103:add|}(int value)
                                    {
                                        _items.Add("instance");
                                    }

                                    public IEnumerator<object> GetEnumerator() => _items.GetEnumerator();

                                    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                }

                                public static class BoxExtensions
                                {
                                    public static void Add(this Box box, int value)
                                    {
                                    }
                                }

                                public static class Consumer
                                {
                                    public static Box Create()
                                    {
                                        return new Box { 1, 2 };
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves a method whose rename would change the method a collection initializer calls, and
    /// renames the other method of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task FixAllSkipsTheRenameThatWouldChangeTheMethodACollectionInitializerCalls()
    {
        const string testCode = """
                                using System.Collections;
                                using System.Collections.Generic;

                                public class Box : IEnumerable<object>
                                {
                                    private readonly List<object> _items = new List<object>();

                                    public void {|RH4103:add|}(int value)
                                    {
                                        _items.Add("instance");
                                    }

                                    public int {|RH4103:count_all|}()
                                    {
                                        return _items.Count;
                                    }

                                    public IEnumerator<object> GetEnumerator() => _items.GetEnumerator();

                                    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                }

                                public static class BoxExtensions
                                {
                                    public static void Add(this Box box, int value)
                                    {
                                    }
                                }

                                public static class Consumer
                                {
                                    public static Box Create()
                                    {
                                        return new Box { 1, 2 };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections;
                                 using System.Collections.Generic;

                                 public class Box : IEnumerable<object>
                                 {
                                     private readonly List<object> _items = new List<object>();

                                     public void {|RH4103:add|}(int value)
                                     {
                                         _items.Add("instance");
                                     }

                                     public int CountAll()
                                     {
                                         return _items.Count;
                                     }

                                     public IEnumerator<object> GetEnumerator() => _items.GetEnumerator();

                                     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                 }

                                 public static class BoxExtensions
                                 {
                                     public static void Add(this Box box, int value)
                                     {
                                     }
                                 }

                                 public static class Consumer
                                 {
                                     public static Box Create()
                                     {
                                         return new Box { 1, 2 };
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that a method whose derived name is a name the compiler looks up is still reported but not fixed, also
    /// when no construct of the code would call another method after the rename
    /// </summary>
    /// <param name="oldName">The reported method name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    [DataRow("add")]
    [DataRow("appendFormatted")]
    [DataRow("appendLiteral")]
    [DataRow("cast")]
    [DataRow("deconstruct")]
    [DataRow("dispose")]
    [DataRow("disposeAsync")]
    [DataRow("getAsyncEnumerator")]
    [DataRow("getAwaiter")]
    [DataRow("getEnumerator")]
    [DataRow("getPinnableReference")]
    [DataRow("groupBy")]
    [DataRow("groupJoin")]
    [DataRow("join")]
    [DataRow("orderBy")]
    [DataRow("orderByDescending")]
    [DataRow("select")]
    [DataRow("selectMany")]
    [DataRow("thenBy")]
    [DataRow("thenByDescending")]
    [DataRow("where")]
    [DataRow("get_enumerator")]
    [DataRow("GET_ENUMERATOR")]
    public async Task NoFixIsOfferedWhenTheDerivedNameIsANameTheCompilerLooksUp(string oldName)
    {
        var testCode = $$"""
                         public class C
                         {
                             public void {|RH4103:{{oldName}}|}()
                             {
                             }
                         }
                         """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method whose derived name is close to, but not one of, the names the compiler looks up is renamed
    /// </summary>
    /// <param name="oldName">The reported method name</param>
    /// <param name="newName">The expected new name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    [DataRow("getEnumerators", "GetEnumerators")]
    [DataRow("moveNext", "MoveNext")]
    [DataRow("select_all", "SelectAll")]
    [DataRow("adder", "Adder")]
    public async Task FixIsOfferedWhenTheDerivedNameIsNotANameTheCompilerLooksUp(string oldName, string newName)
    {
        var testCode = $$"""
                         public class C
                         {
                             public void {|RH4103:{{oldName}}|}()
                             {
                             }
                         }
                         """;

        var fixedCode = $$"""
                          public class C
                          {
                              public void {{newName}}()
                              {
                              }
                          }
                          """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the example of names outside the list: the method is renamed, and a local function named like a looked-up
    /// name is not reported by this rule and left as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-011")]
    [Requirement("RH4103-003")]
    public async Task FixIsOfferedForAMethodWhoseDerivedNameIsOutsideTheList()
    {
        const string testCode = """
                                public class C
                                {
                                    public int {|RH4103:select_all|}()
                                    {
                                        int getEnumerator()
                                        {
                                            return 1;
                                        }

                                        return getEnumerator();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public int SelectAll()
                                     {
                                         int getEnumerator()
                                         {
                                             return 1;
                                         }

                                         return getEnumerator();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would win the call of an extension
    /// method of another class in the same namespace, which the renamer would rewrite into a static call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCall()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
                                    }

                                    public class User
                                    {
                                        public List<int> Get(int[] values) => values.ToList();
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that an extension method is renamed, and the call of the other extension method is left as written, when the call
    /// stands in a namespace that does not import the renamed method's namespace, so its binding does not change
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAnExtensionMethodWhenTheCallOfAnotherNamespaceKeepsItsBinding()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
                                    }
                                }

                                namespace M
                                {
                                    public class User
                                    {
                                        public List<int> Get(int[] values) => values.ToList();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 namespace N
                                 {
                                     public static class Extensions
                                     {
                                         public static List<int> ToList(this IEnumerable<int> source) => new List<int>();
                                     }
                                 }

                                 namespace M
                                 {
                                     public class User
                                     {
                                         public List<int> Get(int[] values) => values.ToList();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would win, from an enclosing namespace,
    /// the call of an extension method that a nested namespace makes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCallOfANestedNamespace()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static int {|RH4103:count|}(this IEnumerable<int> source) => 0;
                                    }

                                    namespace Inner
                                    {
                                        public class User
                                        {
                                            public int Get(int[] values) => values.Count();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that an instance method keeps its diagnostic but gets no fix when its new name would win over the extension method
    /// that a call on its type makes today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinOverAnExtensionMethodCalledOnTheType()
    {
        const string testCode = """
                                using System.Collections;
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Bag : IEnumerable<int>
                                {
                                    public int {|RH4103:first|}() => 0;

                                    public IEnumerator<int> GetEnumerator()
                                    {
                                        yield return 1;
                                    }

                                    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                                }

                                public class User
                                {
                                    public int Get(Bag bag) => bag.First();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would make the call of an extension
    /// method with the same signature in another class ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldMakeAnExtensionMethodCallAmbiguous()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                namespace N
                                {
                                    public static class A
                                    {
                                        public static void Shuffle(this List<int> values)
                                        {
                                        }
                                    }

                                    public static class B
                                    {
                                        public static void {|RH4103:shuffle|}(this List<int> values)
                                        {
                                        }
                                    }

                                    public class User
                                    {
                                        public void Use(List<int> values) => values.Shuffle();
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would win the overload resolution of a call
    /// that binds to another overload today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinOverloadResolution()
    {
        const string testCode = """
                                public class Box
                                {
                                    public void {|RH4103:write|}(int value)
                                    {
                                    }

                                    public void Write(object value)
                                    {
                                    }

                                    public void Use(Box box) => box.Write(1);
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a generic method keeps its diagnostic but gets no fix when its new name would win the overload resolution of a
    /// call that binds to a non-generic overload today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameOfAGenericMethodWouldWinOverloadResolution()
    {
        const string testCode = """
                                public class Box
                                {
                                    public string {|RH4103:format|}<T>(T value) => "generic";

                                    public string Format(object value) => "object";

                                    public string Use(Box box) => box.Format(1);
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method of a nested class keeps its diagnostic but gets no fix when its new name would hide, inside the nested
    /// class, the static method of the outer class that a call there binds to today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAnOuterMethodInANestedClass()
    {
        const string testCode = """
                                public class Outer
                                {
                                    public static void Log(string text)
                                    {
                                    }

                                    public class Inner
                                    {
                                        public void {|RH4103:log|}(string text)
                                        {
                                        }

                                        public void Use() => Log("x");
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would win over a method that a <c>using static</c>
    /// directive imports and a call binds to today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinOverAMethodOfAUsingStaticImport()
    {
        const string testCode = """
                                using static System.Math;

                                public class Calculator
                                {
                                    public int {|RH4103:max|}(int left, int right) => left;

                                    public int Use() => Max(1, 2);
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would hide, inside its class, a type that a member
    /// access there binds to today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideATypeUsedInTheClass()
    {
        const string testCode = """
                                namespace N
                                {
                                    public enum Color
                                    {
                                        Red,
                                        Blue,
                                    }

                                    public class Painter
                                    {
                                        public int {|RH4103:color|}() => 0;

                                        public Color Use() => Color.Red;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would hide an inherited method that an
    /// unqualified call in the same class binds to today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedMethodCalledInTheClass()
    {
        const string testCode = """
                                public class Base
                                {
                                    public int Process() => 1;
                                }

                                public class Derived : Base
                                {
                                    public int {|RH4103:process|}() => 2;

                                    public int Use() => Process();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would hide an inherited method that a member
    /// access on the derived type in another class calls today, so that the unchanged call would call the renamed method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedMethodCalledOnTheDerivedType()
    {
        const string testCode = """
                                public class Base
                                {
                                    public int Process() => 1;
                                }

                                public class Derived : Base
                                {
                                    public int {|RH4103:process|}() => 2;
                                }

                                public class User
                                {
                                    public int Use(Derived derived) => derived.Process();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would hide an inherited virtual method that a
    /// member access on the derived type calls today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedVirtualMethodCalledOnTheDerivedType()
    {
        const string testCode = """
                                public class Base
                                {
                                    public virtual int Process() => 1;
                                }

                                public class Derived : Base
                                {
                                    public int {|RH4103:process|}() => 2;
                                }

                                public class User
                                {
                                    public int Use(Derived derived) => derived.Process();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a static method keeps its diagnostic but gets no fix when its new name would hide an inherited static method that
    /// a call qualified with the derived type calls today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedStaticMethodCalledThroughTheDerivedType()
    {
        const string testCode = """
                                public class Base
                                {
                                    public static int Create() => 1;
                                }

                                public class Derived : Base
                                {
                                    public static int {|RH4103:create|}() => 2;
                                }

                                public class User
                                {
                                    public int Use() => Derived.Create();
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method is renamed when its new name hides an inherited method, but the only call of that method has the base
    /// type as its receiver, so its binding does not change and it is left as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAMethodThatHidesAnInheritedMethodCalledOnlyOnTheBaseType()
    {
        const string testCode = """
                                public class Base
                                {
                                    public int Process() => 1;
                                }

                                public class Derived : Base
                                {
                                    public int {|RH4103:process|}() => 2;
                                }

                                public class User
                                {
                                    public int Use(Base item) => item.Process();
                                }
                                """;

        const string fixedCode = """
                                 public class Base
                                 {
                                     public int Process() => 1;
                                 }

                                 public class Derived : Base
                                 {
                                     public int Process() => 2;
                                 }

                                 public class User
                                 {
                                     public int Use(Base item) => item.Process();
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would win the call of an extension
    /// method that stands in another document of the same project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCallOfAnotherDocument()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
                                    }
                                }
                                """;

        const string userCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public class User
                                    {
                                        public List<int> Get(int[] values) => values.ToList();
                                    }
                                }
                                """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", userCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", userCode));
                     });
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when its new name would win the call of an extension
    /// method that stands in a dependent project
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldWinAnExtensionMethodCallOfADependentProject()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
                                    }
                                }
                                """;

        const string userCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N.Consumer
                                {
                                    public class User
                                    {
                                        public List<int> Get(int[] values) => values.ToList();
                                    }
                                }
                                """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Consumer"].Sources.Add(("/Consumer/User.cs", userCode));
                         test.TestState.AdditionalProjects["Consumer"].AdditionalProjectReferences.Add("TestProject");
                     });
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would hide an inherited method that a member
    /// access on the derived type in a dependent project calls today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAnInheritedMethodCalledInADependentProject()
    {
        const string testCode = """
                                public class Base
                                {
                                    public int Process() => 1;
                                }

                                public class Derived : Base
                                {
                                    public int {|RH4103:process|}() => 2;
                                }
                                """;

        const string userCode = """
                                public class User
                                {
                                    public int Use(Derived derived) => derived.Process();
                                }
                                """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Consumer"].Sources.Add(("/Consumer/User.cs", userCode));
                         test.TestState.AdditionalProjects["Consumer"].AdditionalProjectReferences.Add("TestProject");
                     });
    }

    /// <summary>
    /// Verifies that Fix All renames the method whose new name meets no existing binding and skips the extension method whose new name
    /// would win the call of another extension method, which keeps its diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixAllSkipsTheRenameThatWouldWinAnExtensionMethodCall()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();

                                        public static int {|RH4103:computeTotal|}(this IEnumerable<int> source) => 0;
                                    }

                                    public class User
                                    {
                                        public List<int> Get(int[] values) => values.ToList();

                                        public int Sum(int[] values) => values.computeTotal();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 namespace N
                                 {
                                     public static class Extensions
                                     {
                                         public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();

                                         public static int ComputeTotal(this IEnumerable<int> source) => 0;
                                     }

                                     public class User
                                     {
                                         public List<int> Get(int[] values) => values.ToList();

                                         public int Sum(int[] values) => values.ComputeTotal();
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostic that keeps no fix is marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that Fix All renames the method whose new name meets no existing binding and skips the method whose new name would
    /// overload-resolve a call that binds to another method today, which keeps its diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixAllSkipsTheRenameThatWouldWinOverloadResolution()
    {
        const string testCode = """
                                public class Box
                                {
                                    public void {|RH4103:write|}(int value)
                                    {
                                    }

                                    public void Write(object value)
                                    {
                                    }

                                    public void {|RH4103:flush_all|}()
                                    {
                                    }

                                    public void Use(Box box)
                                    {
                                        box.Write(1);
                                        box.flush_all();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Box
                                 {
                                     public void {|RH4103:write|}(int value)
                                     {
                                     }

                                     public void Write(object value)
                                     {
                                     }

                                     public void FlushAll()
                                     {
                                     }

                                     public void Use(Box box)
                                     {
                                         box.Write(1);
                                         box.FlushAll();
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostic that keeps no fix is marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that Fix All renames the method whose new name meets no existing binding and skips the method whose new name would
    /// hide an inherited method that a call on the derived type calls today, which keeps its diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixAllSkipsTheRenameThatWouldHideAnInheritedMethodCalledOnTheDerivedType()
    {
        const string testCode = """
                                public class Base
                                {
                                    public int Process() => 1;
                                }

                                public class Derived : Base
                                {
                                    public int {|RH4103:process|}() => 2;

                                    public int {|RH4103:compute_total|}() => 3;
                                }

                                public class User
                                {
                                    public int Use(Derived derived) => derived.Process() + derived.compute_total();
                                }
                                """;

        const string fixedCode = """
                                 public class Base
                                 {
                                     public int Process() => 1;
                                 }

                                 public class Derived : Base
                                 {
                                     public int {|RH4103:process|}() => 2;

                                     public int ComputeTotal() => 3;
                                 }

                                 public class User
                                 {
                                     public int Use(Derived derived) => derived.Process() + derived.ComputeTotal();
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostic that keeps no fix is marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that an extension method keeps its diagnostic but gets no fix when the renamer would turn a call of it into a static
    /// call, so that the call keeps binding to the renamed method instead of an imported extension method of the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheRenamerWouldTurnACallOfTheRenamedExtensionMethodIntoAStaticCall()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                namespace N
                                {
                                    public static class Extensions
                                    {
                                        public static List<int> {|RH4103:toList|}(this IEnumerable<int> source) => new List<int>();
                                    }

                                    namespace Inner
                                    {
                                        using System.Linq;

                                        public class User
                                        {
                                            public List<int> Get(int[] values) => values.toList();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when the renamer would cast an argument of a call of it, so that
    /// the call keeps binding to the renamed method instead of an overload of the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheRenamerWouldCastAnArgumentOfACallOfTheRenamedMethod()
    {
        const string testCode = """
                                public class Box
                                {
                                    public void {|RH4103:write|}(object value)
                                    {
                                    }

                                    public void Write(int value)
                                    {
                                    }

                                    public void Use(Box box) => box.write(1);
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name would make it implement an interface method that
    /// its type lists among its own base types and that an inherited method implements today, although no reference names the
    /// interface method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldChangeWhichMethodImplementsAnInterfaceMethod()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    int Process();
                                }

                                public class Base : IProcessor
                                {
                                    public int Process() => 1;
                                }

                                public class Derived : Base, IProcessor
                                {
                                    public int {|RH4103:process|}() => 2;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its new name is that of an interface method that its type implements
    /// only through its base type, although the inherited method keeps implementing the interface method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameIsThatOfAnInterfaceMethodThatTheTypeImplementsThroughItsBaseType()
    {
        const string testCode = """
                                public interface IWorker
                                {
                                    int Work();
                                }

                                public class WorkerBase : IWorker
                                {
                                    public int Work() => 1;
                                }

                                public class WorkerDerived : WorkerBase
                                {
                                    public int {|RH4103:work|}() => 2;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its type implements no interface, but a type derived from it lists an
    /// interface whose method has the new name and is implemented today by a method of the method's base type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenATypeDerivedFromTheMethodsTypeListsAnInterfaceWithAMethodOfTheNewName()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    int Process();
                                }

                                public class Root
                                {
                                    public int Process() => 1;
                                }

                                public class Base : Root
                                {
                                    public int {|RH4103:process|}() => 2;
                                }

                                public class Derived : Base, IProcessor
                                {
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when a type derived from its type lists an interface whose method has the
    /// new name and a default implementation in the interface implements it today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenATypeDerivedFromTheMethodsTypeListsAnInterfaceWhoseMethodOfTheNewNameHasADefaultImplementation()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    int Process() => 1;
                                }

                                public class Base
                                {
                                    public int {|RH4103:process|}() => 2;
                                }

                                public class Derived : Base, IProcessor
                                {
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a method keeps its diagnostic but gets no fix when its base type implements an interface whose method has the new
    /// name and a type derived from the method's type lists that interface again
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenATypeDerivedFromTheMethodsTypeListsAgainAnInterfaceThatTheBaseTypeImplements()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    int Process();
                                }

                                public class Root : IProcessor
                                {
                                    public int Process() => 1;
                                }

                                public class Base : Root
                                {
                                    public int {|RH4103:process|}() => 2;
                                }

                                public class Derived : Base, IProcessor
                                {
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that an interface method keeps its diagnostic but gets no fix when a type that implements its interface also implements
    /// another interface with a method of the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenATypeImplementingTheMethodsInterfaceImplementsAnotherInterfaceWithAMethodOfTheNewName()
    {
        const string testCode = """
                                public interface IFirst
                                {
                                    int Process();
                                }

                                public interface ISecond
                                {
                                    int {|RH4103:process|}();
                                }

                                public class Worker : IFirst, ISecond
                                {
                                    public int Process() => 1;

                                    int ISecond.process() => 2;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that an interface method keeps its diagnostic but gets no fix when an interface that inherits from its interface also
    /// inherits from another interface with a method of the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenAnInterfaceInheritingFromTheMethodsInterfaceInheritsAnotherInterfaceWithAMethodOfTheNewName()
    {
        const string testCode = """
                                public interface IFirst
                                {
                                    int Process();
                                }

                                public interface ISecond
                                {
                                    int {|RH4103:process|}();
                                }

                                public interface IBoth : IFirst, ISecond
                                {
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that the fix renames a method to the name of an interface method when neither its type nor a type derived from it
    /// implements that interface, and the interfaces its type implements have no member of the new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesAMethodToTheNameOfAMethodOfAnInterfaceOutsideItsTypesHierarchy()
    {
        const string testCode = """
                                public interface IProcessor
                                {
                                    int Process();
                                }

                                public class Worker : IProcessor
                                {
                                    public int Process() => 1;
                                }

                                public class Job : System.IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }

                                    public int {|RH4103:process|}() => 2;
                                }
                                """;

        const string fixedCode = """
                                 public interface IProcessor
                                 {
                                     int Process();
                                 }

                                 public class Worker : IProcessor
                                 {
                                     public int Process() => 1;
                                 }

                                 public class Job : System.IDisposable
                                 {
                                     public void Dispose()
                                     {
                                     }

                                     public int Process() => 2;
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