using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

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
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:TestClass|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Type name matches the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TypeNameMatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class Test0
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Enum name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EnumNameMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test enum
                                    /// </summary>
                                    public enum {|#0:TestEnum|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Struct name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StructNameMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test struct
                                    /// </summary>
                                    public struct {|#0:TestStruct|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Delegate name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DelegateNameMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test delegate
                                    /// </summary>
                                    public delegate void {|#0:TestDelegate|}();
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Interface name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InterfaceNameMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test interface
                                    /// </summary>
                                    public interface {|#0:ITestInterface|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Record name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RecordNameMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test record
                                    /// </summary>
                                    public record {|#0:TestRecord|}();
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Code fix renames the file to match the type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixRenamesFile()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:TestClass|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace TestNamespace
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.FixedState.Sources.Add(("/0/TestClass.cs", fixedCode));
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
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:TestClass|}<T>
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace TestNamespace
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestClass<T>
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.FixedState.Sources.Add(("/0/TestClass{T}.cs", fixedCode));
                     },
                     Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Razor code-behind type name matches the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RazorCodeBehindMatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class Test0
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.razor.cs", testCode));
                     });
    }

    /// <summary>
    /// Razor code-behind type name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RazorCodeBehindMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:TestComponent|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.razor.cs", testCode));
                     },
                     Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// Code fix renames the razor code-behind file to match the type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixRenamesRazorFile()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:TestComponent|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace TestNamespace
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestComponent
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.razor.cs", testCode));
                         test.FixedState.Sources.Add(("/0/TestComponent.razor.cs", fixedCode));
                     },
                     Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }

    /// <summary>
    /// XAML code-behind type name matches the filename before the first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task XamlCodeBehindMatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class Test0
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.xaml.cs", testCode));
                     });
    }

    /// <summary>
    /// Split partial filenames only validate the segment before the first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SplitPartialFileMatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public partial class Test0
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.Part1.cs", testCode));
                     });
    }

    /// <summary>
    /// Code fix preserves the suffix after the first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixPreservesDottedFileSuffix()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public partial class {|#0:TestComponent|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace TestNamespace
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public partial class TestComponent
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Test0.Part1.cs", testCode));
                         test.FixedState.Sources.Add(("/0/TestComponent.Part1.cs", fixedCode));
                     },
                     Diagnostics(RH0201TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH0201MessageFormat));
    }
}