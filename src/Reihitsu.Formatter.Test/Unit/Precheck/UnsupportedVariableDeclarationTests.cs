using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Precheck;

/// <summary>
/// Tests for the first unsupported shape of the formatter, a comment inside a variable declaration (FMT-UNSUP-004): every
/// declaration kind, every place in the span, the excepted places, and the siblings that hold no variable declaration, each
/// under LF and CRLF line endings
/// </summary>
[TestClass]
public class UnsupportedVariableDeclarationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example of the issue's five cases: the input is returned unchanged, also the code that
    /// holds no such comment, and the first comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SpecExampleCommentsBetweenTheDeclarationsOwnTokensAreUnsupported()
    {
        const string input = """
                             class C{
                                 void M(){
                                     int /* c1 */ a=1;
                                     int b /* c2 */=1;
                                     int c=// c3
                                         1;
                                     int d=1, /* c4 */ e=2;
                                     int f, // c5
                                         g;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c1 */");
    }

    /// <summary>
    /// Verifies the specification example of comments in the type, among the modifiers, among the attributes and in their
    /// arguments, and on their own line in front of the value and of the semicolon: the input is refused, and the first
    /// comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SpecExampleCommentsAmongTheAttributesModifiersAndTypeAreUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 System.Collections.Generic.Dictionary<int, // first
                                     string> _map;

                                 private
                                     // c
                                     int _b;

                                 [Obsolete /* why */]
                                 private int _c;

                                 [Obsolete("old" /* why */)]
                                 private int _e;

                                 private /* c */ static int _d;

                                 void M()
                                 {
                                     int c =
                                         // c
                                         1;
                                     int d
                                         // c
                                         ;
                                 }
                             }
                             """;

        AssertUnsupported(input, "// first");
    }

    /// <summary>
    /// Verifies the specification example of comments between a value and the comma, semicolon, or closing parenthesis
    /// behind it, and on their own line in front of the closing parenthesis of a using statement: the input is refused
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SpecExampleCommentsBetweenAValueAndTheTokenBehindItAreUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var timeout = 30 /* seconds */;
                                     int b = 1 /* c */, c = 2;

                                     using (var s = Open() /* c */)
                                     {
                                     }

                                     using (var t = Open()
                                         // c
                                         )
                                     {
                                     }
                                 }

                                 System.IDisposable Open() => null;
                             }
                             """;

        AssertUnsupported(input, "/* seconds */");
    }

    /// <summary>
    /// Verifies the specification example of the other side: a documentation comment and a comment above a declaration,
    /// behind its semicolon, inside an initializer value or a lambda body, inside an argument of a value, and behind the
    /// closing parenthesis of a using statement do not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SpecExampleCommentsOutsideTheDeclarationsOwnTokensAreSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 /// <summary>The count</summary>
                                 private int _count;

                                 void M()
                                 {
                                     // c6
                                     int a = 1; // c7

                                     var o = new Options
                                     {
                                         // c8
                                         Retries = 3,
                                     };
                                     Func<int> f = () =>
                                     {
                                         // c9
                                         return 1;
                                     };
                                     int b = Compute(1, /* c10 */ 2);

                                     using (var s = Open()) // c11
                                     {
                                     }
                                 }

                                 IDisposable Open() => null;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies the specification example: a local declared in a lambda body inside the value of another declaration is a
    /// declaration of its own, and its comment refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SpecExampleDeclarationInsideAValueIsCheckedOnItsOwn()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int> f = () =>
                                     {
                                         int /* c */ x = 1;
                                         return x;
                                     };
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies the specification example with a comment in every kind of variable declaration: the input is refused, and
    /// the comment of the first declaration in the text is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SpecExampleEveryKindOfVariableDeclarationIsChecked()
    {
        const string input = """
                             using System;

                             unsafe struct S
                             {
                                 private int _a, /* c */ _b;
                                 public event Action /* c */ Changed;
                                 public fixed int _buffer[4] /* c */;

                                 void M(int[] items)
                                 {
                                     for (int i = 0, /* c */ j = 0; i < j; i++)
                                     {
                                     }

                                     using var /* c */ stream = Open();

                                     fixed (int* /* c */ p = items)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of a local refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBetweenTheTypeAndTheNameIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int /* c */ a = 1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between a declarator's name and its <c>=</c> refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBetweenTheNameAndTheEqualsSignIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a /* c */ = 1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind a declarator's <c>=</c>, on its line, refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheEqualsSignIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = // c
                                         1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a comment between a separating comma and the next declarator refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBetweenTwoDeclaratorsIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1, /* c */ b = 2;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind a separating comma, on its line, refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindASeparatingCommaIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a, // c
                                         b;
                                 }
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a comment between a declarator's name and the separating comma behind it refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfASeparatingCommaIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a /* c */, b;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment in front of a value of one token, on the line of the <c>=</c>, refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfAValueIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = /* c */ 1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment on its own line in front of a value refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentOnItsOwnLineInFrontOfAValueIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a =
                                         // c
                                         1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a comment in front of the first token of a value of several tokens refuses the input: the leading
    /// trivia of the value's first token is not inside the value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfTheFirstTokenOfAValueOfSeveralTokensIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = /* c */ Compute(1);
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind the last token of a value of several tokens refuses the input: the trailing trivia of
    /// the value's last token is not inside the value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheLastTokenOfAValueOfSeveralTokensIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = Compute(1) /* c */;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind a value of one token refuses the input: a value of one token has no inside
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindAValueOfOneTokenIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1 /* c */;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between a value and the separating comma behind it refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBetweenAValueAndASeparatingCommaIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1 /* c */, b = 2;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment in the trailing trivia of the first token of a value of several tokens does not refuse the
    /// input: it is inside the value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInsideAValueBehindItsFirstTokenIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = Compute(/* c */ 1);
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in the leading trivia of the last token of a value of several tokens does not refuse the
    /// input: it is inside the value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInsideAValueInFrontOfItsLastTokenIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1 + /* c */ 2;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment on its own line between two tokens of a value does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentOnItsOwnLineInsideAValueIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = Compute(1,
                                                     // c
                                                     2);
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that comment-looking text inside a string literal of a value is no comment and does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentLookingTextInAStringValueIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     string a = "/* c */ // d";
                                     string b = @"/* c */";
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment on its own line in front of the semicolon refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentOnItsOwnLineInFrontOfTheSemicolonIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1
                                         // c
                                         ;
                                 }
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a comment behind the semicolon does not refuse the input: the trailing trivia of the token that ends
    /// the span is excepted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheSemicolonIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1; // c
                                     int b = 2; /* d */
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment on the line above a declaration does not refuse the input: the leading trivia of the span's
    /// first token is excepted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentAboveTheDeclarationIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // c
                                     int a = 1;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in front of a declaration on its line does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfTheDeclarationOnItsLineIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     /* c */ int a = 1;
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment above a field's attribute list does not refuse the input: it is the leading trivia of the
    /// field's first token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentAboveTheAttributeListOfAFieldIsSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 // c
                                 [Obsolete]
                                 private int _a;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a documentation comment of a field does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationCommentOfAFieldIsSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 /// <summary>
                                 /// The value
                                 /// </summary>
                                 private int _a;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment on its own line between a field's attribute list and its modifiers refuses the input: the
    /// attribute lists are in the span
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBetweenTheAttributeListAndTheModifiersIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 [Obsolete]
                                 // c
                                 private int _a;
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a comment inside a field's attribute list refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInsideAnAttributeListIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 [Obsolete /* c */]
                                 private int _a;
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment in the arguments of a field's attribute refuses the input: an attribute argument is no
    /// initializer value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAnAttributeArgumentIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 [Obsolete("old" /* c */)]
                                 private int _a;
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between two modifiers of a field refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBetweenTwoModifiersIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private /* c */ static int _a;
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment inside a field's type refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInsideTheTypeIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private System. /* c */ Int32 _a;
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment on its own line in front of a later declarator of a field refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfALaterDeclaratorOfAFieldIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int _a,
                                             // c
                                             _b;
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of an event field refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAnEventFieldIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 public event Action /* c */ Changed;
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind the brackets of a fixed-size buffer refuses the input: the brackets and the size are
    /// in the span
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheBracketsOfAFixedSizeBufferIsUnsupported()
    {
        const string input = """
                             unsafe struct S
                             {
                                 public fixed int _buffer[4] /* c */;
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment inside the brackets of a fixed-size buffer refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInsideTheSizeOfAFixedSizeBufferIsUnsupported()
    {
        const string input = """
                             unsafe struct S
                             {
                                 public fixed int _buffer[/* c */ 4];
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between two declarators of a <c>for</c> initializer refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAForInitializerIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     for (int i = 0, /* c */ j = 0; i < j; i++)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment in front of the first semicolon of a <c>for</c> statement refuses the input: that semicolon
    /// ends the initializer's span
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfTheFirstSemicolonOfAForStatementIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     for (int i = 0 /* c */; i < 1; i++)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind the first semicolon of a <c>for</c> statement does not refuse the input: it is the
    /// trailing trivia of the token that ends the span
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheFirstSemicolonOfAForStatementIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     for (int i = 0; /* c */ i < 1; i++)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of a using declaration refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAUsingDeclarationIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     using var /* c */ stream = Open();
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind the <c>using</c> keyword of a using declaration refuses the input: the keyword is the
    /// first token of the local declaration statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheUsingKeywordOfAUsingDeclarationIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     using /* c */ var stream = Open();
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind the <c>const</c> keyword of a local constant refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheConstKeywordIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     const /* c */ int A = 1;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of a using statement's declaration refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAUsingStatementIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     using (var /* c */ s = Open())
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment between the value and the closing parenthesis of a using statement refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInFrontOfTheClosingParenthesisOfAUsingStatementIsUnsupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     using (var s = Open() /* c */)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment behind the closing parenthesis of a using statement does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheClosingParenthesisOfAUsingStatementIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     using (var s = Open()) // c
                                     {
                                     }
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment behind the opening parenthesis of a using statement does not refuse the input: the
    /// statement's keyword and its opening parenthesis are not in the span
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentBehindTheOpeningParenthesisOfAUsingStatementIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     using (/* c */ var s = Open())
                                     {
                                     }
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of a <c>fixed</c> statement's declaration refuses the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAFixedStatementIsUnsupported()
    {
        const string input = """
                             unsafe class C
                             {
                                 void M(int[] items)
                                 {
                                     fixed (int* /* c */ p = items)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* c */");
    }

    /// <summary>
    /// Verifies that a comment on its own line in front of the closing parenthesis of a <c>fixed</c> statement refuses the
    /// input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentOnItsOwnLineInFrontOfTheClosingParenthesisOfAFixedStatementIsUnsupported()
    {
        const string input = """
                             unsafe class C
                             {
                                 void M(int[] items)
                                 {
                                     fixed (int* p = items
                                         // c
                                         )
                                     {
                                     }
                                 }
                             }
                             """;

        AssertUnsupported(input, "// c");
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment in front of a field's semicolon refuses the input when documentation comments
    /// are parsed, where it is the semicolon's leading trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationStyleCommentInFrontOfTheSemicolonIsUnsupportedWhenDocumentationCommentsAreParsed()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 int _a /** x */;
                             }
                             """;

        AssertUnsupported(input, "/** x */", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment in front of a field's semicolon refuses the input when documentation comments
    /// are not parsed, where it is the name's trailing trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationStyleCommentInFrontOfTheSemicolonIsUnsupportedWhenDocumentationCommentsAreNotParsed()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 int _a /** x */;
                             }
                             """;

        AssertUnsupported(input, "/** x */", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment between a value and a separating comma refuses the input when documentation
    /// comments are parsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationStyleCommentBetweenAValueAndACommaIsUnsupportedWhenDocumentationCommentsAreParsed()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1 /** c */, b;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/** c */", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment between a value and a separating comma refuses the input when documentation
    /// comments are not parsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationStyleCommentBetweenAValueAndACommaIsUnsupportedWhenDocumentationCommentsAreNotParsed()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1 /** c */, b;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/** c */", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that a <c>///</c> comment in front of a later declarator of a field refuses the input when documentation
    /// comments are parsed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void SingleLineDocumentationCommentInFrontOfALaterDeclaratorIsUnsupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int _a,
                                             /// <summary>Second.</summary>
                                             _b;
                             }
                             """;

        AssertUnsupported(input, "/// <summary>Second.</summary>", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that a <c>/** */</c> comment behind a field's type refuses the input when documentation comments are
    /// parsed, where it leads the first name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationStyleCommentBehindTheTypeIsUnsupportedWhenDocumentationCommentsAreParsed()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int /** c */ _a, _b;
                             }
                             """;

        AssertUnsupported(input, "/** c */", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that a <c>///</c> comment behind a field's type refuses the input in the documentation mode Diagnose
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DocumentationStyleCommentBehindTheTypeIsUnsupportedInDocumentationModeDiagnose()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int /// <summary>c</summary>
                                     _a, _b;
                             }
                             """;

        AssertUnsupported(input, "/// <summary>c</summary>", 0, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose));
    }

    /// <summary>
    /// Verifies that a directive between a field's declarators does not refuse the input: the shape is a comment, not a
    /// directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void DirectiveInsideADeclarationIsSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int _a,
                                 #if DEBUG
                                             _b,
                                 #endif
                                             _c;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that disabled text that looks like a comment between a field's declarators does not refuse the input:
    /// disabled text is no comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentLookingDisabledTextInsideADeclarationIsSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int _a,
                                 #if NEVER
                                             /* c */ _b,
                                 #endif
                                             _c;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in front of a property initializer's value does not refuse the input: a property holds no
    /// variable declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAPropertyInitializerIsSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 int P { get; } = /* c */ 1;
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in front of a parameter's default value does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAParameterDefaultIsSupported()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M(int x = /* c */ 1)
                                 {
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment in front of an enum member's value does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAnEnumMemberValueIsSupported()
    {
        const string input = """
                             enum E
                             {
                                 A = /* c */ 1
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of a <c>foreach</c> variable does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAForeachVariableIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     foreach (var /* c */ x in Items())
                                     {
                                     }
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of an <c>out</c> variable does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInAnOutVariableIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Read(out var /* c */ x);
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the variables of a deconstruction does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInADeconstructionIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var (a, /* c */ b) = Pair();
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the type and the name of a <c>catch</c> declaration does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInACatchDeclarationIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     try
                                     {
                                     }
                                     catch (Exception /* c */ e)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that a comment between the type and the designation of a declaration pattern does not refuse the input
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-004")]
    public void CommentInADeclarationPatternIsSupported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     if (Value() is int /* c */ x)
                                     {
                                     }
                                 }
                             }
                             """;

        AssertSupported(input);
    }

    /// <summary>
    /// Verifies that of several unsupported comments the one that starts first in the text is reported, also when it is not
    /// the first comment of its declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-004")]
    public void FirstOfSeveralCommentsInTheTextIsReported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int a = 1, b /* first */ = 2;
                                     int /* second */ c = 3;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* first */");
    }

    /// <summary>
    /// Verifies that comments that do not refuse the input do not become the reported location: the first unsupported
    /// comment is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    [Requirement("FMT-UNSUP-004")]
    public void RefusedDeclarationBehindSupportedCommentsIsReported()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     // above
                                     int a = 1; // behind
                                     int b /* refused */ = 2;
                                 }
                             }
                             """;

        AssertUnsupported(input, "/* refused */");
    }

    #endregion // Methods
}