using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
    /// Source of the FMT-PIPE-031 example with a documented declaration, a declaration with an ordinary comment in front of it,
    /// one with a comment between a modifier and the type, and one with a comment in front of its first declarator's value
    /// </summary>
    private const string DocumentationAndCommentExample = """
                                                          class C
                                                          {
                                                              /// <summary>
                                                              /// The coordinates
                                                              /// </summary>
                                                              public int X, Y;

                                                              // The sizes
                                                              public int Width, Height;

                                                              private /* c */ int _first, _second;

                                                              private int _g = /* c */ 1, _h;
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
    /// Verifies the spec example: the method returns false for the documented declaration, and true for the declaration with an
    /// ordinary comment in front of it, for the one with a comment between a modifier and the type, and for the one with a
    /// comment in front of its first declarator's value; <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> returns the comment
    /// between the modifier and the type for that declaration and for the class
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void SpecExampleReturnsFalseOnlyForTheDocumentedDeclaration()
    {
        var root = CSharpSyntaxTree.ParseText(DocumentationAndCommentExample, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsFalse(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "X")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "Width")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_first")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_g")));

        var commentStart = DocumentationAndCommentExample.IndexOf("/* c */ int", StringComparison.Ordinal);

        foreach (var node in new SyntaxNode[] { FindField(root, "_first"), root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single() })
        {
            var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

            Assert.IsNotNull(unsupported, $"The {node.Kind()} must be refused.");
            Assert.AreEqual(new TextSpan(commentStart, "/* c */".Length), unsupported.Location.SourceSpan);
        }
    }

    /// <summary>
    /// Verifies the spec example in the documentation mode None: the documentation-style comment is an ordinary comment, so the
    /// method returns true for the declaration it precedes as well
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-031")]
    public void ReturnsTrueForADeclarationWithADocumentationStyleCommentInDocumentationModeNone()
    {
        var root = CSharpSyntaxTree.ParseText(DocumentationAndCommentExample, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "X")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "Width")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_first")));
        Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_g")));
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
    /// Verifies that the method returns true for a combined declaration with a comment anywhere in it, in every documentation
    /// mode: whether the formatter refuses the declaration as unsupported is not part of the answer
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
    [DataRow("    private int /** c */ _x, _y;")]
    [DataRow("    private int /** c */\n        _x, _y;")]
    [DataRow("    private int /// <summary>c</summary>\n        _x, _y;")]
    [DataRow("    private System.Collections.Generic.List<int> /// c\n        _x, _y;")]
    [DataRow("    private int[] /** c */ _x, _y;")]
    [DataRow("    [System.Obsolete]\n    private int /** c */ _x, _y;")]
    [DataRow("    private int /** c */ /* d */ _x, _y;")]
    [DataRow("    private int /** c */ /* d */\n        _x, _y;")]
    [DataRow("    private int /** c */ // d\n        _x, _y;")]
    [DataRow("    private int /** c */ /* d\n       e */ _x, _y;")]
    [DataRow("    private int /** c */ /** d */ _x, _y;")]
    [DataRow("    private int /** c */ /* d */ /* e */ _x, _y;")]
    [DataRow("    private int /// c\n        /* d */ _x, _y;")]
    [DataRow("    private System.Collections.Generic.List<int> /** c */ /* d */ _x, _y;")]
    [DataRow("    [System.Obsolete]\n    private int /** c */ /* d */ _x, _y;")]
    [DataRow("    private int /* d */ /** c */ _x, _y;")]
    [DataRow("    private int /* d */ /** c */\n        _x, _y;")]
    [DataRow("    private int /* d */ /// c\n        _x, _y;")]
    [DataRow("    private int // d\n        /** c */ _x, _y;")]
    [DataRow("    private /** c */ /* d */ int _x, _y;")]
    [DataRow("    [System.Obsolete] /** c */ /* d */\n    private int _x, _y;")]
    public void ReturnsTrueForACombinedDeclarationWithACommentInEveryDocumentationMode(string declaration)
    {
        var source = $"class C\n{{\n{declaration}\n}}";

        foreach (var documentationMode in new[] { DocumentationMode.Parse, DocumentationMode.Diagnose, DocumentationMode.None })
        {
            var root = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithDocumentationMode(documentationMode), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

            Assert.IsTrue(ReihitsuFormatter.SplitsFieldDeclaration(FindField(root, "_x")), $"Documentation mode {documentationMode}");
        }
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