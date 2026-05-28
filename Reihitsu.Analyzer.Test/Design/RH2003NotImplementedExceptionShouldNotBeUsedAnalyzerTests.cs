using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer>
{
    #region Tests

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

                                internal class RH2003
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

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat, 3));
    }

    #endregion // Tests
}