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
    #region Fields

    /// <summary>
    /// Expected member order after moving a member to the requested position
    /// </summary>
    private static readonly string[] _movedMemberOrder = ["First", "Second"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
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

        CollectionAssert.AreEqual(_movedMemberOrder,
                                  updatedTypeDeclaration.Members.OfType<MethodDeclarationSyntax>()
                                                                .Select(obj => obj.Identifier.ValueText)
                                                                .ToArray());
    }

    /// <summary>
    /// Verifies that moving a static member past a static event field initializer is flagged as changing initializer execution order
    /// </summary>
    [TestMethod]
    public void ChangesInitializerExecutionOrderReturnsTrueWhenMemberIsMovedPastStaticEventFieldInitializer()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                public static event System.EventHandler E = Handler;

                                                                                private static readonly int _order = Register();

                                                                                private static System.EventHandler Handler => null;

                                                                                private static int Register()
                                                                                {
                                                                                    return 0;
                                                                                }
                                                                            }
                                                                            """);
        var eventField = typeDeclaration.Members.OfType<EventFieldDeclarationSyntax>().Single();
        var orderField = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().Single();

        var changesOrder = OrderingDeclarationUtilities.ChangesInitializerExecutionOrder(typeDeclaration, orderField, eventField);

        Assert.IsTrue(changesOrder);
    }

    /// <summary>
    /// Verifies that moving an instance member past an instance event field initializer is flagged as changing initializer execution order
    /// </summary>
    [TestMethod]
    public void ChangesInitializerExecutionOrderReturnsTrueWhenMemberIsMovedPastInstanceEventFieldInitializer()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                public event System.EventHandler E = Handler;

                                                                                private readonly int _order = Register();

                                                                                private static System.EventHandler Handler => null;

                                                                                private static int Register()
                                                                                {
                                                                                    return 0;
                                                                                }
                                                                            }
                                                                            """);
        var eventField = typeDeclaration.Members.OfType<EventFieldDeclarationSyntax>().Single();
        var orderField = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().Single();

        var changesOrder = OrderingDeclarationUtilities.ChangesInitializerExecutionOrder(typeDeclaration, orderField, eventField);

        Assert.IsTrue(changesOrder);
    }

    /// <summary>
    /// Verifies that moving an event field that carries an initializer past another initialized member is itself flagged as changing initializer execution order
    /// </summary>
    [TestMethod]
    public void ChangesInitializerExecutionOrderReturnsTrueWhenEventFieldWithInitializerIsMovedPastInitializedMember()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private static readonly int _order = Register();

                                                                                public static event System.EventHandler E = Handler;

                                                                                private static System.EventHandler Handler => null;

                                                                                private static int Register()
                                                                                {
                                                                                    return 0;
                                                                                }
                                                                            }
                                                                            """);
        var orderField = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().Single();
        var eventField = typeDeclaration.Members.OfType<EventFieldDeclarationSyntax>().Single();

        var changesOrder = OrderingDeclarationUtilities.ChangesInitializerExecutionOrder(typeDeclaration, eventField, orderField);

        Assert.IsTrue(changesOrder);
    }

    /// <summary>
    /// Verifies that an event field without an initializer does not block a move, since it carries no runtime side effect
    /// </summary>
    [TestMethod]
    public void ChangesInitializerExecutionOrderReturnsFalseWhenPassedOverEventFieldHasNoInitializer()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                public static event System.EventHandler E;

                                                                                private static readonly int _order = Register();

                                                                                private static int Register()
                                                                                {
                                                                                    return 0;
                                                                                }
                                                                            }
                                                                            """);
        var eventField = typeDeclaration.Members.OfType<EventFieldDeclarationSyntax>().Single();
        var orderField = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().Single();

        var changesOrder = OrderingDeclarationUtilities.ChangesInitializerExecutionOrder(typeDeclaration, orderField, eventField);

        Assert.IsFalse(changesOrder);
    }

    #endregion // Tests
}