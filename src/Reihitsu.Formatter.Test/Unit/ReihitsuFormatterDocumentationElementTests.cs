using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatter.FormatDocumentationElementInDocumentAsync"/>, which lays out one element of a
/// single-line documentation comment of a document, as the RH8305 and RH8307 code fixes pass it
/// </summary>
[TestClass]
public class ReihitsuFormatterDocumentationElementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example as the RH8307 fix passes it: the remarks element is laid out, and the summary the phase
    /// would also lay out, the <c>///Text</c> line, and the class stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task SpecificationExampleLaysOutTheRemarksAndKeepsTheRestOfTheDocument()
    {
        const string input = """
                             /// <summary>Total</summary>
                             /// <remarks>Sum
                             /// more</remarks>
                             ///Text
                             public class C { int A(int a) => a; }
                             """;

        const string expected = """
                                /// <summary>Total</summary>
                                /// <remarks>
                                /// Sum
                                /// more
                                /// </remarks>
                                ///Text
                                public class C { int A(int a) => a; }
                                """;

        await AssertLaysOut(input, expected, root => GetElement(root, "remarks"));
    }

    /// <summary>
    /// Verifies the spec example as the RH8305 fix passes it: the lines the summary gains start with the spaces in front of
    /// the <c>///</c> of its own line, and the comment's other line keeps its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task SpecificationExampleAddedLinesTakeTheIndentationOfTheStartTagLine()
    {
        const string input = """
                             class C
                             {
                                     /// <summary>Runs</summary>
                                 /// <remarks>Remarks</remarks>
                                 void M()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                        /// <summary>
                                        /// Runs
                                        /// </summary>
                                    /// <remarks>Remarks</remarks>
                                    void M()
                                    {
                                    }
                                }
                                """;

        await AssertLaysOut(input, expected, root => GetElement(root, "summary"));
    }

    /// <summary>
    /// Verifies that an element on a later line of a comment whose <c>///</c> lines are indented differently takes the
    /// indentation of its own start-tag line for every line it writes, not that of the comment's first line nor that of
    /// its own continuation line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task ElementOnALaterLineTakesTheIndentationOfItsOwnStartTagLine()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Runs
                                 /// </summary>
                                       /// <remarks>Sum
                                 /// more</remarks>
                                 void M()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                          /// <remarks>
                                          /// Sum
                                          /// more
                                          /// </remarks>
                                    void M()
                                    {
                                    }
                                }
                                """;

        await AssertLaysOut(input, expected, root => GetElement(root, "remarks"));
    }

    /// <summary>
    /// Verifies that the nested elements of the element are rebuilt in the same run, the content's indentation relative to
    /// the separator kept, as the spec example of the documentation-comment phase shows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task NestedElementsAreRebuiltInTheSameRun()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Gets the value.
                                 /// </summary>
                                 /// <remarks>Use it like this:
                                 /// <code>if (x)
                                 ///     M();</code>
                                 /// </remarks>
                                 int A;
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets the value.
                                    /// </summary>
                                    /// <remarks>
                                    /// Use it like this:
                                    /// <code>
                                    /// if (x)
                                    ///     M();
                                    /// </code>
                                    /// </remarks>
                                    int A;
                                }
                                """;

        await AssertLaysOut(input, expected, root => GetElement(root, "remarks"));
    }

    /// <summary>
    /// Verifies that a summary in a comment at the start of a text that begins with a byte order mark is laid out without
    /// repeating the byte order mark on the lines it adds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task ByteOrderMarkInFrontOfTheSeparatorIsNotRepeated()
    {
        const string input = "﻿/// <summary>S</summary>\npublic class C\n{\n}";
        const string expected = "﻿/// <summary>\n/// S\n/// </summary>\npublic class C\n{\n}";

        await AssertLaysOut(input, expected, root => GetElement(root, "summary"));
    }

    /// <summary>
    /// Verifies that an element already in a canonical shape is left as it is, and the document's text does not change
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task CanonicalElementIsLeftAsItIs()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// Runs
                                 /// </summary>
                                 /// <remarks>Remarks</remarks>
                                 void M(){int a=1;}
                             }
                             """;

        await AssertLaysOut(input, input, root => GetElement(root, "remarks"));
        await AssertLaysOut(input, input, root => GetElement(root, "summary"));
    }

    /// <summary>
    /// Verifies that the separator in front of the start tag and the other lines and elements of the comment, which the
    /// phase would normalize, stay as written, as do the other documentation comment of the declaration and the blank line
    /// between the two comments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-032")]
    public async Task EverythingOutsideTheElementStaysAsWritten()
    {
        const string input = """
                             namespace N
                             {
                                 class C
                                 {
                                     ///<remarks>Other</remarks>

                                     ///   <summary>Total
                                     /// sum</summary>
                                     ///<param name="v">Value
                                     ///</param>
                                     void M(int v){ }
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        ///<remarks>Other</remarks>

                                        ///   <summary>
                                        /// Total
                                        /// sum
                                        /// </summary>
                                        ///<param name="v">Value
                                        ///</param>
                                        void M(int v){ }
                                    }
                                }
                                """;

        await AssertLaysOut(input, expected, root => GetElement(root, "summary"));
    }

    /// <summary>
    /// Verifies that the element is laid out in a document the formatter refuses as unsupported: the entry point refuses no
    /// input and leaves the unsupported shape outside the element as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    public async Task ElementOfADocumentWithAnUnsupportedShapeIsLaidOut()
    {
        const string input = """
                             class C
                             {
                                 /// <summary>Total
                                 /// sum</summary>
                                 void M()
                                 {
                                     int /* c */ y = 1;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// Total
                                    /// sum
                                    /// </summary>
                                    void M()
                                    {
                                        int /* c */ y = 1;
                                    }
                                }
                                """;

        await AssertLaysOut(input, expected, root => GetElement(root, "summary"));
    }

    /// <summary>
    /// Returns the first XML element with the given name in the documentation comments of the root
    /// </summary>
    /// <param name="root">The root of the document</param>
    /// <param name="name">The element's name</param>
    /// <returns>The element</returns>
    private static XmlElementSyntax GetElement(SyntaxNode root, string name)
    {
        return root.DescendantNodes(descendIntoTrivia: true)
                   .OfType<XmlElementSyntax>()
                   .First(element => element.StartTag.Name.LocalName.ValueText == name);
    }

    /// <summary>
    /// Lays out the selected element through <see cref="ReihitsuFormatter.FormatDocumentationElementInDocumentAsync"/>
    /// under LF and CRLF, asserts the document's text, written with the document's line ending, and asserts that laying
    /// out the same element of the result again leaves the document as it is
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectElement">Selects the element in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private Task AssertLaysOut(string inputWithLf, string expectedWithLf, Func<SyntaxNode, XmlElementSyntax> selectElement)
    {
        return AssertLaysOut(inputWithLf, expectedWithLf, selectElement, _lineEndings);
    }

    /// <summary>
    /// Lays out the selected element through <see cref="ReihitsuFormatter.FormatDocumentationElementInDocumentAsync"/>
    /// under the given line endings, asserts the document's text, written with the document's line ending, and asserts
    /// that laying out the same element of the result again leaves the document as it is
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectElement">Selects the element in the document's root</param>
    /// <param name="lineEndings">The line endings to run under</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertLaysOut(string inputWithLf, string expectedWithLf, Func<SyntaxNode, XmlElementSyntax> selectElement, IEnumerable<string> lineEndings)
    {
        foreach (var endOfLine in lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
                                       .WithParseOptions(new CSharpParseOptions(documentationMode: DocumentationMode.Parse));
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(inputWithLf, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                Assert.IsNotNull(root);

                var result = await ReihitsuFormatter.FormatDocumentationElementInDocumentAsync(document, selectElement(root), TestContext.CancellationToken);
                var text = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
                var expected = NormalizeLineEndings(expectedWithLf, endOfLine);

                Assert.AreEqual(expected, text, $"Document output mismatch under {DescribeLineEnding(endOfLine)} line endings.");

                var resultRoot = await result.GetSyntaxRootAsync(TestContext.CancellationToken);

                Assert.IsNotNull(resultRoot);

                var secondPass = await ReihitsuFormatter.FormatDocumentationElementInDocumentAsync(result, selectElement(resultRoot), TestContext.CancellationToken);
                var secondPassText = (await secondPass.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(expected, secondPassText, $"Second pass changed the element under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Methods
}