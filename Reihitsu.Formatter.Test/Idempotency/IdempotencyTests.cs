using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Idempotency;

/// <summary>
/// Verifies that the formatter produces stable output when applied multiple times.
/// </summary>
[TestClass]
public class IdempotencyTests
{
    #region Constants

    private const string BlankLineBeforeStatementTestData = """
                                                            internal class BlankLineBeforeStatementTestData
                                                            {
                                                                public void TryStatement()
                                                                {
                                                                    var x = 1;
                                                                    try
                                                                    {
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }

                                                                public void IfStatement()
                                                                {
                                                                    var x = 1;
                                                                    if (x == 1)
                                                                    {
                                                                    }
                                                                }

                                                                public void WhileStatement()
                                                                {
                                                                    var x = 1;
                                                                    while (x > 0)
                                                                    {
                                                                        x--;
                                                                    }
                                                                }

                                                                public void DoStatement()
                                                                {
                                                                    var x = 1;
                                                                    do
                                                                    {
                                                                        x--;
                                                                    }
                                                                    while (x > 0);
                                                                }

                                                                public void UsingStatement()
                                                                {
                                                                    var x = 1;
                                                                    using (var stream = new System.IO.MemoryStream())
                                                                    {
                                                                    }
                                                                }

                                                                public void ForeachStatement()
                                                                {
                                                                    var x = 1;
                                                                    foreach (var item in new int[0])
                                                                    {
                                                                    }
                                                                }

                                                                public void ForStatement()
                                                                {
                                                                    var x = 1;
                                                                    for (var i = 0; i < 10; i++)
                                                                    {
                                                                    }
                                                                }

                                                                public void ReturnStatement()
                                                                {
                                                                    var x = 1;
                                                                    return;
                                                                }

                                                                public void GotoStatement()
                                                                {
                                                                    var x = 1;
                                                                    goto end;
                                                                    end:
                                                                    return;
                                                                }

                                                                public void BreakStatement()
                                                                {
                                                                    while (true)
                                                                    {
                                                                        var x = 1;
                                                                        break;
                                                                    }
                                                                }

                                                                public void ContinueStatement()
                                                                {
                                                                    while (true)
                                                                    {
                                                                        var x = 1;
                                                                        continue;
                                                                    }
                                                                }

                                                                public void ThrowStatement()
                                                                {
                                                                    var x = 1;
                                                                    throw new System.Exception();
                                                                }

                                                                public void SwitchStatement()
                                                                {
                                                                    var x = 1;
                                                                    switch (x)
                                                                    {
                                                                        case 1:
                                                                            break;
                                                                    }
                                                                }

                                                                public void CheckedStatement()
                                                                {
                                                                    var x = 1;
                                                                    checked
                                                                    {
                                                                        x++;
                                                                    }
                                                                }

                                                                public void UncheckedStatement()
                                                                {
                                                                    var x = 1;
                                                                    unchecked
                                                                    {
                                                                        x++;
                                                                    }
                                                                }

                                                                public unsafe void FixedStatement()
                                                                {
                                                                    var arr = new int[10];
                                                                    fixed (int* p = arr)
                                                                    {
                                                                    }
                                                                }

                                                                public void LockStatement()
                                                                {
                                                                    var obj = new object();
                                                                    lock (obj)
                                                                    {
                                                                    }
                                                                }

                                                                public System.Collections.Generic.IEnumerable<int> YieldReturnStatement()
                                                                {
                                                                    var x = 1;
                                                                    yield return x;
                                                                }

                                                                // --- Cases that should NOT be modified ---

                                                                public void FirstInBlock()
                                                                {
                                                                    try
                                                                    {
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }

                                                                public void ElseIf()
                                                                {
                                                                    var x = 1;

                                                                    if (x == 1)
                                                                    {
                                                                    }
                                                                    else if (x == 2)
                                                                    {
                                                                    }
                                                                }

                                                                public System.Collections.Generic.IEnumerable<int> ConsecutiveYieldReturn()
                                                                {
                                                                    yield return 1;
                                                                    yield return 2;
                                                                    yield return 3;
                                                                }

                                                                public void AlreadyHasBlankLine()
                                                                {
                                                                    var x = 1;

                                                                    if (x == 1)
                                                                    {
                                                                    }
                                                                }

                                                                public void PrecededByComment()
                                                                {
                                                                    var x = 1;
                                                                    // This is a comment
                                                                    if (x == 1)
                                                                    {
                                                                    }
                                                                }

                                                                public void PrecededByBlockComment()
                                                                {
                                                                    var x = 1;
                                                                    /* This is a block comment */
                                                                    if (x == 1)
                                                                    {
                                                                    }
                                                                }
                                                            }
                                                            """;

