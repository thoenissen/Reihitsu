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

    #endregion // Tests
}