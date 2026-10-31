using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that the node-level entry points format a root without a parent, that what the phases know of the document around
/// that root comes from plain-value facts, and that <see cref="ReihitsuFormatter.FormatNode"/> lays out its node at the depth the
/// indentation phase gives it. Every input marks with <c>⇥</c> one place inside the target where a single space stands; each
/// fixture runs once with that space and once with a tab there, which the preparation phase replaces
/// </summary>
[TestClass]
public class ReihitsuFormatterDetachedRootTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Marks the place inside the target that holds either a space or a tab
    /// </summary>
    private const string WhitespaceMarker = "⇥";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The two spellings of the marked whitespace: a space, and a tab that the preparation phase replaces
    /// </summary>
    private static readonly string[] _markedWhitespace = [" ", "\t"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNode"/> on a statement behind a blank line returns the statement
    /// without a line break in front of it, with and without a tab in the statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    public void FormatNodeOnStatementBehindBlankLineReturnsItWithoutLeadingLineBreak()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var a = 1;\n\n        Call(1,⇥2);\n    }\n}";

        AssertFormatNode(input, "        Call(1, 2);", SelectCall, 2);
    }

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> with a statement behind a blank line
    /// keeps the blank line and returns the document as it is, the tab replaced by one space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    public async Task FormatNodeInDocumentOnStatementBehindBlankLineKeepsTheBlankLine()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var a = 1;\n\n        Call(1,⇥2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var a = 1;\n\n        Call(1, 2);\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectCall);
    }

    /// <summary>
    /// Verifies the spec example: the <c>if</c> of an <c>else if</c> as target keeps the line comment behind its condition, with
    /// and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task ElseIfTargetKeepsTheLineCommentBehindItsCondition()
    {
        const string input = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if⇥(y) // note\n        {\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if (y) // note\n             {\n             }\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectElseIf);
    }

    /// <summary>
    /// Verifies that the <c>if</c> of an <c>else if</c> as target keeps the block comment behind its condition, with and without a
    /// tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task ElseIfTargetKeepsTheBlockCommentBehindItsCondition()
    {
        const string input = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if⇥(y) /* explanation */\n        {\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if (y) /* explanation */\n             {\n             }\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectElseIf);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> with the <c>if</c> of an <c>else if</c> as
    /// target and its <c>else</c> clause as context keeps the line comment behind the condition, with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    public async Task ElseIfTargetWithContextKeepsTheLineCommentBehindItsCondition()
    {
        const string input = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if⇥(y) // note\n        {\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if (y) // note\n        {\n        }\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectElseIf, root => SelectElseIf(root).Parent);
    }

    /// <summary>
    /// Verifies that an inner part of a longer member-access chain as target keeps the chain's layout, with and without a tab in
    /// the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task ChainPrefixTargetKeepsTheLayoutOfTheLongerChain()
    {
        const string input = "using System.Linq;\n\nclass C\n{\n    void M(int[] source)\n    {\n        var x = source\n                .Where(i => i >⇥0)\n                .Select(i => i * 2)\n            .ToList();\n    }\n}";
        const string expected = "using System.Linq;\n\nclass C\n{\n    void M(int[] source)\n    {\n        var x = source\n                .Where(i => i > 0)\n                .Select(i => i * 2)\n            .ToList();\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectSelectInvocation);
    }

    /// <summary>
    /// Verifies that an assignment inside a conditional access as target keeps the link of its assigned member on its own line,
    /// with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task ConditionalAccessAssignmentTargetKeepsTheWrappedLinkOfItsAssignedMember()
    {
        const string input = "class C\n{\n    void M(A a)\n    {\n        a?.B\n          .C =⇥5;\n    }\n}";
        const string expected = "class C\n{\n    void M(A a)\n    {\n        a?.B\n          .C = 5;\n    }\n}";

        await AssertFormatsTarget(input, expected, Select<AssignmentExpressionSyntax>());
    }

    /// <summary>
    /// Verifies that the accessor list of a one-line property as target keeps the accessor's attribute list on the property's line,
    /// with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task AccessorListTargetOfOneLinePropertyKeepsTheAccessorAttributeOnThePropertyLine()
    {
        const string input = "class C\n{\n    public int Value { [field: System.Obsolete] get;⇥set; }\n}";
        const string expected = "class C\n{\n    public int Value { [field: System.Obsolete] get; set; }\n}";

        await AssertFormatsTarget(input, expected, Select<AccessorListSyntax>());
    }

    /// <summary>
    /// Verifies that an accessor of a one-line property as target keeps its attribute list on the property's line, with and without
    /// a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task AccessorTargetOfOneLinePropertyKeepsItsAttributeOnThePropertyLine()
    {
        const string input = "class C\n{\n    public int Value { [field: System.Obsolete]⇥get; set; }\n}";
        const string expected = "class C\n{\n    public int Value { [field: System.Obsolete] get; set; }\n}";

        await AssertFormatsTarget(input, expected, Select<AccessorDeclarationSyntax>(accessor => accessor.AttributeLists.Count > 0));
    }

    /// <summary>
    /// Verifies that an object creation inside an argument as target gets the wrap inside it joined, as the list-element join joins
    /// it in a document, with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    [Requirement("FMT-PIPE-024")]
    public async Task ObjectCreationTargetInsideArgumentGetsItsInteriorWrapJoined()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Use(new\n            Example(⇥));\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        Use(new Example());\n    }\n}";

        await AssertFormatsTarget(input, expected, Select<ObjectCreationExpressionSyntax>());
    }

    /// <summary>
    /// Verifies that a parameter as target, which is itself the element of its parameter list, gets the wrap inside it joined, with
    /// and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task ParameterTargetGetsItsInteriorWrapJoined()
    {
        const string input = "class C\n{\n    void M(int\n               x =⇥1, int y)\n    {\n    }\n}";
        const string expected = "class C\n{\n    void M(int x = 1, int y)\n    {\n    }\n}";

        await AssertFormatsTarget(input, expected, Select<ParameterSyntax>(parameter => parameter.Identifier.Text == "x"));
    }

    /// <summary>
    /// Verifies that the bracketed argument list of an element access as target gets its arguments collapsed onto one line, with and
    /// without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task ElementAccessArgumentListTargetGetsItsArgumentsCollapsed()
    {
        const string input = "class C\n{\n    void M(int[,] a)\n    {\n        var v = a[⇥1,\n                  2];\n    }\n}";
        const string expected = "class C\n{\n    void M(int[,] a)\n    {\n        var v = a[1, 2];\n    }\n}";

        await AssertFormatsTarget(input, expected, Select<BracketedArgumentListSyntax>());
    }

    /// <summary>
    /// Verifies that a recursive pattern a <c>case</c> label introduces as target keeps its opening brace on the label's line, with
    /// and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task CaseLabelPatternTargetKeepsItsOpeningBraceOnTheLabelLine()
    {
        const string input = "class C\n{\n    void M(object o)\n    {\n        switch (o)\n        {\n            case Point {\n                     X:⇥1,\n                     Y: 2\n                 }:\n                break;\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M(object o)\n    {\n        switch (o)\n        {\n            case Point {\n                     X: 1,\n                     Y: 2\n                 }:\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, expected, Select<RecursivePatternSyntax>());
    }

    /// <summary>
    /// Verifies that a recursive pattern a <c>case</c> label introduces through a <c>not</c> pattern as target keeps its opening
    /// brace on the label's line, with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task CaseLabelPatternTargetBehindNotKeepsItsOpeningBraceOnTheLabelLine()
    {
        const string input = "class C\n{\n    void M(object o)\n    {\n        switch (o)\n        {\n            case not Point {\n                         X:⇥1,\n                         Y: 2\n                     }:\n                break;\n        }\n    }\n}";
        const string expected = "class C\n{\n    void M(object o)\n    {\n        switch (o)\n        {\n            case not Point {\n                         X: 1,\n                         Y: 2\n                     }:\n                break;\n        }\n    }\n}";

        await AssertFormatsTarget(input, expected, Select<RecursivePatternSyntax>());
    }

    /// <summary>
    /// Verifies that a collection initializer on the right of an assignment as target keeps its opening brace on the assignment's
    /// line and gets the same output with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task AssignedCollectionInitializerTargetKeepsItsOpeningBraceOnTheAssignmentLine()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var d = new D\n        {\n            L = {\n                    1,⇥2\n                }\n        };\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            var withSpace = await Format(NormalizeLineEndings(input, endOfLine).Replace(WhitespaceMarker, " "), SelectAssignedInitializer, null);
            var withTab = await Format(NormalizeLineEndings(input, endOfLine).Replace(WhitespaceMarker, "\t"), SelectAssignedInitializer, null);

            Assert.AreEqual(withSpace, withTab, $"A tab in the target must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.Contains(NormalizeLineEndings("        {\n            L = {", endOfLine), withSpace, $"The opening brace must stay on the assignment's line under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual(withSpace, await Format(withSpace, SelectAssignedInitializer, null), $"The second pass must not change the output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies the spec example: a statement target behind a local declaration gets the blank line its list position calls for,
    /// with and without a tab in the target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task StatementTargetBehindLocalDeclarationGetsTheBlankLineOfItsListPosition()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var a = 1;\n        Call(1,⇥2);\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        var a = 1;\n\n        Call(1, 2);\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectCall);
    }

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNode"/> on a statement behind a local declaration supplies no
    /// list position and returns the statement without a line break in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public void FormatNodeOnStatementBehindLocalDeclarationAddsNoBlankLine()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var a = 1;\n        Call(1,⇥2);\n    }\n}";

        AssertFormatNode(input, "        Call(1, 2);", SelectCall, 2);
    }

    /// <summary>
    /// Verifies that an <c>if</c> target that is the unbraced body of another <c>if</c> and starts behind the last line of a
    /// multi-line comment places its braces and body at the indentation phase's depth, one level deeper than its owner
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task EmbeddedIfTargetBehindMultiLineCommentTakesTheIndentationPhaseDepth()
    {
        const string input = "class C\n{\n    void M(bool a, bool b)\n    {\n        if (a)\n            /* note\n            */ if (b)\n        {\n        X(⇥);\n        }\n    }\n}";
        const string expectedEnd = "            */ if (b)\n            {\n                X();\n            }\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            foreach (var whitespace in _markedWhitespace)
            {
                var description = Describe(endOfLine, whitespace);
                var firstPass = await Format(NormalizeLineEndings(input, endOfLine).Replace(WhitespaceMarker, whitespace), SelectInnerIf, null);

                Assert.EndsWith(NormalizeLineEndings(expectedEnd, endOfLine), firstPass, $"Unexpected braces and body under {description}.");
                Assert.AreEqual(firstPass, await Format(firstPass, SelectInnerIf, null), $"The second pass must not change the output under {description}.");
            }
        }
    }

    /// <summary>
    /// Verifies that an initializer target inside a collection initializer that starts behind the last line of a multi-line comment
    /// places the comment's first line at the indentation phase's depth, which adds no level for the initializer around it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public async Task InitializerTargetBehindMultiLineCommentTakesTheIndentationPhaseDepth()
    {
        const string input = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d = new Dictionary<int, int>\n        {\n            /* a\n             b */ { 1,⇥2 }\n        };\n    }\n}";
        const string expected = "using System.Collections.Generic;\n\nclass C\n{\n    void M()\n    {\n        var d = new Dictionary<int, int>\n        {\n        /* a\n             b */ { 1, 2 }\n        };\n    }\n}";

        await AssertFormatsTarget(input, expected, SelectInnerInitializer);
    }

    /// <summary>
    /// Verifies that no field of <see cref="FormattingContext"/>, or of a type its fields hold, holds a node, a token, or a trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-024")]
    public void FormattingContextFactsHoldOnlyPlainValues()
    {
        var violations = new List<string>();
        var visited = new HashSet<Type>();

        CollectSyntaxHoldingFields(typeof(FormattingContext), visited, violations);

        Assert.IsEmpty(violations, "Fields that hold syntax of the document: " + string.Join(", ", violations));
    }

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNode"/> places the unbraced embedded statement of an <c>if</c>
    /// one level deeper than the <c>if</c>, with the wrapped argument aligned under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnEmbeddedStatementOfIfPlacesItOneLevelDeeper()
    {
        const string input = "class C\n{\n    void M(bool a)\n    {\n        if (a)\n            Call(1,\n            2);\n    }\n}";

        AssertFormatNode(input, "            Call(1,\n                 2);", SelectCall, 3);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> places the unbraced embedded statement of an <c>else</c> one level
    /// deeper than the <c>else</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnEmbeddedStatementOfElsePlacesItOneLevelDeeper()
    {
        const string input = "class C\n{\n    void M(bool a)\n    {\n        if (a)\n            Other();\n        else\n            Call(1,\n            2);\n    }\n}";

        AssertFormatNode(input, "            Call(1,\n                 2);", SelectCall, 3);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> places the unbraced embedded statement of a loop one level deeper
    /// than the loop
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnEmbeddedStatementOfLoopPlacesItOneLevelDeeper()
    {
        const string input = "class C\n{\n    void M(bool a)\n    {\n        while (a)\n            Call(1,\n            2);\n    }\n}";

        AssertFormatNode(input, "            Call(1,\n                 2);", SelectCall, 3);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> adds no level for an <c>else if</c>: the <c>if</c> of the
    /// <c>else</c> clause stands at the depth of the <c>if</c> that owns the <c>else</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnIfOfElseClauseAddsNoLevel()
    {
        const string input = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if (y)\n        {\n            Call(1,⇥2);\n        }\n    }\n}";

        AssertFormatNode(input, "        if (y)\n        {\n            Call(1, 2);\n        }", SelectElseIf, 2);
    }

    /// <summary>
    /// Verifies the spec example: an <c>indentLevel</c> of zero or greater overrides the depth of the embedded statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnEmbeddedStatementTakesTheExplicitIndentLevel()
    {
        const string input = "class C\n{\n    void M(bool a)\n    {\n        if (a)\n            Call(1,\n            2);\n    }\n}";

        AssertFormatNode(input, "Call(1,\n     2);", SelectCall, 0, indentLevel: 0);
        AssertFormatNode(input, "            Call(1,\n                 2);", SelectCall, 3, indentLevel: 3);
    }

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNode"/> adds no level for the object initializer a node lies
    /// in, so the assignment starts at the depth of the statement that holds the initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnObjectInitializerMemberAddsNoLevelForTheInitializer()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var x = new D\n        {\n            P = Call(1,\n            2)\n        };\n    }\n}";

        AssertFormatNode(input, "        P = Call(1,\n                 2)", Select<AssignmentExpressionSyntax>(), 2);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> adds no level for the anonymous object a node lies in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnAnonymousObjectMemberAddsNoLevelForTheAnonymousObject()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var x = new\n        {\n            P = Call(1,\n            2)\n        };\n    }\n}";

        AssertFormatNode(input, "        P = Call(1,\n                 2)", Select<AnonymousObjectMemberDeclaratorSyntax>(), 2);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> on the <c>if</c> of <c>else if (y) // note</c> gets the result of
    /// the same node without a parent: the comment moves onto its own line in front of the <c>if</c> and is not lost
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnElseIfWithTrailingCommentGetsTheResultOfTheNodeWithoutParent()
    {
        const string input = "class C\n{\n    void M(bool x, bool y)\n    {\n        if (x)\n        {\n        }\n        else if⇥(y) // note\n        {\n        }\n    }\n}";

        AssertFormatNode(input, "        // note\n        if (y)\n        {\n        }", SelectElseIf, 2);
        AssertFormatNodeMatchesNodeWithoutParent(input, SelectElseIf, 2);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> on an inner part of a longer member-access chain gets the result of
    /// the same node without a parent, which lays it out as a chain of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnChainPrefixGetsTheResultOfTheNodeWithoutParent()
    {
        const string input = "using System.Linq;\n\nclass C\n{\n    void M(int[] source)\n    {\n        var x = source\n                .Where(i => i >⇥0)\n                .Select(i => i * 2)\n            .ToList();\n    }\n}";

        AssertFormatNode(input, "        source.Where(i => i > 0)\n              .Select(i => i * 2)", SelectSelectInvocation, 2);
        AssertFormatNodeMatchesNodeWithoutParent(input, SelectSelectInvocation, 2);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> on the accessor list of a one-line property gets the result of the
    /// same node without a parent
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnAccessorListOfOneLinePropertyGetsTheResultOfTheNodeWithoutParent()
    {
        const string input = "class C\n{\n    public int Value { [field: System.Obsolete] get;⇥set; }\n}";

        AssertFormatNodeMatchesNodeWithoutParent(input, Select<AccessorListSyntax>(), 1);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> on an object creation inside an argument gets the result of the same
    /// node without a parent
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-012")]
    public void FormatNodeOnObjectCreationInsideArgumentGetsTheResultOfTheNodeWithoutParent()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Use(new\n            Example(⇥));\n    }\n}";

        AssertFormatNodeMatchesNodeWithoutParent(input, Select<ObjectCreationExpressionSyntax>(), 2);
    }

    /// <summary>
    /// Verifies the spec example: <see cref="ReihitsuFormatter.FormatNode"/> on a statement that is part of a tree returns it without
    /// the line break that ends the statement's line in the tree
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-008")]
    public void FormatNodeOnStatementPartOfTreeReturnsItWithoutTrailingLineBreak()
    {
        const string input = "class C\n{\n    void M()\n    {\n        Call(1,⇥2);\n    }\n}";

        AssertFormatNode(input, "        Call(1, 2);", SelectCall, 2);
    }

    /// <summary>
    /// Returns a selector of the first node of the given type that matches the predicate
    /// </summary>
    /// <typeparam name="TNode">Type of the node</typeparam>
    /// <param name="predicate">The predicate, or <see langword="null"/> for the first node of the type</param>
    /// <returns>The selector</returns>
    private static Func<SyntaxNode, SyntaxNode> Select<TNode>(Func<TNode, bool> predicate = null)
        where TNode : SyntaxNode
    {
        return root => root.DescendantNodes().OfType<TNode>().First(node => predicate == null || predicate(node));
    }

    /// <summary>
    /// Selects the statement that calls <c>Call</c>
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The statement</returns>
    private static SyntaxNode SelectCall(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<ExpressionStatementSyntax>().Single(statement => statement.Expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Call" } });
    }

    /// <summary>
    /// Selects the <c>if</c> statement of the <c>else</c> clause
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The <c>if</c> statement</returns>
    private static SyntaxNode SelectElseIf(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<IfStatementSyntax>().Single(statement => statement.Parent is ElseClauseSyntax);
    }

    /// <summary>
    /// Selects the <c>if</c> statement that is the body of another <c>if</c> statement
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The <c>if</c> statement</returns>
    private static SyntaxNode SelectInnerIf(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<IfStatementSyntax>().Single(statement => statement.Parent is IfStatementSyntax);
    }

    /// <summary>
    /// Selects the invocation of <c>Select</c> inside the longer chain that ends in <c>ToList</c>
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The invocation</returns>
    private static SyntaxNode SelectSelectInvocation(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<InvocationExpressionSyntax>().Single(invocation => invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Select" });
    }

    /// <summary>
    /// Selects the collection initializer on the right of an assignment
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The initializer</returns>
    private static SyntaxNode SelectAssignedInitializer(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<InitializerExpressionSyntax>().Single(initializer => initializer.Parent is AssignmentExpressionSyntax);
    }

    /// <summary>
    /// Selects the initializer that is an element of another initializer
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The initializer</returns>
    private static SyntaxNode SelectInnerInitializer(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<InitializerExpressionSyntax>().Single(initializer => initializer.Parent is InitializerExpressionSyntax);
    }

    /// <summary>
    /// Collects every field of the given type, and of the types its fields hold, whose type holds a node, a token, or a trivia
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <param name="visited">The types already inspected</param>
    /// <param name="violations">Receives the violating fields as <c>Type.Field</c></param>
    private static void CollectSyntaxHoldingFields(Type type, HashSet<Type> visited, List<string> violations)
    {
        if (visited.Add(type) == false)
        {
            return;
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (var heldType in ExpandHeldTypes(field.FieldType))
            {
                if (IsSyntaxType(heldType))
                {
                    violations.Add($"{type.Name}.{field.Name}");
                }
                else if (heldType.Assembly == typeof(FormattingContext).Assembly)
                {
                    CollectSyntaxHoldingFields(heldType, visited, violations);
                }
            }
        }
    }

    /// <summary>
    /// Returns the given type together with its element type and its generic type arguments, recursively
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The types the type holds</returns>
    private static IEnumerable<Type> ExpandHeldTypes(Type type)
    {
        yield return type;

        var nested = Array.Empty<Type>();

        if (type.HasElementType)
        {
            nested = [type.GetElementType()];
        }
        else if (type.IsGenericType)
        {
            nested = type.GetGenericArguments();
        }

        foreach (var nestedType in nested)
        {
            foreach (var heldType in ExpandHeldTypes(nestedType))
            {
                yield return heldType;
            }
        }
    }

    /// <summary>
    /// Determines whether the given type is a node, a token, a trivia, or a list of them
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns><see langword="true"/> when the type holds syntax of a tree</returns>
    private static bool IsSyntaxType(Type type)
    {
        if (typeof(SyntaxNode).IsAssignableFrom(type))
        {
            return true;
        }

        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        return definition == typeof(SyntaxToken)
               || definition == typeof(SyntaxTrivia)
               || definition == typeof(SyntaxTriviaList)
               || definition == typeof(SyntaxTokenList)
               || definition == typeof(SyntaxNodeOrToken)
               || definition == typeof(SyntaxNodeOrTokenList)
               || definition == typeof(SyntaxList<>)
               || definition == typeof(SeparatedSyntaxList<>)
               || definition == typeof(SyntaxReference)
               || definition == typeof(SyntaxTree)
               || typeof(SyntaxTree).IsAssignableFrom(type);
    }

    /// <summary>
    /// Describes a line ending and the spelling of the marked whitespace for an assertion message
    /// </summary>
    /// <param name="endOfLine">The line ending</param>
    /// <param name="whitespace">The marked whitespace</param>
    /// <returns>The description</returns>
    private static string Describe(string endOfLine, string whitespace)
    {
        return $"{DescribeLineEnding(endOfLine)} line endings with {(whitespace == "\t" ? "a tab" : "a space")} in the target";
    }

    /// <summary>
    /// Formats the selected node through <see cref="ReihitsuFormatter.FormatNode"/> under LF and CRLF, once with a space and once
    /// with a tab at the marker, and asserts the result, and that formatting the result again at the given depth leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings and the whitespace marker</param>
    /// <param name="expectedWithLf">Expected result with LF line endings</param>
    /// <param name="selectNode">Selects the node in the tree's root</param>
    /// <param name="depth">The depth the result stands at, used for the second pass</param>
    /// <param name="indentLevel">The <c>indentLevel</c> argument of the first pass</param>
    private void AssertFormatNode(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectNode, int depth, int indentLevel = -1)
    {
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var whitespace in _markedWhitespace)
            {
                var description = Describe(endOfLine, whitespace);
                var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(inputWithLf, endOfLine).Replace(WhitespaceMarker, whitespace), cancellationToken: TestContext.CancellationToken);
                var node = selectNode(tree.GetRoot(TestContext.CancellationToken));

                var firstPass = ReihitsuFormatter.FormatNode(node, indentLevel, TestContext.CancellationToken);

                Assert.AreEqual(NormalizeLineEndings(expectedWithLf, endOfLine), firstPass.ToFullString(), $"Unexpected first pass under {description}.");

                var secondPass = ReihitsuFormatter.FormatNode(firstPass, depth, TestContext.CancellationToken);

                Assert.AreEqual(firstPass.ToFullString(), secondPass.ToFullString(), $"The second pass must not change the result under {description}.");
            }
        }
    }

    /// <summary>
    /// Asserts, under LF and CRLF and with a space and a tab at the marker, that <see cref="ReihitsuFormatter.FormatNode"/> gives the
    /// selected node, which is part of a tree, the result that a copy of the node without a parent gets at the given depth
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings and the whitespace marker</param>
    /// <param name="selectNode">Selects the node in the tree's root</param>
    /// <param name="depth">The depth the indentation phase gives the node at its position</param>
    private void AssertFormatNodeMatchesNodeWithoutParent(string inputWithLf, Func<SyntaxNode, SyntaxNode> selectNode, int depth)
    {
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var whitespace in _markedWhitespace)
            {
                var description = Describe(endOfLine, whitespace);
                var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(inputWithLf, endOfLine).Replace(WhitespaceMarker, whitespace), cancellationToken: TestContext.CancellationToken);
                var node = selectNode(tree.GetRoot(TestContext.CancellationToken));
                var nodeWithoutParent = CSharpSyntaxTree.Create((CSharpSyntaxNode)node).GetRoot(TestContext.CancellationToken);

                Assert.IsNull(nodeWithoutParent.Parent, "The copy must have no parent.");

                var attached = ReihitsuFormatter.FormatNode(node, cancellationToken: TestContext.CancellationToken).ToFullString();
                var detached = ReihitsuFormatter.FormatNode(nodeWithoutParent, depth, TestContext.CancellationToken).ToFullString();

                Assert.AreEqual(detached, attached, $"The node that is part of a tree must get the result of the node without a parent under {description}.");
            }
        }
    }

    /// <summary>
    /// Formats the selected target through a node-level entry point under LF and CRLF, once with a space and once with a tab at the
    /// marker, and asserts the document's text after one pass, and that a second pass leaves it unchanged
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings and the whitespace marker</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node, or <see langword="null"/> to format without a context</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsTarget(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            foreach (var whitespace in _markedWhitespace)
            {
                var description = Describe(endOfLine, whitespace);
                var expected = NormalizeLineEndings(expectedWithLf, endOfLine);
                var firstPass = await Format(NormalizeLineEndings(inputWithLf, endOfLine).Replace(WhitespaceMarker, whitespace), selectTarget, selectContext);

                Assert.AreEqual(expected, firstPass, $"Unexpected first pass under {description}.");

                var secondPass = await Format(firstPass, selectTarget, selectContext);

                Assert.AreEqual(expected, secondPass, $"The second pass must not change the output under {description}.");
            }
        }
    }

    /// <summary>
    /// Formats the selected target of the given text through a node-level entry point
    /// </summary>
    /// <param name="text">Document text</param>
    /// <param name="selectTarget">Selects the target in the document's root</param>
    /// <param name="selectContext">Selects the context node, or <see langword="null"/> to format without a context</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> Format(string text, Func<SyntaxNode, SyntaxNode> selectTarget, Func<SyntaxNode, SyntaxNode> selectContext)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            if (root == null)
            {
                Assert.Fail("Expected a syntax root.");
            }

            var target = selectTarget(root);
            var result = selectContext == null
                             ? await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)
                             : await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, selectContext(root), TestContext.CancellationToken);

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}