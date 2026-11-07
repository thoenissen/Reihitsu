using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2004AccessModifierMustBeDeclaredAnalyzer"/> and <see cref="RH2004AccessModifierMustBeDeclaredCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH2004AccessModifierMustBeDeclaredAnalyzerTests : BatchCodeFixTestsBase<RH2004AccessModifierMustBeDeclaredAnalyzer, RH2004AccessModifierMustBeDeclaredCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Source of a method without a modifier behind an attribute list on its own line
    /// </summary>
    private const string AttributedMethodSource = """
                                                  using System;

                                                  public class C
                                                  {
                                                      [Obsolete]
                                                      void {|#0:M|}()
                                                      {
                                                      }
                                                  }
                                                  """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="AttributedMethodSource"/>
    /// </summary>
    private const string AttributedMethodFixedSource = """
                                                       using System;

                                                       public class C
                                                       {
                                                           [Obsolete]
                                                           private void M()
                                                           {
                                                           }
                                                       }
                                                       """;

    /// <summary>
    /// Source of a type behind an attribute list on its own line, and of a member with a comment line and a directive
    /// line between its attribute list and the declaration
    /// </summary>
    private const string AttributedTypeAndMemberSource = """
                                                         using System;

                                                         namespace N
                                                         {
                                                             [Serializable]
                                                             class {|#0:D|}
                                                             {
                                                                 [Obsolete]
                                                                 // note
                                                         #pragma warning disable CS0612
                                                                 static void {|#1:Run|}()
                                                                 {
                                                                 }
                                                             }
                                                         }
                                                         """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="AttributedTypeAndMemberSource"/>
    /// </summary>
    private const string AttributedTypeAndMemberFixedSource = """
                                                              using System;

                                                              namespace N
                                                              {
                                                                  [Serializable]
                                                                  internal class D
                                                                  {
                                                                      [Obsolete]
                                                                      // note
                                                              #pragma warning disable CS0612
                                                                      private static void Run()
                                                                      {
                                                                      }
                                                                  }
                                                              }
                                                              """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying that declarations without explicit accessibility trigger diagnostics and are fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("RH2004-003")]
    [Requirement("RH2004-004")]
    [Requirement("RH2004-005")]
    [Requirement("RH2004-006")]
    public async Task VerifyDiagnosticsAndCodeFix()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                class {|#0:MissingAccessClass|} : IDisposable
                                {
                                    string {|#1:field|};

                                    void {|#2:DoWork|}()
                                    {
                                    }

                                    class {|#3:NestedClass|}
                                    {
                                    }

                                    void IDisposable.Dispose()
                                    {
                                    }
                                }

                                internal interface IContract
                                {
                                    void InterfaceMethod();
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class MissingAccessClass : IDisposable
                                  {
                                      private string field;

                                      private void DoWork()
                                      {
                                      }

                                      private class NestedClass
                                      {
                                      }

                                      void IDisposable.Dispose()
                                      {
                                      }
                                  }

                                  internal interface IContract
                                  {
                                      void InterfaceMethod();
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that direct interface members are exempt while a nested type and its members are analyzed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-004")]
    [Requirement("RH2004-006")]
    public async Task VerifyOnlyDirectInterfaceMembersAreExempt()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal interface IContract
                                {
                                    void InterfaceMethod();

                                    class {|#0:NestedType|}
                                    {
                                        void {|#1:NestedMethod|}()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal interface IContract
                                  {
                                      void InterfaceMethod();

                                      public class NestedType
                                      {
                                          private void NestedMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData.ReplaceLineEndings(System.Environment.NewLine), Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a non-partial record struct without explicit accessibility is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("RH2004-006")]
    public async Task VerifyNonPartialRecordStructIsReportedAndFixed()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                record struct {|#0:Sample|};
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal record struct Sample;
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Verifying that a documented member keeps its XML documentation attached after the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyDocumentedMemberWithoutModifiersKeepsDocumentationAttached()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    /// <summary>
                                    /// Doc.
                                    /// </summary>
                                    void {|#0:DoWork|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      /// <summary>
                                      /// Doc.
                                      /// </summary>
                                      private void DoWork()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that a documented member with a non-accessibility modifier keeps its XML documentation attached after the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyDocumentedMemberWithExistingModifierKeepsDocumentationAttached()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    /// <summary>
                                    /// Doc.
                                    /// </summary>
                                    static void {|#0:DoWork|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      /// <summary>
                                      /// Doc.
                                      /// </summary>
                                      private static void DoWork()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 1));
    }

    /// <summary>
    /// Verifying that partial types with accessibility declared on another part are owned by RH7104
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-002")]
    [Requirement("SHARED-DIAG-004")]
    public async Task VerifyPartialTypeWithAccessibilityOnAnotherPartDoesNotTriggerDiagnostic()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                public partial class Sample
                                {
                                }

                                partial class Sample
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that nested partial types are owned by RH7104
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-002")]
    public async Task VerifyNestedPartialTypeDoesNotTriggerDiagnostic()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal partial class Outer
                                {
                                    protected internal partial class Inner
                                    {
                                    }
                                }

                                internal partial class Outer
                                {
                                    partial class Inner
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that all partial type kinds are owned by RH7104
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-002")]
    public async Task VerifyPartialTypesDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                partial class SampleClass
                                {
                                }

                                partial struct SampleStruct
                                {
                                }

                                partial interface ISample
                                {
                                }

                                partial record SampleRecord
                                {
                                }

                                partial record struct SampleRecordStruct
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that partial methods without explicit accessibility are allowed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-002")]
    public async Task VerifyPartialMethodsWithoutModifierDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal partial class Sample
                                {
                                    partial void OnChanged();
                                }

                                internal partial class Sample
                                {
                                    partial void OnChanged()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying static constructors without access modifiers do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-003")]
    public async Task VerifyStaticConstructorsWithoutModifierDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    static Sample()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying a file-scoped class does not trigger diagnostics, since <see langword="file"/> is the only
    /// accessibility a file-local type can declare and an additional modifier would not compile (CS9052)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    public async Task VerifyFileScopedClassDoesNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                file class Sample
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying a file-scoped struct does not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    public async Task VerifyFileScopedStructDoesNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                file struct Sample
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying a file-scoped interface does not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    public async Task VerifyFileScopedInterfaceDoesNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                file interface ISample
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that type, delegate, and member declarations without an accessibility modifier are reported, and that
    /// declarations with <see langword="file"/> or another accessibility modifier are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("RH2004-005")]
    public async Task VerifyDeclarationsWithoutAccessibilityModifierAreReported()
    {
        const string testData = """
                                using System;

                                delegate void {|#0:Handler|}();

                                enum {|#1:Mode|}
                                {
                                    On,
                                }

                                class {|#2:C|}
                                {
                                    int {|#3:_count|};

                                    event Action {|#4:Changed|};

                                    {|#5:C|}()
                                    {
                                    }

                                    int {|#6:Count|} { get; set; }

                                    int {|#7:this|}[int index] => index;

                                    static void {|#8:Run|}()
                                    {
                                    }

                                    struct {|#9:Nested|}
                                    {
                                    }
                                }

                                interface {|#10:I|}
                                {
                                }

                                record {|#11:R|};

                                record struct {|#12:P|};

                                file class Local
                                {
                                }

                                public class D
                                {
                                    private int _value;

                                    protected internal void M()
                                    {
                                    }

                                    internal event Action Raised
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 13));
    }

    /// <summary>
    /// Verifying that an operator and a conversion operator without an accessibility modifier are reported, on the
    /// operator token and on the <see langword="operator"/> keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("RH2004-005")]
    [Requirement("SHARED-DIAG-007")]
    public async Task VerifyOperatorsWithoutAccessibilityModifierAreReportedOnTheirToken()
    {
        const string testData = """
                                public struct S
                                {
                                    static S operator {|#0:+|}(S a, S b) => a;

                                    static explicit {|#1:operator|} int(S value) => 0;

                                    public static S operator -(S a, S b) => a;
                                }
                                """;

        await Verify(testData,
                     test => test.CompilerDiagnostics = CompilerDiagnostics.None,
                     Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that every part of a partial type and both parts of a partial method are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-002")]
    public async Task VerifyPartialTypeAndPartialMethodPartsAreNotReported()
    {
        const string testData = """
                                partial class C
                                {
                                    partial void OnChanged();
                                }

                                partial class C
                                {
                                    partial void OnChanged()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a static constructor and explicit interface implementations of a method, property, indexer, and
    /// event are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-003")]
    [Requirement("SHARED-DIAG-011")]
    public async Task VerifyStaticConstructorAndExplicitInterfaceImplementationsAreNotReported()
    {
        const string testData = """
                                using System;

                                public interface IContract
                                {
                                    int Value { get; }

                                    int this[int index] { get; }

                                    event Action Changed;
                                }

                                public class C : IDisposable, IContract
                                {
                                    static C()
                                    {
                                    }

                                    void IDisposable.Dispose()
                                    {
                                    }

                                    int IContract.Value => 0;

                                    int IContract.this[int index] => index;

                                    event Action IContract.Changed
                                    {
                                        add
                                        {
                                        }

                                        remove
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that members declared directly in an interface are not reported, while a type and a delegate nested
    /// in it and the members of that type are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-004")]
    public async Task VerifyInterfaceMembersAreNotReportedAndNestedTypesAre()
    {
        const string testData = """
                                using System;

                                public interface I
                                {
                                    void M();

                                    int P { get; }

                                    static int Create() => 0;

                                    event Action Changed;

                                    int this[int index] { get; }

                                    class {|#0:Nested|}
                                    {
                                        void {|#1:Run|}()
                                        {
                                        }
                                    }

                                    delegate void {|#2:Callback|}();

                                    enum {|#3:Kind|}
                                    {
                                        A,
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that a field or event field with several declarators is reported once on its first declarator and an
    /// indexer on its <see langword="this"/> keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-005")]
    [Requirement("SHARED-DIAG-006")]
    [Requirement("SHARED-DIAG-007")]
    public async Task VerifyDiagnosticIsOnFirstDeclaratorAndIndexerKeyword()
    {
        const string testData = """
                                public struct S
                                {
                                    int {|#0:a|}, b;

                                    event System.Action {|#1:First|}, Second;

                                    int {|#2:this|}[int index] => index;
                                }
                                """;

        await Verify(testData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that the fix adds <see langword="internal"/> in a namespace, <see langword="public"/> for a type in an
    /// interface, and <see langword="private"/> in a class and in an extension block as the first modifier, keeping the
    /// other modifiers and the documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    [Requirement("RH2004-001")]
    public async Task VerifyFixAddsDefaultAccessibilityOfEachContext()
    {
        const string testData = """
                                namespace N
                                {
                                    class {|#0:C|}
                                    {
                                        /// <summary>Runs.</summary>
                                        static void {|#1:Run|}()
                                        {
                                        }

                                        readonly int {|#2:_value|};

                                        class {|#3:Nested|}
                                        {
                                        }
                                    }

                                    interface {|#4:I|}
                                    {
                                        class {|#5:Nested|}
                                        {
                                        }
                                    }

                                    static class {|#6:Extensions|}
                                    {
                                        extension(int number)
                                        {
                                            int {|#7:Twice|}() => number * 2;
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace N
                                  {
                                      internal class C
                                      {
                                          /// <summary>Runs.</summary>
                                          private static void Run()
                                          {
                                          }

                                          private readonly int _value;

                                          private class Nested
                                          {
                                          }
                                      }

                                      internal interface I
                                      {
                                          public class Nested
                                          {
                                          }
                                      }

                                      internal static class Extensions
                                      {
                                          extension(int number)
                                          {
                                              private int Twice() => number * 2;
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 8));
    }

    /// <summary>
    /// Verifying that the fix adds <see langword="private"/> to members of a struct, a record, and a record struct, and
    /// <see langword="internal"/> to a type and a delegate at file level
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixAddsPrivateInStructAndRecordsAndInternalAtFileLevel()
    {
        const string testData = """
                                delegate void {|#0:Handler|}();

                                struct {|#1:S|}
                                {
                                    int {|#2:_a|};
                                }

                                record {|#3:R|}
                                {
                                    int {|#4:_b|};
                                }

                                record struct {|#5:P|}
                                {
                                    delegate void {|#6:Callback|}();
                                }
                                """;

        const string resultData = """
                                  internal delegate void Handler();

                                  internal struct S
                                  {
                                      private int _a;
                                  }

                                  internal record R
                                  {
                                      private int _b;
                                  }

                                  internal record struct P
                                  {
                                      private delegate void Callback();
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 7));
    }

    /// <summary>
    /// Characterizes the fix on an operator, which must be <see langword="public"/>: it adds <see
    /// langword="private"/>. This test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAddsPrivateToOperator()
    {
        const string testData = """
                                public struct S
                                {
                                    static S operator -(S a, S b) => a;
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.Contains("private static S operator -(S a, S b) => a;", fixedSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixAddsDefaultAccessibilityOfEachContext"/> with the source normalized to CRLF line
    /// endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixAddsDefaultAccessibilityOfEachContextCarriageReturnLineFeed()
    {
        const string testData = """
                                namespace N
                                {
                                    class {|#0:C|}
                                    {
                                        /// <summary>Runs.</summary>
                                        static void {|#1:Run|}()
                                        {
                                        }
                                    }

                                    interface {|#2:I|}
                                    {
                                        class {|#3:Nested|}
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace N
                                  {
                                      internal class C
                                      {
                                          /// <summary>Runs.</summary>
                                          private static void Run()
                                          {
                                          }
                                      }

                                      internal interface I
                                      {
                                          public class Nested
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(resultData),
                     Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 4));
    }

    /// <summary>
    /// Verifies the specification example of explicit interface implementations of an operator and a conversion
    /// operator: neither is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-003")]
    public async Task VerifyExplicitInterfaceOperatorAndConversionImplementationsAreNotReported()
    {
        const string testData = """
                                public interface IAdd<T>
                                    where T : IAdd<T>
                                {
                                    static abstract T operator +(T a, T b);

                                    static abstract explicit operator int(T value);
                                }

                                public struct N : IAdd<N>
                                {
                                    static N IAdd<N>.operator +(N a, N b) => a;

                                    static explicit IAdd<N>.operator int(N value) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that explicit interface implementations of a unary operator and a checked operator, with block bodies,
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-003")]
    public async Task VerifyExplicitInterfaceUnaryAndCheckedOperatorImplementationsAreNotReported()
    {
        const string testData = """
                                public interface INegate<T>
                                    where T : INegate<T>
                                {
                                    static abstract T operator -(T value);

                                    static abstract T operator +(T a, T b);

                                    static abstract T operator checked +(T a, T b);
                                }

                                public struct N : INegate<N>
                                {
                                    static N INegate<N>.operator -(N value)
                                    {
                                        return value;
                                    }

                                    static N INegate<N>.operator +(N a, N b)
                                    {
                                        return a;
                                    }

                                    static N INegate<N>.operator checked +(N a, N b)
                                    {
                                        return a;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit interface implementation of an implicit conversion operator with a block body is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-003")]
    public async Task VerifyExplicitInterfaceImplicitConversionImplementationIsNotReported()
    {
        const string testData = """
                                public interface IConvert<T>
                                    where T : IConvert<T>
                                {
                                    static abstract implicit operator long(T value);
                                }

                                public struct N : IConvert<N>
                                {
                                    static implicit IConvert<N>.operator long(N value)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of a partial property whose implementing part stands first: it is reported
    /// once, on its defining part, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialPropertyIsReportedOnceOnItsDefiningPartWithoutFix()
    {
        const string testData = """
                                public partial class P
                                {
                                    partial int Prop { get => 1; set { } }

                                    partial int {|RH2004:Prop|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a partial indexer: it is reported once, on its defining part, and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialIndexerIsReportedOnceOnItsDefiningPartWithoutFix()
    {
        const string testData = """
                                public partial class P
                                {
                                    partial int {|RH2004:this|}[int index] { get; }

                                    partial int this[int index] { get => index; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a partial event: it is reported once, on its defining part, and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialEventIsReportedOnceOnItsDefiningPartWithoutFix()
    {
        const string testData = """
                                using System;

                                public partial class P
                                {
                                    partial event Action {|RH2004:Changed|};

                                    partial event Action Changed { add { } remove { } }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a partial constructor: it is reported once, on its defining part, and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-001")]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialConstructorIsReportedOnceOnItsDefiningPartWithoutFix()
    {
        const string testData = """
                                public partial class P
                                {
                                    partial {|RH2004:P|}(int x);

                                    partial P(int x)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a partial property whose two parts lie in two documents is reported once, on its defining part,
    /// when the document of the defining part comes first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialPropertyInTwoDocumentsIsReportedOnTheDefiningPartInTheFirstDocument()
    {
        const string definingPart = """
                                    public partial class P
                                    {
                                        partial int {|RH2004:Prop|} { get; set; }
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial int Prop { get => 1; set { } }
                                        }
                                        """;

        await Verify(definingPart, test => test.TestState.Sources.Add(("/0/Test1.cs", implementingPart)));
    }

    /// <summary>
    /// Verifies that a partial property whose two parts lie in two documents is reported once, on its defining part,
    /// when the document of the implementing part comes first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialPropertyInTwoDocumentsIsReportedOnTheDefiningPartInTheSecondDocument()
    {
        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial int Prop { get => 1; set { } }
                                        }
                                        """;

        const string definingPart = """
                                    public partial class P
                                    {
                                        partial int {|RH2004:Prop|} { get; set; }
                                    }
                                    """;

        await Verify(implementingPart, test => test.TestState.Sources.Add(("/0/Test1.cs", definingPart)));
    }

    /// <summary>
    /// Verifies that a partial property whose defining part lies in generated code is reported once, on its
    /// implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialPropertyWithGeneratedDefiningPartIsReportedOnTheImplementingPart()
    {
        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial int {|RH2004:Prop|} { get => 1; set { } }
                                        }
                                        """;

        const string definingPart = """
                                    public partial class P
                                    {
                                        partial int Prop { get; set; }
                                    }
                                    """;

        await Verify(implementingPart, test => test.TestState.Sources.Add(("/0/P.g.cs", definingPart)));
    }

    /// <summary>
    /// Verifies that a partial property whose implementing part lies in generated code is reported once, on its
    /// defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialPropertyWithGeneratedImplementingPartIsReportedOnTheDefiningPart()
    {
        const string definingPart = """
                                    public partial class P
                                    {
                                        partial int {|RH2004:Prop|} { get; set; }
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial int Prop { get => 1; set { } }
                                        }
                                        """;

        await Verify(definingPart, test => test.TestState.Sources.Add(("/0/P.g.cs", implementingPart)));
    }

    /// <summary>
    /// Verifies that a partial property both of whose parts lie in generated code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialPropertyWithBothPartsGeneratedIsNotReported()
    {
        const string definingPart = """
                                    public partial class P
                                    {
                                        partial int Prop { get; set; }
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial int Prop { get => 1; set { } }
                                        }
                                        """;

        await Verify(string.Empty,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Defining.g.cs", definingPart));
                         test.TestState.Sources.Add(("/0/Implementing.g.cs", implementingPart));
                     });
    }

    /// <summary>
    /// Verifies the specification example of an attribute list on its own line in front of a method without a
    /// modifier: the added modifier stands on the method's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixPutsModifierOnTheMembersLineBehindAnAttributeListOnItsOwnLine()
    {
        await Verify(AttributedMethodSource,
                     AttributedMethodFixedSource,
                     Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixPutsModifierOnTheMembersLineBehindAnAttributeListOnItsOwnLine"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixPutsModifierOnTheMembersLineBehindAnAttributeListOnItsOwnLineCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(AttributedMethodSource),
                     NormalizeToCarriageReturnLineFeed(AttributedMethodFixedSource),
                     Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a type behind an attribute list on its own line, and of a member whose
    /// first token is another modifier, with a comment line and a directive line between its attribute list and the
    /// declaration: the lines stay in front of the declarations, and the added modifiers stand on the declarations'
    /// lines and in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheDeclaration()
    {
        await Verify(AttributedTypeAndMemberSource,
                     AttributedTypeAndMemberFixedSource,
                     Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 2));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheDeclaration"/> with the source normalized
    /// to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixKeepsCommentAndDirectiveLinesInFrontOfTheDeclarationCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(AttributedTypeAndMemberSource),
                     NormalizeToCarriageReturnLineFeed(AttributedTypeAndMemberFixedSource),
                     Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a comment line alone between the attribute list and a method without a modifier stays in front of
    /// the method, and the added modifier stands on the method's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixKeepsCommentLineInFrontOfTheMember()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete]
                                    // note
                                    void {|#0:M|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete]
                                      // note
                                      private void M()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Verifies that a directive line alone between the attribute list and a method without a modifier stays in front
    /// of the method, and the added modifier stands on the method's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixKeepsDirectiveLineInFrontOfTheMember()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete]
                                #pragma warning disable CS0612
                                    void {|#0:M|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete]
                                  #pragma warning disable CS0612
                                      private void M()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Verifies that the added modifier of a member whose first token is another modifier, behind an attribute list on
    /// its own line, stands on the member's line and in its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixPutsModifierInFrontOfStaticBehindAnAttributeListOnItsOwnLine()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete]
                                    static void {|#0:N|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete]
                                      private static void N()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Verifies that the added modifier of a field and a property behind attribute lists on their own lines stands on
    /// the declarations' lines and in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixPutsModifierOnTheLinesOfAttributedFieldAndProperty()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [NonSerialized]
                                    int {|#0:_value|};

                                    [Obsolete]
                                    int {|#1:Value|} { get; set; }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [NonSerialized]
                                      private int _value;

                                      [Obsolete]
                                      private int Value { get; set; }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that an attribute list on the member's own line keeps the added modifier behind it on that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2004-006")]
    public async Task VerifyFixPutsModifierBehindAnAttributeListOnTheMembersLine()
    {
        const string testData = """
                                using System;

                                public class C
                                {
                                    [Obsolete] void {|#0:M|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  public class C
                                  {
                                      [Obsolete] private void M()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat));
    }

    /// <summary>
    /// Verifies that a partial constructor whose implementing part lies in generated code is reported once, on its
    /// defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialConstructorWithGeneratedImplementingPartIsReportedOnTheDefiningPart()
    {
        const string userPart = """
                                public partial class P
                                {
                                    partial {|RH2004:P|}(int x);
                                }
                                """;

        const string generatedPart = """
                                     public partial class P
                                     {
                                         partial P(int x)
                                         {
                                         }
                                     }
                                     """;

        await Verify(userPart, test => test.TestState.Sources.Add(("/0/P.g.cs", generatedPart)));
    }

    /// <summary>
    /// Verifies that a partial constructor whose defining part lies in generated code is reported once, on its
    /// implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialConstructorWithGeneratedDefiningPartIsReportedOnTheImplementingPart()
    {
        const string userPart = """
                                public partial class P
                                {
                                    partial {|RH2004:P|}(int x)
                                    {
                                    }
                                }
                                """;

        const string generatedPart = """
                                     public partial class P
                                     {
                                         partial P(int x);
                                     }
                                     """;

        await Verify(userPart, test => test.TestState.Sources.Add(("/0/P.g.cs", generatedPart)));
    }

    /// <summary>
    /// Verifies that a partial indexer whose implementing part lies in generated code is reported once, on its defining
    /// part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialIndexerWithGeneratedImplementingPartIsReportedOnTheDefiningPart()
    {
        const string userPart = """
                                public partial class P
                                {
                                    partial int {|RH2004:this|}[int index] { get; }
                                }
                                """;

        const string generatedPart = """
                                     public partial class P
                                     {
                                         partial int this[int index] { get => index; }
                                     }
                                     """;

        await Verify(userPart, test => test.TestState.Sources.Add(("/0/P.g.cs", generatedPart)));
    }

    /// <summary>
    /// Verifies that a partial indexer whose defining part lies in generated code is reported once, on its implementing
    /// part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialIndexerWithGeneratedDefiningPartIsReportedOnTheImplementingPart()
    {
        const string userPart = """
                                public partial class P
                                {
                                    partial int {|RH2004:this|}[int index] { get => index; }
                                }
                                """;

        const string generatedPart = """
                                     public partial class P
                                     {
                                         partial int this[int index] { get; }
                                     }
                                     """;

        await Verify(userPart, test => test.TestState.Sources.Add(("/0/P.g.cs", generatedPart)));
    }

    /// <summary>
    /// Verifies that a partial event whose implementing part lies in generated code is reported once, on its defining
    /// part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialEventWithGeneratedImplementingPartIsReportedOnTheDefiningPart()
    {
        const string userPart = """
                                using System;

                                public partial class P
                                {
                                    partial event Action {|RH2004:Changed|};
                                }
                                """;

        const string generatedPart = """
                                     using System;

                                     public partial class P
                                     {
                                         partial event Action Changed { add { } remove { } }
                                     }
                                     """;

        await Verify(userPart, test => test.TestState.Sources.Add(("/0/P.g.cs", generatedPart)));
    }

    /// <summary>
    /// Verifies that a partial event whose defining part lies in generated code is reported once, on its implementing
    /// part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialEventWithGeneratedDefiningPartIsReportedOnTheImplementingPart()
    {
        const string userPart = """
                                using System;

                                public partial class P
                                {
                                    partial event Action {|RH2004:Changed|} { add { } remove { } }
                                }
                                """;

        const string generatedPart = """
                                     using System;

                                     public partial class P
                                     {
                                         partial event Action Changed;
                                     }
                                     """;

        await Verify(userPart, test => test.TestState.Sources.Add(("/0/P.g.cs", generatedPart)));
    }

    /// <summary>
    /// Verifies that a partial constructor whose two parts lie in two documents is reported once, on its defining
    /// part, when the document of the defining part comes first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialConstructorInTwoDocumentsIsReportedOnTheDefiningPartInTheFirstDocument()
    {
        const string definingPart = """
                                    public partial class P
                                    {
                                        partial {|RH2004:P|}(int x);
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial P(int x)
                                            {
                                            }
                                        }
                                        """;

        await Verify(definingPart, test => test.TestState.Sources.Add(("/0/Test1.cs", implementingPart)));
    }

    /// <summary>
    /// Verifies that a partial constructor whose two parts lie in two documents is reported once, on its defining
    /// part, when the document of the implementing part comes first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialConstructorInTwoDocumentsIsReportedOnTheDefiningPartInTheSecondDocument()
    {
        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial P(int x)
                                            {
                                            }
                                        }
                                        """;

        const string definingPart = """
                                    public partial class P
                                    {
                                        partial {|RH2004:P|}(int x);
                                    }
                                    """;

        await Verify(implementingPart, test => test.TestState.Sources.Add(("/0/Test1.cs", definingPart)));
    }

    /// <summary>
    /// Verifies that a partial constructor both of whose parts lie in generated code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialConstructorWithBothPartsGeneratedIsNotReported()
    {
        const string definingPart = """
                                    public partial class P
                                    {
                                        partial P(int x);
                                    }
                                    """;

        const string implementingPart = """
                                        public partial class P
                                        {
                                            partial P(int x)
                                            {
                                            }
                                        }
                                        """;

        await Verify(string.Empty,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Defining.g.cs", definingPart));
                         test.TestState.Sources.Add(("/0/Implementing.g.cs", implementingPart));
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    string {|#0:field|};

                                    void {|#1:DoWork|}()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  internal class Sample
                                  {
                                      private string field;

                                      private void DoWork()
                                      {
                                      }
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH2004AccessModifierMustBeDeclaredAnalyzer.DiagnosticId, AnalyzerResources.RH2004MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}