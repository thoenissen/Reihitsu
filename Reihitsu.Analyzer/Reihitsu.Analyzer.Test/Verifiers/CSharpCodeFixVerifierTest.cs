using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Reihitsu.Analyzer.Test.Verifiers
{
    /// <summary>
    /// Test execution for <see cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix}"/>
    /// </summary>
    /// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
    /// <typeparam name="TCodeFix">Type of the code fix</typeparam>
    public class CSharpCodeFixVerifierTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
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
}