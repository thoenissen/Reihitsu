using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer"/> and <see cref="RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests : BatchCodeFixTestsBase<RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer, RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying unnecessary base qualifier on method call is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    [Requirement("RH3101-004")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier on property access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    [Requirement("RH3101-004")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier on element access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    [Requirement("RH3101-004")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier is not reported when local override exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-002")]
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
    [Requirement("RH3101-002")]
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
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    [Requirement("RH3101-004")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier on field access is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    [Requirement("RH3101-004")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base qualifier with nested invocations is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    [Requirement("RH3101-004")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists."));
    }

    /// <summary>
    /// Verifying base call inside an override with a call chain is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-002")]
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

    /// <summary>
    /// Verifies <c>base</c> accesses of a method, a property, an indexer, and a field that the type does not
    /// override or hide are reported on <c>base</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-001")]
    [Requirement("RH3101-003")]
    public async Task BaseAccessesOfInheritedMembersAreReportedOnTheBaseKeyword()
    {
        const string testCode = """
                                class B
                                {
                                    public virtual int M() => 1;
                                    public int P { get; set; }
                                    public int this[int i] => i;
                                    protected int F;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.M() + {|RH3101:base|}.P + {|RH3101:base|}[0] + {|RH3101:base|}.F;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies <c>base</c> accesses of an overridden member, of a hidden member, and inside the override of the
    /// accessed member are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-002")]
    public async Task BaseAccessesTheTypeNeedsAreNotReported()
    {
        const string testCode = """
                                class B
                                {
                                    public virtual int M() => 1;
                                    public virtual int N() => 1;
                                    public int Q() => 1;
                                }

                                class D : B
                                {
                                    public override int M() => base.M();

                                    public override int N() => 2;

                                    public new int Q() => 2;

                                    int Run()
                                    {
                                        return base.N() + base.Q();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix removes the qualifier of a member access and writes <c>this</c> for an element access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-004")]
    public async Task FixRemovesTheQualifierAndWritesThisForAnElementAccess()
    {
        const string testCode = """
                                class B
                                {
                                    public int M() => 1;
                                    public int P { get; set; }
                                    public int this[int i] => i;
                                    protected int F;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.M() + {|RH3101:base|}.P + {|RH3101:base|}[0] + {|RH3101:base|}.F;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class B
                                 {
                                     public int M() => 1;
                                     public int P { get; set; }
                                     public int this[int i] => i;
                                     protected int F;
                                 }

                                 class D : B
                                 {
                                     int Run()
                                     {
                                         return M() + P + this[0] + F;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the access of a virtual method from a type that is not sealed is reported without a code fix, because
    /// the call without the qualifier would dispatch to the override of a derived type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-007")]
    public async Task VirtualMethodInUnsealedTypeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class B
                                {
                                    public virtual int M() => 1;
                                }

                                class D : B
                                {
                                    public int Run()
                                    {
                                        return {|RH3101:base|}.M();
                                    }
                                }

                                class E : D
                                {
                                    public override int M() => 2;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the access of a virtual method from a sealed type is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-004")]
    [Requirement("RH3101-007")]
    public async Task VirtualMethodInSealedTypeIsFixed()
    {
        const string testCode = """
                                class B
                                {
                                    public virtual int M() => 1;
                                }

                                sealed class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.M();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class B
                                 {
                                     public virtual int M() => 1;
                                 }

                                 sealed class D : B
                                 {
                                     int Run()
                                     {
                                         return M();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the access of an override that is not sealed from a type that is not sealed is reported without a code
    /// fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-007")]
    public async Task UnsealedOverrideInUnsealedTypeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class A
                                {
                                    public virtual int M() => 1;
                                }

                                class B : A
                                {
                                    public override int M() => 2;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.M();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the access of a sealed override from a type that is not sealed is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-004")]
    [Requirement("RH3101-007")]
    public async Task SealedOverrideInUnsealedTypeIsFixed()
    {
        const string testCode = """
                                class A
                                {
                                    public virtual int M() => 1;
                                }

                                class B : A
                                {
                                    public sealed override int M() => 2;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.M();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class A
                                 {
                                     public virtual int M() => 1;
                                 }

                                 class B : A
                                 {
                                     public sealed override int M() => 2;
                                 }

                                 class D : B
                                 {
                                     int Run()
                                     {
                                         return M();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the access of a virtual property and of a virtual indexer from a type that is not sealed is reported
    /// without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-007")]
    public async Task VirtualPropertyAndIndexerInUnsealedTypeAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                class B
                                {
                                    public virtual int P => 1;
                                    public virtual int this[int i] => i;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.P + {|RH3101:base|}[0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the access of a virtual indexer from a sealed type is fixed to <c>this</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-004")]
    [Requirement("RH3101-007")]
    public async Task VirtualIndexerInSealedTypeIsFixed()
    {
        const string testCode = """
                                class B
                                {
                                    public virtual int this[int i] => i;
                                }

                                sealed class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}[0];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class B
                                 {
                                     public virtual int this[int i] => i;
                                 }

                                 sealed class D : B
                                 {
                                     int Run()
                                     {
                                         return this[0];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the access of an inherited override from a struct, which no type can derive from, is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-004")]
    [Requirement("RH3101-007")]
    public async Task InheritedOverrideInStructIsFixed()
    {
        const string testCode = """
                                struct S
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}.GetHashCode();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 struct S
                                 {
                                     int Run()
                                     {
                                         return GetHashCode();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies comments on both sides of the dot of a member access are reported without a code fix, because the fix
    /// would delete them with the qualifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-006")]
    public async Task CommentsAroundTheDotAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                class B
                                {
                                    public int M() => 1;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|} /* a */ . /* b */ M();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment between <c>base</c> and the dot alone is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-006")]
    public async Task CommentInFrontOfTheDotIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class B
                                {
                                    public int M() => 1;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|} /* a */.M();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an end-of-line comment between the dot and the member name alone is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-006")]
    public async Task CommentBehindTheDotIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class B
                                {
                                    public int M() => 1;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}. // b
                                            M();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a preprocessor directive between <c>base</c> and the member name is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-006")]
    public async Task DirectiveBetweenBaseAndTheMemberNameIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class B
                                {
                                    public int M() => 1;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|}
                                #if DEBUG
                                #endif
                                            .M();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment between <c>base</c> and the bracket of an element access does not withhold the fix and stays
    /// behind the <c>this</c> the fix writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-006")]
    public async Task CommentInAnElementAccessStaysBehindThis()
    {
        const string testCode = """
                                class B
                                {
                                    public int this[int i] => i;
                                }

                                class D : B
                                {
                                    int Run()
                                    {
                                        return {|RH3101:base|} /* a */ [0];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class B
                                 {
                                     public int this[int i] => i;
                                 }

                                 class D : B
                                 {
                                     int Run()
                                     {
                                         return this /* a */ [0];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the qualified right operand of a <c>&lt;</c> in an argument is reported without a code fix, because the
    /// name without the qualifier would, with the <c>&gt;</c> of the next argument, read as a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task BaseOperandAfterLessThanBeforeAGreaterThanArgumentIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class P
                                {
                                    protected int B;
                                }

                                class C : P
                                {
                                    int A;
                                    int D;

                                    void M(bool x, bool y)
                                    {
                                    }

                                    void Run()
                                    {
                                        M(A < {|RH3101:base|}.B, D > (7));
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a qualified argument between a <c>&lt;</c> argument and a <c>&gt;</c> argument is reported without a
    /// code fix, because the name without the qualifier would read as a type argument of a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task BaseArgumentBetweenLessThanAndGreaterThanArgumentsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class P
                                {
                                    protected int X;
                                }

                                class C : P
                                {
                                    int A;
                                    int B;
                                    int D;

                                    void M(bool x, int z, bool y)
                                    {
                                    }

                                    void Run()
                                    {
                                        M(A < B, {|RH3101:base|}.X, D > (7));
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the qualified left operand of a <c>&gt;</c> in an argument after a <c>&lt;</c> argument is reported
    /// without a code fix, because the name without the qualifier would read as a type argument of a generic invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task BaseOperandBeforeGreaterThanAfterALessThanArgumentIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class P
                                {
                                    protected int D;
                                }

                                class C : P
                                {
                                    int A;
                                    int B;

                                    void M(bool x, bool y)
                                    {
                                    }

                                    void Run()
                                    {
                                        M(A < B, {|RH3101:base|}.D > (7));
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a qualified delegate field in parentheses that is invoked is reported without a code fix, because the
    /// name alone in parentheses would read as a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task ParenthesizedBaseAccessFollowedByAnArgumentListIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class P
                                {
                                    protected Action<int> A;
                                }

                                class C : P
                                {
                                    void Run()
                                    {
                                        ({|RH3101:base|}.A)(5);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a qualified field in parentheses followed by the null-forgiving operator is reported without a code
    /// fix,
    /// because the name alone in parentheses would read as a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task ParenthesizedBaseAccessFollowedByANullForgivingOperatorIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class P
                                {
                                    protected string S;
                                }

                                class C : P
                                {
                                    int Run()
                                    {
                                        return ({|RH3101:base|}.S)!.Length;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a qualified field in parentheses followed by <c>with</c> is reported without a code fix, because the
    /// name alone in parentheses would read as a cast
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task ParenthesizedBaseAccessFollowedByWithIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                record R(int X);

                                class P
                                {
                                    protected R Q;
                                }

                                class C : P
                                {
                                    R Run()
                                    {
                                        return ({|RH3101:base|}.Q) with { X = 1 };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a qualified field in parentheses is still fixed where the parentheses keep reading as a parenthesized
    /// expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-004")]
    [Requirement("RH3101-005")]
    public async Task ParenthesizedBaseAccessFollowedByABinaryOperatorIsFixed()
    {
        const string testCode = """
                                class P
                                {
                                    protected int N;
                                }

                                class C : P
                                {
                                    int Run()
                                    {
                                        return ({|RH3101:base|}.N) - 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class P
                                 {
                                     protected int N;
                                 }

                                 class C : P
                                 {
                                     int Run()
                                     {
                                         return (N) - 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves an access whose fix would read as a cast as written and reported, and fixes the other
    /// accesses of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3101-005")]
    public async Task FixAllLeavesAnAccessThatWouldReadAsACastReported()
    {
        const string testCode = """
                                using System;

                                class P
                                {
                                    protected Action<int> A;
                                    protected int N;
                                }

                                class C : P
                                {
                                    int Run()
                                    {
                                        ({|RH3101:base|}.A)(5);

                                        return {|RH3101:base|}.N + {|RH3101:base|}.N;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class P
                                 {
                                     protected Action<int> A;
                                     protected int N;
                                 }

                                 class C : P
                                 {
                                     int Run()
                                     {
                                         ({|RH3101:base|}.A)(5);

                                         return N + N;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostics whose fix is withheld stay and are marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class BaseType
                                {
                                    protected int GetValue()
                                    {
                                        return 1;
                                    }

                                    protected int Multiply(int value)
                                    {
                                        return value * 2;
                                    }
                                }

                                public class DerivedType : BaseType
                                {
                                    public int Run()
                                    {
                                        return {|#0:base|}.Multiply({|#1:base|}.GetValue());
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

                                     protected int Multiply(int value)
                                     {
                                         return value * 2;
                                     }
                                 }

                                 public class DerivedType : BaseType
                                 {
                                     public int Run()
                                     {
                                         return Multiply(GetValue());
                                     }
                                 }
                                 """;

        // Both qualifiers sit in one invocation expression, on its callee and inside its argument, but the two
        // rewrites replace nodes whose spans do not overlap, so the batch merges them in a single pass
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base unless a local implementation exists.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}