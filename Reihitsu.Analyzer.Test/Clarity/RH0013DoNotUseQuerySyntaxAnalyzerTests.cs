using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0013DoNotUseQuerySyntaxAnalyzer"/>
/// </summary>
[TestClass]
public class RH0013DoNotUseQuerySyntaxAnalyzerTests : AnalyzerTestsBase<RH0013DoNotUseQuerySyntaxAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifying simple query syntax with where clause is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithWhereClauseIsReported()
    {
        const string testCode = """
                                using System.Linq;

                                public class Test
                                {
                                    public IQueryable<int> Run(IQueryable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               where value > 0
                                               select value;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with orderby clause is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithOrderByIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<int> Run(IEnumerable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               orderby value
                                               select value;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with select projection is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithProjectionIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<string> Run(IEnumerable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               select value.ToString();
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with join is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithJoinIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<string> Run(IEnumerable<int> left, IEnumerable<int> right)
                                    {
                                        return {|#0:from|} l in left
                                               join r in right on l equals r
                                               select l.ToString();
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with group by is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithGroupByIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<IGrouping<int, int>> Run(IEnumerable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               group value by value % 2;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with let clause is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithLetClauseIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<int> Run(IEnumerable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               let doubled = value * 2
                                               select doubled;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with multiple from clauses is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithMultipleFromClausesIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<int> Run(IEnumerable<IEnumerable<int>> values)
                                    {
                                        return {|#0:from|} outer in values
                                               from inner in outer
                                               select inner;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with into continuation is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithIntoContinuationIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<IGrouping<int, int>> Run(IEnumerable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               group value by value % 2 into grouped
                                               select grouped;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying query syntax with descending order is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxWithDescendingOrderIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<int> Run(IEnumerable<int> values)
                                    {
                                        return {|#0:from|} value in values
                                               orderby value descending
                                               select value;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    /// <summary>
    /// Verifying method syntax is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MethodSyntaxIsNotReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<int> Run(IEnumerable<int> values)
                                    {
                                        return values.Where(v => v > 0).Select(v => v);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying nested query syntax is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NestedQuerySyntaxIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public IEnumerable<IEnumerable<int>> Run(IEnumerable<IEnumerable<int>> values)
                                    {
                                        return {|#0:from|} outer in values
                                               select ({|#1:from|} inner in outer select inner);
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax.", 2));
    }

    /// <summary>
    /// Verifying query syntax in variable initializer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task QuerySyntaxInVariableInitializerIsReported()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                public class Test
                                {
                                    public void Run(IEnumerable<int> values)
                                    {
                                        var filtered = {|#0:from|} value in values
                                                       where value > 0
                                                       select value;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0013DoNotUseQuerySyntaxAnalyzer.DiagnosticId, "Do not use query syntax."));
    }

    #endregion // Members
}