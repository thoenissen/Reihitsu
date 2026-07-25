using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8107VoidReturnValueMustNotBeDocumentedAnalyzer"/> and
/// <see cref="RH8107VoidReturnValueMustNotBeDocumentedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8107VoidReturnValueMustNotBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8107VoidReturnValueMustNotBeDocumentedAnalyzer, RH8107VoidReturnValueMustNotBeDocumentedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a void member with a returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForVoidReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<returns>Nothing.</returns>|}
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>Runs the method.</summary>
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps a summary tag which shares the line with the returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixKeepsSummaryTagSharingTheLine()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>{|#0:<returns>Nothing.</returns>|}
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>Runs the method.</summary>
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps the end tag of a preceding element which shares the line with the returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixKeepsPrecedingEndTagSharingTheLine()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Runs the method.
                                  /// </summary>{|#0:<returns>Nothing.</returns>|}
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// Runs the method.
                                       /// </summary>
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix keeps the following tag which starts on the last line of the returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixKeepsFollowingTagSharingTheLastLine()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<returns>
                                  /// Nothing.
                                  /// </returns>|}<param name="value">Value.</param>
                                  internal void TestMethod(int value)
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>Runs the method.</summary>
                                       /// <param name="value">Value.</param>
                                       internal void TestMethod(int value)
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix removes every line of a multi line returns tag which owns its lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixRemovesMultiLineReturnsTagCompletely()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<returns>
                                  /// Nothing.
                                  /// </returns>|}
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>Runs the method.</summary>
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<returns>Nothing.</returns>|}
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}