using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxTokenPositionUtilities"/>
/// </summary>
[TestClass]
public class SyntaxTokenPositionUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that the very first token of a compilation unit starts a line even though it has no predecessor
    /// </summary>
    [TestMethod]
    public void IsFirstOnLineTreatsTokenWithoutPredecessorAsLineStart()
    {
        const string source = """
                              internal class C
                              {
                              }
                              """;

        var declaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration(source);

        Assert.IsTrue(SyntaxTokenPositionUtilities.IsFirstOnLine(declaration.Modifiers[0]));
    }

    /// <summary>
    /// Verifies that a token preceded on the same line by another token does not start a line
    /// </summary>
    [TestMethod]
    public void IsFirstOnLineRejectsTokenSharingALineWithItsPredecessor()
    {
        const string source = """
                              internal class C
                              {
                              }
                              """;

        var declaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration(source);

        Assert.IsFalse(SyntaxTokenPositionUtilities.IsFirstOnLine(declaration.Keyword));
        Assert.IsFalse(SyntaxTokenPositionUtilities.IsFirstOnLine(declaration.Identifier));
    }

    /// <summary>
    /// Verifies that the line break is found when it sits in the previous token's trailing trivia
    /// </summary>
    [TestMethod]
    public void IsFirstOnLineDetectsLineBreakInPreviousTokenTrailingTrivia()
    {
        const string source = """
                              internal class C
                              {
                              }
                              """;

        var declaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration(source);

        Assert.IsTrue(SyntaxTokenPositionUtilities.IsFirstOnLine(declaration.OpenBraceToken));
    }

    /// <summary>
    /// Verifies that a default token is not reported as starting a line. Callers walk tokens with
    /// <c>GetPreviousToken</c>, which yields a <see cref="SyntaxKind.None"/> token past the edge of the tree
    /// </summary>
    [TestMethod]
    public void IsFirstOnLineRejectsNoneToken()
    {
        Assert.IsFalse(SyntaxTokenPositionUtilities.IsFirstOnLine(default));
    }

    /// <summary>
    /// Verifies that the same token reports the same line and column under LF and CRLF. The helper reads trivia
    /// rather than raw text precisely so the separator cannot change the answer
    /// </summary>
    [TestMethod]
    public void GetLineAndGetColumnAreIndependentOfTheLineEnding()
    {
        const string source = "internal class C\n{\n    internal bool Value => true;\n}";

        var lineFeedProperty = CoreSyntaxTestHelper.GetSingleMember<PropertyDeclarationSyntax>(source);
        var carriageReturnProperty = CoreSyntaxTestHelper.GetSingleMember<PropertyDeclarationSyntax>(source.Replace("\n", "\r\n"));

        Assert.AreEqual(2, SyntaxTokenPositionUtilities.GetLine(lineFeedProperty.Modifiers[0]));
        Assert.AreEqual(4, SyntaxTokenPositionUtilities.GetColumn(lineFeedProperty.Modifiers[0]));

        Assert.AreEqual(SyntaxTokenPositionUtilities.GetLine(lineFeedProperty.Modifiers[0]),
                        SyntaxTokenPositionUtilities.GetLine(carriageReturnProperty.Modifiers[0]));
        Assert.AreEqual(SyntaxTokenPositionUtilities.GetColumn(lineFeedProperty.Modifiers[0]),
                        SyntaxTokenPositionUtilities.GetColumn(carriageReturnProperty.Modifiers[0]));

        Assert.IsTrue(SyntaxTokenPositionUtilities.IsFirstOnLine(carriageReturnProperty.Modifiers[0]));
    }

    #endregion // Tests
}