    private const string BlankLineBeforeStatementResultData = """
                                                              internal class BlankLineBeforeStatementTestData
                                                              {
                                                                  public void TryStatement()
                                                                  {
                                                                      var x = 1;

                                                                      try
                                                                      {
                                                                      }
                                                                      catch
                                                                      {
                                                                      }
                                                                  }

                                                                  public void IfStatement()
                                                                  {
                                                                      var x = 1;

                                                                      if (x == 1)
                                                                      {
                                                                      }
                                                                  }

                                                                  public void WhileStatement()
                                                                  {
                                                                      var x = 1;

                                                                      while (x > 0)
                                                                      {
                                                                          x--;
                                                                      }
                                                                  }

                                                                  public void DoStatement()
                                                                  {
                                                                      var x = 1;

                                                                      do
                                                                      {
                                                                          x--;
                                                                      }
                                                                      while (x > 0);
                                                                  }

                                                                  public void UsingStatement()
                                                                  {
                                                                      var x = 1;

                                                                      using (var stream = new System.IO.MemoryStream())
                                                                      {
                                                                      }
                                                                  }

                                                                  public void ForeachStatement()
                                                                  {
                                                                      var x = 1;

                                                                      foreach (var item in new int[0])
                                                                      {
                                                                      }
                                                                  }

                                                                  public void ForStatement()
                                                                  {
                                                                      var x = 1;

                                                                      for (var i = 0; i < 10; i++)
                                                                      {
                                                                      }
                                                                  }

                                                                  public void ReturnStatement()
                                                                  {
                                                                      var x = 1;

                                                                      return;
                                                                  }

                                                                  public void GotoStatement()
                                                                  {
                                                                      var x = 1;

                                                                      goto end;
                                                                      end:
                                                                      return;
                                                                  }

                                                                  public void BreakStatement()
                                                                  {
                                                                      while (true)
                                                                      {
                                                                          var x = 1;

                                                                          break;
                                                                      }
                                                                  }

                                                                  public void ContinueStatement()
                                                                  {
                                                                      while (true)
                                                                      {
                                                                          var x = 1;

                                                                          continue;
                                                                      }
                                                                  }

                                                                  public void ThrowStatement()
                                                                  {
                                                                      var x = 1;

                                                                      throw new System.Exception();
                                                                  }

                                                                  public void SwitchStatement()
                                                                  {
                                                                      var x = 1;

                                                                      switch (x)
                                                                      {
                                                                          case 1:
                                                                              break;
                                                                      }
                                                                  }

                                                                  public void CheckedStatement()
                                                                  {
                                                                      var x = 1;

                                                                      checked
                                                                      {
                                                                          x++;
                                                                      }
                                                                  }

                                                                  public void UncheckedStatement()
                                                                  {
                                                                      var x = 1;

                                                                      unchecked
                                                                      {
                                                                          x++;
                                                                      }
                                                                  }

                                                                  public unsafe void FixedStatement()
                                                                  {
                                                                      var arr = new int[10];

                                                                      fixed (int* p = arr)
                                                                      {
                                                                      }
                                                                  }

                                                                  public void LockStatement()
                                                                  {
                                                                      var obj = new object();

                                                                      lock (obj)
                                                                      {
                                                                      }
                                                                  }

                                                                  public System.Collections.Generic.IEnumerable<int> YieldReturnStatement()
                                                                  {
                                                                      var x = 1;

                                                                      yield return x;
                                                                  }

                                                                  // --- Cases that should NOT be modified ---

                                                                  public void FirstInBlock()
                                                                  {
                                                                      try
                                                                      {
                                                                      }
                                                                      catch
                                                                      {
                                                                      }
                                                                  }

                                                                  public void ElseIf()
                                                                  {
                                                                      var x = 1;

                                                                      if (x == 1)
                                                                      {
                                                                      }
                                                                      else if (x == 2)
                                                                      {
                                                                      }
                                                                  }

                                                                  public System.Collections.Generic.IEnumerable<int> ConsecutiveYieldReturn()
                                                                  {
                                                                      yield return 1;
                                                                      yield return 2;
                                                                      yield return 3;
                                                                  }

                                                                  public void AlreadyHasBlankLine()
                                                                  {
                                                                      var x = 1;

                                                                      if (x == 1)
                                                                      {
                                                                      }
                                                                  }

                                                                  public void PrecededByComment()
                                                                  {
                                                                      var x = 1;

                                                                      // This is a comment
                                                                      if (x == 1)
                                                                      {
                                                                      }
                                                                  }

                                                                  public void PrecededByBlockComment()
                                                                  {
                                                                      var x = 1;

                                                                      /* This is a block comment */
                                                                      if (x == 1)
                                                                      {
                                                                      }
                                                                  }
                                                              }
                                                              """;

