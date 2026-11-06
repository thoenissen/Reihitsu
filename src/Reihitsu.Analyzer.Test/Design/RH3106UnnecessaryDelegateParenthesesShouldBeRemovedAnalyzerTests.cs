using System.Threading.Tasks;

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