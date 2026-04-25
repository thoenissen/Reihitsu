using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — object-initializer alignment
/// </summary>
[TestClass]
public class ObjectInitializerAlignmentFullPipelineTests
{
    #region Constants

    /// <summary>
    /// Input source used for object-initializer-alignment formatting scenarios.
    /// </summary>
    private const string TestData = """
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
    /// Expected formatter output for object-initializer-alignment scenarios.
    /// </summary>
    private const string ResultData = """
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

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that object initializers are formatted correctly.
    /// </summary>
    [TestMethod]
    public void FormatsObjectInitializerLayout()
    {
        // Arrange
        var input = TestData;
        var expected = ResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}