    private const string BlankLineAfterStatementTestData = """
                                                           internal class BlankLineAfterStatementTestData
                                                           {
                                                               public void SwitchWithBreakFollowedByCase()
                                                               {
                                                                   switch (1)
                                                                   {
                                                                       case 1:
                                                                           System.Console.WriteLine();
                                                                           break;
                                                                       case 2:
                                                                           System.Console.WriteLine();
                                                                           break;
                                                                       default:
                                                                           break;
                                                                   }
                                                               }

                                                               public void BreakInLoopFollowedByStatement()
                                                               {
                                                                   for (var i = 0; i < 10; i++)
                                                                   {
                                                                       break;
                                                                       var x = 1;
                                                                   }
                                                               }

                                                               // --- Cases that should NOT be modified ---

                                                               public void BreakLastInBlock()
                                                               {
                                                                   while (true)
                                                                   {
                                                                       break;
                                                                   }
                                                               }

                                                               public void BreakAlreadyFollowedByBlankLine()
                                                               {
                                                                   for (var i = 0; i < 10; i++)
                                                                   {
                                                                       break;

                                                                       var x = 1;
                                                                   }
                                                               }

                                                               public void SwitchBreakLastInSection()
                                                               {
                                                                   switch (1)
                                                                   {
                                                                       case 1:
                                                                           break;
                                                                   }
                                                               }
                                                           }
                                                           """;

    private const string ExpressionBodiedMethodTestData = """
                                                          internal class ExpressionBodiedMethodTestData
                                                          {
                                                              public int GetValue() => 42;

                                                              public void DoWork() => System.Console.WriteLine("hello");

                                                              public string GetName() => "test";

                                                              // Already block body — should not change
                                                              public int GetOther()
                                                              {
                                                                  return 1;
                                                              }
                                                          }
                                                          """;

    private const string ExpressionBodiedConstructorTestData = """
                                                               internal class ExpressionBodiedConstructorTestData
                                                               {
                                                                   private int _value;

                                                                   public ExpressionBodiedConstructorTestData() => _value = 0;

                                                                   public ExpressionBodiedConstructorTestData(int value) => _value = value;

                                                                   // Already block body — should not change
                                                                   public ExpressionBodiedConstructorTestData(string text)
                                                                   {
                                                                       _value = text.Length;
                                                                   }
                                                               }
                                                               """;

    private const string RegionFormattingTestData = """
                                                    internal class RegionFormattingTestData
                                                    {
                                                        #region fields

                                                        private int _value;

                                                        #endregion

                                                        #region Constructor

                                                        public RegionFormattingTestData()
                                                        {
                                                            _value = 0;
                                                        }

                                                        #endregion // constructor

                                                        #region methods

                                                        public int GetValue()
                                                        {
                                                            return _value;
                                                        }

                                                        #endregion // Methods
                                                    }
                                                    """;

    private const string TrailingTriviaCleanupTestData = """
                                                         internal class TrailingTriviaCleanupTestData   
                                                         {
                                                             public void Method()   
                                                             {



                                                                 var x = 1;
                                                             }
                                                         }


                                                         """;

    private const string IndentationTestData = """
                                               internal class IndentationTestData
                                               {
                                                 public void Method()
                                                 {
                                                     var x = 1;

                                                           if (x == 1)
                                                 {
                                                           x = 2;
                                                 }
                                                 }
                                               }
                                               """;

