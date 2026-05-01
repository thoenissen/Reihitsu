using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0103NotImplementedExceptionShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying that NotImplementedException triggers diagnostics in methods and properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNotImplementedExceptionDiagnostics()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class RH0103
                                {
                                    public void ThrowNotImplemented()
                                    {
                                        throw new {|#0:NotImplementedException|}();
                                    }

                                    public void ThrowNotImplementedWithMessage()
                                    {
                                        throw new {|#1:NotImplementedException|}("Not yet implemented");
                                    }

                                    public int Property
                                    {
                                        get
                                        {
                                            throw new {|#2:NotImplementedException|}();
                                        }
                                    }

                                    public void ThrowArgumentException()
                                    {
                                        throw new ArgumentException("test");
                                    }

                                    public void ThrowInvalidOperationException()
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    public void NoThrow()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0103NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0103MessageFormat, 3));
    }

    #endregion // Members
}