using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/> and <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer, RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Source of a property whose multi-line accessor list holds a comment line
    /// </summary>
    private const string MultiLineAccessorListWithCommentSource = """
                                                                  class C
                                                                  {
                                                                      private int {|RH2001:Value|}
                                                                      {
                                                                          // the getter
                                                                          get;
                                                                          set;
                                                                      }
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of a property whose accessor list holds a conditional directive around an accessor
    /// </summary>
    private const string AccessorListWithDirectiveSource = """
                                                           class C
                                                           {
                                                               private int {|RH2001:Value|}
                                                               {
                                                                   get;
                                                           #if DEBUG
                                                                   set;
                                                           #endif
                                                               }
                                                           }
                                                           """;

    /// <summary>
    /// Source of a property whose initializer is followed by a comment line in front of the semicolon
    /// </summary>
    private const string CommentLineInFrontOfTheSemicolonSource = """
                                                                  class C
                                                                  {
                                                                      private int {|RH2001:Value|} { get; set; } = 5
                                                                          // note
                                                                          ;
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of a property whose initializer is followed by a directive line in front of the semicolon
    /// </summary>
    private const string DirectiveLineInFrontOfTheSemicolonSource = """
                                                                    class C
                                                                    {
                                                                        private int {|RH2001:Value|} { get; set; } = 5
                                                                    #pragma warning disable CS0414
                                                                            ;
                                                                    }
                                                                    """;

    /// <summary>
    /// Source of a property with a comment line between its type and its name
    /// </summary>
    private const string CommentLineInFrontOfTheNameSource = """
                                                             class C
                                                             {
                                                                 private int
                                                                     // note
                                                                     {|RH2001:Value|} { get; set; }
                                                             }
                                                             """;

    /// <summary>
    /// Source of a property with a directive line between its type and its name
    /// </summary>
    private const string DirectiveLineInFrontOfTheNameSource = """
                                                               class C
                                                               {
                                                                   private int
                                                               #pragma warning disable CS0169
                                                                       {|RH2001:Value|} { get; set; }
                                                               }
                                                               """;

    /// <summary>
    /// Source of a property whose semicolon stands on a line below its initializer, with no comment or directive in
    /// front of it
    /// </summary>
    private const string SemicolonOnItsOwnLineSource = """
                                                       class C
                                                       {
                                                           private int {|RH2001:Value|} { get; set; } = 5
                                                               ;
                                                       }
                                                       """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="SemicolonOnItsOwnLineSource"/>
    /// </summary>
    private const string SemicolonOnItsOwnLineFixedSource = """
                                                            class C
                                                            {
                                                                private int _value = 5;
                                                            }
                                                            """;

    /// <summary>
    /// Source of a property whose name stands on a line below its type, with no comment or directive in front of it
    /// </summary>
    private const string NameOnItsOwnLineSource = """
                                                  class C
                                                  {
                                                      private int
                                                          {|RH2001:Value|} { get; set; }
                                                  }
                                                  """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="NameOnItsOwnLineSource"/>: the field laid out as the formatter
    /// lays it out
    /// </summary>
    private const string NameOnItsOwnLineFixedSource = """
                                                       class C
                                                       {
                                                           private int
                                                           _value;
                                                       }
                                                       """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the fix, which renames the property through the solution, is not offered when another document that holds a
    /// <c>partial</c> part of the type and a reference to the property holds a comment that another build compiles as code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task NoFixIsOfferedWhenAPartialPartInAnotherDocumentHoldsAHidingCarrier()
    {
        const string source = """
                              internal partial class C
                              {
                                  private int {|RH2001:Value|} { get; set; }
                              }
                              """;
        const string otherSource = """
                                   internal partial class C
                                   {
                                       public int M()
                                       {
                                   #if !X
                                           /*
                                   #else
                                           var s = @"
                                           */
                                           int c = 1; //";
                                   #endif
                                           return Value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));
                     });
    }

    /// <summary>
    /// Verifies that a private auto-property is reported and converted to a backing field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-001")]
    [Requirement("RH2001-003")]
    [Requirement("RH2001-004")]
    public async Task VerifyPrivateAutoPropertyDiagnostic()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private bool {|#0:PrivateAutoProperty|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          private bool _privateAutoProperty;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies that modifiers and initializers are preserved for supported property shapes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyPrivateStaticAutoPropertyCodeFixPreservesModifiersAndInitializer()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private static string {|#0:CacheKey|} { get; set; } = "cached";
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          private static string _cacheKey = "cached";
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies that get-only auto-properties are converted to readonly fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyPrivateGetOnlyAutoPropertyCodeFixCreatesReadonlyField()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private int {|#0:Count|} { get; } = 1;
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          private readonly int _count = 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies that attributed private auto-properties are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-002")]
    public async Task AttributedPrivateAutoPropertyIsNotReported()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        [Obsolete]
                                        private int Value { get; set; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the property is still converted to a field when renamed references precede the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Renamed references before the declaration shift its start offset. The fix must re-locate the declaration
    /// reliably (not via span arithmetic) so the field conversion is not silently dropped in favour of a rename only
    /// </remarks>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyPrivateAutoPropertyCodeFixConvertsFieldWhenReferencesPrecedeDeclaration()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        public int Sum()
                                        {
                                            return Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value;
                                        }

                                        private int {|#0:Value|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          public int Sum()
                                          {
                                              return _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value;
                                          }

                                          private int _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies that init-only auto-properties do not receive an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-005")]
    public async Task NoCodeFixForInitOnlyPrivateAutoProperty()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private int Value { get; init; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that explicit interface implementation auto-properties are not reported, since converting them to
    /// a field would break the interface implementation (CS0535)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-002")]
    public async Task ExplicitInterfaceImplementationAutoPropertyIsNotReported()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal interface IFoo
                                    {
                                        int X { get; set; }
                                    }

                                    internal class RH2001 : IFoo
                                    {
                                        int IFoo.X { get; set; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix refuses explicit interface implementation auto-properties even when a diagnostic
    /// is reported for one, independent of the analyzer-side exemption
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForExplicitInterfaceImplementationAutoProperty()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal interface IFoo
                                    {
                                        int X { get; set; }
                                    }

                                    internal class RH2001 : IFoo
                                    {
                                        int IFoo.X { get; set; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single(property => property.ExplicitInterfaceSpecifier != null)
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the code fix is not offered when the computed field name is already used by another member of
    /// the containing type, to avoid producing a duplicate declaration (CS0102)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-006")]
    public async Task NoCodeFixForPrivateAutoPropertyWithFieldNameCollision()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private readonly int _value;

                                        private int Value { get; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the generated field name is culture-invariant, so a property name starting with 'I' does not
    /// turn into a Turkish dotless i when the fix runs under the Turkish culture
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFieldNameConversionIsCultureInvariant()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private int Id { get; set; }
                                    }
                                }
                                """;

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;

        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

        try
        {
            var fixedSource = await ApplyCodeFixAsync(testData);

            Assert.Contains("_id", fixedSource);
            Assert.DoesNotContain("_ıd", fixedSource);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }
    }

    /// <summary>
    /// Verifying that explicitly and implicitly private auto-properties with get, set, and init accessors, with or
    /// without an initializer, static or instance, in a class, a struct, and an interface are reported on their name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-001")]
    [Requirement("RH2001-003")]
    public async Task VerifyPrivateAutoPropertiesOfEveryShapeAreReported()
    {
        const string testData = """
                                class C
                                {
                                    private int {|#0:Count|} { get; set; }

                                    int {|#1:Implicit|} { get; set; }

                                    private string {|#2:Name|} { get; }

                                    private int {|#3:Id|} { get; init; }

                                    private static int {|#4:Shared|} { get; set; } = 1;
                                }

                                struct S
                                {
                                    private int {|#5:Value|} { get; set; }
                                }

                                record R
                                {
                                    private int {|#6:Number|} { get; set; }
                                }

                                interface I
                                {
                                    private static int {|#7:Counter|} { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat, 8));
    }

    /// <summary>
    /// Verifying that properties that are not private, have no backing field, carry an attribute, or implement an
    /// interface explicitly are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-002")]
    public async Task VerifyOtherPropertiesAreNotReported()
    {
        const string testData = """
                                using System;

                                interface I
                                {
                                    int X { get; set; }
                                }

                                class C : I
                                {
                                    public int Visible { get; set; }

                                    protected int Derived { get; set; }

                                    internal int Assembly { get; set; }

                                    private protected int Narrow { get; set; }

                                    private int Computed => 1;

                                    private int Body
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    }

                                    [Obsolete]
                                    private int Attributed { get; set; }

                                    int I.X { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that the fix keeps type, modifiers, initializer, documentation comment, and trailing comment, makes a
    /// get-only property a readonly field, keeps a name that already is the field name, and renames every reference,
    /// including the one in <see langword="nameof"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-001")]
    public async Task VerifyFixReplacesPropertiesWithFieldsAndRenamesReferences()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// The count.
                                    /// </summary>
                                    private int {|#0:Count|} { get; set; } // counter

                                    private static string {|#1:CacheKey|} { get; } = "key";

                                    private string {|#2:Name|} { get; }

                                    private int {|#3:_done|} { get; set; }

                                    public C(string name)
                                    {
                                        Name = name;
                                    }

                                    public string Describe() => Name + Count + CacheKey + _done + nameof(Name);
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      /// <summary>
                                      /// The count.
                                      /// </summary>
                                      private int _count; // counter

                                      private static readonly string _cacheKey = "key";

                                      private readonly string _name;

                                      private int _done;

                                      public C(string name)
                                      {
                                          _name = name;
                                      }

                                      public string Describe() => _name + _count + _cacheKey + _done + nameof(_name);
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that the fix replaces a property of a struct and renames its references in another type of the
    /// solution's active code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixReplacesStructPropertyAndRenamesReferencesInNestedType()
    {
        const string testData = """
                                struct S
                                {
                                    private int {|#0:Value|} { get; set; }

                                    public int Get() => Value;

                                    class Reader
                                    {
                                        public int Read(S s) => s.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  struct S
                                  {
                                      private int _value;

                                      public int Get() => _value;

                                      class Reader
                                      {
                                          public int Read(S s) => s._value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifying that the field name is derived from the words of the property name, so separators and shouted words
    /// become <c>_camelCase</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("SHARED-CASING-006")]
    public async Task VerifyFixDerivesFieldNameFromWordsOfPropertyName()
    {
        const string testData = """
                                class C
                                {
                                    private int {|#0:URL|} { get; set; }

                                    private int {|#1:first_name|} { get; set; }

                                    private int {|#2:MAX_COUNT|} { get; set; }

                                    public int Sum() => URL + first_name + MAX_COUNT;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _url;

                                      private int _firstName;

                                      private int _maxCount;

                                      public int Sum() => _url + _firstName + _maxCount;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that no fix is offered for a property with an <see langword="init"/> accessor or an accessor that
    /// carries a modifier, while both are still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-005")]
    [Requirement("RH2001-001")]
    public async Task VerifyNoFixIsOfferedForInitAndModifiedAccessors()
    {
        const string testData = """
                                struct S
                                {
                                    private int {|RH2001:Id|} { get; init; }

                                    private int {|RH2001:Value|} { readonly get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifying that no fix is offered when another member of the containing type, a field or a method, already has
    /// the field name, while the property is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-006")]
    public async Task VerifyNoFixIsOfferedWhenFieldOrMethodHasTheFieldName()
    {
        const string testData = """
                                class C
                                {
                                    private readonly int _value;

                                    private int {|RH2001:Value|} { get; }

                                    private int _count() => 0;

                                    private int {|RH2001:Count|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Characterizes a property whose accessors use the <see langword="field"/> keyword with logic of their own: it is
    /// reported and no fix is offered. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyUsingFieldKeywordWithLogicIsReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get => field; set => field = value * 2; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Characterizes properties that use the <see langword="field"/> keyword without logic of their own, in an accessor
    /// body and in an expression body: both are reported and no fix is offered. This test pins today's behavior without
    /// stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertiesUsingFieldKeywordWithoutLogicAreReportedWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Other|} { get => field; }

                                    private int {|RH2001:Plain|} => field;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Characterizes properties with a <c>field:</c>-targeted attribute or an accessor attribute: both are reported and
    /// no fix is offered. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertiesWithFieldTargetedOrAccessorAttributeAreReportedWithoutFix()
    {
        const string testData = """
                                using System;
                                using System.Diagnostics.CodeAnalysis;

                                public class C
                                {
                                    [field: NonSerialized]
                                    private int {|RH2001:Cache|} { get; set; }

                                    private int {|RH2001:Value|} { [ExcludeFromCodeCoverage] get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Characterizes the field name derived from a property name that starts with a run of uppercase letters followed
    /// by lowercase ones: <c>HTTPClient</c> becomes <c>_hTTPClient</c>. The case belongs to an existing follow-up, so
    /// this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixKeepsUppercaseRunOfLeadingAcronymInFieldName()
    {
        const string testData = """
                                class C
                                {
                                    private int {|#0:HTTPClient|} { get; set; }

                                    public int Get() => HTTPClient;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _hTTPClient;

                                      public int Get() => _hTTPClient;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixReplacesPropertiesWithFieldsAndRenamesReferences"/> with the source normalized to
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixReplacesPropertiesWithFieldsAndRenamesReferencesCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// The count.
                                    /// </summary>
                                    private int {|#0:Count|} { get; set; } // counter

                                    private string {|#1:Name|} { get; }

                                    public C(string name)
                                    {
                                        Name = name;
                                    }

                                    public string Describe() => Name + Count;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      /// <summary>
                                      /// The count.
                                      /// </summary>
                                      private int _count; // counter

                                      private readonly string _name;

                                      public C(string name)
                                      {
                                          _name = name;
                                      }

                                      public string Describe() => _name + _count;
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(resultData),
                     Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="readonly"/> property of a struct: the field carries <see
    /// langword="readonly"/> once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsOneReadonlyModifierOfAReadonlyStructProperty()
    {
        const string testData = """
                                struct S
                                {
                                    private readonly int {|#0:Value|} { get; }

                                    public S(int value)
                                    {
                                        Value = value;
                                    }
                                }
                                """;

        const string resultData = """
                                  struct S
                                  {
                                      private readonly int _value;

                                      public S(int value)
                                      {
                                          _value = value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a nullable value type whose underlying type is declared <see
    /// langword="readonly"/> in the referenced framework: the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixReplacesNullableIntProperty()
    {
        const string testData = """
                                class C
                                {
                                    private int? {|#0:Cache|} { get; set; }

                                    int Read() => Cache.HasValue ? Cache.Value : 0;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int? _cache;

                                      int Read() => _cache.HasValue ? _cache.Value : 0;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies that properties of an enum, of a struct declared <see langword="readonly"/>, and of nullable forms of
    /// both are replaced by fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixReplacesPropertiesOfEnumAndReadonlyStructTypes()
    {
        const string testData = """
                                enum Color
                                {
                                    Red,
                                }

                                readonly struct Size
                                {
                                    public readonly int Width;
                                }

                                class C
                                {
                                    private Color {|#0:Tint|} { get; set; }

                                    private Size {|#1:Extent|} { get; set; }

                                    private Color? {|#2:LastTint|} { get; set; }

                                    private Size? {|#3:LastExtent|} { get; set; }

                                    int Read() => (int)Tint + Extent.Width + (LastTint.HasValue ? 1 : 0) + (LastExtent.HasValue ? 1 : 0);
                                }
                                """;

        const string resultData = """
                                  enum Color
                                  {
                                      Red,
                                  }

                                  readonly struct Size
                                  {
                                      public readonly int Width;
                                  }

                                  class C
                                  {
                                      private Color _tint;

                                      private Size _extent;

                                      private Color? _lastTint;

                                      private Size? _lastExtent;

                                      int Read() => (int)_tint + _extent.Width + (_lastTint.HasValue ? 1 : 0) + (_lastExtent.HasValue ? 1 : 0);
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that a property typed by a type parameter constrained to a class type is replaced by a field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixReplacesPropertyOfTypeParameterConstrainedToAClassType()
    {
        const string testData = """
                                class Base
                                {
                                }

                                class C<TBase>
                                    where TBase : Base
                                {
                                    private TBase {|#0:Item|} { get; set; }

                                    TBase Read() => Item;
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                  }

                                  class C<TBase>
                                      where TBase : Base
                                  {
                                      private TBase _item;

                                      TBase Read() => _item;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of code that <c>#if</c> excludes and that names the property: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyNoFixIsOfferedWhenExcludedCodeNamesTheProperty()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Read()
                                    {
                                #if NEVER
                                        return Value;
                                #else
                                        return 0;
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of code that <c>#if</c> excludes and that declares the new name: the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyNoFixIsOfferedWhenExcludedCodeDeclaresTheNewName()
    {
        const string testData = """
                                class C
                                {
                                #if NEVER
                                    private int _value;
                                #endif

                                    private int {|RH2001:Value|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that code in an excluded <c>#elif</c> branch that names the property withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyNoFixIsOfferedWhenAnExcludedElifBranchNamesTheProperty()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Read()
                                    {
                                #if NEVER
                                        return 1;
                                #elif ALSO_NEVER
                                        return Value;
                                #endif
                                        return 0;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that code in an excluded <c>#else</c> branch that names the property withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyNoFixIsOfferedWhenAnExcludedElseBranchNamesTheProperty()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Read()
                                    {
                                #if !NEVER
                                        return 0;
                                #else
                                        return Value;
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that code in an <c>#if false</c> block that names the property withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyNoFixIsOfferedWhenAnIfFalseBlockNamesTheProperty()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Read()
                                    {
                                #if false
                                        return Value;
                                #endif
                                        return 0;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a reference to the property in an active <c>#if</c> branch keeps the fix, which renames it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    public async Task VerifyFixRenamesReferenceInActiveConditionalBranch()
    {
        const string testData = """
                                class C
                                {
                                    private int {|#0:Value|} { get; set; }

                                    int Read()
                                    {
                                #if !NEVER
                                        return Value;
                                #endif
                                    }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value;

                                      int Read()
                                      {
                                  #if !NEVER
                                          return _value;
                                  #endif
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of comments behind the name and inside the accessor list: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForCommentsBehindTheNameAndInsideTheAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} /* name */ { get; /* inner */ set; } // trailing
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment behind the name alone withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheName()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} /* name */ { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment inside a one-line accessor list alone withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentInsideAOneLineAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; /* inner */ set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a comment between the accessor list and the initializer: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentBetweenTheAccessorListAndTheInitializer()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } /* init */ = 5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in a multi-line accessor list: the diagnostic is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorList()
    {
        await VerifyNoCodeFixIsOffered(MultiLineAccessorListWithCommentSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorList"/> with the source normalized to
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorListCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(MultiLineAccessorListWithCommentSource));
    }

    /// <summary>
    /// Verifies the specification example of a directive inside the accessor list: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveInsideTheAccessorList()
    {
        await VerifyNoCodeFixIsOffered(AccessorListWithDirectiveSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveInsideTheAccessorList"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveInsideTheAccessorListCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(AccessorListWithDirectiveSource));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive line between the name and the accessor list withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveBetweenTheNameAndTheAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|}
                                #pragma warning disable CS0414
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment line in front of the property's first token and a comment behind its last token keep
    /// the fix, and both comments stay
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsCommentsInFrontOfTheFirstTokenAndBehindTheLastToken()
    {
        const string testData = """
                                class C
                                {
                                    // counter
                                    private int {|#0:Count|} { get; set; } // trailing

                                    int Read() => Count;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      // counter
                                      private int _count; // trailing

                                      int Read() => _count;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a call that changes the value of a settable property of a struct not
    /// declared <see langword="readonly"/>: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    public async Task VerifyNoFixIsOfferedForAMutableStructPropertyChangedByACall()
    {
        const string testData = """
                                struct Counter
                                {
                                    private int _count;

                                    public void Increment()
                                    {
                                        _count++;
                                    }
                                }

                                class C
                                {
                                    private Counter {|RH2001:Current|} { get; set; }

                                    void M()
                                    {
                                        Current.Increment();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of properties of a struct not declared <see langword="readonly"/> and of its
    /// nullable form that no use changes: both are reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    public async Task VerifyNoFixIsOfferedForMutableStructPropertiesThatNoUseChanges()
    {
        const string testData = """
                                struct Point
                                {
                                    public int X;
                                }

                                class C
                                {
                                    private Point {|RH2001:Origin|} { get; set; }

                                    private Point? {|RH2001:Last|} { get; set; }

                                    int Read() => Origin.X;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a project that targets .NET Standard 2.0, whose <see langword="int"/> is
    /// not declared <see langword="readonly"/>: properties of <see langword="int"/> and <c>int?</c> are reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    public async Task VerifyNoFixIsOfferedForIntPropertiesWhenTargetingNetStandard20()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Count|} { get; set; }

                                    private int? {|RH2001:Cache|} { get; set; }

                                    void M() => Count++;
                                }
                                """;

        await Verify(testData, testData, test => test.ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard20);
    }

    /// <summary>
    /// Verifies the specification example of a get-only property of a struct not declared <see langword="readonly"/>
    /// whose value a constructor changes through a call: the diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    public async Task VerifyNoFixIsOfferedForAGetOnlyMutableStructPropertyChangedInAConstructor()
    {
        const string testData = """
                                struct Counter
                                {
                                    private int _count;

                                    public void Increment()
                                    {
                                        _count++;
                                    }
                                }

                                class C
                                {
                                    private Counter {|RH2001:Current|} { get; }

                                    C()
                                    {
                                        Current.Increment();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a get-only property of a struct not declared <see langword="readonly"/> that no use changes is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    public async Task VerifyNoFixIsOfferedForAGetOnlyMutableStructPropertyThatNoUseChanges()
    {
        const string testData = """
                                struct Point
                                {
                                    public int X;
                                }

                                class C
                                {
                                    private Point {|RH2001:Origin|} { get; }

                                    int Read() => Origin.X;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of properties typed by type parameters: only the one constrained to a
    /// reference type gets the fix, also under Fix All, and the other stays reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixIsOfferedOnlyForTheTypeParameterConstrainedToAReferenceType()
    {
        const string testData = """
                                class C<T, TRef>
                                    where TRef : class
                                {
                                    private T {|RH2001:Item|} { get; set; }

                                    private TRef {|RH2001:Reference|} { get; set; }

                                    TRef Read() => Item == null ? Reference : null;
                                }
                                """;

        const string resultData = """
                                  class C<T, TRef>
                                      where TRef : class
                                  {
                                      private T {|RH2001:Item|} { get; set; }

                                      private TRef _reference;

                                      TRef Read() => Item == null ? _reference : null;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a property typed by a type parameter constrained to a value type is reported and no code fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-008")]
    public async Task VerifyNoFixIsOfferedForATypeParameterConstrainedToAValueType()
    {
        const string testData = """
                                class C<T>
                                    where T : struct
                                {
                                    private T {|RH2001:Item|} { get; set; }

                                    T Read() => Item;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that Fix All replaces the properties that get the fix and leaves the property of a struct not declared
    /// <see langword="readonly"/> and the property with a comment inside it reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [Requirement("RH2001-008")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertiesWhoseFixIsWithheld()
    {
        const string testData = """
                                struct Point
                                {
                                    public int X;
                                }

                                class C
                                {
                                    private int {|RH2001:First|} { get; set; }

                                    private Point {|RH2001:Origin|} { get; set; }

                                    private int {|RH2001:Commented|} /* name */ { get; set; }

                                    private string {|RH2001:Second|} { get; set; }

                                    int Read() => First + Origin.X + Commented + Second.Length;
                                }
                                """;

        const string resultData = """
                                  struct Point
                                  {
                                      public int X;
                                  }

                                  class C
                                  {
                                      private int _first;

                                      private Point {|RH2001:Origin|} { get; set; }

                                      private int {|RH2001:Commented|} /* name */ { get; set; }

                                      private string _second;

                                      int Read() => _first + Origin.X + Commented + _second.Length;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a comment line between the initializer and the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon()
    {
        await VerifyNoCodeFixIsOffered(CommentLineInFrontOfTheSemicolonSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolonCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineInFrontOfTheSemicolonSource));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive line between the initializer and the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolon()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineInFrontOfTheSemicolonSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolon"/> with the source normalized to
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolonCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineInFrontOfTheSemicolonSource));
    }

    /// <summary>
    /// Verifies that an empty <c>#if</c> block between the initializer and the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForAConditionalBlockInFrontOfTheSemicolon()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 5
                                #if DEBUG
                                #endif
                                        ;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of the other side of
    /// <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon"/>: a semicolon on its own line without a
    /// comment or directive in front of it keeps the fix, and the field ends on the initializer's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective()
    {
        await Verify(SemicolonOnItsOwnLineSource, SemicolonOnItsOwnLineFixedSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirectiveCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(SemicolonOnItsOwnLineSource), NormalizeToCarriageReturnLineFeed(SemicolonOnItsOwnLineFixedSource));
    }

    /// <summary>
    /// Verifies the specification example of a comment line between the type and the name: the diagnostic is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheName()
    {
        await VerifyNoCodeFixIsOffered(CommentLineInFrontOfTheNameSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheName"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheNameCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineInFrontOfTheNameSource));
    }

    /// <summary>
    /// Verifies the specification example of a directive line between the type and the name: the diagnostic is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheName()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineInFrontOfTheNameSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheName"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheNameCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineInFrontOfTheNameSource));
    }

    /// <summary>
    /// Verifies the specification example of an <c>#if</c> block with disabled text between the type and the name:
    /// the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForAConditionalBlockInFrontOfTheName()
    {
        const string testData = """
                                class C
                                {
                                    private int
                                #if NEVER
                                        unused
                                #endif
                                        {|RH2001:Third|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the other side of <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheName"/>: a name on its own
    /// line without a comment or directive in front of it keeps the fix, and the fix lays out the field as the
    /// formatter does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-007")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective()
    {
        await Verify(NameOnItsOwnLineSource, NameOnItsOwnLineFixedSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-007")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirectiveCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(NameOnItsOwnLineSource), NormalizeToCarriageReturnLineFeed(NameOnItsOwnLineFixedSource));
    }

    /// <summary>
    /// Verifies that a comment line between the modifier and the type withholds the fix: the field the fix builds would
    /// hold the comment between its own tokens, which the formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [Requirement("RH2001-009")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheType()
    {
        const string testData = """
                                class C
                                {
                                    private
                                        // note
                                        int {|RH2001:Value|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment behind the type on the property's line withholds the fix: the field the fix builds would
    /// hold the comment between its type and its name, which the formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheTypeOnTheSameLine()
    {
        const string testData = """
                                class C
                                {
                                    private int /* note */ {|RH2001:Value|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of fields the fix would build with a comment the formatter refuses: both
    /// properties are reported, and no code fix is offered for either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedWhenTheBuiltFieldHoldsACommentTheFormatterRefuses()
    {
        const string testData = """
                                class C
                                {
                                    private int /* note */ {|RH2001:Value|} { get; set; }

                                    private int {|RH2001:Count|} { get; set; } = /* five */ 5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment behind the initializer's <c>=</c> withholds the fix: the field the fix builds would hold
    /// it in front of its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = /* five */ 5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment on a line of its own in front of the initializer's value withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } =
                                        // five
                                        5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind the initializer value's last token on that token's line withholds the fix: the
    /// field the fix builds would hold it between its value and its <c>;</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 5 /* five */;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment inside the initializer's value, between its first and its last token, does not withhold
    /// the fix, and stays inside the value of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyFixIsOfferedForACommentInsideTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 2 + /* three */ 3;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value = 2 + /* three */ 3;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies the specification example of get-only properties whose modifiers are not in the order of RH7105: the fix
    /// writes the modifiers in that order on one line, keeps the blank line, the comment, and the documentation comment
    /// once in front of the field, and keeps the written order of a property with a <c>set</c> accessor
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesReorderedModifiersOnOneLineBehindTheLeadingTrivia(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _x;

                                    // the shared value
                                    static private int {|RH2001:Value|} { get; }

                                    /// <summary>
                                    /// The name.
                                    /// </summary>
                                    static private string {|RH2001:Name|} { get; } = "v"; // name

                                    static private int {|RH2001:Count|} { get; set; }

                                    int Read() => _x + Value + Name.Length + Count;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _x;

                                      // the shared value
                                      private static readonly int _value;

                                      /// <summary>
                                      /// The name.
                                      /// </summary>
                                      private static readonly string _name = "v"; // name

                                      static private int _count;

                                      int Read() => _x + _value + _name.Length + _count;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a comment in front of the first modifier on its line: it stays once in
    /// front of the reordered modifiers
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsACommentInFrontOfTheFirstModifierOnceInFrontOfTheReorderedModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /* lead */ static private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      /* lead */ private static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes the reordered modifiers of a get-only property with one space between them
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesReorderedModifiersWithOneSpaceBetweenThem(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes three reordered modifiers and the added <c>readonly</c> in the order of RH7105 with
    /// one space between them
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesThreeReorderedModifiersInTheOrderOfRh7105(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static new private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static new readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind the property stays behind the field when the fix reorders the modifiers
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsATrailingCommentWhenItReordersTheModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static private int {|RH2001:Value|} { get; } // t
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static readonly int _value; // t
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies the change-document example of a blank line above a property whose modifiers the fix reorders: the
    /// blank line stays above the field, and the modifiers stay on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsTheBlankLineAboveAndTheReorderedModifiersOnOneLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _x;

                                    static private int {|RH2001:Value|} { get; }

                                    int Read() => _x + Value;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _x;

                                      private static readonly int _value;

                                      int Read() => _x + _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive on the line above a property whose modifiers the fix reorders stays once
    /// in front of the field, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsAPragmaDirectiveAboveOnceInFrontOfTheReorderedModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                #pragma warning disable CS0414
                                    static private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                  #pragma warning disable CS0414
                                      private static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#region</c> directive on the line above a property whose modifiers the fix reorders stays once
    /// in front of the field, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsARegionDirectiveAboveOnceInFrontOfTheReorderedModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    #region Properties

                                    static private int {|RH2001:Value|} { get; }

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      #region Properties

                                      private static readonly int _value;

                                      #endregion // Properties
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes <c>readonly</c> in front of a modifier that RH7105 puts behind it, also when the
    /// property's modifiers already are in order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesReadonlyInFrontOfUnsafe()
    {
        const string testData = """
                                class C
                                {
                                    private unsafe string {|RH2001:First|} { get; }

                                    unsafe private string {|RH2001:Second|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private readonly unsafe string _first;

                                      private readonly unsafe string _second;
                                  }
                                  """;

        await Verify(testData, resultData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies the specification example of comments among the modifiers or in front of the type, whether a reordered
    /// modifier would cross them or not: all four properties are reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForACommentAmongTheModifiersOrInFrontOfTheType(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static /* a */ private int {|RH2001:First|} { get; }

                                    static // b
                                    private int {|RH2001:Second|} { get; }

                                    private /* c */ static int {|RH2001:Third|} { get; }

                                    private static /* d */ int {|RH2001:Fourth|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a directive that a reordered modifier would cross: the property is reported,
    /// and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForADirectiveThatAReorderedModifierWouldCross(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static
                                #pragma warning disable CS0169
                                    private int {|RH2001:Value|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment among the modifiers that no modifier crosses withholds the fix as well: the field the fix
    /// builds would hold the comment, which the formatter refuses
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForACommentThatNoModifierCrosses(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private /* a */ static int {|RH2001:First|} { get; }

                                    private /* b */ new static int {|RH2001:Second|} { get; }

                                    static private /* c */ int {|RH2001:Third|} { get; }

                                    private static /* d */ int {|RH2001:Fourth|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment in front of the place of <c>readonly</c>, with a modifier that RH7105 puts behind
    /// <c>readonly</c>, withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfUnsafe()
    {
        const string testData = """
                                class C
                                {
                                    private /* c */ unsafe string {|RH2001:Value|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that an end-of-line comment between two modifiers that keep their order withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForAnEndOfLineCommentBetweenModifiersThatKeepTheirOrder(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private // a
                                    static int {|RH2001:Value|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive between two modifiers that keep their order keeps its place, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyFixKeepsADirectiveBetweenModifiersThatKeepTheirOrder(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private
                                #pragma warning disable CS0169
                                    static int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private
                                  #pragma warning disable CS0169
                                      static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that Fix All replaces the property whose modifiers the fix reorders and leaves the property with a
    /// comment that a reordered modifier would cross reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-009")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyWithACommentThatAReorderedModifierWouldCross()
    {
        const string testData = """
                                class C
                                {
                                    static /* a */ private int {|RH2001:First|} { get; }

                                    static private int {|RH2001:Second|} { get; }

                                    new static private int {|RH2001:Third|} { get; }

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      static /* a */ private int {|RH2001:First|} { get; }

                                      private static readonly int _second;

                                      private static new readonly int _third;

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a conditional group between the last modifier and the type withholds the fix, also when its branch
    /// holds only <c>static</c>: the property is reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between the modifiers and the type withholds the fix when it holds <c>extern</c>,
    /// which is not valid on a field: written as a field, a build with <c>DEBUG</c> would declare an extern field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalExternBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private
                              #if DEBUG
                                      extern
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between the modifiers and the type withholds the fix when it holds
    /// <c>readonly</c>: with the <c>readonly</c> the fix adds, a build with <c>DEBUG</c> would declare the modifier twice
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalReadonlyBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public struct C
                              {
                                  private
                              #if DEBUG
                                      readonly
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between the modifiers and the type withholds the fix when it holds a modifier
    /// RH7105 puts behind <c>readonly</c>, such as <c>unsafe</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalUnsafeBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private
                              #if DEBUG
                                      unsafe
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group whose <c>#else</c> and <c>#endif</c> stand between two modifiers withholds the fix,
    /// although every branch holds an accessibility modifier: the property is reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalGroupWithADirectiveBetweenTwoModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !DEBUG
                                  private
                              #else
                                  internal
                              #endif
                                  static int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the counterpart of a conditional group between the modifiers: a whole conditional group in front of the
    /// first modifier keeps the fix, and the group stays in front of the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-009")]
    public async Task VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  private int _debug;
                              #endif
                                  private static int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       private int _debug;
                                   #endif
                                       private static readonly int _value;

                                       public int Get()
                                       {
                                           return _value;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group holding <c>extern</c> in front of the type of a get-only property without other
    /// modifiers withholds the fix: written as a field, a build with <c>DEBUG</c> would declare an extern field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAGetOnlyPropertyWithoutModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  extern
                              #endif
                                  int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group holding <c>extern</c> in front of the type of a property with a setter and
    /// without other modifiers withholds the fix: written as a field, a build with <c>DEBUG</c> would declare an extern field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  extern
                              #endif
                                  int {|RH2001:Value|} { get; set; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group holding only a whole member in front of the type of a property with a setter
    /// and without modifiers withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroupInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  int {|RH2001:Value|} { get; set; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of <c>extern</c> in an excluded branch in front of the first modifier of a
    /// property of a class: written as a field, a build with <c>DEBUG</c> would declare an extern field, so the property is
    /// reported and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalExternInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of <c>readonly</c> in an excluded branch in front of the first modifier of a
    /// property of a struct: with the <c>readonly</c> the fix adds, a build with <c>DEBUG</c> would declare the modifier
    /// twice, so the property is reported and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalReadonlyInFrontOfTheFirstModifierInAStruct(string endOfLine)
    {
        const string source = """
                              struct S
                              {
                              #if DEBUG
                                  readonly
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that <c>static</c> in an excluded branch in front of the first modifier withholds the fix: the branch is
    /// no whole member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalStaticInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  static
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an excluded <c>#else</c> branch behind a group of a whole member: without
    /// <c>DEBUG</c> defined, the <c>#else</c> branch holds <c>extern</c>, so the property is reported and no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedElseBranchHoldingExternBehindAWholeMember(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if !DEBUG
                                  private int _trace;
                              #else
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an excluded branch in front of the member of the active <c>#else</c> branch,
    /// where only the <c>#endif</c> stands in the leading trivia of the property's first modifier: in a build with
    /// <c>DEBUG</c>, <c>extern</c> stands in front of the property, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedBranchInFrontOfTheMemberOfTheActiveElseBranch(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  extern
                              #else
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an excluded <c>#else</c> branch holding <c>extern</c> withholds the fix when the symbol of the active
    /// <c>#if</c> branch is defined in the document
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedElseBranchHoldingExternWhenTheSymbolIsDefined(string endOfLine)
    {
        const string source = """
                              #define DEBUG
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #else
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an excluded <c>#elif</c> branch holding <c>extern</c> withholds the fix, although the excluded
    /// <c>#if</c> branch holds a whole member: every excluded branch of the group is read
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedElifBranchHoldingExternBehindAnExcludedWholeMember(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #elif TRACE
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an attribute list in an excluded branch in front of the first modifier: the
    /// branch is no whole member, so the property is reported and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalAttributeListInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  [System.Obsolete]
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an excluded branch in front of the first modifier that holds text that is not code withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedBranchInFrontOfTheFirstModifierHoldingTextThatIsNotCode(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  this is not code
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a group in front of the first modifier that holds another conditional group withholds the fix, although
    /// the inner group holds a whole member: the group is nested
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                              #if TRACE
                                  private int _trace;
                              #endif
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group whose <c>#endif</c> stands behind a property without an initializer
    /// and whose branch holds another group: the group behind the property does not wrap whole members, so no code fix is
    /// offered, although the group in front of the first modifier holds a whole member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAGroupBehindThePropertyThatHoldsAnotherGroupInItsBranch(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if !RELEASE
                                  private int _first;
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member in front of the first modifier: the fix is offered
    /// and keeps the group in front of the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a group whose excluded branches all hold whole members keeps the fix, also when the branches are an
    /// excluded <c>#if</c> branch in front of an active <c>#else</c> member and only the <c>#endif</c> stands in front of
    /// the first modifier
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private long _trace;
                              #else
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       private long _trace;
                                   #else
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a group in front of the first modifier whose excluded branch holds only a comment keeps the fix: the
    /// branch is a sequence of no members
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupInFrontOfTheFirstModifierWhoseExcludedBranchHoldsOnlyAComment(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  // Traced
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       // Traced
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an initializer in an excluded branch behind a property without one: in a
    /// build with <c>X</c>, the field the fix would write would be followed by <c>= 5;</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindAPropertyWithoutOne(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if X
                                  = 5;
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an initializer in an excluded branch behind the last property of a type body withholds the fix: the
    /// token behind the property is the closing brace of the type body
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindTheLastPropertyInFrontOfTheClosingBrace(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int Read() => Value;

                                  private int {|RH2001:Value|} { get; }
                              #if X
                                  = 5;
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an initializer in an excluded branch behind a property without a modifier withholds the fix, as behind
    /// a property with a modifier
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindAPropertyWithoutAModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int {|RH2001:Value|} { get; }
                              #if X
                                  = 5;
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a group behind a property without an initializer whose excluded branch holds a modifier of the next
    /// member withholds the fix: the branch does not read as whole members
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierBehindAPropertyWithoutAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if DEBUG
                                  static
                              #endif
                                  private int _other;

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a group behind a property without an initializer that holds another conditional group withholds the
    /// fix, although the inner group holds a whole member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if DEBUG
                              #if TRACE
                                  private int _trace;
                              #endif
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member behind a property without an initializer: the fix
    /// is offered and keeps the group behind the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if DEBUG
                                  private int _trace;
                              #endif

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _value;
                                   #if DEBUG
                                       private int _trace;
                                   #endif

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group behind a property with an initializer: the property ends in
    /// <c>;</c>, so the excluded <c>static</c> belongs to the next member in every build, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; } = 5;
                              #if DEBUG
                                  static
                              #endif
                                  private int _other;

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _value = 5;
                                   #if DEBUG
                                       static
                                   #endif
                                       private int _other;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member in front of the first modifier inside the active
    /// branch of a group that wraps the file: the wrapping group has no directive in front of the first modifier or behind
    /// the property, so it is not read, and the fix is offered as in multi-targeting code
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile(string endOfLine)
    {
        const string source = """
                              #define NET
                              #if NET
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              #endif
                              """;

        const string fixedSource = """
                                   #define NET
                                   #if NET
                                   class C
                                   {
                                   #if DEBUG
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   #endif
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that Fix All replaces the property behind which a group of a whole member stands and leaves the property
    /// behind which an excluded initializer stands reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyBehindWhichAnExcludedInitializerStands()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Second|} { get; }
                                #if DEBUG
                                    private int _trace;
                                #endif

                                    private int {|RH2001:Third|} { get; }

                                    private int {|RH2001:First|} { get; }
                                #if X
                                    = 5;
                                #endif

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private readonly int _second;
                                  #if DEBUG
                                      private int _trace;
                                  #endif

                                      private readonly int _third;

                                      private int {|RH2001:First|} { get; }
                                  #if X
                                      = 5;
                                  #endif

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that Fix All replaces the property behind a group of a whole member and leaves the property behind a group
    /// holding <c>extern</c> reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    extern
                                #endif
                                    private int {|RH2001:First|} { get; }

                                #if DEBUG
                                    private int _trace;
                                #endif
                                    private int {|RH2001:Second|} { get; }

                                    private int {|RH2001:Third|} { get; }

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                  #if DEBUG
                                      extern
                                  #endif
                                      private int {|RH2001:First|} { get; }

                                  #if DEBUG
                                      private int _trace;
                                  #endif
                                      private readonly int _second;

                                      private readonly int _third;

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              namespace Reihitsu.Analyzer.Test.Design.Resources
                              {
                                  internal class RH2001
                                  {
                                      public int Sum()
                                      {
                                          return Alpha + Beta;
                                      }

                                      private int Alpha { get; set; }

                                      private int Beta { get; set; }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field that is read unqualified, which the rename would rewrite to
    /// <c>base._value</c>, while the property is still reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadUnqualified(string endOfLine)
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field read through <see langword="this"/> and through a receiver
    /// of the type, which would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadThroughThisAndThroughAReceiver()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        return Value + this._value + other._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field read only through <see langword="this"/>, whose text the
    /// rename would leave as written while it would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadOnlyThroughThis()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + this._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited field read only through a receiver of the type, whose text the
    /// rename would leave as written while it would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadOnlyThroughAReceiver()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        return Value + other._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited property named like the field, read through
    /// <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedPropertyIsReadThroughThis()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value { get; set; }
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + this._value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an inherited <see langword="protected internal"/> field read through a
    /// receiver inside a nested type, which would then read the new field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadThroughAReceiverInANestedType()
    {
        const string testData = """
                                class Base
                                {
                                    protected internal int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get() => Value;

                                    class Nested
                                    {
                                        int Read(Sample sample)
                                        {
                                            return sample._value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for an extension method named like the field of a delegate-typed property, whose
    /// call the rename would turn into a static call
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnExtensionMethodIsNamedLikeTheFieldOfADelegateTypedProperty()
    {
        const string testData = """
                                using System;

                                static class Extensions
                                {
                                    public static int _value(this Sample sample) => 1;
                                }

                                class Sample
                                {
                                    private Func<int> {|RH2001:Value|} { get; set; } = () => 2;

                                    int Get()
                                    {
                                        return Value() + this._value();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered next to an extension method named like the field of a property whose type is not
    /// invocable, whose call keeps its text and its binding
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAnExtensionMethodNamedLikeTheFieldWhoseCallKeepsItsBinding()
    {
        const string testData = """
                                static class Extensions
                                {
                                    public static int _value(this Sample sample) => 1;
                                }

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + this._value();
                                    }
                                }
                                """;

        const string resultData = """
                                  static class Extensions
                                  {
                                      public static int _value(this Sample sample) => 1;
                                  }

                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return _value + this._value();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for a statically imported member named like the field, which the rename would
    /// qualify with its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAStaticallyImportedMemberIsNamedLikeTheField()
    {
        const string testData = """
                                using static Holder;

                                static class Holder
                                {
                                    public static int _value = 1;
                                }

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for a static member of the enclosing type named like the field, which the rename
    /// would qualify with the enclosing type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAMemberOfTheEnclosingTypeIsNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private static int _value = 1;

                                    class Sample
                                    {
                                        private int {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return Value + _value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a simple lambda
    /// with an inferred type, returned from a block body, instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenASimpleLambdaParameterInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add()
                                    {
                                        return _value => Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of a simple lambda with an inferred type in an expression body
    /// named like the field, and qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToASimpleLambdaParameterInAnExpressionBodyNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add() => _value => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add() => _value => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a parenthesized
    /// lambda with an inferred type, returned from a block body, instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAParenthesizedLambdaParameterWithAnInferredTypeInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add()
                                    {
                                        return (_value) => Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of a parenthesized lambda with an inferred type in an expression
    /// body named like the field, and qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAParenthesizedLambdaParameterWithAnInferredTypeInAnExpressionBodyNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add() => (_value) => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add() => (_value) => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld for a static property when the renamed reference inside the block body of a static
    /// method would read the parameter of a simple lambda instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInTheBlockBodyOfAStaticMethodIsNamedLikeTheFieldOfAStaticProperty()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private static int {|RH2001:Value|} { get; set; }

                                    static Func<int, int> Add()
                                    {
                                        return _value => Value + _value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a static property next to a lambda parameter named like the field in the
    /// expression body of a static method, and qualifies the renamed reference with the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesTheStaticPropertyReferenceWithTheTypeNextToALambdaParameterInAnExpressionBodyNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private static int {|RH2001:Value|} { get; set; }

                                    static Func<int, int> Add() => _value => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private static int _value;

                                      static Func<int, int> Add() => _value => Sample._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a variable that
    /// <c>out var</c> declares in the same expression instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnOutVarVariableOfTheSameExpressionIsNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Parse(string input) => int.TryParse(input, out var _value) ? Value + _value : 0;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a variable that an <c>out</c>
    /// argument with an explicit type declares in the same expression instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnExplicitlyTypedOutVariableOfTheSameExpressionIsNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Parse(string input) => int.TryParse(input, out int _value) ? Value + _value : 0;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a primary-constructor parameter
    /// named like the field instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPrimaryConstructorParameterIsNamedLikeTheField()
    {
        const string testData = """
                                class Sample(int _value)
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get() => Value + _value;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reference whose binding the rename would change stands in another
    /// document, in another <c>partial</c> part of the type: an inherited field read through <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnInheritedFieldIsReadInAPartialPartInAnotherDocument()
    {
        const string source = """
                              class Base
                              {
                                  protected int _value;
                              }

                              partial class Sample : Base
                              {
                                  private int {|RH2001:Value|} { get; set; }

                                  int Get() => Value;
                              }
                              """;
        const string otherSource = """
                                   partial class Sample
                                   {
                                       int Read()
                                       {
                                           return this._value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));
                     });
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference that would read a lambda parameter instead of
    /// the field stands in a block body in another document, in another <c>partial</c> part of the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInAPartialPartInAnotherDocumentIsNamedLikeTheField()
    {
        const string source = """
                              partial class Sample
                              {
                                  private int {|RH2001:Value|} { get; set; }
                              }
                              """;
        const string otherSource = """
                                   using System;

                                   partial class Sample
                                   {
                                       Func<int, int> Add()
                                       {
                                           return _value => Value + _value;
                                       }
                                   }
                                   """;

        await Verify(source,
                     source,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherSource));
                     });
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a lambda with an
    /// inferred type that initializes a local declaration instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInALocalDeclarationIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        Func<int, int> add = _value => Value + _value;

                                        return add(1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would write the parameter of a lambda that is
    /// assigned in a statement instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInAnAssignmentStatementIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        Func<int, int> add;
                                        add = _value => Value = _value;

                                        return add(1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of a lambda that a
    /// constructor assigns instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenALambdaParameterInAConstructorIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    private readonly Func<int, int> _add;

                                    Sample()
                                    {
                                        _add = _value => Value + _value;
                                    }

                                    int Get() => _add(1);
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read the parameter of an explicitly
    /// typed lambda in a block body instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAnExplicitlyTypedLambdaParameterInABlockBodyIsNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        Func<int, int> add = (int _value) => Value + _value;

                                        return add(1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the renamed property reference would read a query range variable named like
    /// the field instead of the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAQueryRangeVariableIsNamedLikeTheField()
    {
        const string testData = """
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Sum() => (from _value in Enumerable.Range(0, 3) select Value + _value).Sum();
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a lambda parameter named like the field stands in an expression body in
    /// another document, in another <c>partial</c> part of the type, and qualifies the renamed property reference there
    /// with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALambdaParameterInAnExpressionBodyInAPartialPartInAnotherDocument()
    {
        const string source = """
                              partial class Sample
                              {
                                  private int {|RH2001:Value|} { get; set; }
                              }
                              """;
        const string fixedSource = """
                                   partial class Sample
                                   {
                                       private int _value;
                                   }
                                   """;
        const string otherSource = """
                                   using System;

                                   partial class Sample
                                   {
                                       Func<int, int> Add() => _value => Value + _value;
                                   }
                                   """;
        const string fixedOtherSource = """
                                        using System;

                                        partial class Sample
                                        {
                                            Func<int, int> Add() => _value => this._value + _value;
                                        }
                                        """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", fixedOtherSource));
                     });
    }

    /// <summary>
    /// Verifies that Fix All leaves a property whose fix is withheld because an inherited field is named like its field,
    /// and replaces another property of the same type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixAllLeavesAPropertyWhoseFieldNameIsBoundAndReplacesTheOthers()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    private int {|RH2001:Count|} { get; set; }

                                    private int {|RH2001:Total|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value + Count + Total;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int {|RH2001:Value|} { get; set; }

                                      private int _count;

                                      private int _total;

                                      int Get()
                                      {
                                          return Value + _value + _count + _total;
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that the fix is offered for a method parameter named like the field next to an inherited field of that
    /// name, and qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAParameterNamedLikeTheFieldAndAnInheritedField(string endOfLine)
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int _value)
                                    {
                                        return Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      int Get(int _value)
                                      {
                                          return this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is offered next to an inherited method named like the field, which the call still reaches
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAnInheritedMethodNamedLikeTheField()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value() => 1;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value();
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value() => 1;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return _value + _value();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered next to an inherited field named like the field that no reference names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAnInheritedFieldThatNoReferenceNames()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a local declared by a statement and named like the field, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALocalNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var _value = 1;

                                        return Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var _value = 1;

                                          return this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a method parameter named like the field in a type without a base type, and
    /// qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAMethodParameterNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int _value) => Value + _value;
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(int _value) => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of an explicitly typed lambda named like the field, and qualifies
    /// the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAnExplicitlyTypedLambdaParameterNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add() => (int _value) => Value + _value;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add() => (int _value) => this._value + _value;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of an anonymous method named like the field, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAnAnonymousMethodParameterNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int, int> Add()
                                    {
                                        return delegate (int _value) { return Value + _value; };
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int, int> Add()
                                      {
                                          return delegate (int _value) { return this._value + _value; };
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a parameter of a local function named like the field, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALocalFunctionParameterNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Local(1);

                                        int Local(int _value) => Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return Local(1);

                                          int Local(int _value) => this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for pattern variables named like the field, declared by a type pattern and by a
    /// <see langword="var"/> pattern, and qualifies the renamed property references with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAPatternVariableNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(object input)
                                    {
                                        if (input is int _value)
                                        {
                                            return Value + _value;
                                        }

                                        return 0;
                                    }

                                    int Read(int input) => input is var _value ? Value + _value : 0;
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(object input)
                                      {
                                          if (input is int _value)
                                          {
                                              return this._value + _value;
                                          }

                                          return 0;
                                      }

                                      int Read(int input) => input is var _value ? this._value + _value : 0;
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a variable that <c>out var</c> declares in a statement in front of the use, and
    /// qualifies the renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToAnOutVarVariableOfAnEarlierStatement()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Parse(string input)
                                    {
                                        int.TryParse(input, out var _value);

                                        return Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Parse(string input)
                                      {
                                          int.TryParse(input, out var _value);

                                          return this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for <see langword="foreach"/>, <see langword="catch"/>, query range, and
    /// deconstruction variables named like the field, and qualifies the renamed property references with
    /// <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToOtherInnerScopeVariablesNamedLikeTheField()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Sum(List<int> items)
                                    {
                                        var sum = 0;

                                        foreach (var _value in items)
                                        {
                                            sum += Value + _value;
                                        }

                                        return sum;
                                    }

                                    int Fail()
                                    {
                                        try
                                        {
                                            return 0;
                                        }
                                        catch (Exception _value)
                                        {
                                            return Value + _value.Message.Length;
                                        }
                                    }

                                    IEnumerable<int> Query(List<int> items) => from _value in items select Value + _value;

                                    int Split()
                                    {
                                        var (_value, other) = (1, 2);

                                        return Value + _value + other;
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Collections.Generic;
                                  using System.Linq;

                                  class Sample
                                  {
                                      private int _value;

                                      int Sum(List<int> items)
                                      {
                                          var sum = 0;

                                          foreach (var _value in items)
                                          {
                                              sum += this._value + _value;
                                          }

                                          return sum;
                                      }

                                      int Fail()
                                      {
                                          try
                                          {
                                              return 0;
                                          }
                                          catch (Exception _value)
                                          {
                                              return this._value + _value.Message.Length;
                                          }
                                      }

                                      IEnumerable<int> Query(List<int> items) => from _value in items select this._value + _value;

                                      int Split()
                                      {
                                          var (_value, other) = (1, 2);

                                          return this._value + _value + other;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for a local function named like the field, and qualifies the renamed property
    /// reference with <see langword="this"/> while the call still reaches the local function
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToALocalFunctionNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Value + _value();

                                        int _value() => 1;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return this._value + _value();

                                          int _value() => 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered for an outer local named like the field and captured by a lambda, and qualifies the
    /// renamed property reference with <see langword="this"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixQualifiesThePropertyReferenceNextToACapturedLocalNamedLikeTheField()
    {
        const string testData = """
                                using System;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Func<int> Get()
                                    {
                                        var _value = 1;

                                        return () => Value + _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  class Sample
                                  {
                                      private int _value;

                                      Func<int> Get()
                                      {
                                          var _value = 1;

                                          return () => this._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered next to a member named like the field that a nested type declares and uses, which
    /// keeps its text and its binding
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedNextToAMemberOfANestedTypeNamedLikeTheField()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get() => Value + new Inner().Read();

                                    class Inner
                                    {
                                        private int _value = 1;

                                        public int Read() => _value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get() => _value + new Inner().Read();

                                      class Inner
                                      {
                                          private int _value = 1;

                                          public int Read() => _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives an anonymous-object member its inferred
    /// name, which the rename would change so that a read of the member no longer compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var pair = new { Value };

                                        return pair.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property qualified with <c>this.</c> gives an
    /// anonymous-object member its inferred name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAThisQualifiedPropertyReferenceGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var x = new { this.Value };

                                        return x.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property through another instance of the type gives an
    /// anonymous-object member its inferred name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceThroughAnotherInstanceGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample other)
                                    {
                                        var x = new { other.Value };

                                        return x.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives a member of an anonymous object in a
    /// LINQ projection its inferred name, which a later projection reads
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAMemberOfAnAnonymousObjectInAQueryProjectionItsInferredName()
    {
        const string testData = """
                                using System.Linq;

                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int[] Get(int[] items)
                                    {
                                        return items.Select(item => new { Value, item }).Select(pair => pair.Value + pair.item).ToArray();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives a tuple element its inferred name, which
    /// the rename would change so that a read of the element no longer compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesATupleElementItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var pair = (Value, 1);

                                        return pair.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property gives an anonymous-object member its inferred
    /// name that <c>ToString()</c> prints, which the rename would change at run time although the code still compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceGivesAnAnonymousObjectMemberItsInferredNameThatToStringPrints()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    string Get()
                                    {
                                        return new { Value }.ToString();
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a reference to the property is the value of an anonymous-object member with an
    /// explicit name, which keeps its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedAnonymousObjectMember()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var x = new { Value = Value };

                                        return x.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var x = new { Value = _value };

                                          return x.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a reference to the property is the value of a tuple element with an explicit
    /// name, which keeps its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenAPropertyReferenceIsTheValueOfAnExplicitlyNamedTupleElement()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var t = (Value: Value, 1);

                                        return t.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var t = (Value: _value, 1);

                                          return t.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a reference to the property in an expression body gives an
    /// anonymous-object member its inferred name that <c>ToString()</c> prints
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenAPropertyReferenceInAnExpressionBodyGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    string Describe() => new { Value }.ToString();
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of an anonymous-object member and a tuple element with explicit names next to
    /// each other, which keep their names while the fix renames the references to the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenPropertyReferencesAreTheValuesOfAnExplicitlyNamedAnonymousObjectMemberAndTupleElement()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        var pair = new { Value = Value };
                                        var tuple = (Value: Value, 1);

                                        return pair.Value + tuple.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          var pair = new { Value = _value };
                                          var tuple = (Value: _value, 1);

                                          return pair.Value + tuple.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is the receiver of a member of another type named like the field,
    /// which keeps its binding while the receiver is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return Value._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer _value;

                                          int Get()
                                          {
                                              return _value._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property, qualified with <c>this.</c>, is the receiver of a member of
    /// another type named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenTheThisQualifiedPropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return this.Value._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer _value;

                                          int Get()
                                          {
                                              return this._value._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property with the null-forgiving operator is the receiver of a member of
    /// another type named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenTheNullForgivingPropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                #nullable enable
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer? {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return Value!._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  #nullable enable
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer? _value;

                                          int Get()
                                          {
                                              return _value!._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property, cast to another type, is the receiver of a member of that type
    /// named like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenTheCastPropertyIsTheReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private object {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            return ((Outer)Value)._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private object _value;

                                          int Get()
                                          {
                                              return ((Outer)_value)._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is the conditional receiver of a member of another type named
    /// like the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsTheConditionalReceiverOfAMemberNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int? Get()
                                        {
                                            return Value?._value;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  class Outer
                                  {
                                      private int _value;

                                      class Inner
                                      {
                                          private Outer _value;

                                          int? Get()
                                          {
                                              return _value?._value;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a <c>cref</c> in a documentation comment names an inherited member like
    /// the field, which would bind to the new field after the rename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheField()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    /// <summary>
                                    /// Reads <see cref="_value"/>.
                                    /// </summary>
                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a <c>cref</c> in a documentation comment names an inherited member like the
    /// field through its type, which keeps its binding
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenADocumentationCommentReferenceNamesAnInheritedMemberLikeTheFieldThroughItsType()
    {
        const string testData = """
                                class Base
                                {
                                    protected int _value;
                                }

                                class Sample : Base
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    /// <summary>
                                    /// Reads <see cref="Base._value"/>
                                    /// </summary>
                                    /// <returns>Value</returns>
                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Base
                                  {
                                      protected int _value;
                                  }

                                  class Sample : Base
                                  {
                                      private int _value;

                                      /// <summary>
                                      /// Reads <see cref="Base._value"/>
                                      /// </summary>
                                      /// <returns>Value</returns>
                                      public int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix renames a <c>cref</c> in a documentation comment that names the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixRenamesADocumentationCommentReferenceToTheProperty()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    /// <summary>
                                    /// Reads <see cref="Value"/>
                                    /// </summary>
                                    /// <returns>Value</returns>
                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      /// <summary>
                                      /// Reads <see cref="_value"/>
                                      /// </summary>
                                      /// <returns>Value</returns>
                                      public int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is read through another construction of its own generic
    /// type, a shape whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsReadThroughAnotherConstructionOfItsGenericType()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample<int> other)
                                    {
                                        return other.Value + Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is assigned in an object initializer of another construction
    /// of its own generic type, a shape whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAssignedInAnObjectInitializerOfAnotherConstructionOfItsGenericType()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    Sample<TOther> Cast<TOther>()
                                    {
                                        return new Sample<TOther> { Value = Value };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property, read through another construction of its own generic type,
    /// gives an anonymous-object member its inferred name that code reads
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyReadThroughAnotherConstructionOfItsGenericTypeGivesAnAnonymousObjectMemberItsInferredName()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample<int> other)
                                    {
                                        var pair = new { other.Value };

                                        return pair.Value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is the receiver of a member of another type named like the
    /// field and a local named like the field is in scope, so the rename has to qualify the receiver
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsTheReceiverOfAMemberNamedLikeTheFieldNextToALocalNamedLikeTheField()
    {
        const string testData = """
                                class Outer
                                {
                                    private int _value;

                                    class Inner
                                    {
                                        private Outer {|RH2001:Value|} { get; set; }

                                        int Get()
                                        {
                                            var _value = 1;

                                            return Value._value + _value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when a constructor deconstructs its parameters into the properties, since a
    /// deconstruction target gives no tuple element a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenAConstructorDeconstructsItsParametersIntoTheProperties()
    {
        const string testData = """
                                class Sample
                                {
                                    internal Sample(int value, string name) => (Value, Name) = (value, name);

                                    private int {|RH2001:Value|} { get; }

                                    private string {|RH2001:Name|} { get; }

                                    string Describe()
                                    {
                                        return Name + Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      internal Sample(int value, string name) => (_value, _name) = (value, name);

                                      private readonly int _value;

                                      private readonly string _name;

                                      string Describe()
                                      {
                                          return _name + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple converted to the unnamed tuple type a
    /// method returns, which drops the inferred element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleReturnedAsAnUnnamedTupleType()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    (int, int) Get()
                                    {
                                        return (Value, 1);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      (int, int) Get()
                                      {
                                          return (_value, 1);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple assigned to a local of an explicitly
    /// named tuple type, which replaces the inferred element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleAssignedToAnExplicitlyTypedTupleLocal()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        (int First, int Second) pair = (Value, 1);

                                        return pair.First;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          (int First, int Second) pair = (_value, 1);

                                          return pair.First;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple passed to a parameter of an unnamed
    /// tuple type, which drops the inferred element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATuplePassedAsAnUnnamedTupleArgument()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        return Use((Value, 1));
                                    }

                                    private static int Use((int, int) pair)
                                    {
                                        return pair.Item1;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          return Use((_value, 1));
                                      }

                                      private static int Use((int, int) pair)
                                      {
                                          return pair.Item1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is a target of a deconstruction assignment, which gives no tuple
    /// element a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsADeconstructionTarget()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    void Set((int, int) pair)
                                    {
                                        int other;

                                        (Value, other) = pair;
                                    }

                                    int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      void Set((int, int) pair)
                                      {
                                          int other;

                                          (_value, other) = pair;
                                      }

                                      int Get()
                                      {
                                          return _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of the tuple a deconstruction assignment reads,
    /// which gives no tuple element a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfTheSourceOfADeconstruction()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    void Deconstruct(out int value, out int count)
                                    {
                                        (value, count) = (Value, 1);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      void Deconstruct(out int value, out int count)
                                      {
                                          (value, count) = (_value, 1);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple a switch expression matches,
    /// a tuple element with an inferred name whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleASwitchExpressionMatches()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int input)
                                    {
                                        return (Value, input) switch
                                               {
                                                   (1, 2) => 3,
                                                   _ => 0
                                               };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple a switch statement matches,
    /// a tuple element with an inferred name whose fix this rule withholds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleASwitchStatementMatches()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(int input)
                                    {
                                        switch (Value, input)
                                        {
                                            case (1, 2):
                                                return 3;
                                            default:
                                                return 0;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple nested in another tuple, even when
    /// the outer tuple is converted to a type without element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleNestedInAnotherTuple()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        ((int, int), int) pair = ((Value, 1), 2);

                                        return pair.Item1.Item1;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple that is the operand of a cast,
    /// even when the cast's type has no element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsTheOperandOfACast()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        return ((int, int))(Value, 1);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the property is an element of a tuple that is a branch of a conditional
    /// expression, even when the conditional is returned as a type without element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyNoFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsABranchOfAConditionalExpression()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    (int, int) Get(bool flag)
                                    {
                                        return flag ? (Value, 1) : (Value, 2);
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple nested in a deconstruction target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleNestedInADeconstructionTarget()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(((int, int), int) source)
                                    {
                                        int other;
                                        int x;

                                        ((Value, other), x) = source;

                                        return Value + other + x;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get(((int, int), int) source)
                                      {
                                          int other;
                                          int x;

                                          ((_value, other), x) = source;

                                          return _value + other + x;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property of a generic type is read through a reference typed as the
    /// type's own construction
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsReadThroughTheOwnConstructionOfItsGenericType()
    {
        const string testData = """
                                class Sample<T>
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get(Sample<T> other)
                                    {
                                        return other.Value + Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample<T>
                                  {
                                      private int _value;

                                      int Get(Sample<T> other)
                                      {
                                          return other._value + _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple that is a branch of a conditional
    /// expression whose branches carry different names, so that the conditional's type has no name at that position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleThatIsABranchOfAConditionalExpressionWithDifferentlyNamedBranches()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    (int, int) Get(bool flag)
                                    {
                                        return flag ? (Value, 1) : (2, 3);
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      (int, int) Get(bool flag)
                                      {
                                          return flag ? (_value, 1) : (2, 3);
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple whose own conversion goes to
    /// <see cref="object"/>, which is not a tuple type and so keeps no element name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToObject()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        object pair = (Value, 1);

                                        return pair;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      object Get()
                                      {
                                          object pair = (_value, 1);

                                          return pair;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple whose own conversion goes to a
    /// nullable tuple type without element names, which is not a tuple type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToANullableUnnamedTupleType()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    object Get()
                                    {
                                        (int, int)? pair = (Value, 1);

                                        return pair;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      object Get()
                                      {
                                          (int, int)? pair = (_value, 1);

                                          return pair;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    /// <summary>
    /// Verifies that the fix is offered when the property is an element of a tuple whose own conversion goes to a
    /// nullable tuple type that names the element, which is not a tuple type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-011")]
    public async Task VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToANullableNamedTupleType()
    {
        const string testData = """
                                class Sample
                                {
                                    private int {|RH2001:Value|} { get; set; }

                                    int Get()
                                    {
                                        (int Value, int Other)? pair = (Value, 1);

                                        return pair.Value.Value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class Sample
                                  {
                                      private int _value;

                                      int Get()
                                      {
                                          (int Value, int Other)? pair = (_value, 1);

                                          return pair.Value.Value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes the provided text with the given line break
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line break</param>
    /// <returns>The text with every line break replaced by <paramref name="endOfLine"/></returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        public int Sum()
                                        {
                                            return Alpha + Beta;
                                        }

                                        private int {|#0:Alpha|} { get; set; }

                                        private int {|#1:Beta|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          public int Sum()
                                          {
                                              return _alpha + _beta;
                                          }

                                          private int _alpha;

                                          private int _beta;
                                      }
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}