using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="StringInterpolationUtilities"/>
/// </summary>
[TestClass]
public class StringInterpolationUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that interpolation holes are detected correctly
    /// </summary>
    [TestMethod]
    public void HasInterpolationsReturnsExpectedResult()
    {
        var interpolatedStringWithHole = CoreSyntaxTestHelper.GetSingleNode<InterpolatedStringExpressionSyntax>("""
                                                                                                                internal class Sample
                                                                                                                {
                                                                                                                    private string Value(int number) => $"{number}";
                                                                                                                }
                                                                                                                """);
        var interpolatedStringWithoutHole = CoreSyntaxTestHelper.GetSingleNode<InterpolatedStringExpressionSyntax>("""
                                                                                                                   internal class Sample
                                                                                                                   {
                                                                                                                       private string Value() => $"plain";
                                                                                                                   }
                                                                                                                   """);

        Assert.IsTrue(StringInterpolationUtilities.HasInterpolations(interpolatedStringWithHole));
        Assert.IsFalse(StringInterpolationUtilities.HasInterpolations(interpolatedStringWithoutHole));
    }

    /// <summary>
    /// Verifies that unused interpolation markers are removed from plain interpolated strings
    /// </summary>
    [TestMethod]
    public void RemoveInterpolationMarkersReturnsPlainStringLiteralWhenNoHolesExist()
    {
        var interpolatedString = CoreSyntaxTestHelper.GetSingleNode<InterpolatedStringExpressionSyntax>("""
                                                                                                        internal class Sample
                                                                                                        {
                                                                                                            private string Value() => $"plain";
                                                                                                        }
                                                                                                        """);

        var updatedNode = StringInterpolationUtilities.RemoveInterpolationMarkers(interpolatedString);

        Assert.AreEqual("\"plain\"", updatedNode.ToString());
    }

    /// <summary>
    /// Verifies that verbatim string markers are preserved when removing interpolation markers
    /// </summary>
    [TestMethod]
    public void RemoveInterpolationMarkersPreservesVerbatimStringPrefixes()
    {
        var interpolatedString = CoreSyntaxTestHelper.GetSingleNode<InterpolatedStringExpressionSyntax>("""
                                                                                                        internal class Sample
                                                                                                        {
                                                                                                            private string Value() => $@"plain";
                                                                                                        }
                                                                                                        """);

        var updatedNode = StringInterpolationUtilities.RemoveInterpolationMarkers(interpolatedString);

        Assert.AreEqual("@\"plain\"", updatedNode.ToString());
    }

    /// <summary>
    /// Verifies that removing interpolation markers produces a plain string literal even for interpolation-hole text
    /// </summary>
    [TestMethod]
    public void RemoveInterpolationMarkersReturnsPlainStringLiteralForInterpolationHoleText()
    {
        var interpolatedString = CoreSyntaxTestHelper.GetSingleNode<InterpolatedStringExpressionSyntax>("""
                                                                                                        internal class Sample
                                                                                                        {
                                                                                                            private string Value(int number) => $"{number}";
                                                                                                        }
                                                                                                        """);

        var updatedNode = StringInterpolationUtilities.RemoveInterpolationMarkers(interpolatedString);

        Assert.AreEqual("\"{number}\"", updatedNode.ToString());
    }

    #endregion // Tests
}