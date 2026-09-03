using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer"/> and <see cref="RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer, RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a nested collection initializer with the opening brace on the next line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOpeningBraceOnNextLineAfterAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values =
                                                        {
                                                            1,
                                                            2
                                                        }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a nested collection initializer with misaligned braces is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedBracesAreDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                            1,
                                                            2
                                                                  }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly formatted multiline nested collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedMultilineNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Values = {
                                                                     1,
                                                                     2
                                                                 }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an object initializer without nested collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForObjectInitializerWithoutNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public string Name { get; set; }
                                    public int Count { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Name = "test",
                                                        Count = 5
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a correctly formatted single-line nested collection initializer does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Values = { 1, 2, 3, 4, 5 }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested collection initializer within a property initializer does not report for the single-line form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPropertyInitializerWithSingleLineNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    public Example Create() => new Example
                                                               {
                                                                   Values = { 1, 2 }
                                                               };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested collection initializer with target-typed new does not report for correct formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedTargetTypedNestedCollectionInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        Example value = new()
                                                        {
                                                            Values = {
                                                                         1,
                                                                         2
                                                                     }
                                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multiline nested collection initializer with multiple elements on one line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMultipleElementsOnSameLineInMultilineInitializer()
    {
        const string testData = """
                                using System.Collections.Generic;
                                
                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                            1, 2,
                                                            3
                                                                  }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2,
                                                                      3
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned expression inside a multi-line complex element reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyComplexElementInteriorMisalignmentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                                     {
                                                                         existingKey,
                                                                     existingValue
                                                                     }
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that an internally aligned complex element with a shifted anchor reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyShiftedComplexElementAnchorIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                                         {
                                                                             existingKey,
                                                                             existingValue
                                                                         }
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a correctly aligned complex element inside a nested collection does not report
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCorrectlyFormattedComplexElement()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        Values = {
                                                                     {
                                                                         existingKey,
                                                                         existingValue
                                                                     }
                                                                 }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that formatter-stable same-line comments can prefix every token validated inside a complex element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentPrefixedComplexElementTokens()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        Values = {
                                                                     /* Keep element. */ {
                                                                         /* Keep key. */ existingKey,

                                                                         /* Keep value. */ existingValue
                                                                     /* Keep close. */ }
                                                                 }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a shifted standalone comment before a complex-element closing brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStandaloneCommentBeforeComplexCloseBraceIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                                     {
                                                                         existingKey,
                                                                         existingValue
                                                                     /* Keep close. */
                                                                     }
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue

                                                                          /* Keep close. */
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a shifted multi-line comment before an assignment-owned complex expression reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultilineCommentBeforeAssignmentOwnedComplexExpressionIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                                     {
                                                                     /*
                                                                         Keep key.
                                                                         */ existingKey,
                                                                         existingValue
                                                                     }
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          /*
                                                                          Keep key.
                                                                          */ existingKey,
                                                                          existingValue
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a shifted documentation-comment continuation before a complex closing brace reports and is fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationContinuationBeforeComplexCloseBraceIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<Data, Data> Values { get; } = [];

                                    private static void Method(Data existingKey, Data existingValue)
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                                     {
                                         existingKey,
                                         existingValue
                                         /// Keep close.
                                         /// Keep continuation.
                                     }
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<Data, Data> Values { get; } = [];

                                     private static void Method(Data existingKey, Data existingValue)
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      {
                                                                          existingKey,
                                                                          existingValue

                                                                          /// Keep close.
                                                                          /// Keep continuation.
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that a formatter-stable same-line comment can prefix the nested collection closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentPrefixedNestedCollectionClosingBrace()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Values = {
                                                                     1,
                                                                     2
                                                                 /* Keep close. */ }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that misaligned ordinary elements in a multi-line nested collection report and are fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedOrdinaryElementsAreDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:Values = {
                                                            1,
                                                            2
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }

                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat));
    }

    /// <summary>
    /// Verifies that formatter-stable same-line comments can prefix ordinary nested collection elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCommentPrefixedOrdinaryElements()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    public List<int> Values { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        Values = {
                                                                     /* Keep one. */ 1,

                                                                     /* Keep two. */ 2
                                                                 }
                                                    };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that Fix All converges in one iteration for ordinary-element alignment diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixAllConvergesForOrdinaryElementAssignments()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    public List<int> First { get; } = [];
                                    public List<int> Second { get; } = [];

                                    private static void Method()
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:First = {
                                                            1,
                                                                2
                                                                }|},
                                                        {|#1:Second = {
                                                            3,
                                                                4
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     public List<int> First { get; } = [];
                                     public List<int> Second { get; } = [];

                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         First = {
                                                                     1,
                                                                     2
                                                                 },
                                                         Second = {
                                                                      3,
                                                                      4
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Data;

                                internal class Example
                                {
                                    public Dictionary<string, Data> First { get; } = [];
                                    public Dictionary<string, Data> Second { get; } = [];

                                    private static void Method(Data firstValue, Data secondValue)
                                    {
                                        var value = new Example
                                                    {
                                                        {|#0:First = {
                                                                    {
                                                                        "first",
                                                                    firstValue
                                                                    }
                                                                }|},
                                                        {|#1:Second = {
                                                                     {
                                                                         "second",
                                                                     secondValue
                                                                     }
                                                                 }|}
                                                    };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Data;

                                 internal class Example
                                 {
                                     public Dictionary<string, Data> First { get; } = [];
                                     public Dictionary<string, Data> Second { get; } = [];

                                     private static void Method(Data firstValue, Data secondValue)
                                     {
                                         var value = new Example
                                                     {
                                                         First = {
                                                                     {
                                                                         "first",
                                                                         firstValue
                                                                     }
                                                                 },
                                                         Second = {
                                                                      {
                                                                          "second",
                                                                          secondValue
                                                                      }
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        // Verifies that Fix All converges in one iteration for multiple assignment-owned complex elements
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5304MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}