using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7003FileScopedNamespacesShouldBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH7003FileScopedNamespacesShouldBeUsedAnalyzerTests : AnalyzerTestsBase<RH7003FileScopedNamespacesShouldBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that a single top-level block-scoped namespace is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7003-001")]
    [Requirement("RH7003-004")]
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
                     Diagnostics(RH7003FileScopedNamespacesShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7003MessageFormat));
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
    [Requirement("RH7003-002")]
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
    [Requirement("RH7003-002")]
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
    [Requirement("RH7003-003")]
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
    [Requirement("RH7003-001")]
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
                     Diagnostics(RH7003FileScopedNamespacesShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7003MessageFormat));
    }

    /// <summary>
    /// Verifying that directive-containing namespaces are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-008")]
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
                     Diagnostics(RH7003FileScopedNamespacesShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7003MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a block-scoped namespace is reported on its whole name when using
    /// directives and an assembly attribute stand outside it and it contains a using directive, a comment, and a type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7003-001")]
    [Requirement("RH7003-004")]
    public async Task NamespaceBesideUsingDirectivesAndAssemblyAttributesIsReported()
    {
        const string source = """
                              using System;

                              [assembly: CLSCompliant(true)]

                              namespace {|RH7003:A.B|}
                              {
                                  using System.Collections.Generic;

                                  // The only type
                                  class C
                                  {
                                      private List<int> _values = new List<int>();
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: a namespace with a type declared outside it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7003-002")]
    public async Task NamespaceFollowedByATypeOutsideItIsNotReported()
    {
        const string source = """
                              namespace A
                              {
                              }

                              class C
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a namespace beside a top-level statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7003-002")]
    public async Task NamespaceBesideATopLevelStatementIsNotReported()
    {
        const string source = """
                              System.Console.WriteLine();

                              namespace A
                              {
                                  class C
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.TestState.OutputKind = OutputKind.ConsoleApplication);
    }

    /// <summary>
    /// Verifies the specification example of RH7003-001 is not reported when the project uses C# 9, which has no
    /// file-scoped namespaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7003-003")]
    public async Task SpecificationExampleIsNotReportedForLanguageVersionNine()
    {
        const string source = """
                              using System;

                              [assembly: CLSCompliant(true)]

                              namespace A.B
                              {
                                  using System.Collections.Generic;

                                  // The only type
                                  class C
                                  {
                                      private List<int> _values = new List<int>();
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyCSharp9ToTestProject));

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

    #endregion // Tests
}