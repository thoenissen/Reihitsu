using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Base class for the tests of a code fix that deliberately offers no Fix All. Deriving from this base is the
/// declaration that the provider corrects one occurrence at a time
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
/// <typeparam name="TCodeFix">Type of the code fixer</typeparam>
public abstract class SingleCodeFixTestsBase<TAnalyzer, TCodeFix> : AnalyzerTestsBase<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    #region Tests

    /// <summary>
    /// Verifying the code fix does not support Fix All, so this base class is the correct one
    /// </summary>
    [TestMethod]
    public void CodeFixDoesNotProvideFixAllProvider()
    {
        Assert.IsNull(new TCodeFix().GetFixAllProvider(),
                      $"{typeof(TCodeFix).Name} supports Fix All. Derive {GetType().Name} from BatchCodeFixTestsBase instead.");
    }

    #endregion // Tests
}