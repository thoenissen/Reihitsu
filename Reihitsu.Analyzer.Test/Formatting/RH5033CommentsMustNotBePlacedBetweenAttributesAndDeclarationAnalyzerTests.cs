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
    /// Verifies that a documentation comment after an attribute is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDocumentationCommentAfterAttribute()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    /// <summary>Method</summary>
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documentation comment after an attribute is not reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDocumentationCommentAfterAttributeWithoutDocumentationParsing()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    [Obsolete]
                                    /// <summary>Method</summary>
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that a comment starting with four slashes after an attribute is reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}