using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for the clauses of a query expression that start a line: they align with the first token of the query
/// they belong to, outside a list element as inside one, and a line inside a clause that starts at another token of the clause
/// is a continuation line of that clause
/// </summary>
[TestClass]
public class QueryClauseAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: the clauses of a query in a local declaration align with its <c>from</c>, the
    /// <c>equals</c> of a join on its own line is a continuation line of the join, and the clauses behind <c>into</c> belong to
    /// the query that holds the continuation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    public void SpecExampleQueryClausesAlignWithTheQuerysFirstToken()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr, int[] brr)
                                 {
                                     var q = from x in arr
                                     let y = x
                                     where y > 0
                                     select y;
                                     var r = from x in arr
                                     join b in brr on x
                                     equals b
                                     select x into z
                                     select z;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr, int[] brr)
                                    {
                                        var q = from x in arr
                                                let y = x
                                                where y > 0
                                                select y;
                                        var r = from x in arr
                                                join b in brr on x
                                                    equals b
                                                select x into z
                                                select z;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported scenario: the <c>select</c> clause of a query outside a list element aligns with the query's
    /// <c>from</c> instead of taking the statement's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    public void SelectClauseOfAQueryInALocalDeclarationAlignsWithFrom()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var b = from v in arr
                                     select v;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var b = from v in arr
                                                select v;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a query behind a <c>return</c> on the next line joins the <c>return</c>, and its clauses align with its
    /// <c>from</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    [Requirement("FMT-BREAK-041")]
    public void QueryBehindReturnJoinsAndItsClausesAlignWithFrom()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             class C
                             {
                                 IEnumerable<int> M(int[] a)
                                 {
                                     return
                                     from x in a
                                     select x;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;
                                using System.Linq;

                                class C
                                {
                                    IEnumerable<int> M(int[] a)
                                    {
                                        return from x in a
                                               select x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>into</c> that starts its line aligns with the <c>from</c> of the query that holds the continuation,
    /// and so do the clauses behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    public void IntoThatStartsItsLineAlignsWithFrom()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var q = from x in arr
                                     select x
                                     into z
                                     where z > 0
                                     select z;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var q = from x in arr
                                                select x
                                                into z
                                                where z > 0
                                                select z;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a later ordering of an <c>orderby</c> clause and the <c>by</c> of a <c>group</c> clause that start a line are
    /// continuation lines of their clause, one level deeper than the clause's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    [Requirement("FMT-INDENT-034")]
    public void LinesInsideAClauseAreContinuationLinesOfTheClause()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var q = from x in arr
                                     orderby x,
                                     -x
                                     group x
                                     by x into g
                                     select g;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var q = from x in arr
                                                orderby x,
                                                    -x
                                                group x
                                                    by x into g
                                                select g;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the clauses of a nested query align with the nested query's own <c>from</c>, and the outer query's clauses
    /// with the outer <c>from</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    public void ClausesOfANestedQueryAlignWithTheNestedFrom()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var q = from x in arr
                                     from y in (from z in arr
                                     select z)
                                     select y;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int[] arr)
                                    {
                                        var q = from x in arr
                                                from y in (from z in arr
                                                           select z)
                                                select y;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side: clauses that already align with <c>from</c> outside a list element, and the clauses of a query
    /// inside an argument list, keep their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-035")]
    public void AlignedClausesAreLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int[] arr)
                                 {
                                     var q = from x in arr
                                             where x > 0
                                             select x;

                                     System.Console.WriteLine(from x in arr
                                                              select x);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}