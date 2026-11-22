using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Expanding a single-line type that contains a member must place the opening brace on its own
/// line (Allman style) even when a leading <c>using</c> directive precedes the type, and the
/// offset introduced by the using directive must not cause the brace-placement edit to operate
/// on a stale token and leave the opening brace on the declaration line
/// </summary>
[TestClass]
public class SingleLineTypeBraceWithLeadingUsingTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Single-line type containing a field member, preceded by a leading using directive and attribute
    /// </summary>
    private const string Input = """
                                 using System;

                                 [Serializable]
                                 internal class TestClass { private int _value; }
                                 """;

    /// <summary>
    /// Expected output with the opening brace on its own line and the member indented inside the braces
    /// </summary>
    private const string Expected = """
                                    using System;

                                    [Serializable]
                                    internal class TestClass
                                    {
                                        private int _value;
                                    }
                                    """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that a single-line type preceded by a leading using directive expands with the
    /// opening brace on its own line under LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void OpeningBraceMovesToOwnLineWithLeadingUsing()
    {
        AssertRuleResult(Input, Expected);
    }

    /// <summary>
    /// Verifies that the already-expanded output is left unchanged, which makes formatting it idempotent, and that no
    /// phase run alone changes it (FMT-PIPE-027)
    /// </summary>
    [TestMethod]
    public void ExpandedTypeRemainsStableOnSecondPass()
    {
        AssertRuleResult(Expected);
    }

    #endregion // Tests
}