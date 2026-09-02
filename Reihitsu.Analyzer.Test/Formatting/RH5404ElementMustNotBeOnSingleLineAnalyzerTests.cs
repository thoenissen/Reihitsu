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
public class RH5404ElementMustNotBeOnSingleLineAnalyzerTests : AnalyzerTestsBase<RH5404ElementMustNotBeOnSingleLineAnalyzer, RH5404ElementMustNotBeOnSingleLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that fixing an empty single-line type converges to the canonical semicolon declaration in one pass
    /// instead of producing a braced body that would be re-flagged by the empty-type semicolon rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
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
    public async Task VerifySingleLineEnumIsFlagged()
    {
        const string testData = """
                                internal enum {|#0:TestEnum|} { First }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line extension block is flagged. An extension block has no name, so the identifier
    /// the rule reports on is a default token whose location is <see cref="Location.None"/>, and the diagnostic
    /// therefore carries no source location. That is pre-existing behavior which this test pins rather than
    /// endorses - a location-less diagnostic cannot be fixed, and correcting it is tracked separately.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineExtensionBlockIsFlagged()
    {
        const string testData = """
                                internal static class Extensions
                                {
                                    extension(int value) { public int Doubled => value * 2; }
                                }
                                """;

        await Verify(testData, Diagnostic(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId).WithMessage(AnalyzerResources.RH5404MessageFormat));
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
    public async Task VerifySingleLineRecordStructIsFlagged()
    {
        const string testData = """
                                internal record struct {|#0:TestRecordStruct|} { public int Value { get; init; } }
                                """;

        await Verify(testData, Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    #endregion // Tests
}