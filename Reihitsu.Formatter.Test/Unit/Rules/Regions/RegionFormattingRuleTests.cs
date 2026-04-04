using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Regions;

namespace Reihitsu.Formatter.Test.Unit.Rules.Regions;

/// <summary>
/// Tests for <see cref="RegionFormattingRule"/>
/// </summary>
[TestClass]
public class RegionFormattingRuleTests
{
    #region Methods

    /// <summary>
    /// Verifies that a region name starting with a lowercase letter is capitalized.
    /// </summary>
    [TestMethod]
    public void LowercaseRegionNameCapitalizesFirstLetter()
    {
        // Arrange
        const string input = "class C\n{\n    #region methods\n\n    void M() { }\n\n    #endregion // methods\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.Contains("#region Methods", actual, "Region name should be capitalized.");
        Assert.Contains("#endregion // Methods", actual, "Endregion comment should match capitalized region name.");
        Assert.DoesNotContain("#region methods", actual, "Original lowercase region name should be replaced.");
    }

    /// <summary>
    /// Verifies that a region name already starting with an uppercase letter is not changed.
    /// </summary>
    [TestMethod]
    public void AlreadyUppercaseNoChange()
    {
        // Arrange
        const string input = "class C\n{\n    #region Methods\n\n    void M() { }\n\n    #endregion // Methods\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual, "Already-correct regions should not be modified.");
    }

    /// <summary>
    /// Verifies that a missing endregion comment is added to match the region name.
    /// </summary>
    [TestMethod]
    public void EndRegionCommentSynchronizedWithRegionName()
    {
        // Arrange
        const string input = "class C\n{\n    #region Methods\n\n    void M() { }\n\n    #endregion\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.Contains("#endregion // Methods", actual, "Endregion should have a comment matching the region name.");
    }

    /// <summary>
    /// Verifies that a correct endregion comment is not modified.
    /// </summary>
    [TestMethod]
    public void EndRegionCommentAlreadyCorrectNoChange()
    {
        // Arrange
        const string input = "class C\n{\n    #region Constructor\n\n    C() { }\n\n    #endregion // Constructor\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual, "Already-correct endregion comment should not be modified.");
    }

    /// <summary>
    /// Verifies that a mismatched endregion comment is corrected to match the region name.
    /// </summary>
    [TestMethod]
    public void MismatchedEndRegionCommentCorrected()
    {
        // Arrange
        const string input = "class C\n{\n    #region Methods\n\n    void M() { }\n\n    #endregion // Wrong\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.Contains("#endregion // Methods", actual, "Endregion comment should be corrected to match the region name.");
        Assert.DoesNotContain("#endregion // Wrong", actual, "Mismatched endregion comment should be replaced.");
    }

    /// <summary>
    /// Verifies that nested region pairs are both corrected.
    /// </summary>
    [TestMethod]
    public void NestedRegionsBothCorrected()
    {
        // Arrange
        const string input = "class C\n{\n    #region outer\n\n    #region inner\n\n    void M() { }\n\n    #endregion\n\n    #endregion\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.Contains("#region Outer", actual, "Outer region name should be capitalized.");
        Assert.Contains("#region Inner", actual, "Inner region name should be capitalized.");
        Assert.Contains("#endregion // Inner", actual, "Inner endregion should have matching comment.");
        Assert.Contains("#endregion // Outer", actual, "Outer endregion should have matching comment.");
    }

    /// <summary>
    /// Verifies that a region directive without a name is skipped and not modified.
    /// </summary>
    [TestMethod]
    public void RegionWithoutNameIsSkipped()
    {
        // Arrange
        const string input = "class C\n{\n    #region\n\n    void M() { }\n\n    #endregion\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual, "Region without a name should not be modified.");
    }

    /// <summary>
    /// Verifies that multiple region pairs in one file are all corrected.
    /// </summary>
    [TestMethod]
    public void MultipleRegionsAllCorrected()
    {
        // Arrange
        const string input = "class C\n{\n    #region fields\n\n    int _x;\n\n    #endregion\n\n    #region methods\n\n    void M() { }\n\n    #endregion\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.Contains("#region Fields", actual, "First region name should be capitalized.");
        Assert.Contains("#region Methods", actual, "Second region name should be capitalized.");
        Assert.Contains("#endregion // Fields", actual, "First endregion should have matching comment.");
        Assert.Contains("#endregion // Methods", actual, "Second endregion should have matching comment.");
    }

    /// <summary>
    /// Verifies that code without any region directives is not modified.
    /// </summary>
    [TestMethod]
    public void NoRegionsNoChanges()
    {
        // Arrange
        const string input = "class C\n{\n    void M() { }\n}\n";

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual, "Code without regions should not be modified.");
    }

    /// <summary>
    /// Verifies that the <see cref="RegionFormattingRule.Phase"/> property returns <see cref="FormattingPhase.RegionFormatting"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsRegionFormatting()
    {
        // Arrange
        var context = new FormattingContext("\n");
        var rule = new RegionFormattingRule(context, CancellationToken.None);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.RegionFormatting, phase);
    }

    /// <summary>
    /// Applies the <see cref="RegionFormattingRule"/> to the given input source code.
    /// </summary>
    /// <param name="input">The source code to format.</param>
    /// <returns>The formatted source code.</returns>
    private static string ApplyRule(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext("\n");
        var rule = new RegionFormattingRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Methods
}