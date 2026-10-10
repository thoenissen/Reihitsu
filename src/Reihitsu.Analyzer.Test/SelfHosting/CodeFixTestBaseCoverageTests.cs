using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.SelfHosting.Utilities;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Testing that every test class of an analyzer and its code fix declares, through its base class, whether the
/// code fix supports Fix All. <see cref="BatchCodeFixTestsBase{TAnalyzer, TCodeFix}"/> then requires a scenario
/// that corrects more than one diagnostic in one document, and <see cref="SingleCodeFixTestsBase{TAnalyzer, TCodeFix}"/>
/// records that the provider deliberately corrects one occurrence at a time
/// </summary>
[TestClass]
public class CodeFixTestBaseCoverageTests
{
    #region Methods

    /// <summary>
    /// Gets the repository-relative source path a test class is expected to live in
    /// </summary>
    /// <param name="testClassType">Test class type</param>
    /// <returns>The repository-relative source path</returns>
    private static string GetSourcePath(Type testClassType)
    {
        const string projectNamespace = "Reihitsu.Analyzer.Test";

        var subNamespace = testClassType.Namespace?.Length > projectNamespace.Length
                               ? testClassType.Namespace[projectNamespace.Length..].TrimStart('.').Replace('.', '/')
                               : string.Empty;

        return subNamespace.Length > 0
                   ? $"{projectNamespace}/{subNamespace}/{testClassType.Name}.cs"
                   : $"{projectNamespace}/{testClassType.Name}.cs";
    }

    /// <summary>
    /// Gets the name of the code-fix test base a test class has to derive from
    /// </summary>
    /// <param name="testClass">Test class metadata</param>
    /// <returns>The name of the expected code-fix test base</returns>
    private static string GetExpectedBaseName(DiscoveredCodeFixTestClass testClass)
    {
        var expectedBase = testClass.SupportsFixAll
                               ? typeof(BatchCodeFixTestsBase<,>)
                               : typeof(SingleCodeFixTestsBase<,>);

        return expectedBase.Name[..expectedBase.Name.IndexOf('`', StringComparison.Ordinal)];
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifying every code-fix test class derives from one of the two code-fix test bases, so its Fix All
    /// coverage is decided in code instead of being left to whoever writes the next test
    /// </summary>
    [TestMethod]
    public void EveryCodeFixTestClassDerivesFromACodeFixTestsBase()
    {
        var testClasses = AnalyzerMetadataDiscovery.DiscoverCodeFixTestClasses();

        Assert.IsNotEmpty(testClasses);

        var findings = testClasses.Where(testClass => testClass.CodeFixTestsBaseDefinition is null)
                                  .Select(testClass => $"{testClass.TestClassType.FullName} must derive from {GetExpectedBaseName(testClass)} ({GetSourcePath(testClass.TestClassType)})")
                                  .ToArray();

        if (findings.Length > 0)
        {
            Assert.Fail($"The following changes are required:\n\n{string.Join(Environment.NewLine, findings)}");
        }
    }

    #endregion // Tests
}