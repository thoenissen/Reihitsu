using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
    [Requirement("SHARED-DOC-017")]
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

    /// <summary>
    /// Verifies the specification example: a placeholder element without an end tag is reported up to the end of the
    /// documentation comment, and no code fix is offered, so the method that follows the comment stays code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8204-002")]
    [Requirement("RH8204-003")]
    public async Task VerifyNoCodeFixForPlaceholderWithoutEndTag()
    {
        const string source = """
                              /// <summary>C</summary>
                              public class C
                              {
                                  /// <remarks>Fast</remarks>
                                  /// {|RH8204:<placeholder>Later text
                              |}    public void Run()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that no code fix is offered for a placeholder element closed by the end tag of the element around it,
    /// so that end tag is kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8204-002")]
    [Requirement("RH8204-003")]
    public async Task VerifyNoCodeFixForPlaceholderClosedByEndTagOfTheEnclosingElement()
    {
        const string source = """
                              /// <summary>C</summary>
                              public class C
                              {
                                  /// <remarks>{|RH8204:<placeholder>Later text</remarks>|}
                                  public void Stop()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that end-tag names are compared exactly: no code fix is offered for a placeholder element whose end tag
    /// differs only in case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8204-003")]
    public async Task VerifyNoCodeFixForPlaceholderClosedByEndTagDifferingOnlyInCase()
    {
        const string source = """
                              /// <summary>C</summary>
                              public class C
                              {
                                  /// <remarks>{|RH8204:<Placeholder>Later text</placeholder>|}</remarks>
                                  public void Stop()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example of the locations: a placeholder without an end tag is reported through the
    /// line break where the documentation comment ends, one closed by another element's end tag through that end tag,
    /// and no code fix is offered for either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-002")]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifySpecificationExampleLocatesPlaceholdersWithoutEndTagOfTheirOwn()
    {
        const string source = """
                              /// <summary>C</summary>
                              public class C
                              {
                                  /// <remarks>Fast</remarks>
                                  /// {|RH8204:<placeholder>Later text
                              |}    public void Run()
                                  {
                                  }

                                  /// <remarks>{|RH8204:<placeholder>Later text</remarks>|}
                                  public void Stop()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the code fix is still offered for a placeholder element closed by its own end tag when another
    /// element of the same comment has no end tag of its own, and that the other element is kept as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixReplacesPlaceholderBesideElementWithoutEndTagOfItsOwn()
    {
        const string source = """
                              /// <summary>{|#0:<placeholder>A</placeholder>|} <see>b</x></summary>
                              public class C
                              {
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>A <see>b</x></summary>
                                   public class C
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing every diagnostic of a document replaces the placeholder closed by its own end tag and leaves
    /// the placeholder without an end tag reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    public async Task VerifyFixAllLeavesPlaceholderWithoutEndTagUnchanged()
    {
        const string source = """
                              /// <summary>{|RH8204:<placeholder>C</placeholder>|}</summary>
                              public class C
                              {
                                  /// <remarks>Fast</remarks>
                                  /// {|RH8204:<placeholder>Later text
                              |}    public void Run()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>C</summary>
                                   public class C
                                   {
                                       /// <remarks>Fast</remarks>
                                       /// {|RH8204:<placeholder>Later text
                                   |}    public void Run()
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
    /// Verifies that no code fix is offered for a placeholder element without an end tag in a CRLF document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DOC-017")]
    [Requirement("RH8204-003")]
    public async Task VerifyNoCodeFixForPlaceholderWithoutEndTagWithCarriageReturnLineFeed()
    {
        const string source = """
                              /// <summary>C</summary>
                              public class C
                              {
                                  /// <remarks>Fast</remarks>
                                  /// {|RH8204:<placeholder>Later text
                              |}    public void Run()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source));
    }

    /// <summary>
    /// Verifies the specification example of content kept as written: references, a greater-than sign, the quotes and
    /// empty-element form of a nested element, and white-space-only content stay as they are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifySpecificationExampleKeepsContentAsWritten()
    {
        const string source = """
                              /// <summary>
                              /// Saves the {|#0:<placeholder>a &#32; <see cref='C'/> b</placeholder>|}
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Runs if {|#1:<placeholder>a > b</placeholder>|}
                                  /// </summary>
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Stops the{|#2:<placeholder> </placeholder>|}customer
                                  /// </summary>
                                  public void Stop()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   /// <summary>
                                   /// Saves the a &#32; <see cref='C'/> b
                                   /// </summary>
                                   public class C
                                   {
                                       /// <summary>
                                       /// Runs if a > b
                                       /// </summary>
                                       public void Run()
                                       {
                                       }

                                       /// <summary>
                                       /// Stops the customer
                                       /// </summary>
                                       public void Stop()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the fix keeps a character reference in the content as a reference
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsCharacterReferenceInContent()
    {
        await VerifyPlaceholderContentIsKept("a &#32; b");
    }

    /// <summary>
    /// Verifies that the fix keeps a greater-than sign in the content as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsGreaterThanSignInContent()
    {
        await VerifyPlaceholderContentIsKept("a > b");
    }

    /// <summary>
    /// Verifies that the fix keeps the single quotes and the empty-element form of a nested element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsQuotesAndEmptyElementFormOfNestedElement()
    {
        await VerifyPlaceholderContentIsKept("see <see cref='C'/>");
    }

    /// <summary>
    /// Verifies that the fix keeps content of white space only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsWhiteSpaceOnlyContent()
    {
        await VerifyPlaceholderContentIsKept("   ");
    }

    /// <summary>
    /// Verifies that the fix keeps entity references for quotes as references
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsQuoteEntityReferencesInContent()
    {
        await VerifyPlaceholderContentIsKept("&quot;a&quot; &apos;b&apos;");
    }

    /// <summary>
    /// Verifies that the fix keeps a CDATA section in the content as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsCdataSectionInContent()
    {
        await VerifyPlaceholderContentIsKept("a <![CDATA[x < y]]> b");
    }

    /// <summary>
    /// Verifies that the fix keeps an XML comment in the content as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsXmlCommentInContent()
    {
        await VerifyPlaceholderContentIsKept("a <!-- note --> b");
    }

    /// <summary>
    /// Verifies that the fix keeps the attributes of a nested element and the attribute of the placeholder does not
    /// reach the result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsAttributesOfNestedElement()
    {
        const string source = """
                              /// <summary>
                              /// Saves {|#0:<placeholder hint="h">the <c  lang = 'x' >value</c></placeholder>|} now
                              /// </summary>
                              public class C
                              {
                              }
                              """;
        const string fixedSource = """
                                   /// <summary>
                                   /// Saves the <c  lang = 'x' >value</c> now
                                   /// </summary>
                                   public class C
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
    }

    /// <summary>
    /// Verifies the first specification example of the fix in a document whose line breaks are CRLF: the line break
    /// inside the content stays CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifySpecificationExampleReplacesPlaceholderWithItsContentWithCarriageReturnLineFeed()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
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
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
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
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the fix keeps the CRLF line break inside the content of a placeholder in a CRLF document, applied
    /// to that placeholder's diagnostic alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8204-003")]
    public async Task VerifyCodeFixKeepsCarriageReturnLineFeedInsideContent()
    {
        var source = "/// <summary>\r\n/// Saves the <placeholder>current\r\n/// customer</placeholder> now\r\n/// </summary>\r\npublic class C\r\n{\r\n}\r\n";
        var expected = "/// <summary>\r\n/// Saves the current\r\n/// customer now\r\n/// </summary>\r\npublic class C\r\n{\r\n}\r\n";

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Verifies that the fix replaces a placeholder with the given content and keeps the content character for
    /// character
    /// </summary>
    /// <param name="content">Content of the placeholder, as written in the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyPlaceholderContentIsKept(string content)
    {
        var source = $$"""
                       /// <summary>
                       /// Saves {|#0:<placeholder>{{content}}</placeholder>|} now
                       /// </summary>
                       public class C
                       {
                       }
                       """;
        var fixedSource = $$"""
                            /// <summary>
                            /// Saves {{content}} now
                            /// </summary>
                            public class C
                            {
                            }
                            """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
    }

    #endregion // Methods

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