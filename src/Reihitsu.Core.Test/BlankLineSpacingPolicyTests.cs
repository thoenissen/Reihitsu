using System.Linq;

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

    #endregion // Tests

    #region Methods

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