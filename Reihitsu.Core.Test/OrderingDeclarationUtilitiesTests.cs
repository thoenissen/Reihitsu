using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="OrderingDeclarationUtilities"/>
/// </summary>
[TestClass]
public class OrderingDeclarationUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that compound accessibility modifiers are mapped to the expected accessibility group
    /// </summary>
    [TestMethod]
    public void GetAccessibilityGroupReturnsProtectedInternalForCompoundAccessibility()
    {
        var methodDeclaration = CoreSyntaxTestHelper.GetSingleMember<MethodDeclarationSyntax>("""
                                                                                              internal class Sample
                                                                                              {
                                                                                                  protected internal void Run()
                                                                                                  {
                                                                                                  }
                                                                                              }
                                                                                              """);

        var accessibilityGroup = OrderingDeclarationUtilities.GetAccessibilityGroup(methodDeclaration);

        Assert.AreEqual(OrderingAccessibilityGroup.ProtectedInternal, accessibilityGroup);
    }

    /// <summary>
    /// Verifies that member kind, static detection, and const detection are derived from the declaration
    /// </summary>
    [TestMethod]
    public void MemberClassificationHelpersReturnExpectedValues()
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                             internal class Sample
                                                             {
                                                                 private const int Value = 0;

                                                                 private static void Run()
                                                                 {
                                                                 }
                                                             }
                                                             """);
        var fieldDeclaration = root.DescendantNodes().OfType<FieldDeclarationSyntax>().Single();
        var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        Assert.AreEqual(OrderingMemberKindGroup.Field, OrderingDeclarationUtilities.GetMemberKind(fieldDeclaration));
        Assert.AreEqual(OrderingMemberKindGroup.Method, OrderingDeclarationUtilities.GetMemberKind(methodDeclaration));
        Assert.IsTrue(OrderingDeclarationUtilities.IsConst(fieldDeclaration));
        Assert.IsTrue(OrderingDeclarationUtilities.IsStatic(methodDeclaration));
    }

    /// <summary>
    /// Verifies that diagnostic-based lookup helpers find the expected member and containing type
    /// </summary>
    [TestMethod]
    public void DiagnosticLookupHelpersFindContainingDeclarations()
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                             internal class Sample
                                                             {
                                                                 public void Run()
                                                                 {
                                                                 }
                                                             }
                                                             """);
        var typeDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        var diagnostic = CoreSyntaxTestHelper.CreateDiagnostic(methodDeclaration.Identifier.GetLocation());

        var hasContainingTypeAndMember = OrderingDeclarationUtilities.TryGetContainingTypeAndMember(root,
                                                                                                    diagnostic,
                                                                                                    out var resolvedTypeDeclaration,
                                                                                                    out var resolvedMemberDeclaration);
        var hasMember = OrderingDeclarationUtilities.TryGetMemberDeclaration(root, diagnostic, out var resolvedMemberDeclarationOnly);
        var diagnosticLocation = OrderingDeclarationUtilities.GetDiagnosticLocation(methodDeclaration);
        var diagnosticLocationText = diagnosticLocation.SourceTree.GetText(TestContext.CancellationToken)
                                                                  .ToString(diagnosticLocation.SourceSpan);

        Assert.IsTrue(hasContainingTypeAndMember);
        Assert.IsTrue(hasMember);
        Assert.AreSame(typeDeclaration, resolvedTypeDeclaration);
        Assert.AreSame(methodDeclaration, resolvedMemberDeclaration);
        Assert.AreSame(methodDeclaration, resolvedMemberDeclarationOnly);
        Assert.AreEqual("Run", diagnosticLocationText);
    }

    /// <summary>
    /// Verifies that members can be reordered within a type declaration
    /// </summary>
    [TestMethod]
    public void MoveMemberBeforeMovesMemberToTheRequestedPosition()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void Second()
                                                                                {
                                                                                }

                                                                                private void First()
                                                                                {
                                                                                }
                                                                            }
                                                                            """);
        var secondMethod = typeDeclaration.Members.OfType<MethodDeclarationSyntax>().Single(obj => obj.Identifier.ValueText == "Second");
        var firstMethod = typeDeclaration.Members.OfType<MethodDeclarationSyntax>().Single(obj => obj.Identifier.ValueText == "First");

        var updatedTypeDeclaration = OrderingDeclarationUtilities.MoveMemberBefore(typeDeclaration, firstMethod, secondMethod);

        CollectionAssert.AreEqual(new[] { "First", "Second" },
                                  updatedTypeDeclaration.Members.OfType<MethodDeclarationSyntax>()
                                                                .Select(obj => obj.Identifier.ValueText)
                                                                .ToArray());
    }

    #endregion // Tests
}