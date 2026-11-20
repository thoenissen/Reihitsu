using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH8201InheritdocShouldBeUsedCodeFixProvider"/> on members that implement an interface member, and on conditional directives that make the implemented interfaces depend on the build
/// </summary>
[TestClass]
public class RH8201InheritdocShouldBeUsedInterfaceImplementationTests : BatchCodeFixTestsBase<RH8201InheritdocShouldBeUsedAnalyzer, RH8201InheritdocShouldBeUsedCodeFixProvider>
{
    #region Methods

    /// <summary>
    /// Verifies that a documented implicit interface method implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplicitInterfaceMethod()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface property implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplicitInterfaceProperty()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int TestProperty { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public int TestProperty => 0;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int TestProperty { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public int TestProperty => 0;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface indexer implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplicitInterfaceIndexer()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public int this[int index] => index;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int this[int index] { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public int this[int index] => index;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface event implementation with accessors is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplicitInterfaceEvent()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public event EventHandler TestEvent
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

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public event EventHandler TestEvent
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface field-like event implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplicitInterfaceFieldLikeEvent()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public event EventHandler TestEvent;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public event EventHandler TestEvent;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented field-like event is detected and fixed when every declarator implements an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForFieldLikeEventWhoseDeclaratorsAllImplementInterfaceEvents()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler First;

                                    event EventHandler Second;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public event EventHandler First, Second;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler First;

