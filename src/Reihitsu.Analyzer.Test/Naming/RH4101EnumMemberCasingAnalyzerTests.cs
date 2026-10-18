using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4101EnumMemberCasingAnalyzer"/> and <see cref="RH4101EnumMemberCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4101EnumMemberCasingAnalyzerTests : BatchCodeFixTestsBase<RH4101EnumMemberCasingAnalyzer, RH4101EnumMemberCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-001")]
    [Requirement("RH4101-002")]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test enum
                                    /// </summary>
                                    public enum TestEnum
                                    {
                                        /// <summary>
                                        /// Member
                                        /// </summary>
                                        {|#0:member|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 using System.Text;
                                 using System.Threading.Tasks;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test enum
                                     /// </summary>
                                     public enum TestEnum
                                     {
                                         /// <summary>
                                         /// Member
                                         /// </summary>
                                         Member
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4101EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4101MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase enum members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseEnumMembers()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public enum Status
                                    {
                                        Active,
                                        Pending,
                                        Closed
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for multiple enum members with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-001")]
    [Requirement("RH4101-002")]
    public async Task VerifyDiagnosticsForMultipleWrongCasingMembers()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public enum Status
                                    {
                                        {|#0:active|},
                                        Valid,
                                        {|#1:inactive|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public enum Status
                                     {
                                         Active,
                                         Valid,
                                         Inactive
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4101EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4101MessageFormat, 2));
    }

    /// <summary>
    /// Verifying diagnostics for an enum member with an underscore prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-001")]
    [Requirement("RH4101-002")]
    public async Task VerifyDiagnosticsForEnumMemberWithUnderscorePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public enum Permission
                                    {
                                        {|#0:_readAccess|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public enum Permission
                                     {
                                         ReadAccess
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4101EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4101MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a Flags enum with PascalCase members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-001")]
    public async Task VerifyNoDiagnosticsForFlagsEnumMembers()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    [Flags]
                                    public enum FileAccess
                                    {
                                        Read = 1,
                                        Write = 2,
                                        Execute = 4
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a lowercase member, a member with a separator, and a member with a leading underscore are each reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-001")]
    public async Task EachEnumMemberThatIsNotPascalCaseIsReported()
    {
        const string testCode = """
                                public enum Status
                                {
                                    {|RH4101:active|},
                                    Valid,
                                    {|RH4101:in_progress|},
                                    {|RH4101:_readAccess|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames each enum member to PascalCase together with its use in a member access
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4101-002")]
    public async Task FixRenamesEnumMembersAndEveryReference()
    {
        const string testCode = """
                                public enum OrderState
                                {
                                    {|RH4101:created|},
                                    {|RH4101:in_progress|},
                                    {|RH4101:_readAccess|}
                                }

                                public class C
                                {
                                    public OrderState State = OrderState.created;
                                }
                                """;

        const string fixedCode = """
                                 public enum OrderState
                                 {
                                     Created,
                                     InProgress,
                                     ReadAccess
                                 }

                                 public class C
                                 {
                                     public OrderState State = OrderState.Created;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public enum OrderState
                                    {
                                        {|#0:created|},
                                        {|#1:in_progress|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public enum OrderState
                                     {
                                         Created,
                                         InProgress
                                     }
                                 }
                                 """;

        // Both members belong to the same enum declaration, so the two renames are applied to one member
        // list. The second name also exercises the underscore segmentation of the PascalCase conversion
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4101EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4101MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}