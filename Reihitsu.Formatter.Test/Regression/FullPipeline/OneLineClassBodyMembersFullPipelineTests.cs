using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests that members of a class written on one line each start on their own line in the full formatting pipeline
/// </summary>
[TestClass]
public class OneLineClassBodyMembersFullPipelineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that two constructors with block bodies inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesConstructorsInOneLineClassBody()
    {
        // Arrange
        const string input = "class AAttribute : System.Attribute { public AAttribute() { } public AAttribute(string s) { } }";
        const string expected = """
                                class AAttribute : System.Attribute
                                {
                                    public AAttribute()
                                    {
                                    }
                                    public AAttribute(string s)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two constructors in a one-line class without a base list are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesConstructorsInOneLineClassBodyWithoutBaseList()
    {
        // Arrange
        const string input = "class C { public C() { } public C(string s) { } }";
        const string expected = """
                                class C
                                {
                                    public C()
                                    {
                                    }
                                    public C(string s)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two methods with block bodies inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMethodsInOneLineClassBody()
    {
        // Arrange
        const string input = "class C { void A() { } void B() { } }";
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }
                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method followed by a field inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMethodAndFieldInOneLineClassBody()
    {
        // Arrange
        const string input = "class C { void A() { } int _f; }";
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }
                                    int _f;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a field followed by a method inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesFieldAndMethodInOneLineClassBody()
    {
        // Arrange
        const string input = "class C { int _f; void A() { } }";
        const string expected = """
                                class C
                                {
                                    int _f;
                                    void A()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two fields inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesFieldsInOneLineClassBody()
    {
        // Arrange
        const string input = "class C { int a; int b; }";
        const string expected = """
                                class C
                                {
                                    int a;
                                    int b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every gap is split when three members share one line
    /// </summary>
    [TestMethod]
    public void SeparatesEveryMemberWhenThreeShareOneLine()
    {
        // Arrange
        const string input = "class C { int a; int b; int c; }";
        const string expected = """
                                class C
                                {
                                    int a;
                                    int b;
                                    int c;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two members in a one-line struct body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMethodsInOneLineStructBody()
    {
        // Arrange
        const string input = "struct S { void A() { } void B() { } }";
        const string expected = """
                                struct S
                                {
                                    void A()
                                    {
                                    }
                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two members in a one-line record struct body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMembersInOneLineRecordStructBody()
    {
        // Arrange
        const string input = "record struct R { int a; int b; }";
        const string expected = """
                                record struct R
                                {
                                    int a;
                                    int b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two members in a one-line interface body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMembersInOneLineInterfaceBody()
    {
        // Arrange
        const string input = "interface I { void A(); void B(); }";
        const string expected = """
                                interface I
                                {
                                    void A();
                                    void B();
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a member on the closing-brace line of the previous member in a multi-line body is moved to its own line
    /// </summary>
    [TestMethod]
    public void SeparatesMemberFollowingClosingBraceInMultiLineBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void A()
                                 {
                                 } void B()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }
                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members sharing a line are split in both a nested and its containing type
    /// </summary>
    [TestMethod]
    public void SeparatesMembersAtEveryNestingLevel()
    {
        // Arrange
        const string input = "class O { class I { int a; int b; } int c; }";
        const string expected = """
                                class O
                                {
                                    class I
                                    {
                                        int a;
                                        int b;
                                    }
                                    int c;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members sharing a line inside an extension block with expanded braces are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMembersInExtensionBlock()
    {
        // Arrange
        const string input = """
                             static class E
                             {
                                 extension(int i)
                                 {
                                     public void A() { } public void B() { }
                                 }
                             }
                             """;
        const string expected = """
                                static class E
                                {
                                    extension(int i)
                                    {
                                        public void A()
                                        {
                                        }
                                        public void B()
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two types sharing a line inside a block namespace are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesTypesInBlockNamespace()
    {
        // Arrange
        const string input = "namespace N { class A { int a; } class B { int b; } }";
        const string expected = """
                                namespace N
                                {
                                    class A
                                    {
                                        int a;
                                    }
                                    class B
                                    {
                                        int b;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that types sharing a line with each other and with a file-scoped namespace header are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesTypesAndHeaderInFileScopedNamespace()
    {
        // Arrange
        const string input = "namespace N; class A { int a; } class B { int b; }";
        const string expected = """
                                namespace N;
                                class A
                                {
                                    int a;
                                }
                                class B
                                {
                                    int b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two top-level types sharing a line are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesTypesInCompilationUnit()
    {
        // Arrange
        const string input = "class A { int a; } class B { int b; }";
        const string expected = """
                                class A
                                {
                                    int a;
                                }
                                class B
                                {
                                    int b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type sharing a line with a using directive starts its own line
    /// </summary>
    [TestMethod]
    public void SeparatesTypeFollowingUsingDirective()
    {
        // Arrange
        const string input = "using System; class A { int a; }";
        const string expected = """
                                using System;
                                class A
                                {
                                    int a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type sharing a line with an extern alias directive starts its own line
    /// </summary>
    [TestMethod]
    public void SeparatesTypeFollowingExternAlias()
    {
        // Arrange
        const string input = "extern alias X; class A { int a; }";
        const string expected = """
                                extern alias X;
                                class A
                                {
                                    int a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that top-level statements sharing a line are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesTopLevelStatements()
    {
        // Arrange
        const string input = "int x = 1; int y = 2;";
        const string expected = """
                                int x = 1;
                                int y = 2;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type sharing a line with a top-level statement starts its own line
    /// </summary>
    [TestMethod]
    public void SeparatesTypeFollowingTopLevelStatement()
    {
        // Arrange
        const string input = "System.Console.WriteLine(); class A { int a; }";
        const string expected = """
                                System.Console.WriteLine();
                                class A
                                {
                                    int a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the line break before an attributed member is inserted before its attribute list
    /// </summary>
    [TestMethod]
    public void PlacesAttributedMemberOnItsOwnLine()
    {
        // Arrange
        const string input = "class C { void A() { } [System.Obsolete] void B() { } }";
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }
                                    [System.Obsolete]
                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line block comment between two members stays on the line of the preceding member
    /// </summary>
    [TestMethod]
    public void KeepsBlockCommentTrailingPreviousMember()
    {
        // Arrange
        const string input = "class C { void A() { } /* c */ void B() { } }";
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    } /* c */
                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members already on their own lines without a blank line between them stay unchanged
    /// </summary>
    [TestMethod]
    public void KeepsAlreadySeparatedMembersWithoutBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int a;
                                 int b;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that members already on their own lines with one blank line between them stay unchanged
    /// </summary>
    [TestMethod]
    public void KeepsAlreadySeparatedMembersWithBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int a;

                                 int b;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a member following a multi-line comment that ends mid-line is not moved, because the comment already breaks the line
    /// </summary>
    [TestMethod]
    public void KeepsMemberAfterMultiLineCommentEndingMidLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int a; /* first
                                 second */ int b;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that enum members, which form a comma-separated list rather than a member declaration list, keep sharing a line
    /// </summary>
    [TestMethod]
    public void KeepsEnumMembersOnSharedLine()
    {
        // Arrange
        const string input = "enum E { A, B }";
        const string expected = """
                                enum E
                                {
                                    A, B
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty top-level statement stays on the line of the preceding statement
    /// </summary>
    [TestMethod]
    public void KeepsEmptyTopLevelStatementOnSharedLine()
    {
        // Arrange
        const string input = "System.Console.WriteLine();;";

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a type preceded only by a comment, without any preceding token, is not moved onto a new line
    /// </summary>
    [TestMethod]
    public void KeepsHeaderCommentBeforeFirstType()
    {
        // Arrange
        const string input = "/* header */ class A { int a; }";
        const string expected = """
                                /* header */ class A
                                {
                                    int a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written after a member keeps its own relocated line and the member it documents is not merged onto it
    /// </summary>
    [TestMethod]
    public void KeepsRelocatedDocumentationCommentBeforeMember()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void A() { } /// <summary>x</summary>
                                 void B() { }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }

                                    /// <summary>
                                    /// x
                                    /// </summary>
                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members sharing a line inside an active conditional region are split while the directives stay in place
    /// </summary>
    [TestMethod]
    public void SeparatesMembersInsideActiveConditionalRegion()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if !DEBUG
                                 int a; int b;
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if !DEBUG
                                    int a;
                                    int b;
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members inside an inactive conditional branch stay untouched as disabled text while the active branch is split
    /// </summary>
    [TestMethod]
    public void KeepsDisabledTextWhileSplittingActiveBranch()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if DEBUG
                                 int a; int b;
                             #else
                                 int c; int d;
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if DEBUG
                                    int a; int b;
                                #else
                                    int c;
                                    int d;
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members sharing a line next to pragma directives are split while the directive lines stay in place
    /// </summary>
    [TestMethod]
    public void SeparatesMembersAroundPragmaDirectives()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int a; int b;
                             #pragma warning disable CS0169
                                 int c; int d;
                             #pragma warning restore CS0169
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int a;
                                    int b;
                                #pragma warning disable CS0169
                                    int c;
                                    int d;
                                #pragma warning restore CS0169
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a top-level statement following an empty top-level statement stays on its line
    /// </summary>
    [TestMethod]
    public void KeepsStatementAfterEmptyTopLevelStatementOnSharedLine()
    {
        // Arrange
        const string input = "System.Console.WriteLine();; System.Console.WriteLine();";

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}