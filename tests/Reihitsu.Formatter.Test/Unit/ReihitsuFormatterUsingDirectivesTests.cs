using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>, which organizes the using block of one scope
/// of a document, as the RH7201–RH7207 code fixes pass it
/// </summary>
[TestClass]
public class ReihitsuFormatterUsingDirectivesTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: with a namespace as scope, the directive that shared a line comes first, each directive stands
    /// on a line of its own at the scope's indentation, and the class behind the block is not formatted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task NamespaceScopeIsOrganizedAndTheCodeBehindTheBlockIsNotFormatted()
    {
        const string input = """
                             namespace N
                             {
                                 using System.Linq; using System;

                                 class C{ }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    using System;
                                    using System.Linq;

                                    class C{ }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies that the using block of a document that the formatter refuses as unsupported is organized: the method runs no
    /// formatting phase but the using phase and refuses no input
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-UNSUP-002")]
    public async Task UsingBlockOfADocumentWithAnUnsupportedShapeIsOrganized()
    {
        const string input = """
                             namespace N
                             {
                                 using System.Linq; using System;

                                 class C{ int /* c */ _a; }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    using System;
                                    using System.Linq;

                                    class C{ int /* c */ _a; }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies the spec example: a scope that does not start its line is read where it stands in the document, so the directive
    /// moved onto a line of its own gets the indentation of the line the block's first directive starts on, which is none, and the
    /// code behind the block stays on its line. The input holds no line break, so the formatter writes
    /// <see cref="Environment.NewLine"/> (FMT-PIPE-007), and the test runs under that line ending only
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task ScopeThatDoesNotStartItsLineIsReadWhereItStandsInTheDocument()
    {
        const string input = "namespace A { namespace B { using System.Linq; using System; class C { } } }";
        const string expected = "namespace A { namespace B { using System;\nusing System.Linq; class C { } } }";

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single(scope => scope.Name.ToString() == "B"), [Environment.NewLine]);
    }

    /// <summary>
    /// Verifies that the compilation unit as scope gets its block organized, and the block of a namespace nested in it is left as it
    /// is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task CompilationUnitScopeIsOrganizedAndANestedNamespaceIsLeftAsItIs()
    {
        const string input = """
                             using System.Linq;
                             using System;

                             namespace N
                             {
                                 using System.Text; using System.Collections;

                                 class C{ }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Linq;

                                namespace N
                                {
                                    using System.Text; using System.Collections;

                                    class C{ }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that a file-scoped namespace as scope gets its block organized, each directive on a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    public async Task FileScopedNamespaceScopeIsOrganized()
    {
        const string input = """
                             namespace N;

                             using System.Linq; using System;

                             class C{ }
                             """;

        const string expected = """
                                namespace N;

                                using System;
                                using System.Linq;

                                class C{ }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies that the entry point organizes the block of a document that contains a syntax error, which it does not check
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-PIPE-005")]
    public async Task BlockOfADocumentWithASyntaxErrorIsOrganized()
    {
        const string input = """
                             using System.Linq;
                             using System;

                             class C{ void M(){ var a = ; } }
                             """;

        const string expected = """
                                using System;
                                using System.Linq;

                                class C{ void M(){ var a = ; } }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that the entry point organizes the block of a document whose header marks it as generated, which it does not check
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-PIPE-006")]
    public async Task BlockOfADocumentWithAGeneratedHeaderIsOrganized()
    {
        const string input = """
                             // <auto-generated/>
                             using System.Linq;
                             using System;

                             class C{ }
                             """;

        const string expected = """
                                // <auto-generated/>
                                using System;
                                using System.Linq;

                                class C{ }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies the spec example: in a namespace with a body, a header with no blank line below it stays at the top, and the
    /// original first directive that moves away from the top keeps the whitespace of its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderWithoutBlankLineChangesNoDirectiveIndentationInANamespaceWithABody()
    {
        const string input = """
                             namespace N
                             {
                                 // Header
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies the spec example: in a namespace with a body, a header with a blank line below it stays at the top, and the
    /// directive that takes the first position starts at the whitespace of the original first directive's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderWithBlankLineChangesNoDirectiveIndentationInANamespaceWithABody()
    {
        const string input = """
                             namespace N
                             {
                                 // Header

                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header

                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies the spec example: when a comment of its own leads the directive that takes the first position, that comment
    /// starts at the whitespace of the original first directive's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task CommentLeadingTheNewFirstDirectiveStartsAtTheOriginalFirstDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 // Header

                                 using System.Linq;
                                 // About System
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header

                                    // About System
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies the spec statement: both blocks of the namespace examples without the header are organized with both
    /// directives at the original first directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task BlockWithoutHeaderKeepsItsIndentationInANamespaceWithABody()
    {
        const string input = """
                             namespace N
                             {
                                 using System.Linq;
                                 // About System
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // About System
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies the spec statement for a namespace indented with a tab: the tab stays in front of the header and both
    /// directives, without a blank line below the header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task TabIsKeptInFrontOfEveryDirectiveBelowAHeaderWithoutBlankLine()
    {
        const string input = """
                             namespace N
                             {
                                 // Header
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                    }
                                }
                                """;

        await AssertOrganizes(IndentWithTabs(input), IndentWithTabs(expected), SelectNamespace);
    }

    /// <summary>
    /// Verifies the spec statement for a namespace indented with a tab: the tab stays in front of the header and both
    /// directives, with a blank line below the header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task TabIsKeptInFrontOfEveryDirectiveBelowAHeaderWithBlankLine()
    {
        const string input = """
                             namespace N
                             {
                                 // Header

                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header

                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                    }
                                }
                                """;

        await AssertOrganizes(IndentWithTabs(input), IndentWithTabs(expected), SelectNamespace);
    }

    /// <summary>
    /// Verifies the spec example: in a file-scoped namespace whose directives are indented, a header changes no directive's
    /// indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderChangesNoDirectiveIndentationInAnIndentedFileScopedNamespace()
    {
        const string input = """
                             namespace N;

                                 // Header
                                 using System.Linq;
                                 using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                namespace N;

                                    // Header
                                    using System;
                                    using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies the spec statement: in a compilation unit whose directives are indented, a header changes no directive's
    /// indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderChangesNoDirectiveIndentationInAnIndentedCompilationUnit()
    {
        const string input = """
                                 // Header
                                 using System.Linq;
                                 using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                    // Header
                                    using System;
                                    using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that a header in a namespace nested in another namespace with a body changes no directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderChangesNoDirectiveIndentationInANestedNamespace()
    {
        const string input = """
                             namespace A
                             {
                                 namespace B
                                 {
                                     // Header
                                     using System.Linq;
                                     using System;

                                     class C
                                     {
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                namespace A
                                {
                                    namespace B
                                    {
                                        // Header
                                        using System;
                                        using System.Linq;

                                        class C
                                        {
                                        }
                                    }
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single(scope => scope.Name.ToString() == "B"));
    }

    /// <summary>
    /// Verifies that a delimited header on a line of its own changes no directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task DelimitedHeaderChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 /* Header */
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    /* Header */
                                    using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a multi-line delimited header keeps its own lines and changes no directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task MultiLineDelimitedHeaderChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 /*
                                  * Header
                                  */
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    /*
                                     * Header
                                     */
                                    using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a single-line documentation comment as header changes no directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task SingleLineDocumentationHeaderChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 /// Header
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    /// Header
                                    using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment as header changes no directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task DelimitedDocumentationHeaderChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 /** Header */
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    /** Header */
                                    using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a header of two single-line comments changes no directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderOfTwoCommentLinesChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 // Header
                                 // More header
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header
                                    // More header
                                    using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a blank line between the namespace's opening brace and the header stays, and the header changes no
    /// directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderBelowABlankLineAfterTheBraceChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {

                                 // Header
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {

                                    // Header
                                    using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that the original first directive keeps the whitespace of its own line when it moves to the last position,
    /// and the directive that keeps its middle position stays where it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task OriginalFirstDirectiveMovedBehindAMiddleDirectiveKeepsItsIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 // Header
                                 using System.Text;
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header
                                    using System;
                                    using System.Linq;
                                    using System.Text;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that the original first directive keeps the whitespace of its own line when it moves into another group,
    /// behind the group's blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task OriginalFirstDirectiveMovedIntoAnotherGroupKeepsItsIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 // Header
                                 using Zeta.Alpha;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header
                                    using System;

                                    using Zeta.Alpha;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that, with a blank line below the header, the comment that moves with the original first directive and that
    /// directive keep the whitespace of their own lines, and the new first directive starts at the original first directive's
    /// indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task CommentMovingWithTheOriginalFirstDirectiveKeepsItsIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 // Header

                                 // About Linq
                                 using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    // Header

                                    using System;

                                    // About Linq
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a comment on the line of the original first directive, which stays at the top (FMT-USING-009), leaves every
    /// directive at the scope's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task CommentOnTheOriginalFirstDirectivesLineChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 /* c */ using System.Linq;
                                 using System;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    /* c */ using System;
                                    using System.Linq;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a comment leading the second directive, while the first directive keeps its position, changes no
    /// directive's indentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task CommentLeadingASecondDirectiveThatMovesChangesNoDirectiveIndentation()
    {
        const string input = """
                             namespace N
                             {
                                 using System;
                                 using System.Text;
                                 // About Linq
                                 using System.Linq;
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    using System;
                                    // About Linq
                                    using System.Linq;
                                    using System.Text;
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies that a header in a compilation unit whose directives stand in column 0 keeps every directive in column 0, with
    /// and without a blank line below the header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderKeepsEveryDirectiveInColumnZeroInACompilationUnit()
    {
        const string input = """
                             // Header
                             using System.Linq;
                             using System;

                             class C
                             {
                             }
                             """;

        const string expected = """
                                // Header
                                using System;
                                using System.Linq;

                                class C
                                {
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);

        const string inputWithBlankLine = """
                                          // Header

                                          using System.Linq;
                                          using System;

                                          class C
                                          {
                                          }
                                          """;

        const string expectedWithBlankLine = """
                                             // Header

                                             using System;
                                             using System.Linq;

                                             class C
                                             {
                                             }
                                             """;

        await AssertOrganizes(inputWithBlankLine, expectedWithBlankLine, root => root);
    }

    /// <summary>
    /// Verifies that a header in a file-scoped namespace whose directives stand in column 0 keeps every directive in column 0
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-USING-011")]
    public async Task HeaderKeepsEveryDirectiveInColumnZeroInAFileScopedNamespace()
    {
        const string input = """
                             namespace N;

                             // Header

                             using System.Linq;
                             using System;

                             class C
                             {
                             }
                             """;

        const string expected = """
                                namespace N;

                                // Header

                                using System;
                                using System.Linq;

                                class C
                                {
                                }
                                """;

        await AssertOrganizes(input, expected, root => root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Single());
    }

    /// <summary>
    /// Verifies the specification example: with the compilation unit as scope, a <c>#region</c> in front of the block stays directly above the new first directive, the <c>#endregion</c> behind the block stays directly below the last one, and the region is not framed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-USING-012")]
    public async Task RegionOpeningTheBlockStaysAsWrittenAtTheTopOfTheReorderedBlock()
    {
        const string input = """
                             #region Usings
                             using System.Linq;
                             using System;
                             #endregion

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                #region Usings
                                using System;
                                using System.Linq;
                                #endregion

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies the change doc's case of a comment header above a directive: with the compilation unit as scope, the header, its blank line, and the directive stay at the top
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-USING-012")]
    public async Task HeaderDirectiveBehindAHeaderBlankLineStaysAtTheTopOfTheReorderedBlock()
    {
        const string input = """
                             // Header

                             #nullable enable
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                // Header

                                #nullable enable
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies the change doc's case of a comment behind a blank line below a header directive: with the compilation unit as scope, the directive, the blank line, and the comment stay at the top
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-USING-012")]
    public async Task CommentBehindABlankLineBelowAHeaderDirectiveStaysAtTheTopOfTheReorderedBlock()
    {
        const string input = """
                             #nullable enable

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                #nullable enable

                                // About Linq
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that in a namespace with a body, a header directive stays at the top as written, and the directives keep the whitespace of the original first directive's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("FMT-USING-011")]
    [Requirement("FMT-USING-012")]
    public async Task HeaderDirectiveChangesNoDirectiveIndentationInANamespaceWithABody()
    {
        const string input = """
                             namespace N
                             {
                                 #nullable enable
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        const string expected = """
                                namespace N
                                {
                                    #nullable enable
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await AssertOrganizes(input, expected, SelectNamespace);
    }

    /// <summary>
    /// Verifies the specification example: with the compilation unit as scope, a block led by a header directive whose order is canonical gets the blank line between its groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("SHARED-USING-009")]
    public async Task HeaderDirectiveBlockGetsItsGroupBlankLine()
    {
        const string input = """
                             #nullable enable
                             using System;
                             using Microsoft.Win32;

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                #nullable enable
                                using System;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Verifies that with the compilation unit as scope, a block behind a conditional group is not reordered and only the line two directives share is split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-030")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockBehindAConditionalGroupIsNotReordered()
    {
        const string input = """
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             using System.Linq; using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        const string expected = """
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using System.Linq;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        await AssertOrganizes(input, expected, root => root);
    }

    /// <summary>
    /// Selects the only namespace with a body in the document's root
    /// </summary>
    /// <param name="root">The document's root</param>
    /// <returns>The namespace</returns>
    private static SyntaxNode SelectNamespace(SyntaxNode root)
    {
        return root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
    }

    /// <summary>
    /// Replaces every four spaces of indentation at the start of a line with one tab
    /// </summary>
    /// <param name="text">Text indented with spaces</param>
    /// <returns>The text indented with tabs</returns>
    private static string IndentWithTabs(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            var spaces = line.Length - line.TrimStart(' ').Length;

            lines[index] = $"{new string('\t', spaces / 4)}{new string(' ', spaces % 4)}{line[spaces..]}";
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Organizes the using block of the selected scope through <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>
    /// under LF and CRLF, and asserts the document's text, written with the document's line ending
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectScope">Selects the scope in the document's root</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private Task AssertOrganizes(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectScope)
    {
        return AssertOrganizes(inputWithLf, expectedWithLf, selectScope, _lineEndings);
    }

    /// <summary>
    /// Organizes the using block of the selected scope through <see cref="ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync"/>
    /// under the given line endings, and asserts the document's text, written with the document's line ending
    /// </summary>
    /// <param name="inputWithLf">Input with LF line endings</param>
    /// <param name="expectedWithLf">Expected output with LF line endings</param>
    /// <param name="selectScope">Selects the scope in the document's root</param>
    /// <param name="lineEndings">The line endings to run under</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertOrganizes(string inputWithLf, string expectedWithLf, Func<SyntaxNode, SyntaxNode> selectScope, IEnumerable<string> lineEndings)
    {
        foreach (var endOfLine in lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(inputWithLf, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

                Assert.IsNotNull(root);

                var result = await ReihitsuFormatter.OrganizeUsingDirectivesInDocumentAsync(document, selectScope(root), TestContext.CancellationToken);
                var text = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();

                Assert.AreEqual(NormalizeLineEndings(expectedWithLf, endOfLine), text, $"Document output mismatch under {DescribeLineEnding(endOfLine)} line endings.");
            }
        }
    }

    #endregion // Methods
}