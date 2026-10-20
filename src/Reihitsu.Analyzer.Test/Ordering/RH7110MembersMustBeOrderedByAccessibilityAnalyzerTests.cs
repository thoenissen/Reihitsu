using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7110MembersMustBeOrderedByAccessibilityAnalyzer"/>
/// </summary>
[TestClass]
public class RH7110MembersMustBeOrderedByAccessibilityAnalyzerTests : AnalyzerTestsBase<RH7110MembersMustBeOrderedByAccessibilityAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifying a public method after a private method of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    [Requirement("RH7110-004")]
    public async Task PublicMethodAfterPrivateMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private void Helper()
                                    {
                                    }

                                    public void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a protected member after a private member of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    public async Task ProtectedMethodAfterPrivateMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private void Helper()
                                    {
                                    }

                                    protected void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying an internal member after a protected member of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    public async Task InternalMethodAfterProtectedMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    protected void Helper()
                                    {
                                    }

                                    internal void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a protected internal member after a protected member of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    public async Task ProtectedInternalMethodAfterProtectedMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    protected void Helper()
                                    {
                                    }

                                    protected internal void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a public field after a private field is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    [Requirement("RH7110-004")]
    public async Task PublicFieldAfterPrivateFieldIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    public int {|#0:Value|};
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a public member after a private member of the same kind within the same region is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-003")]
    public async Task PublicMemberAfterPrivateMemberOfSameKindWithinSameRegionIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Methods
                                    private void Helper()
                                    {
                                    }

                                    public void {|#0:Run|}()
                                    {
                                    }
                                    #endregion
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying the specification example: each member is compared with the latest accessibility in the order that
    /// came before it in its group, and both spellings of the compound accessibilities share one position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    [Requirement("RH7110-004")]
    public async Task MembersAfterAMemberLaterInTheAccessibilityOrderAreReported()
    {
        const string source = """
                              class C
                              {
                                  private int _a;
                                  public int {|RH7110:B|};
                                  internal int {|RH7110:D|};
                                  internal protected int {|RH7110:E|};

                                  protected void M()
                                  {
                                  }

                                  protected private void N()
                                  {
                                  }

                                  internal protected void {|RH7110:O|}()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: only members of the same kind are compared, and the diagnostic is on the
    /// constructor's identifier, the delegate's identifier, and the indexer's <c>this</c> keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-002")]
    [Requirement("RH7110-004")]
    public async Task OnlyMembersOfTheSameKindAreCompared()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private int _value;
                                  public int Value { get; set; }

                                  private event Action _changed;
                                  public event Action Changed
                                  {
                                      add
                                      {
                                      }

                                      remove
                                      {
                                      }
                                  }

                                  private C()
                                  {
                                  }

                                  public {|RH7110:C|}(int value)
                                  {
                                  }

                                  private delegate void Handler();
                                  public delegate void {|RH7110:Callback|}();

                                  private int this[int index] => index;
                                  public int {|RH7110:this|}[string key] => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that an event field is reported on the identifier of its first declarator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-004")]
    public async Task EventFieldIsReportedOnItsFirstDeclarator()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private event Action _changed;
                                  public event Action {|RH7110:Changed|}, Updated;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: a member in a nested region is compared within its top-level region, and
    /// a member outside every region is not compared with the members of the region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-003")]
    public async Task MemberInANestedRegionIsComparedWithinItsTopLevelRegion()
    {
        const string source = """
                              class C
                              {
                                  #region Members

                                  private void A()
                                  {
                                  }

                                  #region Inner

                                  public void {|RH7110:B|}()
                                  {
                                  }

                                  #endregion

                                  #endregion

                                  public void D()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that an operator is reported on its operator token and a conversion operator on its
    /// <c>operator</c> keyword. Operators must be public, so the input does not compile and only the location is
    /// checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-004")]
    public async Task OperatorsAreReportedOnTheirOperatorTokens()
    {
        const string source = """
                              class C
                              {
                                  private static C operator +(C a, C b) => a;
                                  public static C operator {|RH7110:-|}(C a, C b) => a;

                                  private static explicit operator long(C c) => 0;
                                  public static implicit {|RH7110:operator|} int(C c) => 0;
                              }
                              """;

        await Verify(source, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    #endregion // Diagnostic cases

    #region Non-diagnostic cases

    /// <summary>
    /// Verifying members ordered from most to least accessible produce no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-001")]
    public async Task MembersOrderedFromMostToLeastAccessibleProduceNoDiagnostic()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    internal void Configure()
                                    {
                                    }

                                    protected void Prepare()
                                    {
                                    }

                                    private void Helper()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the static ordering within an accessibility group does not conflict with this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticBeforeInstanceWithinAccessibilityGroupProducesNoDiagnostic()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public static void Create()
                                    {
                                    }

                                    public void Run()
                                    {
                                    }

                                    private static void CreateHelper()
                                    {
                                    }

                                    private void Helper()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying members of different kinds are not compared against each other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-002")]
    public async Task DifferentKindsAreNotComparedAgainstEachOther()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    public void Run()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying accessibility ordering is evaluated within each region independently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-003")]
    public async Task OrderingIsEvaluatedWithinEachRegionIndependently()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region First
                                    private void FirstHelper()
                                    {
                                    }
                                    #endregion

                                    #region Second
                                    public void SecondRun()
                                    {
                                    }
                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying members without an explicit accessibility modifier are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MembersWithoutExplicitAccessibilityAreIgnored()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    void Helper()
                                    {
                                    }

                                    public void Run()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the specification example: a member in a region is not compared with a member outside every region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7110-003")]
    public async Task MemberInARegionIsNotComparedWithAMemberOutsideEveryRegion()
    {
        const string source = """
                              class C
                              {
                                  private void A()
                                  {
                                  }

                                  #region Api

                                  public void B()
                                  {
                                  }

                                  #endregion
                              }
                              """;

        await Verify(source);
    }

    #endregion // Non-diagnostic cases
}