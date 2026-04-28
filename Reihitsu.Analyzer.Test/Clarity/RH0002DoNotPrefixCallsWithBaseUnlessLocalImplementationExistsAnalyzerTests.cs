using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer"/> and <see cref="RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests : AnalyzerTestsBase<RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer, RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider>
{
    /// <summary>
    /// Verifying unnecessary base qualifier on method call is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierOnInheritedMethodIsReportedAndFixed()
    {
        const string testCode = """
                                public class BaseType
                                {
                                    protected int GetValue()
                                    {
                                        return 1;
                                    }
                                }

                                public class DerivedType : BaseType
                                {
                                    public int Run()
                                    {
                                        return {|#0:base|}.GetValue();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class BaseType
                                 {
                                     protected int GetValue()
                                     {
                                         return 1;
                                     }
                                 }

                                 public class DerivedType : BaseType
                                 {
                                     public int Run()
                                     {
                                         return GetValue();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier on property access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierOnInheritedPropertyIsReportedAndFixed()
    {
        const string testCode = """
                                public class Vehicle
                                {
                                    protected int Speed { get; set; }
                                }

                                public class Car : Vehicle
                                {
                                    public void Accelerate()
                                    {
                                        {|#0:base|}.Speed += 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Vehicle
                                 {
                                     protected int Speed { get; set; }
                                 }

                                 public class Car : Vehicle
                                 {
                                     public void Accelerate()
                                     {
                                         Speed += 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier on element access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierOnInheritedIndexerIsReportedAndFixed()
    {
        const string testCode = """
                                public class Collection
                                {
                                    protected string this[int index]
                                    {
                                        get => "item";
                                    }
                                }

                                public class CustomCollection : Collection
                                {
                                    public string GetItem(int idx)
                                    {
                                        return {|#0:base|}[idx];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Collection
                                 {
                                     protected string this[int index]
                                     {
                                         get => "item";
                                     }
                                 }

                                 public class CustomCollection : Collection
                                 {
                                     public string GetItem(int idx)
                                     {
                                         return this[idx];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier is not reported when local override exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierWithLocalOverrideIsNotReported()
    {
        const string testCode = """
                                public class Animal
                                {
                                    public virtual void MakeSound()
                                    {
                                    }
                                }

                                public class Dog : Animal
                                {
                                    public override void MakeSound()
                                    {
                                        base.MakeSound();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying base qualifier is not reported when calling shadowed member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierWithShadowedMemberIsNotReported()
    {
        const string testCode = """
                                public class Shape
                                {
                                    public int Area()
                                    {
                                        return 0;
                                    }
                                }

                                public class Circle : Shape
                                {
                                    public new int Area()
                                    {
                                        return base.Area() + 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying unnecessary base qualifier in chained member access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierInChainedMemberAccessIsReportedAndFixed()
    {
        const string testCode = """
                                public class Logger
                                {
                                    protected string GetTimestamp()
                                    {
                                        return string.Empty;
                                    }
                                }

                                public class FileLogger : Logger
                                {
                                    public int GetLength()
                                    {
                                        return {|#0:base|}.GetTimestamp().Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Logger
                                 {
                                     protected string GetTimestamp()
                                     {
                                         return string.Empty;
                                     }
                                 }

                                 public class FileLogger : Logger
                                 {
                                     public int GetLength()
                                     {
                                         return GetTimestamp().Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier on field access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierOnInheritedFieldIsReportedAndFixed()
    {
        const string testCode = """
                                public class Counter
                                {
                                    protected int count;
                                }

                                public class Timer : Counter
                                {
                                    public int GetCount()
                                    {
                                        return {|#0:base|}.count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Counter
                                 {
                                     protected int count;
                                 }

                                 public class Timer : Counter
                                 {
                                     public int GetCount()
                                     {
                                         return count;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier with nested invocations is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierWithNestedInvocationIsReportedAndFixed()
    {
        const string testCode = """
                                public class Calculator
                                {
                                    protected int Add(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }

                                public class ScientificCalculator : Calculator
                                {
                                    public int Compute()
                                    {
                                        return {|#0:base|}.Add(1, 2);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Calculator
                                 {
                                     protected int Add(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }

                                 public class ScientificCalculator : Calculator
                                 {
                                     public int Compute()
                                     {
                                         return Add(1, 2);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base call inside an override with a call chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BaseQualifierInsideOverrideWithCallChainIsNotReported()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                public class BaseType
                                {
                                    public virtual Task Initialize(IServiceProvider serviceProvider)
                                    {
                                        return Task.CompletedTask;
                                    }
                                }

                                public class DerivedType : BaseType
                                {
                                    public override async Task Initialize(IServiceProvider serviceProvider)
                                    {
                                        await base.Initialize(serviceProvider)
                                                  .ConfigureAwait(false);
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}