using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// Characterizes the fix on a property with comments inside its declaration: the comments inside are dropped and
    /// only the trailing comment is kept. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixDropsCommentsInsidePropertyDeclaration()
    {
        const string testData = """
                                public class C
                                {
                                    private int {|#0:Value|} /* name */ { get; /* inner */ set; } // trailing

                                    public int Get() => Value;
                                }
                                """;

        const string resultData = """
                                  public class C
                                  {
                                      private int _value; // trailing

                                      public int Get() => _value;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Characterizes the fix on a property that code excluded by <c>#if</c> refers to: the fix is offered and the
    /// excluded reference keeps the property name. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixLeavesReferenceInExcludedCodeUnchanged()
    {
        const string testData = """
                                public class C
                                {
                                    private int {|#0:Value|} { get; set; }

                                    public int Get()
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
                                      private int _value;

                                      public int Get()
                                      {
                                  #if NEVER
                                          return Value;
                                  #else
                                          return 0;
                                  #endif
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Characterizes the fix on a property of a mutable struct type: the property becomes a field, so a mutating call
    /// that changed a copy now changes the field. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixReplacesPropertyOfMutableStructType()
    {
        const string testData = """
                                public struct Counter
                                {
                                    public int Count;

                                    public void Increment() => Count++;
                                }

                                public class C
                                {
                                    private Counter {|#0:Current|} { get; set; }

                                    public int Run()
                                    {
                                        Current.Increment();

                                        return Current.Count;
                                    }
                                }
                                """;

        const string resultData = """
                                  public struct Counter
                                  {
                                      public int Count;

                                      public void Increment() => Count++;
                                  }

                                  public class C
                                  {
                                      private Counter _current;

                                      public int Run()
                                      {
                                          _current.Increment();

                                          return _current.Count;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Characterizes the fix on a get-only property that already carries <see langword="readonly"/>: the field gets
    /// <see langword="readonly"/> twice. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixOnReadonlyPropertyWritesReadonlyTwice()
    {
        const string testData = """
                                public struct S
                                {
                                    private readonly int Value { get; }

                                    public S(int value)
                                    {
                                        Value = value;
                                    }

                                    public int Get() => Value;
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.Contains("private readonly readonly int _value;", fixedSource);
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