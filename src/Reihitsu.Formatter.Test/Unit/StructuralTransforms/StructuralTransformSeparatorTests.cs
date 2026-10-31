using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests that the structural transforms write only the separators their text needs (FMT-STRUCT-026), asserted on the
/// output of the structural-transform phase under LF and CRLF line endings
/// </summary>
[TestClass]
public class StructuralTransformSeparatorTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The name under which the pipeline observer reports the structural-transform phase
    /// </summary>
    private const string PhaseName = "StructuralTransformPhase";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that a created <c>return</c> in front of a literal that followed the arrow without whitespace gets exactly one
    /// space, because <c>return42</c> would read as one identifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfALiteralGetsOneSpace()
    {
        const string input = """
                             class C
                             {
                                 int Get()=>42;
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var returnStatement = output.DescendantNodes().OfType<ReturnStatementSyntax>().Single();

                              Assert.AreEqual(" ", GetGap(output, returnStatement.ReturnKeyword, returnStatement.Expression.GetFirstToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that a created <c>return</c> in front of an opening parenthesis that followed the arrow without whitespace
    /// gets no space, because the two read as two tokens without one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAParenthesisGetsNoSpace()
    {
        const string input = """
                             class C
                             {
                                 int Get()=>(42);
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var returnStatement = output.DescendantNodes().OfType<ReturnStatementSyntax>().Single();

                              Assert.AreEqual(string.Empty, GetGap(output, returnStatement.ReturnKeyword, returnStatement.Expression.GetFirstToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that a block comment that starts the line after the arrow without whitespace in front of it follows the created
    /// <c>return</c> without a space, because the author wrote none in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfACommentAtColumnZeroGetsNoSpace()
    {
        const string input = """
                             class C
                             {
                                 int Get() =>
                             /* c */ 42;
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.Contains("return/* c */ 42;", output.ToFullString(), description);
                          });
    }

    /// <summary>
    /// Verifies that a block comment that starts an indented line after the arrow follows the created <c>return</c> with the
    /// eight spaces the author wrote in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAnIndentedCommentKeepsTheAuthorsWhitespace()
    {
        const string input = """
                             class C
                             {
                                 int Get() =>
                                     /* c */ 42;
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.Contains("return        /* c */ 42;", output.ToFullString(), description);
                          });
    }

    /// <summary>
    /// Verifies that the created <c>}</c> of a converted expression body takes the trailing trivia of the replaced <c>;</c>,
    /// its comment, the whitespace in front of it, and its line break included
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedCloseBraceOfAConvertedBodyTakesTheSemicolonsTrailingTrivia()
    {
        const string input = """
                             class C
                             {
                                 int Get() => 42;    // tail
                                 int Other;
                             }
                             """;

        ForEachLineEnding(input,
                          (output, endOfLine, description) =>
                          {
                              var block = output.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body;

                              Assert.AreEqual("    // tail" + endOfLine, block.CloseBraceToken.TrailingTrivia.ToFullString(), description);
                          });
    }

    /// <summary>
    /// Verifies that a single-line comment that leads the converted expression on a line of its own is followed by a line
    /// break in front of the created <c>return</c>, and is preceded by one behind the inserted <c>{</c>, so the comment keeps
    /// leading the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void OwnLineCommentMovedInFrontOfTheCreatedReturnKeepsLeadingIt()
    {
        const string input = """
                             class C
                             {
                                 int Get() =>
                                     // c
                                     42;
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var block = output.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body;
                              var returnKeyword = block.Statements.Single().GetFirstToken();
                              var gap = GetGap(output, block.OpenBraceToken, returnKeyword);

                              AssertLineBreakBefore(gap, "// c", description);
                              AssertLineBreakAfter(gap, "// c", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a trailing single-line comment of a braced statement at the end of the file, with no line break behind
    /// it, gets a line break in front of the inserted <c>}</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TrailingSingleLineCommentWithoutLineBreakGetsOneInFrontOfTheInsertedBrace()
    {
        const string input = """
                             if (args.Length > 0)
                                 Foo(); // c
                             """;

        ForEachLineEnding(input,
                          (output, endOfLine, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;
                              var gap = GetGap(output, block.Statements.Single().GetLastToken(), block.CloseBraceToken);

                              Assert.AreEqual(" // c" + endOfLine, gap, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a trailing block comment of a braced statement on the line of <c>if</c> and <c>else</c> gets no line break
    /// in front of the inserted <c>}</c>, and that no line break follows that <c>}</c> in front of <c>else</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TrailingBlockCommentOnTheIfLineGetsNoLineBreakAroundTheInsertedBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a) Foo(); /* x */ else Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var ifStatement = output.DescendantNodes().OfType<IfStatementSyntax>().Single();
                              var block = (BlockSyntax)ifStatement.Statement;

                              Assert.AreEqual(" /* x */ ", GetGap(output, block.Statements.Single().GetLastToken(), block.CloseBraceToken), description);
                              AssertNoLineBreak(GetGap(output, block.CloseBraceToken, ifStatement.Else.ElseKeyword), description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that no line break follows an inserted <c>{</c> that the braced statement follows without a leading comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedOpenBraceWithoutLeadingCommentIsFollowedByNoLineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;

                              AssertNoLineBreak(GetGap(output, block.OpenBraceToken, block.Statements.Single().GetFirstToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that a line break follows an inserted <c>{</c> in front of a single-line comment that leads the braced
    /// statement, and that the author's line break behind the comment is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedOpenBraceInFrontOfALeadingCommentIsFollowedByALineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         // leading
                                         Foo();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;
                              var gap = GetGap(output, block.OpenBraceToken, block.Statements.Single().GetFirstToken());

                              AssertLineBreakBefore(gap, "// leading", description);
                              AssertLineBreakAfter(gap, "// leading", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that two block comments that lead a braced statement on its line stay on that line with the space the author
    /// wrote between them, behind a line break that follows the inserted <c>{</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TwoLeadingBlockCommentsStayOnTheStatementsLine()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         /* x */ /* y */ Foo();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;
                              var gap = GetGap(output, block.OpenBraceToken, block.Statements.Single().GetFirstToken());

                              AssertLineBreakBefore(gap, "/* x */", description);
                              Assert.EndsWith("/* x */ /* y */ ", gap, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that the blank line the author wrote between two single-line comments that lead a braced statement is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void BlankLineBetweenTwoLeadingCommentsIsKept()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         // one

                                         // two
                                         Foo();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, endOfLine, description) =>
                          {
                              var text = output.ToFullString();

                              Assert.Contains($"// one{endOfLine}{endOfLine}            // two{endOfLine}", text, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a line break separates an inserted <c>}</c> from a preprocessor directive behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedCloseBraceInFrontOfADirectiveIsFollowedByALineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                             #if DEBUG
                                     Bar();
                             #endif
                                 }
                             }
                             """;

        foreach (var parseOptions in new[] { CSharpParseOptions.Default, CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") })
        {
            ForEachLineEnding(input,
                              (output, _, description) =>
                              {
                                  var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;
                                  var gap = GetGap(output, block.CloseBraceToken, block.CloseBraceToken.GetNextToken());

                                  AssertLineBreakBefore(gap, "#if DEBUG", description);
                                  AssertSameTree(output, description, parseOptions);
                              },
                              parseOptions);
        }
    }

    /// <summary>
    /// Verifies that no line break follows an inserted <c>}</c> that a statement without a leading comment follows, while the
    /// line break the author wrote behind the braced statement stays in front of the <c>}</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedCloseBraceInFrontOfAStatementIsFollowedByNoLineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                                     Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, endOfLine, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;

                              Assert.AreEqual(endOfLine, GetGap(output, block.Statements.Single().GetLastToken(), block.CloseBraceToken), description);
                              AssertNoLineBreak(GetGap(output, block.CloseBraceToken, block.CloseBraceToken.GetNextToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that a line break follows an inserted <c>}</c> in front of a comment that leads the next statement, so the
    /// comment keeps leading it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedCloseBraceInFrontOfACommentLeadingTheNextStatementIsFollowedByALineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                                     // about Bar
                                     Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;

                              AssertLineBreakBefore(GetGap(output, block.CloseBraceToken, block.CloseBraceToken.GetNextToken()), "// about Bar", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a line break follows an inserted <c>}</c> in front of a comment that leads <c>else</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedCloseBraceInFrontOfACommentLeadingElseIsFollowedByALineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                                     // otherwise
                                     else
                                         Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var ifStatement = output.DescendantNodes().OfType<IfStatementSyntax>().Single();
                              var block = (BlockSyntax)ifStatement.Statement;

                              AssertLineBreakBefore(GetGap(output, block.CloseBraceToken, ifStatement.Else.ElseKeyword), "// otherwise", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a line break follows an inserted <c>}</c> in front of a documentation comment that leads a local function
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedCloseBraceInFrontOfADocumentationCommentIsFollowedByALineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                                     /// <summary>Local</summary>
                                     void Local()
                                     {
                                     }
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var block = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;

                              AssertLineBreakBefore(GetGap(output, block.CloseBraceToken, block.CloseBraceToken.GetNextToken()), "///", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that no line break follows the inserted <c>}</c> of an <c>if</c> body in front of <c>else</c>, nor the inserted
    /// <c>}</c> of the <c>else</c> body in front of the enclosing block's <c>}</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void InsertedCloseBracesInFrontOfElseAndOfTheEnclosingBraceAreFollowedByNoLineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo(); // trailing
                                     else
                                         Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var ifStatement = output.DescendantNodes().OfType<IfStatementSyntax>().Single();
                              var ifBlock = (BlockSyntax)ifStatement.Statement;
                              var elseBlock = (BlockSyntax)ifStatement.Else.Statement;

                              AssertNoLineBreak(GetGap(output, ifBlock.CloseBraceToken, ifStatement.Else.ElseKeyword), description);
                              AssertNoLineBreak(GetGap(output, elseBlock.CloseBraceToken, elseBlock.CloseBraceToken.GetNextToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that the gaps in front of the trailing comments of braced statements stay as the author wrote them: four
    /// spaces, none, and one space between two comments
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void GapsInFrontOfCarriedTrailingCommentsStayAsWritten()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();    // x
                                     if (a)
                                         Foo();// y
                                     if (a)
                                         Foo(); /* a */ // b
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, endOfLine, description) =>
                          {
                              var gaps = output.DescendantNodes()
                                               .OfType<IfStatementSyntax>()
                                               .Select(ifStatement => (BlockSyntax)ifStatement.Statement)
                                               .Select(block => GetGap(output, block.Statements.Single().GetLastToken(), block.CloseBraceToken))
                                               .ToList();

                              Assert.AreEqual("    // x" + endOfLine, gaps[0], description);
                              Assert.AreEqual("// y" + endOfLine, gaps[1], description);
                              Assert.AreEqual(" /* a */ // b" + endOfLine, gaps[2], description);
                          });
    }

    /// <summary>
    /// Verifies that the accessor transform writes one space on each side of the <c>=&gt;</c> it creates, also where the author
    /// wrote no whitespace around the removed tokens
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void AccessorTransformWritesOneSpaceOnEachSideOfTheCreatedArrow()
    {
        const string input = """
                             class C
                             {
                                 string _s;

                                 string P{get{return"";}set{_s=value;}}
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var arrows = output.DescendantNodes().OfType<AccessorDeclarationSyntax>().Select(accessor => accessor.ExpressionBody).ToList();

                              Assert.HasCount(2, arrows, description);

                              foreach (var arrow in arrows)
                              {
                                  Assert.IsNotNull(arrow, description);
                                  Assert.AreEqual(" ", GetGap(output, arrow.ArrowToken.GetPreviousToken(), arrow.ArrowToken), description);
                                  Assert.AreEqual(" ", GetGap(output, arrow.ArrowToken, arrow.Expression.GetFirstToken()), description);
                              }
                          });
    }

    /// <summary>
    /// Verifies that the get-only-member transform writes one space on each side of the <c>=&gt;</c> it creates, also where the
    /// author wrote no whitespace around the removed tokens
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheCreatedArrow()
    {
        const string input = """
                             class C
                             {
                                 string P{get{return"";}}
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var arrow = output.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single().ExpressionBody;

                              Assert.IsNotNull(arrow, description);
                              Assert.AreEqual(" ", GetGap(output, arrow.ArrowToken.GetPreviousToken(), arrow.ArrowToken), description);
                              Assert.AreEqual(" ", GetGap(output, arrow.ArrowToken, arrow.Expression.GetFirstToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that the get-only-member transform writes one space on each side of the <c>=&gt;</c> it creates behind an
    /// indexer's <c>]</c>, where the author wrote none around the removed tokens
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void GetOnlyMemberTransformWritesOneSpaceOnEachSideOfTheArrowBehindAnIndexer()
    {
        const string input = """
                             class C
                             {
                                 int[] _data;

                                 int this[int i]{get{return _data[i];}}
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var arrow = output.DescendantNodes().OfType<IndexerDeclarationSyntax>().Single().ExpressionBody;

                              Assert.IsNotNull(arrow, description);
                              Assert.AreEqual(" ", GetGap(output, arrow.ArrowToken.GetPreviousToken(), arrow.ArrowToken), description);
                              Assert.AreEqual(" ", GetGap(output, arrow.ArrowToken, arrow.Expression.GetFirstToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that no token the accessor and get-only-member transforms create carries elastic trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TokensCreatedForConvertedAccessorsCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 string _s;

                                 string P{get{return"";}set{_s=value;}}

                                 string Q { get { return _s; } }
                             }
                             """;

        ForEachLineEnding(input, (output, _, description) => AssertNoElasticTrivia(output, description));
    }

    /// <summary>
    /// Verifies that no token the brace transform creates carries elastic trivia, with and without comments around the braced
    /// statements
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TokensCreatedForBracedBodiesCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo();
                                     else
                                         Bar();
                                     if (a)
                                         // leading
                                         Foo(); // trailing
                                     // about Bar
                                     Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input, (output, _, description) => AssertNoElasticTrivia(output, description));
    }

    /// <summary>
    /// Verifies that no token the expression-body conversion creates carries elastic trivia, for a converted value expression,
    /// a converted statement expression, and a comment moved in front of the created statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TokensCreatedForConvertedExpressionBodiesCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 int Get() => 42;

                                 void Run() => Get();

                                 int Other() =>
                                     // c
                                     42;
                             }
                             """;

        ForEachLineEnding(input, (output, _, description) => AssertNoElasticTrivia(output, description));
    }

    /// <summary>
    /// Verifies the nested-<c>if</c> example of FMT-STRUCT-026: no line break stands between the inner inserted <c>}</c> and
    /// the outer inserted <c>}</c> behind it, and a line break stands between the outer inserted <c>}</c> and the comment that
    /// leads the next statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void NestedInsertedCloseBracesInFrontOfACommentBreakOnlyBehindTheOuterBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b)
                                 {
                                     if (a)
                                         if (b)
                                             Foo();
                                     // about Bar
                                     Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var (inner, outer) = GetNestedIfBlocks(output);

                              AssertNoLineBreak(GetGap(output, inner.CloseBraceToken, outer.CloseBraceToken), description);
                              AssertLineBreakBefore(GetGap(output, outer.CloseBraceToken, outer.CloseBraceToken.GetNextToken()), "// about Bar", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies the directive variant of the nested-<c>if</c> example of FMT-STRUCT-026: no line break between the two inserted
    /// <c>}</c>, and a line break between the outer one and <c>#if DEBUG</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void NestedInsertedCloseBracesInFrontOfADirectiveBreakOnlyBehindTheOuterBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b)
                                 {
                                     if (a)
                                         if (b)
                                             Foo();
                             #if DEBUG
                                     Bar();
                             #endif
                                 }
                             }
                             """;

        foreach (var parseOptions in new[] { CSharpParseOptions.Default, CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG") })
        {
            ForEachLineEnding(input,
                              (output, _, description) =>
                              {
                                  var (inner, outer) = GetNestedIfBlocks(output);

                                  AssertNoLineBreak(GetGap(output, inner.CloseBraceToken, outer.CloseBraceToken), description);
                                  AssertLineBreakBefore(GetGap(output, outer.CloseBraceToken, outer.CloseBraceToken.GetNextToken()), "#if DEBUG", description);
                                  AssertSameTree(output, description, parseOptions);
                              },
                              parseOptions);
        }
    }

    /// <summary>
    /// Verifies the <c>else</c> variant of the nested-<c>if</c> example of FMT-STRUCT-026: no line break between the inserted
    /// <c>}</c> of the inner <c>else</c> body and the outer inserted <c>}</c>, and a line break between the outer one and the
    /// comment that leads the next statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void NestedElseBodyCloseBraceInFrontOfTheOuterInsertedBraceGetsNoLineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b)
                                 {
                                     if (a)
                                         if (b)
                                             Foo();
                                         else
                                             Baz();
                                     // about Bar
                                     Bar();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var outerIf = output.DescendantNodes().OfType<IfStatementSyntax>().First();
                              var outer = (BlockSyntax)outerIf.Statement;
                              var innerIf = outer.Statements.OfType<IfStatementSyntax>().Single();
                              var innerElse = (BlockSyntax)innerIf.Else.Statement;

                              AssertNoLineBreak(GetGap(output, innerElse.CloseBraceToken, outer.CloseBraceToken), description);
                              AssertLineBreakBefore(GetGap(output, outer.CloseBraceToken, outer.CloseBraceToken.GetNextToken()), "// about Bar", description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies the other side of the nested-<c>if</c> example: three nested unbraced <c>if</c> statements in front of the
    /// enclosing block's <c>}</c> get no line break behind any inserted <c>}</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ThreeNestedInsertedCloseBracesGetNoLineBreak()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b, bool c)
                                 {
                                     if (a)
                                         if (b)
                                             if (c)
                                                 Foo();
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var blocks = output.DescendantNodes().OfType<IfStatementSyntax>().Select(ifStatement => (BlockSyntax)ifStatement.Statement).ToList();

                              Assert.HasCount(3, blocks, description);
                              AssertNoLineBreak(GetGap(output, blocks[2].CloseBraceToken, blocks[1].CloseBraceToken), description);
                              AssertNoLineBreak(GetGap(output, blocks[1].CloseBraceToken, blocks[0].CloseBraceToken), description);
                              AssertNoLineBreak(GetGap(output, blocks[0].CloseBraceToken, blocks[0].CloseBraceToken.GetNextToken()), description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies the <c>return</c> example of FMT-STRUCT-026 for an identifier: <c>int A() =&gt; a;</c> gives <c>return a;</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAnIdentifierGetsOneSpace()
    {
        AssertCreatedReturnStatement("int A() => a;", "return a;");
    }

    /// <summary>
    /// Verifies the <c>return</c> example of FMT-STRUCT-026 for an identifier that starts with a Unicode escape:
    /// <c>int B() =&gt; \u0061;</c> gives <c>return \u0061;</c>, because <c>return\u0061</c> reads as one identifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAUnicodeEscapeIdentifierGetsOneSpace()
    {
        AssertCreatedReturnStatement(@"int B() => \u0061;", @"return \u0061;");
    }

    /// <summary>
    /// Verifies the <c>return</c> example of FMT-STRUCT-026 for a verbatim identifier: <c>int C() =&gt; @a;</c> gives
    /// <c>return@a;</c>, because the two read as two tokens without a space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAVerbatimIdentifierGetsNoSpace()
    {
        AssertCreatedReturnStatement("int C() => @a;", "return@a;");
    }

    /// <summary>
    /// Verifies the <c>return</c> example of FMT-STRUCT-026 for a verbatim string: <c>int D() =&gt; @"s";</c> gives
    /// <c>return@"s";</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAVerbatimStringGetsNoSpace()
    {
        AssertCreatedReturnStatement("""int D() => @"s";""", """return@"s";""");
    }

    /// <summary>
    /// Verifies the <c>return</c> example of FMT-STRUCT-026 for a parenthesized expression: <c>int E() =&gt; (a);</c> gives
    /// <c>return(a);</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAParenthesizedIdentifierGetsNoSpace()
    {
        AssertCreatedReturnStatement("int E() => (a);", "return(a);");
    }

    /// <summary>
    /// Verifies the separator bullet of FMT-STRUCT-026 for a string: a created <c>return</c> in front of <c>"s"</c> gets no
    /// space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CreatedReturnInFrontOfAStringGetsNoSpace()
    {
        AssertCreatedReturnStatement("""string F() => "s";""", """return"s";""");
    }

    /// <summary>
    /// Verifies the at-arrow example of FMT-STRUCT-026 for a block comment that leads the expression on the line behind
    /// <c>=&gt; // c</c>: a line break stands between the created <c>return</c> and <c>/* d */</c>, which keeps leading the
    /// expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void BlockCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt()
    {
        AssertLineBreakBehindCreatedReturn("""
                                           class C
                                           {
                                               int Get() => // c
                                                   /* d */ 42;
                                           }
                                           """,
                                           "/* d */");
    }

    /// <summary>
    /// Verifies the at-arrow example of FMT-STRUCT-026 for a single-line comment on a line of its own between the arrow's line
    /// and the expression
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void OwnLineCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt()
    {
        AssertLineBreakBehindCreatedReturn("""
                                           class C
                                           {
                                               int Get() => // c
                                                   // d
                                                   42;
                                           }
                                           """,
                                           "// d");
    }

    /// <summary>
    /// Verifies the at-arrow example of FMT-STRUCT-026 for a comment that precedes the arrow on its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void BlockCommentLeadingTheExpressionBehindACommentInFrontOfTheArrowKeepsLeadingIt()
    {
        AssertLineBreakBehindCreatedReturn("""
                                           class C
                                           {
                                               int Get()
                                                   /* c */ =>
                                                   /* d */ 42;
                                           }
                                           """,
                                           "/* d */");
    }

    /// <summary>
    /// Verifies the at-arrow example of FMT-STRUCT-026 for a local function
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void LocalFunctionCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt()
    {
        AssertLineBreakBehindCreatedReturn("""
                                           class C
                                           {
                                               void M()
                                               {
                                                   int L() => // c
                                                       /* d */ 42;
                                               }
                                           }
                                           """,
                                           "/* d */");
    }

    /// <summary>
    /// Verifies the at-arrow example of FMT-STRUCT-026 for an operator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void OperatorCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt()
    {
        AssertLineBreakBehindCreatedReturn("""
                                           class C
                                           {
                                               public static C operator +(C x, C y) => // c
                                                   /* d */ x;
                                           }
                                           """,
                                           "/* d */");
    }

    /// <summary>
    /// Verifies the at-arrow example of FMT-STRUCT-026 for a conversion operator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ConversionCommentLeadingTheExpressionBehindAnAtArrowCommentKeepsLeadingIt()
    {
        AssertLineBreakBehindCreatedReturn("""
                                           class C
                                           {
                                               public static implicit operator int(C c) => // c
                                                   /* d */ 0;
                                           }
                                           """,
                                           "/* d */");
    }

    /// <summary>
    /// Verifies that a converted expression statement behind a comment at the arrow keeps the comment that leads it as its
    /// leading trivia, with no created token in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ExpressionStatementBehindAnAtArrowCommentKeepsItsLeadingComment()
    {
        const string input = """
                             class C
                             {
                                 void Run() => // c
                                     /* d */ Foo();
                             }
                             """;

        ForEachLineEnding(input, (output, _, description) => AssertSameTree(output, description));
    }

    /// <summary>
    /// Verifies that no token the transforms create around a parenthesized <c>throw</c> operand of an accessor carries elastic
    /// trivia, the <c>(</c> and <c>)</c> included
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ParenthesesAroundAThrowOperandOfAnAccessorCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 bool _b;

                                 int P { get { throw _b ? new System.Exception() : null; } set { _b = true; } }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.IsNotEmpty(output.DescendantNodes().OfType<ThrowExpressionSyntax>().Where(throwExpression => throwExpression.Expression is ParenthesizedExpressionSyntax).ToList(), description);
                              AssertNoElasticTrivia(output, description);
                          });
    }

    /// <summary>
    /// Verifies that no token the transforms create for a get-only property whose getter throws a parenthesized operand carries
    /// elastic trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ParenthesesAroundAThrowOperandOfAGetOnlyPropertyCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 bool _b;

                                 int P { get { throw _b ? new System.Exception() : null; } }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.IsNotEmpty(output.DescendantNodes().OfType<ThrowExpressionSyntax>().Where(throwExpression => throwExpression.Expression is ParenthesizedExpressionSyntax).ToList(), description);
                              AssertNoElasticTrivia(output, description);
                          });
    }

    /// <summary>
    /// Verifies that no token the transforms create for a get-only indexer whose getter throws a parenthesized assignment
    /// carries elastic trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void ParenthesesAroundAThrowOperandOfAGetOnlyIndexerCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 System.Exception _e;

                                 int this[int i] { get { throw _e = new System.Exception(); } }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.IsNotEmpty(output.DescendantNodes().OfType<ThrowExpressionSyntax>().Where(throwExpression => throwExpression.Expression is ParenthesizedExpressionSyntax).ToList(), description);
                              AssertNoElasticTrivia(output, description);
                          });
    }

    /// <summary>
    /// Verifies that the semicolon the empty-type transform creates carries no elastic trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void SemicolonCreatedForAnEmptyTypeCarriesNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.IsTrue(output.DescendantNodes().OfType<ClassDeclarationSyntax>().Single().SemicolonToken.IsKind(SyntaxKind.SemicolonToken), description);
                              AssertNoElasticTrivia(output, description);
                          });
    }

    /// <summary>
    /// Verifies that no token the expression-body conversion creates for a <c>throw</c> body carries elastic trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void TokensCreatedForAConvertedThrowBodyCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 void M() => throw new System.Exception();
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              Assert.IsNotNull(output.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body, description);
                              AssertNoElasticTrivia(output, description);
                          });
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Runs the structural-transform phase on the input under LF and CRLF line endings and passes its output to the assertion
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="assert">Receives the phase output, the line ending, and a description of the line ending</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    private static void ForEachLineEnding(string input, Action<SyntaxNode, string, string> assert, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(input, endOfLine), endOfLine, PhaseName, parseOptions);

            assert(output, endOfLine, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output.ToFullString()}");
        }
    }

    /// <summary>
    /// Returns the text between two tokens of the phase output
    /// </summary>
    /// <param name="output">The phase output</param>
    /// <param name="first">The token in front of the gap</param>
    /// <param name="second">The token behind the gap</param>
    /// <returns>The text between the end of the first token and the start of the second</returns>
    private static string GetGap(SyntaxNode output, SyntaxToken first, SyntaxToken second)
    {
        return output.ToFullString().Substring(first.Span.End, second.SpanStart - first.Span.End);
    }

    /// <summary>
    /// Asserts that a gap holds no line break
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <param name="description">The description of the case</param>
    private static void AssertNoLineBreak(string gap, string description)
    {
        Assert.IsFalse(gap.Contains('\n') || gap.Contains('\r'), $"Unexpected line break in '{gap}'. {description}");
    }

    /// <summary>
    /// Asserts that a line break stands in a gap in front of the given text
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <param name="text">The text in the gap</param>
    /// <param name="description">The description of the case</param>
    private static void AssertLineBreakBefore(string gap, string text, string description)
    {
        var index = gap.IndexOf(text, StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, index, $"'{text}' should stand in '{gap}'. {description}");
        Assert.IsTrue(gap.Substring(0, index).Contains('\n'), $"A line break should stand in front of '{text}'. {description}");
    }

    /// <summary>
    /// Asserts that a line break stands in a gap behind the given text
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <param name="text">The text in the gap</param>
    /// <param name="description">The description of the case</param>
    private static void AssertLineBreakAfter(string gap, string text, string description)
    {
        var index = gap.IndexOf(text, StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, index, $"'{text}' should stand in '{gap}'. {description}");
        Assert.IsTrue(gap.Substring(index + text.Length).Contains('\n'), $"A line break should stand behind '{text}'. {description}");
    }

    /// <summary>
    /// Returns the inserted blocks of an unbraced <c>if</c> nested in another
    /// </summary>
    /// <param name="output">The phase output</param>
    /// <returns>The inner and the outer block</returns>
    private static (BlockSyntax Inner, BlockSyntax Outer) GetNestedIfBlocks(SyntaxNode output)
    {
        var outer = (BlockSyntax)output.DescendantNodes().OfType<IfStatementSyntax>().First().Statement;
        var inner = (BlockSyntax)outer.Statements.OfType<IfStatementSyntax>().Single().Statement;

        return (inner, outer);
    }

    /// <summary>
    /// Converts the member in a class and asserts the text of the statement the conversion creates in the phase output
    /// </summary>
    /// <param name="member">The expression-bodied member</param>
    /// <param name="expectedStatement">The expected text of the created statement</param>
    private static void AssertCreatedReturnStatement(string member, string expectedStatement)
    {
        ForEachLineEnding($"class C\n{{\n    {member}\n}}",
                          (output, _, description) =>
                          {
                              var statement = output.DescendantNodes().OfType<ReturnStatementSyntax>().Single();

                              Assert.AreEqual(expectedStatement, statement.ToString(), description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Asserts that a line break stands between the created <c>return</c> and the comment that leads its expression in the
    /// phase output, and that the output parses back to its tree
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="comment">The comment that leads the expression</param>
    private static void AssertLineBreakBehindCreatedReturn(string input, string comment)
    {
        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var statement = output.DescendantNodes().OfType<ReturnStatementSyntax>().Single();

                              AssertLineBreakBefore(GetGap(output, statement.ReturnKeyword, statement.Expression.GetFirstToken()), comment, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Asserts that no token of the phase output carries elastic trivia. The parsed input carries none, so any elastic trivia
    /// in the output stands on a token the phase created
    /// </summary>
    /// <param name="output">The phase output</param>
    /// <param name="description">The description of the case</param>
    private static void AssertNoElasticTrivia(SyntaxNode output, string description)
    {
        var elastic = output.DescendantTokens()
                            .SelectMany(token => token.LeadingTrivia.Select(trivia => (Token: token, Side: "leading", Trivia: trivia))
                                                                    .Concat(token.TrailingTrivia.Select(trivia => (Token: token, Side: "trailing", Trivia: trivia))))
                            .Where(entry => entry.Trivia.HasAnnotation(SyntaxAnnotation.ElasticAnnotation))
                            .Select(entry => $"{entry.Side} trivia of {entry.Token.Kind()} '{entry.Token.Text}' at {entry.Token.SpanStart}")
                            .ToList();

        Assert.IsEmpty(elastic, $"{string.Join(Environment.NewLine, elastic)}{Environment.NewLine}{description}");
    }

    /// <summary>
    /// Asserts that the phase output parses back to its tree (FMT-PIPE-021)
    /// </summary>
    /// <param name="output">The phase output</param>
    /// <param name="description">The description of the case</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    private static void AssertSameTree(SyntaxNode output, string description, CSharpParseOptions parseOptions = null)
    {
        var difference = PhaseTreeRoundTrip.FindDifference(output, parseOptions ?? CSharpParseOptions.Default);

        Assert.IsNull(difference, $"{difference}{Environment.NewLine}{description}");
    }

    #endregion // Methods
}