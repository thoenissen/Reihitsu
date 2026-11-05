using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.StructuralTransforms;

/// <summary>
/// Tests that the structural-transform phase splits the attribute lists of declarations into one list per attribute
/// (FMT-STRUCT-028) and merges the lists of one target as FMT-STRUCT-029 states, asserted on the
/// output of that phase under LF and CRLF line endings, and that the line-break phase no longer changes the token sequence
/// of those lists (FMT-BREAK-027)
/// </summary>
[TestClass]
public class AttributeListStructuralTransformTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The name under which the pipeline observer reports the structural-transform phase
    /// </summary>
    private const string StructuralPhaseName = "StructuralTransformPhase";

    /// <summary>
    /// The name under which the pipeline observer reports the line-break phase
    /// </summary>
    private const string LineBreakPhaseName = "LineBreakPhase";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the structural-transform phase splits a list with several attributes on each owner FMT-STRUCT-028 names,
    /// and writes nothing between the lists it creates
    /// </summary>
    /// <param name="input">The input with LF line endings, holding exactly one attribute list</param>
    /// <param name="expected">The expected text from the first created <c>[</c> to the last created <c>]</c></param>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    [DataRow("[assembly: A, B]\n", "[assembly: A][assembly: B]", DisplayName = "Assembly")]
    [DataRow("[module: A, B]\n", "[module: A][module: B]", DisplayName = "Module")]
    [DataRow("[A, B] class D\n{\n    int _x;\n}\n", "[A][B]", DisplayName = "Class")]
    [DataRow("[A, B] struct D\n{\n    int _x;\n}\n", "[A][B]", DisplayName = "Struct")]
    [DataRow("[A, B] interface I\n{\n    void M();\n}\n", "[A][B]", DisplayName = "Interface")]
    [DataRow("[A, B] record R(int X);\n", "[A][B]", DisplayName = "Record")]
    [DataRow("[A, B] enum E\n{\n    X\n}\n", "[A][B]", DisplayName = "Enum")]
    [DataRow("[A, B] delegate void D();\n", "[A][B]", DisplayName = "Delegate")]
    [DataRow("class C\n{\n    [A, B] void M()\n    {\n    }\n}\n", "[A][B]", DisplayName = "Method")]
    [DataRow("class C\n{\n    [A, B] C()\n    {\n    }\n}\n", "[A][B]", DisplayName = "Constructor")]
    [DataRow("class C\n{\n    [A, B] ~C()\n    {\n    }\n}\n", "[A][B]", DisplayName = "Destructor")]
    [DataRow("class C\n{\n    [A, B] int P { get; set; }\n}\n", "[A][B]", DisplayName = "Property")]
    [DataRow("class C\n{\n    [A, B] int this[int i] { get { return i; } }\n}\n", "[A][B]", DisplayName = "Indexer")]
    [DataRow("class C\n{\n    [A, B] int _f;\n}\n", "[A][B]", DisplayName = "Field")]
    [DataRow("class C\n{\n    [A, B] event System.Action E;\n}\n", "[A][B]", DisplayName = "EventField")]
    [DataRow("class C\n{\n    [A, B] event System.Action E { add { } remove { } }\n}\n", "[A][B]", DisplayName = "Event")]
    [DataRow("class C\n{\n    [A, B] public static C operator +(C x, C y)\n    {\n        return x;\n    }\n}\n", "[A][B]", DisplayName = "Operator")]
    [DataRow("class C\n{\n    [A, B] public static implicit operator int(C c)\n    {\n        return 0;\n    }\n}\n", "[A][B]", DisplayName = "Conversion")]
    [DataRow("enum E\n{\n    [A, B] X\n}\n", "[A][B]", DisplayName = "EnumMember")]
    [DataRow("class C\n{\n    void M()\n    {\n        [A, B] void L()\n        {\n        }\n    }\n}\n", "[A][B]", DisplayName = "LocalFunction")]
    public void ListOfEachDeclarationOwnerIsSplitWithNothingBetweenTheCreatedLists(string input, string expected)
    {
        AssertAttributeListText(input, expected);
    }

    /// <summary>
    /// Verifies that each created list keeps the original list's target specifier, also a known specifier that is not the
    /// default target of its declaration
    /// </summary>
    /// <param name="list">The attribute list in front of the method</param>
    /// <param name="expected">The expected text from the first created <c>[</c> to the last created <c>]</c></param>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    [DataRow("[method: A, B]", "[method: A][method: B]", DisplayName = "Method")]
    [DataRow("[return: A, B]", "[return: A][return: B]", DisplayName = "Return")]
    [DataRow("[A, B, C]", "[A][B][C]", DisplayName = "ThreeAttributes")]
    public void CreatedListsKeepTheSpecifierAndTheWrittenOrder(string list, string expected)
    {
        AssertAttributeListText($"class C\n{{\n    {list} int M()\n    {{\n        return 0;\n    }}\n}}\n", expected);
    }

    /// <summary>
    /// Verifies that the whitespace, line breaks, and blank lines between two attributes are dropped with the comma, wherever
    /// the comma stands among them
    /// </summary>
    /// <param name="list">The attribute list in front of the method, with LF line endings</param>
    /// <param name="expected">The expected text from the first created <c>[</c> to the last created <c>]</c></param>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    [DataRow("[A,\n     B]", "[A][B]", DisplayName = "LineBreakBehindComma")]
    [DataRow("[A\n     , B]", "[A][B]", DisplayName = "LineBreakInFrontOfComma")]
    [DataRow("[A,\n\n     B]", "[A][B]", DisplayName = "BlankLineBehindComma")]
    [DataRow("[A\n\n     ,\n     B]", "[A][B]", DisplayName = "BlankLineInFrontOfAndLineBreakBehindComma")]
    [DataRow("[A , B]", "[A][B]", DisplayName = "SpaceOnBothSidesOfComma")]
    [DataRow("[A,B]", "[A][B]", DisplayName = "NoSpace")]
    [DataRow("[Obsolete( \"x\" ) , B( 1 )]", "[Obsolete( \"x\" )][B( 1 )]", DisplayName = "AttributeArguments")]
    public void SeparatorsBetweenTwoAttributesAreDroppedWithTheComma(string list, string expected)
    {
        AssertAttributeListText($"class C\n{{\n    {list} void M()\n    {{\n    }}\n}}\n", expected);
    }

    /// <summary>
    /// Verifies that the first created list holds nothing between its <c>[</c> and its specifier or attribute: whitespace and
    /// line breaks behind the original list's <c>[</c> are dropped, also when a target specifier follows them
    /// </summary>
    /// <param name="list">The attribute list in front of the method, with LF line endings</param>
    /// <param name="expected">The expected text from the first created <c>[</c> to the last created <c>]</c></param>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    [DataRow("[\n        A, B]", "[A][B]", DisplayName = "LineBreakBehindOpeningBracket")]
    [DataRow("[\n     A,\n     B]", "[A][B]", DisplayName = "LineBreakBehindOpeningBracketAndComma")]
    [DataRow("[  A, B]", "[A][B]", DisplayName = "SpacesBehindOpeningBracket")]
    [DataRow("[\n        method: A, B]", "[method: A][method: B]", DisplayName = "LineBreakBehindOpeningBracketInFrontOfSpecifier")]
    [DataRow("[ return: A, B]", "[return: A][return: B]", DisplayName = "SpaceBehindOpeningBracketInFrontOfSpecifier")]
    public void NothingBehindTheOpeningBracketStaysInTheFirstCreatedList(string list, string expected)
    {
        AssertAttributeListText($"class C\n{{\n    {list} void M()\n    {{\n    }}\n}}\n", expected);
    }

    /// <summary>
    /// Verifies that the first created list keeps the original list's leading trivia, comment included, and the last one its
    /// trailing trivia, and that a list with a comment in front of it and behind it is split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void FirstCreatedListKeepsTheLeadingTriviaAndTheLastTheTrailingTrivia()
    {
        const string input = """
                             class C
                             {
                                 // lead
                                 [A, B] // tail
                                 void M()
                                 {
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, endOfLine, description) =>
                          {
                              var lists = output.DescendantNodes().OfType<AttributeListSyntax>().ToList();

                              Assert.HasCount(2, lists, description);
                              Assert.AreEqual($"    // lead{endOfLine}    ", lists[0].GetLeadingTrivia().ToFullString(), description);
                              Assert.AreEqual($" // tail{endOfLine}", lists[1].GetTrailingTrivia().ToFullString(), description);
                              Assert.AreEqual("[A][B]", GetText(output, lists[0].GetFirstToken(), lists[1].GetLastToken()), description);
                              AssertNoElasticTrivia(output, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that the attribute lists of an accessor are not split or merged by the structural-transform phase, because
    /// their shape follows the layout of their accessor list (FMT-BREAK-032)
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void AccessorListsAreLeftToTheLineBreakPhase()
    {
        const string input = """
                             class C
                             {
                                 int P { [A] [B] get; [C, D] set; }

                                 int Q
                                 {
                                     [A, B] get;
                                     [C] [D] set;
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var accessorLists = output.DescendantNodes()
                                                        .OfType<AccessorDeclarationSyntax>()
                                                        .Select(accessor => string.Join(" ", accessor.AttributeLists.Select(list => list.ToString())))
                                                        .ToList();

                              Assert.AreSequenceEqual(new[] { "[A] [B]", "[C, D]", "[A, B]", "[C] [D]" }, accessorLists, description);
                          });
    }

    /// <summary>
    /// Verifies that a list of a parameter is not split by the structural-transform phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void ParameterListWithSeveralAttributesIsNotSplit()
    {
        const string input = """
                             class C
                             {
                                 void M([A, B] int x)
                                 {
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var lists = output.DescendantNodes().OfType<ParameterSyntax>().Single().AttributeLists;

                              Assert.HasCount(1, lists, description);
                              Assert.AreEqual("[A, B]", lists[0].ToString(), description);
                          });
    }

    /// <summary>
    /// Verifies that the structural-transform phase merges the lists without a specifier of a parameter into the first of
    /// them, and that the commas it creates carry no whitespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void ParameterListsWithoutSpecifierAreMergedWithoutWrittenWhitespace()
    {
        const string input = """
                             class C
                             {
                                 void M([A] [B] [C] int a)
                                 {
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var parameter = output.DescendantNodes().OfType<ParameterSyntax>().Single();

                              Assert.HasCount(1, parameter.AttributeLists, description);
                              Assert.AreEqual("[A,B,C]", parameter.AttributeLists[0].ToString(), description);
                              AssertNoLineBreak(GetGap(output, parameter.AttributeLists[0].CloseBracketToken, parameter.Type.GetFirstToken()), description);
                              AssertNoElasticTrivia(output, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that the structural-transform phase merges the lists of a type parameter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void TypeParameterListsAreMerged()
    {
        const string input = """
                             class C<[A] [B] T>
                             {
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var typeParameter = output.DescendantNodes().OfType<TypeParameterSyntax>().Single();

                              Assert.HasCount(1, typeParameter.AttributeLists, description);
                              Assert.AreEqual("[A,B]", typeParameter.AttributeLists[0].ToString(), description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that the lists of each target of one parameter are merged on their own into the first list of that target,
    /// across lists of another target between them, and keep their specifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void ListsOfEachTargetAreMergedOnTheirOwn()
    {
        const string input = """
                             record R([A] [property: B] [C] [property: D] int Id);
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var lists = output.DescendantNodes().OfType<ParameterSyntax>().Single().AttributeLists.Select(list => list.ToString()).ToList();

                              Assert.AreSequenceEqual(new[] { "[A,C]", "[property: B,D]" }, lists, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a <c>param:</c> list and a list without a specifier name the same target and are merged into the first
    /// of them, which keeps its specifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void ParamSpecifierAndNoSpecifierAreMergedIntoTheFirstList()
    {
        const string input = """
                             class C
                             {
                                 void M([param: A] [B] int c)
                                 {
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var lists = output.DescendantNodes().OfType<ParameterSyntax>().Single().AttributeLists.Select(list => list.ToString()).ToList();

                              Assert.AreSequenceEqual(new[] { "[param: A,B]" }, lists, description);
                          });
    }

    /// <summary>
    /// Verifies that a line break the author wrote in the stretch the merge rewrites stays between the merged list and the
    /// parameter's type, wherever it stood in that stretch
    /// </summary>
    /// <param name="lists">The attribute lists in front of the parameter type, with LF line endings</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    [DataRow("[A]\n           [B] ", DisplayName = "BehindTheFirstList")]
    [DataRow("[A] [B]\n           ", DisplayName = "BehindTheLastList")]
    [DataRow("[A] [B]\n           [C] ", DisplayName = "BehindARemovedList")]
    [DataRow("[A]\n           [B]\n           ", DisplayName = "BehindEveryList")]
    [DataRow("[A,\n            C] [B] ", DisplayName = "BehindACommaOfTheFirstList")]
    [DataRow("[A\n            , C] [B] ", DisplayName = "InFrontOfACommaOfTheFirstList")]
    [DataRow("[A\n           ] [B] ", DisplayName = "InFrontOfTheClosingBracketOfTheFirstList")]
    [DataRow("[A] [B\n           ] ", DisplayName = "InFrontOfTheClosingBracketOfARemovedList")]
    [DataRow("[A] [\n            B] ", DisplayName = "BehindTheOpeningBracketOfARemovedList")]
    public void LineBreakInTheStretchIsKeptOnce(string lists)
    {
        AssertLineBreaksBehindMergedList($"class C\n{{\n    void P({lists}int x, int y)\n    {{\n    }}\n}}\n", exactlyOne: true);
    }

    /// <summary>
    /// Verifies that a line break directly behind the <c>[</c> of the first list of the target stays there when the lists are
    /// merged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakDirectlyBehindTheOpeningBracketStaysThere()
    {
        ForEachLineEnding("class C\n{\n    void W([\n            A] [B] int x, int y)\n    {\n    }\n}\n",
                          (output, _, description) =>
                          {
                              var list = output.DescendantNodes().OfType<ParameterSyntax>().First().AttributeLists.Single();

                              Assert.AreEqual(1, CountLineBreaks(GetGap(output, list.OpenBracketToken, list.Attributes[0].GetFirstToken())), description);
                              Assert.AreEqual(2, list.Attributes.Count, description);
                              AssertNoElasticTrivia(output, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that a blank line in the stretch the merge rewrites leaves at least one line break between the merged list
    /// and the parameter's type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void BlankLineInTheStretchLeavesALineBreak()
    {
        AssertLineBreaksBehindMergedList("class C\n{\n    void P([A]\n\n           [B] int x, int y)\n    {\n    }\n}\n", exactlyOne: false);
    }

    /// <summary>
    /// Verifies that a line break in the stretch of a type parameter's lists stays between the merged list and the type
    /// parameter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void LineBreakInTheStretchOfATypeParameterIsKept()
    {
        const string input = """
                             class G<[A]
                                     [B] T, U>
                             {
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var typeParameter = output.DescendantNodes().OfType<TypeParameterSyntax>().First();

                              Assert.HasCount(1, typeParameter.AttributeLists, description);
                              Assert.AreEqual(1, CountLineBreaks(GetGap(output, typeParameter.AttributeLists[0].CloseBracketToken, typeParameter.Identifier)), description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that without a line break in the stretch the merge's output holds none between the merged list and the
    /// parameter's type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    public void NoLineBreakInTheStretchKeepsNone()
    {
        const string input = """
                             class C
                             {
                                 void R([A] [B] int x, int y)
                                 {
                                 }
                             }
                             """;

        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var parameter = output.DescendantNodes().OfType<ParameterSyntax>().First();

                              Assert.HasCount(1, parameter.AttributeLists, description);
                              AssertNoLineBreak(GetGap(output, parameter.AttributeLists[0].CloseBracketToken, parameter.Type.GetFirstToken()), description);
                          });
    }

    /// <summary>
    /// Verifies that the structural-transform phase merges the <c>param:</c> lists and the <c>typevar:</c> lists of an owner
    /// other than a parameter or type parameter, each into the first of them, does not split a <c>param:</c> or
    /// <c>typevar:</c> list, and splits the other lists of the owner
    /// </summary>
    /// <param name="input">The input with LF line endings, whose first declaration with attribute lists is the owner</param>
    /// <param name="expected">The expected lists of the owner, separated by <c>|</c></param>
    [TestMethod]
    [Requirement("FMT-STRUCT-029")]
    [DataRow("class C\n{\n    [param: A] [param: B] [typevar: C, D] [E, F] void M()\n    {\n    }\n}\n", "[param: A,B]|[typevar: C, D]|[E]|[F]", DisplayName = "MethodExample")]
    [DataRow("[param: A] [param: B] delegate void D();\n", "[param: A,B]", DisplayName = "Delegate")]
    [DataRow("[typevar: A] [typevar: B] class K\n{\n}\n", "[typevar: A,B]", DisplayName = "Class")]
    [DataRow("class C\n{\n    [param: A] [param: B] int P { get; set; }\n}\n", "[param: A,B]", DisplayName = "Property")]
    [DataRow("enum E\n{\n    [param: A] [param: B] X\n}\n", "[param: A,B]", DisplayName = "EnumMember")]
    [DataRow("class C\n{\n    void N()\n    {\n        [param: A] [param: B] void L()\n        {\n        }\n    }\n}\n", "[param: A,B]", DisplayName = "LocalFunction")]
    [DataRow("class C\n{\n    [param: A, B] void O()\n    {\n    }\n}\n", "[param: A, B]", DisplayName = "ParamListWithSeveralAttributes")]
    [DataRow("class C\n{\n    [typevar: A, B] [C] void O()\n    {\n    }\n}\n", "[typevar: A, B]|[C]", DisplayName = "TypevarListWithSeveralAttributes")]
    public void ParamAndTypevarListsOfAnotherOwnerAreMergedAndNotSplit(string input, string expected)
    {
        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var owner = output.DescendantNodes().First(node => node is MemberDeclarationSyntax or LocalFunctionStatementSyntax && GetAttributeLists(node).Count > 0);
                              var lists = string.Join("|", GetAttributeLists(owner).Select(list => list.ToString()));

                              Assert.AreEqual(expected, lists, description);
                              AssertNoElasticTrivia(output, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Verifies that the line-break phase leaves the token sequence of declaration, parameter, and type-parameter attribute
    /// lists unchanged, because how many attributes a list holds is decided before it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-027")]
    public void LineBreakPhaseDoesNotChangeTheTokensOfDeclarationAttributeLists()
    {
        const string input = """
                             [assembly: A, B]

                             class C<[A] [B] T>
                             {
                                 [A, B] void M([A] [B] int x, [property: C] [property: D] int y)
                                 {
                                 }

                                 [A,
                                  B] int P { get; set; }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var phase = ObservedPipeline.Run(NormalizeLineEndings(input, endOfLine), endOfLine).Single(observedPhase => observedPhase.Name == LineBreakPhaseName);
            var tokensBefore = phase.Input.DescendantTokens().Select(token => token.Text).ToList();
            var tokensAfter = phase.Output.DescendantTokens().Select(token => token.Text).ToList();

            Assert.AreSequenceEqual(tokensBefore, tokensAfter, $"Under {DescribeLineEnding(endOfLine)} line endings the line-break phase changed tokens:{Environment.NewLine}{phase.Input.ToFullString()}{Environment.NewLine}->{Environment.NewLine}{phase.Output.ToFullString()}");
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Runs the structural-transform phase on the input under LF and CRLF line endings and passes its output to the assertion
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="assert">Receives the phase output, the line ending, and a description of the line ending</param>
    private static void ForEachLineEnding(string input, Action<SyntaxNode, string, string> assert)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(input, endOfLine), endOfLine, StructuralPhaseName);

            assert(output, endOfLine, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output.ToFullString()}");
        }
    }

    /// <summary>
    /// Asserts the text from the first to the last attribute list of the phase output, that no created token carries elastic
    /// trivia, and that the output parses back to its tree
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="expected">The expected text from the first <c>[</c> to the last <c>]</c></param>
    private static void AssertAttributeListText(string input, string expected)
    {
        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var lists = output.DescendantNodes().OfType<AttributeListSyntax>().ToList();

                              Assert.IsNotEmpty(lists, description);
                              Assert.AreEqual(expected, GetText(output, lists[0].GetFirstToken(), lists[^1].GetLastToken()), description);
                              AssertNoElasticTrivia(output, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Asserts that the first parameter of the phase output holds one merged list followed by a line break in front of its
    /// type, and that the output parses back to its tree
    /// </summary>
    /// <param name="input">The input with LF line endings</param>
    /// <param name="exactlyOne">Whether exactly one line break is expected rather than at least one</param>
    private static void AssertLineBreaksBehindMergedList(string input, bool exactlyOne)
    {
        ForEachLineEnding(input,
                          (output, _, description) =>
                          {
                              var parameter = output.DescendantNodes().OfType<ParameterSyntax>().First();

                              Assert.HasCount(1, parameter.AttributeLists, description);

                              var lineBreaks = CountLineBreaks(GetGap(output, parameter.AttributeLists[0].CloseBracketToken, parameter.Type.GetFirstToken()));

                              if (exactlyOne)
                              {
                                  Assert.AreEqual(1, lineBreaks, description);
                              }
                              else
                              {
                                  Assert.IsGreaterThanOrEqualTo(1, lineBreaks, description);
                              }

                              AssertNoElasticTrivia(output, description);
                              AssertSameTree(output, description);
                          });
    }

    /// <summary>
    /// Returns the attribute lists of a member declaration or local function
    /// </summary>
    /// <param name="node">The member declaration or local function</param>
    /// <returns>Its attribute lists</returns>
    private static SyntaxList<AttributeListSyntax> GetAttributeLists(SyntaxNode node)
    {
        return node switch
               {
                   MemberDeclarationSyntax member => member.AttributeLists,
                   LocalFunctionStatementSyntax localFunction => localFunction.AttributeLists,
                   _ => default
               };
    }

    /// <summary>
    /// Returns the text of the phase output from the start of one token to the end of another
    /// </summary>
    /// <param name="output">The phase output</param>
    /// <param name="first">The first token</param>
    /// <param name="last">The last token</param>
    /// <returns>The text between the start of the first token and the end of the last</returns>
    private static string GetText(SyntaxNode output, SyntaxToken first, SyntaxToken last)
    {
        return output.ToFullString().Substring(first.SpanStart, last.Span.End - first.SpanStart);
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
    /// Counts the line breaks of a gap, a CRLF pair counted once
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <returns>The number of line breaks</returns>
    private static int CountLineBreaks(string gap)
    {
        return gap.Replace("\r\n", "\n").Count(character => character == '\n');
    }

    /// <summary>
    /// Asserts that a gap holds no line break
    /// </summary>
    /// <param name="gap">The gap</param>
    /// <param name="description">The description of the case</param>
    private static void AssertNoLineBreak(string gap, string description)
    {
        Assert.AreEqual(0, CountLineBreaks(gap), $"Unexpected line break in '{gap}'. {description}");
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
                            .SelectMany(token => token.LeadingTrivia.Concat(token.TrailingTrivia).Select(trivia => (Token: token, Trivia: trivia)))
                            .Where(entry => entry.Trivia.HasAnnotation(SyntaxAnnotation.ElasticAnnotation))
                            .Select(entry => $"trivia of {entry.Token.Kind()} '{entry.Token.Text}' at {entry.Token.SpanStart}")
                            .ToList();

        Assert.IsEmpty(elastic, $"{string.Join(Environment.NewLine, elastic)}{Environment.NewLine}{description}");
    }

    /// <summary>
    /// Asserts that the phase output parses back to its tree (FMT-PIPE-021)
    /// </summary>
    /// <param name="output">The phase output</param>
    /// <param name="description">The description of the case</param>
    private static void AssertSameTree(SyntaxNode output, string description)
    {
        var difference = PhaseTreeRoundTrip.FindDifference(output, CSharpParseOptions.Default);

        Assert.IsNull(difference, $"{difference}{Environment.NewLine}{description}");
    }

    #endregion // Methods
}