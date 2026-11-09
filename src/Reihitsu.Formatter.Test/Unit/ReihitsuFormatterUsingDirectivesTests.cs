using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>, which organizes the using block of one scope
/// of a document, as the RH7201–RH7207 code fixes pass it
/// </summary>
[TestClass]
public class ReihitsuFormatterUsingDirectivesTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: with a namespace as scope, the directive that shared a line comes first, each directive stands
    /// on a line of its own at the scope's indentation, and the class behind the block is not formatted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task NamespaceScopeIsOrganizedAndTheCodeBehindTheBlockIsNotFormatted()
    {
        const string input = """
                             namespace N
                             {
                                 using System.Linq; using System;

                                 class C{ }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    using System;
                                    using System.Linq;

                                    class C{ }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies that the using block of a document that the formatter refuses as unsupported is organized: the method runs no
    /// formatting phase but the using phase and refuses no input
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    public async Task UsingBlockOfADocumentWithAnUnsupportedShapeIsOrganized()
    {
        const string input = """
                             namespace N
                             {
                                 using System.Linq; using System;

                                 class C{ int /* c */ _a; }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    using System;
                                    using System.Linq;

                                    class C{ int /* c */ _a; }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies the spec example: a scope that does not start its line is read where it stands in the document, so the directive
    /// moved onto a line of its own gets the indentation of the line the block's first directive starts on, which is none, and the
    /// code behind the block stays on its line. The input holds no line break, so the formatter writes
    /// <see cref="Environment.NewLine"/> (FMT-PIPE-007), and the test runs under that line ending only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task ScopeThatDoesNotStartItsLineIsReadWhereItStandsInTheDocument()
    {
        const string input = "namespace A { namespace B { using System.Linq; using System; class C { } } }";
        const string expected = "namespace A { namespace B { using System;\nusing System.Linq; class C { } } }";

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single(scope => scope.Name.ToString() == "B"), [Environment.NewLine]);
    }

    /// <summary>
    /// Verifies that the compilation unit as scope gets its block organized, and the block of a namespace nested in it is left as it
    /// is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task CompilationUnitScopeIsOrganizedAndANestedNamespaceIsLeftAsItIs()
    {
        const string input = """
                             using System.Linq;
                             using System;

                             namespace N
                             {
                                 using System.Text; using System.Collections;

                                 class C{ }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Linq;

                                namespace N
                                {
                                    using System.Text; using System.Collections;

                                    class C{ }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that a file-scoped namespace as scope gets its block organized, each directive on a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task FileScopedNamespaceScopeIsOrganized()
    {
        const string input = """
                             namespace N;

                             using System.Linq; using System;

                             class C{ }
                             """;

        const string expected = """
                                namespace N;

                                using System;
                                using System.Linq;

                                class C{ }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies that the entry point organizes the block of a document that contains a syntax error, which it does not check
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-PIPE-005")]
    public async Task BlockOfADocumentWithASyntaxErrorIsOrganized()
    {
        const string input = """
                             using System.Linq;
                             using System;

                             class C{ void M(){ var a = ; } }
                             """;

        const string expected = """
                                using System;
                                using System.Linq;

                                class C{ void M(){ var a = ; } }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that the entry point organizes the block of a document whose header marks it as generated, which it does not check
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-PIPE-006")]
    public async Task BlockOfADocumentWithAGeneratedHeaderIsOrganized()
    {
        const string input = """
                             // <auto-generated/>
                             using System.Linq;
                             using System;

                             class C{ }
                             """;

        const string expected = """
                                // <auto-generated/>
                                using System;
                                using System.Linq;

                                class C{ }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Organizes the using block of the selected scope through <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>
    /// under LF and CRLF, and asserts the document's text, written with the document's line ending
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectScope">Selects the scope in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private Task AssertOrganizes(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectScope)
    {
        return AssertOrganizes(inputWithLf, expectedWithLf, selectScope, _lineEndings);
    }

    /// <summary>
    /// Organizes the using block of the selected scope through <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>
    /// under the given line endings, and asserts the document's text, written with the document's line ending
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectScope">Selects the scope in the document's root</param>
    /// <param name="lineEndings">The line endings to run under</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertOrganizes(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectScope, IEnumerable<string> lineEndings)
    {
        foreach (var endOfLine in lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(inputWithLf, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                Assert.IsNotNull(root);

                var result = await ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync(document, selectScope(root), TestContext.CancellationToken);
                var text = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(expectedWithLf, endOfLine), text, $"Document output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Methods
}