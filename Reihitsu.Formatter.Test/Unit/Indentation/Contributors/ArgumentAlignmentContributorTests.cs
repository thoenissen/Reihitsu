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
    /// Gets or sets the test context for the current test
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(argumentList, scope, model, context);

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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(argumentList, scope, model, context);

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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var parameterList = root.DescendantNodes().OfType<ParameterListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(parameterList, scope, model, context);

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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var bracketedList = root.DescendantNodes().OfType<BracketedArgumentListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(bracketedList, scope, model, context);

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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var attrArgList = root.DescendantNodes().OfType<AttributeArgumentListSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Act
        contributor.Contribute(attrArgList, scope, model, context);

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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ArgumentAlignmentContributor();

        // Find the argument list of the Add call
        var argList = root.DescendantNodes().OfType<ArgumentListSyntax>().Last();
        var scope = new FormattingScope(0);

        // Act
        contributor.Contribute(argList, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce alignment layouts for tuple argument elements");
    }

    #endregion // Methods
}