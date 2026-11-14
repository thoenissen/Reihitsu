using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer"/>
/// </summary>
[TestClass]
public class RH8203InheritdocMustBeUsedWithInheritingClassAnalyzerTests : AnalyzerTestsBase<RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer>
{
    #region Constants

    /// <summary>
    /// Included documentation file for the partial member tests: member <c>I</c> holds an inheritdoc element, member
    /// <c>S</c> a summary
    /// </summary>
    private const string PartialMemberIncludedDocumentation = """
                                                              <doc>
                                                                  <members>
                                                                      <member name="I">
                                                                          <inheritdoc/>
                                                                      </member>
                                                                      <member name="S">
                                                                          <summary>Runs</summary>
                                                                      </member>
                                                                  </members>
                                                              </doc>
                                                              """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for inheritdoc on a standalone class
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyDiagnosticForStandaloneInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;

                              /// {|#0:<inheritdoc/>|}
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies that implicit interface implementations may use inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyNoDiagnosticForImplicitInterfaceImplementation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface ITest
                              {
                                  /// <summary>
                                  /// Does work.
                                  /// </summary>
                                  void Execute();
                              }

                              internal class TestClass : ITest
                              {
                                  /// <inheritdoc/>
                                  public void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for inheritdoc on an operator that inherits nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyDiagnosticForOperatorInheritdocNotInheriting()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// {|#0:<inheritdoc/>|}
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for inheritdoc on a conversion operator that inherits nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyDiagnosticForConversionOperatorInheritdocNotInheriting()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// {|#0:<inheritdoc/>|}
                                  public static implicit operator int(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies that an operator implicitly implementing a static abstract interface operator may use inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyNoDiagnosticForImplicitInterfaceOperatorImplementation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface IAddable<T> where T : IAddable<T>
                              {
                                  /// <summary>Adds two values.</summary>
                                  static abstract T operator +(T left, T right);
                              }

                              internal readonly struct Money : IAddable<Money>
                              {
                                  /// <inheritdoc/>
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that inheritdoc nested in remarks is ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyNoDiagnosticForInheritdocNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <remarks><inheritdoc/></remarks>
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// {|#0:<inheritdoc/>|}
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: inheritdoc without cref is reported on a constructor without a matching base constructor, a member that neither overrides nor implements, a field, a type without a base list, an enum, a delegate, and an operator, while an explicit static abstract operator implementation is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifySpecificationExampleReportsInheritdocOnDeclarationsThatInheritNothing()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Base
                              /// </summary>
                              public class B
                              {
                                  /// <summary>
                                  /// Creates
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  public B(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public virtual void Run()
                                  {
                                  }
                              }

                              /// <inheritdoc/>
                              public class D : B
                              {
                                  /// <inheritdoc/>
                                  public D(int value)
                                      : base(value)
                                  {
                                  }

                                  /// {|#0:<inheritdoc/>|}
                                  public D(string text)
                                      : base(0)
                                  {
                                  }

                                  /// <inheritdoc/>
                                  public override void Run()
                                  {
                                  }

                                  /// {|#1:<inheritdoc/>|}
                                  public void Stop()
                                  {
                                  }

                                  /// <inheritdoc cref="Run"/>
                                  public void Restart()
                                  {
                                  }

                                  /// <summary>
                                  /// Pauses
                                  /// </summary>
                                  /// <remarks>
                                  /// <inheritdoc/>
                                  /// </remarks>
                                  public void Pause()
                                  {
                                  }

                                  /// {|#2:<inheritdoc/>|}
                                  public int Count;
                              }

                              /// {|#3:<inheritdoc/>|}
                              public class Standalone
                              {
                              }

                              /// <inheritdoc/>
                              public class Resource : IDisposable
                              {
                                  /// <inheritdoc/>
                                  public void Dispose()
                                  {
                                  }
                              }

                              /// {|#4:<inheritdoc/>|}
                              public enum E
                              {
                              }

                              /// {|#5:<inheritdoc/>|}
                              public delegate void Handler();

                              /// <summary>
                              /// Addition
                              /// </summary>
                              /// <typeparam name="T">Operand</typeparam>
                              public interface IAdd<T>
                                  where T : IAdd<T>
                              {
                                  /// <summary>
                                  /// Adds
                                  /// </summary>
                                  /// <param name="left">Left</param>
                                  /// <param name="right">Right</param>
                                  /// <returns>Sum</returns>
                                  static abstract T operator +(T left, T right);
                              }

                              /// <summary>
                              /// Value
                              /// </summary>
                              public class V : IAdd<V>
                              {
                                  /// <inheritdoc/>
                                  static V IAdd<V>.operator +(V left, V right) => left;

                                  /// {|#6:<inheritdoc/>|}
                                  public static V operator -(V left, V right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat, 7));
    }

    /// <summary>
    /// Verifies the specification example: an inheritdoc element in a later documentation comment is reported on the name of the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifySpecificationExampleReportsLaterCommentOnName()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public void {|#0:Run|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: an inheritdoc element brought in by an include element is reported on the this keyword of an indexer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifySpecificationExampleReportsIncludedInheritdocOnIndexerKeyword()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public int {|#0:this|}[int index] => 0;
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="I">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies an inheritdoc element in a later documentation comment is reported on the first variable of a field, the variable of a field-like event, the operator token, and the operator keyword of a conversion operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyDiagnosticIsOnNameOfDeclarationsDocumentedByLaterComment()
    {
        const string source = """
                              using System;

                              /// <summary>Service</summary>
                              public class C
                              {
                                  /// <summary>Count</summary>

                                  /// <inheritdoc/>
                                  public int {|#0:a|}, b;

                                  /// <summary>Raised</summary>

                                  /// <inheritdoc/>
                                  public event EventHandler {|#1:E1|};

                                  /// <summary>Adds</summary>

                                  /// <inheritdoc/>
                                  public static C operator {|#2:+|}(C left, C right) => left;

                                  /// <summary>Converts</summary>

                                  /// <inheritdoc/>
                                  public static implicit {|#3:operator|} int(C value) => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat, 4));
    }

    /// <summary>
    /// Verifies inheritdoc on a destructor and on a static constructor is reported, while it is not on the parameterless constructor of a class without a base list or of a struct
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyDiagnosticForDestructorAndStaticConstructor()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class C
                              {
                                  /// <inheritdoc/>
                                  public C()
                                  {
                                  }

                                  /// {|#0:<inheritdoc/>|}
                                  static C()
                                  {
                                  }

                                  /// {|#1:<inheritdoc/>|}
                                  ~C()
                                  {
                                  }
                              }

                              /// <summary>Value</summary>
                              public struct S
                              {
                                  /// <inheritdoc/>
                                  public S()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat, 2));
    }

    /// <summary>
    /// Verifies inheritdoc is not reported on struct, interface, record, and record struct declarations with a base list, on an explicit conversion operator implementation, or on a property and an event that implement interface members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyNoDiagnosticForTypesWithBaseListAndInheritingMembers()
    {
        const string source = """
                              using System;

                              /// <summary>Convertible</summary>
                              /// <typeparam name="T">Type</typeparam>
                              public interface IConvert<T>
                                  where T : IConvert<T>
                              {
                                  /// <summary>Converts</summary>
                                  /// <param name="value">Value</param>
                                  /// <returns>Number</returns>
                                  static abstract explicit operator int(T value);
                              }

                              /// <summary>Notifier</summary>
                              public interface INotifier
                              {
                                  /// <summary>Count</summary>
                                  int Count { get; }

                                  /// <summary>Raised</summary>
                                  event EventHandler Changed;
                              }

                              /// <inheritdoc/>
                              public struct S : IDisposable
                              {
                                  /// <inheritdoc/>
                                  public void Dispose()
                                  {
                                  }
                              }

                              /// <inheritdoc/>
                              public interface IDerived : INotifier
                              {
                              }

                              /// <inheritdoc/>
                              public record R : INotifier
                              {
                                  /// <inheritdoc/>
                                  public int Count => 0;

                                  /// <inheritdoc/>
                                  public event EventHandler Changed;
                              }

                              /// <inheritdoc/>
                              public record struct Q : IConvert<Q>
                              {
                                  /// <inheritdoc/>
                                  static explicit IConvert<Q>.operator int(Q value) => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies an inheritdoc element with a cref attribute in included content is not reported, while one without cref is reported on the method name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyIncludedInheritdocWithCrefIsNotReported()
    {
        const string source = """
                              /// <summary>Service</summary>
                              public class C
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="R"]/*'/>
                                  public void Run()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public void {|#0:Stop|}()
                                  {
                                  }
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="R">
                                                 <inheritdoc cref="C"/>
                                             </member>
                                             <member name="I">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.DiagnosticId, AnalyzerResources.RH8203MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a field-like event that declares several events and carries inheritdoc is
    /// reported when one of its events implements nothing, whatever the position of that event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifySpecificationExampleFieldLikeEventWithEventInheritingNothingIsReported()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public interface INotifier
                              {
                                  /// <summary>
                                  /// First
                                  /// </summary>
                                  event EventHandler First;

                                  /// <summary>
                                  /// Second
                                  /// </summary>
                                  event EventHandler Second;
                              }

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public class Notifier : INotifier
                              {
                                  /// {|RH8203:<inheritdoc/>|}
                                  public event EventHandler First, Fourth;

                                  /// {|RH8203:<inheritdoc/>|}
                                  public event EventHandler Fifth, Second;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a field-like event whose events all implement interface events is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyFieldLikeEventWhoseEventsAllImplementIsNotReported()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public interface INotifier
                              {
                                  /// <summary>
                                  /// First
                                  /// </summary>
                                  event EventHandler First;

                                  /// <summary>
                                  /// Second
                                  /// </summary>
                                  event EventHandler Second;
                              }

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public class Notifier : INotifier
                              {
                                  /// <inheritdoc/>
                                  public event EventHandler First, Second;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example of a partial method: neither part of a member that implements an interface
    /// member is reported, both parts of a member that inherits nothing are, and of a member that inherits nothing with
    /// inheritdoc only on its implementing part, only that part is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifySpecificationExamplePartialMethodIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial void M();

                                  /// <inheritdoc/>
                                  public partial void M()
                                  {
                                  }

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial void N();

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial void N()
                                  {
                                  }

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  public partial void O();

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial void O()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that the parts of a partial property are judged part by part, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyPartialPropertyIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  int P { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial int P { get; }

                                  /// <inheritdoc/>
                                  public partial int P => 0;

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial int Q { get; }

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial int Q => 0;

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  public partial int R { get; }

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial int R => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that the parts of a partial indexer are judged part by part, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyPartialIndexerIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Item
                                  /// </summary>
                                  /// <param name="index">Index</param>
                                  int this[int index] { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial int this[int index] { get; }

                                  /// <inheritdoc/>
                                  public partial int this[int index] => index;

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial int this[string key] { get; }

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial int this[string key] => 0;

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  /// <param name="key">Key</param>
                                  public partial int this[long key] { get; }

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial int this[long key] => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that the parts of a partial event, whose defining part is a field-like event, are judged part by part, as
    /// for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyPartialEventIsJudgedPartByPart()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Raised
                                  /// </summary>
                                  event EventHandler E;
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial event EventHandler E;

                                  /// <inheritdoc/>
                                  public partial event EventHandler E
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial event EventHandler F;

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial event EventHandler F
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  public partial event EventHandler G;

                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial event EventHandler G
                                  {
                                      add { }
                                      remove { }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a partial method that inherits nothing, with inheritdoc only on its defining part, is reported on that
    /// part alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyPartialMethodWithInheritdocOnlyOnDefiningPartIsReportedThere()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C
                              {
                                  /// {|RH8203:<inheritdoc/>|}
                                  public partial void O();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  public partial void O()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that neither part of a partial method declared override is reported when both carry inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyOverridingPartialMethodWithInheritdocOnBothPartsIsNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void M();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : B
                              {
                                  /// <inheritdoc/>
                                  public override partial void M();

                                  /// <inheritdoc/>
                                  public override partial void M()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that the parts of a partial method implementing an interface member, declared in two documents, are not
    /// reported when both carry inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyPartialMethodImplementingInterfaceMemberAcrossDocumentsIsNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial void M();
                              }
                              """;
        const string otherSource = """
                                   /// <summary>
                                   /// Service
                                   /// </summary>
                                   public partial class C
                                   {
                                       /// <inheritdoc/>
                                       public partial void M()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, test => test.TestState.Sources.Add(("/0/Test1.cs", otherSource)));
    }

    /// <summary>
    /// Verifies that an inheritdoc element in a later documentation comment of a part of a partial method is considered
    /// for that part alone and reported on its name: neither part of a member that implements an interface member is
    /// reported, both parts of a member that inherits nothing are, and of a member with the later comment on one part only,
    /// only that part is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyInheritdocInLaterCommentOfPartialMethodPartIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void M();

                                  /// <summary>
                                  /// Runs
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void M()
                                  {
                                  }

                                  /// <summary>
                                  /// Stops
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void {|RH8203:N|}();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void {|RH8203:N|}()
                                  {
                                  }

                                  /// <summary>
                                  /// Pauses
                                  /// </summary>
                                  public partial void O();

                                  /// <summary>
                                  /// Pauses
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void {|RH8203:O|}()
                                  {
                                  }

                                  /// <summary>
                                  /// Resumes
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void {|RH8203:P|}();

                                  /// <summary>
                                  /// Resumes
                                  /// </summary>
                                  public partial void P()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that an inheritdoc element in a later documentation comment of a part of a partial property is considered
    /// for that part alone and reported on its name, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyInheritdocInLaterCommentOfPartialPropertyPartIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  int M { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <summary>
                                  /// Value
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int M { get; }

                                  /// <summary>
                                  /// Value
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int M => 0;

                                  /// <summary>
                                  /// Value
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:N|} { get; }

                                  /// <summary>
                                  /// Value
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:N|} => 0;

                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  public partial int O { get; }

                                  /// <summary>
                                  /// Value
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:O|} => 0;

                                  /// <summary>
                                  /// Value
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:P|} { get; }

                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  public partial int P => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that an inheritdoc element in a later documentation comment of a part of a partial indexer is considered
    /// for that part alone and reported on its this keyword, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyInheritdocInLaterCommentOfPartialIndexerPartIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Item
                                  /// </summary>
                                  /// <param name="index">Index</param>
                                  int this[int index] { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <summary>
                                  /// Item
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int this[int index] { get; }

                                  /// <summary>
                                  /// Item
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int this[int index] => index;

                                  /// <summary>
                                  /// Item
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:this|}[string key] { get; }

                                  /// <summary>
                                  /// Item
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:this|}[string key] => 0;

                                  /// <summary>
                                  /// Item
                                  /// </summary>
                                  public partial int this[long key] { get; }

                                  /// <summary>
                                  /// Item
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:this|}[long key] => 0;

                                  /// <summary>
                                  /// Item
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial int {|RH8203:this|}[short key] { get; }

                                  /// <summary>
                                  /// Item
                                  /// </summary>
                                  public partial int this[short key] => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that an inheritdoc element in a later documentation comment of a part of a partial event, whose defining
    /// part is a field-like event, is considered for that part alone and reported on its name, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyInheritdocInLaterCommentOfPartialEventPartIsJudgedPartByPart()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Raised
                                  /// </summary>
                                  event EventHandler M;
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <summary>
                                  /// Raised
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial event EventHandler M;

                                  /// <summary>
                                  /// Raised
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial event EventHandler M
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <summary>
                                  /// Raised
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial event EventHandler {|RH8203:N|};

                                  /// <summary>
                                  /// Raised
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial event EventHandler {|RH8203:N|}
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <summary>
                                  /// Raised
                                  /// </summary>
                                  public partial event EventHandler O;

                                  /// <summary>
                                  /// Raised
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial event EventHandler {|RH8203:O|}
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <summary>
                                  /// Raised
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial event EventHandler {|RH8203:P|};

                                  /// <summary>
                                  /// Raised
                                  /// </summary>
                                  public partial event EventHandler P
                                  {
                                      add { }
                                      remove { }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that an inheritdoc element that an include element brings into a part of a partial method is considered
    /// for that part alone and reported on its name: neither part of a member that implements an interface member is
    /// reported, both parts of a member that inherits nothing are, and of a member with the include element on one part
    /// only, only that part is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyIncludedInheritdocOfPartialMethodPartIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void M();

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void M()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void {|RH8203:N|}();

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void {|RH8203:N|}()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial void O();

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void {|RH8203:O|}()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void {|RH8203:P|}();

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial void P()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", PartialMemberIncludedDocumentation)));
    }

    /// <summary>
    /// Verifies that an inheritdoc element that an include element brings into a part of a partial property is considered
    /// for that part alone and reported on its name, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyIncludedInheritdocOfPartialPropertyPartIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  int M { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int M { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int M => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:N|} { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:N|} => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial int O { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:O|} => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:P|} { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial int P => 0;
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", PartialMemberIncludedDocumentation)));
    }

    /// <summary>
    /// Verifies that an inheritdoc element that an include element brings into a part of a partial indexer is considered
    /// for that part alone and reported on its this keyword, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyIncludedInheritdocOfPartialIndexerPartIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Item
                                  /// </summary>
                                  /// <param name="index">Index</param>
                                  int this[int index] { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int this[int index] { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int this[int index] => index;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:this|}[string key] { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:this|}[string key] => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial int this[long key] { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:this|}[long key] => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial int {|RH8203:this|}[short key] { get; }

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial int this[short key] => 0;
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", PartialMemberIncludedDocumentation)));
    }

    /// <summary>
    /// Verifies that an inheritdoc element that an include element brings into a part of a partial event, whose defining
    /// part is a field-like event, is considered for that part alone and reported on its name, as for a partial method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    [Requirement("RH8203-002")]
    public async Task VerifyIncludedInheritdocOfPartialEventPartIsJudgedPartByPart()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Raised
                                  /// </summary>
                                  event EventHandler M;
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial event EventHandler M;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial event EventHandler M
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial event EventHandler {|RH8203:N|};

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial event EventHandler {|RH8203:N|}
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial event EventHandler O;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial event EventHandler {|RH8203:O|}
                                  {
                                      add { }
                                      remove { }
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial event EventHandler {|RH8203:P|};

                                  /// <include file='docs.xml' path='doc/members/member[@name="S"]/*'/>
                                  public partial event EventHandler P
                                  {
                                      add { }
                                      remove { }
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", PartialMemberIncludedDocumentation)));
    }

    /// <summary>
    /// Verifies that an inheritdoc element with a cref attribute in a later documentation comment of a part of a partial
    /// method is not reported, on either part, while an inheritdoc element without a cref attribute in a later comment of
    /// the other part still is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyInheritdocWithCrefInLaterCommentOfPartialMethodPartIsNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>

                                  /// <inheritdoc cref="C"/>
                                  public partial void M();

                                  /// <summary>
                                  /// Runs
                                  /// </summary>

                                  /// <inheritdoc cref="C"/>
                                  public partial void M()
                                  {
                                  }

                                  /// <summary>
                                  /// Stops
                                  /// </summary>

                                  /// <inheritdoc cref="C"/>
                                  public partial void N();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void {|RH8203:N|}()
                                  {
                                  }

                                  /// <summary>
                                  /// Pauses
                                  /// </summary>

                                  /// <inheritdoc/>
                                  public partial void {|RH8203:P|}();

                                  /// <summary>
                                  /// Pauses
                                  /// </summary>

                                  /// <inheritdoc cref="C"/>
                                  public partial void P()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that an inheritdoc element with a cref attribute that an include element brings into a part of a partial
    /// method is not reported, on either part, while an included inheritdoc element without a cref attribute on the other
    /// part still is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8203-001")]
    public async Task VerifyIncludedInheritdocWithCrefOfPartialMethodPartIsNotReported()
    {
        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="I">
                                                 <inheritdoc/>
                                             </member>
                                             <member name="R">
                                                 <inheritdoc cref="C"/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="R"]/*'/>
                                  public partial void M();

                                  /// <include file='docs.xml' path='doc/members/member[@name="R"]/*'/>
                                  public partial void M()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="R"]/*'/>
                                  public partial void N();

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void {|RH8203:N|}()
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public partial void {|RH8203:P|}();

                                  /// <include file='docs.xml' path='doc/members/member[@name="R"]/*'/>
                                  public partial void P()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)));
    }

    /// <summary>
    /// Characterizes the input of a partial method implementing an interface member without documentation on either part:
    /// nothing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUndocumentedPartialMethodImplementingInterfaceMemberIsNotReported()
    {
        const string source = """
                              interface I
                              {
                                  /// <summary>Does it.</summary>
                                  void M();
                              }

                              partial class C : I
                              {
                                  public partial void M();
                              }

                              partial class C
                              {
                                  public partial void M() { }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}