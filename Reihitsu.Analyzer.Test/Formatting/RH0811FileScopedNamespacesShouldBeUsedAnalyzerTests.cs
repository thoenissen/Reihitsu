using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0811FileScopedNamespacesShouldBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0811FileScopedNamespacesShouldBeUsedAnalyzerTests : AnalyzerTestsBase<RH0811FileScopedNamespacesShouldBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that a single top-level block-scoped namespace is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleTopLevelBlockScopedNamespaceIsDetected()
    {
        const string testData = """
                                namespace {|#0:Test.Sample|}
                                {
                                    using System;
                                    using System.Collections.Generic;
                                
                                    internal class Example
                                    {
                                        private readonly List<int> _values = new List<int>();
                                
                                        internal void Add(int value)
                                        {
                                            Console.WriteLine(value);
                                            _values.Add(value);
                                        }
                                    }
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH0811FileScopedNamespacesShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0811MessageFormat));
    }

    /// <summary>
    /// Verifying that file-scoped namespaces are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFileScopedNamespaceIsNotFlagged()
    {
        const string testData = """
                                namespace Test.Sample;
                                
                                internal class Example
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that files with multiple namespaces are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleNamespacesAreNotFlagged()
    {
        const string testData = """
                                namespace First
                                {
                                }
                                
                                namespace Second
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that files with additional top-level members are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNamespaceWithAdditionalTopLevelMemberIsNotFlagged()
    {
        const string testData = """
                                namespace Test.Sample
                                {
                                }
                                
                                internal class Example
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that unsupported language versions are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUnsupportedLanguageVersionIsNotFlagged()
    {
        const string testData = """
                                namespace Test.Sample
                                {
                                    internal class Example
                                    {
                                    }
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add(ApplyCSharp9ToTestProject));

        static Solution ApplyCSharp9ToTestProject(Solution solution, ProjectId projectId)
        {
            var project = solution.GetProject(projectId);

            if (project?.ParseOptions is CSharpParseOptions parseOptions)
            {
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.CSharp9));
            }

            return solution;
        }
    }

    /// <summary>
    /// Verifying that commented namespaces are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedNamespaceIsReported()
    {
        const string testData = """
                                namespace {|#0:Test.Sample|}
                                {
                                    // Comment
                                    internal class Example
                                    {
                                    }
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH0811FileScopedNamespacesShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0811MessageFormat));
    }

    /// <summary>
    /// Verifying that directive-containing namespaces are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveContainingNamespaceIsReported()
    {
        const string testData = """
                                namespace {|#0:Test.Sample|}
                                {
                                #if DEBUG
                                    internal class Example
                                    {
                                    }
                                #endif
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH0811FileScopedNamespacesShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0811MessageFormat));
    }

    #endregion // Tests
}