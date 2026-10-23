using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7104PartialElementsMustDeclareAccessModifierAnalyzer"/> and <see cref="RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests : BatchCodeFixTestsBase<RH7104PartialElementsMustDeclareAccessModifierAnalyzer, RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying partial types without an access modifier are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PartialTypesWithoutAccessModifierAreReportedAndFixed()
    {
        const string testCode = """
                                partial class {|#0:TestClass|}
                                {
                                    public int Bar { get; set; }
                                }

                                partial struct {|#1:TestStruct|}
                                {
                                }

                                partial interface {|#2:ITest|}
                                {
                                }

                                partial record {|#3:TestRecord|};

                                partial record struct {|#4:TestRecordStruct|};
                                """;

        const string fixedCode = """
                                 internal partial class TestClass
                                 {
                                     public int Bar { get; set; }
                                 }

                                 internal partial struct TestStruct;

                                 internal partial interface ITest;

                                 internal partial record TestRecord;

                                 internal partial record struct TestRecordStruct;
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7104PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH7104MessageFormat, 5));
    }

    /// <summary>
    /// Verifying that partial parts inherit and declare the accessibility selected by another part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PartialTypesWithPublicAndCompoundAccessibilityAreFixed()
    {
        const string testCode = """
                                public partial class Sample
                                {
                                }

                                partial class {|#0:Sample|}
                                {
                                }

                                internal partial class Outer
                                {
                                    protected internal partial class Inner
                                    {
                                    }

                                    partial class {|#1:Inner|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public partial class Sample
                                 {
                                 }

                                 public partial class Sample;

                                 internal partial class Outer
                                 {
                                     protected internal partial class Inner
                                     {
                                     }

                                     protected internal partial class Inner;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7104PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH7104MessageFormat, 2));
    }

    /// <summary>
    /// Verifying the specification example: partial classes, structs, interfaces, records, and record structs without
    /// an accessibility modifier are reported on their names, at the top level and nested
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7104-001")]
    [Requirement("RH7104-003")]
    public async Task PartialTypesWithoutAccessibilityModifierAreReportedOnTheirNames()
    {
        const string source = """
                              partial class {|RH7104:C|}
                              {
                                  partial struct {|RH7104:S|}
                                  {
                                      int _value;
                                  }
                              }

                              partial interface {|RH7104:I|}
                              {
                                  void M();
                              }

                              partial record {|RH7104:R|}(int X);

                              partial record struct {|RH7104:P|}(int X);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: partial types with an accessibility modifier, <c>file</c> and compound
    /// accessibilities included, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7104-002")]
    public async Task PartialTypesWithAccessibilityModifierAreNotReported()
    {
        const string source = """
                              internal partial class C
                              {
                                  private protected partial class N
                                  {
                                      int _value;
                                  }
                              }

                              file partial class F
                              {
                                  int _value;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix inserts <c>internal</c> in front of the first modifier, after the
    /// documentation comment and the attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7104-004")]
    public async Task FixInsertsInternalAfterDocumentationCommentAndAttributes()
    {
        const string source = """
                              /// <summary>
                              /// Doc
                              /// </summary>
                              [System.Obsolete]
                              static partial class {|RH7104:C|}
                              {
                                  static void M()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>
                                   /// Doc
                                   /// </summary>
                                   [System.Obsolete]
                                   internal static partial class C
                                   {
                                       static void M()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the documentation comment of a declaration without attributes stays in front of the inserted
    /// keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7104-004")]
    public async Task FixKeepsTheDocumentationCommentInFrontOfTheInsertedKeyword()
    {
        const string source = """
                              /// <summary>
                              /// Doc
                              /// </summary>
                              static partial class {|RH7104:C|}
                              {
                                  static void M()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>
                                   /// Doc
                                   /// </summary>
                                   internal static partial class C
                                   {
                                       static void M()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: a nested type gets the default accessibility of its container,
    /// <c>public</c> in an interface and <c>private</c> in a struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7104-004")]
    public async Task FixInsertsTheDefaultAccessibilityOfTheContainer()
    {
        const string source = """
                              internal interface I
                              {
                                  partial class {|RH7104:N|}
                                  {
                                      int _value;
                                  }
                              }

                              internal struct S
                              {
                                  partial class {|RH7104:M|}
                                  {
                                      int _value;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal interface I
                                   {
                                       public partial class N
                                       {
                                           int _value;
                                       }
                                   }

                                   internal struct S
                                   {
                                       private partial class M
                                       {
                                           int _value;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a type nested in a class or a record gets <c>private</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7104-004")]
    public async Task FixInsertsPrivateForATypeNestedInAClassOrRecord()
    {
        const string source = """
                              internal class C
                              {
                                  partial class {|RH7104:N|}
                                  {
                                      int _value;
                                  }
                              }

                              internal record R
                              {
                                  partial struct {|RH7104:M|}
                                  {
                                      int _value;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class C
                                   {
                                       private partial class N
                                       {
                                           int _value;
                                       }
                                   }

                                   internal record R
                                   {
                                       private partial struct M
                                       {
                                           int _value;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                partial class {|#0:Outer|}
                                {
                                    partial class {|#1:Inner|}
                                    {
                                        public int Value { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal partial class Outer
                                 {
                                     private partial class Inner
                                     {
                                         public int Value { get; set; }
                                     }
                                 }
                                 """;

        // The nested declaration sits inside the declaration the outer fix rewrites, so both corrections are
        // computed against overlapping subtrees while their text changes stay confined to the inserted keyword
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7104PartialElementsMustDeclareAccessModifierAnalyzer.DiagnosticId, AnalyzerResources.RH7104MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}