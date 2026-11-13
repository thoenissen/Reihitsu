using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer"/> and <see cref="RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests : BatchCodeFixTestsBase<RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer, RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that empty anonymous method parentheses trigger a diagnostic and are removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("SHARED-PARENS-001")]
    [Requirement("SHARED-PARENS-002")]
    public async Task VerifyDiagnosticAndCodeFix()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    internal void Verify()
                                    {
                                        Action action = delegate{|#0:()|} { };
                                        Action<int> other = delegate(int value) { };
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      internal void Verify()
                                      {
                                          Action action = delegate { };
                                          Action<int> other = delegate(int value) { };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH3106MessageFormat));
    }

    /// <summary>
    /// Verifies empty parameter lists of plain and <c>async</c> anonymous methods that initialize delegate-typed
    /// variables are reported over the parentheses, while anonymous methods without a parameter list or with
    /// parameters are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("SHARED-PARENS-001")]
    public async Task EmptyParameterListsOfAnonymousMethodsAreReportedOverTheParentheses()
    {
        const string testCode = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate{|RH3106:()|} { };
                                        Func<Task> b = async delegate {|RH3106:()|} { await Task.Yield(); };
                                        Action c = delegate { };
                                        Action<int> d = delegate (int x) { };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix removes an empty parameter list that directly follows <c>delegate</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixRemovesTheEmptyParameterList()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate{|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to overloads with different delegate types is
    /// reported without a code fix, because without the list the call would be ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodPassedToOverloadsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Run(Action<int> a)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to a constructor initializer with two applicable
    /// constructors, and to a generic method whose type argument is inferred from it, is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodPassedToAConstructorInitializerOrAnInferredGenericMethodIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class B
                                {
                                    public B(Action a)
                                    {
                                    }

                                    public B(Action<int> a)
                                    {
                                    }
                                }

                                class D : B
                                {
                                    public D()
                                        : base(delegate {|RH3106:()|} { })
                                    {
                                    }

                                    static T Run<T>(T t) => t;

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to an overload that takes <see cref="object"/>
    /// next to one that takes a delegate with a parameter is reported without a code fix, because without the list the
    /// call would bind to the other overload
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodWhoseCallWouldBindToAnotherOverloadIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action<int> a)
                                    {
                                    }

                                    void Run(object o)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to an object creation with two applicable
    /// constructors is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodPassedToAnObjectCreationWithOverloadsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System.Threading;

                                class C
                                {
                                    Thread M()
                                    {
                                        return new Thread(delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to a target-typed object creation and to an
    /// element access, each with two applicable overloads, is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodPassedToATargetTypedCreationOrAnElementAccessWithOverloadsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    public C(Action a)
                                    {
                                    }

                                    public C(Action<int> a)
                                    {
                                    }

                                    int this[Action a] => 1;

                                    int this[Action<int> a] => 2;

                                    int M()
                                    {
                                        C c = new(delegate {|RH3106:()|} { });

                                        return c[delegate {|RH3106:()|} { }];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to the only applicable overload is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodPassedToASingleOverloadIsFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void M()
                                     {
                                         Run(delegate { });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method passed to overloads is removed where the call still
    /// binds to the same overload without it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodPassedToOverloadsThatStillBindTheSameIsFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a)
                                    {
                                    }

                                    void Run(Func<int, int> f)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a)
                                     {
                                     }

                                     void Run(Func<int, int> f)
                                     {
                                     }

                                     void M()
                                     {
                                         Run(delegate { });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method assigned to an implicitly typed variable is not
    /// reported, because <c>delegate { }</c> has no natural type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInImplicitlyTypedVariableIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = delegate () { };
                                        a();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example of anonymous methods that take their natural type is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsThatTakeTheirNaturalTypeAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static void Run(object o)
                                    {
                                    }

                                    object M(bool b)
                                    {
                                        var a = delegate () { };
                                        var e = (delegate () { });
                                        var h = new[] { delegate () { } };
                                        var f = () => delegate () { };
                                        var k = () =>
                                        {
                                            return delegate () { };
                                        };
                                        Delegate d = delegate () { };
                                        object o = delegate () { };
                                        Run(delegate () { });
                                        return delegate () { };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method assigned to a variable of type <see cref="object"/> and cast to
    /// <see cref="System.Delegate"/> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodAssignedToObjectOrCastToDelegateIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    object _o;

                                    void M()
                                    {
                                        _o = delegate () { };
                                        var d = (Delegate)delegate () { };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method whose target is a delegate type through a conditional
    /// expression is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodWithADelegateTargetThroughAConditionalIsReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b)
                                    {
                                        Action a = b ? delegate {|RH3106:()|} { } : null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(bool b)
                                     {
                                         Action a = b ? delegate { } : null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method assigned or returned where the type is a delegate type
    /// is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    public async Task AnonymousMethodAssignedOrReturnedAsADelegateIsReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    Action _a;

                                    Action M()
                                    {
                                        _a = delegate {|RH3106:()|} { };

                                        return delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a comment inside the empty parameter list is reported without a code fix, because the fix would
    /// delete it with the parentheses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-003")]
    public async Task CommentInsideTheParameterListIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate {|RH3106:( /* none */ )|} { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a preprocessor directive inside the empty parameter list is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-003")]
    public async Task DirectiveInsideTheParameterListIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate {|RH3106:(
                                #if DEBUG
                                #endif
                                            )|} { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies a comment in front of the parameter list does not withhold the fix and stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    [Requirement("SHARED-PARENS-003")]
    public async Task CommentInFrontOfTheParameterListDoesNotWithholdTheFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate /* a */ {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate /* a */ { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All leaves a parameter list with a comment inside as written and reported, and removes the other
    /// empty parameter lists of the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-003")]
    public async Task FixAllLeavesAParameterListWithACommentInsideReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate {|RH3106:( /* none */ )|} { };
                                        Action b = delegate {|RH3106:()|} { };
                                        Action c = delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate {|RH3106:( /* none */ )|} { };
                                         Action b = delegate { };
                                         Action c = delegate { };
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

    /// <summary>
    /// Verifies the fix leaves one space between <c>delegate</c> and the body where whitespace stood on both sides of
    /// the removed parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixLeavesOneSpaceBetweenDelegateAndTheBody()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix leaves one space between <c>delegate</c> and the body in a document whose line breaks are
    /// CRLF, and keeps those line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixLeavesOneSpaceBetweenDelegateAndTheBodyWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         using System;

                                                         class C
                                                         {
                                                             void M()
                                                             {
                                                                 Action a = delegate () { };
                                                             }
                                                         }
                                                         """);

        var expectedCode = NormalizeToCarriageReturnLineFeed("""
                                                             using System;

                                                             class C
                                                             {
                                                                 void M()
                                                                 {
                                                                     Action a = delegate { };
                                                                 }
                                                             }
                                                             """);

        Assert.AreEqual(expectedCode, await ApplyCodeFixAsync(testCode));
    }

    /// <summary>
    /// Verifies the fix keeps the line break in front of a body on the next line and leaves no whitespace behind
    /// <c>delegate</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-PARENS-002")]
    public async Task FixKeepsTheLineBreakInFrontOfABodyOnTheNextLine()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate {|RH3106:()|}
                                        {
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate
                                         {
                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method inside parentheses that initialize an implicitly typed
    /// variable is not reported, because the parentheses pass the position on to the anonymous method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task ParenthesizedAnonymousMethodInImplicitlyTypedVariableIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var e = (delegate () { });
                                        e();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is an element of an implicitly typed array creation
    /// is not reported, because the array takes its element type from the anonymous method's natural type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayCreationIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var h = new[] { delegate () { } };
                                        h[0]();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the expression body of a lambda without a
    /// delegate target is not reported, because the lambda infers its return type from the anonymous method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByTheExpressionBodyOfALambdaWithoutATargetIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var f = () => delegate () { };
                                        f()();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method returned by a <see langword="return"/> statement of a
    /// lambda without a delegate target is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByAReturnStatementOfALambdaWithoutATargetIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var k = () =>
                                        {
                                            return delegate () { };
                                        };
                                        k()();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method in a branch of a conditional expression or an arm of a
    /// switch expression whose target is <see cref="object"/> is not reported, because the conditional and the switch
    /// expression pass their position on to the anonymous method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAConditionalOrASwitchArmWithAnObjectTargetIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    object M(bool b, int i)
                                    {
                                        object g = b ? delegate () { } : delegate () { };

                                        return i switch
                                               {
                                                   1 => delegate () { },
                                                   _ => (delegate () { })
                                               };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method inside parentheses, as an element of an array creation
    /// that names its element type, and returned by a lambda whose target is a delegate type is reported and removed,
    /// because a delegate type is its target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodWithADelegateTargetThroughParenthesesAnArrayOrALambdaIsReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action e = (delegate {|RH3106:()|} { });
                                        Action[] h = new Action[] { delegate {|RH3106:()|} { } };
                                        Func<Action> f = () => delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action e = (delegate { });
                                         Action[] h = new Action[] { delegate { } };
                                         Func<Action> f = () => delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of anonymous methods whose conditional, switch expression, or implicitly typed
    /// array creation takes the delegate type of another branch, arm, or element under <see langword="var"/> is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsWithADelegateTypedOtherBranchArmOrElementUnderVarAreReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b, int i, Action someAction)
                                    {
                                        var x = b ? delegate {|RH3106:()|} { } : someAction;
                                        var y = i switch
                                                {
                                                    1 => delegate {|RH3106:()|} { },
                                                    _ => someAction
                                                };
                                        var z = new[] { delegate {|RH3106:()|} { }, someAction };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method in a conditional expression under
    /// <see langword="var"/> whose other branch is of a delegate type is reported and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAConditionalWithADelegateTypedOtherBranchUnderVarIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b, Action someAction)
                                    {
                                        var x = b ? delegate {|RH3106:()|} { } : someAction;
                                        x();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(bool b, Action someAction)
                                     {
                                         var x = b ? delegate { } : someAction;
                                         x();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method in an arm of a switch expression under
    /// <see langword="var"/> whose other arm is of a delegate type is reported and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInASwitchArmWithADelegateTypedOtherArmUnderVarIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(int i, Action someAction)
                                    {
                                        var y = i switch
                                                {
                                                    1 => delegate {|RH3106:()|} { },
                                                    _ => someAction
                                                };
                                        y();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(int i, Action someAction)
                                     {
                                         var y = i switch
                                                 {
                                                     1 => delegate { },
                                                     _ => someAction
                                                 };
                                         y();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method in an implicitly typed array creation under
    /// <see langword="var"/> whose other element is of a delegate type is reported and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWithADelegateTypedOtherElementUnderVarIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(Action someAction)
                                    {
                                        var z = new[] { delegate {|RH3106:()|} { }, someAction };
                                        z[0]();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(Action someAction)
                                     {
                                         var z = new[] { delegate { }, someAction };
                                         z[0]();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method in a conditional expression, a switch expression, or an
    /// implicitly typed array creation whose other branch, arm, or element is not of a delegate type is not reported,
    /// because the anonymous method then takes its natural type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodWhoseOtherBranchArmOrElementIsNotOfADelegateTypeIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    object M(bool b, int i)
                                    {
                                        object x = b ? delegate () { } : new object();
                                        var z = new[] { delegate () { }, new object() };

                                        return i switch
                                               {
                                                   1 => delegate () { },
                                                   _ => new object()
                                               };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the right operand of a binary operator with two
    /// user-defined operators is reported without a code fix, because without the list the operator would be ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodAsTheRightOperandOfCompetingUserDefinedOperatorsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    public static C operator +(C c, Action a) => c;

                                    public static C operator +(C c, Action<int> a) => c;

                                    void M(C c)
                                    {
                                        _ = c + delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the left operand of a binary operator with two
    /// user-defined operators is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodAsTheLeftOperandOfCompetingUserDefinedOperatorsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    public static C operator +(Action a, C c) => c;

                                    public static C operator +(Action<int> a, C c) => c;

                                    void M(C c)
                                    {
                                        _ = delegate {|RH3106:()|} { } + c;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is an operand of a binary operator with one
    /// user-defined operator is removed, because the operator still binds the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodAsAnOperandOfASingleUserDefinedOperatorIsFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    public static C operator +(C c, Action a) => c;

                                    void M(C c)
                                    {
                                        _ = c + delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     public static C operator +(C c, Action a) => c;

                                     void M(C c)
                                     {
                                         _ = c + delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies Fix All decides each removal on the code the fixes in front of it produce: of two arguments that each
    /// pin the overload alone, the first list is removed and the second stays reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllLeavesTheArgumentThatWouldMakeTheCallAmbiguousReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a, Action b)
                                    {
                                    }

                                    void Run(Action<int> a, Action<int> b)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { }, delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a, Action b)
                                     {
                                     }

                                     void Run(Action<int> a, Action<int> b)
                                     {
                                     }

                                     void M()
                                     {
                                         Run(delegate { }, delegate {|RH3106:()|} { });
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostic whose fix is withheld once the fixes in front of it have run stays and is marked
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies Fix All removes both lists of two arguments when the call still binds the same without both
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllRemovesTheListsOfArgumentsThatTogetherKeepTheBinding()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void Run(Action a, Action b)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(delegate {|RH3106:()|} { }, delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void Run(Action a, Action b)
                                     {
                                     }

                                     void M()
                                     {
                                         Run(delegate { }, delegate { });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the value of a compound assignment with two
    /// competing <see langword="static"/> user-defined operators is reported without a code fix, because without the
    /// list the compound assignment would be ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CompoundAssignmentWithCompetingStaticOperatorsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    public static C operator +(C c, Action a) => c;

                                    public static C operator +(C c, Action<int> a) => c;

                                    void M(C c)
                                    {
                                        c += delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the value of a compound assignment with one
    /// <see langword="static"/> user-defined operator is removed, because the compound assignment still binds the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CompoundAssignmentWithASingleStaticOperatorIsFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    public static C operator +(C c, Action a) => c;

                                    void M(C c)
                                    {
                                        c += delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     public static C operator +(C c, Action a) => c;

                                     void M(C c)
                                     {
                                         c += delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the value of a compound assignment with two
    /// competing instance compound assignment operators is reported without a code fix, because without the list the
    /// call of the operator would be ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CompoundAssignmentWithCompetingInstanceCompoundAssignmentOperatorsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class D
                                {
                                    public void operator +=(Action a)
                                    {
                                    }

                                    public void operator +=(Action<int> a)
                                    {
                                    }

                                    void M(D d)
                                    {
                                        d += delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is the value of a compound assignment with one
    /// instance compound assignment operator is removed, because the compound assignment still binds the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CompoundAssignmentWithASingleInstanceCompoundAssignmentOperatorIsFixed()
    {
        const string testCode = """
                                using System;

                                class D
                                {
                                    public void operator +=(Action a)
                                    {
                                    }

                                    void M(D d)
                                    {
                                        d += delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class D
                                 {
                                     public void operator +=(Action a)
                                     {
                                     }

                                     void M(D d)
                                     {
                                         d += delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is an element of a collection initializer with two
    /// competing <c>Add</c> methods is reported without a code fix, because without the list the call of <c>Add</c>
    /// would be ambiguous
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CollectionInitializerElementWithCompetingAddMethodsIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;
                                using System.Collections;

                                class Coll : IEnumerable
                                {
                                    public void Add(Action a)
                                    {
                                    }

                                    public void Add(Action<int> a)
                                    {
                                    }

                                    public IEnumerator GetEnumerator() => throw new NotSupportedException();
                                }

                                class E
                                {
                                    void M()
                                    {
                                        var e = new Coll { delegate {|RH3106:()|} { } };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is an element of a collection initializer with one
    /// <c>Add</c> method is removed, because the collection initializer still binds the same
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CollectionInitializerElementWithASingleAddMethodIsFixed()
    {
        const string testCode = """
                                using System;
                                using System.Collections;

                                class Coll : IEnumerable
                                {
                                    public void Add(Action a)
                                    {
                                    }

                                    public IEnumerator GetEnumerator() => throw new NotSupportedException();
                                }

                                class E
                                {
                                    void M()
                                    {
                                        var e = new Coll { delegate {|RH3106:()|} { } };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections;

                                 class Coll : IEnumerable
                                 {
                                     public void Add(Action a)
                                     {
                                     }

                                     public IEnumerator GetEnumerator() => throw new NotSupportedException();
                                 }

                                 class E
                                 {
                                     void M()
                                     {
                                         var e = new Coll { delegate { } };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of the compound assignments and the collection initializer whose binding
    /// competing operators or <c>Add</c> methods decide: every list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task CompoundAssignmentsAndACollectionInitializerWithCompetingCandidatesAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;
                                using System.Collections;

                                class C
                                {
                                    public static C operator +(C c, Action a) => c;

                                    public static C operator +(C c, Action<int> a) => c;
                                }

                                class D
                                {
                                    public void operator +=(Action a)
                                    {
                                    }

                                    public void operator +=(Action<int> a)
                                    {
                                    }
                                }

                                class Coll : IEnumerable
                                {
                                    public void Add(Action a)
                                    {
                                    }

                                    public void Add(Action<int> a)
                                    {
                                    }

                                    public IEnumerator GetEnumerator() => throw new NotSupportedException();
                                }

                                class E
                                {
                                    void M(C c, D d)
                                    {
                                        c += delegate {|RH3106:()|} { };
                                        d += delegate {|RH3106:()|} { };
                                        var e = new Coll { delegate {|RH3106:()|} { } };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method inside a lambda passed to a non-generic method with one
    /// overload is removed, because the lambda's delegate type gives the anonymous method its target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-002")]
    public async Task AnonymousMethodReturnedByALambdaPassedToASingleNonGenericOverloadIsFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static Action Run(Func<Action> f) => f();

                                    void M()
                                    {
                                        Run(() => delegate {|RH3106:()|} { });
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     static Action Run(Func<Action> f) => f();

                                     void M()
                                     {
                                         Run(() => delegate { });
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of an anonymous method that another anonymous method under <c>var</c>
    /// returns, and of an element of a collection expression whose elements are <see cref="object"/>: neither is
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByAnAnonymousMethodOrInACollectionExpressionOfObjectElementsIsNotReported()
    {
        const string testCode = """
                                using System;
                                using System.Collections;

                                class Coll : IEnumerable
                                {
                                    public void Add(Action a)
                                    {
                                    }

                                    public void Add(Action<int> a)
                                    {
                                    }

                                    public IEnumerator GetEnumerator() => throw new NotSupportedException();
                                }

                                class C
                                {
                                    void M()
                                    {
                                        var n = delegate () { return delegate () { }; };
                                        Coll e = [delegate () { }];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method that one of several <see langword="return"/> statements of an anonymous method under
    /// <c>var</c> returns is not reported, because the outer anonymous method's return type is inferred from it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByOneOfSeveralReturnsOfAnAnonymousMethodUnderVarIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var n = delegate (int x)
                                                {
                                                    if (x > 0)
                                                    {
                                                        return delegate () { };
                                                    }

                                                    return null;
                                                };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method that another anonymous method returns is reported and fixed when the outer
    /// anonymous method's target is a delegate type, which gives the inner one its target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByAnAnonymousMethodWithADelegateTargetIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<Action> n = delegate {|RH3106:()|} { return delegate {|RH3106:()|} { }; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Func<Action> n = delegate { return delegate { }; };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that is an element of a collection expression whose
    /// element type is a delegate type is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInACollectionExpressionOfDelegateElementsIsReportedAndFixed()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        List<Action> l = [delegate {|RH3106:()|} { }];
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     void M()
                                     {
                                         List<Action> l = [delegate { }];
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example of anonymous methods whose target is a delegate type, also where a lambda
    /// declares its return type: every list is reported and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsWithADelegateTargetAlsoFromADeclaredLambdaReturnTypeAreReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b)
                                    {
                                        Action a = b ? delegate {|RH3106:()|} { } : null;
                                        Action e = (delegate {|RH3106:()|} { });
                                        Action[] h = new Action[] { delegate {|RH3106:()|} { } };
                                        Func<Action> f = () => delegate {|RH3106:()|} { };
                                        var g = Action () => delegate {|RH3106:()|} { };
                                        var k = Action () =>
                                        {
                                            return delegate {|RH3106:()|} { };
                                        };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(bool b)
                                     {
                                         Action a = b ? delegate { } : null;
                                         Action e = (delegate { });
                                         Action[] h = new Action[] { delegate { } };
                                         Func<Action> f = () => delegate { };
                                         var g = Action () => delegate { };
                                         var k = Action () =>
                                         {
                                             return delegate { };
                                         };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that a lambda with a declared return type returns as its
    /// expression body is reported and removed under <c>var</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByTheExpressionBodyOfALambdaWithADeclaredReturnTypeIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        var f = Action () => delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         var f = Action () => delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method that a lambda with a declared return type returns is
    /// reported and removed where the lambda is converted to <see cref="object"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByALambdaWithADeclaredReturnTypeUnderObjectIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        object o = Action () => delegate {|RH3106:()|} { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         object o = Action () => delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an empty parameter list of an anonymous method in a conditional expression whose other branch is of a
    /// delegate type is reported and removed where the conditional is converted to <see cref="object"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAConditionalWithADelegateTypedOtherBranchUnderObjectIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b, Action someAction)
                                    {
                                        object o = b ? delegate {|RH3106:()|} { } : someAction;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(bool b, Action someAction)
                                     {
                                         object o = b ? delegate { } : someAction;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside an <see langword="if"/> block is not reported, also where a
    /// later statement of the block calls the local whose type follows the anonymous method's natural type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAnIfBlockWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (b)
                                        {
                                            var a = delegate () { };
                                            a();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside a <see langword="using"/> block is not reported, also where
    /// a later statement of the block converts the local to <see cref="System.Action"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAUsingBlockWithALaterUseOfTheLocalIsNotReported()
    {
        const string testCode = """
                                using System;
                                using System.IO;

                                class C
                                {
                                    void M()
                                    {
                                        using (var s = new MemoryStream())
                                        {
                                            var a = delegate () { };
                                            Action b = a;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array under <c>var</c> inside a <see langword="foreach"/>
    /// block is not reported, also where a later statement of the block calls an element of the array
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayInAForeachBlockWithALaterCallOfAnElementIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] items)
                                    {
                                        foreach (var i in items)
                                        {
                                            var arr = new[] { delegate () { } };
                                            arr[0]();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method that the expression body of a lambda under <c>var</c> returns inside an
    /// <see langword="if"/> block is not reported, also where a later statement of the block calls the result of the
    /// lambda
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodReturnedByALambdaUnderVarInAnIfBlockWithALaterCallIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (b)
                                        {
                                            var g = () => delegate () { };
                                            g()();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> in the block body of a lambda is not reported, also where a later
    /// statement of the body calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInALambdaBodyWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action x = () =>
                                                   {
                                                       var a = delegate () { };
                                                       a();
                                                   };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> in the body of another anonymous method is not reported, also
    /// where a later statement of the body calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAnAnonymousMethodBodyWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action x = delegate
                                                   {
                                                       var a = delegate () { };
                                                       a();
                                                   };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> in the body of a local function is not reported, also where a
    /// later statement of the body calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInALocalFunctionBodyWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        L();

                                        void L()
                                        {
                                            var a = delegate () { };
                                            a();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside an <see langword="if"/> block of top-level statements is
    /// not reported, also where a later statement of the block calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAnIfBlockOfTopLevelStatementsWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                if (args.Length > 0)
                                {
                                    var a = delegate () { };
                                    a();
                                }
                                """;

        await Verify(testCode, static test => test.TestState.OutputKind = OutputKind.ConsoleApplication);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside an <see langword="if"/> block of an async method is not
    /// reported, also where a later statement of the block calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAnIfBlockOfAnAsyncMethodWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task M(bool b)
                                    {
                                        await Task.Yield();

                                        if (b)
                                        {
                                            var a = delegate () { };
                                            a();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside an <see langword="if"/> block of an iterator method is not
    /// reported, also where a later statement of the block calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAnIfBlockOfAnIteratorMethodWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    IEnumerable<int> M(bool b)
                                    {
                                        if (b)
                                        {
                                            var a = delegate () { };
                                            a();
                                        }

                                        yield return 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside a labeled block of a method with <see langword="goto"/> is
    /// not reported, also where a later statement of the block calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInALabeledBlockOfAGotoMethodWithALaterCallOfTheLocalIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int n)
                                    {
                                    Start:
                                        {
                                            var a = delegate () { };
                                            a();
                                        }

                                        if (--n > 0)
                                        {
                                            goto Start;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> inside an <see langword="if"/> block is not reported where no
    /// later statement uses the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAnIfBlockWithoutALaterUseIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (b)
                                        {
                                            var a = delegate () { };
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method assigned to <see cref="object"/> inside an <see langword="if"/> block is not
    /// reported, also where a later statement of the block casts the local to <see cref="System.Action"/> and calls it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderObjectInAnIfBlockWithALaterCastAndCallIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (b)
                                        {
                                            object o = delegate () { };
                                            ((Action)o)();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method with a delegate type as its target inside an <see langword="if"/> block is reported
    /// and fixed, also where a later statement of the block calls the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-001")]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodWithADelegateTargetInAnIfBlockWithALaterCallOfTheLocalIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M(bool b)
                                    {
                                        if (b)
                                        {
                                            Action a = delegate {|RH3106:()|} { };
                                            a();
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M(bool b)
                                     {
                                         if (b)
                                         {
                                             Action a = delegate { };
                                             a();
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> in the initializer of a <see langword="for"/> statement is not
    /// reported, also where the condition and the incrementor of the statement use the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAForInitializerWithAUseInTheConditionAndIncrementorIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        for (var a = delegate () { }; a != null; a())
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> in the initializer of a <see langword="for"/> statement in the
    /// block body of a lambda is not reported, also where the condition and the incrementor of the statement use the
    /// local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAForInitializerInALambdaBodyWithAUseInTheConditionAndIncrementorIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action x = () =>
                                                   {
                                                       for (var a = delegate () { }; a != null; a())
                                                       {
                                                       }
                                                   };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array under <c>var</c> in the initializer of a
    /// <see langword="for"/> statement is not reported, also where the condition and the incrementor of the statement
    /// use the array
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayInAForInitializerWithAUseInTheConditionAndIncrementorIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        for (var arr = new[] { delegate () { } }; arr.Length > 0; arr[0]())
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example of anonymous methods under <c>var</c> in the initializers of
    /// <see langword="for"/> statements, directly in a method body, in a lambda body, and in an implicitly typed array:
    /// none is reported, although the condition and the incrementor of each statement use the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsUnderVarInForInitializersOfTheSpecificationExampleAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        for (var a = delegate () { }; a != null; a()) { }
                                        Action f = () =>
                                        {
                                            for (var b = delegate () { }; b != null; b()) { }
                                        };
                                        for (var arr = new[] { delegate () { } }; arr.Length > 0; arr[0]()) { }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method under <c>var</c> in the initializer of a <see langword="for"/> statement is not
    /// reported where only the body of the statement uses the local
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodUnderVarInAForInitializerWithAUseOnlyInTheBodyIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool b)
                                    {
                                        for (var a = delegate () { }; b;)
                                        {
                                            a();
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example of the deliberate gap: an element of an implicitly typed array creation whose
    /// delegate is invoked after an element access in the same expression, directly or through <c>Invoke</c>, is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsInImplicitlyTypedArraysWhoseElementIsInvokedInTheSameExpressionAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        var r = new[] { delegate {|RH3106:()|} { return 1; } }[0]();
                                        new[] { delegate {|RH3106:()|} { } }[0]();
                                        new[] { delegate {|RH3106:()|} { } }[0].Invoke();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an element of an implicitly typed array creation whose delegate is invoked after an element access,
    /// with its result assigned to a local, is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementResultIsAssignedIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var r = new[] { delegate {|RH3106:()|} { return 1; } }[0]();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an element of an implicitly typed array creation whose delegate is invoked after an element access as
    /// an expression statement is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedAsAStatementIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        new[] { delegate {|RH3106:()|} { } }[0]();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an element of an implicitly typed array creation whose delegate is invoked through <c>Invoke</c> after
    /// an element access is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedThroughInvokeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        new[] { delegate {|RH3106:()|} { } }[0].Invoke();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of implicitly typed array creations used without invoking an element in the
    /// same expression: the length, an element's <c>Method</c>, and a type test on an element do not keep the anonymous
    /// method from taking its natural type, and none is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsInImplicitlyTypedArraysUsedWithoutInvokingAnElementAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        var n = new[] { delegate () { } }.Length;
                                        var m = new[] { delegate () { } }[0].Method;
                                        var t = new[] { delegate () { } }[0] is Action;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element invoked through
    /// <c>BeginInvoke</c>: the empty parameter list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedThroughBeginInvokeIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        new[] { delegate {|RH3106:()|} { } }[0].BeginInvoke(null, null);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element that initializes a local of
    /// type <c>Action</c>: the empty parameter list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesADelegateTypedLocalIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action x = new[] { delegate {|RH3106:()|} { } }[0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element that initializes a local of
    /// type <c>Func&lt;int&gt;</c>: the empty parameter list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesAFuncTypedLocalIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int> g = new[] { delegate {|RH3106:()|} { return 1; } }[0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element assigned to a local of type
    /// <c>Action</c>: the empty parameter list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsAssignedToADelegateTypedLocalIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action y;
                                        y = new[] { delegate {|RH3106:()|} { } }[0];
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element passed as the argument of
    /// the only overload, which takes an <c>Action</c>: the empty parameter list is reported, and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsPassedAsADelegateArgumentIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static void Run(Action a)
                                    {
                                    }

                                    void M()
                                    {
                                        Run(new[] { delegate {|RH3106:()|} { } }[0]);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an array creation that initializes a
    /// local of type <c>Action[]</c>: the empty parameter list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayThatInitializesADelegateArrayLocalIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action[] arr = new[] { delegate {|RH3106:()|} { } };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an array creation that initializes a
    /// local of type <c>IEnumerable&lt;Action&gt;</c>: the empty parameter list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayThatInitializesADelegateCollectionLocalIsReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        IEnumerable<Action> e = new[] { delegate {|RH3106:()|} { } };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element that is the receiver of
    /// <c>DynamicInvoke()</c>: the anonymous method takes its natural type, and nothing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedThroughDynamicInvokeIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = new[] { delegate () { } }[0].DynamicInvoke();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element that initializes a local of
    /// type <c>object</c>: the anonymous method takes its natural type, and nothing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesAnObjectLocalIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        object o = new[] { delegate () { } }[0];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an anonymous method in an implicitly typed array creation, with an element that initializes a local of
    /// type <c>System.Delegate</c>: the anonymous method takes its natural type, and nothing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesADelegateLocalIsNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Delegate d = new[] { delegate () { } }[0];
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example of the deliberate gap: anonymous methods in implicitly typed array creations
    /// whose element is invoked, or whose array or element is converted to a delegate type or a collection of
    /// delegates, are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsInImplicitlyTypedArraysOfTheDeliberateGapAreReportedWithoutACodeFix()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    static void Run(Action a)
                                    {
                                    }

                                    void M()
                                    {
                                        var r = new[] { delegate {|RH3106:()|} { return 1; } }[0]();
                                        new[] { delegate {|RH3106:()|} { } }[0]();
                                        new[] { delegate {|RH3106:()|} { } }[0].Invoke();
                                        new[] { delegate {|RH3106:()|} { } }[0].BeginInvoke(null, null);
                                        Action x = new[] { delegate {|RH3106:()|} { } }[0];
                                        Func<int> g = new[] { delegate {|RH3106:()|} { return 1; } }[0];
                                        Action y;
                                        y = new[] { delegate {|RH3106:()|} { } }[0];
                                        Run(new[] { delegate {|RH3106:()|} { } }[0]);
                                        Action[] arr = new[] { delegate {|RH3106:()|} { } };
                                        IEnumerable<Action> e = new[] { delegate {|RH3106:()|} { } };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the specification example of the shapes of the deliberate gap that are not reported: the length, an
    /// element's <c>Method</c>, <c>DynamicInvoke()</c>, a type test, an <c>object</c> or <c>System.Delegate</c> local,
    /// and an array with an element that is not a delegate
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3106-003")]
    public async Task AnonymousMethodsInImplicitlyTypedArraysOfTheNotReportedShapesAreNotReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        var n = new[] { delegate () { } }.Length;
                                        var m = new[] { delegate () { } }[0].Method;
                                        var t = new[] { delegate () { } }[0] is Action;
                                        var v = new[] { delegate () { } }[0].DynamicInvoke();
                                        object o = new[] { delegate () { } }[0];
                                        Delegate d = new[] { delegate () { } }[0];
                                        var z = new[] { delegate () { }, new object() };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that removing the empty parameter list of an anonymous method behind a space leaves one space between the
    /// keyword and the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixLeavesOneSpaceBetweenTheKeywordAndTheBlock()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = delegate {|RH3106:()|} { };
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a = delegate { };
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    internal void Verify()
                                    {
                                        Action outer = delegate{|#0:()|}
                                        {
                                            Action inner = delegate{|#1:()|} { };
                                        };
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      internal void Verify()
                                      {
                                          Action outer = delegate
                                          {
                                              Action inner = delegate { };
                                          };
                                      }
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.DiagnosticId, AnalyzerResources.RH3106MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}