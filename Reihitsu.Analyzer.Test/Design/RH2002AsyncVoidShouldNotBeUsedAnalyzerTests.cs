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

    #endregion // Tests
}