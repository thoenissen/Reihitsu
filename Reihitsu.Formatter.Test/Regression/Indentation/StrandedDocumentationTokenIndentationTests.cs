using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for the indentation of a token that follows a stray documentation comment written behind
/// the previous token on the same line
/// </summary>
[TestClass]
public class StrandedDocumentationTokenIndentationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a statement terminator below a stray documentation comment behind a return expression keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsTerminatorIndentationBelowStrayDocumentationCommentBehindReturnExpression()
    {
        const string input = """
                             public class C
                             {
                                 public int M()
                                 {
                                     return 1 /// stray
                                     ;
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a closing bracket below a stray documentation comment behind an attribute argument list keeps its indentation
    /// </summary>
    [TestMethod]
    public void KeepsClosingBracketIndentationBelowStrayDocumentationCommentBehindAttributeArguments()
    {
        const string input = """
                             public class C
                             {
                                 [System.Obsolete("x") /// stray
                                 ]
                                 public int P { get; set; }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Methods
}