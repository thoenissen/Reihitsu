using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="AccessorOrderingUtilities"/>
/// </summary>
[TestClass]
public class AccessorOrderingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that an out-of-order accessor pair is detected correctly
    /// </summary>
    [TestMethod]
    public void TryGetAccessorMoveReturnsAccessorPairWhenAccessorOrderIsInvalid()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          set;
                                                                                          get;
                                                                                      }
                                                                                  }
                                                                                  """);

        var result = AccessorOrderingUtilities.TryGetAccessorMove(accessorList,
                                                                  SyntaxKind.GetAccessorDeclaration,
                                                                  [SyntaxKind.SetAccessorDeclaration],
                                                                  out var accessorToMove,
                                                                  out var targetAccessor);

        Assert.IsTrue(result);
        Assert.AreEqual(SyntaxKind.GetAccessorDeclaration, accessorToMove.Kind());
        Assert.AreEqual(SyntaxKind.SetAccessorDeclaration, targetAccessor.Kind());
    }

    /// <summary>
    /// Verifies that an already ordered accessor list is ignored
    /// </summary>
    [TestMethod]
    public void TryGetAccessorMoveReturnsFalseWhenAccessorOrderIsAlreadyCorrect()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          get;
                                                                                          set;
                                                                                      }
                                                                                  }
                                                                                  """);

        var result = AccessorOrderingUtilities.TryGetAccessorMove(accessorList,
                                                                  SyntaxKind.GetAccessorDeclaration,
                                                                  [SyntaxKind.SetAccessorDeclaration],
                                                                  out _,
                                                                  out _);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a preprocessor directive in the affected leading trivia is detected
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenLeadingTriviaContainsDirective()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                      #region SetterRegion
                                                                                          set
                                                                                          {
                                                                                          }
                                                                                      #endregion
                                                                                          get
                                                                                          {
                                                                                          }
                                                                                      }
                                                                                  }
                                                                                  """);
        var setAccessor = accessorList.Accessors[0];
        var getAccessor = accessorList.Accessors[1];

        var result = AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, getAccessor, setAccessor);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a preprocessor directive placed between an accessor attribute list and the accessor keyword is detected.
    /// The directive attaches to a token after the first token of the accessor, so it is invisible to a leading-trivia-only scan
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenDirectiveFollowsAttributeList()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          set
                                                                                          {
                                                                                          }

                                                                                          [System.Obsolete]
                                                                                  #if !DEBUG
                                                                                          get
                                                                                          {
                                                                                              return 0;
                                                                                          }
                                                                                  #endif
                                                                                      }
                                                                                  }
                                                                                  """);
        var setAccessor = accessorList.Accessors[0];
        var getAccessor = accessorList.Accessors[1];

        var result = AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, getAccessor, setAccessor);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a conditional directive pair contained completely inside the body of a crossed accessor does
    /// not block the move
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenCrossedAccessorBodyContainsBalancedConditional()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          set
                                                                                          {
                                                                                  #if DEBUG
                                                                                              Log(value);
                                                                                  #endif
                                                                                          }

                                                                                          get
                                                                                          {
                                                                                              return 0;
                                                                                          }
                                                                                      }
                                                                                  }
                                                                                  """);
        var setAccessor = accessorList.Accessors[0];
        var getAccessor = accessorList.Accessors[1];

        var result = AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, getAccessor, setAccessor);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning</c> pair that ends its own effect inside the body of a crossed accessor does not
    /// block the move: the pair stays with the accessor it lies in
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public void MoveRangeContainsDirectivesReturnsFalseWhenCrossedAccessorBodyContainsClosedWarningPair()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          set
                                                                                          {
                                                                                  #pragma warning disable CS0219
                                                                                              var unused = value;
                                                                                  #pragma warning restore CS0219
                                                                                          }

                                                                                          get
                                                                                          {
                                                                                              return 0;
                                                                                          }
                                                                                      }
                                                                                  }
                                                                                  """);
        var setAccessor = accessorList.Accessors[0];
        var getAccessor = accessorList.Accessors[1];

        var result = AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, getAccessor, setAccessor);

        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a <c>#nullable disable</c> inside the body of a crossed accessor that the opposite setting closes,
    /// instead of <c>#nullable restore</c>, blocks the move
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public void MoveRangeContainsDirectivesReturnsTrueWhenCrossedAccessorBodyContainsNullableDisableClosedByEnable()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          set
                                                                                          {
                                                                                  #nullable disable
                                                                                              string text = null;
                                                                                  #nullable enable
                                                                                          }

                                                                                          get
                                                                                          {
                                                                                              return 0;
                                                                                          }
                                                                                      }
                                                                                  }
                                                                                  """);
        var setAccessor = accessorList.Accessors[0];
        var getAccessor = accessorList.Accessors[1];

        var result = AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, getAccessor, setAccessor);

        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that an accessor list without preprocessor directives is reported as safe to move
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsFalseWhenNoDirectivesArePresent()
    {
        var accessorList = CoreSyntaxTestHelper.GetSingleNode<AccessorListSyntax>("""
                                                                                  internal class Sample
                                                                                  {
                                                                                      public int Value
                                                                                      {
                                                                                          set;
                                                                                          get;
                                                                                      }
                                                                                  }
                                                                                  """);
        var setAccessor = accessorList.Accessors[0];
        var getAccessor = accessorList.Accessors[1];

        var result = AccessorOrderingUtilities.MoveRangeContainsDirectives(accessorList, getAccessor, setAccessor);

        Assert.IsFalse(result);
    }

    #endregion // Tests
}