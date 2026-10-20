using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8204DoNotUsePlaceholderElementsAnalyzer"/> and
/// <see cref="RH8204DoNotUsePlaceholderElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8204DoNotUsePlaceholderElementsAnalyzerTests : BatchCodeFixTestsBase<RH8204DoNotUsePlaceholderElementsAnalyzer, RH8204DoNotUsePlaceholderElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a placeholder tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-001")]
    [Requirement("RH8204-002")]
    [Requirement("RH8204-003")]
    public async Task VerifyDiagnosticAndCodeFixForPlaceholderElement()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method does work.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the code fix strips a placeholder whose content is not well-formed XML instead of throwing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixForPlaceholderWithMalformedContent()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>handles a & b</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method handles a & b.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a placeholder element is reported at the top level of a summary, nested in a later documentation comment, and inside a code sample
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-001")]
    [Requirement("RH8204-002")]
    public async Task VerifySpecificationExampleReportsPlaceholderElements()
    {
        const string source = """
                              /// <summary>
                              /// Saves the {|#0:<placeholder>current customer</placeholder>|}
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>

                                  /// <remarks>
                                  /// {|#1:<placeholder/>|}
                                  /// </remarks>
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Count
                                  /// </summary>
                                  /// <example><code>{|#2:<placeholder>Sample</placeholder>|}</code></example>
                                  private int _count;
                              }
                              """;

        await Verify(source, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: a placeholder element nested inside another one is reported as well, each on its whole element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-001")]
    [Requirement("RH8204-002")]
    public async Task VerifySpecificationExampleReportsNestedPlaceholderElements()
    {
        const string source = """
                              /// <summary>
                              /// Saves {|#0:<placeholder>the {|#1:<placeholder>current</placeholder>|} customer</placeholder>|}
                              /// </summary>
                              public class C
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: the fix replaces a multi-line placeholder with its content, removes an empty one, and removes only the tags around content that is not well-formed XML
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-002")]
    [Requirement("RH8204-003")]
    public async Task VerifySpecificationExampleReplacesPlaceholderWithItsContent()
    {
        const string source = """
                              /// <summary>
                              /// Saves the {|#0:<placeholder>current
                              /// customer</placeholder>|} now
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Runs {|#1:<placeholder/>|} quickly
                                  /// </summary>
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Handles {|#2:<placeholder>a & b</placeholder>|}
                                  /// </summary>
                                  public void Stop()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>
                                   /// Saves the current
                                   /// customer now
                                   /// </summary>
                                   public class C
                                   {
                                       /// <summary>
                                       /// Runs  quickly
                                       /// </summary>
                                       public void Run()
                                       {
                                       }

                                       /// <summary>
                                       /// Handles a & b
                                       /// </summary>
                                       public void Stop()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a placeholder element is reported in a delimited documentation comment and on property, enum member, and parameter documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-001")]
    [Requirement("RH8204-002")]
    public async Task VerifyDiagnosticForPlaceholderInDelimitedCommentAndOtherDeclarations()
    {
        const string source = """
                              /// <summary>Kind</summary>
                              public enum E
                              {
                                  /// <summary>{|#0:<placeholder>First</placeholder>|}</summary>
                                  First,
                              }

                              /// <summary>Service</summary>
                              public class C
                              {
                                  /**
                                   * <summary>Gets the size {|#1:<placeholder/>|}</summary>
                                   */
                                  public int Size { get; }

                                  /// <summary>Runs</summary>
                                  /// <param name="value">{|#2:<placeholder>Value</placeholder>|}</param>
                                  public void Run(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 3));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|} and {|#1:<placeholder>returns nothing</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method does work and returns nothing.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        // Two placeholders on the same summary line produce disjoint text edits, so the batch fixer can apply
        // both without either shifting the other's span
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}