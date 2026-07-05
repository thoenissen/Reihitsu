using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="ArgumentAlignmentContributor"/>
/// </summary>
[TestClass]
public class ArgumentAlignmentContributorTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that arguments spanning multiple lines are aligned to the column after the opening parenthesis
    /// </summary>
    [TestMethod]
    public void AlignsMultiLineArgumentsToOpenParen()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(a,
                                         b,
                                         c);
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(argumentList, model, context);

        // Assert
        var openParenColumn = LayoutComputer.GetColumn(argumentList.OpenParenToken) + 1;

        foreach (var argument in argumentList.Arguments)
        {
            var firstToken = argument.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout), $"Expected layout for line {line}");
                Assert.AreEqual(openParenColumn, layout.Column, $"Argument on line {line} should align to column after open paren");
            }
        }
    }

    /// <summary>
    /// Verifies that single-line argument lists do not produce any layout entries
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineArguments()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Foo(a, b, c);
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(argumentList, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line argument list should not produce layout entries");
    }

    /// <summary>
    /// Verifies that parameters spanning multiple lines are aligned to the column after the opening parenthesis
    /// </summary>
    [TestMethod]
    public void AlignsMultiLineParametersToOpenParen()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a,
                                        int b,
                                        int c)
                                 {
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var parameterList = root.DescendantNodes().OfType<ParameterListSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(parameterList, model, context);

        // Assert
        var openParenColumn = LayoutComputer.GetColumn(parameterList.OpenParenToken) + 1;

        for (var parameterIndex = 1; parameterIndex < parameterList.Parameters.Count; parameterIndex++)
        {
            var firstToken = parameterList.Parameters[parameterIndex].GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout), $"Expected layout for parameter on line {line}");
                Assert.AreEqual(openParenColumn, layout.Column);
            }
        }
    }

    /// <summary>
    /// Verifies that bracketed argument lists spanning multiple lines are aligned to the column after the opening bracket
    /// </summary>
    [TestMethod]
    public void AlignsBracketedArgumentList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = arr[a,
                                                 b];
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var bracketedList = root.DescendantNodes().OfType<BracketedArgumentListSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(bracketedList, model, context);

        // Assert
        var openBracketColumn = LayoutComputer.GetColumn(bracketedList.OpenBracketToken) + 1;
        var secondArg = bracketedList.Arguments[1].GetFirstToken();

        if (LayoutComputer.IsFirstOnLine(secondArg))
        {
            var line = LayoutComputer.GetLine(secondArg);

            Assert.IsTrue(model.TryGetLayout(line, out var layout));
            Assert.AreEqual(openBracketColumn, layout.Column);
        }
    }

    /// <summary>
    /// Verifies that non-matching node types are ignored by the contributor
    /// </summary>
    [TestMethod]
    public void IgnoresNonArgumentNodes()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(classDecl, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-argument nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that attribute arguments spanning multiple lines are aligned to the column after the opening parenthesis
    /// </summary>
    [TestMethod]
    public void AlignsAttributeArguments()
    {
        // Arrange
        const string input = """
                             [MyAttr(A,
                                     B,
                                     C)]
                             class C
                             {
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var attrArgList = root.DescendantNodes().OfType<AttributeArgumentListSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(attrArgList, model, context);

        // Assert
        var openParenColumn = LayoutComputer.GetColumn(attrArgList.OpenParenToken) + 1;
        var secondArg = attrArgList.Arguments[1].GetFirstToken();

        if (LayoutComputer.IsFirstOnLine(secondArg))
        {
            var line = LayoutComputer.GetLine(secondArg);

            Assert.IsTrue(model.TryGetLayout(line, out var layout));
            Assert.AreEqual(openParenColumn, layout.Column);
        }
    }

    /// <summary>
    /// Verifies that a multi-line dictionary indexer key (an <see cref="ImplicitElementAccessSyntax"/>
    /// inside a collection initializer) indents the key body one level deeper than the
    /// opening bracket and aligns the closing bracket with the opening bracket
    /// </summary>
    [TestMethod]
    public void AlignsMultiLineDictionaryIndexerKey()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class Data
                             {
                                 public int A { get; set; }
                             }

                             class C
                             {
                                 void M()
                                 {
                                     var v = new Dictionary<Data, string>()
                                             {
                                                 [
                                                     new Data
                                                     {
                                                         A = 1
                                                     }
                                                 ] = "x"
                                             };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var bracketedList = root.DescendantNodes()
                                .OfType<BracketedArgumentListSyntax>()
                                .First(static list => list.Parent is ImplicitElementAccessSyntax);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        var openColumn = LayoutComputer.GetColumn(bracketedList.OpenBracketToken);

        // Act
        contributor.Contribute(bracketedList, model, context);

        // Assert
        var keyFirstToken = bracketedList.Arguments[0].GetFirstToken();
        var keyLine = LayoutComputer.GetLine(keyFirstToken);

        Assert.IsTrue(LayoutComputer.IsFirstOnLine(keyFirstToken), "Key body should be first on its line");
        Assert.IsTrue(model.TryGetLayout(keyLine, out var keyLayout), "Expected layout for key body line");
        Assert.AreEqual(openColumn + FormattingContext.IndentSize, keyLayout.Column, "Key body should be indented one level deeper than open bracket");

        var closeLine = LayoutComputer.GetLine(bracketedList.CloseBracketToken);

        Assert.IsTrue(LayoutComputer.IsFirstOnLine(bracketedList.CloseBracketToken), "Close bracket should be first on its line");
        Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout), "Expected layout for close bracket line");
        Assert.AreEqual(openColumn, closeLayout.Column, "Close bracket should align with open bracket");
    }

    /// <summary>
    /// Verifies that a single-line dictionary indexer key does not produce any layout entries
    /// so that simple keys remain unchanged
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineDictionaryIndexerKey()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var v = new Dictionary<string, int>()
                                             {
                                                 ["abc"] = 1
                                             };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var bracketedList = root.DescendantNodes()
                                .OfType<BracketedArgumentListSyntax>()
                                .First(static list => list.Parent is ImplicitElementAccessSyntax);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(bracketedList, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line dictionary indexer key should not produce layout entries");
    }

    /// <summary>
    /// Verifies that tuple elements in a method call argument produce alignment layouts
    /// </summary>
    [TestMethod]
    public void AlignsTupleArgumentElements()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(Dictionary<string, (List<string>, bool)> dict)
                                 {
                                     dict.Add("key",
                                         (new List<string> { "a", "b" },
                                         true));
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Find the argument list of the Add call
        var argList = root.DescendantNodes().OfType<ArgumentListSyntax>().Last();

        // Act
        contributor.Contribute(argList, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce alignment layouts for tuple argument elements");
    }

    #endregion // Methods
}