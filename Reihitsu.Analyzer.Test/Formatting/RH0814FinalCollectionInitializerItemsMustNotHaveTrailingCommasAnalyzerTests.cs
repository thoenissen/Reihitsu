using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer"/> and <see cref="RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzerTests : AnalyzerTestsBase<RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer, RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider>
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

        await Verify(testData, fixedData, Diagnostics(RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0814MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0814MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH0814FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId, AnalyzerResources.RH0814MessageFormat));
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

    #endregion // Tests
}