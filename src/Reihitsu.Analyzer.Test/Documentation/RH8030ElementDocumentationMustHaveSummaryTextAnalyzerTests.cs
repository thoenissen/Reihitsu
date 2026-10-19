using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8030ElementDocumentationMustHaveSummaryTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH8030ElementDocumentationMustHaveSummaryTextAnalyzerTests : AnalyzerTestsBase<RH8030ElementDocumentationMustHaveSummaryTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("RH8030-002")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyDiagnosticForEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// {|#0:<summary></summary>|}
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an enum member with an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyDiagnosticForEnumMemberWithEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              internal enum Status
                              {
                                  /// {|#0:<summary></summary>|}
                                  Active,
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an operator with an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyDiagnosticForOperatorWithEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// {|#0:<summary></summary>|}
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a conversion operator with an empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyDiagnosticForConversionOperatorWithEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// {|#0:<summary></summary>|}
                                  public static implicit operator int(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a class with a non-empty summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyNoDiagnosticForClassWithNonEmptySummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A test class.</summary>
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that an empty summary nested in remarks is ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyNoDiagnosticForEmptySummaryNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks><summary></summary></remarks>
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// {|#0:<summary></summary>|}
                              internal partial class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: summaries without text are reported on the whole element, also next to an inheritdoc, while a summary with text or nested in remarks is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("RH8030-002")]
    public async Task VerifySpecificationExampleReportsSummariesWithoutText()
    {
        const string source = """
                              /// {|#0:<summary></summary>|}
                              public class Service
                              {
                                  /// {|#1:<summary>
                                  /// </summary>|}
                                  private int _count;

                                  /// <summary>
                                  /// Runs the service
                                  /// </summary>
                                  public void Run()
                                  {
                                  }

                                  /// {|#2:<summary/>|}
                                  ~Service()
                                  {
                                  }
                              }

                              /// <summary>
                              /// Status
                              /// </summary>
                              public enum Status
                              {
                                  /// {|#3:<summary></summary>|}
                                  Active,
                              }

                              /// <inheritdoc/>
                              /// {|#4:<summary></summary>|}
                              public class Derived : Service
                              {
                              }

                              /// <remarks>
                              /// <summary></summary>
                              /// </remarks>
                              public class Nested
                              {
                              }

                              /// <summary>
                              /// Money
                              /// </summary>
                              public readonly struct Money
                              {
                                  /// {|#5:<summary></summary>|}
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 6));
    }

    /// <summary>
    /// Verifies the specification example of summaries without text, with line feed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("RH8030-002")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifySpecificationExampleOfSummaryWithoutTextWithLineFeeds()
    {
        const string source = """
                              /// {|#0:<summary></summary>|}
                              public class A
                              {
                              }

                              /// {|#1:<summary/>|}
                              public class B
                              {
                              }

                              /// {|#2:<summary>
                              /// </summary>|}
                              public class C
                              {
                              }

                              /// {|#3:<summary>&#32;</summary>|}
                              public class D
                              {
                              }

                              /**
                               * {|#4:<summary>
                               * </summary>|}
                               */
                              public class E
                              {
                              }

                              /**
                               ** {|#5:<summary>
                               ** </summary>|}
                               */
                              public class J
                              {
                              }

                              /** <summary> * </summary> */
                              public class K
                              {
                              }

                              /// {|#6:<summary>&#160;</summary>|}
                              public class F
                              {
                              }

                              /// <summary>
                              /// Text
                              /// </summary>
                              public class G
                              {
                              }

                              /**
                               * <summary>
                               * Text
                               * </summary>
                               */
                              public class H
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 7));
    }

    /// <summary>
    /// Verifies the specification example of summaries without text, with carriage return and line feed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("RH8030-002")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifySpecificationExampleOfSummaryWithoutTextWithCarriageReturnLineFeeds()
    {
        const string source = """
                              /// {|#0:<summary></summary>|}
                              public class A
                              {
                              }

                              /// {|#1:<summary/>|}
                              public class B
                              {
                              }

                              /// {|#2:<summary>
                              /// </summary>|}
                              public class C
                              {
                              }

                              /// {|#3:<summary>&#32;</summary>|}
                              public class D
                              {
                              }

                              /**
                               * {|#4:<summary>
                               * </summary>|}
                               */
                              public class E
                              {
                              }

                              /**
                               ** {|#5:<summary>
                               ** </summary>|}
                               */
                              public class J
                              {
                              }

                              /** <summary> * </summary> */
                              public class K
                              {
                              }

                              /// {|#6:<summary>&#160;</summary>|}
                              public class F
                              {
                              }

                              /// <summary>
                              /// Text
                              /// </summary>
                              public class G
                              {
                              }

                              /**
                               * <summary>
                               * Text
                               * </summary>
                               */
                              public class H
                              {
                              }
                              """;

        await Verify(source.ReplaceLineEndings("\r\n"), Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 7));
    }

    /// <summary>
    /// Verifies summaries that contain only Unicode white space, written as character references, are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyDiagnosticForSummaryOfUnicodeWhiteSpaceOnly()
    {
        const string source = """
                              /// {|#0:<summary>&#9;</summary>|}
                              public class Tab
                              {
                              }


                              /// {|#1:<summary>&#x2003;</summary>|}
                              public class EmSpace
                              {
                              }

                              /// {|#2:<summary>&#x85;</summary>|}
                              public class NextLine
                              {
                              }

                              /// {|#3:<summary>&#xA0;&#32;&#160;</summary>|}
                              public class Mixed
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 4));
    }

    /// <summary>
    /// Verifies a run of stars that starts a line of a block documentation comment counts as a line break, while a star after a space is text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("SHARED-DOC-010")]
    public async Task VerifyStarsOfBlockDocumentationComments()
    {
        const string source = """
                              /**
                               *** {|#0:<summary>
                               *** </summary>|}
                               */
                              public class Stars
                              {
                              }

                              /**
                               * <summary>
                               * * </summary>
                               */
                              public class StarAfterSpace
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    /// <summary>
    /// Verifies summaries without text are reported on struct, interface, record, record struct, enum, and delegate declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyDiagnosticForSummariesWithoutTextOfEveryTypeKind()
    {
        const string source = """
                              /// {|#0:<summary></summary>|}
                              public struct S
                              {
                              }

                              /// {|#1:<summary></summary>|}
                              public interface I
                              {
                              }

                              /// {|#2:<summary></summary>|}
                              public record R;

                              /// {|#3:<summary></summary>|}
                              public record struct P;

                              /// {|#4:<summary></summary>|}
                              public enum E
                              {
                              }

                              /// {|#5:<summary></summary>|}
                              public delegate void D();
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 6));
    }

    /// <summary>
    /// Verifies summaries without text are reported on constructor, method, property, indexer, field, and event declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyDiagnosticForSummariesWithoutTextOfEveryMemberKind()
    {
        const string source = """
                              using System;

                              /// <summary>Holder</summary>
                              public class Holder
                              {
                                  /// {|#0:<summary></summary>|}
                                  public Holder()
                                  {
                                  }

                                  /// {|#1:<summary></summary>|}
                                  public void Run()
                                  {
                                  }

                                  /// {|#2:<summary></summary>|}
                                  public int Value { get; }

                                  /// {|#3:<summary></summary>|}
                                  public int this[int index] => index;

                                  /// {|#4:<summary></summary>|}
                                  public int Field;

                                  /// {|#5:<summary></summary>|}
                                  public event Action Changed;

                                  /// {|#6:<summary></summary>|}
                                  public event Action Closed
                                  {
                                      add
                                      {
                                      }
                                      remove
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 7));
    }

    /// <summary>
    /// Verifies summaries without text are reported on private, implicitly private, file-local, and nested declarations alike
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyDiagnosticForSummariesWithoutTextWhateverAccessibilityAndNesting()
    {
        const string source = """
                              /// <summary>Outer</summary>
                              public class Outer
                              {
                                  /// {|#0:<summary></summary>|}
                                  private class Hidden
                                  {
                                      /// {|#1:<summary></summary>|}
                                      void Run()
                                      {
                                      }
                                  }
                              }

                              /// {|#2:<summary></summary>|}
                              file class Local
                              {
                              }

                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  /// {|#3:<summary></summary>|}
                                  int _x;
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 4));
    }

    /// <summary>
    /// Verifies a summary without text on an extension declaration is not reported, while one on a member of the extension block is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    public async Task VerifyNoDiagnosticForExtensionDeclarationButForItsMember()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary></summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      /// {|#0:<summary></summary>|}
                                      public int WordCount() => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    /// <summary>
    /// Verifies a declaration of several fields or events with a summary without text is reported once, on its summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("RH8030-002")]
    public async Task VerifyDiagnosticForDeclarationOfSeveralFieldsOrEventsIsReportedOnce()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public class Point
                              {
                                  /// {|#0:<summary></summary>|}
                                  public int X, Y;

                                  /// {|#1:<summary></summary>|}
                                  public event System.Action Moved, Resized;
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a summary before the attribute list, a directive, or an ordinary comment is checked, while one after the attribute list is not part of the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("SHARED-DOC-004")]
    public async Task VerifySummaryPlacementFollowsTheDocumentationCommentsOfTheDeclaration()
    {
        const string source = """
                              using System;

                              /// {|#0:<summary></summary>|}
                              [Obsolete]
                              public class A
                              {
                              }

                              [Obsolete]
                              /// <summary></summary>
                              public class B
                              {
                              }

                              /// {|#1:<summary></summary>|}
                              #if DEBUG
                              #endif
                              public class C
                              {
                              }

                              /// {|#2:<summary></summary>|}
                              // Ordinary comment
                              public class D
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a summary without text is reported when the documentation mode is parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8030-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyDiagnosticForSummaryWithoutTextInParseMode()
    {
        const string source = """
                              /// {|#0:<summary></summary>|}
                              public class A
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject), Diagnostics(RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.DiagnosticId, AnalyzerResources.RH8030MessageFormat));
    }

    #endregion // Tests
}