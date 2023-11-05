using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Test execution for <see cref="CSharpAnalyzerVerifier{TAnalyzer}"/>
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public class CSharpAnalyzerVerifierTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public CSharpAnalyzerVerifierTest()
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
    private Solution OnTransform(Solution solution, ProjectId projectId)
    {
        if (solution != null)
        {
            var project = solution.GetProject(projectId);

            if (project?.CompilationOptions != null)
            {
                solution = solution.WithProjectCompilationOptions(projectId, project.CompilationOptions.WithSpecificDiagnosticOptions(project.CompilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.GetNullableWarnings())));
            }
        }

        return solution;
    }

    #endregion // Methods
}