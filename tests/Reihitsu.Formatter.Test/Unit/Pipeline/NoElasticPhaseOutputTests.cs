using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that no phase adds elastic trivia (FMT-PIPE-034): for a tree parsed from text, no phase output carries any, on any
/// token, also on a token inside a directive or a documentation comment, and the node-level entry points add none to a node
/// parsed from text
/// </summary>
[TestClass]
public class NoElasticPhaseOutputTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The example of FMT-PIPE-034: the <c>param:</c> lists of a setter in an accessor list on the declaration's line and in
    /// one over several lines
    /// </summary>
    private const string RequirementExample = """
                                              class C
                                              {
                                                  int P { get; [param: A] [param: B] set; }

                                                  int Q
                                                  {
                                                      get; [param: A]
                                                      [param: B] set;
                                                  }
                                              }
                                              """;

    /// <summary>
    /// The formatted example of FMT-PIPE-034
    /// </summary>
    private const string RequirementExampleExpected = """
                                                      class C
                                                      {
                                                          int P { get; [param: A, B] set; }

                                                          int Q
                                                          {
                                                              get;
                                                              [param: A, B] set;
                                                          }
                                                      }
                                                      """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Runs the pipeline on the input under LF and CRLF line endings and asserts that the output of every phase, each
    /// sub-phase of the preparation phase included, carries no elastic trivia
    /// </summary>
    /// <param name="input">The input source text, which is parsed from text and so carries no elastic trivia</param>
    private static void AssertNoPhaseOutputCarriesElasticTrivia(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var phases = ObservedPipeline.Run(normalizedInput, endOfLine);

            Assert.IsNull(PhaseTreeRoundTrip.FindElasticTrivia(phases[0].Input), "The parsed input must carry no elastic trivia.");
            Assert.Contains(ObservedPipeline.TabReplacementPhaseName, phases.Select(phase => phase.Name), "The pipeline should report the TabReplacement sub-phase.");
            Assert.Contains(ObservedPipeline.LineEndingsPhaseName, phases.Select(phase => phase.Name), "The pipeline should report the LineEndings sub-phase.");

            foreach (var phase in phases)
            {
                var elastic = PhaseTreeRoundTrip.FindElasticTrivia(phase.Output);

                Assert.IsNull(elastic, $"The output of {phase.Name} carries elastic trivia under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{elastic}{Environment.NewLine}{phase.Output.ToFullString()}");
            }
        }
    }

    /// <summary>
    /// Formats the first property of the input with <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, as a code fix
    /// passes it, under LF and CRLF line endings, and asserts the formatted text and that the returned document carries no
    /// elastic trivia
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="expected">The expected text of the returned document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatNodeInDocumentAddsNoElasticTriviaAsync(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            using (var workspace = new AdhocWorkspace())
            {
                var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
                var document = project.AddDocument("Test.cs", SourceText.From(NormalizeLineEndings(input, endOfLine)));
                var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);
                var target = root?.DescendantNodes().OfType<PropertyDeclarationSyntax>().First();

                Assert.IsNotNull(target);
                Assert.IsNull(PhaseTreeRoundTrip.FindElasticTrivia(root), "The parsed input must carry no elastic trivia.");

                var result = (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)).Output;
                var resultRoot = await result.GetSyntaxRootAsync(TestContext.CancellationToken);
                var resultText = (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
                var elastic = PhaseTreeRoundTrip.FindElasticTrivia(resultRoot);

                Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), resultText, $"Under {DescribeLineEnding(endOfLine)} line endings.");
                Assert.IsNull(elastic, $"The returned document carries elastic trivia under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{elastic}");
            }
        }
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies the example of FMT-PIPE-034: no phase output carries elastic trivia, the output of the line-break phase,
    /// which merges the <c>param:</c> lists, and every phase after it included
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void EveryPhaseOutputOfTheRequirementExampleCarriesNoElasticTrivia()
    {
        AssertNoPhaseOutputCarriesElasticTrivia(RequirementExample);
    }

    /// <summary>
    /// Verifies the output of the example of FMT-PIPE-034, which the requirement leaves unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void RequirementExampleKeepsItsOutput()
    {
        AssertRuleResult(RequirementExample, RequirementExampleExpected);
    }

    /// <summary>
    /// Verifies that no phase output carries elastic trivia where the merged accessor lists stay on the declaration's line and
    /// hold no specifier, and where the first list already holds a comma
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void MergedListsWithoutSpecifierOnTheDeclarationsLineCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 int P { [A] [B] get; set; }

                                 int R { [A, B] [C] get; set; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { [A, B] get; set; }

                                    int R { [A, B, C] get; set; }
                                }
                                """;

        AssertNoPhaseOutputCarriesElasticTrivia(input);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no phase output carries elastic trivia for an indexer whose accessor list the line-break phase joins onto
    /// several lines and whose first <c>param:</c> list already holds a comma, which the merge rebuilds
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void IndexerWhoseFirstParamListHoldsACommaCarriesNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 int this[int i] { get => 0; [param: A, B] [param: C] set { } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int i]
                                    {
                                        get => 0;
                                        [param: A, B, C] set
                                        {
                                        }
                                    }
                                }
                                """;

        AssertNoPhaseOutputCarriesElasticTrivia(input);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no phase output carries elastic trivia for a property whose three <c>param:</c> lists stand in an
    /// accessor list over several lines and whose first attribute holds an argument list with a comma
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void ThreeParamListsWithAnArgumentListCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 int R
                                 {
                                     get; [param: A(1,2)] [param: B] [param: C]
                                     set;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int R
                                    {
                                        get;
                                        [param: A(1, 2), B, C] set;
                                    }
                                }
                                """;

        AssertNoPhaseOutputCarriesElasticTrivia(input);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no phase output carries elastic trivia for an event whose <c>param:</c> lists stand on the
    /// <c>remove</c> accessor of an accessor list the line-break phase lays out over several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void EventRemoveAccessorParamListsCarryNoElasticTrivia()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 event EventHandler E { add { } [param: A] [param: B] remove { } }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    event EventHandler E
                                    {
                                        add
                                        {
                                        }
                                        [param: A, B] remove
                                        {
                                        }
                                    }
                                }
                                """;

        AssertNoPhaseOutputCarriesElasticTrivia(input);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no phase output carries elastic trivia for <c>typevar:</c> lists of a setter in an accessor list over
    /// several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void TypevarListsOfASetterCarryNoElasticTrivia()
    {
        const string input = """
                             class C
                             {
                                 int Q
                                 {
                                     get => 1;
                                     [typevar: A] [typevar: B] set { }
                                 }
                             }
                             """;

        AssertNoPhaseOutputCarriesElasticTrivia(input);
    }

    /// <summary>
    /// Verifies that no phase output carries elastic trivia, also on the tokens inside directives and documentation comments,
    /// for a file in which the phases rewrite region directives, a documentation comment, using directives, a field
    /// declaration, an expression body, an unbraced <c>if</c> body, switch sections, a raw string literal, tabs, and an
    /// accessor's attribute lists, and that each of the phases that rewrite these changes the file
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void EveryPhaseOutputOfAFileEveryPhaseRewritesCarriesNoElasticTrivia()
    {
        const string input = """"
                             using System.Text;
                             using System;

                             namespace N
                             {
                                 #region   fields of c
                                 class C
                                 {
                                     int a, b;

                                     ///<summary>Gets P.</summary>
                                     int P { get; [param: A] [param: B] set; }

                                     int Get() => 42;

                                     void M(int x)
                                     {
                                 	    if (x > 0)
                                             M(x);
                                         switch (x)
                                         {
                                             case 1:
                                             {
                                                 M(2);
                                                 break;
                                             }
                                             case 2:
                                                 M(3);
                                                 M(4);
                                                 break;
                                         }
                                         var s = """
                                 a
                                 """;
                                         var t = x+1;
                                     }
                                 }
                                 #endregion
                             }
                             """";

        AssertNoPhaseOutputCarriesElasticTrivia(input);

        var changedPhases = ObservedPipeline.Run(NormalizeLineEndings(input, "\n"), "\n")
                                            .Where(phase => phase.Input.ToFullString() != phase.Output.ToFullString())
                                            .Select(phase => phase.Name)
                                            .ToList();
        var expectedChanges = new[]
                              {
                                  ObservedPipeline.TabReplacementPhaseName,
                                  "StructuralTransformPhase",
                                  "RegionFormattingPhase",
                                  "DocumentationCommentFormattingPhase",
                                  "UsingDirectiveOrderingPhase",
                                  "LineBreakPhase",
                                  "SwitchCaseBracePhase",
                                  "BlankLinePhase",
                                  "HorizontalSpacingPhase",
                                  "IndentationPhase"
                              };

        foreach (var phaseName in expectedChanges)
        {
            Assert.Contains(phaseName, changedPhases, $"The fixture should make {phaseName} change the file; changed: {string.Join(", ", changedPhases)}");
        }
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, given a property parsed from text whose
    /// setter carries two <c>param:</c> lists, as the RH5111 code fix passes it, returns a document that carries no elastic
    /// trivia, with the lists merged and the initializer joined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public async Task FormatNodeInDocumentAsyncAddsNoElasticTriviaToAMergedAccessorList()
    {
        const string input = """
                             class C
                             {
                                 int P { get; [param: A] [param: B] set; }
                                     = 1;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { get; [param: A, B] set; } = 1;
                                }
                                """;

        await AssertFormatNodeInDocumentAddsNoElasticTriviaAsync(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, given a property parsed from text whose
    /// accessor list it lays out over several lines and whose setter carries <c>param:</c> lists, the first holding a comma,
    /// returns a document that carries no elastic trivia
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public async Task FormatNodeInDocumentAsyncAddsNoElasticTriviaToAMergedAccessorListOverSeveralLines()
    {
        const string input = """
                             class C
                             {
                                 int P { get => 1; [param: A, B] [param: C] set { } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P
                                    {
                                        get => 1;
                                        [param: A, B, C] set
                                        {
                                        }
                                    }
                                }
                                """;

        await AssertFormatNodeInDocumentAddsNoElasticTriviaAsync(input, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNode"/>, given a property parsed from text whose setter carries two
    /// <c>param:</c> lists, returns a node that carries no elastic trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-034")]
    public void FormatNodeAddsNoElasticTriviaToAMergedAccessorList()
    {
        const string input = """
                             class C
                             {
                                 int P { get; [param: A] [param: B] set; }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
            var target = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Single();

            var result = ReihitsuFormatter.FormatNode(target, cancellationToken: TestContext.CancellationToken).Output;
            var elastic = PhaseTreeRoundTrip.FindElasticTrivia(result);

            Assert.Contains("[param: A, B] set;", result.ToFullString(), $"Under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.IsNull(elastic, $"The returned node carries elastic trivia under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{elastic}");
        }
    }

    /// <summary>
    /// Verifies that the check the per-phase observer of every formatter fixture runs finds elastic trivia on a token of the
    /// code and on a token inside a directive, and finds none in a tree parsed from text
    /// </summary>
    [TestMethod]
    public void ElasticTriviaCheckFindsMarkersOnCodeAndDirectiveTokens()
    {
        const string input = """
                             #define X
                             class C
                             {
                                 int _a;
                             }
                             """;

        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        Assert.IsNull(PhaseTreeRoundTrip.FindElasticTrivia(root));

        var semicolon = root.DescendantTokens().Single(token => token.IsKind(SyntaxKind.SemicolonToken));
        var onCode = root.ReplaceToken(semicolon, semicolon.WithTrailingTrivia(SyntaxFactory.ElasticMarker));

        Assert.IsNotNull(PhaseTreeRoundTrip.FindElasticTrivia(onCode));

        var directive = root.DescendantNodes(descendIntoTrivia: true).OfType<DefineDirectiveTriviaSyntax>().Single();
        var directiveName = directive.Name;
        var onDirective = root.ReplaceToken(directiveName, directiveName.WithLeadingTrivia(SyntaxFactory.ElasticSpace));

        Assert.IsNotNull(PhaseTreeRoundTrip.FindElasticTrivia(onDirective));
    }

    #endregion // Tests
}