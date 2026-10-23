using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer"/>
/// </summary>
[TestClass]
public class RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzerTests : AnalyzerTestsBase<RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a comment between an attribute and a method is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentBetweenAttributeAndMethodIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    {|#0:// note|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between two attribute lists is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentBetweenTwoAttributeListsIsReported()
    {
        const string testData = """
                                using System;

                                [Obsolete]
                                {|#0:// note|}
                                [Serializable]
                                public class Data
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment sharing the line with an attribute is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    [Requirement("SHARED-COMMENT-003")]
    public async Task VerifySameLineCommentAfterAttributeIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete] {|#0:/* note */|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a parameter attribute and the parameter is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterParameterAttributeIsReported()
    {
        const string testData = """
                                using System.Runtime.CompilerServices;

                                internal class TestClass
                                {
                                    public void Method([CallerMemberName] {|#0:/* name */|} string name = "")
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the attributes is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyNoDiagnosticForCommentBeforeAttributes()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    // note
                                    [Obsolete]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after an assembly attribute is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-002")]
    public async Task VerifyNoDiagnosticForCommentAfterAssemblyAttribute()
    {
        const string testData = """
                                using System;

                                [assembly: CLSCompliant(false)]

                                // note
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment starting with four slashes after an attribute is reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-001")]
    public async Task VerifyQuadrupleSlashCommentAfterAttributeWithoutDocumentationParsingIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    {|#0://// note|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment inside a preprocessor directive after an attribute is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-002")]
    public async Task VerifyNoDiagnosticForCommentInsideDirective()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    #if DEBUG // note
                                    #endif
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment in disabled code after an attribute is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentInDisabledCode()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    #if UNDEFINED_SYMBOL
                                    // note
                                    #endif
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment at the start of the file is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentAtStartOfFile()
    {
        const string testData = """
                                // header
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between two attributes of one attribute list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyNoDiagnosticForCommentInsideAttributeList()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete,
                                        // compliance
                                        CLSCompliant(false)]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment between an attribute and an enum member is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterEnumMemberAttributeIsReported()
    {
        const string testData = """
                                using System;

                                public enum Kind
                                {
                                    [Obsolete]
                                    {|#0:// note|}
                                    First,
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between an attribute and an accessor is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterAccessorAttributeIsReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        [DebuggerStepThrough]
                                        {|#0:// note|}
                                        get
                                        {
                                            return 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between an attribute and a type parameter is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterTypeParameterAttributeIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass<[Marker] {|#0:/* key */|} TKey>
                                {
                                }

                                [AttributeUsage(AttributeTargets.GenericParameter)]
                                internal sealed class MarkerAttribute : Attribute
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between an attribute and a local function is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterLocalFunctionAttributeIsReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        [DebuggerStepThrough]
                                        {|#0:// note|}
                                        int Local() => 1;

                                        _ = Local();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between an attribute and a lambda is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterLambdaAttributeIsReported()
    {
        const string testData = """
                                using System;
                                using System.Diagnostics;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Func<int> function = [DebuggerStepThrough] {|#0:/* note */|} () => 1;
                                        _ = function;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment after a module attribute is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-002")]
    public async Task VerifyNoDiagnosticForCommentAfterModuleAttribute()
    {
        const string testData = """
                                using System;

                                [module: CLSCompliant(false)]

                                // note
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty block comment after an attribute is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-001")]
    [Requirement("RH5033-001")]
    public async Task VerifyEmptyBlockCommentAfterAttributeIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    {|#0:/**/|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty block comment after an attribute is reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-COMMENT-001")]
    [Requirement("RH5033-001")]
    public async Task VerifyEmptyBlockCommentAfterAttributeWithoutDocumentationParsingIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    {|#0:/**/|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between an attribute and a method is reported with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    public async Task VerifyCommentAfterAttributeIsReportedWithCrLfLineEndings()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    {|#0:// note|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData.ReplaceLineEndings("\r\n"), Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment starting with four slashes after an attribute is reported when documentation comments are parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5033-001")]
    [Requirement("SHARED-COMMENT-001")]
    public async Task VerifyQuadrupleSlashCommentAfterAttributeIsReported()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    {|#0://// note|}
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5033MessageFormat));
    }

    #endregion // Tests
}