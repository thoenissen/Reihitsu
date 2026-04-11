using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="GenericConstraintContributor"/>
/// </summary>
[TestClass]
public class GenericConstraintContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the where keyword is indented one level from the parent declaration.
    /// </summary>
    [TestMethod]
    public void AlignsWhereKeywordToParentPlusIndent()
    {
        // Arrange
        const string input = """
            class C<T>
                where T : class
            {
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var constraint = root.DescendantNodes().OfType<TypeParameterConstraintClauseSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populate layout for the class declaration line
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var parentFirstToken = classDecl.GetFirstToken();
        var parentLine = LayoutComputer.GetLine(parentFirstToken);
        var parentColumn = LayoutComputer.GetColumn(parentFirstToken);
        model.Set(parentLine, new TokenLayout(parentColumn, "Block"));

        var contributor = new GenericConstraintContributor();
        var expectedWhereColumn = parentColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(constraint, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(constraint.WhereKeyword))
        {
            var whereLine = LayoutComputer.GetLine(constraint.WhereKeyword);
            Assert.IsTrue(model.TryGetLayout(whereLine, out var whereLayout));
            Assert.AreEqual(expectedWhereColumn, whereLayout.Column, "where keyword should be indented +4 from parent");
        }
    }

    /// <summary>
    /// Verifies that constraints on a method are also aligned to the method declaration plus one indent level.
    /// </summary>
    [TestMethod]
    public void AlignsMethodConstraintToMethodPlusIndent()
    {
        // Arrange
        const string input = """
            class C
            {
                void M<T>()
                    where T : struct
                {
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var constraint = root.DescendantNodes().OfType<TypeParameterConstraintClauseSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populate layout for the method declaration line
        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var methodFirstToken = methodDecl.GetFirstToken();
        var methodLine = LayoutComputer.GetLine(methodFirstToken);
        var methodColumn = LayoutComputer.GetColumn(methodFirstToken);
        model.Set(methodLine, new TokenLayout(methodColumn, "Block"));

        var contributor = new GenericConstraintContributor();
        var expectedWhereColumn = methodColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(constraint, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(constraint.WhereKeyword))
        {
            var whereLine = LayoutComputer.GetLine(constraint.WhereKeyword);
            Assert.IsTrue(model.TryGetLayout(whereLine, out var whereLayout));
            Assert.AreEqual(expectedWhereColumn, whereLayout.Column);
        }
    }

    /// <summary>
    /// Verifies that no layout is set when the parent declaration line has no pre-existing layout.
    /// </summary>
    [TestMethod]
    public void DoesNothingWhenParentHasNoLayout()
    {
        // Arrange
        const string input = """
            class C<T>
                where T : class
            {
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var constraint = root.DescendantNodes().OfType<TypeParameterConstraintClauseSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new GenericConstraintContributor();

        // Act — no pre-populated layout for parent
        contributor.Contribute(constraint, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Without parent layout, no constraint layout should be set");
    }

    /// <summary>
    /// Verifies that non-constraint nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonConstraintNodes()
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
        var contributor = new GenericConstraintContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-constraint nodes should not produce layout entries");
    }

    #endregion // Methods
}