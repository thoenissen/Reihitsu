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

    #endregion // Tests
}