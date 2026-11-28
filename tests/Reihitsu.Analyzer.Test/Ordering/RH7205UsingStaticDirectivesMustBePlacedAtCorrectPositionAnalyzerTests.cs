using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer"/> and <see cref="RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzerTests : BatchCodeFixTestsBase<RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer, RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying static usings are reported and fixed when they appear before regular usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-003")]
    [Requirement("RH7205-004")]
    public async Task StaticUsingsAreReportedAndFixedWhenTheyAppearBeforeRegularUsings()
    {
        const string testCode = """
                                using static {|#0:System.Math|};
                                using Alpha;

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using Alpha;
                                 
                                 using static System.Math;

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a block whose excluded conditional group lies between two directives is reported as the build compiles it,
    /// without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-004")]
    [Requirement("SHARED-USING-009")]
    public async Task DisabledConditionalUsingBlockIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                using static {|RH7205:System.Math|};
                                #if FEATURE
                                using System.Text;
                                #endif
                                using System;
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a static directive above a regular directive is reported on its type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-003")]
    public async Task StaticDirectiveAboveARegularDirectiveIsReportedOnItsTypeName()
    {
        const string testCode = """
                                using static {|RH7205:System.Math|};
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a static directive that both follows an alias and precedes a regular directive is reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-003")]
    public async Task StaticDirectiveMisplacedOnBothSidesIsReportedOnce()
    {
        const string testCode = """
                                using A = System.Text;
                                using static {|RH7205:System.Math|};
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a global static directive is not compared with a local regular directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalStaticDirectiveIsNotComparedWithALocalDirective()
    {
        const string testCode = """
                                global using static System.Math;
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example: a block containing a preprocessor directive is reported in a build that does not
    /// define the conditional symbol, without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-004")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithPreprocessorDirectiveIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                using static {|RH7205:System.Math|};
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using System;

                                class C
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a block holding a <c>#region</c> directive is reported, without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-004")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithRegionDirectiveIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                using static {|RH7205:System.Math|};
                                #region Imports
                                using System;
                                #endregion

                                class C
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a regular directive inside a conditional group below a static directive is compared in a build that
    /// defines the symbol, so the static directive is reported without a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("RH7205-004")]
    [Requirement("SHARED-USING-009")]
    public async Task CompiledConditionalRegularDirectiveBelowAStaticDirectiveIsReportedWithoutCodeFix()
    {
        const string testCode = """
                                using static {|RH7205:System.Math|};
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using static System.Console;

                                class C
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode, null, "DEBUG");
    }

    /// <summary>
    /// Verifies that a regular directive inside a conditional group below a static directive is absent in a build that does
    /// not define the symbol, so the block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-001")]
    [Requirement("SHARED-USING-009")]
    public async Task ExcludedConditionalRegularDirectiveBelowAStaticDirectiveIsNotReported()
    {
        const string testCode = """
                                using static System.Math;
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using static System.Console;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that static directives between the regular and the alias directives, and the other kinds, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-002")]
    [Requirement("SHARED-USING-003")]
    public async Task StaticDirectivesBetweenRegularAndAliasDirectivesAreNotReported()
    {
        const string testCode = """
                                using System;
                                using static System.Math;
                                using static System.Console;
                                using A = System.Text;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the code fix organizes the whole using block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-004")]
    [Requirement("SHARED-USING-003")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using A = System.Text;
                                using static {|RH7205:System.Math|};
                                using System;

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using static System.Math;

                                 using A = System.Text;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the code fix leaves the code after the using block unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-004")]
    public async Task CodeFixLeavesTheCodeAfterTheBlockUnchanged()
    {
        const string testCode = """
                                using static {|RH7205:System.Math|};
                                using System;

                                class C
                                {
                                    void M( ) {   }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using static System.Math;

                                 class C
                                 {
                                     void M( ) {   }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment before the first directive is comment text,
    /// so the block is reported and fixed
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInCommentDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                /*
                                # Header
                                */
                                using static {|RH7205:System.Math|};
                                using Alpha;

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 /*
                                 # Header
                                 */
                                 using Alpha;

                                 using static System.Math;

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 public class TestClass
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix keeps the indentation of a namespace with a body when a header without a blank line stands above
    /// the first directive and that directive moves away from the top
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-004")]
    [Requirement("FMT-USING-011")]
    public async Task CodeFixKeepsNamespaceIndentationBelowAHeader()
    {
        const string testCode = """
                                namespace N
                                {
                                    // Header
                                    using static {|RH7205:System.Math|};
                                    using System;

                                    class C
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     // Header
                                     using System;

                                     using static System.Math;

                                     class C
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the specification example: a block with a preprocessor directive in front of its first directive is fixed, with the directive at the top
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7205-004")]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task BlockLedByANullableDirectiveIsFixedWithTheDirectiveAtTheTop(string endOfLine)
    {
        const string testCode = """
                                #nullable enable
                                using static {|RH7205:System.Math|};
                                using System;

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 #nullable enable
                                 using System;

                                 using static System.Math;

                                 class C
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using static {|#0:System.Console|};
                                using static {|#1:System.Math|};
                                using Alpha;

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using Alpha;

                                 using static System.Console;
                                 using static System.Math;

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 public class TestClass
                                 {
                                 }
                                 """;

        // Both static directives precede the same regular using, and each fix reorganizes the whole
        // compilation-unit using list, so the two text changes cover the identical span and the surviving one
        // already clears both
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}