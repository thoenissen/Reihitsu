using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests that the structural transforms outside the accessor and get-only member shapes also apply through the
/// node-level entry points of <see cref="ReihitsuFormatter"/>
/// </summary>
[TestClass]
public class NodeLevelStructuralTransformTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Source carrying an unbraced <c>if</c> body, an expression-bodied method, an empty class, an enum with a
    /// trailing comma, and a combined field declaration
    /// </summary>
    private const string Input = """
                                 class C
                                 {
                                     private int _a, _b;

                                     int Get() => 42;

                                     void M(bool flag)
                                     {
                                         if (flag)
                                             Get();
                                     }

                                     class Empty
                                     {
                                     }

                                     enum E
                                     {
                                         A,
                                         B,
                                     }
                                 }
                                 """;

    /// <summary>
    /// Expected node-level result for <see cref="Input"/>
    /// </summary>
    private const string Expected = """
                                    class C
                                    {
                                        private int _a;
                                        private int _b;

                                        int Get()
                                        {
                                            return 42;
                                        }

                                        void M(bool flag)
                                        {
                                            if (flag)
                                            {
                                                Get();
                                            }
                                        }

                                        class Empty;

                                        enum E
                                        {
                                            A,
                                            B
                                        }
                                    }
                                    """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/> applies the brace, expression-body, empty-type,
    /// trailing-comma, and field-split transforms to the node it formats
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-016")]
    public void FormatNodeAppliesTheOtherStructuralTransforms()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = NormalizeLineEndings(Input, endOfLine);
            var expected = NormalizeLineEndings(Expected, endOfLine);
            var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            // Act
            var result = ReihitsuFormatter.FormatNode(classDeclaration, cancellationToken: TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(expected, result.ToFullString(), $"Node-level formatting must apply the other structural transforms under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> applies the brace, expression-body,
    /// empty-type, trailing-comma, and field-split transforms to the node it formats
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-016")]
    public async Task FormatNodeInDocumentAsyncAppliesTheOtherStructuralTransforms()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = NormalizeLineEndings(Input, endOfLine);
            var expected = NormalizeLineEndings(Expected, endOfLine);

            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(input));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var classDeclaration = root?.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

                Assert.IsNotNull(classDeclaration, "Expected a class declaration in the test document.");

                // Act
                var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, classDeclaration, TestContext.CancellationToken);
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                // Assert
                Assert.AreEqual(expected, resultText, $"Node-level formatting must apply the other structural transforms under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Methods
}