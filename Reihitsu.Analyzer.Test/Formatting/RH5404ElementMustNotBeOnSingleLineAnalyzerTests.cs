using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5404ElementMustNotBeOnSingleLineAnalyzer"/> and <see cref="RH5404ElementMustNotBeOnSingleLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5404ElementMustNotBeOnSingleLineAnalyzerTests : AnalyzerTestsBase<RH5404ElementMustNotBeOnSingleLineAnalyzer, RH5404ElementMustNotBeOnSingleLineCodeFixProvider>
{
    #region Tests

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
                                internal class {|#0:TestClass|} { public void Foo() { } }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public void Foo()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing an empty single-line type converges to the canonical semicolon declaration in one pass
    /// instead of producing a braced body that would be re-flagged by the empty-type semicolon rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyTypeConvergesToSemicolonDeclaration()
    {
        const string testData = """
                                internal class {|#0:TestClass|} { }

                                """;
        const string fixedData = """
                                 internal class TestClass;

                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that the inserted line breaks match the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreaksUseDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class Other
                                {
                                }

                                internal class TestClass { }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
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

    /// <summary>
    /// Verifies that an attribute on its own line above a single-line type body is still flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAttributeOnOwnLineWithSingleLineBodyIsFlagged()
    {
        const string testData = """
                                using System;

                                [Serializable]
                                internal class {|#0:TestClass|} { }

                                """;
        const string fixedData = """
                                 using System;

                                 [Serializable]
                                 internal class TestClass;

                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute on its own line above a multi-line type body is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAttributeOnOwnLineWithMultiLineBodyIsNotFlagged()
    {
        const string testData = """
                                using System;

                                [Serializable]
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}