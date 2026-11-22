using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider"/> on <c>#pragma warning</c> and <c>#nullable</c> directives in and in front of the moved and the crossed members
/// </summary>
[TestClass]
public class RH7103StaticElementsMustAppearBeforeInstanceElementsWarningPairTests : BatchCodeFixTestsBase<RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer, RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying the SHARED-ORDER-011 example: a <c>#pragma warning</c> pair inside a crossed method's body keeps the
    /// fix, and the pair stays in the method's body, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairInsideACrossedMethodBody(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  public void Run()
                                  {
                                  }

                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Run()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a <c>#pragma warning</c> pair inside the moved method's own body keeps the fix and moves with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberWhoseBodyHoldsAWarningPair()
    {
        const string source = """
                              public class C
                              {
                                  public void Run()
                                  {
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }

                                       public void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-011 example of a lone directive: a <c>#pragma warning disable</c> inside a crossed
    /// method's body that no <c>restore</c> closes withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenACrossedMethodBodyHoldsALoneWarningDisable()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a <c>restore</c> whose IDs differ from those of the <c>disable</c>, or name them in another order,
    /// or spell them differently, closes no pair, so the fix is withheld
    /// </summary>
    /// <param name="disable">IDs of the <c>disable</c></param>
    /// <param name="restore">IDs of the <c>restore</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("CS0168", "CS0219", DisplayName = "Other ID")]
    [DataRow("CS0168, CS0219", "CS0219, CS0168", DisplayName = "Other order")]
    [DataRow("CS0168", "168", DisplayName = "Other spelling")]
    [DataRow("CS0168, CS0219", "CS0168", DisplayName = "Fewer IDs")]
    [DataRow("CS0168", "", DisplayName = "IDs only on the disable")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenTheRestoreOfAPairInACrossedBodyNamesOtherIds(string disable, string restore)
    {
        var source = $$"""
                       public class C
                       {
                           public void Reset()
                           {
                       #pragma warning disable {{disable}}
                               int unused;
                       #pragma warning restore {{restore}}
                           }

                           public static void {|RH7103:Create|}()
                           {
                           }
                       }
                       """;

        // A restore without IDs enables every warning again, also behind a disable that the test harness puts in front
        await VerifyNoCodeFixIsOffered(source, test => test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck);
    }

    /// <summary>
    /// Verifying that a <c>disable</c> and a <c>restore</c> that both name no ID form a pair, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairWithoutIds()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable
                                      int unused;
                              #pragma warning restore
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable
                                           int unused;
                                   #pragma warning restore
                                       }
                                   }
                                   """;

        // A restore without IDs enables every warning again, also behind a disable that the test harness puts in front
        await Verify(source, fixedSource, test => test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck);
    }

    /// <summary>
    /// Verifying that a pair whose <c>disable</c> and <c>restore</c> name the same IDs in the same order, more than one,
    /// keeps the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairWithSeveralIds()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168, CS0219
                                      int unused;
                              #pragma warning restore CS0168, CS0219
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168, CS0219
                                           int unused;
                                   #pragma warning restore CS0168, CS0219
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the IDs are compared as tokens: the comment that Visual Studio's <c>Suppress in Source</c> writes
    /// behind both directives does not keep them from forming a pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairWithATrailingCommentOnBothDirectives()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168 // Variable is declared but never used
                                      int unused;
                              #pragma warning restore CS0168 // Variable is declared but never used
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168 // Variable is declared but never used
                                           int unused;
                                   #pragma warning restore CS0168 // Variable is declared but never used
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that two opening directives in a row, closed in reverse order, are not consecutive pairs, so the fix is
    /// withheld
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenACrossedBodyHoldsNestedWarningPairs()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                              #pragma warning disable CS0219
                                      int unused;
                                      int assigned = 0;
                              #pragma warning restore CS0219
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that two consecutive pairs inside one crossed body keep the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossTwoConsecutiveWarningPairsInACrossedBody()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                              #pragma warning disable CS0219
                                      int assigned = 0;
                              #pragma warning restore CS0219
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                   #pragma warning disable CS0219
                                           int assigned = 0;
                                   #pragma warning restore CS0219
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-011 sentence on <c>#nullable</c>: a <c>#nullable disable</c> closed by a
    /// <c>#nullable restore</c> with the same target inside a crossed method's body keeps the fix
    /// </summary>
    /// <param name="target">Target of both directives</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("", DisplayName = "No target")]
    [DataRow(" warnings", DisplayName = "Warnings")]
    [DataRow(" annotations", DisplayName = "Annotations")]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossANullableDisableClosedByRestoreInACrossedBody(string target)
    {
        var source = $$"""
                       public class C
                       {
                           public void Reset()
                           {
                       #nullable disable{{target}}
                               string text = null;
                       #nullable restore{{target}}
                           }

                           public static void {|RH7103:Create|}()
                           {
                           }
                       }
                       """;

        var fixedSource = $$"""
                            public class C
                            {
                                public static void Create()
                                {
                                }

                                public void Reset()
                                {
                            #nullable disable{{target}}
                                    string text = null;
                            #nullable restore{{target}}
                                }
                            }
                            """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a <c>#nullable enable</c> closed by <c>#nullable restore</c> inside a crossed method's body keeps
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossANullableEnableClosedByRestoreInACrossedBody()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #nullable enable
                                      string text = "";
                              #nullable restore
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #nullable enable
                                           string text = "";
                                   #nullable restore
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a <c>#nullable</c> directive closed by the opposite setting, or by a <c>#nullable restore</c> with
    /// another target, closes no pair, so the fix is withheld
    /// </summary>
    /// <param name="opening">Opening directive</param>
    /// <param name="closing">Closing directive</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#nullable disable", "#nullable enable", DisplayName = "Disable closed by enable")]
    [DataRow("#nullable enable", "#nullable disable", DisplayName = "Enable closed by disable")]
    [DataRow("#nullable disable warnings", "#nullable restore", DisplayName = "Restore of another target")]
    [DataRow("#nullable disable", "#nullable restore annotations", DisplayName = "Restore of a narrower target")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenANullableDirectiveInACrossedBodyIsNotClosedByItsRestore(string opening, string closing)
    {
        var source = $$"""
                       public class C
                       {
                           public void Reset()
                           {
                       {{opening}}
                               string text = "";
                       {{closing}}
                           }

                           public static void {|RH7103:Create|}()
                           {
                           }
                       }
                       """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a <c>#pragma warning</c> pair and a <c>#nullable</c> pair are judged each by its own kind: both
    /// pairs in one crossed body keep the fix, also where one pair encloses the other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairThatEnclosesANullablePair()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                              #nullable disable
                                      int unused;
                              #nullable restore
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                   #nullable disable
                                           int unused;
                                   #nullable restore
                                   #pragma warning restore CS0168
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the boundary of <i>Inside one declaration</i> on its refusing side: a <c>disable</c> in the leading
    /// trivia of a crossed declaration's first token is SHARED-ORDER-005's and withholds the fix, also when its
    /// <c>restore</c> stands in the declaration's body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenTheDisableOfAPairStandsInFrontOfTheFirstTokenOfACrossedMethod()
    {
        const string source = """
                              public class C
                              {
                                  public void Run()
                                  {
                                  }

                              #pragma warning disable CS0168
                                  public void Reset()
                                  {
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the boundary of <i>Inside one declaration</i> on its keeping side: a <c>disable</c> behind the attribute
    /// list of a crossed method, in the leading trivia of its first modifier, lies inside the declaration, so the pair it
    /// opens with a <c>restore</c> in the body keeps the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAPairWhoseDisableStandsBehindTheAttributeListOfACrossedMethod()
    {
        const string source = """
                              public class C
                              {
                                  public void Run()
                                  {
                                  }

                                  [System.Obsolete]
                              #pragma warning disable CS0168
                                  public void Reset()
                                  {
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Run()
                                       {
                                       }

                                       [System.Obsolete]
                                   #pragma warning disable CS0168
                                       public void Reset()
                                       {
                                           int unused;
                                   #pragma warning restore CS0168
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a member of a nested type is part of the nested type's declaration: a pair whose <c>disable</c>
    /// stands in front of the nested type's first member and whose <c>restore</c> stands in front of its second keeps the
    /// fix that moves a static member across the nested type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAPairBetweenTheMembersOfACrossedNestedType()
    {
        const string source = """
                              public class C
                              {
                                  public class Inner
                                  {
                              #pragma warning disable CS0169
                                      private int _unused;
                              #pragma warning restore CS0169
                                      private int _used;
                                  }

                                  public static class {|RH7103:Factory|}
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static class Factory
                                       {
                                       }

                                       public class Inner
                                       {
                                   #pragma warning disable CS0169
                                           private int _unused;
                                   #pragma warning restore CS0169
                                           private int _used;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-011 example of a directive of the pair's kind in front of the spans: a file-level
    /// <c>#pragma warning disable</c> that no <c>restore</c> closes withholds the fix, whichever IDs it names, also when it
    /// stands in an excluded branch
    /// </summary>
    /// <param name="inFront">Directives in front of the type</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#pragma warning disable CS1591\n", DisplayName = "Other ID")]
    [DataRow("#pragma warning disable CS0168\n", DisplayName = "Same ID")]
    [DataRow("#if NEVER_DEFINED\n#pragma warning disable CS1591\n#endif\n", DisplayName = "Excluded branch")]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenAnUnclosedWarningDirectiveStandsInFrontOfTheSpans(string inFront)
    {
        var source = $$"""
                       {{inFront}}public class C
                       {
                           public void Run()
                           {
                           }

                           public void Reset()
                           {
                       #pragma warning disable CS0168
                               int unused;
                       #pragma warning restore CS0168
                           }

                           public static void {|RH7103:Create|}()
                           {
                           }
                       }
                       """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that <i>Nothing open in front</i> reads only the directives of the pair's kind: a file-level
    /// <c>#nullable enable</c> withholds the fix across a <c>#nullable</c> pair, and keeps it across a <c>
    /// #pragma
    /// warning
    /// </c> pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task UnclosedNullableDirectiveInFrontOfTheSpansWithholdsOnlyTheMoveAcrossANullablePair()
    {
        const string nullablePair = """
                                    #nullable enable
                                    public class C
                                    {
                                        public void Reset()
                                        {
                                    #nullable disable
                                            string text = null;
                                    #nullable restore
                                        }

                                        public static void {|RH7103:Create|}()
                                        {
                                        }
                                    }
                                    """;

        await VerifyNoCodeFixIsOffered(nullablePair);

        const string warningPair = """
                                   #nullable enable
                                   public class C
                                   {
                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }

                                       public static void {|RH7103:Create|}()
                                       {
                                       }
                                   }
                                   """;

        const string warningPairFixed = """
                                        #nullable enable
                                        public class C
                                        {
                                            public static void Create()
                                            {
                                            }

                                            public void Reset()
                                            {
                                        #pragma warning disable CS0168
                                                int unused;
                                        #pragma warning restore CS0168
                                            }
                                        }
                                        """;

        await Verify(warningPair, warningPairFixed);
    }

    /// <summary>
    /// Verifying the SHARED-ORDER-011 example of a closed pair in front of the spans: a pair inside an earlier member's
    /// body leaves the project's setting behind it, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7103-007")]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairBehindAClosedPairInFrontOfTheSpans()
    {
        const string source = """
                              public class C
                              {
                                  public static void Log()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public void Run()
                                  {
                                  }

                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Log()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }

                                       public static void Create()
                                       {
                                       }

                                       public void Run()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a closed pair in front of the spans need not stand inside one declaration: a pair around an earlier
    /// type keeps the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberAcrossAWarningPairBehindAClosedPairAroundAnEarlierType()
    {
        const string source = """
                              #pragma warning disable CS0169
                              public class Earlier
                              {
                                  private int _unused;
                              }
                              #pragma warning restore CS0169

                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   #pragma warning disable CS0169
                                   public class Earlier
                                   {
                                       private int _unused;
                                   }
                                   #pragma warning restore CS0169

                                   public class C
                                   {
                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a closed pair in front of the spans alone, with no directive in the spans, keeps the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberBehindAClosedWarningPairInFrontOfTheSpans()
    {
        const string source = """
                              public class C
                              {
                                  public static void Log()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public void Run()
                                  {
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Log()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }

                                       public static void Create()
                                       {
                                       }

                                       public void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying <i>One branch</i> for the pairs in front of the spans: a pair in an earlier member's body whose
    /// <c>restore</c> stands in a conditional branch that its <c>disable</c> is not in is no closed pair, so a pair in a
    /// crossed body withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenAPairInFrontOfTheSpansIsSplitAcrossAConditionalBranch()
    {
        const string source = """
                              public class C
                              {
                                  public static void Log()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #if DEBUG
                              #pragma warning restore CS0168
                              #endif
                                  }

                                  public void Run()
                                  {
                                  }

                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a conditional group that lies wholly between the two directives of a pair in front of the spans
    /// keeps the pair closed, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task FixMovesAMemberBehindAPairInFrontOfTheSpansThatEnclosesAWholeConditionalGroup()
    {
        const string source = """
                              public class C
                              {
                                  public static void Log()
                                  {
                              #pragma warning disable CS0168
                              #if DEBUG
                                      int unused;
                              #endif
                              #pragma warning restore CS0168
                                  }

                                  public void Reset()
                                  {
                              #pragma warning disable CS0168
                                      int unused;
                              #pragma warning restore CS0168
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       public static void Log()
                                       {
                                   #pragma warning disable CS0168
                                   #if DEBUG
                                           int unused;
                                   #endif
                                   #pragma warning restore CS0168
                                       }

                                       public static void Create()
                                       {
                                       }

                                       public void Reset()
                                       {
                                   #pragma warning disable CS0168
                                           int unused;
                                   #pragma warning restore CS0168
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that every directive that is neither a conditional, a region, a <c>#pragma warning</c>, nor a
    /// <c>#nullable</c> directive refuses the move wherever it stands in the spans, also inside a crossed method's body
    /// </summary>
    /// <param name="directives">Directives inside the crossed body</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#line default\n        int unused;", DisplayName = "Line")]
    [DataRow("#warning Check this\n        int unused;", DisplayName = "Warning")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenACrossedMethodBodyHoldsAnotherPositionSensitiveDirective(string directives)
    {
        var source = $$"""
                       public class C
                       {
                           public void Reset()
                           {
                       {{directives}}
                           }

                           public static void {|RH7103:Create|}()
                           {
                           }
                       }
                       """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the guard tells a <c>#pragma warning</c> directive apart from another <c>#pragma</c>: a
    /// <c>#pragma checksum</c> inside a crossed method's body withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenACrossedMethodBodyHoldsAPragmaChecksum()
    {
        const string source = """
                              public class C
                              {
                                  public void Reset()
                                  {
                              #pragma checksum "file.cs" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ab007f1d23d9"
                                  }

                                  public static void {|RH7103:Create|}()
                                  {
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    [TestMethod]
    [Requirement("RH7103-008")]
    [Requirement("SHARED-ORDER-011")]
    public override async Task FixAllCorrectsEveryDiagnostic()
    {
        await base.FixAllCorrectsEveryDiagnostic();
    }

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class C
                                {
                                    public void Reset()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }

                                    public static void {|#0:CreateA|}()
                                    {
                                    }

                                    public static void {|#1:CreateB|}()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     public static void CreateA()
                                     {
                                     }

                                     public static void CreateB()
                                     {
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                     }

                                     public void Reset()
                                     {
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                     }
                                 }
                                 """;

        // Verifying that Fix All moves two static members across a pair in a crossed body, one of them with a pair in its own body, in one application
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, MessageFormat, 2),
                                  test => test.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}