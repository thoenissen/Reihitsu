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

    #endregion // Constants

    #region Tests

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

        await Verify(testData, resultData);
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

        await Verify(testData, resultData);
    }

    #endregion // Tests

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