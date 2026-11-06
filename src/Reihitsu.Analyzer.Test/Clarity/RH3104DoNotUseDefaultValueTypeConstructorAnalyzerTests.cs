using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3104DoNotUseDefaultValueTypeConstructorAnalyzer"/> and <see cref="RH3104DoNotUseDefaultValueTypeConstructorCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3104DoNotUseDefaultValueTypeConstructorAnalyzerTests : BatchCodeFixTestsBase<RH3104DoNotUseDefaultValueTypeConstructorAnalyzer, RH3104DoNotUseDefaultValueTypeConstructorCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying default value type constructor with explicit type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultValueTypeConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid Run()
                                    {
                                        return new {|#0:Guid|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid Run()
                                     {
                                         return default(Guid);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default int constructor is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultIntConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run()
                                    {
                                        return new {|#0:int|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run()
                                     {
                                         return default(int);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying implicit object creation for value type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task ImplicitValueTypeCreationIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid Run()
                                    {
                                        Guid value = {|#0:new|}();
                                        return value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid Run()
                                     {
                                         Guid value = default(Guid);
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default DateTime constructor is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultDateTimeConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public DateTime Run()
                                    {
                                        return new {|#0:DateTime|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public DateTime Run()
                                     {
                                         return default(DateTime);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default custom struct constructor is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultCustomStructConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                public struct Point
                                {
                                    public int X { get; set; }
                                    public int Y { get; set; }
                                }

                                public class Test
                                {
                                    public Point Run()
                                    {
                                        return new {|#0:Point|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public struct Point
                                 {
                                     public int X { get; set; }
                                     public int Y { get; set; }
                                 }

                                 public class Test
                                 {
                                     public Point Run()
                                     {
                                         return default(Point);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying value type constructor with arguments is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task ValueTypeConstructorWithArgumentsIsNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public DateTime Run()
                                    {
                                        return new DateTime(2024, 1, 1);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying value type constructor with initializer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task ValueTypeConstructorWithInitializerIsNotReported()
    {
        const string testCode = """
                                public struct Point
                                {
                                    public int X { get; set; }
                                    public int Y { get; set; }
                                }

                                public class Test
                                {
                                    public Point Run()
                                    {
                                        return new Point { X = 1, Y = 2 };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying reference type constructor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReferenceTypeConstructorIsNotReported()
    {
        const string testCode = """
                                public class MyClass
                                {
                                }

                                public class Test
                                {
                                    public MyClass Run()
                                    {
                                        return new MyClass();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying default value type constructor in field is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultValueTypeConstructorInFieldIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    private Guid _id = new {|#0:Guid|}();
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     private Guid _id = default(Guid);
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default value type constructor in array is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultValueTypeConstructorInArrayIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid[] Run()
                                    {
                                        return new[] { new {|#0:Guid|}() };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid[] Run()
                                     {
                                         return new[] { default(Guid) };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying default value type constructor as method argument is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    [Requirement("RH3104-004")]
    public async Task DefaultValueTypeConstructorAsArgumentIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Method(Guid id)
                                    {
                                    }

                                    public void Run()
                                    {
                                        Method(new {|#0:Guid|}());
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Method(Guid id)
                                     {
                                     }

                                     public void Run()
                                     {
                                         Method(default(Guid));
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }

    /// <summary>
    /// Verifying a struct with an explicit parameterless constructor is not reported, because <c>new S()</c> runs the constructor while <c>default(S)</c> does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task StructWithExplicitParameterlessConstructorIsNotReported()
    {
        const string testCode = """
                                public struct Point
                                {
                                    public Point()
                                    {
                                        X = 1;
                                    }

                                    public int X { get; set; }
                                }

                                public class Test
                                {
                                    public Point Run()
                                    {
                                        return new Point();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying an implicit object creation for a struct with an explicit parameterless constructor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task ImplicitStructWithExplicitParameterlessConstructorIsNotReported()
    {
        const string testCode = """
                                public struct Point
                                {
                                    public Point()
                                    {
                                        X = 1;
                                    }

                                    public int X { get; set; }
                                }

                                public class Test
                                {
                                    public Point Run()
                                    {
                                        Point value = new();
                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying a struct-constrained type parameter creation is not reported, because a substituted struct may declare a parameterless constructor that <c>new T()</c> invokes but <c>default(T)</c> does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task StructConstrainedTypeParameterCreationIsNotReported()
    {
        const string testCode = """
                                public class Factory<T>
                                    where T : struct
                                {
                                    public T Create()
                                    {
                                        return new T();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying an implicit creation of a struct-constrained type parameter is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task ImplicitStructConstrainedTypeParameterCreationIsNotReported()
    {
        const string testCode = """
                                public class Factory<T>
                                    where T : struct
                                {
                                    public T Create()
                                    {
                                        T value = new();
                                        return value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies parameterless creations of a predefined value type, a struct, an enum, a nullable value type, and a
    /// qualified struct are reported on the type, and a target-typed creation on <c>new</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-003")]
    public async Task ParameterlessValueTypeCreationsAreReportedOnTheTypeOrOnNew()
    {
        const string testCode = """
                                using System;

                                struct S
                                {
                                    public int X;
                                }

                                enum E
                                {
                                    A
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var a = new {|RH3104:int|}();
                                        var b = new {|RH3104:Guid|}();
                                        S c = {|RH3104:new|}();
                                        var e = new {|RH3104:E|}();
                                        var f = new {|RH3104:int?|}();
                                        var g = new {|RH3104:System.DateTime|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 struct S
                                 {
                                     public int X;
                                 }

                                 enum E
                                 {
                                     A
                                 }

                                 class C
                                 {
                                     void M()
                                     {
                                         var a = default(int);
                                         var b = default(Guid);
                                         S c = default(S);
                                         var e = default(E);
                                         var f = default(int?);
                                         var g = default(DateTime);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a creation with arguments, with an initializer, of a struct with its own parameterless constructor,
    /// and of a type parameter is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-002")]
    public async Task CreationsThatRunCodeAreNotReported()
    {
        const string testCode = """
                                using System;

                                struct S
                                {
                                    public S()
                                    {
                                        X = 1;
                                    }

                                    public int X;
                                }

                                struct P
                                {
                                    public int X;
                                }

                                class C
                                {
                                    T M<T>() where T : struct
                                    {
                                        var a = new DateTime(1);
                                        var b = new P { X = 1 };
                                        var c = new S();
                                        return new T();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix writes <c>default</c> with the type in its shortest form that binds in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-004")]
    public async Task FixWritesDefaultWithTheShortestTypeName()
    {
        const string testCode = """
                                using System;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var a = new {|RH3104:int|}();
                                        S c = {|RH3104:new|}();
                                        var g = new {|RH3104:System.DateTime|}();
                                        var f = new {|RH3104:int?|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 struct S
                                 {
                                     public int X;
                                 }

                                 class C
                                 {
                                     void M()
                                     {
                                         var a = default(int);
                                         S c = default(S);
                                         var g = default(DateTime);
                                         var f = default(int?);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of creations that stand as statements is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationsThatStandAsStatementsAreNotReported()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    C() => new S();

                                    int P
                                    {
                                        set => new S();
                                    }

                                    void M(bool b)
                                    {
                                        new S();

                                        for (new S(); b; new S())
                                        {
                                        }

                                        Action a = () => new S();
                                        Func<Task> t = async () => new S();
                                    }

                                    void N() => new S();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies creations whose value the code uses are reported and fixed, also one assigned to a discard, one that is
    /// the expression body of a lambda that returns the value, and one that is returned
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-005")]
    public async Task CreationsWhoseValueIsUsedAreReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    S M()
                                    {
                                        _ = new {|RH3104:S|}();
                                        Func<S> f = () => new {|RH3104:S|}();

                                        return new {|RH3104:S|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 struct S
                                 {
                                     public int X;
                                 }

                                 class C
                                 {
                                     S M()
                                     {
                                         _ = default(S);
                                         Func<S> f = () => default(S);

                                         return default(S);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies creations that are the expression body of a <see langword="get"/> accessor, of a property, and of an
    /// <see langword="async"/> lambda whose task carries a value are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-001")]
    [Requirement("RH3104-005")]
    public async Task CreationsAsExpressionBodiesOfFunctionsThatReturnAValueAreReported()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    S P
                                    {
                                        get => new {|RH3104:S|}();
                                    }

                                    S Q => new {|RH3104:S|}();

                                    void M()
                                    {
                                        Func<Task<S>> t = async () => new {|RH3104:S|}();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a comment inside the argument list of a creation is reported without a code fix, because
    /// <c>default(S)</c> is written from the type alone and would delete it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-006")]
    public async Task CommentInsideTheArgumentListIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new {|RH3104:int|}(/* x */);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment between <c>new</c> and the type is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-006")]
    public async Task CommentBetweenNewAndTheTypeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var b = new /* x */ {|RH3104:int|}();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment inside the argument list of a target-typed creation is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-006")]
    public async Task CommentInsideATargetTypedCreationIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        int a = {|RH3104:new|}(/* x */);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a preprocessor directive inside a creation is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-006")]
    public async Task DirectiveInsideTheCreationIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new {|RH3104:int|}(
                                #if DEBUG
                                #endif
                                            );
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the comments in front of and after a creation do not withhold the fix and stay
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-004")]
    [Requirement("RH3104-006")]
    public async Task CommentsAroundTheCreationDoNotWithholdTheFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = /* a */ new {|RH3104:int|}() /* b */;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = /* a */ default(int) /* b */;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves a creation with a comment inside as written and reported, and fixes the other creations
    /// of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-006")]
    public async Task FixAllLeavesACreationWithACommentInsideReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new {|RH3104:int|}(/* x */);
                                        var b = new {|RH3104:int|}();
                                        var c = new {|RH3104:long|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = new {|RH3104:int|}(/* x */);
                                         var b = default(int);
                                         var c = default(long);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression of an expression statement is not reported, because <c>default(S)</c>
    /// is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionStatementIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void M()
                                    {
                                        new S();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is an initializer or an iterator of a <see langword="for"/> statement is not reported,
    /// because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsForInitializerAndIteratorIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void M(bool b)
                                    {
                                        for (new S(); b; new S())
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of a method that returns <see langword="void"/> is not reported,
    /// because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfVoidMethodIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void N() => new S();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of a local function that returns <see langword="void"/> is not
    /// reported, because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfVoidLocalFunctionIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void M()
                                    {
                                        L();

                                        void L() => new S();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of a lambda converted to <see cref="System.Action"/> is not
    /// reported, because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfActionLambdaIsNotReported()
    {
        const string testCode = """
                                using System;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void M()
                                    {
                                        Action a = () => new S();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of an <see langword="async"/> lambda converted to a <see
    /// cref="System.Func{TResult}"/> of <see cref="System.Threading.Tasks.Task"/> is not reported, because
    /// <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfAsyncTaskLambdaIsNotReported()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    void M()
                                    {
                                        Func<Task> t = async () => new S();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of an <see langword="async"/> method that returns <see
    /// cref="System.Threading.Tasks.ValueTask"/> is not reported, because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfAsyncValueTaskMethodIsNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    async ValueTask N() => new S();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of an <see langword="async"/> method that returns <see
    /// langword="void"/> is not reported, because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfAsyncVoidMethodIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    async void N() => new S();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of a constructor is not reported, because <c>default(S)</c> is
    /// no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfConstructorIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    C() => new S();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of a finalizer is not reported, because <c>default(S)</c> is no
    /// statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfFinalizerIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    ~C() => new S();
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of a <see langword="set"/> or an <see langword="init"/> accessor
    /// is not reported, because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfSetAndInitAccessorsIsNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    int P
                                    {
                                        set => new S();
                                    }

                                    int Q
                                    {
                                        init => new S();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a creation that is the expression body of an <see langword="add"/> or a <see langword="remove"/>
    /// accessor is not reported, because <c>default(S)</c> is no statement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3104-005")]
    public async Task CreationAsExpressionBodyOfEventAccessorsIsNotReported()
    {
        const string testCode = """
                                using System;

                                struct S
                                {
                                    public int X;
                                }

                                class C
                                {
                                    event Action E
                                    {
                                        add => new S();
                                        remove => new S();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public bool Run()
                                    {
                                        return new {|#0:Guid|}() == new {|#1:Guid|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public bool Run()
                                     {
                                         return default(Guid) == default(Guid);
                                     }
                                 }
                                 """;

        // Both operands of the same comparison are rewritten, so the two replacements share one parent node
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor.", 2));
    }

    #endregion // BatchCodeFixTestsBase
}