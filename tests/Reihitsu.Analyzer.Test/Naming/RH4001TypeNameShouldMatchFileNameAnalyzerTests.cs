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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH4001TypeNameShouldMatchFileNameAnalyzer.DiagnosticId, MessageFormat));
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

    /// <summary>
    /// Verifies that a type name written with the verbatim prefix is compared by its value, so <c>@class</c> matches
    /// the file <c>class.cs</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task VerbatimKeywordTypeNameMatchesFileNamedAfterItsValue()
    {
        const string testCode = """
                                public class @class
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/class.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a type name written with the verbatim prefix in front of an ordinary identifier matches the file
    /// named after the identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task VerbatimTypeNameMatchesFileNamedAfterItsValue()
    {
        const string testCode = """
                                public class @Order
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
    /// Verifies that a type named <c>@class</c> is reported in a file named <c>@class.cs</c>, because the type is named
    /// <c>class</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    [Requirement("RH4001-004")]
    public async Task VerbatimTypeNameIsReportedInFileNamedWithTheVerbatimPrefix()
    {
        const string testCode = """
                                public class {|RH4001:@class|}
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/@class.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a delegate name written with the verbatim prefix is compared by its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task VerbatimDelegateNameMatchesFileNamedAfterItsValue()
    {
        const string testCode = """
                                public delegate void @Handler();
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Handler.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a type name written with a Unicode escape is compared by the character the escape stands for
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    public async Task UnicodeEscapedTypeNameMatchesFileNamedAfterItsValue()
    {
        const string testCode = """
                                public class \u0041bc
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Abc.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a type name written with a Unicode escape is reported in a file named after another type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-002")]
    [Requirement("RH4001-004")]
    public async Task UnicodeEscapedTypeNameIsReportedInFileNamedAfterAnotherType()
    {
        const string testCode = """
                                public class {|RH4001:\u0041bc|}
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a type parameter written with the verbatim prefix is compared by its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    public async Task VerbatimTypeParameterMatchesFileNamedAfterItsValue()
    {
        const string testCode = """
                                public class Cache<@TKey>
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Cache{TKey}.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a type parameter written with the verbatim prefix is reported in a file whose type-parameter list
    /// keeps the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    public async Task VerbatimTypeParameterIsReportedInFileNamedWithTheVerbatimPrefix()
    {
        const string testCode = """
                                public class {|RH4001:Cache|}<@TKey>
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Cache{@TKey}.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a type parameter written with a Unicode escape is compared by the character the escape stands for
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    public async Task UnicodeEscapedTypeParameterMatchesFileNamedAfterItsValue()
    {
        const string testCode = """
                                public class Cache<\u0054Key>
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Cache{TKey}.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that the code fix derives the new file name from the value of a type name written with the verbatim
    /// prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesFileAfterValueOfVerbatimTypeName()
    {
        const string testCode = """
                                public class {|RH4001:@Order|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class @Order
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
    /// Verifies that the code fix renames a file named with the verbatim prefix to the value of the keyword type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesFileWithVerbatimPrefixAfterValueOfKeywordTypeName()
    {
        const string testCode = """
                                public class {|RH4001:@class|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class @class
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/@class.cs", testCode));
                         test.FixedState.Sources.Add(("/0/class.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Verifies that the code fix derives the type-parameter part of the new file name from the value of a type
    /// parameter written with the verbatim prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesFileAfterValueOfVerbatimTypeParameter()
    {
        const string testCode = """
                                public class {|RH4001:Cache|}<@TKey>
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class Cache<@TKey>
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Cache{TKey}.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Verifies that the code fix replaces a Unicode escape in the type name by the character it stands for
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    public async Task CodeFixRenamesFileAfterValueOfUnicodeEscapedTypeName()
    {
        const string testCode = """
                                public class {|RH4001:\u0041bc|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 public class \u0041bc
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Invoice.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Abc.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Verifies that the code fix is withheld when a document in the same folder already has the file name derived
    /// from the value of a type name written with the verbatim prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-006")]
    public async Task NoFixIsOfferedWhenDocumentWithFileNameOfVerbatimTypeNameValueExists()
    {
        const string testCode = """
                                namespace Sales
                                {
                                    public class {|RH4001:@Order|}
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
    /// Verifies that a file with top-level statements is not reported, although the type declared after them does not match the file name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-007")]
    public async Task FileWithTopLevelStatementsIsNotReported()
    {
        const string testCode = """
                                using System;

                                Console.WriteLine(Helper.Greeting);

                                internal class Helper
                                {
                                    public const string Greeting = "Hello";
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.OutputKind = OutputKind.ConsoleApplication;
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Program.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a file with a top-level statement and a top-level local function is not reported, although the type declared in a namespace after them does not match the file name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-007")]
    public async Task FileWithTopLevelLocalFunctionAndNamespacedTypeIsNotReported()
    {
        const string testCode = """
                                var total = Add(1, 2);

                                static int Add(int left, int right) => left + right;

                                namespace Tools
                                {
                                    internal class Calculator
                                    {
                                    }
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.OutputKind = OutputKind.ConsoleApplication;
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Program.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a file whose statements stand only inside a method of its first type, and not at the top level, is not reported when that type matches the file name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-007")]
    public async Task FileWithStatementsOnlyInsideMainAndMatchingFirstTypeIsNotReported()
    {
        const string testCode = """
                                internal static class Program
                                {
                                    private static void Main()
                                    {
                                        System.Console.WriteLine();
                                    }
                                }

                                internal class Helper
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         test.TestState.OutputKind = OutputKind.ConsoleApplication;
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Program.cs", testCode));
                     });
    }

    /// <summary>
    /// Verifies that a file whose statements stand only inside a method of a type is checked, so a mismatched first type is reported and the file is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-007")]
    public async Task FileWithStatementsOnlyInsideTypeMemberIsReportedAndFileIsRenamed()
    {
        const string testCode = """
                                internal static class {|RH4001:Program|}
                                {
                                    private static void Main()
                                    {
                                        System.Console.WriteLine();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal static class Program
                                 {
                                     private static void Main()
                                     {
                                         System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.OutputKind = OutputKind.ConsoleApplication;
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Startup.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Program.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Verifies that a file without top-level statements is reported and renamed also when another file of the project holds top-level statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-007")]
    public async Task FileWithoutTopLevelStatementsIsReportedWhenAnotherFileHoldsThem()
    {
        const string programCode = """
                                   System.Console.WriteLine(new Helper());
                                   """;

        const string testCode = """
                                internal class {|RH4001:Helper|}
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Helper
                                 {
                                 }
                                 """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.OutputKind = OutputKind.ConsoleApplication;
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/Program.cs", programCode));
                         test.TestState.Sources.Add(("/0/Startup.cs", testCode));
                         test.FixedState.Sources.Add(("/0/Program.cs", programCode));
                         test.FixedState.Sources.Add(("/0/Helper.cs", fixedCode));
                     });
    }

    /// <summary>
    /// Verifies that the code fix keeps the 14-digit timestamp prefix of a migration file and replaces only the name behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-008")]
    public async Task CodeFixKeepsTimestampPrefix()
    {
        await VerifyRename("public class {|RH4001:SomeMigration|}", "public class SomeMigration", "20210501083748_InitialCreate.cs", "20210501083748_SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that the code fix keeps the timestamp prefix and the part of the file name from its first dot
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-008")]
    public async Task CodeFixKeepsTimestampPrefixAndDottedFileSuffix()
    {
        await VerifyRename("public partial class {|RH4001:SomeMigration|}", "public partial class SomeMigration", "20210501083748_InitialCreate.Part1.cs", "20210501083748_SomeMigration.Part1.cs");
    }

    /// <summary>
    /// Verifies that the code fix keeps the timestamp prefix and writes the type parameters of a generic type in braces behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-003")]
    [Requirement("RH4001-008")]
    public async Task CodeFixKeepsTimestampPrefixForGenericType()
    {
        await VerifyRename("public class {|RH4001:Cache|}<T>", "public class Cache<T>", "20210501083748_InitialCreate.cs", "20210501083748_Cache{T}.cs");
    }

    /// <summary>
    /// Verifies that the code fix keeps the timestamp prefix and derives the name behind it from the value of a verbatim type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-008")]
    public async Task CodeFixKeepsTimestampPrefixAndRenamesAfterValueOfVerbatimTypeName()
    {
        await VerifyRename("public class {|RH4001:@SomeMigration|}", "public class @SomeMigration", "20210501083748_InitialCreate.cs", "20210501083748_SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that a prefix of 13 digits followed by an underscore is no timestamp prefix, so the code fix replaces the whole name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    [Requirement("RH4001-008")]
    public async Task CodeFixReplacesThirteenDigitPrefixWithTheName()
    {
        await VerifyRename("public class {|RH4001:SomeMigration|}", "public class SomeMigration", "2021050108374_InitialCreate.cs", "SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that a prefix of 15 digits is no timestamp prefix, because the 14 digits are not followed by an underscore, so the code fix replaces the whole name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    [Requirement("RH4001-008")]
    public async Task CodeFixReplacesFifteenDigitPrefixWithTheName()
    {
        await VerifyRename("public class {|RH4001:SomeMigration|}", "public class SomeMigration", "202105010837480_InitialCreate.cs", "SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that a prefix that is not all digits is no timestamp prefix, so the code fix replaces the whole name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-005")]
    [Requirement("RH4001-008")]
    public async Task CodeFixReplacesNonDigitPrefixWithTheName()
    {
        await VerifyRename("public class {|RH4001:SomeMigration|}", "public class SomeMigration", "V2021050108374_InitialCreate.cs", "SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that the code fix keeps the timestamp prefix when nothing stands behind it in the file name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-008")]
    public async Task CodeFixKeepsTimestampPrefixWithoutNameBehindIt()
    {
        await VerifyRename("public class {|RH4001:SomeMigration|}", "public class SomeMigration", "20210501083750_.cs", "20210501083750_SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that the code fix keeps only the first timestamp prefix and replaces everything behind it, a second prefix included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-008")]
    public async Task CodeFixKeepsOnlyTheFirstOfTwoTimestampPrefixes()
    {
        await VerifyRename("public class {|RH4001:SomeMigration|}", "public class SomeMigration", "20210501083751_20210501083748_X.cs", "20210501083751_SomeMigration.cs");
    }

    /// <summary>
    /// Verifies that no code fix is offered when the folder already holds a document with the new file name that keeps the timestamp prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-006")]
    public async Task NoFixIsOfferedWhenDocumentWithTheTimestampPrefixedNewFileNameExists()
    {
        const string testCode = """
                                namespace Migrations
                                {
                                    public class {|RH4001:SomeMigration|}
                                    {
                                    }
                                }
                                """;

        const string existingTargetCode = """
                                          namespace Archive
                                          {
                                              public class SomeMigration
                                              {
                                              }
                                          }
                                          """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/20210501083748_InitialCreate.cs", testCode));
                         test.TestState.Sources.Add(("/0/20210501083748_SomeMigration.cs", existingTargetCode));
                         test.FixedState.Sources.Add(("/0/20210501083748_InitialCreate.cs", testCode));
                         test.FixedState.Sources.Add(("/0/20210501083748_SomeMigration.cs", existingTargetCode));
                     });
    }

    /// <summary>
    /// Verifies that a document named after the type without the timestamp prefix does not withhold the code fix, which keeps the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4001-006")]
    [Requirement("RH4001-008")]
    public async Task FixIsOfferedWhenOnlyDocumentWithTheUnprefixedNameExists()
    {
        const string testCode = """
                                namespace Migrations
                                {
                                    public class {|RH4001:SomeMigration|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Migrations
                                 {
                                     public class SomeMigration
                                     {
                                     }
                                 }
                                 """;

        const string existingCode = """
                                    namespace Archive
                                    {
                                        public class SomeMigration
                                        {
                                        }
                                    }
                                    """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(("/0/20210501083748_InitialCreate.cs", testCode));
                         test.TestState.Sources.Add(("/0/SomeMigration.cs", existingCode));
                         test.FixedState.Sources.Add(("/0/SomeMigration.cs", existingCode));
                         test.FixedState.Sources.Add(("/0/20210501083748_SomeMigration.cs", fixedCode));
                     });
    }

    #endregion // Tests

    #region Helpers

    /// <summary>
    /// Verifies that the code fix renames a file that declares one type with an empty body, and leaves its content unchanged
    /// </summary>
    /// <param name="declaration">The type declaration without its body, with the expected diagnostic marked</param>
    /// <param name="fixedDeclaration">The same declaration without the markup</param>
    /// <param name="fileName">The file name before the fix</param>
    /// <param name="newFileName">The file name the fix is expected to produce</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyRename(string declaration, string fixedDeclaration, string fileName, string newFileName)
    {
        var testCode = $$"""
                         {{declaration}}
                         {
                         }
                         """;

        var fixedCode = $$"""
                          {{fixedDeclaration}}
                          {
                          }
                          """;

        await Verify(testCode,
                     null,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add(($"/0/{fileName}", testCode));
                         test.FixedState.Sources.Add(($"/0/{newFileName}", fixedCode));
                     });
    }

    #endregion // Helpers
}