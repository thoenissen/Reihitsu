using System;
using System.Linq;

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

        Assert.IsEmpty(AttributeTargetUtilities.GetAttributeLists(updated));
    }

    /// <summary>
    /// Verifies that attribute lists can be replaced on a lambda expression
    /// </summary>
    [TestMethod]
    public void WithAttributeListsClearsLambdaAttributes()
    {
        var lambda = CoreSyntaxTestHelper.GetSingleNode<LambdaExpressionSyntax>(LambdaSource);

        var updated = AttributeTargetUtilities.WithAttributeLists(lambda, default);

        Assert.IsEmpty(AttributeTargetUtilities.GetAttributeLists(updated));
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list on a parameter resolves to the parameter layout target
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetResolvesPropertyTargetOnParameterToParameter()
    {
        var attributeList = GetAttributeList("internal record Sample([property: System.Obsolete] int Value);", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Parameter, target);
    }

    /// <summary>
    /// Verifies that a <c>field:</c> list on a parameter resolves to the parameter layout target
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetResolvesFieldTargetOnParameterToParameter()
    {
        var attributeList = GetAttributeList("internal class Sample([field: System.Obsolete] int value);", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Parameter, target);
    }

    /// <summary>
    /// Verifies that a <c>typevar:</c> list on a parameter keeps its generic-parameter target
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetKeepsTypeVariableTargetOnParameter()
    {
        var attributeList = GetAttributeList("internal record Sample([typevar: System.Obsolete] int Value);", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.GenericParameter, target);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list on a type parameter resolves to the generic-parameter layout target
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetResolvesPropertyTargetOnTypeParameterToGenericParameter()
    {
        var attributeList = GetAttributeList("internal class Sample<[property: System.Obsolete] T>;", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.GenericParameter, target);
    }

    /// <summary>
    /// Verifies that a <c>param:</c> list on a type parameter keeps its parameter target
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetKeepsParameterTargetOnTypeParameter()
    {
        var attributeList = GetAttributeList("internal class Sample<[param: System.Obsolete] T>;", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Parameter, target);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list on a property keeps its property target
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetKeepsPropertyTargetOnProperty()
    {
        var attributeList = GetAttributeList("internal class Sample { [property: System.Obsolete] public int Value { get; set; } }", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target);

        Assert.IsTrue(resolved);
        Assert.AreEqual(AttributeTargets.Property, target);
    }

    /// <summary>
    /// Verifies that an unrecognized specifier on a parameter is not resolved
    /// </summary>
    [TestMethod]
    public void TryResolveLayoutTargetRejectsUnrecognizedTargetOnParameter()
    {
        var attributeList = GetAttributeList("internal record Sample([unknown: System.Obsolete] int Value);", 0);

        var resolved = AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out _);

        Assert.IsFalse(resolved);
    }

    /// <summary>
    /// Verifies that two lists with the same explicit target are recognized as having the same target
    /// </summary>
    [TestMethod]
    public void HaveSameTargetReturnsTrueForListsWithSameTarget()
    {
        const string source = "internal record Sample([property: System.Obsolete] [property: System.CLSCompliant(false)] int Value);";

        Assert.IsTrue(AttributeTargetUtilities.HaveSameTarget(GetAttributeList(source, 0), GetAttributeList(source, 1)));
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list and an implicit list on one parameter do not have the same target
    /// </summary>
    [TestMethod]
    public void HaveSameTargetReturnsFalseForPropertyAndImplicitParameterTargets()
    {
        const string source = "internal record Sample([property: System.Obsolete] [System.CLSCompliant(false)] int Value);";

        Assert.IsFalse(AttributeTargetUtilities.HaveSameTarget(GetAttributeList(source, 0), GetAttributeList(source, 1)));
    }

    /// <summary>
    /// Verifies that a <c>param:</c> list and an implicit list on one parameter have the same target
    /// </summary>
    [TestMethod]
    public void HaveSameTargetReturnsTrueForParameterAndImplicitParameterTargets()
    {
        const string source = "internal record Sample([param: System.Obsolete] [System.CLSCompliant(false)] int Value);";

        Assert.IsTrue(AttributeTargetUtilities.HaveSameTarget(GetAttributeList(source, 0), GetAttributeList(source, 1)));
    }

    /// <summary>
    /// Verifies that two lists with an unrecognized specifier are never treated as having the same target
    /// </summary>
    [TestMethod]
    public void HaveSameTargetReturnsFalseForUnrecognizedTargets()
    {
        const string source = "internal record Sample([unknown: System.Obsolete] [unknown: System.CLSCompliant(false)] int Value);";

        Assert.IsFalse(AttributeTargetUtilities.HaveSameTarget(GetAttributeList(source, 0), GetAttributeList(source, 1)));
    }

    /// <summary>
    /// Verifies that a <c>type:</c> list on a parameter, which the compiler ignores, never has the same target as a
    /// list without a specifier
    /// </summary>
    [TestMethod]
    public void HaveSameTargetReturnsFalseForTypeTargetOnParameter()
    {
        const string source = "internal record Sample([System.CLSCompliant(false)] [type: System.Obsolete] int Value);";

        Assert.IsFalse(AttributeTargetUtilities.HaveSameTarget(GetAttributeList(source, 0), GetAttributeList(source, 1)));
    }

    /// <summary>
    /// Verifies that a <c>type:</c> list on a class has the same target as a list without a specifier
    /// </summary>
    [TestMethod]
    public void HaveSameTargetReturnsTrueForTypeTargetOnClass()
    {
        const string source = "[type: System.Obsolete] [System.CLSCompliant(false)] internal class Sample { }";

        Assert.IsTrue(AttributeTargetUtilities.HaveSameTarget(GetAttributeList(source, 0), GetAttributeList(source, 1)));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets an attribute list from the given source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="index">Zero-based index of the attribute list in document order</param>
    /// <returns>The attribute list</returns>
    private static AttributeListSyntax GetAttributeList(string source, int index)
    {
        return CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                   .DescendantNodes()
                                   .OfType<AttributeListSyntax>()
                                   .ElementAt(index);
    }

    #endregion // Methods
}