    private const string HorizontalSpacingTestData = """
                                                     internal class HorizontalSpacingTestData
                                                     {
                                                         public void Method()
                                                         {
                                                             var x=1;
                                                             var y = x+2;
                                                             var z = x  +  y;
                                                             var list = new int[] { 1,2,3 };

                                                             if(x == 1)
                                                             {
                                                                 System.Console.WriteLine( x );
                                                             }

                                                             for (var i=0; i<10; i++)
                                                             {
                                                             }
                                                         }
                                                     }
                                                     """;

    private const string ObjectInitializerLayoutTestData = """
                                                           internal class ObjectInitializerLayoutTestData
                                                           {
                                                               // --- Object initializer with misaligned braces ---

                                                               public void ObjectInitializerWithWrongBraceAlignment()
                                                               {
                                                                   var obj = new System.Text.StringBuilder()
                                                                               {
                                                                                   Capacity = 100
                                                                               };
                                                               }

                                                               // --- Nested object initializer ---

                                                               public void NestedObjectInitializer()
                                                               {
                                                                   var obj = new ObjectInitializerLayoutTestData.Outer()
                                                                                   {
                                                                                       Name = "test",
                                                                                       Inner = new ObjectInitializerLayoutTestData.Inner()
                                                                                                       {
                                                                                                           Value = 42
                                                                                                       }
                                                                                   };
                                                               }

                                                               // --- Object creation without initializer (should stay unchanged) ---

                                                               public void ObjectCreationWithoutInitializer()
                                                               {
                                                                   var obj = new System.Text.StringBuilder();
                                                               }

                                                               // --- Collection initializer (should not be affected by ObjectInitializerLayoutRule) ---

                                                               public void CollectionInitializer()
                                                               {
                                                                   var list = new System.Collections.Generic.List<int>()
                                                                   {
                                                                       1,
                                                                       2,
                                                                       3
                                                                   };
                                                               }

                                                               // --- Object initializer with single assignment ---

                                                               public void SingleAssignment()
                                                               {
                                                                   var obj = new System.Text.StringBuilder()
                                                                                       {
                                                                                           Capacity = 50
                                                                                       };
                                                               }

                                                               // --- Already correct alignment ---

                                                               public void AlreadyCorrectAlignment()
                                                               {
                                                                   var obj = new System.Text.StringBuilder()
                                                                   {
                                                                       Capacity = 200
                                                                   };
                                                               }

                                                               internal class Outer
                                                               {
                                                                   public string Name { get; set; }
                                                                   public Inner Inner { get; set; }
                                                               }

                                                               internal class Inner
                                                               {
                                                                   public int Value { get; set; }
                                                               }
                                                           }
                                                           """;

    private const string MethodChainAlignmentTestData = """
                                                        internal class MethodChainAlignmentTestData
                                                        {
                                                            // --- Multi-line method chain with misaligned dots ---

                                                            public void MultiLineChainMisaligned()
                                                            {
                                                                var result = new System.Collections.Generic.List<int> { 1, 2, 3 }
                                                                        .Where(x => x > 0)
                                                                            .Select(x => x * 2)
                                                                        .ToList();
                                                            }

                                                            // --- Single-line chain (should stay unchanged) ---

                                                            public void SingleLineChain()
                                                            {
                                                                var result = new System.Collections.Generic.List<int> { 1, 2, 3 }.Where(x => x > 0).ToList();
                                                            }

                                                            // --- Chain with conditional access (?.) ---

                                                            public string ConditionalAccessChain(string input)
                                                            {
                                                                var result = input?
                                                                        .Trim()
                                                                            .ToUpper();

                                                                return result;
                                                            }

                                                            // --- Short chain with only one link (should stay unchanged) ---

                                                            public void ShortChain()
                                                            {
                                                                var result = "hello"
                                                                    .ToUpper();
                                                            }

                                                            // --- Multi-line chain starting at various indentation levels ---

                                                            public void ChainWithIndentation()
                                                            {
                                                                var result = System.Linq.Enumerable.Range(0, 10)
                                                                                .Where(x => x > 2)
                                                                        .Select(x => x.ToString())
                                                                                    .ToList();
                                                            }
                                                        }
                                                        """;

