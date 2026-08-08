using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxTriviaUtilities"/>
/// </summary>
[TestClass]
public class SyntaxTriviaUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a single-line (///) documentation comment is recognized as a documentation comment
    /// </summary>
    [TestMethod]
    public void IsDocumentationCommentTriviaReturnsTrueForSingleLineDocumentationComment()
    {
        var trivia = GetFirstTrivia("/// <summary>Doc.</summary>\nclass TestClass;\n", SyntaxKind.SingleLineDocumentationCommentTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment is recognized as a documentation comment
    /// </summary>
    [TestMethod]
    public void IsDocumentationCommentTriviaReturnsTrueForMultiLineDocumentationComment()
    {
        var trivia = GetFirstTrivia("/** <summary>Doc.</summary> */\nclass TestClass;\n", SyntaxKind.MultiLineDocumentationCommentTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia));
    }

    /// <summary>
    /// Verifies that an ordinary single-line comment is not treated as a documentation comment
    /// </summary>
    [TestMethod]
    public void IsDocumentationCommentTriviaReturnsFalseForSingleLineComment()
    {
        var trivia = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia));
    }

    /// <summary>
    /// Verifies that an ordinary multi-line comment is not treated as a documentation comment
    /// </summary>
    [TestMethod]
    public void IsDocumentationCommentTriviaReturnsFalseForMultiLineComment()
    {
        var trivia = GetFirstTrivia("/* note */\nvar x = 1;\n", SyntaxKind.MultiLineCommentTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsDocumentationCommentTrivia(trivia));
    }

    /// <summary>
    /// Verifies that a preprocessor directive is recognized as directive trivia
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsTrueForDirective()
    {
        var trivia = GetFirstTrivia("#if DEBUG\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that disabled text is recognized as protected trivia
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsTrueForDisabledText()
    {
        var trivia = GetFirstTrivia("#if UNDEFINED\nvar x = 1;\n#endif\n", SyntaxKind.DisabledTextTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that a comment is not treated as a directive or disabled text
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsFalseForComment()
    {
        var trivia = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that whitespace is not treated as a directive or disabled text
    /// </summary>
    [TestMethod]
    public void IsDirectiveOrDisabledTextTriviaReturnsFalseForWhitespace()
    {
        var trivia = GetFirstTrivia("    var x = 1;\n", SyntaxKind.WhitespaceTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Verifies that the first significant trivia index skips whitespace and end-of-line trivia
    /// </summary>
    [TestMethod]
    public void FindFirstSignificantTriviaIndexSkipsWhitespaceAndEndOfLineTrivia()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "),
                                              SyntaxFactory.EndOfLine("\n"),
                                              SyntaxFactory.Comment("// note"));

        Assert.AreEqual(2, SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(trivia));
    }

    /// <summary>
    /// Verifies that the first significant trivia index is zero when content comes first
    /// </summary>
    [TestMethod]
    public void FindFirstSignificantTriviaIndexReturnsZeroForLeadingContent()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Comment("// note"), SyntaxFactory.EndOfLine("\n"));

        Assert.AreEqual(0, SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(trivia));
    }

    /// <summary>
    /// Verifies that no significant trivia index is returned for whitespace-only trivia
    /// </summary>
    [TestMethod]
    public void FindFirstSignificantTriviaIndexReturnsMinusOneForWhitespaceOnlyTrivia()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "), SyntaxFactory.EndOfLine("\n"));

        Assert.AreEqual(-1, SyntaxTriviaUtilities.FindFirstSignificantTriviaIndex(trivia));
    }

    /// <summary>
    /// Verifies that comments prevent adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsTrueForComment()
    {
        var comment = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(SyntaxFactory.TriviaList(comment)));
    }

    /// <summary>
    /// Verifies that directives prevent adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsTrueForDirective()
    {
        var directive = GetFirstTrivia("#if DEBUG\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(SyntaxFactory.TriviaList(directive)));
    }

    /// <summary>
    /// Verifies that disabled text prevents adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsTrueForDisabledText()
    {
        var disabledText = GetFirstTrivia("#if UNDEFINED\nvar x = 1;\n#endif\n", SyntaxKind.DisabledTextTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(SyntaxFactory.TriviaList(disabledText)));
    }

    /// <summary>
    /// Verifies that whitespace alone does not prevent adjacent tokens from being joined
    /// </summary>
    [TestMethod]
    public void ContainsUnjoinableTriviaReturnsFalseForWhitespace()
    {
        var triviaList = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "), SyntaxFactory.EndOfLine("\n"));

        Assert.IsFalse(SyntaxTriviaUtilities.ContainsUnjoinableTrivia(triviaList));
    }

    /// <summary>
    /// Verifies that a directive as the last non-whitespace, non-end-of-line trivia is detected
    /// </summary>
    [TestMethod]
    public void IsPrecededByDirectiveReturnsTrueForDirectiveAsLastContent()
    {
        var directive = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var trivia = new[] { SyntaxFactory.EndOfLine("\n"), directive, SyntaxFactory.Whitespace("    ") };

        Assert.IsTrue(SyntaxTriviaUtilities.IsPrecededByDirective(trivia));
    }

    /// <summary>
    /// Verifies that a comment as the last non-whitespace, non-end-of-line trivia is not treated as a directive
    /// </summary>
    [TestMethod]
    public void IsPrecededByDirectiveReturnsFalseForCommentAsLastContent()
    {
        var comment = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);
        var trivia = new[] { SyntaxFactory.EndOfLine("\n"), comment, SyntaxFactory.EndOfLine("\n"), SyntaxFactory.Whitespace("    ") };

        Assert.IsFalse(SyntaxTriviaUtilities.IsPrecededByDirective(trivia));
    }

    /// <summary>
    /// Verifies that an empty trivia sequence is not treated as directive-preceded
    /// </summary>
    [TestMethod]
    public void IsPrecededByDirectiveReturnsFalseForEmptySequence()
    {
        Assert.IsFalse(SyntaxTriviaUtilities.IsPrecededByDirective(Enumerable.Empty<SyntaxTrivia>()));
    }

    /// <summary>
    /// Verifies that a directive as the first non-whitespace, non-end-of-line trivia is detected
    /// </summary>
    [TestMethod]
    public void IsFollowedByDirectiveReturnsTrueForDirectiveAsFirstContent()
    {
        var directive = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var trivia = new[] { SyntaxFactory.Whitespace("    "), directive, SyntaxFactory.EndOfLine("\n") };

        Assert.IsTrue(SyntaxTriviaUtilities.IsFollowedByDirective(trivia));
    }

    /// <summary>
    /// Verifies that a comment as the first non-whitespace, non-end-of-line trivia is not treated as a directive
    /// </summary>
    [TestMethod]
    public void IsFollowedByDirectiveReturnsFalseForCommentAsFirstContent()
    {
        var comment = GetFirstTrivia("// note\nvar x = 1;\n", SyntaxKind.SingleLineCommentTrivia);
        var trivia = new[] { SyntaxFactory.EndOfLine("\n"), comment, SyntaxFactory.EndOfLine("\n") };

        Assert.IsFalse(SyntaxTriviaUtilities.IsFollowedByDirective(trivia));
    }

    /// <summary>
    /// Verifies that a conditional directive pair contained in the span is treated as balanced
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedConditionalDirectivesReturnsFalseForCompletePair()
    {
        const string source = "class C\n{\n#if DEBUG\n    void M()\n    {\n    }\n#endif\n}\n";

        var start = source.IndexOf("#if", StringComparison.Ordinal);
        var end = source.IndexOf("#endif", StringComparison.Ordinal) + "#endif".Length;

        Assert.IsFalse(ContainsUnbalancedConditionalDirectives(source, start, end));
    }

    /// <summary>
    /// Verifies that a span without any conditional directive is treated as balanced
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedConditionalDirectivesReturnsFalseWithoutConditionalDirectives()
    {
        const string source = "class C\n{\n    #region Methods\n\n    void M()\n    {\n    }\n\n    #endregion\n}\n";

        Assert.IsFalse(ContainsUnbalancedConditionalDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that an opening conditional directive without its closing partner is reported
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedConditionalDirectivesReturnsTrueForUnclosedIfDirective()
    {
        const string source = "class C\n{\n#if DEBUG\n    void M()\n    {\n    }\n#endif\n}\n";

        Assert.IsTrue(ContainsUnbalancedConditionalDirectives(source, source.IndexOf("#if", StringComparison.Ordinal), source.IndexOf("#endif", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a closing conditional directive without its opening partner is reported
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedConditionalDirectivesReturnsTrueForUnopenedEndIfDirective()
    {
        const string source = "class C\n{\n#if DEBUG\n    void M()\n    {\n    }\n#endif\n}\n";

        Assert.IsTrue(ContainsUnbalancedConditionalDirectives(source, source.IndexOf("void", StringComparison.Ordinal), source.Length));
    }

    /// <summary>
    /// Verifies that an else branch whose opening conditional directive lies outside the span is reported
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedConditionalDirectivesReturnsTrueForDetachedElseDirective()
    {
        const string source = "class C\n{\n#if DEBUG\n    void M()\n    {\n    }\n#else\n    void N()\n    {\n    }\n#endif\n}\n";

        Assert.IsTrue(ContainsUnbalancedConditionalDirectives(source, source.IndexOf("#else", StringComparison.Ordinal), source.Length));
    }

    /// <summary>
    /// Verifies that an unusable root refuses the move instead of reporting balanced directives
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedConditionalDirectivesReturnsTrueForMissingRoot()
    {
        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(null, TextSpan.FromBounds(0, 1)));
    }

    /// <summary>
    /// Verifies that a region directive pair contained in the span is treated as balanced
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedRegionDirectivesReturnsFalseForCompletePair()
    {
        const string source = "class C\n{\n#region Methods\n    void M()\n    {\n    }\n#endregion\n}\n";

        var start = source.IndexOf("#region", StringComparison.Ordinal);
        var end = source.IndexOf("#endregion", StringComparison.Ordinal) + "#endregion".Length;

        Assert.IsFalse(ContainsUnbalancedRegionDirectives(source, start, end));
    }

    /// <summary>
    /// Verifies that a span without any region directive is treated as balanced
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedRegionDirectivesReturnsFalseWithoutRegionDirectives()
    {
        const string source = "class C\n{\n#if DEBUG\n    void M()\n    {\n    }\n#endif\n}\n";

        Assert.IsFalse(ContainsUnbalancedRegionDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that an opening region directive without its closing partner is reported
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedRegionDirectivesReturnsTrueForUnclosedRegionDirective()
    {
        const string source = "class C\n{\n#region Methods\n    void M()\n    {\n    }\n#endregion\n}\n";

        Assert.IsTrue(ContainsUnbalancedRegionDirectives(source, source.IndexOf("#region", StringComparison.Ordinal), source.IndexOf("#endregion", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a closing region directive without its opening partner is reported
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedRegionDirectivesReturnsTrueForUnopenedEndRegionDirective()
    {
        const string source = "class C\n{\n#region Methods\n    void M()\n    {\n    }\n#endregion\n}\n";

        Assert.IsTrue(ContainsUnbalancedRegionDirectives(source, source.IndexOf("void", StringComparison.Ordinal), source.Length));
    }

    /// <summary>
    /// Verifies that a conditional branch directive detached from its opener does not trip the region predicate.
    /// Regions have no <c>#elif</c>/<c>#else</c> equivalent, so the detached-branch refusal is conditional-only
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedRegionDirectivesReturnsFalseForDetachedElseDirective()
    {
        const string source = "class C\n{\n#if DEBUG\n    void M()\n    {\n    }\n#else\n    void N()\n    {\n    }\n#endif\n}\n";

        Assert.IsFalse(ContainsUnbalancedRegionDirectives(source, source.IndexOf("#else", StringComparison.Ordinal), source.Length));
    }

    /// <summary>
    /// Verifies that an unusable root refuses the move instead of reporting balanced region directives
    /// </summary>
    [TestMethod]
    public void ContainsUnbalancedRegionDirectivesReturnsTrueForMissingRoot()
    {
        Assert.IsTrue(SyntaxTriviaUtilities.ContainsUnbalancedRegionDirectives(null, TextSpan.FromBounds(0, 1)));
    }

    /// <summary>
    /// Verifies that a conditional directive inside the span is reported
    /// </summary>
    [TestMethod]
    public void ContainsDirectivesReturnsTrueForConditionalDirective()
    {
        const string source = "class C\n{\n#if DEBUG\n    private int _x;\n#endif\n}\n";
        var endBeforeEndIf = source.IndexOf("#endif", StringComparison.Ordinal);

        Assert.IsTrue(ContainsDirectives(source, 0, endBeforeEndIf));
    }

    /// <summary>
    /// Verifies that a span holding only code and whitespace carries no directive
    /// </summary>
    [TestMethod]
    public void ContainsDirectivesReturnsFalseForDirectiveOutsideTheSpan()
    {
        const string source = "class C\n{\n#if DEBUG\n    private int _x;\n#endif\n}\n";
        var spanStart = source.IndexOf("private", StringComparison.Ordinal);

        Assert.IsFalse(ContainsDirectives(source, spanStart, spanStart + "private int _x;".Length));
    }

    /// <summary>
    /// Verifies that a pragma directive inside the span is reported
    /// </summary>
    [TestMethod]
    public void ContainsDirectivesReturnsTrueForPragmaDirectiveInSpan()
    {
        const string source = "class C\n{\n#pragma warning disable CS0169\n    private int _x;\n}\n";

        Assert.IsTrue(ContainsDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that a region directive inside the span is reported
    /// </summary>
    [TestMethod]
    public void ContainsDirectivesReturnsTrueForRegionDirective()
    {
        const string source = "class C\n{\n    #region Fields\n\n    private int _x;\n\n    #endregion\n}\n";
        var endBeforeEndRegion = source.IndexOf("#endregion", StringComparison.Ordinal);

        Assert.IsTrue(ContainsDirectives(source, 0, endBeforeEndRegion));
    }

    /// <summary>
    /// Verifies that a span carrying no directive at all is not reported
    /// </summary>
    [TestMethod]
    public void ContainsDirectivesReturnsFalseWithoutAnyDirective()
    {
        const string source = "class C\n{\n    private int _x;\n}\n";

        Assert.IsFalse(ContainsDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that an unusable root refuses the rewrite instead of reporting a directive-free span
    /// </summary>
    [TestMethod]
    public void ContainsDirectivesReturnsTrueForMissingRoot()
    {
        Assert.IsTrue(SyntaxTriviaUtilities.ContainsDirectives(null, TextSpan.FromBounds(0, 1)));
    }

    /// <summary>
    /// Verifies that a pragma warning directive is reported as position sensitive
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsTrueForPragmaWarningDirective()
    {
        const string source = "class C\n{\n#pragma warning disable CS0169\n    private int _x;\n}\n";

        Assert.IsTrue(ContainsPositionSensitiveDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that a nullable directive is reported as position sensitive
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsTrueForNullableDirective()
    {
        const string source = "class C\n{\n#nullable enable\n    private string _x;\n}\n";

        Assert.IsTrue(ContainsPositionSensitiveDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that a line directive is reported as position sensitive
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsTrueForLineDirective()
    {
        const string source = "class C\n{\n#line 200\n    private int _x;\n}\n";

        Assert.IsTrue(ContainsPositionSensitiveDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that region directives are not reported, since a region reorder moves them on purpose
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsFalseForRegionDirectives()
    {
        const string source = "class C\n{\n    #region Fields\n\n    private int _x;\n\n    #endregion\n}\n";

        Assert.IsFalse(ContainsPositionSensitiveDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that conditional directives are not reported, since their nesting is checked separately
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsFalseForConditionalDirectives()
    {
        const string source = "class C\n{\n#if DEBUG\n    private int _x;\n#else\n    private int _y;\n#endif\n}\n";

        Assert.IsFalse(ContainsPositionSensitiveDirectives(source, 0, source.Length));
    }

    /// <summary>
    /// Verifies that a directive outside the inspected span is not reported
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsFalseForDirectiveOutsideTheSpan()
    {
        const string source = "class C\n{\n#pragma warning disable CS0169\n    private int _x;\n}\n";

        Assert.IsFalse(ContainsPositionSensitiveDirectives(source, source.IndexOf("    private", StringComparison.Ordinal), source.Length));
    }

    /// <summary>
    /// Verifies that an unusable root refuses the move instead of reporting no position sensitive directives
    /// </summary>
    [TestMethod]
    public void ContainsPositionSensitiveDirectivesReturnsTrueForMissingRoot()
    {
        Assert.IsTrue(SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives(null, TextSpan.FromBounds(0, 1)));
    }

    /// <summary>
    /// Verifies that the insertion index stays at zero when no directive is present
    /// </summary>
    [TestMethod]
    public void FindIndexAfterLeadingDirectivesReturnsZeroWhenNoDirectivePresent()
    {
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "));

        Assert.AreEqual(0, SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(trivia));
    }

    /// <summary>
    /// Verifies that the insertion index lands immediately after a single leading directive
    /// </summary>
    [TestMethod]
    public void FindIndexAfterLeadingDirectivesReturnsIndexAfterSingleDirective()
    {
        var directive = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("    "), directive, SyntaxFactory.Whitespace("    "));

        Assert.AreEqual(2, SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(trivia));
    }

    /// <summary>
    /// Verifies that the insertion index lands after the last of multiple leading directives
    /// </summary>
    [TestMethod]
    public void FindIndexAfterLeadingDirectivesReturnsIndexAfterLastOfMultipleDirectives()
    {
        var ifDirective = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.IfDirectiveTrivia);
        var endIfDirective = GetFirstTrivia("#if true\nvar x = 1;\n#endif\n", SyntaxKind.EndIfDirectiveTrivia);
        var trivia = SyntaxFactory.TriviaList(ifDirective, endIfDirective, SyntaxFactory.Whitespace("    "));

        Assert.AreEqual(2, SyntaxTriviaUtilities.FindIndexAfterLeadingDirectives(trivia));
    }

    /// <summary>
    /// Verifies that a position inside a single-line comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForSingleLineComment()
    {
        const string source = "// note\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside a multi-line comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForMultiLineComment()
    {
        const string source = "/* first\r\n   note */\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside a single-line documentation comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForSingleLineDocumentationComment()
    {
        const string source = "/// note\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside a multi-line documentation comment is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForMultiLineDocumentationComment()
    {
        const string source = "/**\r\n * note\r\n */\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside preprocessor-disabled text is recognized as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsTrueForDisabledText()
    {
        const string source = "#if UNDEFINED\r\nvar note = 1;\r\n#endif\r\n";
        var root = GetRoot(source);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside an active preprocessor directive is not treated as non-formattable,
    /// since the formatter can rewrite that content
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsFalseForActiveDirective()
    {
        const string source = "#pragma warning disable CS0168\r\nvar x = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsFalse(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("warning", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that a position inside ordinary code is not treated as non-formattable
    /// </summary>
    [TestMethod]
    public void IsInsideCommentOrDisabledTextReturnsFalseForOrdinaryCode()
    {
        const string source = "var note = 1;\r\n";
        var root = GetRoot(source);

        Assert.IsFalse(SyntaxTriviaUtilities.IsInsideCommentOrDisabledText(root, source.IndexOf("note", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Verifies that an already normalized single-space gap keeps the original token
    /// </summary>
    [TestMethod]
    public void SetTrailingWhitespaceReturnsOriginalTokenForNormalizedSingleSpace()
    {
        var token = GetRoot("int x;").DescendantTokens().First();

        var result = SyntaxTriviaUtilities.SetTrailingWhitespace(token, 1);

        Assert.AreEqual(token, result);
    }

    /// <summary>
    /// Verifies that an already normalized comment-bearing gap keeps the original token
    /// </summary>
    [TestMethod]
    public void SetTrailingWhitespaceReturnsOriginalTokenForNormalizedCommentGap()
    {
        var token = GetRoot("int /* keep */ x;").DescendantTokens().First();

        var result = SyntaxTriviaUtilities.SetTrailingWhitespace(token, 1);

        Assert.AreEqual(token, result);
    }

    /// <summary>
    /// Verifies that a region directive inside a branch that was not taken is reported as inactive
    /// </summary>
    [TestMethod]
    public void IsInactiveDirectiveReturnsTrueForDirectiveInSkippedBranch()
    {
        var trivia = GetFirstTrivia("class C\n{\n#if false\n    #region Test\n    #endregion // Test\n#endif\n}", SyntaxKind.RegionDirectiveTrivia);

        Assert.IsTrue(SyntaxTriviaUtilities.IsInactiveDirective(trivia));
    }

    /// <summary>
    /// Verifies that a region directive outside any conditional branch is not reported as inactive
    /// </summary>
    [TestMethod]
    public void IsInactiveDirectiveReturnsFalseForUnconditionalDirective()
    {
        var trivia = GetFirstTrivia("class C\n{\n    #region Test\n    #endregion // Test\n}", SyntaxKind.RegionDirectiveTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsInactiveDirective(trivia));
    }

    /// <summary>
    /// Verifies that trivia without a directive structure is not reported as inactive
    /// </summary>
    [TestMethod]
    public void IsInactiveDirectiveReturnsFalseForComment()
    {
        var trivia = GetFirstTrivia("class C\n{\n    // note\n}", SyntaxKind.SingleLineCommentTrivia);

        Assert.IsFalse(SyntaxTriviaUtilities.IsInactiveDirective(trivia));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source and checks the requested span for unbalanced conditional directives
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="start">Start of the span to inspect</param>
    /// <param name="end">End of the span to inspect</param>
    /// <returns><see langword="true"/> if the span contains an unbalanced conditional directive</returns>
    private static bool ContainsUnbalancedConditionalDirectives(string source, int start, int end)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedConditionalDirectives(GetRoot(source), TextSpan.FromBounds(start, end));
    }

    /// <summary>
    /// Parses the source and checks the requested span for unbalanced region directives
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="start">Start of the span to inspect</param>
    /// <param name="end">End of the span to inspect</param>
    /// <returns><see langword="true"/> if the span contains an unbalanced region directive</returns>
    private static bool ContainsUnbalancedRegionDirectives(string source, int start, int end)
    {
        return SyntaxTriviaUtilities.ContainsUnbalancedRegionDirectives(GetRoot(source), TextSpan.FromBounds(start, end));
    }

    /// <summary>
    /// Parses the source and checks the requested span for position sensitive directives
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="start">Start of the span to inspect</param>
    /// <param name="end">End of the span to inspect</param>
    /// <returns><see langword="true"/> if the span contains a position sensitive directive</returns>
    private static bool ContainsPositionSensitiveDirectives(string source, int start, int end)
    {
        return SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives(GetRoot(source), TextSpan.FromBounds(start, end));
    }

    /// <summary>
    /// Parses the source and checks the requested span for directives
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="start">Span start</param>
    /// <param name="end">Span end</param>
    /// <returns><see langword="true"/> if the span contains a directive; otherwise, <see langword="false"/></returns>
    private static bool ContainsDirectives(string source, int start, int end)
    {
        return SyntaxTriviaUtilities.ContainsDirectives(GetRoot(source), TextSpan.FromBounds(start, end));
    }

    /// <summary>
    /// Parses the source and returns the first trivia of the requested kind
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="kind">The trivia kind to locate</param>
    /// <returns>The first matching trivia</returns>
    private static SyntaxTrivia GetFirstTrivia(string source, SyntaxKind kind)
    {
        var root = GetRoot(source);

        return root.DescendantTrivia(descendIntoTrivia: true).First(trivia => trivia.IsKind(kind));
    }

    /// <summary>
    /// Parses the source and returns its syntax root
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The parsed syntax root</returns>
    private static SyntaxNode GetRoot(string source)
    {
        return CSharpSyntaxTree.ParseText(source).GetRoot();
    }

    #endregion // Methods
}