using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Unit tests for <see cref="UsingDirectiveCodeFixUtilities"/>
/// </summary>
[TestClass]
public class UsingDirectiveCodeFixUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Members

    /// <summary>
    /// Verifies that using scopes with fewer than two directives are skipped
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrganizeScopeUsingsReturnsOriginalDocumentWhenScopeHasSingleUsing()
    {
        const string input = """
                             using System;
                             internal class Example
                             {
                             }
                             """;

        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(input));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            Assert.IsNotNull(root);

            var result = await UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync(document, root, TestContext.CancellationToken);

            Assert.AreSame(document, result);
        }
    }

    /// <summary>
    /// Verifies that unsafe trivia prevents reordering
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrganizeScopeUsingsReturnsOriginalDocumentWhenReorderingIsUnsafe()
    {
        const string input = """
                             using System;
                             #pragma warning disable CS8019
                             using System.Linq;
                             internal class Example
                             {
                             }
                             """;

        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(input));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            Assert.IsNotNull(root);

            var result = await UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync(document, root, TestContext.CancellationToken);

            Assert.AreSame(document, result);
        }
    }

    /// <summary>
    /// Verifies that already organized using directives are left unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrganizeScopeUsingsReturnsOriginalDocumentWhenUsingsAreAlreadyOrganized()
    {
        const string input = """
                             using System;
                             using System.Linq;
                             internal class Example
                             {
                             }
                             """;

        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(input));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            Assert.IsNotNull(root);

            var result = await UsingDirectiveCodeFixUtilities.OrganizeScopeUsingsAsync(document, root, TestContext.CancellationToken);
            var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

            Assert.AreEqual(input, resultText);
        }
    }

    #endregion // Members
}