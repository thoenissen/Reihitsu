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

    #endregion // Tests
}