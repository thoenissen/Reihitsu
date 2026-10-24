using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
public class RH8107VoidReturnValueMustNotBeDocumentedAnalyzerTests : BatchCodeFixTestsBase<RH8107VoidReturnValueMustNotBeDocumentedAnalyzer, RH8107VoidReturnValueMustNotBeDocumentedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a void member with a returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8107-001")]
    [Requirement("RH8107-005")]
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    [Requirement("RH8107-001")]
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
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
    /// Verifies no code fix is offered when the returns tag has no end tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Roslyn extends the element span past the trailing line break when the end tag is missing, so the following
    /// declaration would be pulled into the documentation comment
    /// </remarks>
    [TestMethod]
    [Requirement("RH8107-001")]
    [Requirement("RH8107-005")]
    [Requirement("RH8107-007")]
    [Requirement("SHARED-DOC-017")]
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
    [Requirement("RH8107-007")]
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
    /// Verifies a diagnostic and code fix for a void compound-assignment operator with a returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8107-001")]
    public async Task VerifyDiagnosticAndCodeFixForVoidCompoundAssignmentOperatorReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class Money
                              {
                                  public int Amount;

                                  /// <summary>Adds an amount in place.</summary>
                                  /// <param name="other">The amount to add.</param>
                                  /// {|#0:<returns>Nothing.</returns>|}
                                  public void operator +=(Money other)
                                  {
                                      Amount += other.Amount;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class Money
                                   {
                                       public int Amount;

                                       /// <summary>Adds an amount in place.</summary>
                                       /// <param name="other">The amount to add.</param>
                                       public void operator +=(Money other)
                                       {
                                           Amount += other.Amount;
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
    [Requirement("RH8107-004")]
    [Requirement("SHARED-DOC-008")]
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
    [Requirement("RH8107-003")]
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
    [Requirement("RH8107-003")]
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
    [Requirement("RH8107-003")]
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
    [Requirement("RH8107-003")]
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

    /// <summary>
    /// Verifies no diagnostic is reported for a returns tag on a member whose return type is a task, not void
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8107-002")]
    public async Task VerifyNoDiagnosticForReturnsTagOnTaskReturningMember()
    {
        const string source = """
                              using System.Threading.Tasks;

                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <returns>A task</returns>
                                  internal Task RunAsync()
                                  {
                                      return Task.CompletedTask;
                                  }

                                  /// <summary>Runs the method.</summary>
                                  /// <returns>A task</returns>
                                  internal async ValueTask RunValueAsync()
                                  {
                                      await Task.CompletedTask;
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the code fix changes the words of the documentation comment only in the element its diagnostic
    /// reports, and removes the space in front of that element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyCodeFixChangesOnlyTheReportedElementOfTheDocumentationComment()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>Runs</summary> {|RH8107:<returns>Nothing</returns>|}
                                  public void Run()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       /// <summary>Runs</summary>
                                       public void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example of an empty <c>returns</c> element: it needs no end tag, so the code fix is
    /// offered and removes its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixRemovesEmptyReturnsElement()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#0:<returns/>|}
                                  public void Run()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       public void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: no code fix is offered for a <c>returns</c> element that the end tag of the
    /// enclosing <c>remarks</c> element closes, so that end tag is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8107-007")]
    public async Task VerifyNoCodeFixForReturnsElementClosedByEndTagOfTheEnclosingElement()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <remarks>{|RH8107:<returns>Nothing</remarks>|}
                                  public void Run()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: end-tag names are compared exactly, so no code fix is offered for a
    /// <c>Returns</c> element that <c>&lt;/returns&gt;</c> closes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8107-007")]
    public async Task VerifyNoCodeFixForReturnsElementClosedByEndTagDifferingOnlyInCase()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|RH8107:<Returns>Nothing</returns>|}
                                  public void Run()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: no code fix is offered for a <c>returns</c> element without an end tag,
    /// which is reported through the line break at the end of its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8107-007")]
    public async Task VerifySpecificationExampleWithholdsFixForReturnsElementWithoutEndTag()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|RH8107:<returns>Nothing
                              |}    public void Run()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the code fix is still offered for a <c>returns</c> element closed by its own end tag when another
    /// element of the comment has no end tag of its own, and that the other element is kept as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8107-006")]
    [Requirement("SHARED-DOC-018")]
    public async Task VerifyCodeFixRemovesReturnsElementBesideElementWithoutEndTagOfItsOwn()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs <see>x</y>
                                  /// </summary>
                                  /// {|#0:<returns>Nothing</returns>|}
                                  public void Run()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       /// <summary>
                                       /// Runs <see>x</y>
                                       /// </summary>
                                       public void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing every diagnostic of a document removes the <c>returns</c> element closed by its own end tag
    /// and leaves the one closed by another element's end tag reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyFixAllLeavesReturnsElementWithoutEndTagOfItsOwnUnchanged()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|RH8107:<returns>Nothing</returns>|}
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  /// <remarks>{|RH8107:<returns>Nothing</remarks>|}
                                  public void Stop()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       public void Run()
                                       {
                                       }

                                       /// <summary>
                                       /// Stops
                                       /// </summary>
                                       /// <remarks>{|RH8107:<returns>Nothing</remarks>|}
                                       public void Stop()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the specification example of the shared element removal: an element on lines of its own is removed with
    /// those lines, and an element sharing its line with a summary is removed with the white space before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-018")]
    [Requirement("RH8107-006")]
    public async Task VerifySpecificationExampleRemovesReturnsElementWithTheLinesItOwns()
    {
        const string source = """
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#0:<returns>
                                  /// Nothing
                                  /// </returns>|}
                                  /// <param name="a">A</param>
                                  public void Run(int a)
                                  {
                                  }

                                  /// <summary>Runs</summary> {|#1:<returns>Nothing</returns>|}
                                  public void Run2()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       /// <param name="a">A</param>
                                       public void Run(int a)
                                       {
                                       }

                                       /// <summary>Runs</summary>
                                       public void Run2()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        // Verifies multiple returns tags are detected and fixed together
        return new FixAllScenario(source,
                                  fixedSource,
                                  Diagnostics(RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8107MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}