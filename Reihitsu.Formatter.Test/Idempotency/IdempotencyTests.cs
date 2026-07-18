using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Idempotency;

/// <summary>
/// Verifies that the formatter produces stable output when applied multiple times
/// </summary>
[TestClass]
public class IdempotencyTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used to verify blank-line-before-statement idempotency
    /// </summary>
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

    /// <summary>
    /// Expected formatter output for blank-line-before-statement idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify blank-line-after-statement idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify expression-bodied-method idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify expression-bodied-constructor idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify that comments and directives attached to the arrow or semicolon
    /// token survive expression-body-to-block conversion, under repeated formatting (issue #422)
    /// </summary>
    private const string ExpressionBodiedCommentTestData = """
                                                           internal class ExpressionBodiedCommentTestData
                                                           {
                                                               public int TrailingArrowComment() => /* keep me */ 42;

                                                               public int LeadingSemicolonComment() => 42
                                                                   // keep me
                                                                   ;

                                                               public int LeadingSemicolonDirective() => 42
                                                           #pragma warning disable CS0168
                                                                   ;
                                                           }
                                                           """;

    /// <summary>
    /// Input source used to verify region-formatting idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify trailing-trivia-cleanup idempotency
    /// </summary>
    private const string TrailingTriviaCleanupTestData = """
                                                         internal class TrailingTriviaCleanupTestData   
                                                         {
                                                             public void Method()   
                                                             {



                                                                 var x = 1;
                                                             }
                                                         }


                                                         """;

    /// <summary>
    /// Input source used to verify indentation idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify horizontal-spacing idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify object-initializer-layout idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify method-chain-alignment idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify logical-expression-layout idempotency
    /// </summary>
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

    /// <summary>
    /// Input source used to verify that line joins never collapse code into a trailing comment (issue #226)
    /// </summary>
    private const string CommentJoinTestData = """
                                               internal class CommentJoinTestData
                                               {
                                                   public void Method()
                                                   {
                                                       var chain = Compute() // chain comment
                                                           .Continue();

                                                       var sum = First() + // binary comment
                                                           Second();
                                                   }

                                                   private object Compute()
                                                   {
                                                       return null;
                                                   }

                                                   private object Continue()
                                                   {
                                                       return null;
                                                   }

                                                   private int First()
                                                   {
                                                       return 0;
                                                   }

                                                   private int Second()
                                                   {
                                                       return 0;
                                                   }
                                               }
                                               """;

    /// <summary>
    /// Input source used to verify recursive (property) pattern layout idempotency
    /// </summary>
    private const string RecursivePatternLayoutTestData = """
                                                          internal class RecursivePatternLayoutTestData
                                                          {
                                                              public bool Check(object value)
                                                              {
                                                                  return value is
                                                                               {
                                                                                   Inner:
                                                                                   {
                                                                                       A: 1,
                                                                                       B: 2
                                                                                   },
                                                                                   Count: 0
                                                                               };
                                                              }
                                                          }
                                                          """;

    /// <summary>
    /// Input source used to verify complex-element (dictionary-style pair) initializer layout idempotency (issue #425)
    /// </summary>
    private const string ComplexElementInitializerLayoutTestData = """
                                                                   using System.Collections.Generic;

                                                                   internal class ComplexElementInitializerLayoutTestData
                                                                   {
                                                                       private readonly Dictionary<string, int> _map = new Dictionary<string, int>
                                                                       {
                                                                           { "a", 1 },
                                                                           { "b", 2 },
                                                                       };
                                                                   }
                                                                   """;

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that applying the formatter twice to BlankLineBeforeStatement test data produces the same result
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeStatementIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(BlankLineBeforeStatementTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to BlankLineAfterStatement test data produces the same result
    /// </summary>
    [TestMethod]
    public void BlankLineAfterStatementIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(BlankLineAfterStatementTestData);
    }

    /// <summary>
    /// Verifies that already-formatted code produces no changes when formatted again
    /// </summary>
    [TestMethod]
    public void AlreadyFormattedCodeProducesNoChanges()
    {
        AssertNoChangesUnderBothEndings(BlankLineBeforeStatementResultData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedMethod test data produces the same result
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedMethodIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(ExpressionBodiedMethodTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedConstructor test data produces the same result
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedConstructorIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(ExpressionBodiedConstructorTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ExpressionBodiedComment test data produces the same
    /// result and that the arrow-trailing comment, semicolon-leading comment, and semicolon-leading
    /// directive all survive both passes (issue #422)
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedCommentIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(ExpressionBodiedCommentTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to RegionFormatting test data produces the same result
    /// </summary>
    [TestMethod]
    public void RegionFormattingIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(RegionFormattingTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to TrailingTriviaCleanup test data produces the same result
    /// </summary>
    [TestMethod]
    public void TrailingTriviaCleanupIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(TrailingTriviaCleanupTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to Indentation test data produces the same result
    /// </summary>
    [TestMethod]
    public void IndentationIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(IndentationTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to HorizontalSpacing test data produces the same result
    /// </summary>
    [TestMethod]
    public void HorizontalSpacingIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(HorizontalSpacingTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ObjectInitializerLayout test data produces the same result
    /// </summary>
    [TestMethod]
    public void ObjectInitializerLayoutIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(ObjectInitializerLayoutTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to MethodChainAlignment test data produces the same result
    /// </summary>
    [TestMethod]
    public void MethodChainAlignmentIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(MethodChainAlignmentTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to LogicalExpressionLayout test data produces the same result
    /// </summary>
    [TestMethod]
    public void LogicalExpressionLayoutIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(LogicalExpressionLayoutTestData);
    }

    /// <summary>
    /// Verifies that line joins never collapse code into a trailing comment and remain idempotent (issue #226)
    /// </summary>
    [TestMethod]
    public void CommentJoinIsIdempotent()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = NormalizeLineEndings(CommentJoinTestData, endOfLine);
            var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);
            var secondPass = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationToken);
            var formatted = firstPass.GetRoot(TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(formatted, secondPass.GetRoot(TestContext.CancellationToken).ToFullString(), $"Formatter must be idempotent under {DescribeLineEnding(endOfLine)} line endings.");
            AssertUsesLineEnding(formatted, endOfLine);
            Assert.Contains("// chain comment" + endOfLine, formatted, "The chain dot must not be collapsed into the comment.");
            Assert.Contains("// binary comment" + endOfLine, formatted, "The binary operand must not be collapsed into the comment.");
        }
    }

    /// <summary>
    /// Verifies that applying the formatter twice to RecursivePatternLayout test data produces the same result
    /// </summary>
    [TestMethod]
    public void RecursivePatternLayoutIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(RecursivePatternLayoutTestData);
    }

    /// <summary>
    /// Verifies that applying the formatter twice to ComplexElementInitializerLayout test data produces the same result (issue #425)
    /// </summary>
    [TestMethod]
    public void ComplexElementInitializerLayoutIsIdempotent()
    {
        AssertIdempotentUnderBothEndings(ComplexElementInitializerLayoutTestData);
    }

    /// <summary>
    /// Asserts that formatting the given source twice yields the same result and that the formatter
    /// honors the requested line ending verbatim, under both LF and CRLF (issue #330)
    /// </summary>
    /// <param name="input">The source text to format</param>
    private void AssertIdempotentUnderBothEndings(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalized = NormalizeLineEndings(input, endOfLine);
            var firstPass = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(normalized, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);
            var firstResult = firstPass.GetRoot(TestContext.CancellationToken).ToFullString();
            var secondResult = ReihitsuFormatter.FormatSyntaxTree(firstPass, TestContext.CancellationToken).GetRoot(TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(firstResult, secondResult, $"Formatter must be idempotent under {DescribeLineEnding(endOfLine)} line endings.");
            AssertUsesLineEnding(firstResult, endOfLine);
        }
    }

    /// <summary>
    /// Asserts that the formatter produces no changes for the given already-formatted source under
    /// both LF and CRLF (issue #330)
    /// </summary>
    /// <param name="input">The already-formatted source text</param>
    private void AssertNoChangesUnderBothEndings(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalized = NormalizeLineEndings(input, endOfLine);
            var formatted = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(normalized, cancellationToken: TestContext.CancellationToken), TestContext.CancellationToken);
            var actual = formatted.GetRoot(TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(normalized, actual, $"Formatter must produce no changes under {DescribeLineEnding(endOfLine)} line endings.");
            AssertUsesLineEnding(actual, endOfLine);
        }
    }

    #endregion // Methods
}