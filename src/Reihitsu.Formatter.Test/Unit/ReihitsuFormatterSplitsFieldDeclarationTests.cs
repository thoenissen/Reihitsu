using System.Linq;

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