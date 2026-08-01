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
    public void IsDirectSwitchSectionBreakReturnsTrueForDirectBreak()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: Consume(); break; } } void Consume() { } }";
        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.IsTrue(BlankLineSpacingPolicy.IsDirectSwitchSectionBreak(breakStatement));
    }

    /// <summary>
    /// Verifies a break statement owned by a block within a switch section is not exempt
    /// </summary>
    [TestMethod]
    public void IsDirectSwitchSectionBreakReturnsFalseForBlockOwnedBreak()
    {
        const string source = "class C { void M(int value) { switch (value) { case 1: { Consume(); break; } } } void Consume() { } }";
        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.IsFalse(BlankLineSpacingPolicy.IsDirectSwitchSectionBreak(breakStatement));
    }

    #endregion // Tests
}