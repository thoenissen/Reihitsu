using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzerTests : AnalyzerTestsBase<RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an extension declaration without documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("RH8031-003")]
    public async Task VerifyDiagnosticForMissingDocumentationOnExtensionDeclaration()
    {
        const string source = """
                              public static class Extensions
                              {
                                  {|#0:extension|}(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an extension declaration with an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-002")]
    [Requirement("RH8031-003")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyDiagnosticForEmptySummaryOnExtensionDeclaration()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// {|#0:<summary></summary>|}
                                  extension(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a documented generic extension declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    public async Task VerifyNoDiagnosticForGenericExtensionDeclarationWithSummary()
    {
        const string source = """
                              using System.Collections.Generic;
                              
                              public static class Extensions
                              {
                                  /// <summary>Provides collection extensions.</summary>
                                  extension<T>(IEnumerable<T> values)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a nested summary does not satisfy an extension declaration's documentation requirement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("RH8031-003")]
    public async Task VerifyDiagnosticForSummaryNestedInRemarks()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <remarks><summary>Nested summary.</summary></remarks>
                                  {|#0:extension|}(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              public static class Extensions
                              {
                                  {|#0:extension|}(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source,
                     test =>
                     {
                         test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject);
                     });
    }

    /// <summary>
    /// Verifies the specification example: extension declarations without a top-level summary are reported on their extension keyword, whatever the documentation and accessibility of the static class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("RH8031-003")]
    [Requirement("SHARED-DIAG-007")]
    public async Task VerifySpecificationExampleReportsExtensionDeclarationsWithoutSummary()
    {
        const string source = """
                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  {|#0:extension|}(string value)
                                  {
                                  }

                                  /// <remarks>
                                  /// <summary>Nested</summary>
                                  /// </remarks>
                                  {|#1:extension|}(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Adds helpers to long values
                                  /// </summary>
                                  extension(long value)
                                  {
                                  }
                              }

                              internal static class Hidden
                              {
                                  {|#2:extension|}(byte value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: summaries without text on extension declarations are reported on the whole summary element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-002")]
    [Requirement("RH8031-003")]
    [Requirement("SHARED-DIAG-007")]
    public async Task VerifySpecificationExampleReportsExtensionDeclarationSummariesWithoutText()
    {
        const string source = """
                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// {|#0:<summary></summary>|}
                                  extension(string value)
                                  {
                                  }

                                  /// {|#1:<summary>
                                  /// </summary>|}
                                  extension(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat, 2));
    }

    /// <summary>
    /// Verifies extension declarations whose documentation comment has only remarks or plain text, and an undocumented generic one, are reported on their extension keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("RH8031-003")]
    public async Task VerifyDiagnosticForExtensionDeclarationsWithoutSummaryElement()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <remarks>Remarks</remarks>
                                  {|#0:extension|}(string value)
                                  {
                                  }

                                  /// Plain text
                                  {|#1:extension|}(int value)
                                  {
                                  }

                                  {|#2:extension|}<T>(T[] values)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat, 3));
    }

    /// <summary>
    /// Verifies every form of an extension declaration summary without text is reported on the summary element, with line feed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-002")]
    [Requirement("RH8031-003")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyDiagnosticForExtensionDeclarationSummaryFormsWithoutTextWithLineFeeds()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// {|#0:<summary/>|}
                                  extension(string value)
                                  {
                                  }

                                  /// {|#1:<summary>
                                  /// </summary>|}
                                  extension(int value)
                                  {
                                  }

                                  /// {|#2:<summary>&#32;</summary>|}
                                  extension(long value)
                                  {
                                  }

                                  /// {|#3:<summary>&#160;</summary>|}
                                  extension(byte value)
                                  {
                                  }

                                  /**
                                   * {|#4:<summary>
                                   * </summary>|}
                                   */
                                  extension(short value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat, 5));
    }

    /// <summary>
    /// Verifies every form of an extension declaration summary without text is reported on the summary element, with carriage return and line feed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-002")]
    [Requirement("RH8031-003")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyDiagnosticForExtensionDeclarationSummaryFormsWithoutTextWithCarriageReturnLineFeeds()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// {|#0:<summary/>|}
                                  extension(string value)
                                  {
                                  }

                                  /// {|#1:<summary>
                                  /// </summary>|}
                                  extension(int value)
                                  {
                                  }

                                  /// {|#2:<summary>&#32;</summary>|}
                                  extension(long value)
                                  {
                                  }

                                  /// {|#3:<summary>&#160;</summary>|}
                                  extension(byte value)
                                  {
                                  }

                                  /**
                                   * {|#4:<summary>
                                   * </summary>|}
                                   */
                                  extension(short value)
                                  {
                                  }
                              }
                              """;

        await Verify(source.ReplaceLineEndings("\r\n"), Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat, 5));
    }

    /// <summary>
    /// Verifies extension declaration summaries with text, or with a star that does not start a line, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-002")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyNoDiagnosticForExtensionDeclarationSummariesWithText()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /**
                                   * <summary>
                                   * Text
                                   * </summary>
                                   */
                                  extension(string value)
                                  {
                                  }

                                  /** <summary> * </summary> */
                                  extension(long value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a documented extension declaration is not reported when the static class that declares it is undocumented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    public async Task VerifyNoDiagnosticForDocumentedExtensionDeclarationOfUndocumentedClass()
    {
        const string source = """
                              internal static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  extension(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a documentation comment followed by an ordinary comment or a directive still belongs to the extension declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("RH8031-002")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifyDocumentationCommentBeforeDirectiveOrOrdinaryCommentBelongsToExtensionDeclaration()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  // Ordinary comment
                                  extension(string value)
                                  {
                                  }

                                  /// {|#0:<summary></summary>|}
                              #if DEBUG
                              #endif
                                  extension(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat));
    }

    /// <summary>
    /// Verifies an undocumented extension declaration is reported when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8031-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyDiagnosticForExtensionDeclarationWithoutSummaryInParseMode()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  {|#0:extension|}(string value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject), Diagnostics(RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8031MessageFormat));
    }

    #endregion // Tests
}