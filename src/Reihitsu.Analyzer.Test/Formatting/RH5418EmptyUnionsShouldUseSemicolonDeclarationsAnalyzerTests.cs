using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer"/> and <see cref="RH5418EmptyUnionsShouldUseSemicolonDeclarationsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzerTests : BatchCodeFixTestsBase<RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer, RH5418EmptyUnionsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies the example of RH5418-001: empty unions are reported on their names with the rule's message, top-level
    /// and nested, with modifiers, type parameters, and constraint clauses; the fix writes the semicolon declaration,
    /// and no fix is offered for the union whose body holds a comment; a union with a semicolon or a member and a
    /// struct are not reported
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5418-001")]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-002")]
    [Requirement("SHARED-EMPTY-003")]
    [Requirement("SHARED-EMPTY-004")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task EmptyUnionsAreReportedAndFixedAsSpecified(string endOfLine)
    {
        const string testCode = """
                                union {|#0:A|}(int, string)
                                {
                                }

                                public readonly union {|#1:B|}<T>(T, string)
                                    where T : class
                                {
                                    // reserved
                                }

                                union C(int, string);

                                union D(int, string)
                                {
                                    public int Get() => 1;
                                }

                                struct E
                                {
                                }

                                class Outer
                                {
                                    union {|#2:F|}(long, string) { }
                                }
                                """;

        const string fixedCode = """
                                 union A(int, string);

                                 public readonly union {|#0:B|}<T>(T, string)
                                     where T : class
                                 {
                                     // reserved
                                 }

                                 union C(int, string);

                                 union D(int, string)
                                 {
                                     public int Get() => 1;
                                 }

                                 struct E
                                 {
                                 }

                                 class Outer
                                 {
                                     union F(long, string);
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(ConvertEndOfLine(testCode, endOfLine),
                     ConvertEndOfLine(fixedCode, endOfLine),
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.FixedState.ExpectedDiagnostics.AddRange(Diagnostics(RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5418MessageFormat));
                     },
                     Diagnostics(RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5418MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that empty structs and record structs are not reported by this rule, although the compiler emits a
    /// union as a struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5418-001")]
    public async Task EmptyStructsAndRecordStructsAreNotReported()
    {
        const string testCode = """
                                struct S
                                {
                                }

                                record struct R
                                {
                                }

                                readonly struct T { }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that an empty union nested in a struct, a record, a record struct, an interface, and a union is
    /// reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5418-001")]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    public async Task EmptyUnionNestedInEveryKindOfTypeIsReportedAndFixed()
    {
        const string testCode = """
                                struct S
                                {
                                    union {|RH5418:InStruct|}(int, string)
                                    {
                                    }
                                }

                                record R
                                {
                                    union {|RH5418:InRecord|}(int, string)
                                    {
                                    }
                                }

                                record struct RS
                                {
                                    union {|RH5418:InRecordStruct|}(int, string)
                                    {
                                    }
                                }

                                interface I
                                {
                                    union {|RH5418:InInterface|}(int, string)
                                    {
                                    }
                                }

                                union Outer(int, string)
                                {
                                    union {|RH5418:InUnion|}(long, string)
                                    {
                                    }

                                    public int Get() => 1;
                                }
                                """;

        const string fixedCode = """
                                 struct S
                                 {
                                     union InStruct(int, string);
                                 }

                                 record R
                                 {
                                     union InRecord(int, string);
                                 }

                                 record struct RS
                                 {
                                     union InRecordStruct(int, string);
                                 }

                                 interface I
                                 {
                                     union InInterface(int, string);
                                 }

                                 union Outer(int, string)
                                 {
                                     union InUnion(long, string);

                                     public int Get() => 1;
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that both parts of a partial union with empty bodies are reported, and the fix gives both the semicolon
    /// declaration, the part without a case-type list included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5418-001")]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-003")]
    public async Task EmptyPartsOfPartialUnionAreReportedAndFixed()
    {
        const string testCode = """
                                partial union {|RH5418:P|}(int, string)
                                {
                                }

                                partial union {|RH5418:P|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 partial union P(int, string);

                                 partial union P;
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a file-local union and a generic union with a constraint clause are reported, and the fix keeps
    /// the clause on its own line in front of the semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5418-001")]
    [Requirement("SHARED-EMPTY-003")]
    public async Task FileLocalAndConstrainedUnionsAreReportedAndFixed()
    {
        const string testCode = """
                                file union {|RH5418:FileU|}(int, string)
                                {
                                }

                                union {|RH5418:V|}<T>(T, string)
                                    where T : class
                                {
                                }
                                """;

        const string fixedCode = """
                                 file union FileU(int, string);

                                 union V<T>(T, string)
                                     where T : class;
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a comment behind the closing brace follows the semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task CommentBehindClosingBraceFollowsTheSemicolon()
    {
        const string testCode = """
                                union {|RH5418:U|}(int, string) { } // note

                                union {|RH5418:W|}(int, string)
                                {
                                } // note
                                """;

        const string fixedCode = """
                                 union U(int, string); // note

                                 union W(int, string); // note
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union with a comment behind the header, in front of the opening brace, behind the opening brace,
    /// or in front of the closing brace is reported and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task UnionWithCommentThatKeepsTheBracesIsReportedWithoutFix()
    {
        const string testCode = """
                                union {|RH5418:A|}(int, string) // note
                                {
                                }

                                union {|RH5418:B|}(int, string)
                                // note
                                {
                                }

                                union {|RH5418:C|}(int, string)
                                { // note
                                }

                                union {|RH5418:D|}(int, string)
                                {
                                    // reserved
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union whose body holds a region or a pragma directive is reported and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    [Requirement("SHARED-EMPTY-004")]
    public async Task UnionWithRegionOrPragmaInBodyIsReportedWithoutFix()
    {
        const string testCode = """
                                union {|RH5418:A|}(int, string)
                                {
                                    #region Members

                                    #endregion
                                }

                                union {|RH5418:B|}(int, string)
                                {
                                #pragma warning disable CS0168
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union whose case-type list holds a comment between a case type and a comma on a later line, a
    /// shape the formatter does not support, is reported and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task UnionWithUnsupportedCommentInCaseTypeListIsReportedWithoutFix()
    {
        const string testCode = """
                                union {|RH5418:U|}(int // c
                                    , string)
                                {
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a comment elsewhere in the case-type list keeps the fix, and the comment stays where it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task UnionWithCommentInCaseTypeListIsFixedAndKeepsTheComment()
    {
        const string testCode = """
                                union {|RH5418:U|}(int /* a */, string)
                                {
                                }
                                """;

        const string fixedCode = """
                                 union U(int /* a */, string);
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that the fix formats the union it fixes, so the union's attribute list with two attributes is split as
    /// the formatter splits it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-003")]
    public async Task FixSplitsTheAttributeListOfTheUnionAsTheFormatterDoes()
    {
        const string testCode = """
                                using System;

                                [Serializable, Obsolete] union {|RH5418:U|}(int, string)
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 [Serializable]
                                 [Obsolete]
                                 union U(int, string);
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union with braces followed by a semicolon is not reported, since it ends with its semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-EMPTY-001")]
    public async Task UnionWithBracesAndSemicolonIsNotReported()
    {
        const string testCode = """
                                union S1(int, string) { };
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                union {|#0:A|}(int, string)
                                {
                                }

                                class Outer
                                {
                                    union {|#1:F|}(long, string) { }
                                }
                                """;

        const string fixedCode = """
                                 union A(int, string);

                                 class Outer
                                 {
                                     union F(long, string);
                                 }
                                 """;

        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5418MessageFormat, 2),
                                  test => test.TestState.AdditionalReferences.Add(UnionSupport.GetReference()));
    }

    #endregion // BatchCodeFixTestsBase
}