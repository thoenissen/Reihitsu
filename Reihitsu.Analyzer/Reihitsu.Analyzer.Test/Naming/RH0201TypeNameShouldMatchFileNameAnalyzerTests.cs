using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0201TypeNameShouldMatchFileNameAnalyzer"/> and <see cref="RH0201TypeNameShouldMatchFileNameCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0201TypeNameShouldMatchFileNameAnalyzerTests : AnalyzerTestsBase<RH0201TypeNameShouldMatchFileNameAnalyzer, RH0201TypeNameShouldMatchFileNameCodeFixProvider>
{
    /// <summary>
    /// Type name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TypeNameMismatch()
    {
        await Verify(TestData.RH0201TestData, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Type name matches the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TypeNameMatch()
    {
        await Verify(TestData.RH0201TypeNameMatch);
    }

    /// <summary>
    /// Enum name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EnumNameMismatch()
    {
        await Verify(TestData.RH0201EnumNameMismatch, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Struct name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StructNameMismatch()
    {
        await Verify(TestData.RH0201StructNameMismatch, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Delegate name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DelegateNameMismatch()
    {
        await Verify(TestData.RH0201DelegateNameMismatch, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Interface name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InterfaceNameMismatch()
    {
        await Verify(TestData.RH0201InterfaceNameMismatch, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Record name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RecordNameMismatch()
    {
        await Verify(TestData.RH0201RecordNameMismatch, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Code fix renames the file to match the type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixRenamesFile()
    {
        await Verify(TestData.RH0201TestData,
                     null,
                     test =>
                     {
                         test.FixedState.Sources.Add(("/0/TestClass.cs", TestData.RH0201ResultData));
                     },
                     Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Code fix renames the file for a generic type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixRenamesFileForGenericType()
    {
        await Verify(TestData.RH0201GenericTypeTestData,
                     null,
                     test =>
                     {
                         test.FixedState.Sources.Add(("/0/TestClass{T}.cs", TestData.RH0201GenericTypeResultData));
                     },
                     Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }
}
