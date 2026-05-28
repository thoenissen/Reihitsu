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

    #endregion // Tests
}