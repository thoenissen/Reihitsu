using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0361ElementMustNotBeOnSingleLineAnalyzer"/> and <see cref="RH0361ElementMustNotBeOnSingleLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0361ElementMustNotBeOnSingleLineAnalyzerTests : AnalyzerTestsBase<RH0361ElementMustNotBeOnSingleLineAnalyzer, RH0361ElementMustNotBeOnSingleLineCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class {|#0:TestClass|} { }
                                
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                 }
                                 
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0361ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH0361MessageFormat));
    }

    /// <summary>
    /// Verifies that record structs without braces do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPrimaryConstructorRecordStructDoesNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal readonly record struct DiffHunk(int OriginalStart, int OriginalCount, int FormattedStart, int FormattedCount, List<int> Operations);
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface declaration using the semicolon-body syntax does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterfaceSemicolonBodyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal interface IBase
                                {
                                }

                                internal interface IMySpecialInterface : IBase;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a class declaration using the semicolon-body syntax does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyClassSemicolonBodyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class BaseClass
                                {
                                }

                                internal class DerivedClass : BaseClass;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a struct declaration using the semicolon-body syntax does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStructSemicolonBodyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal interface IBase
                                {
                                }

                                internal struct MyStruct : IBase;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a record declaration using the semicolon-body syntax does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordSemicolonBodyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal interface IBase
                                {
                                }

                                internal record MyRecord : IBase;
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a record struct declaration using the semicolon-body syntax does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordStructSemicolonBodyDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal interface IBase
                                {
                                }

                                internal record struct MyRecordStruct : IBase;
                                """;

        await Verify(testData);
    }

    #endregion // Members
}