using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="BaseTypeListContributor"/>
/// </summary>
[TestClass]
public class BaseTypeListContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that subsequent base types are aligned to the column of the first base type
    /// </summary>
    [TestMethod]
    public void AlignsSubsequentBaseTypesToFirst()
    {
        // Arrange
        const string input = """
                             class C : IFoo,
                                       IBar,
                                       IBaz
                             {
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var baseList = root.DescendantNodes().OfType<BaseListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BaseTypeListContributor();

        var firstBaseColumn = LayoutComputer.GetColumn(baseList.Types[0].GetFirstToken());

        // Act
        contributor.Contribute(baseList, scope, model, context);

        // Assert
        for (var typeIndex = 1; typeIndex < baseList.Types.Count; typeIndex++)
        {
            var firstToken = baseList.Types[typeIndex].GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout), $"Expected layout for type {typeIndex} on line {line}");
                Assert.AreEqual(firstBaseColumn, layout.Column, $"Base type {typeIndex} should align to first base type column");
            }
        }
    }

    /// <summary>
    /// Verifies that a single base type does not produce any layout entries
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleBaseType()
    {
        // Arrange
        const string input = """
                             class C : IFoo
                             {
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var baseList = root.DescendantNodes().OfType<BaseListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BaseTypeListContributor();

        // Act
        contributor.Contribute(baseList, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single base type should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non-base-list nodes are ignored by the contributor
    /// </summary>
    [TestMethod]
    public void IgnoresNonBaseListNodes()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BaseTypeListContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-base-list nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that base types on the same line do not produce layout entries
    /// </summary>
    [TestMethod]
    public void DoesNotAlignBaseTypesOnSameLine()
    {
        // Arrange
        const string input = """
                             class C : IFoo, IBar, IBaz
                             {
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var baseList = root.DescendantNodes().OfType<BaseListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BaseTypeListContributor();

        // Act
        contributor.Contribute(baseList, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Base types all on the same line should not produce layout entries");
    }

    #endregion // Methods
}