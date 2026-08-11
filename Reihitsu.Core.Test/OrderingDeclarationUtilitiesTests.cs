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
    /// Verifies that readonly ordering treats implicit interface accessibility as public while preserving explicit
    /// public and private modifiers
    /// </summary>
    [TestMethod]
    public void GetEffectiveAccessibilityGroupForReadonlyOrderingUsesInterfaceDefaults()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            public interface ISample
                                                                            {
                                                                                static int Implicit = 0;
                                                                                public static readonly int ExplicitPublic = 1;
                                                                                private static readonly int ExplicitPrivate = 2;
                                                                            }
                                                                            """);
        var fields = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().ToArray();

        Assert.AreEqual(OrderingAccessibilityGroup.None, OrderingDeclarationUtilities.GetAccessibilityGroup(fields[0]));
        Assert.AreEqual(OrderingAccessibilityGroup.Public, OrderingDeclarationUtilities.GetAccessibilityGroup(fields[1]));
        Assert.AreEqual(OrderingAccessibilityGroup.Public, OrderingDeclarationUtilities.GetEffectiveAccessibilityGroupForReadonlyOrdering(typeDeclaration, fields[0]));
        Assert.AreEqual(OrderingAccessibilityGroup.Public, OrderingDeclarationUtilities.GetEffectiveAccessibilityGroupForReadonlyOrdering(typeDeclaration, fields[1]));
        Assert.AreEqual(OrderingAccessibilityGroup.Private, OrderingDeclarationUtilities.GetEffectiveAccessibilityGroupForReadonlyOrdering(typeDeclaration, fields[2]));
    }

    /// <summary>
    /// Verifies that implicit accessibility outside interfaces retains the existing syntactic accessibility group
    /// </summary>
    [TestMethod]
    public void GetEffectiveAccessibilityGroupForReadonlyOrderingKeepsNonInterfaceDefaults()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                static int Implicit;
                                                                                private static readonly int ExplicitPrivate;
                                                                            }
                                                                            """);
        var fields = typeDeclaration.Members.OfType<FieldDeclarationSyntax>().ToArray();

        Assert.AreEqual(OrderingAccessibilityGroup.None, OrderingDeclarationUtilities.GetEffectiveAccessibilityGroupForReadonlyOrdering(typeDeclaration, fields[0]));
        Assert.AreEqual(OrderingAccessibilityGroup.Private, OrderingDeclarationUtilities.GetEffectiveAccessibilityGroupForReadonlyOrdering(typeDeclaration, fields[1]));
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
    /// Verifies that a preprocessor directive in the leading trivia of a crossed member is detected
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenLeadingTriviaContainsDirective()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                            #region Fields

                                                                                private int _instance;

                                                                                private static int _static;

                                                                            #endregion
                                                                            }
                                                                            """);
        var instanceField = typeDeclaration.Members[0];
        var staticField = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, staticField, instanceField);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a preprocessor directive placed between an attribute list and the declaration keyword is detected.
    /// The directive attaches to a token after the first token of the member, so it is invisible to a leading-trivia-only scan
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenDirectiveFollowsAttributeList()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                                }

                                                                                [System.Obsolete]
                                                                            #if !DEBUG
                                                                                private static void Second()
                                                                                {
                                                                                }
                                                                            #endif
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a preprocessor directive placed between two modifiers is detected
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenDirectiveFollowsModifier()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                                }

                                                                                private
                                                                            #region Second
                                                                                static void Second()
                                                                                {
                                                                                }
                                                                            #endregion
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a conditional directive pair contained completely inside the body of a crossed member does not
    /// block the move. The pair stays intact and the moved member is outside it before and after the move
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenCrossedMemberBodyContainsBalancedConditional()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                            #if DEBUG
                                                                                    Log();
                                                                            #endif
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                                }
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a conditional directive pair contained completely inside the body of the moved member does not
    /// block the move, since the pair travels together with the member
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenMovedMemberBodyContainsBalancedConditional()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                            #if DEBUG
                                                                                    Log();
                                                                            #endif
                                                                                }
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a region pair contained completely inside a crossed nested type does not block the move
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenCrossedMemberContainsBalancedRegion()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                            #region Body

                                                                                    Log();

                                                                            #endregion
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                                }
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a conditional directive whose partner sits outside the moved member blocks the move
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenMovedMemberBodyContainsUnbalancedConditional()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                            #if !DEBUG
                                                                                }

                                                                                private void Third()
                                                                                {
                                                                                }
                                                                            #endif
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a position sensitive directive inside the body of a crossed member is detected.
    /// <c>#pragma</c> applies from its own position onwards, so the guard cannot prove the move keeps its coverage
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenCrossedMemberBodyContainsDirective()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                            #pragma warning disable CS0219
                                                                                    var unused = 0;
                                                                            #pragma warning restore CS0219
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                                }
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a directive on a member outside the moved range does not block the move,
    /// so the widened scan stays limited to the members the move actually touches
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenDirectiveSitsOutsideTheMovedRange()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                private void First()
                                                                                {
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                                }

                                                                            #region Untouched

                                                                                private int _untouched;

                                                                            #endregion
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a type declaration without preprocessor directives is reported as safe to reorder
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenNoDirectivesArePresent()
    {
        var typeDeclaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration("""
                                                                            internal class Sample
                                                                            {
                                                                                [System.Obsolete]
                                                                                private void First()
                                                                                {
                                                                                }

                                                                                private static void Second()
                                                                                {
                                                                                }
                                                                            }
                                                                            """);
        var firstMethod = typeDeclaration.Members[0];
        var secondMethod = typeDeclaration.Members[1];

        var result = OrderingDeclarationUtilities.MoveRangeContainsDirectives(typeDeclaration, secondMethod, firstMethod);

        Assert.IsFalse(result);
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