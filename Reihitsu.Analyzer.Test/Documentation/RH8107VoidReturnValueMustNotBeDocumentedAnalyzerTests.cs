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
    /// Verifies the code fix keeps the tags on both sides of the returns tag on a shared line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixKeepsSurroundingTagsSharingTheLine()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>{|#0:<returns>Nothing.</returns>|}<param name="value">Value.</param>
                                  internal void TestMethod(int value)
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /// <summary>Runs the method.</summary><param name="value">Value.</param>
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
    /// Verifies the code fix removes the returns tag from a delimited documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixRemovesReturnsTagFromDelimitedDocumentationComment()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /**
                                   * <summary>Runs the method.</summary>
                                   * {|#0:<returns>Nothing.</returns>|}
                                   */
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       /**
                                        * <summary>Runs the method.</summary>
                                        */
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
    /// Verifies multiple returns tags are detected and fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReturnsTagsAreFixedTogether()
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

                                  /// <summary>Runs the other method.</summary>{|#1:<returns>Nothing.</returns>|}
                                  internal void TestOtherMethod()
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

                                       /// <summary>Runs the other method.</summary>
                                       internal void TestOtherMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no code fix is offered when the returns tag has no end tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Roslyn extends the element span past the trailing line break when the end tag is missing, so the following
    /// declaration would be pulled into the documentation comment
    /// </remarks>
    [TestMethod]
    public async Task VerifyNoCodeFixWhenEndTagIsMissing()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <returns>Nothing.
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        await Verify(source,
                     source,
                     Diagnostic(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId).WithSpan(6, 9, 7, 1)
                                                                                              .WithMessage(AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies no code fix is offered when a returns tag without an end tag swallows a following tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoCodeFixWhenEndTagIsMissingAndFollowingTagIsSwallowed()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <returns>Nothing.
                                  /// <param name="value">Value.</param>
                                  internal void TestMethod(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source,
                     source,
                     Diagnostic(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId).WithSpan(5, 9, 7, 1)
                                                                                              .WithMessage(AnalyzerResources.RH8107MessageFormat));
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

    /// <summary>
    /// Verifies a diagnostic is reported for a returns tag nested in a remarks element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnsTagNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <remarks>
                                  /// {|#0:<returns>Nothing.</returns>|}
                                  /// </remarks>
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
                                       /// <remarks>
                                       /// </remarks>
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
    /// Verifies no diagnostic is reported for a returns tag which only appears inside a code sample
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnsTagInsideCodeSample()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Saves the customer.
                                  /// <code>
                                  /// <returns>Sample tag inside a code block.</returns>
                                  /// </code>
                                  /// </summary>
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a returns tag which only appears inside an example sample
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnsTagInsideExampleSample()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Saves the customer.</summary>
                                  /// <example>
                                  /// <returns>Sample tag inside an example.</returns>
                                  /// </example>
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a top level returns tag is still detected when a code sample also contains one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTopLevelReturnsTagIsDetectedBesideCodeSample()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>
                                  /// Saves the customer.
                                  /// <code>
                                  /// <returns>Sample tag inside a code block.</returns>
                                  /// </code>
                                  /// </summary>
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
                                       /// <summary>
                                       /// Saves the customer.
                                       /// <code>
                                       /// <returns>Sample tag inside a code block.</returns>
                                       /// </code>
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

    #endregion // Tests
}