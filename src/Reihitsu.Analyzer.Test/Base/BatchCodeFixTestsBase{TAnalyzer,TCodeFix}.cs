using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Base class for the tests of a code fix that supports Fix All. The base owns the assertions, so the derived
/// test class only supplies the scenario and cannot satisfy the requirement with an empty override
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
/// <typeparam name="TCodeFix">Type of the code fixer</typeparam>
public abstract class BatchCodeFixTestsBase<TAnalyzer, TCodeFix> : AnalyzerTestsBase<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    #region Methods

    /// <summary>
    /// Gets the scenario used to verify that the code fix corrects every diagnostic of one document
    /// </summary>
    /// <returns>The Fix All scenario</returns>
    protected abstract FixAllScenario GetFixAllScenario();

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifying the code fix corrects every diagnostic of one document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FixAllCorrectsEveryDiagnostic()
    {
        var scenario = GetFixAllScenario();

        Assert.IsNotNull(scenario, $"The Fix All scenario of {GetType().Name} must not be null.");
        Assert.IsNotNull(scenario.Expected, $"The Fix All scenario of {GetType().Name} must declare its expected diagnostics.");
        Assert.IsGreaterThanOrEqualTo(2,
                                      scenario.Expected.Length,
                                      $"The Fix All scenario of {GetType().Name} must report at least two diagnostics in one document.");
        Assert.AreNotEqual(scenario.Source,
                           scenario.FixedSource,
                           $"The Fix All scenario of {GetType().Name} must change the source.");

        await Verify(scenario.Source,
                     scenario.FixedSource,
                     test => scenario.Configure?.Invoke(test),
                     scenario.Expected);
    }

    /// <summary>
    /// Verifying the code fix supports Fix All, so this base class is the correct one
    /// </summary>
    [TestMethod]
    public void CodeFixProvidesFixAllProvider()
    {
        Assert.IsNotNull(new TCodeFix().GetFixAllProvider(),
                         $"{typeof(TCodeFix).Name} does not support Fix All. Derive {GetType().Name} from SingleCodeFixTestsBase instead.");
    }

    #endregion // Tests
}