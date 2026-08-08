using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7101DoNotCombineFieldsAnalyzer"/> and <see cref="RH7101DoNotCombineFieldsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7101DoNotCombineFieldsAnalyzerTests : AnalyzerTestsBase<RH7101DoNotCombineFieldsAnalyzer, RH7101DoNotCombineFieldsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that single field declarations do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForSingleFieldDeclarations()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField;
                                    private int secondField;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that combined field declarations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCombinedFieldDeclarationsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix splits only the flagged field and leaves unrelated members in the type untouched,
    /// so the fix diff does not inherit unrelated whole-type reformatting (issue #456)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixDoesNotReformatUnrelatedMembers()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|};

                                    void Unrelated()
                                    {
                                System.Console.WriteLine();
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField;

                                     void Unrelated()
                                     {
                                 System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that comments attached to declarators and their separators are preserved when the fix splits the fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentsArePreservedWhenSplitting()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, // first
                                                {|#0:secondField|}; // second
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField; // first
                                     private int secondField; // second
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a documentation comment written before the semicolon appears exactly once and keeps its
    /// position. The fix applies the split transform without running the formatting pipeline, so this output is
    /// what the user sees (issue #625)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentBeforeSemicolonIsNotDuplicated()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|} /** Trailing note. */;
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField /** Trailing note. */;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a documentation comment written before the separator is preserved rather than dropped on the
    /// code fix surface, where no later phase runs to recover it (issue #624)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentBeforeSeparatorIsPreserved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField /** First field. */, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField /** First field. */;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix indents every generated field at the member level when the declaration is documented.
    /// The fix applies the split transform without running the formatting pipeline, so no later indentation phase
    /// repairs the anchor and this output is what the user sees (issue #592)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedCombinedFieldsAreFixedAtMemberIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Two fields.
                                     /// </summary>
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix anchors the generated fields on the line the field declaration starts on rather than on
    /// the documentation comment, which may be indented differently (issue #592)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedDocumentationCommentFixUsesFieldLineIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                  /// <summary>
                                  /// Two fields.
                                  /// </summary>
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                   /// <summary>
                                   /// Two fields.
                                   /// </summary>
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix still finds the field's own indentation when the documentation comment starts on the
    /// opening brace line, so the generated field does not fall back to column zero (issue #592)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentOnOpeningBraceLineFixUsesFieldLineIndentation()
    {
        const string testData = """
                                internal class TestClass
                                { /// <summary>
                                  /// Two fields.
                                  /// </summary>
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 { /// <summary>
                                   /// Two fields.
                                   /// </summary>
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line documentation comment re-attached to a generated field is not followed by a blank
    /// line. This form carries its own line break, so the split must not append a second one. The formatting
    /// pipeline's blank-line phase absorbs the stray break, which is why only the code fix can observe it (issue #592)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineDocumentationCommentBeforeDeclaratorGetsNoBlankLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField,
                                                /// <summary>Second field.</summary>
                                                {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     /// <summary>Second field.</summary>
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a delimited documentation comment re-attached to a generated field keeps a line break after it.
    /// Unlike a single-line documentation comment it carries none of its own, so this is the side of the boundary
    /// that a guard written against the trivia kind instead of the trivia text would break (issue #592)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineDocumentationCommentBeforeDeclaratorKeepsItsLineBreak()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField,
                                                /** Second field. */
                                                {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     /** Second field. */
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator is preserved by the fix. The fix runs the split
    /// transform without the formatting pipeline behind it, so this is the surface where the comment's own whitespace
    /// reaches the user unchanged (issue #636)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeFirstDeclaratorIsPreservedByTheFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int
                                    /* c */ firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int
                                     /* c */ firstField;
                                     private int
                                 secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix emits the comment's own indentation verbatim. No later phase runs on this surface, so
    /// the column the author wrote the comment at is the column the user sees, unlike the formatter surface where the
    /// indentation phase re-anchors it (issue #636)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsTheOwnIndentationOfTheCommentBeforeFirstDeclarator()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int
                                /* c */ firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int
                                 /* c */ firstField;
                                     private int
                                 secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is not offered when the combined field carries a preprocessor directive, because the
    /// split transform leaves directive-bearing fields intact and the fix would otherwise be a no-op (issue #456)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenFieldCarriesDirective()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if DEBUG
                                    private int firstField, secondField;
                                #endif
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7101DoNotCombineFieldsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<FieldDeclarationSyntax>()
                                                               .Single()
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}