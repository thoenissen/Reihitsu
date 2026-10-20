using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2002AsyncVoidShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2002AsyncVoidShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2002AsyncVoidShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that async void methods trigger diagnostics in various contexts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-001")]
    [Requirement("RH2002-004")]
    [Requirement("RH2002-005")]
    public async Task VerifyAsyncVoidDiagnostics()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class RH2002
                                {
                                    public async Task ValidAsyncTask()
                                    {
                                        await Task.Delay(10);
                                    }

                                    public async void {|#0:InvalidAsyncVoid|}()
                                    {
                                        await Task.Delay(10);
                                    }

                                    private async void {|#1:OnClick|}(object sender, EventArgs e)
                                    {
                                        await Task.Delay(10);
                                    }

                                    public void NormalVoid()
                                    {
                                    }

                                    public async Task<int> ValidAsyncTaskWithReturn()
                                    {
                                        await Task.Delay(10);

                                        return 42;
                                    }
                                }

                                public class BaseClass
                                {
                                    public virtual async void {|#2:DoSomething|}() => await Task.Delay(10);
                                }

                                public class DerivedClass : BaseClass
                                {
                                    public override async void {|#3:DoSomething|}() => await Task.Delay(10);
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that an async void local function triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-002")]
    [Requirement("RH2002-005")]
    public async Task VerifyAsyncVoidLocalFunctionIsFlagged()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                internal class RH2002
                                {
                                    public void Execute()
                                    {
                                        async void {|#0:LocalFunction|}()
                                        {
                                            await Task.Delay(10);
                                        }

                                        LocalFunction();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat));
    }

    /// <summary>
    /// Verifies that an async Task local function does not trigger a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-004")]
    public async Task VerifyAsyncTaskLocalFunctionIsNotFlagged()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                internal class RH2002
                                {
                                    public async Task Execute()
                                    {
                                        async Task LocalFunction()
                                        {
                                            await Task.Delay(10);
                                        }

                                        await LocalFunction();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an async void lambda assigned to an <see cref="System.Action"/> triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-003")]
    [Requirement("RH2002-005")]
    public async Task VerifyAsyncVoidLambdaIsFlagged()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class RH2002
                                {
                                    public void Execute()
                                    {
                                        Action action = {|#0:async|} () => await Task.Delay(10);

                                        action();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat));
    }

    /// <summary>
    /// Verifies that an async void anonymous method assigned to an <see cref="System.Action"/> triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-003")]
    [Requirement("RH2002-005")]
    public async Task VerifyAsyncVoidAnonymousMethodIsFlagged()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class RH2002
                                {
                                    public void Execute()
                                    {
                                        Action action = {|#0:async|} delegate
                                        {
                                            await Task.Delay(10);
                                        };

                                        action();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat));
    }

    /// <summary>
    /// Verifies that an async lambda assigned to a <see cref="System.Func{TResult}"/> returning a task does not
    /// trigger a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-003")]
    public async Task VerifyAsyncTaskLambdaIsNotFlagged()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                internal class RH2002
                                {
                                    public void Execute()
                                    {
                                        Func<Task> func = async () => await Task.Delay(10);

                                        func();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that static, event-handler, virtual, and override async void methods are reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-001")]
    [Requirement("RH2002-005")]
    public async Task VerifyStaticEventHandlerVirtualAndOverrideAsyncVoidMethodsAreReported()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    async void {|#0:Run|}()
                                    {
                                        await Task.Delay(1);
                                    }

                                    static async void {|#1:Start|}()
                                    {
                                        await Task.Delay(1);
                                    }

                                    async void {|#2:OnClick|}(object sender, EventArgs e)
                                    {
                                        await Task.Delay(1);
                                    }

                                    public virtual async void {|#3:Load|}() => await Task.Delay(1);
                                }

                                class D : C
                                {
                                    public override async void {|#4:Load|}() => await Task.Delay(1);
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat, 5));
    }

    /// <summary>
    /// Verifying that async anonymous functions converted to void-returning delegates are reported on their
    /// <see langword="async"/> keyword, and that those converted to or inferred as task-returning delegates are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-003")]
    [Requirement("RH2002-005")]
    public async Task VerifyAsyncAnonymousFunctionsAreReportedOnlyForVoidDelegates()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    void M()
                                    {
                                        Action a = {|#0:async|} () => await Task.Delay(1);
                                        Action<int> b = {|#1:async|} x => await Task.Delay(x);
                                        EventHandler c = {|#2:async|} (sender, e) => await Task.Delay(1);
                                        Action d = {|#3:async|} delegate
                                        {
                                            await Task.Delay(1);
                                        };
                                        Func<Task> e = async () => await Task.Delay(1);
                                        var f = async () => await Task.Delay(1);
                                        Func<int, Task<int>> g = async x => await Task.FromResult(x);
                                        Task.Run(async () => await Task.Delay(1));
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that async functions returning <see cref="ValueTask"/> and synchronous void functions are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2002-004")]
    public async Task VerifyAsyncValueTaskAndSynchronousVoidFunctionsAreNotReported()
    {
        const string testData = """
                                using System;
                                using System.Threading.Tasks;

                                class C
                                {
                                    async Task Run()
                                    {
                                        await Task.Delay(1);
                                    }

                                    async Task<int> Count()
                                    {
                                        await Task.Delay(1);

                                        return 1;
                                    }

                                    async ValueTask Wait()
                                    {
                                        await Task.Delay(1);
                                    }

                                    void Plain()
                                    {
                                        void Local()
                                        {
                                        }

                                        Action a = () => Local();
                                        Action b = delegate { Local(); };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes an async void explicit interface implementation: it is not reported, while the implicit
    /// implementation is. The case is an open question about intended behavior, so this test pins today's behavior
    /// without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncVoidExplicitInterfaceImplementationIsNotReported()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                interface I
                                {
                                    void M();

                                    void N();
                                }

                                class C : I
                                {
                                    async void I.M()
                                    {
                                        await Task.Delay(1);
                                    }

                                    public async void {|#0:N|}()
                                    {
                                        await Task.Delay(1);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat));
    }

    /// <summary>
    /// Characterizes an async void partial method: it is reported twice, on the defining declaration without
    /// <see langword="async"/> and on the implementing declaration. The case is an open question about intended
    /// behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAsyncVoidPartialMethodIsReportedOnBothDeclarations()
    {
        const string testData = """
                                using System.Threading.Tasks;

                                partial class P
                                {
                                    partial void {|#0:Q|}();

                                    async partial void {|#1:Q|}()
                                    {
                                        await Task.Delay(1);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2002AsyncVoidShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2002MessageFormat, 2));
    }

    #endregion // Tests
}