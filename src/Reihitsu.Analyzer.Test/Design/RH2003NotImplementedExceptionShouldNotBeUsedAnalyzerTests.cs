using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2003NotImplementedExceptionShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that NotImplementedException triggers diagnostics in methods and properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    [Requirement("RH2003-003")]
    [Requirement("RH2003-004")]
    public async Task VerifyNotImplementedExceptionDiagnostics()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class RH2003
                                {
                                    public void ThrowNotImplemented()
                                    {
                                        throw new {|#0:NotImplementedException|}();
                                    }

                                    public void ThrowNotImplementedWithMessage()
                                    {
                                        throw new {|#1:NotImplementedException|}("Not yet implemented");
                                    }

                                    public int Property
                                    {
                                        get
                                        {
                                            throw new {|#2:NotImplementedException|}();
                                        }
                                    }

                                    public void ThrowArgumentException()
                                    {
                                        throw new ArgumentException("test");
                                    }

                                    public void ThrowInvalidOperationException()
                                    {
                                        throw new InvalidOperationException();
                                    }

                                    public void NoThrow()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that implicit creation of <see cref="System.NotImplementedException"/> triggers a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-002")]
    [Requirement("RH2003-004")]
    public async Task VerifyImplicitNotImplementedExceptionDiagnostic()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    private readonly NotImplementedException exception = {|#0:new|}();
                                }
                                """;

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat));
    }

    /// <summary>
    /// Verifying that implicit creation of another exception type does not trigger a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-003")]
    public async Task VerifyImplicitOtherExceptionDoesNotTriggerDiagnostic()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    private readonly InvalidOperationException exception = new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the verbatim prefix and whitespace or a comment inside the qualified name do not change the created
    /// type's name, so all three creations are reported like the plain name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-002")]
    [Requirement("RH2003-001")]
    public async Task VerifyTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsReportedLikeThePlainName()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    object a = new {|#0:@NotImplementedException|}();
                                    object b = new {|#1:System . NotImplementedException|}();
                                    object c = new {|#2:System./* c */NotImplementedException|}();
                                }
                                """;

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat, 3));
    }

    /// <summary>
    /// Verifies another exception type written with the verbatim prefix or with whitespace or a comment inside its
    /// qualified name is not reported, since its value is not the name <c>NotImplementedException</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-002")]
    public async Task VerifyOtherTypeNameWrittenWithVerbatimPrefixOrInnerTriviaIsNotReported()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    object a = new @ArgumentException();
                                    object b = new System . ArgumentException();
                                    object c = new System./* c */ArgumentException();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that explicit creations with arguments, with an object initializer, and with a qualified or
    /// <see langword="global"/>-qualified type name are reported on the whole created type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    [Requirement("RH2003-004")]
    public async Task VerifyQualifiedAndInitializedCreationsAreReportedOnCreatedType()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void A() => throw new {|#0:NotImplementedException|}();

                                    void B() => throw new {|#1:NotImplementedException|}("Later");

                                    void D() => throw new {|#2:global::System.NotImplementedException|}();

                                    void F() => throw new {|#3:System.NotImplementedException|}("Later", null);

                                    Exception E() => new {|#4:NotImplementedException|} { Source = "C" };

                                    int P
                                    {
                                        get
                                        {
                                            throw new {|#5:NotImplementedException|}();
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat, 6));
    }

    /// <summary>
    /// Verifying that a target-typed creation in a <see langword="throw"/> expression and as an argument is reported
    /// on its <see langword="new"/> keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-002")]
    [Requirement("RH2003-004")]
    public async Task VerifyTargetTypedCreationsAreReportedOnNewKeyword()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    NotImplementedException _exception = {|#0:new|}();

                                    NotImplementedException Create() => {|#1:new|}("Later");

                                    void Take(NotImplementedException exception)
                                    {
                                    }

                                    void M() => Take({|#2:new|}());
                                }
                                """;

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that other exception types, a type derived from <see cref="System.NotImplementedException"/>, and a type
    /// named <c>NotImplementedException</c> in another namespace are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-003")]
    public async Task VerifyDerivedAndSameNamedTypesAreNotReported()
    {
        const string testData = """
                                using System;

                                class MyException : NotImplementedException
                                {
                                }

                                class C
                                {
                                    void A() => throw new NotSupportedException();

                                    void B() => throw new MyException();

                                    InvalidOperationException _exception = new();

                                    MyException _derived = new();
                                }

                                namespace Other
                                {
                                    class NotImplementedException : System.Exception
                                    {
                                    }

                                    class D
                                    {
                                        void M() => throw new NotImplementedException();

                                        NotImplementedException _exception = new();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies a name qualified through a <see langword="using"/> alias of the namespace <see cref="System"/> is resolved
    /// to the type the alias names: <c>S.NotImplementedException</c> is reported like
    /// <c>System.NotImplementedException</c>, and <c>S.ArgumentException</c> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-002")]
    [Requirement("RH2003-001")]
    public async Task VerifyCreationThroughNamespaceAliasIsReportedLikeTheTypeName()
    {
        const string testData = """
                                using S = System;

                                class C
                                {
                                    object a = new {|#0:S.NotImplementedException|}();
                                    object b = new S.ArgumentException();
                                }
                                """;

        await Verify(testData, Diagnostics(RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2003MessageFormat));
    }

    /// <summary>
    /// Verifies that a creation through a <see langword="using"/> alias of <see cref="System.NotImplementedException"/>
    /// is reported like the type's own name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    [Requirement("RH2003-004")]
    public async Task VerifyCreationThroughUsingAliasOfTheTypeIsReported()
    {
        const string testData = """
                                using NIE = System.NotImplementedException;

                                class C
                                {
                                    void M() => throw new {|RH2003:NIE|}();

                                    void N() => throw new {|RH2003:System.NotImplementedException|}();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: a creation through an alias of the type, and through an alias of its
    /// namespace written with a dot or with <c>::</c>, is reported on the whole written type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    [Requirement("RH2003-004")]
    public async Task VerifyCreationsThroughTypeAndNamespaceAliasesAreReported()
    {
        const string testData = """
                                using NIE = System.NotImplementedException;
                                using S = System;

                                class C
                                {
                                    void A() => throw new {|RH2003:NIE|}();

                                    void B() => throw new {|RH2003:S.NotImplementedException|}();

                                    void D() => throw new {|RH2003:S::NotImplementedException|}();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a creation through an alias of the type with arguments and an object initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    public async Task VerifyCreationThroughUsingAliasWithArgumentsAndInitializerIsReported()
    {
        const string testData = """
                                using NIE = System.NotImplementedException;

                                class C
                                {
                                    void A() => throw new {|RH2003:NIE|}("Later");

                                    object B() => new {|RH2003:NIE|} { Source = "C" };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an alias name written with the verbatim prefix is resolved like the alias
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    public async Task VerifyCreationThroughVerbatimAliasNameIsReported()
    {
        const string testData = """
                                using NIE = System.NotImplementedException;

                                class C
                                {
                                    void M() => throw new {|RH2003:@NIE|}();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a creation through an alias declared inside a namespace block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    public async Task VerifyCreationThroughAliasDeclaredInNamespaceIsReported()
    {
        const string testData = """
                                namespace N
                                {
                                    using NIE = System.NotImplementedException;

                                    class C
                                    {
                                        void M() => throw new {|RH2003:NIE|}();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a creation through a global <see langword="using"/> alias declared in another file is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    public async Task VerifyCreationThroughGlobalAliasOfAnotherFileIsReported()
    {
        const string testData = """
                                class C
                                {
                                    void M() => throw new {|RH2003:NIE|}();
                                }
                                """;

        const string aliasFile = """
                                 global using NIE = System.NotImplementedException;
                                 """;

        await Verify(testData, test => test.TestState.Sources.Add(("/0/Test1.cs", aliasFile)));
    }

    /// <summary>
    /// Verifies that a creation through an alias of another exception type is not reported, while a creation through an
    /// alias of <see cref="System.NotImplementedException"/> next to it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-001")]
    [Requirement("RH2003-003")]
    public async Task VerifyCreationThroughAliasOfAnotherTypeIsNotReported()
    {
        const string testData = """
                                using NSE = System.NotSupportedException;
                                using NIE = System.NotImplementedException;

                                class C
                                {
                                    void A() => throw new NSE();

                                    void B() => throw new {|RH2003:NIE|}();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: an alias named <c>NotImplementedException</c> of another type is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-003")]
    public async Task VerifyAliasNamedNotImplementedExceptionOfAnotherTypeIsNotReported()
    {
        const string testData = """
                                using NotImplementedException = System.NotSupportedException;

                                class C
                                {
                                    void M() => throw new NotImplementedException();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an alias of a type derived from <see cref="System.NotImplementedException"/> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-003")]
    public async Task VerifyCreationThroughAliasOfDerivedTypeIsNotReported()
    {
        const string testData = """
                                using Derived = N.MyException;

                                namespace N
                                {
                                    class MyException : System.NotImplementedException
                                    {
                                    }
                                }

                                class C
                                {
                                    void M() => throw new Derived();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a target-typed creation whose target is written through an alias is reported on <see langword="new"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2003-002")]
    public async Task VerifyTargetTypedCreationThroughAliasIsReportedOnNewKeyword()
    {
        const string testData = """
                                using NIE = System.NotImplementedException;

                                class C
                                {
                                    NIE _exception = {|RH2003:new|}();
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}