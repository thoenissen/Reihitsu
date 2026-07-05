using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="CaseWhenClauseContributor"/>
/// </summary>
[TestClass]
public class CaseWhenClauseContributorTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a wrapped guard keyword is indented one level past the case keyword
    /// </summary>
    [TestMethod]
    public void AlignsWrappedWhenKeywordToCasePlusIndent()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object value)
                                 {
                                     switch (value)
                                     {
                                         case int n
                                             when n > 0:
                                             break;
                                     }
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var label = root.DescendantNodes().OfType<CasePatternSwitchLabelSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        var caseLine = LayoutComputer.GetLine(label.Keyword);
        var caseColumn = LayoutComputer.GetColumn(label.Keyword);

        model.Set(caseLine, new TokenLayout(caseColumn, "Block"));

        var contributor = new CaseWhenClauseContributor();
        var expectedWhenColumn = caseColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(label, model, context);

        // Assert
        var whenLine = LayoutComputer.GetLine(label.WhenClause.WhenKeyword);

        Assert.IsTrue(model.TryGetLayout(whenLine, out var whenLayout));
        Assert.AreEqual(expectedWhenColumn, whenLayout.Column, "when keyword should be indented +4 from the case keyword");
    }

    /// <summary>
    /// Verifies that an inline guard keyword produces no layout entry because it is not first on its line
    /// </summary>
    [TestMethod]
    public void DoesNothingForInlineGuard()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object value)
                                 {
                                     switch (value)
                                     {
                                         case int n when n > 0:
                                             break;
                                     }
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var label = root.DescendantNodes().OfType<CasePatternSwitchLabelSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CaseWhenClauseContributor();

        // Act
        contributor.Contribute(label, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "An inline guard keyword should not produce a layout entry");
    }

    /// <summary>
    /// Verifies that a case label without a guard clause is ignored
    /// </summary>
    [TestMethod]
    public void IgnoresCaseLabelWithoutGuard()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object value)
                                 {
                                     switch (value)
                                     {
                                         case int n:
                                             break;
                                     }
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var label = root.DescendantNodes().OfType<CasePatternSwitchLabelSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CaseWhenClauseContributor();

        // Act
        contributor.Contribute(label, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "A case label without a guard should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non case pattern label nodes are ignored by the contributor
    /// </summary>
    [TestMethod]
    public void IgnoresNonCaseLabelNodes()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object value)
                                 {
                                     switch (value)
                                     {
                                         case int n when n > 0:
                                             break;
                                     }
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var switchStatement = root.DescendantNodes().OfType<SwitchStatementSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CaseWhenClauseContributor();

        // Act
        contributor.Contribute(switchStatement, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non case pattern label nodes should not produce layout entries");
    }

    #endregion // Methods
}