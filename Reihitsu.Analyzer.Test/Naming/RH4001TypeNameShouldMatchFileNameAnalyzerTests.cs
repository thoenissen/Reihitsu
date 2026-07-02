using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4001TypeNameShouldMatchFileNameAnalyzer"/> and <see cref="RH4001TypeNameShouldMatchFileNameCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4001TypeNameShouldMatchFileNameAnalyzerTests : AnalyzerTestsBase<RH4001TypeNameShouldMatchFileNameAnalyzer, RH4001TypeNameShouldMatchFileNameCodeFixProvider>
{
    #region Tests

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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
    }

    /// <summary>
    /// Timestamp-prefixed filename matches the type name after stripping the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TimestampPrefixedFileMatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class InitialCreate
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/20210501083748_InitialCreate.cs", testCode));
                     });
    }

    /// <summary>
    /// Filename with an underscore at position 14 but non-digit characters in the prefix is treated as a regular name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NonDigitPrefixWithUnderscoreIsNotStripped()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class A2345678901234_Something
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/A2345678901234_Something.cs", testCode));
                     });
    }

    /// <summary>
    /// Timestamp-prefixed filename does not match the type name after stripping the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task TimestampPrefixedFileMismatch()
    {
        const string testCode = """
                                namespace TestNamespace
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class {|#0:SomeMigration|}
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/20210501083748_InitialCreate.cs", testCode));
                     },
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, AnalyzerResources.RH4001MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered when the project already contains another document with the target file
    /// name, because renaming would overwrite that unrelated document on save
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixNotOfferedWhenTargetFileAlreadyExists()
    {
        const string mismatchedSource = """
                                        namespace TestNamespace
                                        {
                                            public class TestClass
                                            {
                                            }
                                        }
                                        """;
        const string existingTargetSource = """
                                            namespace OtherNamespace
                                            {
                                                public class TestClass
                                                {
                                                }
                                            }
                                            """;

        using (var workspace = new AdhocWorkspace())
        {
            var projectId = ProjectId.CreateNewId();
            var mismatchedDocumentId = DocumentId.CreateNewId(projectId);
            var existingTargetDocumentId = DocumentId.CreateNewId(projectId);
            var solution = workspace.CurrentSolution
                                    .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
                                    .AddDocument(mismatchedDocumentId, "Mismatched.cs", SourceText.From(mismatchedSource), filePath: "/0/Mismatched.cs")
                                    .AddDocument(existingTargetDocumentId, "TestClass.cs", SourceText.From(existingTargetSource), filePath: "/0/TestClass.cs");
            var document = solution.GetDocument(mismatchedDocumentId)
                               ?? throw new InvalidOperationException("Failed to resolve the mismatched test document.");
            var root = await document.GetSyntaxRootAsync(CancellationToken.None)
                           ?? throw new InvalidOperationException("Failed to parse the mismatched test document.");
            var typeDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var descriptor = new DiagnosticDescriptor(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId,
                                                      "Title",
                                                      "Message",
                                                      "Testing",
                                                      DiagnosticSeverity.Warning,
                                                      isEnabledByDefault: true);
            var diagnostic = Microsoft.CodeAnalysis.Diagnostic.Create(descriptor, typeDeclaration.Identifier.GetLocation());
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);

            await new RH4001TypeNameShouldMatchFileNameCodeFixProvider().RegisterCodeFixesAsync(context);

            Assert.IsEmpty(actions);
        }
    }

    #endregion // Tests
}