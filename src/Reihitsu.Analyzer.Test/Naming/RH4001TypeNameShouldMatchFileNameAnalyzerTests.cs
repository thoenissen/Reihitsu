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
public class RH4001TypeNameShouldMatchFileNameAnalyzerTests : SingleCodeFixTestsBase<RH4001TypeNameShouldMatchFileNameAnalyzer, RH4001TypeNameShouldMatchFileNameCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Type name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-001")]
    [Requirement("RH4001-002")]
    [Requirement("RH4001-004")]
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
    [Requirement("RH4001-002")]
    public async Task TypeNameMatch()
    {
        const string testCode = """
                                public class Order
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Order.cs", testCode));
                     });
    }

    /// <summary>
    /// Enum name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-001")]
    [Requirement("RH4001-004")]
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
    [Requirement("RH4001-001")]
    [Requirement("RH4001-004")]
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
    [Requirement("RH4001-001")]
    [Requirement("RH4001-004")]
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
    [Requirement("RH4001-001")]
    [Requirement("RH4001-004")]
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
    [Requirement("RH4001-001")]
    [Requirement("RH4001-004")]
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
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesFile()
    {
        const string testCode = """
                                public class {|RH4001:Order|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class Order
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Order.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Code fix renames the file for a generic type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesFileForGenericType()
    {
        const string testCode = """
                                public class {|RH4001:Order|}<T>
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class Order<T>
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Order{T}.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Razor code-behind type name matches the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task RazorCodeBehindMatch()
    {
        const string testCode = """
                                public class Order
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Order.razor.cs", testCode));
                     });
    }

    /// <summary>
    /// Razor code-behind type name does not match the filename
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
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
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesRazorFile()
    {
        const string testCode = """
                                public class {|RH4001:OrderView|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class OrderView
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.razor.cs", testCode));
                         test.FixedState.Sources.Add(("/0/OrderView.razor.cs", fixedCode));
                     });
    }

    /// <summary>
    /// XAML code-behind type name matches the filename before the first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task XamlCodeBehindMatch()
    {
        const string testCode = """
                                public class Order
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Order.xaml.cs", testCode));
                     });
    }

    /// <summary>
    /// Split partial filenames only validate the segment before the first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task SplitPartialFileMatch()
    {
        const string testCode = """
                                public class Order
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Order.Part1.cs", testCode));
                     });
    }

    /// <summary>
    /// Code fix preserves the suffix after the first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    public async Task CodeFixPreservesDottedFileSuffix()
    {
        const string testCode = """
                                public partial class {|RH4001:Order|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public partial class Order
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.Part1.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Order.Part1.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Timestamp-prefixed filename matches the type name after stripping the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
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
    [Requirement("RH4001-002")]
    public async Task NonDigitPrefixWithUnderscoreIsNotStripped()
    {
        const string testCode = """
                                public class V2021050108374_Order
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/V2021050108374_Order.cs", testCode));
                     });
    }

    /// <summary>
    /// Timestamp-prefixed filename does not match the type name after stripping the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
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
    [Requirement("RH4001-006")]
    public async Task CodeFixNotOfferedWhenTargetFileAlreadyExists()
    {
        const string mismatchedSource = """
                                        namespace Sales
                                        {
                                            public class Order
                                            {
                                            }
                                        }
                                        """;
        const string existingTargetSource = """
                                            namespace Archive
                                            {
                                                public class Order
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
                                    .AddDocument(mismatchedDocumentId, "Invoice.cs", SourceText.From(mismatchedSource), filePath: "/0/Invoice.cs")
                                    .AddDocument(existingTargetDocumentId, "Order.cs", SourceText.From(existingTargetSource), filePath: "/0/Order.cs");
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

    /// <summary>
    /// Verifies that an empty first namespace does not prevent discovery of a type in a later sibling namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-001")]
    [Requirement("RH4001-005")]
    public async Task LaterSiblingNamespaceTypeIsDetectedAndFileIsRenamed()
    {
        const string testCode = """
                                namespace Billing
                                {
                                }

                                namespace Sales
                                {
                                    internal class {|RH4001:Order|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Billing
                                 {
                                 }

                                 namespace Sales
                                 {
                                     internal class Order
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Order.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Verifies that only the first type of a file is compared, so a nested type and a later type do not cause a report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-001")]
    public async Task NestedAndLaterTypesAreNotComparedWithTheFileName()
    {
        const string testCode = """
                                public class Outer
                                {
                                    public class Inner
                                    {
                                    }
                                }

                                public class Customer
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Outer.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a delegate declared first is the type compared with the file name, located on its identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-001")]
    [Requirement("RH4001-004")]
    public async Task DelegateDeclaredBeforeTheMatchingTypeIsComparedWithTheFileName()
    {
        const string testCode = """
                                public delegate void {|RH4001:Handler|}();

                                public class Order
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Order.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a file that declares only a namespace is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-001")]
    public async Task FileWithoutTypeIsNotReported()
    {
        const string testCode = """
                                namespace Sales { }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Order.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a file name that differs from the type name only in casing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task TypeNameIsComparedWithTheFileNameCaseSensitively()
    {
        const string testCode = """
                                public class {|RH4001:Order|}
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/order.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a generic type matches a file name that lists its type parameters in braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    public async Task GenericTypeMatchesFileNameWithTypeParametersInBraces()
    {
        const string testCode = """
                                public class Cache<TKey, TValue>
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Cache{TKey,TValue}.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a generic type is reported in a file named after the type without its type parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    public async Task GenericTypeIsReportedInFileNamedWithoutTypeParameters()
    {
        const string testCode = """
                                public class {|RH4001:Cache|}<TKey, TValue>
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Cache.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that the mismatch is still reported but no file rename is offered when the folder already contains a document with the new file name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-006")]
    public async Task NoFixIsOfferedWhenDocumentWithTheNewFileNameExists()
    {
        const string testCode = """
                                namespace Sales
                                {
                                    public class {|RH4001:Order|}
                                    {
                                    }
                                }
                                """;

        const string existingTargetCode = """
                                          namespace Archive
                                          {
                                              public class Order
                                              {
                                              }
                                          }
                                          """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.TestState.Sources.Add(("/0/Order.cs", existingTargetCode));
                         test.FixedState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Order.cs", existingTargetCode));
                     });
    }

    /// <summary>
    /// Verifies that the existing file names are compared case-insensitively, so no file rename is offered when the new file name exists in other casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-006")]
    public async Task NoFixIsOfferedWhenDocumentWithTheNewFileNameInOtherCasingExists()
    {
        const string testCode = """
                                namespace Sales
                                {
                                    public class {|RH4001:Order|}
                                    {
                                    }
                                }
                                """;

        const string existingTargetCode = """
                                          namespace Archive
                                          {
                                              public class order
                                              {
                                              }
                                          }
                                          """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.TestState.Sources.Add(("/0/order.cs", existingTargetCode));
                         test.FixedState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/order.cs", existingTargetCode));
                     });
    }

    /// <summary>
    /// Verifies that a document with the new file name in another folder does not withhold the fix, which renames the file in its own folder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-006")]
    [Requirement("RH4001-005")]
    public async Task FixIsOfferedWhenDocumentWithTheNewFileNameIsInAnotherFolder()
    {
        const string testCode = """
                                namespace Sales
                                {
                                    public class {|RH4001:Order|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Sales
                                 {
                                     public class Order
                                     {
                                     }
                                 }
                                 """;

        const string archivedCode = """
                                    namespace Archive
                                    {
                                        public class Order
                                        {
                                        }
                                    }
                                    """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.TestState.Sources.Add(("/0/Archive/Order.cs", archivedCode));
                         test.FixedState.Sources.Add(("/0/Archive/Order.cs", archivedCode));
                         test.FixedState.Sources.Add(("/0/Order.cs", fixedCode));
                     });
    }

    #endregion // Tests
}