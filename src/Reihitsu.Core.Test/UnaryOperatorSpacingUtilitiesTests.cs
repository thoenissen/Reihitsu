using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="UnaryOperatorSpacingUtilities"/>
/// </summary>
[TestClass]
public class UnaryOperatorSpacingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a unary minus followed by a negated operand is detected as gluing
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsTrueForNestedUnaryMinus()
    {
        var node = GetOuterPrefixUnary("class C { void M(int x) { var y = - -x; } }");

        Assert.IsTrue(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that a unary minus followed by a pre-decrement operand is detected as gluing
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsTrueForUnaryMinusBeforePreDecrement()
    {
        var node = GetOuterPrefixUnary("class C { void M(int x) { var y = - --x; } }");

        Assert.IsTrue(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that a unary plus followed by a positively signed operand is detected as gluing
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsTrueForNestedUnaryPlus()
    {
        var node = GetOuterPrefixUnary("class C { void M(int x) { var y = + +x; } }");

        Assert.IsTrue(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that a unary plus followed by a pre-increment operand is detected as gluing
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsTrueForUnaryPlusBeforePreIncrement()
    {
        var node = GetOuterPrefixUnary("class C { void M(int x) { var y = + ++x; } }");

        Assert.IsTrue(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that a unary minus in front of a literal is not detected as gluing
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsFalseForUnaryMinusBeforeLiteral()
    {
        var node = GetOuterPrefixUnary("class C { void M() { var y = -1; } }");

        Assert.IsFalse(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that mixed signs are not detected as gluing because they do not re-lex into a single operator
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsFalseForMixedSigns()
    {
        var node = GetOuterPrefixUnary("class C { void M(int x) { var y = - +x; } }");

        Assert.IsFalse(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that an address-of operator followed by another address-of operator is detected as gluing, because removing the space would re-lex the two ampersands into the logical-and operator
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsTrueForNestedAddressOf()
    {
        var node = GetOuterPrefixUnary("class C { unsafe void M(int x) { int** pp = & &x; } }");

        Assert.IsTrue(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that an address-of operator in front of an identifier operand is not detected as gluing
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsFalseForAddressOfBeforeIdentifier()
    {
        var node = GetOuterPrefixUnary("class C { unsafe void M(int x) { int* p = &x; } }");

        Assert.IsFalse(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    /// <summary>
    /// Verifies that a pointer-indirection operator followed by another pointer-indirection operator is not detected as gluing, because C# has no merged token for two adjacent asterisks
    /// </summary>
    [TestMethod]
    public void WouldGlueReturnsFalseForNestedPointerIndirection()
    {
        var node = GetOuterPrefixUnary("class C { unsafe void M(int** pp) { int y = * *pp; } }");

        Assert.IsFalse(UnaryOperatorSpacingUtilities.WouldGlueIntoDifferentOperator(node));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the outermost prefix unary expression from the source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The outermost prefix unary expression</returns>
    private static PrefixUnaryExpressionSyntax GetOuterPrefixUnary(string source)
    {
        return CoreSyntaxTestHelper.ParseCompilationUnit(source).DescendantNodes().OfType<PrefixUnaryExpressionSyntax>().First();
    }

    #endregion // Methods
}