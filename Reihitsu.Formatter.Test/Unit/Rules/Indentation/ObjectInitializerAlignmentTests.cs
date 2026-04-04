using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Tests for <see cref="IndentationAndAlignmentRule"/> — object-initializer alignment
/// </summary>
[TestClass]
public class ObjectInitializerAlignmentTests
{
    #region Methods

    /// <summary>
    /// Verifies that an object creation without an initializer remains unchanged.
    /// </summary>
    [TestMethod]
    public void NoInitializerRemainsUnchanged()
    {
        // Arrange
        const string input = """
        var x = new Foo();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that a multi-line object initializer aligns braces to the <c>new</c> keyword
    /// column and assignments are indented by +4.
    /// </summary>
    [TestMethod]
    public void MultiLineInitializerAlignsToNewKeyword()
    {
        // Arrange
        const string input = """
        var x = new Foo
                  {
                            A = 1
                  };
        """;

        const string expected = """
        var x = new Foo
                {
                    A = 1
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that multiple assignments in an object initializer remain aligned,
    /// including fluent-chain continuation lines.
    /// </summary>
    [TestMethod]
    public void MultipleAssignmentsWithFluentChainAlignCorrectly()
    {
        // Arrange
        const string input = """
        var list = new List<string>();
        var x = new Foo
        {
            A = "123",
               B = "123",
               C = list.Where(s => s == "123")
                 .FirstOrDefault(),
               D = "123"
        };
        """;

        const string expected = """
        var list = new List<string>();
        var x = new Foo
                {
                    A = "123",
                    B = "123",
                    C = list.Where(s => s == "123")
                            .FirstOrDefault(),
                    D = "123"
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that an empty object initializer has its braces realigned to the <c>new</c> keyword column.
    /// </summary>
    [TestMethod]
    public void EmptyInitializerBracesAlignToNewKeyword()
    {
        // Arrange — multi-line empty initializer with wrong brace positions
        const string input = """
        var x = new Foo
                     {
                     };
        """;

        const string expected = """
        var x = new Foo
                {

                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a collection initializer is reformatted with proper indentation.
    /// </summary>
    [TestMethod]
    public void CollectionInitializerIsReformatted()
    {
        // Arrange
        const string input = """
        var x = new List<int> { 1, 2, 3 };
        """;

        const string expected = """
        var x = new List<int>         {
                    1,
                    2,
                    3
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that an indented object creation (column > 0) aligns correctly.
    /// The combined rule normalizes block indentation, so the global statement
    /// moves to column 0 and the initializer aligns relative to the <c>new</c> keyword.
    /// </summary>
    [TestMethod]
    public void IndentedObjectCreationAlignsCorrectly()
    {
        // Arrange — new at column 12
        const string input = """
            var x = new Foo
                         {
                                   A = 1
                         };
        """;

        const string expected = """
        var x = new Foo
                {
                    A = 1
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a multi-line assignment with a method chain inside an
    /// object initializer keeps chain continuation alignment relative to the assignment.
    /// </summary>
    [TestMethod]
    public void MultilineAssignmentAlignsCorrectly()
    {
        // Arrange
        const string input = """
        var l = new List<string>();
        var x = new Foo
        {
        A = 1,
        B = l.Where(s => s.Length > 3)
        .FirstOrDefault()
        };
        """;

        const string expected = """
        var l = new List<string>();
        var x = new Foo
                {
                    A = 1,
                    B = l.Where(s => s.Length > 3)
                         .FirstOrDefault()
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a nested initializer inside another initializer aligns correctly.
    /// </summary>
    [TestMethod]
    public void NestedInitializerAlignsCorrectly()
    {
        // Arrange — outer new at col 8, inner new at col 18 after formatting
        const string input = """
        var x = new Foo
                  {
                       Bar = new Baz
                                 {
                                      C = 3
                                 }
                  };
        """;

        const string expected = """
        var x = new Foo
                {
                    Bar = new Baz
                          {
                              C = 3
                          }
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that an already correctly aligned initializer is not modified.
    /// </summary>
    [TestMethod]
    public void AlreadyCorrectLayoutNoChange()
    {
        // Arrange — new at col 8, brace at col 8, assignment at col 12
        const string input = """
        var x = new Foo
                {
                    A = 1
                };
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that the rule reports <see cref="FormattingPhase.Indentation"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsIndentation()
    {
        // Arrange
        var context = new FormattingContext("\n");
        var rule = new IndentationAndAlignmentRule(context, CancellationToken.None);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.Indentation, phase);
    }

    #endregion // Methods

    #region Helper

    /// <summary>
    /// Normalizes line endings in a string to LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The text with LF line endings.</returns>
    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Applies the <see cref="IndentationAndAlignmentRule"/> to the given input.
    /// </summary>
    /// <param name="input">The source code to format.</param>
    /// <returns>The formatted source code.</returns>
    private static string ApplyRule(string input)
    {
        input = Normalize(input);

        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext("\n");
        var rule = new IndentationAndAlignmentRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Helper
}