                                      event EventHandler Second;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public event EventHandler First, Second;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented field-like event is not reported when only its first declarator implements an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyNoDiagnosticForFieldLikeEventWhoseSecondDeclaratorImplementsNothing()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler First;
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public event EventHandler First, Second;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented field-like event is not reported when only its second declarator implements an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyNoDiagnosticForFieldLikeEventWhoseFirstDeclaratorImplementsNothing()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler Second;
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public event EventHandler First, Second;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit interface method implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForExplicitInterfaceMethod()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void ITest.TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      void ITest.TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit interface property implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForExplicitInterfaceProperty()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int TestProperty { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    int ITest.TestProperty => 0;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int TestProperty { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      int ITest.TestProperty => 0;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit interface indexer implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForExplicitInterfaceIndexer()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    int ITest.this[int index] => index;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int this[int index] { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      int ITest.this[int index] => index;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit interface event implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForExplicitInterfaceEvent()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    event EventHandler ITest.TestEvent
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

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      event EventHandler ITest.TestEvent
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of an interface declared in metadata is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForMetadataInterfaceImplementation()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of a generic interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForGenericInterfaceImplementation()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IEquatable<TestImplementation>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public bool Equals(TestImplementation other)
                                    {
                                        return ReferenceEquals(this, other);
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IEquatable<TestImplementation>
                                  {
                                      /// <inheritdoc/>
                                      public bool Equals(TestImplementation other)
                                      {
                                          return ReferenceEquals(this, other);
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of a member declared on a base interface is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplementationOfInheritedInterfaceMember()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestImplementation : IDerived
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface IBase
                                  {
                                      void TestMethod();
                                  }

                                  internal interface IDerived : IBase
                                  {
                                  }

                                  internal class TestImplementation : IDerived
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented virtual member implementing an interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForVirtualInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public virtual void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public virtual void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented member implementing members of several interfaces is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForMemberImplementingSeveralInterfaces()
    {
        const string testData = """
                                internal interface IFirst
                                {
                                    void TestMethod();
                                }

                                internal interface ISecond
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : IFirst, ISecond
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface IFirst
                                  {
                                      void TestMethod();
                                  }

                                  internal interface ISecond
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : IFirst, ISecond
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a member implementing several interfaces is not reported when it selects the inherited documentation with &lt;inheritdoc cref="..."/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyNoDiagnosticForMemberImplementingSeveralInterfacesWithInheritdocCref()
    {
        const string testData = """
                                internal interface IFirst
                                {
                                    void TestMethod();
                                }

                                internal interface ISecond
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : IFirst, ISecond
                                {
                                    /// <inheritdoc cref="IFirst.TestMethod"/>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implementation of a static abstract interface method is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForStaticAbstractInterfaceMethod()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    static abstract void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public static void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      static abstract void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public static void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit implementation of a static abstract interface operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForImplicitStaticAbstractOperator()
    {
        const string testData = """
                                internal interface IAddable<T>
                                    where T : IAddable<T>
                                {
                                    static abstract T operator +(T left, T right);
                                }

                                internal readonly struct Amount : IAddable<Amount>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public static Amount operator +(Amount left, Amount right) => left;
                                }
                                """;

        const string resultData = """
                                  internal interface IAddable<T>
                                      where T : IAddable<T>
                                  {
                                      static abstract T operator +(T left, T right);
                                  }

                                  internal readonly struct Amount : IAddable<Amount>
                                  {
                                      /// <inheritdoc/>
                                      public static Amount operator +(Amount left, Amount right) => left;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit implementation of a static abstract interface operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForExplicitStaticAbstractOperator()
    {
        const string testData = """
                                internal interface IAddable<T>
                                    where T : IAddable<T>
                                {
                                    static abstract T operator +(T left, T right);
                                }

                                internal readonly struct Amount : IAddable<Amount>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    static Amount IAddable<Amount>.operator +(Amount left, Amount right) => left;
                                }
                                """;

        const string resultData = """
                                  internal interface IAddable<T>
                                      where T : IAddable<T>
                                  {
                                      static abstract T operator +(T left, T right);
                                  }

                                  internal readonly struct Amount : IAddable<Amount>
                                  {
                                      /// <inheritdoc/>
                                      static Amount IAddable<Amount>.operator +(Amount left, Amount right) => left;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of a static abstract interface conversion operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForStaticAbstractConversionOperator()
    {
        const string testData = """
                                internal interface IConvertible<T>
                                    where T : IConvertible<T>
                                {
                                    static abstract implicit operator int(T value);
                                }

                                internal readonly struct Amount : IConvertible<Amount>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public static implicit operator int(Amount value) => 0;
                                }
                                """;

        const string resultData = """
                                  internal interface IConvertible<T>
                                      where T : IConvertible<T>
                                  {
                                      static abstract implicit operator int(T value);
                                  }

                                  internal readonly struct Amount : IConvertible<Amount>
                                  {
                                      /// <inheritdoc/>
                                      public static implicit operator int(Amount value) => 0;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented operator that implements no interface member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForOperatorWithoutInterface()
    {
        const string testData = """
                                internal readonly struct Amount
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public static Amount operator +(Amount left, Amount right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented conversion operator that implements no interface member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForConversionOperatorWithoutInterface()
    {
        const string testData = """
                                internal readonly struct Amount
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public static implicit operator int(Amount value) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit implementation of a base interface member inside an interface is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForInterfaceMemberOverridingBaseInterfaceMember()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived : IBase
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void IBase.TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface IBase
                                  {
                                      void TestMethod();
                                  }

                                  internal interface IDerived : IBase
                                  {
                                      /// <inheritdoc/>
                                      void IBase.TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented interface member hiding a base interface member with new is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForInterfaceMemberHidingBaseInterfaceMember()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived : IBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    new void TestMethod();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member declared by an interface itself is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForInterfaceOwnMember()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    void TestMethod();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member re-implementing an interface member with new is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForReimplementationWithNew()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public new void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestBase : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }

                                  internal class TestImplementation : TestBase, ITest
                                  {
                                      /// <inheritdoc/>
                                      public new void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented member hiding an inherited implementation is not reported when its type does not list the interface itself
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForHidingMemberWithoutReimplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public new void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented base member is not reported when only a derived type lists the interface it satisfies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForBaseMemberWhenOnlyDerivedTypeListsInterface()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented overload of an implemented member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForOverloadOfImplementedMember()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented public member is not reported when an explicit implementation of the same interface member exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForPublicMemberBesideExplicitImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    void ITest.TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member of a type implementing an interface is not reported when it implements no interface member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForMemberImplementingNoInterface()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void OtherMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the documented defining declaration of a partial method implementing an interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDiagnosticForDocumentedDefiningPartialInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal partial class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public partial void TestMethod();

                                    public partial void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal partial class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public partial void TestMethod();

                                      public partial void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that the documented implementing declaration of a partial method implementing an interface member is
    /// detected and fixed, as the defining declaration is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForDocumentedImplementingPartialInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal partial class TestImplementation : ITest
                                {
                                    public partial void TestMethod();

                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public partial void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal partial class TestImplementation : ITest
                                  {
                                      public partial void TestMethod();

                                      /// <inheritdoc/>
                                      public partial void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that override, implicit, and explicit interface implementation diagnostics in one document are fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticsForOverrideAndInterfaceImplementationsAreFixedTogether()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void First();

                                    void Second();
                                }

                                internal abstract class TestBase
                                {
                                    public abstract void Third();
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void First()
                                    {
                                    }

                                    ///{|#1: <summary>Implementation documentation</summary>
                                |}    void ITest.Second()
                                    {
                                    }

                                    ///{|#2: <summary>Implementation documentation</summary>
                                |}    public override void Third()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void First();

                                      void Second();
                                  }

                                  internal abstract class TestBase
                                  {
                                      public abstract void Third();
                                  }

                                  internal class TestImplementation : TestBase, ITest
                                  {
                                      /// <inheritdoc/>
                                      public void First()
                                      {
                                      }

                                      /// <inheritdoc/>
                                      void ITest.Second()
                                      {
                                      }

                                      /// <inheritdoc/>
                                      public override void Third()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForImplicitImplementationWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyWithPreprocessorSymbols(RemoveMarkup(testData), "FEATURE");
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the interface list of its type contains disabled text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForImplicitImplementationWhenAnotherInterfaceIsDisabled()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                #if FEATURE
                                    , ICloneable
                                #endif
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when another partial declaration of its type has a conditionally compiled interface list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForImplicitImplementationWhenOtherPartialDeclarationIsConditional()
    {
        const string testData = """
                                using System;

                                internal partial class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                }

                                internal partial class TestImplementation
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented field-like event implementing an interface event is reported without a code fix when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForFieldLikeEventWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation
                                #if !FEATURE
                                    : ITest
                                #endif
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public event EventHandler TestEvent;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit interface implementation is detected and fixed even when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForExplicitImplementationWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void IDisposable.Dispose()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation
                                  #if !FEATURE
                                      : IDisposable
                                  #endif
                                  {
                                      /// <inheritdoc/>
                                      void IDisposable.Dispose()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented override is detected and fixed even when the base list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForOverrideWhenBaseListIsConditional()
    {
        const string testData = """
                                internal abstract class TestBase
                                {
                                    public abstract void TestMethod();
                                }

                                internal class TestImplementation
                                #if !FEATURE
                                    : TestBase
                                #endif
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public override void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal abstract class TestBase
                                  {
                                      public abstract void TestMethod();
                                  }

                                  internal class TestImplementation
                                  #if !FEATURE
                                      : TestBase
                                  #endif
                                  {
                                      /// <inheritdoc/>
                                      public override void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when directives appear only inside the type body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForImplicitImplementationWhenDirectiveIsInsideTypeBody()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    #region Methods

                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                      #region Methods

                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }

                                      #endregion // Methods
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when a conditional region encloses the partial declaration that lists the interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenConditionalRegionEnclosesPartialDeclaration()
    {
        const string testData = """
                                using System;

                                internal partial class TestImplementation
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }

                                #if !FEATURE
                                internal partial class TestImplementation : IDisposable
                                {
                                }
                                #endif
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when conditional regions supply alternative headers for its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenConditionalRegionSuppliesAlternativeHeaders()
    {
        const string testData = """
                                using System;

                                #if !FEATURE
                                internal class TestImplementation : IDisposable
                                {
                                #else
                                internal class TestImplementation
                                {
                                #endif
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the implemented interface member is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenInterfaceMemberIsConditional()
    {
        const string testData = """
                                internal interface ITest
                                {
                                #if !FEATURE
                                    void TestMethod();
                                #endif
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the base list of an implemented interface is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenInterfaceBaseListIsConditional()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived
                                #if !FEATURE
                                    : IBase
                                #endif
                                {
                                }

                                internal class TestImplementation : IDerived
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the file declaring its type contains any conditional region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenFileOfTypeContainsUnrelatedConditionalRegion()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                #if DEBUG
                                        Console.WriteLine();
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when the file contains directives other than conditional regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticForImplicitImplementationWhenFileContainsOnlyPragmaDirective()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                #pragma warning disable CS0168
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                #pragma warning restore CS0168
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                  #pragma warning disable CS0168
                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }
                                  #pragma warning restore CS0168
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is reported without a code fix when the file declaring the
    /// implemented interface contains a conditional region, even though the file declaring the type contains none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenInterfaceFileContainsConditionalRegion()
    {
        const string testData = """
                                internal class TestImplementation : ITest
                                {
                                    ///{|RH8201: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string interfaceData = """
                                     internal interface ITest
                                     {
                                         void TestMethod();
                                     #if FEATURE
                                         void OtherMethod();
                                     #endif
                                     }
                                     """;

        await Verify(testData,
                     testData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", interfaceData));
                     });
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when only a file that
    /// declares neither its type nor its interfaces contains a conditional region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    public async Task VerifyDiagnosticWhenOnlyUnrelatedFileContainsConditionalRegion()
    {
        const string testData = """
                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        const string interfaceData = """
                                     internal interface ITest
                                     {
                                         void TestMethod();
                                     }
                                     """;

        const string unrelatedData = """
                                     internal class Unrelated
                                     {
                                     #if FEATURE
                                         public void OtherMethod()
                                         {
                                         }
                                     #endif
                                     }
                                     """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.TestState.Sources.Add(("/0/Test2.cs", unrelatedData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.FixedState.Sources.Add(("/0/Test2.cs", unrelatedData));
                     },
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();

                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract int TestProperty { get; set; }
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                        public override void TestMethod()
                                        {
                                        }

                                        /**{|#1: <summary>Implementation documentation</summary> */|}
                                        public override int TestProperty { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();

                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract int TestProperty { get; set; }
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          public override void TestMethod()
                                          {
                                          }

                                          /// <inheritdoc/>
                                          public override int TestProperty { get; set; }
                                      }
                                  }
                                  """;

        // Verifies that Fix All replaces every multi-line (/** */) documentation comment in a type in one iteration, which is the common shape when a type overrides several documented members
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}