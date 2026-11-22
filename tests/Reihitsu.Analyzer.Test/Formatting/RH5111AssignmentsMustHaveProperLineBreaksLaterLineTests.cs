using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider"/> on declarators, parameters, and enum members that stand on a later line, whose column and the lines around them the fix keeps
/// </summary>
[TestClass]
public class RH5111AssignmentsMustHaveProperLineBreaksLaterLineTests : BatchCodeFixTestsBase<RH5111AssignmentsMustHaveProperLineBreaksAnalyzer, RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line of its parameter list joins it and keeps the parameter's
    /// column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterOnALaterLineIsJoinedInItsColumn()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                               {|#0:int b =
                                               5|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                                int b = 5)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member that starts a later line of its enum joins it and keeps the member's column as
    /// in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberOnALaterLineIsJoinedInItsColumn()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,
                                      {|#0:B =
                                      2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                       B = 2
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of a declaration indented with tabs keeps the tabs and
    /// spaces in front of the declarator as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyDeclaratorOnALaterLineOfATabIndentedDeclarationKeepsTheWhitespaceInFrontOfIt()
    {
        const string testData = "internal class TestClass\n{\n\tvoid Method()\n\t{\n\t\tint a = 1,\n\t\t    {|#0:b =\n\t\t        2|};\n\t}\n}\n";
        const string fixedData = "internal class TestClass\n{\n\tvoid Method()\n\t{\n\t\tint a = 1,\n\t\t    b = 2;\n\t}\n}\n";

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose documentation comment is written behind the member in front of it keeps
    /// that line as written, with nothing inserted in front of the comment, and keeps the member's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAsWritten()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First, /// Doc
                                    {|#0:Second =
                                        2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First, /// Doc
                                     Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter whose documentation comment is written behind the parameter in front of it keeps
    /// that line as written, with nothing inserted in front of the comment, and keeps the parameter's column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-010")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterWithDocumentationCommentBehindThePreviousParameterKeepsThatLineAsWritten()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first, /// Doc
                                                {|#0:int second =
                                                    1|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first, /// Doc
                                                 int second = 1)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind two blank lines keeps both blank lines, one of which document-level
    /// formatting would remove, and the member's column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindTwoBlankLinesKeepsBothBlankLines()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,


                                  {|#0:Second =
                                  2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,


                                   Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a conditional directive and a blank line keeps the directive, the blank
    /// line, and the columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindConditionalDirectiveKeepsTheDirectiveAndTheBlankLineAsInTheDocument()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                  #if !DEBUG

                                      {|#0:Second =
                                      2|},
                                #endif
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                   #if !DEBUG

                                       Second = 2,
                                 #endif
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a region directive adds none of the blank lines document-level formatting
    /// puts around the directive, and keeps the directive and the member in their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindRegionDirectiveAddsNoBlankLineAndKeepsTheColumns()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                  #region Values
                                  {|#0:Second =
                                  2|},
                                  #endregion
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                   #region Values
                                   Second = 2,
                                   #endregion
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a documentation comment adds no blank line above the comment, and keeps
    /// every line of the comment and the member in their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindDocumentationCommentAddsNoBlankLineAndKeepsTheColumns()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                      /// <summary>
                                      /// Second
                                      /// </summary>
                                  {|#0:Second =
                                  2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                       /// <summary>
                                       /// Second
                                       /// </summary>
                                   Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind two comments with a blank line between them keeps exactly that blank
    /// line, and both comments and the member in their columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindSeveralCommentsKeepsThemAsInTheDocument()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                      // b

                                  /* c */
                                      {|#0:Second =
                                      2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                       // b

                                   /* c */
                                       Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind two comments with a blank line between them keeps them as in the document
    /// under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindSeveralCommentsKeepsThemAsInTheDocumentWithCrLf()
    {
        const string testData = """
                                internal enum Values
                                {
                                    First,
                                      // b

                                  /* c */
                                      {|#0:Second =
                                      2|},
                                    Third,
                                }
                                """;
        const string fixedData = """
                                 internal enum Values
                                 {
                                     First,
                                       // b

                                   /* c */
                                       Second = 2,
                                     Third,
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter behind two blank lines and a comment keeps both blank lines, the comment, and the
    /// columns as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterBehindTwoBlankLinesAndCommentKeepsThemAsInTheDocument()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first,


                                      // second
                                                    {|#0:int second =
                                                    1|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,


                                       // second
                                                     int second = 1)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind a blank line on a later line of its declaration keeps the blank line and
    /// the declarator's column as in the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyDeclaratorBehindBlankLineKeepsTheBlankLineAndItsColumn()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 1,

                                              {|#0:second =
                                              2|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 1,

                                               second = 2;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of its declaration keeps the declarator in its column
    /// in the document, where formatting the document would put it in another column, and keeps the wrapped argument under
    /// the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyDeclaratorOnALaterLineKeepsItsColumnAndKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1,
                                              {|#0:b =
                                                  Other(1,
                                                        2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1,
                                               b = Other(1,
                                                         2);
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of its declaration, whose value is an object
    /// creation, puts the braces under new and the member one level deeper
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyDeclaratorOnALaterLineKeepsTheBracesOfAnObjectCreationUnderNew()
    {
        const string testData = """
                                class C
                                {
                                    int P { get; set; }

                                    void M()
                                    {
                                        C a = null,
                                              {|#0:b =
                                                  new C
                                                  {
                                                      P = 1
                                                  }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int P { get; set; }

                                     void M()
                                     {
                                         C a = null,
                                               b = new C
                                                   {
                                                       P = 1
                                                   };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of its declaration, whose value is a lambda, puts the
    /// lambda's block under its parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyDeclaratorOnALaterLineKeepsTheBlockOfALambdaUnderItsParameterList()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action c = null,
                                              {|#0:d =
                                                  () =>
                                                  {
                                                      M();
                                                  }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action c = null,
                                               d = () =>
                                                   {
                                                       M();
                                                   };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member that starts a later line of its enum keeps the member in its column and puts
    /// the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyEnumMemberOnALaterLineKeepsItsColumnAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,
                                      {|#0:B =
                                      1 +
                                      2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                       B = 1
                                           + 2
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line of its parameter list in another column than
    /// formatting the document gives it keeps the parameter in its column and puts the wrapped binary operator under the
    /// value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyParameterOnALaterLineKeepsItsColumnAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                                {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                                 string c = "x"
                                                            + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that shares its line with the parameter in front of it keeps it on that line,
    /// where formatting the document would put it on a line of its own, and puts the wrapped binary operator under the
    /// value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyParameterBehindAnotherParameterOnItsLineStaysOnThatLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                           int b, {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                            int b, string c = "x"
                                                              + "y")
                                     {
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind the closing line of a raw string literal that formatting moves keeps
    /// the literal's lines and the declarator as in the document and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyDeclaratorBehindTheClosingLineOfARawStringKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        string a = """
                                            x
                                            """, {|#0:b =
                                            "y" +
                                            "z"|};
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         string a = """
                                             x
                                             """, b = "y"
                                                      + "z";
                                     }
                                 }
                                 """";

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of the first declarator on the line after its type keeps the declarator in its column and the
    /// wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFirstDeclaratorOnTheLineAfterItsTypeKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int
                                            {|#0:a =
                                                Other(1,
                                                      2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int
                                             a = Other(1,
                                                       2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a field declarator that starts a later line of its declaration keeps the declarator in its
    /// column and the wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFieldDeclaratorOnALaterLineKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    private static int _a = 1,
                                                       {|#0:_b =
                                                           Other(1,
                                                                 2)|};
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     private static int _a = 1,
                                                        _b = Other(1,
                                                                   2);
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of a for initializer keeps the declarator in its
    /// column and the wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyForInitializerDeclaratorOnALaterLineKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        for (int i = 0,
                                                 {|#0:j =
                                                     Other(1,
                                                           2)|}; i < j; i++)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         for (int i = 0,
                                                  j = Other(1,
                                                            2); i < j; i++)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of a tab-indented declaration keeps the tabs in front
    /// of it and puts the wrapped argument under the first argument, with each tab in front of the declarator counted as
    /// one column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyDeclaratorOnALaterLineOfATabIndentedDeclarationKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = "class C\n{\n\tstatic int Other(int x, int y)\n\t{\n\t\treturn x + y;\n\t}\n\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    {|#0:b =\n\t\t        Other(1,\n\t\t              2)|};\n\t}\n}\n";
        const string fixedData = "class C\n{\n\tstatic int Other(int x, int y)\n\t{\n\t\treturn x + y;\n\t}\n\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    b = Other(1,\n                2);\n\t}\n}\n";

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first parameter on the line after the opening parenthesis keeps it on its line in its
    /// column, where formatting the document would join it to the parenthesis, and puts the wrapped binary operator under
    /// the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyFirstParameterOnTheLineAfterTheOpeningParenthesisStaysOnItsLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(
                                        {|#0:int a
                                            = 1 +
                                              2|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(
                                         int a = 1
                                                 + 2)
                                     {
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind the opening brace keeps it on that line, where formatting the
    /// document would put it on a line of its own, and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyEnumMemberBehindTheOpeningBraceStaysOnItsLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E { {|#0:A =
                                           1 +
                                           2|} }
                                """;
        const string fixedData = """
                                 enum E { A = 1
                                              + 2 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first declarator behind its type and several spaces keeps those spaces and puts the
    /// wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFirstDeclaratorBehindItsTypeAndSeveralSpacesKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int    {|#0:a =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int    a = Other(1,
                                                          2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first parameter behind the opening parenthesis and several spaces keeps those spaces
    /// and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFirstParameterBehindTheOpeningParenthesisAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(   {|#0:int a
                                                  = 1 +
                                                    2|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(   int a = 1
                                                       + 2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter behind the parameter in front of it and several spaces keeps those spaces and
    /// puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyParameterBehindAnotherParameterAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,    {|#0:string c
                                                         = "x" +
                                                           "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,    string c = "x"
                                                                 + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind the declarator in front of it and several spaces keeps those spaces and
    /// puts the wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyDeclaratorBehindAnotherDeclaratorAndSeveralSpacesKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1,    {|#0:b =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1,    b = Other(1,
                                                                 2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind the enum member in front of it and several spaces keeps those spaces
    /// and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyEnumMemberBehindAnotherEnumMemberAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,    {|#0:B =
                                        1 +
                                        2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,    B = 1
                                                   + 2
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter behind the closing line of a raw string literal that formatting moves keeps the
    /// literal's lines and the parameter as in the document and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyParameterBehindTheClosingLineOfARawStringKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string a = """
                                        x
                                        """, {|#0:string c =
                                        "y" +
                                        "z"|})
                                    {
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M(string a = """
                                         x
                                         """, string c = "y"
                                                         + "z")
                                     {
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first parameter on the line after the opening parenthesis, in the column joining it to
    /// the parenthesis would give it, keeps it on its line and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyFirstParameterOnTheLineAfterTheOpeningParenthesisInTheJoinedColumnKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(
                                           {|#0:int a
                                               = 1 +
                                                 2|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(
                                            int a = 1
                                                    + 2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator one space behind the declarator in front of it puts the wrapped argument under
    /// the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyDeclaratorOneSpaceBehindAnotherDeclaratorKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1, {|#0:b =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1, b = Other(1,
                                                              2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member one space behind the enum member in front of it puts the wrapped binary
    /// operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyEnumMemberOneSpaceBehindAnotherEnumMemberKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1, {|#0:B =
                                        1 +
                                        2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1, B = 1
                                                + 2
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line of its parameter list in the column formatting the
    /// document gives it puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyParameterOnALaterLineInTheColumnOfTheFormatterKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                           {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                            string c = "x"
                                                       + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind the last line of a multi-line comment that stands behind the
    /// separating comma is withheld: the fix formats the declarator in its declaration statement, and the formatter
    /// refuses that statement for the comment between the declaration's own tokens
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForADeclaratorBehindAMultiLineCommentBehindTheSeparatingComma()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1, /* x
                                                      y */ {|RH5111:b =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line behind a comment line keeps the comment and the
    /// parameter in their columns and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterOnALaterLineBehindACommentLineKeepsTheCommentAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                                // c
                                                {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                                 // c
                                                 string c = "x"
                                                            + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose documentation comment is written behind the member in front of it
    /// keeps that line as written, keeps the member in its column, and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A, /// Doc
                                      {|#0:B =
                                      1 +
                                      2|},
                                    C,
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A, /// Doc
                                       B = 1
                                           + 2,
                                     C,
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a conditional directive and a blank line keeps both and the member's
    /// column as in the document and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindConditionalDirectiveKeepsTheDirectiveAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                  #if !DEBUG

                                      {|#0:B =
                                      1 +
                                      2|},
                                #endif
                                    C,
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                   #if !DEBUG

                                       B = 1
                                           + 2,
                                 #endif
                                     C,
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing two enum members that start later lines of their enum, also through Fix All, keeps both members
    /// in their columns and puts each wrapped binary operator under its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyTwoEnumMembersOnLaterLinesAreFixedTogetherAndKeepTheWrappedOperatorsUnderTheirValues()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,
                                      {|#0:B =
                                      1 +
                                      2|},
                                      {|#1:C =
                                      3 +
                                      4|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                       B = 1
                                           + 2,
                                       C = 3
                                           + 4
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the one diagnostic marked in the source and the fix result under LF and under CRLF
    /// </summary>
    /// <param name="testData">The source with LF line endings</param>
    /// <param name="fixedData">The fixed source with LF line endings</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyUnderBothLineEndings(string testData, string fixedData)
    {
        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            int {|#0:first
                                                = 1|}, {|#1:second
                                                = 2|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             int first = 1, second = 2;
                                         }
                                     }
                                 }
                                 """;

        // Verifies that Fix All converges for multiple declarators in the same declaration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}