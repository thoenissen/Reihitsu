using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="BlankLineSpacingPolicy"/>
/// </summary>
[TestClass]
public class BlankLineSpacingPolicyTests
{
    #region Tests

    /// <summary>
    /// Verifies a break statement owned directly by a switch section is exempt
    /// </summary>
    [TestMethod]
    public void IsTerminalDirectSwitchSectionBreakReturnsTrueForTerminalDirectBreak()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: Consume(); break; } } void Consume() { } }";
        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.IsTrue(BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(breakStatement));
    }

    /// <summary>
    /// Verifies a direct break with a following switch-section statement is not exempt
    /// </summary>
    [TestMethod]
    public void IsTerminalDirectSwitchSectionBreakReturnsFalseForNonTerminalDirectBreak()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: Consume(); break; Consume(); } } void Consume() { } }";
        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.IsFalse(BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(breakStatement));
    }

    /// <summary>
    /// Verifies a break statement owned by a block within a switch section is not exempt
    /// </summary>
    [TestMethod]
    public void IsTerminalDirectSwitchSectionBreakReturnsFalseForBlockOwnedBreak()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: { Consume(); break; } } } void Consume() { } }";
        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.IsFalse(BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(breakStatement));
    }

    /// <summary>
    /// Verifies a statement on the line directly below a closing brace requires a blank line
    /// </summary>
    [TestMethod]
    public void RequiresBlankLineAfterClosingBraceReturnsTrueForBraceDirectlyFollowedByStatement()
    {
        const string source = "class C\n{\n    void M()\n    {\n        if (true)\n        {\n        }\n        M();\n    }\n}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<MethodDeclarationSyntax>(source).Body.Statements;

        Assert.IsTrue(BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(statements[0], statements[1]));
    }

    /// <summary>
    /// Verifies an existing blank line after the closing brace satisfies the rule
    /// </summary>
    [TestMethod]
    public void RequiresBlankLineAfterClosingBraceReturnsFalseWhenBlankLineExists()
    {
        const string source = "class C\n{\n    void M()\n    {\n        if (true)\n        {\n        }\n\n        M();\n    }\n}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<MethodDeclarationSyntax>(source).Body.Statements;

        Assert.IsFalse(BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(statements[0], statements[1]));
    }

    /// <summary>
    /// Verifies a statement that does not end with a closing brace requires no blank line
    /// </summary>
    [TestMethod]
    public void RequiresBlankLineAfterClosingBraceReturnsFalseWhenStatementDoesNotEndWithBrace()
    {
        const string source = "class C\n{\n    void M()\n    {\n        M();\n        M();\n    }\n}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<MethodDeclarationSyntax>(source).Body.Statements;

        Assert.IsFalse(BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(statements[0], statements[1]));
    }

    /// <summary>
    /// Verifies a statement on the same line as the closing brace is left alone
    /// </summary>
    [TestMethod]
    public void RequiresBlankLineAfterClosingBraceReturnsFalseForSameLine()
    {
        const string source = "class C\n{\n    void M()\n    {\n        if (true) { } M();\n    }\n}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<MethodDeclarationSyntax>(source).Body.Statements;

        Assert.IsFalse(BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(statements[0], statements[1]));
    }

    /// <summary>
    /// Verifies a terminal break owned directly by a switch section is exempt
    /// </summary>
    [TestMethod]
    public void RequiresBlankLineAfterClosingBraceReturnsFalseForTerminalSwitchSectionBreak()
    {
        const string source = "class C\n{\n    void M(int value)\n    {\n        switch (value)\n        {\n            case 1:\n                if (true)\n                {\n                }\n                break;\n        }\n    }\n}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source).Statements;

        Assert.IsFalse(BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(statements[0], statements[1]));
    }

    /// <summary>
    /// Verifies a non-break statement after a closing brace in a switch section requires a blank line
    /// </summary>
    [TestMethod]
    public void RequiresBlankLineAfterClosingBraceReturnsTrueForNonBreakInSwitchSection()
    {
        const string source = "class C\n{\n    int M(int value)\n    {\n        switch (value)\n        {\n            case 1:\n                if (true)\n                {\n                }\n                return 1;\n        }\n\n        return 0;\n    }\n}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source).Statements;

        Assert.IsTrue(BlankLineSpacingPolicy.RequiresBlankLineAfterClosingBrace(statements[0], statements[1]));
    }

    /// <summary>
    /// Verifies a block that follows another statement of its block is a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsTrueForBlockAfterStatementInBlock()
    {
        const string source = "class C { void M() { A(); { Target(); } } void A() { } void Target() { } }";

        Assert.IsTrue(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies a block that follows a statement ending with a closing brace is a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsTrueForBlockAfterClosingBrace()
    {
        const string source = "class C { void M() { { A(); } { Target(); } } void A() { } void Target() { } }";

        Assert.IsTrue(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies a block that follows another statement of its switch section is a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsTrueForBlockAfterStatementInSwitchSection()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: A(); { Target(); } break; } } void A() { } void Target() { } }";

        Assert.IsTrue(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies a block that is the first statement of its block is not a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsFalseForFirstStatementOfBlock()
    {
        const string source = "class C { void M() { { Target(); } A(); } void A() { } void Target() { } }";

        Assert.IsFalse(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies a block that is the first statement of its switch section is not a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("RH5025-005")]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsFalseForFirstStatementOfSwitchSection()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: { Target(); } break; } } void Target() { } }";

        Assert.IsFalse(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies the body of an <c>if</c> statement that follows another statement is not a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsFalseForIfStatementBody()
    {
        const string source = "class C { void M(bool a) { A(); if (a) { Target(); } } void A() { } void Target() { } }";

        Assert.IsFalse(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies the body of a method is not a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsFalseForMethodBody()
    {
        const string source = "class C { void M() { Target(); } void Target() { } }";

        Assert.IsFalse(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies the block of a labeled statement that follows another statement is not a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsFalseForBlockOfLabeledStatement()
    {
        const string source = "class C { void M() { A(); L: { Target(); } } void A() { } void Target() { } }";

        Assert.IsFalse(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies a top-level block that follows a top-level statement is not a block statement after a statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-017")]
    public void IsBlockStatementAfterStatementReturnsFalseForTopLevelBlock()
    {
        const string source = "A(); { Target(); } static void A() { } static void Target() { }";

        Assert.IsFalse(BlankLineSpacingPolicy.IsBlockStatementAfterStatement(GetTargetBlock(source)));
    }

    /// <summary>
    /// Verifies a statement behind a statement that ends with a closing brace requires a blank line, and a statement
    /// behind any other last token does not
    /// </summary>
    /// <param name="previousLastTokenKind">The kind of the last token of the preceding statement</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("SHARED-BLANK-003")]
    [DataRow(SyntaxKind.CloseBraceToken, true)]
    [DataRow(SyntaxKind.SemicolonToken, false)]
    public void RequiresBlankLineBehindClosingBraceDependsOnTheLastTokenOfThePrecedingStatement(SyntaxKind previousLastTokenKind, bool expected)
    {
        Assert.AreEqual(expected, BlankLineSpacingPolicy.RequiresBlankLineBehindClosingBrace(previousLastTokenKind, false));
    }

    /// <summary>
    /// Verifies the terminal <c>break</c> owned directly by a switch section requires no blank line behind a closing brace
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-004")]
    public void RequiresBlankLineBehindClosingBraceReturnsFalseForTerminalSwitchSectionBreak()
    {
        Assert.IsFalse(BlankLineSpacingPolicy.RequiresBlankLineBehindClosingBrace(SyntaxKind.CloseBraceToken, true));
    }

    /// <summary>
    /// Verifies a jump or compound statement requires a blank line in front of it, whatever statement precedes it
    /// </summary>
    /// <param name="previous">The preceding statement</param>
    /// <param name="statement">The statement to inspect</param>
    [TestMethod]
    [Requirement("FMT-BLANK-001")]
    [DataRow("a++;", "return a;")]
    [DataRow("a++;", "if (a > 0) { }")]
    [DataRow("a++;", "throw null;")]
    [DataRow("a++;", "lock (this) { }")]
    [DataRow("a++;", "unchecked { a++; }")]
    [DataRow(";", "return a;")]
    [DataRow("return a;", "return a;")]
    public void RequiresBlankLineForStatementKindReturnsTrueForJumpAndCompoundStatements(string previous, string statement)
    {
        Assert.IsTrue(RequiresBlankLineForStatementKind(previous, statement, false));
    }

    /// <summary>
    /// Verifies a local declaration is separated from a preceding statement of another kind, and an expression statement
    /// that is not an assignment from a preceding local declaration
    /// </summary>
    /// <param name="previous">The preceding statement</param>
    /// <param name="statement">The statement to inspect</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("FMT-BLANK-002")]
    [DataRow("a++;", "var b = a;", true)]
    [DataRow(";", "var b = a;", true)]
    [DataRow("var c = a;", "var b = a;", false)]
    [DataRow("var b = a;", "Foo(b);", true)]
    [DataRow("var b = a;", "a = b;", false)]
    public void RequiresBlankLineForStatementKindSeparatesLocalDeclarations(string previous, string statement, bool expected)
    {
        Assert.AreEqual(expected, RequiresBlankLineForStatementKind(previous, statement, false));
    }

    /// <summary>
    /// Verifies a <c>yield</c> statement is separated from a preceding statement that is not a <c>yield</c> statement
    /// </summary>
    /// <param name="previous">The preceding statement</param>
    /// <param name="statement">The statement to inspect</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("FMT-BLANK-003")]
    [DataRow("a++;", "yield return a;", true)]
    [DataRow("a++;", "yield break;", true)]
    [DataRow("yield return a;", "yield return a;", false)]
    [DataRow("yield return a;", "yield break;", false)]
    [DataRow("yield break;", "yield return a;", false)]
    public void RequiresBlankLineForStatementKindSeparatesYieldStatements(string previous, string statement, bool expected)
    {
        Assert.AreEqual(expected, RequiresBlankLineForStatementKind(previous, statement, false));
    }

    /// <summary>
    /// Verifies a <c>break</c> statement requires a blank line in front of it unless it is the terminal <c>break</c> owned
    /// directly by a switch section
    /// </summary>
    /// <param name="isTerminalDirectSwitchSectionBreak">Whether the <c>break</c> is the terminal one of its switch section</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("FMT-BLANK-004")]
    [Requirement("SHARED-BLANK-004")]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void RequiresBlankLineForStatementKindExemptsOnlyTheTerminalSwitchSectionBreak(bool isTerminalDirectSwitchSectionBreak, bool expected)
    {
        Assert.AreEqual(expected, RequiresBlankLineForStatementKind("a++;", "break;", isTerminalDirectSwitchSectionBreak));
    }

    /// <summary>
    /// Verifies an expression statement and an empty statement require no blank line for their kind
    /// </summary>
    /// <param name="previous">The preceding statement</param>
    /// <param name="statement">The statement to inspect</param>
    [TestMethod]
    [DataRow("a++;", "a--;")]
    [DataRow("a++;", ";")]
    [DataRow(";", "a++;")]
    [DataRow(";", ";")]
    [DataRow("return a;", "a++;")]
    public void RequiresBlankLineForStatementKindReturnsFalseForStatementsNoRuleSeparates(string previous, string statement)
    {
        Assert.IsFalse(RequiresBlankLineForStatementKind(previous, statement, false));
    }

    /// <summary>
    /// Verifies which pairs of neighbouring statements a statement rule separates, read from the syntax alone: by the kind
    /// of the second statement, or by the closing brace that ends the first, and not by whether they share a line
    /// </summary>
    /// <param name="body">The method body, whose last two statements are inspected</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [DataRow("a++; return a;", true)]
    [DataRow("a++;\n\nreturn a;", true)]
    [DataRow("; return a;", true)]
    [DataRow("a++; ;", false)]
    [DataRow("; a++;", false)]
    [DataRow("a++; a--;", false)]
    [DataRow("if (a > 0) { } a++;", true)]
    [DataRow("if (a > 0) { } ;", true)]
    [DataRow("if (a > 0) { }\n\na++;", true)]
    [DataRow("int b = a; Foo(b);", true)]
    [DataRow("a++; /* x\n */ return a;", true)]
    [DataRow("a++; /* x\n */ a--;", false)]
    public void RequiresBlankLineBetweenStatementsReadsTheStatementRulesFromTheSyntax(string body, bool expected)
    {
        var statements = GetMethodStatements(body);

        Assert.AreEqual(expected, BlankLineSpacingPolicy.RequiresBlankLineBetweenStatements(statements[statements.Count - 2], statements[statements.Count - 1]));
    }

    /// <summary>
    /// Verifies the terminal <c>break</c> owned directly by a switch section is separated neither from a statement that
    /// ends with a closing brace nor from any other statement, and a <c>break</c> in a block of the section is
    /// </summary>
    /// <param name="section">The statements of the switch section, whose last two are inspected</param>
    /// <param name="expected">The expected result</param>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-BLANK-004")]
    [DataRow("if (a > 0) { } break;", false)]
    [DataRow("a++; break;", false)]
    [DataRow("; break;", false)]
    [DataRow("if (a > 0) { } return a;", true)]
    public void RequiresBlankLineBetweenStatementsExemptsTheTerminalSwitchSectionBreak(string section, bool expected)
    {
        var source = $"class C {{ int M(int a) {{ switch (a) {{ case 1: {section} }} return 0; }} }}";
        var statements = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source).Statements;

        Assert.AreEqual(expected, BlankLineSpacingPolicy.RequiresBlankLineBetweenStatements(statements[statements.Count - 2], statements[statements.Count - 1]));
    }

    /// <summary>
    /// Verifies a <c>break</c> in a block owned by a switch section is separated from the statement in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    [Requirement("SHARED-BLANK-004")]
    public void RequiresBlankLineBetweenStatementsSeparatesBreakInsideBlockOfSwitchSection()
    {
        const string source = "class C { void M(int a) { switch (a) { case 1: { a++; break; } } } }";
        var statements = CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                             .DescendantNodes()
                                             .OfType<BlockSyntax>()
                                             .Last()
                                             .Statements;

        Assert.IsTrue(BlankLineSpacingPolicy.RequiresBlankLineBetweenStatements(statements[0], statements[1]));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the statements of the body of a method that holds the given statements
    /// </summary>
    /// <param name="body">The statements of the method body</param>
    /// <returns>The statements of the method body</returns>
    private static SyntaxList<StatementSyntax> GetMethodStatements(string body)
    {
        return CoreSyntaxTestHelper.GetSingleNode<MethodDeclarationSyntax>($"class C {{ int M(int a) {{ {body} }} }}").Body.Statements;
    }

    /// <summary>
    /// Parses two neighbouring statements and determines whether the second one requires a blank line for its kind
    /// </summary>
    /// <param name="previous">The preceding statement</param>
    /// <param name="statement">The statement to inspect</param>
    /// <param name="isTerminalDirectSwitchSectionBreak">The value passed for the terminal-<c>break</c> exemption</param>
    /// <returns>The result of <see cref="BlankLineSpacingPolicy.RequiresBlankLineForStatementKind"/></returns>
    private static bool RequiresBlankLineForStatementKind(string previous, string statement, bool isTerminalDirectSwitchSectionBreak)
    {
        var statements = GetMethodStatements($"{previous} {statement}");

        Assert.HasCount(2, statements);

        return BlankLineSpacingPolicy.RequiresBlankLineForStatementKind(statements[1], statements[0].Kind(), isTerminalDirectSwitchSectionBreak);
    }

    /// <summary>
    /// Gets the block whose own statement list holds the call <c>Target();</c>
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The block that directly contains the call</returns>
    private static BlockSyntax GetTargetBlock(string source)
    {
        return CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                   .DescendantNodes()
                                   .OfType<BlockSyntax>()
                                   .Single(block => block.Statements.Any(statement => statement.ToString() == "Target();"));
    }

    #endregion // Methods
}