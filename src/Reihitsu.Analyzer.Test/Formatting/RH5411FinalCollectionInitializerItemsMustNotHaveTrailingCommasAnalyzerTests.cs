using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer"/> and <see cref="RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzerTests : BatchCodeFixTestsBase<RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer, RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a trailing comma on the final collection initializer item is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnCollectionInitializerIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> Values = new()
                                                                          {
                                                                              1,
                                                                              2{|#0:,|}
                                                                          };
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static readonly List<int> Values = new()
                                                                           {
                                                                               1,
                                                                               2
                                                                           };
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5411MessageFormat));
    }

    /// <summary>
    /// Verifies that collection initializer assignments inside object initializers are detected and fixed surgically
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnCollectionInitializerAssignmentIsRemovedSurgically()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    public List<int> Values { get; } = [];

                                    private static Example Create()
                                    {
                                        return new Example { Values = { 1,  2{|#0:,|} } };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     public List<int> Values { get; } = [];

                                     private static Example Create()
                                     {
                                         return new Example { Values = { 1,  2 } };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5411MessageFormat));
    }

    /// <summary>
    /// Verifies that a same-line comment after the final collection initializer item is preserved by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaWithSameLineCommentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> Values = new()
                                                                          {
                                                                              1,
                                                                              2{|#0:,|} // Final value
                                                                          };
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static readonly List<int> Values = new()
                                                                           {
                                                                               1,
                                                                               2 // Final value
                                                                           };
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5411MessageFormat));
    }

    /// <summary>
    /// Verifies that collection initializers without a trailing comma on the final item are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionInitializerWithoutTrailingCommaIsNotFlagged()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> Values = new()
                                                                          {
                                                                              1,
                                                                              2
                                                                          };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that empty collection initializers are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyCollectionInitializerIsNotFlagged()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> Values = new()
                                                                          {
                                                                          };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the trailing comma is not flagged when a conditional-compilation block carrying a further
    /// item follows the final active item, since the formatter withholds its own removal for the same shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionWithConditionalBlockAfterFinalItemIsNotFlagged()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new List<int>
                                        {
                                            1,
                                #if SYMBOL
                                            2,
                                #endif
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing comma on an array initializer is not flagged, since that shape belongs to RH5410
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnArrayInitializerIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var values = new[]
                                        {
                                            1,
                                            2,
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing comma on an object initializer is not flagged, since no trailing-comma rule covers that kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommaOnObjectInitializerIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private int First { get; set; }

                                    private int Second { get; set; }

                                    private static void Method()
                                    {
                                        var value = new Example
                                        {
                                            First = 1,
                                            Second = 2,
                                        };
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                internal class Example
                                {
                                    private static readonly List<int> First = new() { 1, 2{|#0:,|} };
                                    private static readonly List<int> Second = new() { 3, 4{|#1:,|} };
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 internal class Example
                                 {
                                     private static readonly List<int> First = new() { 1, 2 };
                                     private static readonly List<int> Second = new() { 3, 4 };
                                 }
                                 """;

        // The rule reports at most one trailing comma per collection initializer (its final item), so a shared
        // owner between two occurrences is unreachable; two independent, adjacent field initializers exercise the
        // batch fixer applying two genuinely separate, surgical comma removals
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH5411MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}