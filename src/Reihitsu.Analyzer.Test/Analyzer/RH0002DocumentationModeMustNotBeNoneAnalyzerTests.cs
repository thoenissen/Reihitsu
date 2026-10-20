using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Analyzer;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Analyzer;

/// <summary>
/// Tests for <see cref="RH0002DocumentationModeMustNotBeNoneAnalyzer"/>
/// </summary>
[TestClass]
public class RH0002DocumentationModeMustNotBeNoneAnalyzerTests : AnalyzerTestsBase<RH0002DocumentationModeMustNotBeNoneAnalyzer>
{
    #region Tests

    /// <summary>
    /// Documentation mode none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-001")]
    [Requirement("RH0002-002")]
    public async Task DocumentationModeNone()
    {
        const string source = """
                              {|#0:namespace|} TestNamespace;
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeNone),
                     Diagnostic(RH0002DocumentationModeMustNotBeNoneAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                          .WithMessage(AnalyzerResources.RH0002MessageFormat));
    }

    /// <summary>
    /// Documentation mode parse
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-001")]
    public async Task DocumentationModeParse()
    {
        const string source = """
                              namespace TestNamespace;
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeParse));
    }

    /// <summary>
    /// Diagnostic is reported for every syntax tree whose documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-001")]
    [Requirement("RH0002-002")]
    public async Task DiagnosticReportedForEverySyntaxTreeWithDocumentationModeNone()
    {
        await Verify("""
                     namespace PlaceholderNamespace;
                     """,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.cs",
                                                     """
                                                     {|#0:namespace|} FirstNamespace;
                                                     """));
                         test.TestState.Sources.Add(("/0/Test1.cs",
                                                     """
                                                     {|#1:namespace|} SecondNamespace;
                                                     """));
                         test.SolutionTransforms.Add(ApplyDocumentationModeNone);
                     },
                     Diagnostic(RH0002DocumentationModeMustNotBeNoneAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                          .WithMessage(AnalyzerResources.RH0002MessageFormat),
                     Diagnostic(RH0002DocumentationModeMustNotBeNoneAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                          .WithMessage(AnalyzerResources.RH0002MessageFormat));
    }

    /// <summary>
    /// Verifying that a file parsed in the documentation mode <see cref="DocumentationMode.Diagnose"/> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-001")]
    public async Task DocumentationModeDiagnose()
    {
        const string source = """
                              namespace N;

                              class C
                              {
                              }
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add((solution, projectId) => ApplyDocumentationMode(solution, projectId, DocumentationMode.Diagnose)));
    }

    /// <summary>
    /// Verifying that a file with documentation mode none is reported on its first token, also when the token is
    /// preceded by comments and a documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-001")]
    [Requirement("RH0002-002")]
    public async Task DocumentationModeNoneIsReportedOnFirstTokenAfterLeadingComments()
    {
        const string source = """
                              // File header

                              /// <summary>
                              /// Documented
                              /// </summary>
                              {|#0:class|} C
                              {
                              }
                              """;

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeNone),
                     Diagnostic(RH0002DocumentationModeMustNotBeNoneAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                          .WithMessage(AnalyzerResources.RH0002MessageFormat));
    }

    /// <summary>
    /// Verifying that a file holding only a comment is reported with the empty span after its final line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-002")]
    public async Task DocumentationModeNoneCommentsOnlyFileIsReportedAtEndOfFile()
    {
        const string source = "// Only a comment\n{|#0:|}";

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeNone),
                     Diagnostic(RH0002DocumentationModeMustNotBeNoneAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                          .WithMessage(AnalyzerResources.RH0002MessageFormat));
    }

    /// <summary>
    /// Verifying that an empty file is reported with the empty span at its start
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0002-002")]
    public async Task DocumentationModeNoneEmptyFileIsReportedAtStart()
    {
        await Verify("{|#0:|}",
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeNone),
                     Diagnostic(RH0002DocumentationModeMustNotBeNoneAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                                                                          .WithMessage(AnalyzerResources.RH0002MessageFormat));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Apply documentation mode none
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution ApplyDocumentationModeNone(Solution solution, ProjectId projectId)
    {
        return ApplyDocumentationMode(solution, projectId, DocumentationMode.None);
    }

    /// <summary>
    /// Apply documentation mode parse
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution ApplyDocumentationModeParse(Solution solution, ProjectId projectId)
    {
        return ApplyDocumentationMode(solution, projectId, DocumentationMode.Parse);
    }

    /// <summary>
    /// Apply documentation mode
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="documentationMode">Documentation mode</param>
    /// <returns>Solution</returns>
    private static Solution ApplyDocumentationMode(Solution solution, ProjectId projectId, DocumentationMode documentationMode)
    {
        var project = solution.GetProject(projectId);

        if (project?.ParseOptions is CSharpParseOptions parseOptions)
        {
            solution = solution.WithProjectParseOptions(projectId, parseOptions.WithDocumentationMode(documentationMode));
        }

        return solution;
    }

    #endregion // Methods
}