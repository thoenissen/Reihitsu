using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0205EnumMemberCasingAnalyzer"/> and <see cref="RH0205EnumMemberCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0205EnumMemberCasingAnalyzerTests : AnalyzerTestsBase<RH0205EnumMemberCasingAnalyzer, RH0205EnumMemberCasingCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0205EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0205MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for PascalCase enum members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0205EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0205MessageFormat, 2));
    }

    /// <summary>
    /// Verifying diagnostics for an enum member with an underscore prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testCode, fixedCode, Diagnostics(RH0205EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0205MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a Flags enum with PascalCase members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
}