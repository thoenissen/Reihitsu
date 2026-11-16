using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Model;

using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Test execution for <see cref="AnalyzerTestsBase{TAnalyzer}"/>. Besides the checks of the testing library, it fails a
/// test whose fixed state, or Fix All fixed state, holds a warning-wave diagnostic the source state does not hold
/// (<see cref="WarningWaveDiagnostics"/>); every code-fix test reaches the check, also one that constructs this
/// verifier directly
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
/// <typeparam name="TCodeFix">Type of the code fix</typeparam>
public class CSharpCodeFixVerifierTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public CSharpCodeFixVerifierTest()
    {
        SolutionTransforms.Add(OnTransform);
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Added transforms to the solution
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Transformed solution</returns>
    private static Solution OnTransform(Solution solution, ProjectId projectId)
    {
        if (solution != null)
        {
            var project = solution.GetProject(projectId);

            if (project?.ParseOptions is CSharpParseOptions parseOptions)
            {
                solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.Latest));
            }

            if (project?.CompilationOptions != null)
            {
                solution = solution.WithProjectCompilationOptions(projectId, project.CompilationOptions.WithSpecificDiagnosticOptions(project.CompilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.GetNullableWarnings())));
            }
        }

        return solution;
    }

    /// <summary>
    /// Determines whether two states hold the same sources, so the second needs no check of its own
    /// </summary>
    /// <param name="first">First state</param>
    /// <param name="second">Second state</param>
    /// <returns><see langword="true"/> when both states hold the same sources in every project</returns>
    private static bool HaveSameSources(SolutionState first, SolutionState second)
    {
        return HaveSameSources((ProjectState)first, second)
               && first.AdditionalProjects.Count == second.AdditionalProjects.Count
               && first.AdditionalProjects.All(pair => second.AdditionalProjects.TryGetValue(pair.Key, out var project)
                                                       && HaveSameSources(pair.Value, project));
    }

    /// <summary>
    /// Determines whether two projects hold the same sources
    /// </summary>
    /// <param name="first">First project</param>
    /// <param name="second">Second project</param>
    /// <returns><see langword="true"/> when both projects hold the same sources</returns>
    private static bool HaveSameSources(ProjectState first, ProjectState second)
    {
        return first.Sources.Count == second.Sources.Count
               && first.Sources.Zip(second.Sources).All(pair => pair.First.filename == pair.Second.filename && pair.First.content.ToString() == pair.Second.content.ToString());
    }

    /// <summary>
    /// Fails the test when its fixed state or its Fix All fixed state holds a warning-wave diagnostic its source state
    /// does not hold. The states are evaluated as the testing library evaluates them for its own checks, after those
    /// checks passed, so the fixed states hold exactly the text the code fix produced
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task VerifyNoWarningWaveDiagnosticIsIntroducedAsync(CancellationToken cancellationToken)
    {
        var analyzers = GetDiagnosticAnalyzers().ToArray();
        var defaultDiagnostic = GetDefaultDiagnostic(analyzers);
        var supportedDiagnostics = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics).ToImmutableArray();
        var fixableDiagnostics = GetCodeFixProviders().SelectMany(provider => provider.FixableDiagnosticIds).ToImmutableArray();
        var testState = TestState.WithInheritedValuesApplied(null, fixableDiagnostics)
                                 .WithProcessedMarkup(MarkupOptions, defaultDiagnostic, supportedDiagnostics, fixableDiagnostics, DefaultFilePath);
        var fixedState = FixedState.WithInheritedValuesApplied(testState, fixableDiagnostics)
                                   .WithProcessedMarkup(MarkupOptions, defaultDiagnostic, supportedDiagnostics, fixableDiagnostics, DefaultFilePath);
        var batchFixedState = BatchFixedState.WithInheritedValuesApplied(fixedState, fixableDiagnostics)
                                             .WithProcessedMarkup(MarkupOptions, defaultDiagnostic, supportedDiagnostics, fixableDiagnostics, DefaultFilePath);
        var checksFixedState = HaveSameSources(testState, fixedState) == false;
        var checksBatchFixedState = CodeFixTestBehaviors.HasFlag(CodeFixTestBehaviors.SkipFixAllCheck) == false
                                    && HaveSameSources(testState, batchFixedState) == false
                                    && (checksFixedState == false || HaveSameSources(fixedState, batchFixedState) == false);

        if (checksFixedState == false
            && checksBatchFixedState == false)
        {
            return;
        }

        var sourceSolution = await GetStateSolutionAsync(testState, cancellationToken).ConfigureAwait(false);

        if (checksFixedState)
        {
            await VerifyNoWarningWaveDiagnosticIsIntroducedAsync(sourceSolution, fixedState, "fixed state", cancellationToken).ConfigureAwait(false);
        }

        if (checksBatchFixedState)
        {
            await VerifyNoWarningWaveDiagnosticIsIntroducedAsync(sourceSolution, batchFixedState, "Fix All fixed state", cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Fails the test when the fixed state holds a warning-wave diagnostic the source state does not hold
    /// </summary>
    /// <param name="sourceSolution">Solution of the source state</param>
    /// <param name="fixedState">Fixed state</param>
    /// <param name="stateName">Name of the fixed state in the failure message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task VerifyNoWarningWaveDiagnosticIsIntroducedAsync(Solution sourceSolution, SolutionState fixedState, string stateName, CancellationToken cancellationToken)
    {
        var fixedSolution = await GetStateSolutionAsync(fixedState, cancellationToken).ConfigureAwait(false);
        var introduced = await WarningWaveDiagnostics.GetIntroducedAsync(sourceSolution, fixedSolution, cancellationToken).ConfigureAwait(false);

        Verify.True(introduced.IsEmpty, WarningWaveDiagnostics.CreateMessage(stateName, introduced));
    }

    /// <summary>
    /// Creates the solution of a test state the way the testing library creates it for its own checks, with the
    /// solution transforms of the test applied
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution</returns>
    private async Task<Solution> GetStateSolutionAsync(SolutionState state, CancellationToken cancellationToken)
    {
        var additionalProjects = state.AdditionalProjects
                                      .Values
                                      .Select(project => new EvaluatedProjectState(project, ReferenceAssemblies))
                                      .ToImmutableArray();
        var project = await CreateProjectAsync(new EvaluatedProjectState(state, ReferenceAssemblies), additionalProjects, cancellationToken).ConfigureAwait(false);
        var solution = project.Solution;

        foreach (var transform in SolutionTransforms)
        {
            solution = transform(solution, project.Id);
        }

        return solution;
    }

    #endregion // Methods

    #region AnalyzerTest

    /// <inheritdoc/>
    protected override async Task RunImplAsync(CancellationToken cancellationToken)
    {
        await base.RunImplAsync(cancellationToken).ConfigureAwait(false);
        await VerifyNoWarningWaveDiagnosticIsIntroducedAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion // AnalyzerTest
}