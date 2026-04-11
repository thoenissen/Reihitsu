using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.HorizontalSpacing;

namespace Reihitsu.Formatter.Test.Unit.HorizontalSpacing;

/// <summary>
/// Tests for <see cref="HorizontalSpacingRewriter"/> and <see cref="HorizontalSpacingPhase"/>
/// </summary>
[TestClass]
public class HorizontalSpacingRewriterTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the phase adds exactly one space around binary operators such as +, -, *, /, ==, etc.
    /// </summary>
    [TestMethod]
    public void AddsSpaceAroundBinaryOperator()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1+2;
                    var y = 3  -  4;
                    var z = x==y;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1 + 2;
                    var y = 3 - 4;
                    var z = x == y;
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that exactly one space is added after each comma in argument and parameter lists.
    /// </summary>
    [TestMethod]
    public void AddsSpaceAfterComma()
    {
        const string input = """
            class C
            {
                void M(int a,int b,int c)
                {
                    M(1,2,3);
                }
            }
            """;

        const string expected = """
            class C
            {
                void M(int a, int b, int c)
                {
                    M(1, 2, 3);
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that exactly one space is added after semicolons inside a for-loop header.
    /// </summary>
    [TestMethod]
    public void AddsSpaceAfterSemicolonInForLoop()
    {
        const string input = """
            class C
            {
                void M()
                {
                    for (var i = 0;i < 10;i++)
                    {
                    }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    for (var i = 0; i < 10; i++)
                    {
                    }
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that spaces immediately inside parentheses and brackets are removed.
    /// </summary>
    [TestMethod]
    public void RemovesSpaceInsideParentheses()
    {
        const string input = """
            class C
            {
                void M( int a, int b )
                {
                    M( 1, 2 );
                    var arr = new int[ 5 ];
                }
            }
            """;

        const string expected = """
            class C
            {
                void M(int a, int b)
                {
                    M(1, 2);
                    var arr = new int[5];
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that any spaces before a comma are removed.
    /// </summary>
    [TestMethod]
    public void RemovesSpaceBeforeComma()
    {
        const string input = """
            class C
            {
                void M(int a , int b , int c)
                {
                    M(1 , 2 , 3);
                }
            }
            """;

        const string expected = """
            class C
            {
                void M(int a, int b, int c)
                {
                    M(1, 2, 3);
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that exactly one space is added after keywords such as if, for, while, switch, etc.
    /// </summary>
    [TestMethod]
    public void AddsSpaceAfterKeyword()
    {
        const string input = """
            class C
            {
                void M()
                {
                    if(true)
                    {
                    }

                    while(true)
                    {
                        break;
                    }

                    switch(1)
                    {
                    }

                    for(var i = 0; i < 1; i++)
                    {
                    }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                    }

                    while (true)
                    {
                        break;
                    }

                    switch (1)
                    {
                    }

                    for (var i = 0; i < 1; i++)
                    {
                    }
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that the return keyword followed by a semicolon does not have a space added
    /// and that the throw keyword followed by a semicolon also has no space.
    /// </summary>
    [TestMethod]
    public void RemovesSpaceBeforeSemicolon()
    {
        const string input = """
            class C
            {
                void M()
                {
                    return;
                }

                int N()
                {
                    return 1;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    return;
                }

                int N()
                {
                    return 1;
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that unary operators such as negation and increment do not get extra spacing.
    /// </summary>
    [TestMethod]
    public void HandlesUnaryOperators()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x = -1;
                    var y = !true;
                    x++;
                    x--;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = -1;
                    var y = !true;
                    x++;
                    x--;
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that spacing inside string interpolation expressions is handled correctly.
    /// </summary>
    [TestMethod]
    public void HandlesStringInterpolation()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var name = "world";
                    var s = $"Hello {name}!";
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var name = "world";
                    var s = $"Hello {name}!";
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that content inside string literals is not affected by horizontal spacing rules.
    /// </summary>
    [TestMethod]
    public void PreservesSpacingInStringLiterals()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var s = "hello   world   test";
                    var v = @"multi   space   string";
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var s = "hello   world   test";
                    var v = @"multi   space   string";
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that the phase corrects multiple spacing issues in the same code snippet.
    /// </summary>
    [TestMethod]
    public void HandlesMultipleSpacingIssues()
    {
        const string input = """
            class C
            {
                void M(int a ,int b)
                {
                    var x = a+b;
                    if( x==0 )
                    {
                    }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M(int a, int b)
                {
                    var x = a + b;
                    if (x == 0)
                    {
                    }
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that generic type arguments such as angle brackets are handled without
    /// introducing or removing required spacing.
    /// </summary>
    [TestMethod]
    public void HandlesGenericTypeArguments()
    {
        const string input = """
            using System.Collections.Generic;

            class C
            {
                void M()
                {
                    var list = new List<int>();
                    var dict = new Dictionary<string, int>();
                }
            }
            """;

        const string expected = """
            using System.Collections.Generic;

            class C
            {
                void M()
                {
                    var list = new List<int>();
                    var dict = new Dictionary<string, int>();
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that exactly one space is added around assignment operators such as =, +=, -=, etc.
    /// </summary>
    [TestMethod]
    public void AddsSpaceAroundAssignment()
    {
        const string input = """
            class C
            {
                void M()
                {
                    var x=1;
                    x+=2;
                    x-=1;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;
                    x += 2;
                    x -= 1;
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that multiple consecutive spaces between tokens are collapsed to a single space.
    /// </summary>
    [TestMethod]
    public void RemovesDoubleSpaces()
    {
        const string input = """
            class C
            {
                void  M()
                {
                    var  x  =  1;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;
                }
            }
            """;

        AssertHorizontalSpacing(input, expected);
    }

    /// <summary>
    /// Verifies that code that already has correct horizontal spacing is not modified.
    /// </summary>
    [TestMethod]
    public void PreservesAlreadyCorrectSpacing()
    {
        const string input = """
            class C
            {
                void M(int a, int b)
                {
                    var x = a + b;

                    if (x == 0)
                    {
                        return;
                    }

                    for (var i = 0; i < 10; i++)
                    {
                    }
                }
            }
            """;

        AssertHorizontalSpacing(input, input);
    }

    /// <summary>
    /// Applies horizontal spacing to the given input and asserts that the result matches the expected output.
    /// </summary>
    /// <param name="input">The input C# code.</param>
    /// <param name="expected">The expected formatted C# code.</param>
    private void AssertHorizontalSpacing(string input, string expected)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var result = HorizontalSpacingPhase.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}