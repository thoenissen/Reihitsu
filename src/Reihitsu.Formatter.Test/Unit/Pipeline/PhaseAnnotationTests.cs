using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that every phase keeps the annotations its input carries on the nodes and tokens of the code it keeps (FMT-PIPE-022)
/// </summary>
[TestClass]
public class PhaseAnnotationTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The kind of every annotation these tests put on the input
    /// </summary>
    private const string AnnotationKind = "PhaseAnnotationTests";

    /// <summary>
    /// The example of FMT-PIPE-022: an interpolated raw string literal whose content lines, hole line, and closing line stand
    /// left of its opening quotes
    /// </summary>
    private const string MisalignedLiteral = """"
                                             class C
                                             {
                                                 void M()
                                                 {
                                                     var s = $"""
                                                         a {Call(1,
                                                         2)} b
                                                         """;
                                                 }
                                             }
                                             """";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies the example of FMT-PIPE-022: the annotations on <c>Call</c> and on the text token that holds <c>a </c> stay
    /// on those tokens after every phase, and the final output is the moved literal
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void AnnotationsOnCallAndOnTheContentTextTokenSurviveTheRawStringMove()
    {
        const string expected = """"
                                class C
                                {
                                    void M()
                                    {
                                        var s = $"""
                                                 a {Call(1,
                                                         2)} b
                                                 """;
                                    }
                                }
                                """";

        foreach (var endOfLine in _lineEndings)
        {
            var result = FormatWithAnnotations(NormalizeLineEndings(MisalignedLiteral, endOfLine),
                                               endOfLine,
                                               null,
                                               root => root.DescendantTokens()
                                                           .Where(token => token.Text == "Call"
                                                                           || (token.IsKind(SyntaxKind.InterpolatedStringTextToken) && token.Text.EndsWith("a ", StringComparison.Ordinal)))
                                                           .Select(token => (SyntaxNodeOrToken)token)
                                                           .ToList());

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), result, $"Under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the annotations on the start token, the content tokens, and the closing token of a moved multi-line
    /// interpolated raw string literal, and on every node and token inside its holes, stay on them after every phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void AnnotationsOnEveryElementOfAMovedInterpolatedRawStringLiteralSurviveEveryPhase()
    {
        foreach (var endOfLine in _lineEndings)
        {
            FormatWithAnnotations(NormalizeLineEndings(MisalignedLiteral, endOfLine),
                                  endOfLine,
                                  null,
                                  root =>
                                  {
                                      var literal = root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>().Single();
                                      var holeNodes = literal.Contents.OfType<InterpolationSyntax>().SelectMany(hole => hole.DescendantNodesAndSelf());

                                      return literal.DescendantTokens()
                                                    .Select(token => (SyntaxNodeOrToken)token)
                                                    .Concat(holeNodes.Select(node => (SyntaxNodeOrToken)node))
                                                    .ToList();
                                  });
        }
    }

    /// <summary>
    /// Verifies that the annotation on a multi-line raw string literal token, whose content line and closing line the
    /// indentation phase moves, stays on that token although its text changes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void AnnotationOnAMovedRawStringLiteralTokenSurvivesEveryPhase()
    {
        const string input = """"
                             class C
                             {
                                 void M()
                                 {
                                     var s = """
                                         a
                                         """;
                                 }
                             }
                             """";

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var result = FormatWithAnnotations(normalizedInput,
                                               endOfLine,
                                               null,
                                               root => root.DescendantTokens()
                                                           .Where(token => token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
                                                           .Select(token => (SyntaxNodeOrToken)token)
                                                           .ToList());

            Assert.AreNotEqual(normalizedInput, result, $"The literal should move under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the annotations on a raw string literal nested in a hole of a moved interpolated raw string literal, and
    /// on the invocation around it, stay on them after every phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void AnnotationsInsideARawStringLiteralNestedInAHoleSurviveEveryPhase()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = $\"\"\"\n            a {Wrap(\"\"\"\n            b\n            \"\"\")} c\n            \"\"\";\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            FormatWithAnnotations(NormalizeLineEndings(input, endOfLine),
                                  endOfLine,
                                  null,
                                  root => root.DescendantTokens()
                                              .Where(token => token.Text == "Wrap"
                                                              || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
                                              .Select(token => (SyntaxNodeOrToken)token)
                                              .Concat(root.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(node => (SyntaxNodeOrToken)node))
                                              .ToList());
        }
    }

    /// <summary>
    /// Verifies that the annotations on the elements of an interpolated raw string literal that already stands under its
    /// opening quotes stay on them after every phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void AnnotationsOnAnAlignedInterpolatedRawStringLiteralSurviveEveryPhase()
    {
        const string input = "class C\n{\n    void M()\n    {\n        var s = $\"\"\"\n                 a {Call(1,\n                         2)} b\n                 \"\"\";\n    }\n}";

        foreach (var endOfLine in _lineEndings)
        {
            FormatWithAnnotations(NormalizeLineEndings(input, endOfLine),
                                  endOfLine,
                                  null,
                                  root => root.DescendantNodes()
                                              .OfType<InterpolatedStringExpressionSyntax>()
                                              .Single()
                                              .DescendantTokens()
                                              .Select(token => (SyntaxNodeOrToken)token)
                                              .ToList());
        }
    }

    /// <summary>
    /// Verifies that the annotations on the kept tokens and nodes of a converted expression body — the method name, the
    /// expression, and its literal — stay on them, while the replaced semicolon's annotation does not reach the semicolon the
    /// conversion creates
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void ConvertedExpressionBodyKeepsTheAnnotationsOfItsKeptElementsAndNotThoseOfTheReplacedSemicolon()
    {
        const string input = """
                             class C
                             {
                                 int Get() => 42;
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var tree = CSharpSyntaxTree.ParseText(normalizedInput, cancellationToken: TestContext.CancellationToken);
            var root = tree.GetRoot(TestContext.CancellationToken);
            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            var semicolonAnnotation = new SyntaxAnnotation(AnnotationKind, "semicolon");
            var annotated = Annotate(root, [method.Identifier, method.ExpressionBody.Expression, method.ExpressionBody.Expression.GetFirstToken()], out var kept);

            annotated = annotated.ReplaceToken(annotated.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().SemicolonToken,
                                               annotated.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().SemicolonToken.WithAdditionalAnnotations(semicolonAnnotation));

            var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));
            var output = FormattingPipeline.ExecutePhases(annotated,
                                                          context,
                                                          [PhaseStabilityCheck.CreateProductionPhases().Single(phase => phase.GetType().Name == "StructuralTransformPhase")],
                                                          null,
                                                          CancellationToken.None);
            var description = $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output.ToFullString()}";

            Assert.IsNotNull(output.DescendantNodes().OfType<MethodDeclarationSyntax>().Single().Body, description);
            AssertKept(output, kept, "StructuralTransformPhase", description);
            Assert.IsEmpty(output.GetAnnotatedNodesAndTokens(semicolonAnnotation).ToList(), description);
        }
    }

    /// <summary>
    /// Verifies that the annotations on a braced statement and on its first token stay on them when the structural-transform
    /// phase moves the statement into the block it creates
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-022")]
    public void BracedStatementKeepsItsAnnotations()
    {
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         Foo(); // trailing
                                 }
                             }
                             """;

        foreach (var endOfLine in _lineEndings)
        {
            FormatWithAnnotations(NormalizeLineEndings(input, endOfLine),
                                  endOfLine,
                                  null,
                                  root =>
                                  {
                                      var statement = root.DescendantNodes().OfType<IfStatementSyntax>().Single().Statement;

                                      return [statement, statement.GetFirstToken(), statement.GetLastToken()];
                                  });
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Puts one annotation of its own on every given node and token
    /// </summary>
    /// <param name="root">The root</param>
    /// <param name="elements">The nodes and tokens of the root to annotate</param>
    /// <param name="expectations">Each annotation with the kind and a description of the element that carries it</param>
    /// <returns>The annotated root</returns>
    private static SyntaxNode Annotate(SyntaxNode root, IReadOnlyList<SyntaxNodeOrToken> elements, out List<(SyntaxAnnotation Annotation, SyntaxKind Kind, string Description)> expectations)
    {
        var annotations = new Dictionary<SyntaxNodeOrToken, SyntaxAnnotation>();
        var collected = new List<(SyntaxAnnotation Annotation, SyntaxKind Kind, string Description)>();

        foreach (var element in elements.Distinct())
        {
            var description = element.IsToken
                                  ? $"token {element.Kind()} '{element.AsToken().Text}' at {element.SpanStart}"
                                  : $"node {element.Kind()} at {element.SpanStart}";
            var annotation = new SyntaxAnnotation(AnnotationKind, description);

            annotations.Add(element, annotation);
            collected.Add((annotation, element.Kind(), description));
        }

        expectations = collected;

        var nodes = annotations.Keys.Where(element => element.IsNode).Select(element => element.AsNode()).ToList();
        var tokens = annotations.Keys.Where(element => element.IsToken).Select(element => element.AsToken()).ToList();

        return root.ReplaceSyntax(nodes,
                                  (original, rewritten) => rewritten.WithAdditionalAnnotations(annotations[original]),
                                  tokens,
                                  (original, rewritten) => rewritten.WithAdditionalAnnotations(annotations[original]),
                                  [],
                                  (_, rewritten) => rewritten);
    }

    /// <summary>
    /// Asserts that a phase's output carries each annotation on exactly one element of the original kind
    /// </summary>
    /// <param name="output">The phase's output</param>
    /// <param name="expectations">The annotations with their kinds and descriptions</param>
    /// <param name="phaseName">The name of the phase</param>
    /// <param name="description">The description of the case</param>
    private static void AssertKept(SyntaxNode output, IReadOnlyList<(SyntaxAnnotation Annotation, SyntaxKind Kind, string Description)> expectations, string phaseName, string description)
    {
        var lost = new List<string>();

        foreach (var (annotation, kind, elementDescription) in expectations)
        {
            var carriers = output.GetAnnotatedNodesAndTokens(annotation).ToList();

            if (carriers.Count != 1
                || carriers[0].IsKind(kind) == false)
            {
                lost.Add($"{elementDescription}: {carriers.Count} carriers");
            }
        }

        Assert.IsEmpty(lost, $"{phaseName} dropped annotations (FMT-PIPE-022). {description}{Environment.NewLine}{string.Join(Environment.NewLine, lost)}{Environment.NewLine}{output.ToFullString()}");
    }

    /// <summary>
    /// Puts one annotation of its own on every selected node and token of the parsed input, formats it through the production
    /// pipeline, and fails for every annotation that a phase's output no longer carries on exactly one element of the
    /// original kind
    /// </summary>
    /// <param name="input">The input with the line ending already applied</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <param name="select">Selects the nodes and tokens to annotate</param>
    /// <returns>The final output</returns>
    private string FormatWithAnnotations(string input, string endOfLine, CSharpParseOptions parseOptions, Func<SyntaxNode, IReadOnlyList<SyntaxNodeOrToken>> select)
    {
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var selected = select(root);

        Assert.IsNotEmpty(selected, "The test should annotate at least one element.");

        var annotated = Annotate(root, selected, out var expectations);
        var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));
        var description = $"Under {DescribeLineEnding(endOfLine)} line endings.";
        var result = FormattingPipeline.Execute(annotated,
                                                context,
                                                (phaseName, _, after) => AssertKept(after, expectations, phaseName, description),
                                                CancellationToken.None);

        return result.ToFullString();
    }

    #endregion // Methods
}