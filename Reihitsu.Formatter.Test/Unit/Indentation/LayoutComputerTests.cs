using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests for <see cref="LayoutComputer"/>
/// </summary>
[TestClass]
public class LayoutComputerTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> returns a non-empty model for a simple class.
    /// </summary>
    [TestMethod]
    public void ComputeReturnsNonEmptyModelForSimpleClass()
    {
        // Arrange
        const string input = """
            class Foo
            {
                public int Value { get; set; }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Model should contain at least one layout entry.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> assigns the correct column to class members.
    /// </summary>
    [TestMethod]
    public void ComputeAssignsCorrectColumnToClassMembers()
    {
        // Arrange
        const string input = """
            class Foo
            {
                public int Value;
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — line 0: "class" at column 0, line 2: "public" at column 4
        Assert.IsTrue(model.TryGetLayout(0, out var classLayout), "Line 0 should have a layout entry.");
        Assert.AreEqual(0, classLayout.Column, "Class keyword should be at column 0.");

        Assert.IsTrue(model.TryGetLayout(2, out var memberLayout), "Line 2 should have a layout entry.");
        Assert.AreEqual(4, memberLayout.Column, "Class member should be indented to column 4.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> assigns the correct column to nested blocks.
    /// </summary>
    [TestMethod]
    public void ComputeAssignsCorrectColumnToNestedBlock()
    {
        // Arrange
        const string input = """
            class Foo
            {
                void Bar()
                {
                    int x = 1;
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — line 4: "int" at column 8 (nested inside class + method)
        Assert.IsTrue(model.TryGetLayout(4, out var nestedLayout), "Line 4 should have a layout entry.");
        Assert.AreEqual(8, nestedLayout.Column, "Statement inside method body should be at column 8.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a file-scoped namespace correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesFileScopedNamespace()
    {
        // Arrange
        const string input = """
            namespace Test;

            class Foo
            {
                int Value;
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — file-scoped namespace does not add indentation
        Assert.IsTrue(model.TryGetLayout(2, out var classLayout), "Line 2 should have a layout entry.");
        Assert.AreEqual(0, classLayout.Column, "Class after file-scoped namespace should be at column 0.");

        Assert.IsTrue(model.TryGetLayout(4, out var memberLayout), "Line 4 should have a layout entry.");
        Assert.AreEqual(4, memberLayout.Column, "Member inside class should be at column 4.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a block-scoped namespace correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesBlockScopedNamespace()
    {
        // Arrange
        const string input = """
            namespace Test
            {
                class Foo
                {
                    int Value;
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — block-scoped namespace adds one level of indentation
        Assert.IsTrue(model.TryGetLayout(2, out var classLayout), "Line 2 should have a layout entry.");
        Assert.AreEqual(4, classLayout.Column, "Class inside block namespace should be at column 4.");

        Assert.IsTrue(model.TryGetLayout(4, out var memberLayout), "Line 4 should have a layout entry.");
        Assert.AreEqual(8, memberLayout.Column, "Member inside class inside block namespace should be at column 8.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a method body correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesMethodBody()
    {
        // Arrange
        const string input = """
            class Foo
            {
                void Bar()
                {
                    var x = 1;
                    var y = 2;
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(4, out var stmt1), "Line 4 should have a layout entry.");
        Assert.AreEqual(8, stmt1.Column, "First statement in method body should be at column 8.");

        Assert.IsTrue(model.TryGetLayout(5, out var stmt2), "Line 5 should have a layout entry.");
        Assert.AreEqual(8, stmt2.Column, "Second statement in method body should be at column 8.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles if/else blocks correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesIfElseBlocks()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        var a = 1;
                    }
                    else
                    {
                        var b = 2;
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — line 6: "var a" at column 12, line 10: "var b" at column 12
        Assert.IsTrue(model.TryGetLayout(6, out var ifBody), "Line 6 (if body) should have a layout entry.");
        Assert.AreEqual(12, ifBody.Column, "Statement inside if block should be at column 12.");

        Assert.IsTrue(model.TryGetLayout(10, out var elseBody), "Line 10 (else body) should have a layout entry.");
        Assert.AreEqual(12, elseBody.Column, "Statement inside else block should be at column 12.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles try/catch/finally blocks correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesTryCatchFinally()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    try
                    {
                        var a = 1;
                    }
                    catch
                    {
                        var b = 2;
                    }
                    finally
                    {
                        var c = 3;
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(6, out var tryBody), "Line 6 (try body) should have a layout entry.");
        Assert.AreEqual(12, tryBody.Column, "Statement inside try block should be at column 12.");

        Assert.IsTrue(model.TryGetLayout(10, out var catchBody), "Line 10 (catch body) should have a layout entry.");
        Assert.AreEqual(12, catchBody.Column, "Statement inside catch block should be at column 12.");

        Assert.IsTrue(model.TryGetLayout(14, out var finallyBody), "Line 14 (finally body) should have a layout entry.");
        Assert.AreEqual(12, finallyBody.Column, "Statement inside finally block should be at column 12.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a for loop correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesForLoop()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var x = i;
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — line 6: "var x" at column 12
        Assert.IsTrue(model.TryGetLayout(6, out var loopBody), "Line 6 (for body) should have a layout entry.");
        Assert.AreEqual(12, loopBody.Column, "Statement inside for loop body should be at column 12.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a switch statement with case sections correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesSwitchStatement()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            break;
                        default:
                            break;
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — line 6: "case 1:" at column 12
        Assert.IsTrue(model.TryGetLayout(6, out var caseLabel), "Line 6 (case label) should have a layout entry.");
        Assert.AreEqual(12, caseLabel.Column, "Case label should be at column 12.");

        // line 7: "break;" at column 16 (switch section body gets extra indent)
        Assert.IsTrue(model.TryGetLayout(7, out var caseBody), "Line 7 (case body) should have a layout entry.");
        Assert.AreEqual(16, caseBody.Column, "Statement in case section should be at column 16.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a lambda expression correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesLambdaExpression()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    Action a = () =>
                    {
                        var x = 1;
                    };
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — the lambda body should be indented
        Assert.IsTrue(model.TryGetLayout(6, out var lambdaBody), "Line 6 (lambda body) should have a layout entry.");
        Assert.IsGreaterThan(8, lambdaBody.Column, "Statement inside lambda body should be indented beyond the method body level.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles an object initializer correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesObjectInitializer()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var obj = new Foo
                              {
                                  Bar = 1,
                                  Baz = 2
                              };
                }
            }
            class Foo { public int Bar; public int Baz; }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — the initializer properties should have layout entries
        Assert.IsTrue(model.TryGetLayout(6, out var prop1), "Line 6 should have a layout entry for initializer property.");
        Assert.IsTrue(model.TryGetLayout(7, out var prop2), "Line 7 should have a layout entry for initializer property.");
        Assert.AreEqual(prop1.Column, prop2.Column, "Initializer properties should be aligned at the same column.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a method chain correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesMethodChain()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = "hello"
                            .Trim()
                            .ToUpper();
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — chained members should have layout entries
        Assert.IsTrue(model.TryGetLayout(5, out var chain1), "Line 5 should have a layout entry for chained call.");
        Assert.IsTrue(model.TryGetLayout(6, out var chain2), "Line 6 should have a layout entry for chained call.");
        Assert.AreEqual(chain1.Column, chain2.Column, "Chained method calls should be aligned at the same column.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles an empty class correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesEmptyClass()
    {
        // Arrange
        const string input = """
            class Foo
            {
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(0, out var classLine), "Line 0 should have a layout entry.");
        Assert.AreEqual(0, classLine.Column, "Class keyword should be at column 0.");

        Assert.IsTrue(model.TryGetLayout(1, out var openBrace), "Line 1 should have a layout entry.");
        Assert.AreEqual(0, openBrace.Column, "Open brace should be at column 0.");

        Assert.IsTrue(model.TryGetLayout(2, out var closeBrace), "Line 2 should have a layout entry.");
        Assert.AreEqual(0, closeBrace.Column, "Close brace should be at column 0.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a property with accessors correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesPropertyWithAccessors()
    {
        // Arrange
        const string input = """
            class Foo
            {
                public int Value
                {
                    get
                    {
                        return 1;
                    }
                    set
                    {
                        _ = value;
                    }
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — accessor keywords at column 8, accessor body statements at column 12
        Assert.IsTrue(model.TryGetLayout(4, out var getKeyword), "Line 4 (get) should have a layout entry.");
        Assert.AreEqual(8, getKeyword.Column, "Get accessor keyword should be at column 8.");

        Assert.IsTrue(model.TryGetLayout(6, out var getBody), "Line 6 (return 1) should have a layout entry.");
        Assert.AreEqual(12, getBody.Column, "Return statement in get accessor should be at column 12.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles nested classes correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesNestedClasses()
    {
        // Arrange
        const string input = """
            class Outer
            {
                class Inner
                {
                    int Value;
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(2, out var innerClass), "Line 2 (inner class) should have a layout entry.");
        Assert.AreEqual(4, innerClass.Column, "Inner class keyword should be at column 4.");

        Assert.IsTrue(model.TryGetLayout(4, out var innerMember), "Line 4 (inner member) should have a layout entry.");
        Assert.AreEqual(8, innerMember.Column, "Member of inner class should be at column 8.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> applies the base indent level offset correctly.
    /// </summary>
    [TestMethod]
    public void ComputeWithBaseIndentLevel()
    {
        // Arrange — using baseIndentLevel = 1, so everything shifts by 4 columns
        const string input = """
            class Foo
            {
                int Value;
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext("\n", baseIndentLevel: 1);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — base indent shifts everything by 4
        Assert.IsTrue(model.TryGetLayout(0, out var classLine), "Line 0 should have a layout entry.");
        Assert.AreEqual(4, classLine.Column, "Class keyword with baseIndentLevel=1 should be at column 4.");

        Assert.IsTrue(model.TryGetLayout(2, out var memberLine), "Line 2 should have a layout entry.");
        Assert.AreEqual(8, memberLine.Column, "Member with baseIndentLevel=1 should be at column 8.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a collection expression correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesCollectionExpression()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int[] items =
                    [
                        1,
                        2,
                        3
                    ];
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — collection elements should have layout entries
        Assert.IsTrue(model.TryGetLayout(6, out var elem1), "Line 6 should have a layout entry for collection element.");
        Assert.IsTrue(model.TryGetLayout(7, out var elem2), "Line 7 should have a layout entry for collection element.");
        Assert.IsGreaterThan(0, elem1.Column, "Collection elements should have non-zero indentation.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a LINQ expression correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesLinqExpression()
    {
        // Arrange
        const string input = """
            using System.Linq;
            class C
            {
                void M()
                {
                    var q = new[] { 1, 2, 3 }
                            .Where(x => x > 1)
                            .Select(x => x * 2);
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — chained LINQ calls should have layout entries
        Assert.IsTrue(model.TryGetLayout(6, out var where), "Line 6 (.Where) should have a layout entry.");
        Assert.IsTrue(model.TryGetLayout(7, out var select), "Line 7 (.Select) should have a layout entry.");
        Assert.AreEqual(8, where.Column, "Chained LINQ .Where should be at block indentation column.");
        Assert.AreEqual(0, select.Column, "Chained LINQ .Select should be aligned to the chain anchor column.");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutComputer.Compute"/> handles a binary expression that spans multiple lines correctly.
    /// </summary>
    [TestMethod]
    public void ComputeHandlesBinaryExpression()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var result = true
                                 && false
                                 || true;
                }
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var model = LayoutComputer.Compute(root, context);

        // Assert — continuation lines should have layout entries
        Assert.IsTrue(model.TryGetLayout(5, out var line5), "Line 5 (&& false) should have a layout entry.");
        Assert.IsTrue(model.TryGetLayout(6, out var line6), "Line 6 (|| true) should have a layout entry.");
        Assert.IsGreaterThan(0, line5.Column, "Binary expression continuation should have non-zero indentation.");
    }

    #endregion // Methods
}