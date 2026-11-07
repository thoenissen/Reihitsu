using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2005FieldsMustBePrivateAnalyzer"/> and <see cref="RH2005FieldsMustBePrivateCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH2005FieldsMustBePrivateAnalyzerTests : BatchCodeFixTestsBase<RH2005FieldsMustBePrivateAnalyzer, RH2005FieldsMustBePrivateCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Source of a <c>static public</c> field behind an attribute list on its own line
    /// </summary>
    private const string AttributedStaticPublicFieldSource = """
                                                             using System;

                                                             public class C
                                                             {
                                                                 [Obsolete]
                                                                 static public int {|#0:Counter|};
                                                             }
                                                             """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="AttributedStaticPublicFieldSource"/>
    /// </summary>
    private const string AttributedStaticPublicFieldFixedSource = """
                                                                  using System;

                                                                  public class C
                                                                  {
                                                                      [Obsolete]
                                                                      private static int Counter;
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of a <c>static public</c> field with a comment line and a directive line between its attribute list and
    /// the field
    /// </summary>
    private const string AttributedFieldWithCommentAndDirectiveSource = """
                                                                        using System;

                                                                        public class C
                                                                        {
                                                                            [Obsolete]
                                                                            // note
                                                                        #pragma warning disable CS0414
                                                                            static public int {|#0:Counter|} = 1;
                                                                        }
                                                                        """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="AttributedFieldWithCommentAndDirectiveSource"/>
    /// </summary>
    private const string AttributedFieldWithCommentAndDirectiveFixedSource = """
                                                                             using System;

                                                                             public class C
                                                                             {
                                                                                 [Obsolete]
                                                                                 // note
                                                                             #pragma warning disable CS0414
                                                                                 private static int Counter = 1;
                                                                             }
                                                                             """;

    /// <summary>
    /// Source of a <c>static</c> field whose <see langword="public"/> modifier starts its own line behind <c>static</c>
    /// </summary>
    private const string PublicOnItsOwnLineSource = """
                                                    public class C
                                                    {
                                                        static
                                                            public int {|#0:Counter|};
                                                    }
                                                    """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="PublicOnItsOwnLineSource"/>: <see langword="private"/> where
    /// <c>static</c> stood, and the line of the removed <see langword="public"/> kept in place
    /// </summary>
    private const string PublicOnItsOwnLineFixedSource = """
                                                         public class C
                                                         {
                                                             private static
                                                                 int Counter;
                                                         }
                                                         """;

    /// <summary>
    /// Source of a <c>static</c> field with a comment line between <c>static</c> and <see langword="public"/>
    /// </summary>
    private const string CommentLineInFrontOfPublicSource = """
                                                            public class C
                                                            {
                                                                static
                                                                    // note
                                                                    public int {|RH2005:Counter|};
                                                            }
                                                            """;

    /// <summary>
    /// Source of a <c>static</c> field with a directive line between <c>static</c> and <see langword="public"/>
    /// </summary>
    private const string DirectiveLineInFrontOfPublicSource = """
                                                              public class C
                                                              {
                                                                  static
                                                              #pragma warning disable CS0649
                                                                      public int {|RH2005:Counter|};
                                                              }
                                                              """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnotherModifier"/>
    /// </summary>
    private const string PublicEndingItsLineSource = """
                                                     public class C
                                                     {
                                                         static public
                                                             int {|#0:Counter|};
                                                     }
                                                     """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="PublicEndingItsLineSource"/>
    /// </summary>
    private const string PublicEndingItsLineFixedSource = """
                                                          public class C
                                                          {
                                                              private static
                                                                  int Counter;
                                                          }
                                                          """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBehindAnotherModifier"/>
    /// </summary>
    private const string PublicStartingAndEndingItsLineSource = """
                                                                public class C
                                                                {
                                                                    static
                                                                        public
                                                                        int {|#0:Counter|};
                                                                }
                                                                """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="PublicStartingAndEndingItsLineSource"/>
    /// </summary>
    private const string PublicStartingAndEndingItsLineFixedSource = """
                                                                     public class C
                                                                     {
                                                                         private static
                                                                             int Counter;
                                                                     }
                                                                     """;

    /// <summary>
    /// Source of <see cref="VerifyFixMovesTheTypeIntoTheColumnOfAPublicThatStartsAndEndsItsOwnLine"/>
    /// </summary>
    private const string PublicStartingAndEndingItsLineInAnotherColumnSource = """
                                                                               public class C
                                                                               {
                                                                                   static
                                                                                       public
                                                                                           int {|#0:Counter|};
                                                                               }
                                                                               """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="PublicStartingAndEndingItsLineInAnotherColumnSource"/>
    /// </summary>
    private const string PublicStartingAndEndingItsLineInAnotherColumnFixedSource = """
                                                                                    public class C
                                                                                    {
                                                                                        private static
                                                                                            int Counter;
                                                                                    }
                                                                                    """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBetweenTwoModifiers"/>
    /// </summary>
    private const string MiddlePublicEndingItsLineSource = """
                                                           public class C
                                                           {
                                                               static public
                                                                   volatile int {|#0:Counter|};
                                                           }
                                                           """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="MiddlePublicEndingItsLineSource"/>
    /// </summary>
    private const string MiddlePublicEndingItsLineFixedSource = """
                                                                public class C
                                                                {
                                                                    private static
                                                                        volatile int Counter;
                                                                }
                                                                """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBetweenTwoModifiers"/>
    /// </summary>
    private const string MiddlePublicStartingAndEndingItsLineSource = """
                                                                      public class C
                                                                      {
                                                                          static
                                                                              public
                                                                              volatile int {|#0:Counter|};
                                                                      }
                                                                      """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="MiddlePublicStartingAndEndingItsLineSource"/>
    /// </summary>
    private const string MiddlePublicStartingAndEndingItsLineFixedSource = """
                                                                           public class C
                                                                           {
                                                                               private static
                                                                                   volatile int Counter;
                                                                           }
                                                                           """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBetweenTwoModifiers"/>
    /// </summary>
    private const string MiddlePublicStartingItsLineSource = """
                                                             public class C
                                                             {
                                                                 static
                                                                     public volatile int {|#0:Counter|};
                                                             }
                                                             """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="MiddlePublicStartingItsLineSource"/>
    /// </summary>
    private const string MiddlePublicStartingItsLineFixedSource = """
                                                                  public class C
                                                                  {
                                                                      private static
                                                                          volatile int Counter;
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindTwoModifiers"/>
    /// </summary>
    private const string LastPublicEndingItsLineSource = """
                                                         public class C
                                                         {
                                                             static volatile public
                                                                 int {|#0:Counter|};
                                                         }
                                                         """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="LastPublicEndingItsLineSource"/>
    /// </summary>
    private const string LastPublicEndingItsLineFixedSource = """
                                                              public class C
                                                              {
                                                                  private static volatile
                                                                      int Counter;
                                                              }
                                                              """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLinesOfModifiersInFrontOfAPublicThatEndsItsLine"/>
    /// </summary>
    private const string LastPublicEndingASecondLineSource = """
                                                             public class C
                                                             {
                                                                 static
                                                                     volatile public
                                                                         int {|#0:Counter|};
                                                             }
                                                             """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="LastPublicEndingASecondLineSource"/>
    /// </summary>
    private const string LastPublicEndingASecondLineFixedSource = """
                                                                  public class C
                                                                  {
                                                                      private static
                                                                          volatile
                                                                              int Counter;
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnAttributeList"/>
    /// </summary>
    private const string AttributedPublicEndingItsLineSource = """
                                                               public class C
                                                               {
                                                                   [System.Obsolete]
                                                                   static public
                                                                       int {|#0:Counter|};
                                                               }
                                                               """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="AttributedPublicEndingItsLineSource"/>
    /// </summary>
    private const string AttributedPublicEndingItsLineFixedSource = """
                                                                    public class C
                                                                    {
                                                                        [System.Obsolete]
                                                                        private static
                                                                            int Counter;
                                                                    }
                                                                    """;

    /// <summary>
    /// Source of <see cref="VerifyFixReplacesAPublicBetweenTwoModifiersOnOneLine"/>
    /// </summary>
    private const string MiddlePublicOnOneLineSource = """
                                                       public class C
                                                       {
                                                           static public volatile int {|#0:Counter|};
                                                       }
                                                       """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="MiddlePublicOnOneLineSource"/>
    /// </summary>
    private const string MiddlePublicOnOneLineFixedSource = """
                                                            public class C
                                                            {
                                                                private static volatile int Counter;
                                                            }
                                                            """;

    /// <summary>
    /// Source of <see cref="VerifyFixReplacesAPublicBehindTwoModifiersOnOneLine"/>
    /// </summary>
    private const string LastPublicOnOneLineSource = """
                                                     public class C
                                                     {
                                                         static volatile public int {|#0:Counter|};
                                                     }
                                                     """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="LastPublicOnOneLineSource"/>
    /// </summary>
    private const string LastPublicOnOneLineFixedSource = """
                                                          public class C
                                                          {
                                                              private static volatile int Counter;
                                                          }
                                                          """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheLineBreakBehindAFirstPublicThatEndsItsLine"/>
    /// </summary>
    private const string FirstPublicEndingItsLineSource = """
                                                          public class C
                                                          {
                                                              public
                                                                  static int {|#0:Counter|};
                                                          }
                                                          """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="FirstPublicEndingItsLineSource"/>
    /// </summary>
    private const string FirstPublicEndingItsLineFixedSource = """
                                                               public class C
                                                               {
                                                                   private
                                                                       static int Counter;
                                                               }
                                                               """;

    /// <summary>
    /// Source of <see cref="VerifyFixKeepsTheCommentBehindAFirstPublicThatEndsItsLine"/>
    /// </summary>
    private const string FirstPublicWithACommentBehindItSource = """
                                                                 public class C
                                                                 {
                                                                     public // note
                                                                         static int {|#0:Counter|};
                                                                 }
                                                                 """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="FirstPublicWithACommentBehindItSource"/>
    /// </summary>
    private const string FirstPublicWithACommentBehindItFixedSource = """
                                                                      public class C
                                                                      {
                                                                          private // note
                                                                              static int Counter;
                                                                      }
                                                                      """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForACommentBehindAPublicAtTheEndOfItsLine"/>
    /// </summary>
    private const string CommentBehindPublicSource = """
                                                     public class C
                                                     {
                                                         static public // note
                                                             int {|RH2005:Counter|};
                                                     }
                                                     """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheType"/>
    /// </summary>
    private const string BlockCommentBehindPublicSource = """
                                                          public class C
                                                          {
                                                              static public /* note */ int {|RH2005:Count|};
                                                          }
                                                          """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheType"/>
    /// </summary>
    private const string CommentLineBehindPublicSource = """
                                                         public class C
                                                         {
                                                             static public
                                                                 // note
                                                                 int {|RH2005:Total|};
                                                         }
                                                         """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForADirectiveLineBetweenAPublicAndTheType"/>
    /// </summary>
    private const string DirectiveLineBehindPublicSource = """
                                                           public class C
                                                           {
                                                               static public
                                                           #pragma warning disable CS0649
                                                                   int {|RH2005:Counter|};
                                                           }
                                                           """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForAnExcludedModifierBetweenAPublicAndTheType"/>
    /// </summary>
    private const string ExcludedModifierBehindPublicSource = """
                                                              public class C
                                                              {
                                                                  static public
                                                              #if NEVER
                                                                      readonly
                                                              #endif
                                                                      int {|RH2005:Total|};
                                                              }
                                                              """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForACommentBehindAPublicBetweenTwoModifiersAtTheEndOfItsLine"/>
    /// </summary>
    private const string CommentBehindMiddlePublicSource = """
                                                           public class C
                                                           {
                                                               static public // note
                                                                   volatile int {|RH2005:Counter|};
                                                           }
                                                           """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheNextModifier"/>
    /// </summary>
    private const string BlockCommentBehindMiddlePublicSource = """
                                                                public class C
                                                                {
                                                                    static public /* note */ volatile int {|RH2005:Counter|};
                                                                }
                                                                """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheNextModifier"/>
    /// </summary>
    private const string CommentLineBehindMiddlePublicSource = """
                                                               public class C
                                                               {
                                                                   static public
                                                                       // note
                                                                       volatile int {|RH2005:Counter|};
                                                               }
                                                               """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForACommentBehindAPublicThatStartsItsOwnLine"/>
    /// </summary>
    private const string CommentBehindPublicStartingItsLineSource = """
                                                                    public class C
                                                                    {
                                                                        static
                                                                            public // note
                                                                            int {|RH2005:Counter|};
                                                                    }
                                                                    """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForACommentLineBehindAPublicThatStartsItsOwnLine"/>
    /// </summary>
    private const string CommentLineBehindPublicStartingItsLineSource = """
                                                                        public class C
                                                                        {
                                                                            static
                                                                                public
                                                                                // note
                                                                                int {|RH2005:Counter|};
                                                                        }
                                                                        """;

    /// <summary>
    /// Source of <see cref="VerifyNoFixIsOfferedForADirectiveLineBehindAPublicBehindTwoModifiers"/>
    /// </summary>
    private const string DirectiveLineBehindLastPublicSource = """
                                                               public class C
                                                               {
                                                                   static volatile public
                                                               #pragma warning disable CS0649
                                                                       int {|RH2005:Counter|};
                                                               }
                                                               """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying that public fields in classes and record classes trigger diagnostics and are fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-001")]
    [Requirement("RH2005-002")]
    [Requirement("RH2005-003")]
    public async Task VerifyDiagnosticsAndCodeFix()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                public class Sample
                                {
                                    public string {|#0:publicField|};

                                    protected int protectedField;

                                    internal bool internalField;

                                    protected internal int protectedInternalField;

                                    private protected int privateProtectedField;

                                    private string privateField;

                                    string implicitPrivateField;

                                    public static readonly string Shared = string.Empty;

                                    public const int Constant = 1;
                                }

                                public struct SampleStruct
                                {
                                    public int PublicField;
                                }

                                public record SampleRecord
                                {
                                    public int {|#1:recordField|};
                                }

                                public record struct SampleRecordStruct
                                {
                                    public int PublicField;
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  public class Sample
                                  {
                                      private string publicField;

                                      protected int protectedField;

                                      internal bool internalField;

                                      protected internal int protectedInternalField;

                                      private protected int privateProtectedField;

                                      private string privateField;

                                      string implicitPrivateField;

                                      public static readonly string Shared = string.Empty;

                                      public const int Constant = 1;
                                  }

                                  public struct SampleStruct
                                  {
                                      public int PublicField;
                                  }

                                  public record SampleRecord
                                  {
                                      private int recordField;
                                  }

                                  public record struct SampleRecordStruct
                                  {
                                      public int PublicField;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that non-public fields in classes and record classes do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-001")]
    public async Task VerifyNonPublicFieldsDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class SampleClass
                                {
                                    internal int InternalField;
                                    protected int ProtectedField;
                                    protected internal int ProtectedInternalField;
                                    private protected int PrivateProtectedField;
                                    private int PrivateField;
                                }

                                internal record SampleRecord
                                {
                                    internal int InternalField;
                                    protected int ProtectedField;
                                    protected internal int ProtectedInternalField;
                                    private protected int PrivateProtectedField;
                                    private int PrivateField;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="public"/> modifier that is not the first modifier: <see
    /// langword="private"/> takes its place in front of the other modifiers, on the field's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixPutsPrivateInFrontOfStaticInTheFieldsColumn()
    {
        const string testData = """
                                public class C
                                {
                                    static public int {|#0:Counter|};
                                }
                                """;

        const string resultData = """
                                  public class C
                                  {
                                      private static int Counter;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of an attribute list on its own line in front of <c>static public</c>: <see
    /// langword="private"/> stands on the field's line and in its column, not behind the attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixPutsPrivateOnTheFieldsLineBehindAnAttributeListOnItsOwnLine()
    {
        await Verify(AttributedStaticPublicFieldSource,
                     AttributedStaticPublicFieldFixedSource,
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixPutsPrivateOnTheFieldsLineBehindAnAttributeListOnItsOwnLine"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixPutsPrivateOnTheFieldsLineBehindAnAttributeListOnItsOwnLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(AttributedStaticPublicFieldSource),
                     NormalizeToCarriageReturnLineFeed(AttributedStaticPublicFieldFixedSource),
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a comment line and a directive line between the attribute list and the
    /// field: both lines stay in front of the field, and <see langword="private"/> stands on the field's line and in
    /// its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheField()
    {
        await Verify(AttributedFieldWithCommentAndDirectiveSource,
                     AttributedFieldWithCommentAndDirectiveFixedSource,
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheField"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheFieldCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(AttributedFieldWithCommentAndDirectiveSource),
                     NormalizeToCarriageReturnLineFeed(AttributedFieldWithCommentAndDirectiveFixedSource),
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment line alone between the attribute list and the field stays in front of the field, and
    /// <see langword="private"/> stands on the field's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsCommentLineInFrontOfTheField()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete]
                                    // note
                                    static public int {|#0:Counter|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete]
                                      // note
                                      private static int Counter;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a directive line alone between the attribute list and the field stays in front of the field, and
    /// <see langword="private"/> stands on the field's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsDirectiveLineInFrontOfTheField()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete]
                                #pragma warning disable CS0414
                                    static public int {|#0:Counter|} = 1;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete]
                                  #pragma warning disable CS0414
                                      private static int Counter = 1;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list on the field's own line keeps <see langword="private"/> behind it on that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixPutsPrivateBehindAnAttributeListOnTheFieldsLine()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete] static public int {|#0:Counter|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete] private static int Counter;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that <see langword="private"/> replaces a <see langword="public"/> modifier that is the first token
    /// behind an attribute list on its own line, in that token's line and column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixReplacesPublicThatFollowsAnAttributeListOnItsOwnLine()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [NonSerialized]
                                    public int {|#0:Value|};
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [NonSerialized]
                                      private int Value;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a field read from an unrelated type: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldReadFromAnotherType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }

                                public class User
                                {
                                    public int Read(C c) => c.Value;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a field read from a derived type: the diagnostic is reported and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldReadFromADerivedType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }

                                public class D : C
                                {
                                    public int Read() => Value;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a field read from a type nested in a derived type is used outside its type: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldReadFromATypeNestedInADerivedType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }

                                public class D : C
                                {
                                    private class Reader
                                    {
                                        public int Read(D d) => d.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a field of a nested type that the enclosing type reads is used outside its containing type: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldOfANestedTypeReadByTheEnclosingType()
    {
        const string testData = """
                                public class Outer
                                {
                                    public int Read(Inner inner) => inner.Value;

                                    public class Inner
                                    {
                                        public int {|RH2005:Value|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that <c>nameof</c> of the field in another type is a reference outside the type: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldNamedByNameofInAnotherType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }

                                public class User
                                {
                                    public string Name() => nameof(C.Value);
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a field that another part of the partial type and a nested type read:
    /// both lie inside the containing type, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyFixIsOfferedForAFieldReadFromAnotherPartAndANestedType()
    {
        const string testData = """
                                public partial class C
                                {
                                    public int {|#0:Value|};

                                    private class Nested
                                    {
                                        public int Read(C c) => c.Value;
                                    }
                                }

                                public partial class C
                                {
                                    public int Twice() => Value * 2;
                                }
                                """;

        const string resultData = """
                                  public partial class C
                                  {
                                      private int Value;

                                      private class Nested
                                      {
                                          public int Read(C c) => c.Value;
                                      }
                                  }

                                  public partial class C
                                  {
                                      public int Twice() => Value * 2;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a field read from another part of the partial type in another document is read inside its type,
    /// so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyFixIsOfferedForAFieldReadFromAnotherPartInAnotherDocument()
    {
        const string testData = """
                                public partial class C
                                {
                                    public int {|#0:Value|};
                                }
                                """;

        const string resultData = """
                                  public partial class C
                                  {
                                      private int Value;
                                  }
                                  """;

        const string otherPart = """
                                 public partial class C
                                 {
                                     public int Twice() => Value * 2;
                                 }
                                 """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPart));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPart));
                     },
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a field read from a type in another document of the solution is used outside its type: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldReadFromAnotherDocument()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }
                                """;

        const string user = """
                            public class User
                            {
                                public int Read(C c) => c.Value;
                            }
                            """;

        await Verify(testData,
                     testData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", user));
                         test.FixedState.Sources.Add(("/0/Test1.cs", user));
                     });
    }

    /// <summary>
    /// Verifies the specification example of one declaration with two fields of which only the second is read outside
    /// the type: the diagnostic is reported and no code fix is offered, because the fix makes both fields private
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedWhenAnyFieldOfTheDeclarationIsReadOutsideTheType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:A|}, B;
                                }

                                public class User
                                {
                                    public int Read(C c) => c.B;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of <c>nameof</c> in an attribute of the containing type itself, which would
    /// still compile once the field is private: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldNamedInAnAttributeOfItsType()
    {
        const string testData = """
                                using System.Diagnostics;

                                [DebuggerDisplay(nameof(C.Value))]
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a <c>cref</c> to the field in the documentation comment of another type,
    /// which would still compile once the field is private: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldNamedInACrefOfAnotherType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }

                                /// <summary>See <see cref="C.Value"/>.</summary>
                                public class User
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Diagnose);
    }

    /// <summary>
    /// Verifies that a field read outside its type only in code that an <c>#if</c> branch excludes gets no fix, because
    /// the build that compiles the branch would no longer compile
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyNoFixIsOfferedForAFieldReadOutsideItsTypeInExcludedCode()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }

                                public class User
                                {
                                    public int Read(C c)
                                    {
                                #if NEVER
                                        return c.Value;
                                #else
                                        return 0;
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a field read inside its own type in code that an <c>#if</c> branch excludes keeps the fix, because
    /// the field stays accessible there once it is private
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyFixIsOfferedForAFieldReadInsideItsTypeInExcludedCode()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|#0:Value|};

                                    public int Read()
                                    {
                                #if NEVER
                                        return Value;
                                #else
                                        return 0;
                                #endif
                                    }
                                }
                                """;

        const string resultData = """
                                  public class C
                                  {
                                      private int Value;

                                      public int Read()
                                      {
                                  #if NEVER
                                          return Value;
                                  #else
                                          return 0;
                                  #endif
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All makes private only the fields that no code outside their type reads, and leaves the
    /// diagnostic of the field read outside its type without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsTheFieldReadOutsideItsType()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|#0:First|};

                                    public int {|RH2005:Used|};

                                    public int {|#1:Second|};
                                }

                                public class User
                                {
                                    public int Read(C c) => c.Used;
                                }
                                """;

        const string resultData = """
                                  public class C
                                  {
                                      private int First;

                                      public int {|RH2005:Used|};

                                      private int Second;
                                  }

                                  public class User
                                  {
                                      public int Read(C c) => c.Used;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     },
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a field read from another project of the solution is used outside its type: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-004")]
    public async Task VerifyNoFixIsOfferedForAFieldReadFromAnotherProject()
    {
        const string testData = """
                                public class C
                                {
                                    public int {|RH2005:Value|};
                                }
                                """;

        const string user = """
                            public class User
                            {
                                public int Read(C c) => c.Value;
                            }
                            """;

        await Verify(testData,
                     testData,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Consumer"].Sources.Add(("/Consumer/User.cs", user));
                         test.TestState.AdditionalProjects["Consumer"].AdditionalProjectReferences.Add("TestProject");
                     });
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="public"/> that is not the first modifier and starts its
    /// own line: <see langword="private"/> takes the place of <c>static</c>, and <see langword="int"/> takes the line
    /// and the column of the removed <see langword="public"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier()
    {
        await Verify(PublicOnItsOwnLineSource,
                     PublicOnItsOwnLineFixedSource,
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifier"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBehindAnotherModifierCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(PublicOnItsOwnLineSource),
                     NormalizeToCarriageReturnLineFeed(PublicOnItsOwnLineFixedSource),
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a comment line between <c>static</c> and a <see langword="public"/> that
    /// starts its own line: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfAPublicBehindAnotherModifier()
    {
        await VerifyNoCodeFixIsOffered(CommentLineInFrontOfPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfAPublicBehindAnotherModifier"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfAPublicBehindAnotherModifierCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineInFrontOfPublicSource));
    }

    /// <summary>
    /// Verifies the specification example of a directive line between <c>static</c> and a <see langword="public"/>
    /// that starts its own line: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfAPublicBehindAnotherModifier()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineInFrontOfPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineInFrontOfAPublicBehindAnotherModifier"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfAPublicBehindAnotherModifierCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineInFrontOfPublicSource));
    }

    /// <summary>
    /// Verifies the specification example of an <c>#if</c> block whose disabled text holds a modifier, between
    /// <c>static</c> and a <see langword="public"/> that starts its own line: the diagnostic is reported, and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForAnExcludedModifierInFrontOfAPublicBehindAnotherModifier()
    {
        const string testData = """
                                public class C
                                {
                                    static
                                #if NEVER
                                        readonly
                                #endif
                                        public int {|RH2005:Total|};
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="public"/> behind another modifier that ends its line: <see langword="private"/> takes the place of <c>static</c>, <c>static</c> ends the line, and <see langword="int"/> keeps its line and column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnotherModifier()
    {
        await Verify(PublicEndingItsLineSource, PublicEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnotherModifier"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnotherModifierCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(PublicEndingItsLineSource), NormalizeToCarriageReturnLineFeed(PublicEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="public"/> behind another modifier that starts and ends its own line: <see langword="int"/> stands on the line of the removed <see langword="public"/>, in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBehindAnotherModifier()
    {
        await Verify(PublicStartingAndEndingItsLineSource, PublicStartingAndEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBehindAnotherModifier"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBehindAnotherModifierCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(PublicStartingAndEndingItsLineSource), NormalizeToCarriageReturnLineFeed(PublicStartingAndEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> behind another modifier that starts and ends its own line hands its line and its column to the following token, also when that token stood in another column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixMovesTheTypeIntoTheColumnOfAPublicThatStartsAndEndsItsOwnLine()
    {
        await Verify(PublicStartingAndEndingItsLineInAnotherColumnSource, PublicStartingAndEndingItsLineInAnotherColumnFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixMovesTheTypeIntoTheColumnOfAPublicThatStartsAndEndsItsOwnLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixMovesTheTypeIntoTheColumnOfAPublicThatStartsAndEndsItsOwnLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(PublicStartingAndEndingItsLineInAnotherColumnSource), NormalizeToCarriageReturnLineFeed(PublicStartingAndEndingItsLineInAnotherColumnFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> between two modifiers that ends its line leaves the line break in place: <c>static</c> ends the line, and <c>volatile</c> keeps its line and column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBetweenTwoModifiers()
    {
        await Verify(MiddlePublicEndingItsLineSource, MiddlePublicEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBetweenTwoModifiers"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBetweenTwoModifiersCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(MiddlePublicEndingItsLineSource), NormalizeToCarriageReturnLineFeed(MiddlePublicEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> between two modifiers that starts and ends its own line hands its line and its column to <c>volatile</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBetweenTwoModifiers()
    {
        await Verify(MiddlePublicStartingAndEndingItsLineSource, MiddlePublicStartingAndEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBetweenTwoModifiers"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsAndEndsItsOwnLineBetweenTwoModifiersCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(MiddlePublicStartingAndEndingItsLineSource), NormalizeToCarriageReturnLineFeed(MiddlePublicStartingAndEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> between two modifiers that starts its own line hands its line and its column to <c>volatile</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBetweenTwoModifiers()
    {
        await Verify(MiddlePublicStartingItsLineSource, MiddlePublicStartingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBetweenTwoModifiers"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineOfAPublicThatStartsItsOwnLineBetweenTwoModifiersCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(MiddlePublicStartingItsLineSource), NormalizeToCarriageReturnLineFeed(MiddlePublicStartingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> behind two modifiers that ends its line leaves the line break in place: <c>volatile</c> ends the line, and <see langword="int"/> keeps its line and column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindTwoModifiers()
    {
        await Verify(LastPublicEndingItsLineSource, LastPublicEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindTwoModifiers"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindTwoModifiersCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(LastPublicEndingItsLineSource), NormalizeToCarriageReturnLineFeed(LastPublicEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> that ends the second line of the modifiers leaves both lines in place: <c>volatile</c> ends the second line, and <see langword="int"/> keeps its line and its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLinesOfModifiersInFrontOfAPublicThatEndsItsLine()
    {
        await Verify(LastPublicEndingASecondLineSource, LastPublicEndingASecondLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLinesOfModifiersInFrontOfAPublicThatEndsItsLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLinesOfModifiersInFrontOfAPublicThatEndsItsLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(LastPublicEndingASecondLineSource), NormalizeToCarriageReturnLineFeed(LastPublicEndingASecondLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> behind another modifier that ends its line, behind an attribute list on its own line, leaves the attribute list and the line break in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnAttributeList()
    {
        await Verify(AttributedPublicEndingItsLineSource, AttributedPublicEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnAttributeList"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixKeepsTheLineBreakBehindAPublicThatEndsItsLineBehindAnAttributeListCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(AttributedPublicEndingItsLineSource), NormalizeToCarriageReturnLineFeed(AttributedPublicEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> between two modifiers on the field's line is replaced by <see langword="private"/> in front of the other modifiers, one space apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixReplacesAPublicBetweenTwoModifiersOnOneLine()
    {
        await Verify(MiddlePublicOnOneLineSource, MiddlePublicOnOneLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixReplacesAPublicBetweenTwoModifiersOnOneLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixReplacesAPublicBetweenTwoModifiersOnOneLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(MiddlePublicOnOneLineSource), NormalizeToCarriageReturnLineFeed(MiddlePublicOnOneLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies that a <see langword="public"/> behind two modifiers on the field's line is replaced by <see langword="private"/> in front of the other modifiers, one space apart
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixReplacesAPublicBehindTwoModifiersOnOneLine()
    {
        await Verify(LastPublicOnOneLineSource, LastPublicOnOneLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixReplacesAPublicBehindTwoModifiersOnOneLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    public async Task VerifyFixReplacesAPublicBehindTwoModifiersOnOneLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(LastPublicOnOneLineSource), NormalizeToCarriageReturnLineFeed(LastPublicOnOneLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Characterizes a first <see langword="public"/> that ends its line: <see langword="private"/> takes its place, and the line break behind it stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsTheLineBreakBehindAFirstPublicThatEndsItsLine()
    {
        await Verify(FirstPublicEndingItsLineSource, FirstPublicEndingItsLineFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheLineBreakBehindAFirstPublicThatEndsItsLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsTheLineBreakBehindAFirstPublicThatEndsItsLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(FirstPublicEndingItsLineSource), NormalizeToCarriageReturnLineFeed(FirstPublicEndingItsLineFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Characterizes a first <see langword="public"/> with a comment behind it at the end of its line: <see langword="private"/> takes its place, and the comment and the line break behind it stay
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsTheCommentBehindAFirstPublicThatEndsItsLine()
    {
        await Verify(FirstPublicWithACommentBehindItSource, FirstPublicWithACommentBehindItFixedSource, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsTheCommentBehindAFirstPublicThatEndsItsLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsTheCommentBehindAFirstPublicThatEndsItsLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(FirstPublicWithACommentBehindItSource), NormalizeToCarriageReturnLineFeed(FirstPublicWithACommentBehindItFixedSource), Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a comment behind a <see langword="public"/> behind another modifier, at the end of its line: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAPublicAtTheEndOfItsLine()
    {
        await VerifyNoCodeFixIsOffered(CommentBehindPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentBehindAPublicAtTheEndOfItsLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAPublicAtTheEndOfItsLineCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentBehindPublicSource));
    }

    /// <summary>
    /// Verifies the specification example of a block comment between a <see langword="public"/> behind another modifier and the type, on one line: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheType()
    {
        await VerifyNoCodeFixIsOffered(BlockCommentBehindPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheType"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheTypeCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(BlockCommentBehindPublicSource));
    }

    /// <summary>
    /// Verifies the specification example of a comment line between a <see langword="public"/> behind another modifier and the type: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheType()
    {
        await VerifyNoCodeFixIsOffered(CommentLineBehindPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheType"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheTypeCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineBehindPublicSource));
    }

    /// <summary>
    /// Verifies the specification example of a directive line between a <see langword="public"/> behind another modifier and the type: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineBetweenAPublicAndTheType()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineBehindPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineBetweenAPublicAndTheType"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineBetweenAPublicAndTheTypeCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineBehindPublicSource));
    }

    /// <summary>
    /// Verifies the specification example of an <c>#if</c> block whose disabled text holds a modifier, between a <see langword="public"/> behind another modifier and the type: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForAnExcludedModifierBetweenAPublicAndTheType()
    {
        await VerifyNoCodeFixIsOffered(ExcludedModifierBehindPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForAnExcludedModifierBetweenAPublicAndTheType"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForAnExcludedModifierBetweenAPublicAndTheTypeCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExcludedModifierBehindPublicSource));
    }

    /// <summary>
    /// Verifies that a comment behind a <see langword="public"/> between two modifiers, at the end of its line, withholds the fix: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAPublicBetweenTwoModifiersAtTheEndOfItsLine()
    {
        await VerifyNoCodeFixIsOffered(CommentBehindMiddlePublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentBehindAPublicBetweenTwoModifiersAtTheEndOfItsLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAPublicBetweenTwoModifiersAtTheEndOfItsLineCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentBehindMiddlePublicSource));
    }

    /// <summary>
    /// Verifies that a block comment between a <see langword="public"/> and the next modifier, on one line, withholds the fix: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheNextModifier()
    {
        await VerifyNoCodeFixIsOffered(BlockCommentBehindMiddlePublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheNextModifier"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForABlockCommentBetweenAPublicAndTheNextModifierCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(BlockCommentBehindMiddlePublicSource));
    }

    /// <summary>
    /// Verifies that a comment line between a <see langword="public"/> and the next modifier withholds the fix: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheNextModifier()
    {
        await VerifyNoCodeFixIsOffered(CommentLineBehindMiddlePublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheNextModifier"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineBetweenAPublicAndTheNextModifierCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineBehindMiddlePublicSource));
    }

    /// <summary>
    /// Verifies that a comment behind a <see langword="public"/> that starts its own line behind another modifier, at the end of that line, withholds the fix: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAPublicThatStartsItsOwnLine()
    {
        await VerifyNoCodeFixIsOffered(CommentBehindPublicStartingItsLineSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentBehindAPublicThatStartsItsOwnLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAPublicThatStartsItsOwnLineCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentBehindPublicStartingItsLineSource));
    }

    /// <summary>
    /// Verifies that a comment line between the type and a <see langword="public"/> that starts its own line behind another modifier withholds the fix: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineBehindAPublicThatStartsItsOwnLine()
    {
        await VerifyNoCodeFixIsOffered(CommentLineBehindPublicStartingItsLineSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineBehindAPublicThatStartsItsOwnLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForACommentLineBehindAPublicThatStartsItsOwnLineCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineBehindPublicStartingItsLineSource));
    }

    /// <summary>
    /// Verifies that a directive line between the type and a <see langword="public"/> behind two modifiers withholds the fix: the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineBehindAPublicBehindTwoModifiers()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineBehindLastPublicSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineBehindAPublicBehindTwoModifiers"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-005")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineBehindAPublicBehindTwoModifiersCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineBehindLastPublicSource));
    }

    /// <summary>
    /// Verifies that Fix All replaces every later <see langword="public"/> that ends its line, keeping each line in
    /// place, and leaves the diagnostic of the field with a comment behind its <see langword="public"/> without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2005-003")]
    [Requirement("RH2005-005")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllKeepsTheLinesOfLaterPublicModifiersAndSkipsACommentBehindOne()
    {
        const string testData = """
                                public class C
                                {
                                    static public
                                        int {|#0:First|};

                                    static
                                        public
                                        int {|#1:Second|};

                                    static public // note
                                        int {|RH2005:Third|};

                                    static volatile public
                                        int {|#2:Fourth|};
                                }
                                """;

        const string resultData = """
                                  public class C
                                  {
                                      private static
                                          int First;

                                      private static
                                          int Second;

                                      static public // note
                                          int {|RH2005:Third|};

                                      private static volatile
                                          int Fourth;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     },
                     Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 3));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                public class Sample
                                {
                                    public string {|#0:first|};

                                    public int {|#1:second|};
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  public class Sample
                                  {
                                      private string first;

                                      private int second;
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}