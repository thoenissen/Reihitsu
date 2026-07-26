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
    /// Verifies that accessors can be reordered by moving one accessor before another
    /// </summary>
    [TestMethod]
    public void MoveAccessorBeforeMovesAccessorToTheRequestedPosition()
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

        var updatedAccessorList = AccessorOrderingUtilities.MoveAccessorBefore(accessorList, getAccessor, setAccessor);

        Assert.AreEqual(SyntaxKind.GetAccessorDeclaration, updatedAccessorList.Accessors[0].Kind());
        Assert.AreEqual(SyntaxKind.SetAccessorDeclaration, updatedAccessorList.Accessors[1].Kind());
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
    /// Verifies that a position sensitive directive inside the body of a crossed accessor is detected.
    /// <c>#pragma</c> applies from its own position onwards, so the guard cannot prove the move keeps its coverage
    /// </summary>
    [TestMethod]
    public void MoveRangeContainsDirectivesReturnsTrueWhenCrossedAccessorBodyContainsDirective()
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