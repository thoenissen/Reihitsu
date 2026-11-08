using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatter.SplitsFieldDeclaration"/>, which answers whether the structural phase splits a field
/// declaration, for the field declaration of an RH7101 diagnostic
/// </summary>
[TestClass]
public class ReihitsuFormatterSplitsFieldDeclarationTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Source of the FMT-PIPE-031 example with a documented declaration, a declaration with a comment behind the type, one
    /// with a comment in its first declarator's initializer, and one with a <c>/** */</c> comment behind the type
    /// </summary>
    private const string DocumentationAndCommentExample = """
                                                          class C
                                                          {
                                                              /// <summary>
                                                              /// The coordinates
                                                              /// </summary>
                                                              public int X, Y;

                                                              private int /* c */ _first, _second;

                                                              private int _a = /* c */ 1, _b;

                                                              private int /** c */ _g, _h;
                                                          }
                                                          """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies the spec example: the method returns true for a combined declaration without a directive, and false for one with
    /// an <c>#if</c> directive between its declarators
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void ReturnsTrueForACombinedDeclarationAndFalseForOneWithADirective()
    {
        const string source = """
                              class C
                              {
                                  private int _a, _b;
                                  private int _c,
                              #if DEBUG
                                              _d,
                              #endif
                                              _e;
                              }
                              """;

        var root = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_a")));
        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_c")));
    }

    /// <summary>
    /// Verifies that the method returns true for a combined declaration in every type kind the structural phase splits in
    /// </summary>
    /// <param name="keyword">The keyword that declares the type</param>
    /// <param name="modifiers">The modifiers of the fields</param>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    [DataRow("class", "private")]
    [DataRow("struct", "private")]
    [DataRow("record", "private")]
    [DataRow("record struct", "private")]
    [DataRow("interface", "private static")]
    public void ReturnsTrueForACombinedDeclarationInEveryTypeKindTheSplitSplitsIn(string keyword, string modifiers)
    {
        var root = CSharpSyntaxTree.ParseText($"{keyword} T\n{{\n    {modifiers} int _x, _y;\n}}", cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")));
    }

    /// <summary>
    /// Verifies that the method returns false for a declaration with one declarator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void ReturnsFalseForADeclarationWithOneDeclarator()
    {
        var root = CSharpSyntaxTree.ParseText("class C\n{\n    private int _x;\n}", cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")));
    }

    /// <summary>
    /// Verifies that the method returns false for a combined declaration in an extension block, which is no class, struct,
    /// record, or interface
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void ReturnsFalseForACombinedDeclarationInAnExtensionBlock()
    {
        const string source = """
                              static class E
                              {
                                  extension(int i)
                                  {
                                      private static int _x, _y;
                                  }
                              }
                              """;

        var root = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")));
    }

    /// <summary>
    /// Verifies that the method returns false for a combined declaration with a directive in its leading trivia or between its
    /// declarators, as the split refuses it
    /// </summary>
    /// <param name="declaration">The combined declaration with its directive</param>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    [DataRow("    #region Fields\n    private int _x, _y;\n    #endregion")]
    [DataRow("    private int _x,\n#pragma warning disable CS0169\n                _y;\n#pragma warning restore CS0169")]
    [DataRow("#if DEBUG\n    private int _x, _y;\n#endif")]
    public void ReturnsFalseForACombinedDeclarationThatCarriesADirective(string declaration)
    {
        var options = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");
        var root = CSharpSyntaxTree.ParseText($"class C\n{{\n{declaration}\n}}", options, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")));
    }

    /// <summary>
    /// Verifies the spec example: when the documentation mode is Parse, the method returns false for a documented declaration and
    /// for one with a comment behind the type, and true for one with a comment in its first declarator's initializer and for one
    /// with a <c>/** */</c> comment behind the type, which belongs to the first declarator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void ReturnsFalseForADocumentedDeclarationAndForOneWithACommentBehindTheType()
    {
        var root = CSharpSyntaxTree.ParseText(DocumentationAndCommentExample, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "X")));
        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_first")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_a")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_g")));
    }

    /// <summary>
    /// Verifies the spec example in the documentation mode None: the documentation-style comment is an ordinary comment, so the
    /// method returns true for the declaration it precedes, while the comment behind the type still keeps its declaration whole,
    /// and so does the <c>/** */</c> comment behind the type, which is an ordinary comment as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void ReturnsTrueForADeclarationWithADocumentationStyleCommentInDocumentationModeNone()
    {
        var root = CSharpSyntaxTree.ParseText(DocumentationAndCommentExample, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "X")));
        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_first")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_a")));
        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_g")));
    }

    /// <summary>
    /// Verifies that the method returns false for a declaration documented with a <c>///</c> or a <c>/** */</c> comment in the
    /// documentation modes Parse and Diagnose, also when the comment is written behind the code of the member in front or in front
    /// of the attribute list, and true in the documentation mode None
    /// </summary>
    /// <param name="declaration">The documented combined declaration</param>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    [DataRow("    /// <summary>The coordinates</summary>\n    public int X, _y;")]
    [DataRow("    /** <summary>The sizes</summary> */\n    public int X, _y;")]
    [DataRow("    private int _p; /// <summary>X.</summary>\n    public int X, _y;")]
    [DataRow("    private int _p; /** X. */ public int X, _y;")]
    [DataRow("    /// <summary>The coordinates</summary>\n    [System.Obsolete]\n    public int X, _y;")]
    public void ReturnsFalseForADocumentedDeclarationUnlessDocumentationCommentsAreNotParsed(string declaration)
    {
        var source = $"class C\n{{\n{declaration}\n}}";

        foreach (var documentationMode in new[] { DocumentationMode.Parse, DocumentationMode.Diagnose })
        {
            var root = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithDocumentationMode(documentationMode), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

            Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "X")), $"Documentation mode {documentationMode}");
        }

        var rootWithoutDocumentation = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(rootWithoutDocumentation, "X")));
    }

    /// <summary>
    /// Verifies that the method returns false for a combined declaration with a comment between its first token and its first
    /// declarator, in every slot of the part every field repeats
    /// </summary>
    /// <param name="declaration">The combined declaration with its comment</param>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    [DataRow("    [System.Obsolete /* c */]\n    private int _x, _y;")]
    [DataRow("    [System.Obsolete] /* c */\n    private int _x, _y;")]
    [DataRow("    [System.Obsolete]\n    // c\n    private int _x, _y;")]
    [DataRow("    [System.Obsolete]\n    /// <summary>m</summary>\n    public int _x, _y;")]
    [DataRow("    public /* c */ static int _x, _y;")]
    [DataRow("    private /* c */ int _x, _y;")]
    [DataRow("    private System. /* c */ Int32 _x, _y;")]
    [DataRow("    private int /* c */ _x, _y;")]
    [DataRow("    private int // c\n        _x, _y;")]
    [DataRow("    private int /* c */\n        _x, _y;")]
    [DataRow("    private /* a\n       b */ int _x, _y;")]
    public void ReturnsFalseForACombinedDeclarationWithACommentInThePartEveryFieldRepeats(string declaration)
    {
        foreach (var documentationMode in new[] { DocumentationMode.Parse, DocumentationMode.None })
        {
            var root = CSharpSyntaxTree.ParseText($"class C\n{{\n{declaration}\n}}", CSharpParseOptions.Default.WithDocumentationMode(documentationMode), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

            Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")), $"Documentation mode {documentationMode}");
        }
    }

    /// <summary>
    /// Verifies that the method returns true for a combined declaration whose comments stand outside the part every field repeats:
    /// in its leading trivia, on a line of its own in front of the first declarator's name, inside or behind the first declarator,
    /// behind a separating comma, and in front of a later declarator
    /// </summary>
    /// <param name="declaration">The combined declaration with its comment</param>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    [DataRow("    // The coordinates\n    private int _x, _y;")]
    [DataRow("    /* c */ private int _x, _y;")]
    [DataRow("    private int\n        /* c */ _x, _y;")]
    [DataRow("    private int\n        // c\n        _x, _y;")]
    [DataRow("    private int\n        /// <summary>First.</summary>\n        _x, _y;")]
    [DataRow("    private int\n        /** c */ _x, _y;")]
    [DataRow("    private int _x = /* c */ 1, _y;")]
    [DataRow("    private int _x /* c */, _y;")]
    [DataRow("    private int _x, /* c */ _y;")]
    [DataRow("    private int _x, // c\n                _y;")]
    [DataRow("    private int _x,\n                /// <summary>Second.</summary>\n                _y;")]
    [DataRow("    private int _x,\n                /** Second. */\n                _y;")]
    public void ReturnsTrueForACombinedDeclarationWithACommentOutsideThePartEveryFieldRepeats(string declaration)
    {
        var root = CSharpSyntaxTree.ParseText($"class C\n{{\n{declaration}\n}}", CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")));
    }

    /// <summary>
    /// Verifies that the method returns true for a combined declaration with a <c>///</c> or <c>/** */</c> comment behind the type
    /// on the type's line in the documentation modes Parse and Diagnose, where the comment belongs to the first declarator, also
    /// when the first declarator starts the next line, and false in the documentation mode None, where it is an ordinary comment
    /// behind the type
    /// </summary>
    /// <param name="declaration">The combined declaration with its comment</param>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    [DataRow("    private int /** c */ _x, _y;")]
    [DataRow("    private int /** c */\n        _x, _y;")]
    [DataRow("    private int /// <summary>c</summary>\n        _x, _y;")]
    [DataRow("    private System.Collections.Generic.List<int> /// c\n        _x, _y;")]
    [DataRow("    private int[] /** c */ _x, _y;")]
    [DataRow("    [System.Obsolete]\n    private int /** c */ _x, _y;")]
    public void ReturnsTrueForADocumentationStyleCommentBehindTheTypeUnlessDocumentationCommentsAreNotParsed(string declaration)
    {
        var source = $"class C\n{{\n{declaration}\n}}";

        foreach (var documentationMode in new[] { DocumentationMode.Parse, DocumentationMode.Diagnose })
        {
            var root = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithDocumentationMode(documentationMode), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

            Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")), $"Documentation mode {documentationMode}");
        }

        var rootWithoutDocumentation = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(rootWithoutDocumentation, "_x")));
    }

    /// <summary>
    /// Finds the field declaration that declares the variable with the given name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="variableName">The declared variable name</param>
    /// <returns>The field declaration</returns>
    private static FieldDeclarationSyntax FindField(Microsoft.CodeAnalysis.SyntaxNode root, string variableName)
    {
        return root.DescendantNodes()
                   .OfType<FieldDeclarationSyntax>()
                   .Single(field => field.Declaration.Variables.Any(variable => variable.Identifier.ValueText == variableName));
    }

    #endregion // Methods
}