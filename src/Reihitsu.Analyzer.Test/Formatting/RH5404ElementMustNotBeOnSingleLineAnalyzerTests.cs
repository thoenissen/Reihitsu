using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5404ElementMustNotBeOnSingleLineAnalyzer"/> and <see cref="RH5404ElementMustNotBeOnSingleLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5404ElementMustNotBeOnSingleLineAnalyzerTests : BatchCodeFixTestsBase<RH5404ElementMustNotBeOnSingleLineAnalyzer, RH5404ElementMustNotBeOnSingleLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
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
    /// Verifies that fixing a single-line type with several members places every member on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifyFixPlacesEveryMemberOnItsOwnLine()
    {
        const string testData = """
                                internal class {|#0:TestClass|} { public void Foo() { } public void Bar() { } }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public void Foo()
                                     {
                                     }
                                     public void Bar()
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
    [Requirement("RH5404-001")]
    [Requirement("RH5404-003")]
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
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
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
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
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
    [Requirement("RH5404-001")]
    [Requirement("RH5404-003")]
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
    [Requirement("RH5404-001")]
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

    /// <summary>
    /// Verifies that a single-line enum declaration is flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    public async Task VerifySingleLineEnumIsFlagged()
    {
        const string testData = """
                                internal enum {|#0:TestEnum|} { First }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line extension block is reported on its <c>extension</c> keyword, and that no code fix is
    /// offered for it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifySingleLineExtensionBlockIsFlagged()
    {
        const string testData = """
                                internal static class Extensions
                                {
                                    {|#0:extension|}(int value) { public int Doubled => value * 2; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
        await VerifyNoCodeFixIsOffered(testData.Replace("{|#0:extension|}", "{|RH5404:extension|}"));
    }

    /// <summary>
    /// Verifies that an empty single-line extension block is reported on its <c>extension</c> keyword, and that no code
    /// fix is offered for it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifyEmptySingleLineExtensionBlockIsReportedOnItsKeywordWithoutCodeFix()
    {
        const string testData = """
                                internal static class Extensions
                                {
                                    {|RH5404:extension|}(int value) { }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a generic single-line extension block is reported on its <c>extension</c> keyword, and that no code
    /// fix is offered for it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifyGenericSingleLineExtensionBlockIsReportedOnItsKeywordWithoutCodeFix()
    {
        const string testData = """
                                internal static class Extensions
                                {
                                    {|RH5404:extension|}<T>(T[] values) { public int Count2 => values.Length * 2; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a single-line extension block in a file whose first type is another declaration is reported on its
    /// own keyword, and that no code fix is offered, so no other type of the file is changed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifySingleLineExtensionBlockBehindAnotherTypeIsReportedOnItsKeywordWithoutCodeFix()
    {
        const string testData = """
                                internal class A
                                {
                                }

                                internal static class X
                                {
                                    {|RH5404:extension|}(int value) { public int Doubled => value * 2; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a single-line extension block in a file that starts with a using directive is reported on its
    /// keyword, and that no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifySingleLineExtensionBlockBehindAUsingDirectiveIsReportedOnItsKeywordWithoutCodeFix()
    {
        const string testData = """
                                using System;

                                internal static class X
                                {
                                    {|RH5404:extension|}(int value) { public int Doubled => Math.Abs(value) * 2; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the requirement's example: in a type on one line holding an extension block on one line, the type is
    /// reported on its name and the block on its <c>extension</c> keyword; the type's fix is offered and the block gets
    /// none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-002")]
    [Requirement("RH5404-003")]
    public async Task VerifySingleLineTypeHoldingASingleLineExtensionBlockKeepsItsFixWhileTheBlockGetsNone()
    {
        const string testData = """
                                class {|RH5404:A|} { class {|RH5404:B|} { } }

                                static class {|RH5404:X|} { {|RH5404:extension|}(int value) { } }
                                """;
        const string source = """
                              class A { class B { } }

                              static class X { extension(int value) { } }
                              """;

        await Verify(testData);

        var typeActions = await GetCodeFixActionsAsync(source,
                                                       RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId,
                                                       root => root.DescendantNodes()
                                                                   .OfType<ClassDeclarationSyntax>()
                                                                   .Single(declaration => declaration.Identifier.ValueText == "X")
                                                                   .Identifier
                                                                   .GetLocation());
        var blockActions = await GetCodeFixActionsAsync(source,
                                                        RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId,
                                                        root => root.DescendantNodes()
                                                                    .OfType<ExtensionBlockDeclarationSyntax>()
                                                                    .Single()
                                                                    .Keyword
                                                                    .GetLocation());

        Assert.IsNotEmpty(typeActions, "The fix of the type holding the extension block must be offered.");
        Assert.IsEmpty(blockActions, "No fix may be offered for the extension block.");
    }

    /// <summary>
    /// Verifies the requirement's example of reported and unreported declarations, the extension blocks included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    public async Task VerifyTheRequirementExampleIsReported()
    {
        const string testData = """
                                class {|RH5404:A|} { int _x; }

                                struct {|RH5404:B|} { }

                                enum {|RH5404:E|} { One, Two }

                                class D;

                                record struct P(int X);

                                class F
                                {
                                }

                                static class X
                                {
                                    {|RH5404:extension|}(int value) { public int Doubled => value * 2; }

                                    extension(string text)
                                    {
                                        public int Length2 => text.Length * 2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-line extension block is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineExtensionBlockIsNotFlagged()
    {
        const string testData = """
                                internal static class Extensions
                                {
                                    extension(int value)
                                    {
                                        public int Doubled => value * 2;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the set of declaration types reachable by the rule is still the one its registration was
    /// written against. The rule registers an explicit syntax-kind list, so a declaration type added by a future
    /// Roslyn version would silently fall out of scope; this test fails when that happens, and the registration in
    /// <see cref="RH5404ElementMustNotBeOnSingleLineAnalyzer"/> has to be revisited together with
    /// <c>documentation/rules/RH5404.md</c>.
    /// <para>
    /// This guards the declaration <em>types</em>. Individual syntax <em>kinds</em> are guarded by one positive
    /// test per registered kind, which is the check that fails if a kind is dropped from the registration - a
    /// distinction that matters because <see cref="RecordDeclarationSyntax"/> alone carries two kinds.
    /// </para>
    /// <para>
    /// <see cref="UnionDeclarationSyntax"/> appears here but is deliberately not registered; union declarations
    /// are left out until the language feature is released.
    /// </para>
    /// </summary>
    [TestMethod]
    public void VerifyBaseTypeDeclarationTypesAreFullyEnumerated()
    {
        var declarationTypes = typeof(BaseTypeDeclarationSyntax).Assembly
                                                                .GetTypes()
                                                                .Where(type => type.IsAbstract == false
                                                                               && typeof(BaseTypeDeclarationSyntax).IsAssignableFrom(type))
                                                                .Select(type => type.Name)
                                                                .OrderBy(name => name, StringComparer.Ordinal)
                                                                .ToArray();

        string[] expected = [
                                "ClassDeclarationSyntax",
                                "EnumDeclarationSyntax",
                                "ExtensionBlockDeclarationSyntax",
                                "InterfaceDeclarationSyntax",
                                "RecordDeclarationSyntax",
                                "StructDeclarationSyntax",
                                "UnionDeclarationSyntax"
                            ];

        Assert.AreSequenceEqual(expected,
                                declarationTypes,
                                $"Declaration types reachable by the rule changed: [{string.Join(", ", declarationTypes)}]");
    }

    /// <summary>
    /// Verifies that a single-line struct declaration is flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    public async Task VerifySingleLineStructIsFlagged()
    {
        const string testData = """
                                internal struct {|#0:TestStruct|} { private int _value; }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line interface declaration is flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    public async Task VerifySingleLineInterfaceIsFlagged()
    {
        const string testData = """
                                internal interface {|#0:ITestInterface|} { void Method(); }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line record declaration is flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    public async Task VerifySingleLineRecordIsFlagged()
    {
        const string testData = """
                                internal record {|#0:TestRecord|} { public int Value { get; init; } }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line record struct declaration is flagged. It carries its own syntax kind, so it is
    /// pinned separately from the record declaration above
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    public async Task VerifySingleLineRecordStructIsFlagged()
    {
        const string testData = """
                                internal record struct {|#0:TestRecordStruct|} { public int Value { get; init; } }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that a class, struct, or enum whose body is on one line is reported on its name, also when the body is
    /// empty, while declarations without a body and a body whose braces are on different lines are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-001")]
    [Requirement("RH5404-002")]
    public async Task VerifyTypeBodyOnOneLineIsReportedOnName()
    {
        const string testCode = """
                                class {|RH5404:A|} { int _x; }

                                struct {|RH5404:B|} { }

                                enum {|RH5404:E|} { One, Two }

                                class D;

                                record struct P(int X);

                                class F
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a type nested in a one-line type body is reported on its own name, apart from its container
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-002")]
    public async Task VerifyNestedTypesAreReportedOnTheirOwnNames()
    {
        const string testCode = """
                                class {|RH5404:A|} { class {|RH5404:B|} { } }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix puts the braces on their own lines and every member of a class or interface on its own
    /// line, turns an empty struct into a semicolon declaration, and keeps the member layout and the braces of an enum
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-003")]
    public async Task VerifyFixLaysOutTypeDeclarations()
    {
        const string testCode = """
                                class {|RH5404:A|} { int _x; }

                                struct {|RH5404:B|} { }

                                interface {|RH5404:I|} { void M(); }

                                enum {|RH5404:E|} { One, Two }

                                enum {|RH5404:F|} { }
                                """;
        const string fixedCode = """
                                 class A
                                 {
                                     int _x;
                                 }

                                 struct B;

                                 interface I
                                 {
                                     void M();
                                 }

                                 enum E
                                 {
                                     One, Two
                                 }

                                 enum F
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts the braces on their own lines and every member of a class or interface on its own
    /// line, turns an empty struct into a semicolon declaration, and keeps the member layout and the braces of an enum,
    /// with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5404-003")]
    public async Task VerifyFixLaysOutTypeDeclarationsWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class {|RH5404:A|} { int _x; }

                                struct {|RH5404:B|} { }

                                interface {|RH5404:I|} { void M(); }

                                enum {|RH5404:E|} { One, Two }

                                enum {|RH5404:F|} { }
                                """;
        const string fixedCode = """
                                 class A
                                 {
                                     int _x;
                                 }

                                 struct B;

                                 interface I
                                 {
                                     void M();
                                 }

                                 enum E
                                 {
                                     One, Two
                                 }

                                 enum F
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal class Outer { internal class Inner { } }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies the spec example: no blank line is inserted above a comment that leads the declaration whose body the
    /// fix lays out, where formatting the document would insert one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5404-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheDeclaration(string endOfLine)
    {
        const string testData = """
                                namespace N
                                {
                                    class A
                                    {
                                        int _y;
                                    }
                                    // c
                                    class {|RH5404:B|} { int _x; }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     class A
                                     {
                                         int _y;
                                     }
                                     // c
                                     class B
                                     {
                                         int _x;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a one-line documentation comment above the declaration whose body the fix lays out stays on one
    /// line, where formatting the document would spread it over three lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsAOneLineDocumentationCommentAboveTheDeclaration(string endOfLine)
    {
        const string testData = """
                                namespace N
                                {
                                    /// <summary>B</summary>
                                    class {|RH5404:B|} { int _x; }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     /// <summary>B</summary>
                                     class B
                                     {
                                         int _x;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a nested type whose body the fix lays out, and which shares its line with a member in front of it
    /// and a member behind it, starts on a line of its own, and that the member behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesANestedTypeThatSharesItsLineWithOtherMembersALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class A
                                {
                                    int _a; class {|RH5404:B|} { int _x; } int _b;
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     int _a;
                                     class B
                                     {
                                         int _x;
                                     }
                                     int _b;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class {|#0:Outer|} { internal class {|#1:Inner|} { } }
                                """;

        const string fixedCode = """
                                 internal class Outer
                                 {
                                     internal class Inner;
                                 }
                                 """;

        // The inner class declaration is entirely nested inside the outer class's single-line body, so the outer
        // fix's reformatted node span fully contains the inner occurrence, exercising the batch fixer's
        // overlap-discarding path rather than two independently applied changes
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}