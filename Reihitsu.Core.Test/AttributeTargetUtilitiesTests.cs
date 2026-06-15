using System;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="AttributeTargetUtilities"/>
/// </summary>
[TestClass]
public class AttributeTargetUtilitiesTests
{
    #region Constants

    /// <summary>
    /// Destructor carrying an attribute
    /// </summary>
    private const string DestructorSource = """
                                            internal class Sample
                                            {
                                                [System.Obsolete]
                                                ~Sample()
                                                {
                                                }
                                            }
                                            """;

    /// <summary>
    /// Enum member carrying an attribute
    /// </summary>
    private const string EnumMemberSource = """
                                            internal enum Sample
                                            {
                                                [System.Obsolete]
                                                First,
                                            }
                                            """;

    /// <summary>
    /// Lambda carrying a C# 10 lambda attribute
    /// </summary>
    private const string LambdaSource = """
                                        internal class Sample
                                        {
                                            private void Run()
                                            {
                                                System.Action action = [System.Obsolete] () => { };
                                            }
                                        }
                                        """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that attribute lists on a destructor are returned
    /// </summary>
    [TestMethod]
    public void GetAttributeListsReturnsDestructorAttributes()
    {
        var destructor = CoreSyntaxTestHelper.GetSingleNode<DestructorDeclarationSyntax>(DestructorSource);

        var attributeLists = AttributeTargetUtilities.GetAttributeLists(destructor);

        Assert.HasCount(1, attributeLists);
    }

    /// <summary>
    /// Verifies that attribute lists on an enum member are returned
    /// </summary>
    [TestMethod]
    public void GetAttributeListsReturnsEnumMemberAttributes()
    {
        var enumMember = CoreSyntaxTestHelper.GetSingleNode<EnumMemberDeclarationSyntax>(EnumMemberSource);

        var attributeLists = AttributeTargetUtilities.GetAttributeLists(enumMember);

        Assert.HasCount(1, attributeLists);
    }

    /// <summary>
    /// Verifies that attribute lists on a lambda expression are returned
    /// </summary>
    [TestMethod]
    public void GetAttributeListsReturnsLambdaAttributes()
    {
        var lambda = CoreSyntaxTestHelper.GetSingleNode<LambdaExpressionSyntax>(LambdaSource);

        var attributeLists = AttributeTargetUtilities.GetAttributeLists(lambda);

        Assert.HasCount(1, attributeLists);
    }

    /// <summary>
    /// Verifies that a destructor attribute resolves to the method target
    /// </summary>
    [TestMethod]
    public void TryResolveTargetResolvesDestructorAttributeToMethod()
    {
        var destructor = CoreSyntaxTestHelper.GetSingleNode<DestructorDeclarationSyntax>(DestructorSource);

        var resolved = AttributeTargetUtilities.TryResolveTarget(AttributeTargetUtilities.GetAttributeLists(destructor)[0], out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Method, target);
    }

    /// <summary>
    /// Verifies that an enum member attribute resolves to the field target
    /// </summary>
    [TestMethod]
    public void TryResolveTargetResolvesEnumMemberAttributeToField()
    {
        var enumMember = CoreSyntaxTestHelper.GetSingleNode<EnumMemberDeclarationSyntax>(EnumMemberSource);

        var resolved = AttributeTargetUtilities.TryResolveTarget(AttributeTargetUtilities.GetAttributeLists(enumMember)[0], out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Field, target);
    }

    /// <summary>
    /// Verifies that a lambda attribute resolves to the method target
    /// </summary>
    [TestMethod]
    public void TryResolveTargetResolvesLambdaAttributeToMethod()
    {
        var lambda = CoreSyntaxTestHelper.GetSingleNode<LambdaExpressionSyntax>(LambdaSource);

        var resolved = AttributeTargetUtilities.TryResolveTarget(AttributeTargetUtilities.GetAttributeLists(lambda)[0], out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Method, target);
    }

    /// <summary>
    /// Verifies that attribute lists can be replaced on a destructor
    /// </summary>
    [TestMethod]
    public void WithAttributeListsClearsDestructorAttributes()
    {
        var destructor = CoreSyntaxTestHelper.GetSingleNode<DestructorDeclarationSyntax>(DestructorSource);

        var updated = AttributeTargetUtilities.WithAttributeLists(destructor, default);

        Assert.HasCount(0, AttributeTargetUtilities.GetAttributeLists(updated));
    }

    /// <summary>
    /// Verifies that attribute lists can be replaced on a lambda expression
    /// </summary>
    [TestMethod]
    public void WithAttributeListsClearsLambdaAttributes()
    {
        var lambda = CoreSyntaxTestHelper.GetSingleNode<LambdaExpressionSyntax>(LambdaSource);

        var updated = AttributeTargetUtilities.WithAttributeLists(lambda, default);

        Assert.HasCount(0, AttributeTargetUtilities.GetAttributeLists(updated));
    }

    #endregion // Tests
}