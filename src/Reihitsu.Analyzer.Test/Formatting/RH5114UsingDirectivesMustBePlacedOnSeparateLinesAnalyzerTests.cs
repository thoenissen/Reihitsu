using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer"/> and <see cref="RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzerTests : BatchCodeFixTestsBase<RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer, RH5114UsingDirectivesMustBePlacedOnSeparateLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that using directives on separate lines are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenUsingDirectivesAreOnSeparateLines()
    {
        const string testData = """
                                using System;
                                using System.Linq;

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a directive directly behind the previous one on its line is reported and moved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ZeroGapPairIsDetectedAndFixed()
    {
        const string testData = """
                                using System;{|#0:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that a directive separated from the previous one by a space is reported and moved without leaving trailing whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SpaceSeparatedPairIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic; {|#0:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that a directive separated from the previous one by a tab is reported and moved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TabSeparatedPairIsDetectedAndFixed()
    {
        const string testData = """
                                using System;	{|#0:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that a block comment between the directives stays on the line of the previous directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BlockCommentStaysWithPreviousDirective()
    {
        const string testData = """
                                using System; /* core */ {|#0:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System; /* core */
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that every directive after the first one of a shared line is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ChainOfDirectivesOnOneLineReportsEveryFollowingDirective()
    {
        const string testData = """
                                using System; {|#0:using System.IO;|} {|#1:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.IO;
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a directive moved inside a block namespace receives the indentation of the namespace's first directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveInBlockNamespaceIsMovedToTheScopeIndentation()
    {
        const string testData = """
                                namespace Example
                                {
                                    using System; {|#0:using System.Linq;|}

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace Example
                                 {
                                     using System;
                                     using System.Linq;

                                     internal class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that directives sharing a line inside a file-scoped namespace are reported and split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveInFileScopedNamespaceIsDetectedAndFixed()
    {
        const string testData = """
                                namespace Example;

                                using System; {|#0:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace Example;

                                 using System;
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that global using directives sharing a line are reported and split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task GlobalDirectivesAreDetectedAndFixed()
    {
        const string testData = """
                                global using System; {|#0:global using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 global using System;
                                 global using System.Linq;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that static using directives sharing a line are reported and split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticDirectivesAreDetectedAndFixed()
    {
        const string testData = """
                                using static System.Math; {|#0:using static System.Console;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using static System.Math;
                                 using static System.Console;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that alias directives sharing a line are reported and split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AliasDirectivesAreDetectedAndFixed()
    {
        const string testData = """
                                using Text = System.Text; {|#0:using IO = System.IO;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using Text = System.Text;
                                 using IO = System.IO;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that directives of different groups sharing a line are reported and split without adding the group separator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CrossGroupPairIsDetectedAndSplitWithoutBlankLine()
    {
        const string testData = """
                                using System; {|#0:using Microsoft.Win32;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using Microsoft.Win32;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that directives in non-canonical order are split but keep their order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OutOfOrderPairIsSplitWithoutReordering()
    {
        const string testData = """
                                using System.Linq; {|#0:using System.Collections.Generic;|}

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System.Linq;
                                 using System.Collections.Generic;

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that directives sharing a line are reported and split even when a preprocessor directive prevents the block from being reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PairInBlockWithPreprocessorDirectiveIsDetectedAndFixed()
    {
        const string testData = """
                                using System; {|#0:using System.Linq;|}
                                #pragma warning disable CS8019
                                using System.IO;
                                #pragma warning restore CS8019

                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;
                                 #pragma warning disable CS8019
                                 using System.IO;
                                 #pragma warning restore CS8019

                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that a directive behind a line comment that ends the previous directive's line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenLineCommentEndsThePreviousLine()
    {
        const string testData = """
                                using System; // core
                                using System.Linq;

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a directive behind a block comment that spans lines is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenMultiLineBlockCommentSeparatesTheDirectives()
    {
        const string testData = """
                                using System; /* core
                                   helpers */ using System.Linq;

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that extern alias directives sharing a line with each other or with a using directive are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticForExternAliasDirectives()
    {
        const string testData = """
                                extern alias First; extern alias Second; using System;

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that directives of different scopes are not treated as neighbors
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticForDirectivesInDifferentScopes()
    {
        const string testData = """
                                using System; namespace Example { using System.Linq;

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the first directive of a namespace sharing the line of the opening brace is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticForSingleDirectiveSharingTheLineOfTheNamespaceBrace()
    {
        const string testData = """
                                namespace Example { using System;

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a directive behind a single-line documentation comment that ends the previous
    /// directive's line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenSingleLineDocumentationCommentEndsThePreviousLine()
    {
        const string testData = """
                                using System; /// core
                                using System.Linq;

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a directive moved inside a namespace whose first directive shares the line of the
    /// opening brace receives no indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveInSingleLineNamespaceHeaderIsMovedWithoutIndentation()
    {
        const string testData = """
                                namespace Example { using System; {|#0:using System.Linq;|}

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace Example { using System;
                                 using System.Linq;

                                     internal class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix inserts the line ending of a CRLF document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixUsesTheLineEndingOfTheDocument()
    {
        const string testData = "using System; using System.Linq;\r\n\r\ninternal class TestClass\r\n{\r\n}\r\n";
        const string expected = "using System;\r\nusing System.Linq;\r\n\r\ninternal class TestClass\r\n{\r\n}\r\n";

        var actual = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a form feed between the directives is removed together with the other whitespace, so
    /// the fix leaves no trailing whitespace behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FormFeedBetweenDirectivesIsRemovedByTheFix()
    {
        const string testData = "using System;\fusing System.Linq;\n\ninternal class TestClass\n{\n}\n";
        const string expected = "using System;\nusing System.Linq;\n\ninternal class TestClass\n{\n}\n";

        var actual = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a no-break space between the directives is removed together with the other whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoBreakSpaceBetweenDirectivesIsRemovedByTheFix()
    {
        const string testData = "using System;\u00A0using System.Linq;\n\ninternal class TestClass\n{\n}\n";
        const string expected = "using System;\nusing System.Linq;\n\ninternal class TestClass\n{\n}\n";

        var actual = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the directive is still reported but no fix is offered when a delimited documentation
    /// comment leads it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedWhenDocumentationCommentLeadsTheMovedDirective()
    {
        const string testData = """
                                using System; /** core */ {|#0:using System.Linq;|}

                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData, testData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that no fix is offered when a syntax error sits between the directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixIsNotOfferedWhenSyntaxErrorSitsBetweenTheDirectives()
    {
        const string testData = """
                                using System; ` using System.Linq;

                                internal class TestClass
                                {
                                }
                                """;

        var (usingDirectiveCount, hasSyntaxError) = InspectParsedSource(testData);

        Assert.AreEqual(2, usingDirectiveCount);
        Assert.IsTrue(hasSyntaxError);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<UsingDirectiveSyntax>()
                                                               .Last()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the fix places the moved directive at the indentation of its scope's line when a block
    /// comment precedes the first directive on that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveBehindCommentedFirstDirectiveIsMovedToTheLineIndentation()
    {
        const string testData = """
                                namespace Example
                                {
                                    /* core */ using System; {|#0:using System.Linq;|}

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace Example
                                 {
                                     /* core */ using System;
                                     using System.Linq;

                                     internal class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix adds no indentation when the first directive's line starts inside a comment
    /// that spans lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveBehindCommentContinuationLineIsMovedWithoutIndentation()
    {
        const string testData = "/* first\n   second */ using System; using System.Linq;\n\ninternal class TestClass\n{\n}\n";
        const string expected = "/* first\n   second */ using System;\nusing System.Linq;\n\ninternal class TestClass\n{\n}\n";

        var actual = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the fix places the moved directive at the indentation of the line on which a comment
    /// continued by the first directive's line starts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveBehindIndentedCommentContinuationLineIsMovedToTheCommentIndentation()
    {
        const string testData = "namespace Example\n{\n    /* first\n       second */ using System; using System.Linq;\n\n    internal class TestClass\n    {\n    }\n}\n";
        const string expected = "namespace Example\n{\n    /* first\n       second */ using System;\n    using System.Linq;\n\n    internal class TestClass\n    {\n    }\n}\n";

        var actual = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the fix reads the indentation from the first directive's own line rather than from a
    /// less indented header comment above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DirectiveBelowUnindentedHeaderCommentIsMovedToTheDirectiveIndentation()
    {
        const string testData = """
                                namespace Example
                                {
                                // Header
                                    using System; {|#0:using System.Linq;|}

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace Example
                                 {
                                 // Header
                                     using System;
                                     using System.Linq;

                                     internal class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and reports how many using directives the compilation unit holds and whether the
    /// parse produced a syntax error, so a test can prove its input really reaches the code fix
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The number of using directives and whether a syntax error was reported</returns>
    private static (int UsingDirectiveCount, bool HasSyntaxError) InspectParsedSource(string source)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();

        return (root.Usings.Count, root.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System; {|#0:using System.IO;|} {|#1:using System.Linq;|}

                                namespace Example
                                {
                                    using System.Text; {|#2:using System.Xml;|}

                                    internal class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;
                                 using System.IO;
                                 using System.Linq;

                                 namespace Example
                                 {
                                     using System.Text;
                                     using System.Xml;

                                     internal class TestClass
                                     {
                                     }
                                 }
                                 """;

        // Verifies a chain in the compilation unit and a pair in a namespace are all split in one Fix All iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat, 3),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}