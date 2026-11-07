using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
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

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 2));
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