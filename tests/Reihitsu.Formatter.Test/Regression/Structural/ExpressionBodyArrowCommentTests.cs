using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for comments next to the arrow of an expression body the formatter converts: comments in front of the
/// arrow (FMT-UNSUP-013) and behind it (FMT-UNSUP-012) refuse the input as unsupported, and a comment above the member is
/// formatted with it
/// </summary>
[TestClass]
public class ExpressionBodyArrowCommentTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that the inputs whose comments FMT-STRUCT-006 moved from in front of the arrow into the block of a member the
    /// formatter converts are refused as unsupported: the input is returned unchanged, and the first comment in front of the arrow is
    /// reported
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    [DataRow("    int Other()\n    // before arrow\n    => 1;\n\n    void Run() // run\n        // first\n        => Foo();\n\n    void Foo()\n    {\n    }", "// before arrow", DisplayName = "Removed example of FMT-STRUCT-006")]
    [DataRow("    void Run() // run\n        // first\n        => Foo();\n\n    void Foo()\n    {\n    }", "// run", DisplayName = "Comment behind the signature and a comment line in front of the arrow")]
    [DataRow("    int Two()\n        // a\n\n        // b\n        => 42;", "// a", DisplayName = "Two comment lines with a blank line between them")]
    [DataRow("    int Multi()\n        /* first\n           second */\n        => 1;", "/* first\n           second */", DisplayName = "Multi-line block comment")]
    [DataRow("    void M()\n    {\n        int Local()\n            // note\n            => 1;\n    }", "// note", DisplayName = "Local function")]
    [DataRow("    ~C()\n        // note\n        => Release();\n\n    void Release()\n    {\n    }", "// note", DisplayName = "Finalizer")]
    [DataRow("    public static C operator +(C a, C b)\n        // note\n        => a;", "// note", DisplayName = "Operator")]
    [DataRow("    public static implicit operator int(C c)\n        // note\n        => 1;", "// note", DisplayName = "Conversion operator")]
    [DataRow("    T Get<T>()\n        where T : new()\n        // note\n        => new T();", "// note", DisplayName = "Method with a constraint clause")]
    [DataRow("    int A4()\n    // a\n    /* c */ => 42;", "// a", DisplayName = "Comment line and a block comment in front of the arrow on its line")]
    [DataRow("    int B()\n        /* c */ => 1;", "/* c */", DisplayName = "Block comment in front of the arrow on its line")]
    [DataRow("    int M() /* c */ => 1;", "/* c */", DisplayName = "Block comment in front of the arrow on one line")]
    [DataRow("    int M()\n    // a\n    => 42 /* t */;", "// a", DisplayName = "Comment line in front of the arrow and a comment in front of the semicolon")]
    [DataRow("    int N()\n    // b\n    => 42; // t", "// b", DisplayName = "Comment line in front of the arrow and a comment behind the semicolon")]
    [DataRow("    int M() /* c */\n    // a\n    => 42;", "/* c */", DisplayName = "Block comment behind the signature and a comment line in front of the arrow")]
    public void CommentsInFrontOfTheArrowOfAConvertedMemberAreUnsupported(string members, string comment)
    {
        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, $"class C\n{{\n{members}\n}}", comment);
    }

    /// <summary>
    /// Verifies the change's alternative to a comment in front of the arrow: written above the member, the comment stays above it,
    /// and the member is converted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    public void CommentAboveTheMemberStaysAboveTheConvertedMember()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // before arrow
                                 int Other() => 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // before arrow
                                    int Other()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the inputs whose comments FMT-STRUCT-006 placed between the arrow and the expression of a member the formatter
    /// converts are refused as unsupported: the input is returned unchanged, and the first comment behind the arrow is reported
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="comment">The comment that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    int Get() =>\n        // after arrow\n        42;", "// after arrow", DisplayName = "Comment line behind the arrow of a returning method")]
    [DataRow("    int Get() =>\n    // c\n    42;", "// c", DisplayName = "Comment line behind the arrow in the member's column")]
    [DataRow("    void Run() =>\n        // run\n        Foo();", "// run", DisplayName = "Comment line behind the arrow of a void method")]
    [DataRow("    int Fail() =>\n        // never\n        throw new System.InvalidOperationException();", "// never", DisplayName = "Comment line in front of a throw expression")]
    [DataRow("    int Value() =>\n        /* c */ 42;", "/* c */", DisplayName = "Block comment in front of the expression on its line")]
    [DataRow("    int Two() =>\n        // a\n\n        // b\n        42;", "// a", DisplayName = "Two comment lines with a blank line between them behind the arrow")]
    [DataRow("    void M()\n    {\n        int Local() =>\n            // note\n            1;\n    }", "// note", DisplayName = "Comment line behind the arrow of a local function")]
    [DataRow("    int M() =>\n        // a\n        42 /* t */;", "// a", DisplayName = "Comment line behind the arrow and a comment in front of the semicolon")]
    [DataRow("    int N() =>\n        // b\n        42; // t", "// b", DisplayName = "Comment line behind the arrow and a comment behind the semicolon")]
    public void CommentsBehindTheArrowOfAConvertedMemberAreUnsupported(string members, string comment)
    {
        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, $"class C\n{{\n{members}\n}}", comment);
    }

    #endregion // Tests
}