    private const string LogicalExpressionLayoutTestData = """
                                                           internal class LogicalExpressionLayoutTestData
                                                           {
                                                               // --- Multi-line && expression with misaligned operators ---

                                                               public bool LogicalAndMisaligned(int x, int y)
                                                               {
                                                                   return x > 0
                                                                           && y > 0
                                                                       && x < 100;
                                                               }

                                                               // --- Multi-line || expression ---

                                                               public bool LogicalOrMisaligned(int x, int y)
                                                               {
                                                                   return x == 0
                                                                               || y == 0
                                                                       || x == 100;
                                                               }

                                                               // --- Single-line logical expression (should stay unchanged) ---

                                                               public bool SingleLineExpression(int x, int y)
                                                               {
                                                                   return x > 0 && y > 0;
                                                               }

                                                               // --- Non-logical binary expression (should stay unchanged) ---

                                                               public int NonLogicalExpression(int x, int y)
                                                               {
                                                                   return x + y;
                                                               }

                                                               // --- Mixed && and || in multi-line ---

                                                               public bool MixedLogicalOperators(int a, int b, int c)
                                                               {
                                                                   return a > 0
                                                                               && b > 0
                                                                           || c > 0;
                                                               }

                                                               // --- Nested logical expression ---

                                                               public bool NestedLogicalExpression(int x, int y, int z)
                                                               {
                                                                   var result = x > 0
                                                                                   && y > 0
                                                                           && z > 0;

                                                                   return result;
                                                               }

                                                               // --- Already correctly aligned ---

                                                               public bool AlreadyAligned(int x, int y)
                                                               {
                                                                   return x > 0
                                                                          && y > 0;
                                                               }
                                                           }
                                                           """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that applying the formatter twice to BlankLineBeforeStatement test data produces the same result.
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeStatementIsIdempotent()
    {
        // Arrange
        var input = BlankLineBeforeStatementTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to BlankLineAfterStatement test data produces the same result.
    /// </summary>
    [TestMethod]
    public void BlankLineAfterStatementIsIdempotent()
    {
        // Arrange
        var input = BlankLineAfterStatementTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that already-formatted code produces no changes when formatted again.
    /// </summary>
    [TestMethod]
    public void AlreadyFormattedCodeProducesNoChanges()
    {
        // Arrange
        var input = BlankLineBeforeStatementResultData;

        // Act
        var formatted = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var actual = formatted.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedMethod test data produces the same result.
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedMethodIsIdempotent()
    {
        // Arrange
        var input = ExpressionBodiedMethodTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedConstructor test data produces the same result.
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedConstructorIsIdempotent()
    {
        // Arrange
        var input = ExpressionBodiedConstructorTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to RegionFormatting test data produces the same result.
    /// </summary>
    [TestMethod]
    public void RegionFormattingIsIdempotent()
    {
        // Arrange
        var input = RegionFormattingTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to TrailingTriviaCleanup test data produces the same result.
    /// </summary>
    [TestMethod]
    public void TrailingTriviaCleanupIsIdempotent()
    {
        // Arrange
        var input = TrailingTriviaCleanupTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to Indentation test data produces the same result.
    /// </summary>
    [TestMethod]
    public void IndentationIsIdempotent()
    {
        // Arrange
        var input = IndentationTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to HorizontalSpacing test data produces the same result.
    /// </summary>
    [TestMethod]
    public void HorizontalSpacingIsIdempotent()
    {
        // Arrange
        var input = HorizontalSpacingTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ObjectInitializerLayout test data produces the same result.
    /// </summary>
    [TestMethod]
    public void ObjectInitializerLayoutIsIdempotent()
    {
        // Arrange
        var input = ObjectInitializerLayoutTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to MethodChainAlignment test data produces the same result.
    /// </summary>
    [TestMethod]
    public void MethodChainAlignmentIsIdempotent()
    {
        // Arrange
        var input = MethodChainAlignmentTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    /// <summary>
    /// Verifies that applying the formatter twice to LogicalExpressionLayout test data produces the same result.
    /// </summary>
    [TestMethod]
    public void LogicalExpressionLayoutIsIdempotent()
    {
        // Arrange
        var input = LogicalExpressionLayoutTestData;

        // Act
        var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token), TestContext.CancellationTokenSource.Token);
        var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(firstPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), secondPass.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString(), "Formatter must be idempotent");
    }

    #endregion // Methods
}