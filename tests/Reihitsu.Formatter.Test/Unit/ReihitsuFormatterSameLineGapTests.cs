using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that the node-level entry points the code fixes use space the gaps the spacing phase owns like whole-document
/// formatting does: a property arrow, a one-line accessor list, accessor modifiers joined in a collapsed list, a named
/// argument, and a lambda
/// </summary>
[TestClass]
public class ReihitsuFormatterSameLineGapTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> spaces the arrow of an expression-bodied property and the inside of
    /// a one-line accessor list, written on one line or joined by the formatter
    /// </summary>
    /// <param name="property">The property declaration</param>
    /// <param name="expected">The formatted property</param>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    [Requirement("FMT-SPACE-022")]
    [DataRow("int P =>1;", "int P => 1;")]
    [DataRow("int P=>1;", "int P => 1;")]
    [DataRow("int P\n    =>1;", "int P => 1;")]
    [DataRow("int P {get;set;}", "int P { get; set; }")]
    [DataRow("int P {[System.Obsolete] get;set;}", "int P { [System.Obsolete] get; set; }")]
    [DataRow("int P\n{\n    get;\n    set;\n}", "int P { get; set; }")]
    public void FormatNodeSpacesThePropertyArrowAndTheAccessorList(string property, string expected)
    {
        var result = FormatPropertyNode(property);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> keeps two accessor modifiers it joins in a collapsed list apart
    /// </summary>
    /// <param name="first">The modifier on the first line</param>
    /// <param name="second">The modifier on the second line</param>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    [DataRow("protected", "internal")]
    [DataRow("internal", "protected")]
    [DataRow("private", "protected")]
    [DataRow("protected", "private")]
    public void FormatNodeKeepsJoinedAccessorModifiersApart(string first, string second)
    {
        var result = FormatPropertyNode($"public int P\n{{\n    get;\n    {first}\n    {second} set;\n}}");

        Assert.AreEqual($"public int P {{ get; {first} {second} set; }}", result);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> spaces the colon of a named argument and the arrow of a lambda
    /// when the argument is the formatted root
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-019")]
    [Requirement("FMT-SPACE-017")]
    public void FormatNodeSpacesANamedLambdaArgument()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var source = NormalizeLineEndings("class C\n{\n    void M()\n    {\n        N(f:y=>y);\n    }\n}", endOfLine);
            var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
            var argument = tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<ArgumentSyntax>().Single();

            var result = ReihitsuFormatter.FormatNode(argument, cancellationToken: TestContext.CancellationToken).Output;

            Assert.AreEqual("f: y => y", result.ToString(), $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> spaces the arrow of the target property and leaves
    /// the next property as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public async Task FormatNodeInDocumentAsyncSpacesThePropertyArrow()
    {
        const string input = """
                             class C
                             {
                                 int P
                                     =>1;

                                 int Q =>1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P => 1;

                                    int Q =>1;
                                }
                                """;

        await AssertFormatNodeInDocument(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> spaces the accessor list of the target property
    /// and leaves the next property as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public async Task FormatNodeInDocumentAsyncSpacesTheAccessorList()
    {
        const string input = """
                             class C
                             {
                                 int P {get;set;}

                                 int Q {get;set;}
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { get; set; }

                                    int Q {get;set;}
                                }
                                """;

        await AssertFormatNodeInDocument(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> keeps two accessor modifiers it joins in a collapsed
    /// list apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public async Task FormatNodeInDocumentAsyncKeepsJoinedAccessorModifiersApart()
    {
        const string input = """
                             class C
                             {
                                 public int P
                                 {
                                     get;
                                     protected
                                     internal set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int P { get; protected internal set; }
                                }
                                """;

        await AssertFormatNodeInDocument(input, expected);
    }

    /// <summary>
    /// Formats the first property of the given class with <see cref="ReihitsuFormatter.FormatNode"/> under LF and CRLF line
    /// endings and returns its text without leading and trailing trivia, after asserting that both line endings give the same text
    /// </summary>
    /// <param name="property">The property declaration, with LF line endings</param>
    /// <returns>The formatted property's text</returns>
    private string FormatPropertyNode(string property)
    {
        string result = null;

        foreach (var endOfLine in _lineEndings)
        {
            var source = NormalizeLineEndings($"class C\n{{\n{property}\n}}", endOfLine);
            var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
            var target = tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<PropertyDeclarationSyntax>().First();
            var text = ReihitsuFormatter.FormatNode(target, cancellationToken: TestContext.CancellationToken).Output.ToString();

            if (result == null)
            {
                result = text;
            }
            else
            {
                Assert.AreEqual(result, text, $"FormatNode differs under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }

        return result;
    }

    /// <summary>
    /// Formats the first property of the given document with <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> under LF
    /// and CRLF line endings and asserts the document's text
    /// </summary>
    /// <param name="input">The document text</param>
    /// <param name="expected">The expected document text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatNodeInDocument(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<PropertyDeclarationSyntax>().First();

                Assert.IsNotNull(target);

                var result = (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)).Output;
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), resultText, $"Under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Methods
}