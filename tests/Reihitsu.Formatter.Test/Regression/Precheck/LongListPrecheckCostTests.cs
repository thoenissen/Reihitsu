using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Precheck;

/// <summary>
/// Regression tests for the cost of the precheck on a single separated list with many elements: deciding whether a comment,
/// a directive, or disabled text stands in front of a comma on a later line must not take time that grows with the square of
/// the number of the list's elements. Each test checks one long list, with and without the shape, through whole-document
/// formatting and through the node-level check a code fix asks when it registers, and asserts the same answer the precheck
/// gives for a short list, within a time budget that a scan linear in the list's length meets by orders of magnitude
/// </summary>
[TestClass]
public class LongListPrecheckCostTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The number of elements of each long list
    /// </summary>
    private const int ElementCount = 30000;

    /// <summary>
    /// The comment that forms the shape in front of the last comma of a list
    /// </summary>
    private const string ShapeComment = "/* c */";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The time one precheck of a long list may take. A scan linear in the list's length takes milliseconds; a scan that asks
    /// for each comma's neighboring token takes tens of seconds, so the budget leaves a wide margin on both sides
    /// </summary>
    private static readonly TimeSpan _budget = TimeSpan.FromSeconds(3);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that a whole document with one long array initializer that holds no shape is not refused, and that the check
    /// stays within the budget
    /// </summary>
    [TestMethod]
    public void LongArrayInitializerWithoutShapeIsCheckedWithinBudget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var root = Parse(CreateArrayInitializerDocument(false), endOfLine);

            var unsupported = MeasureFindUnsupportedSyntax(root, DescribeLineEnding(endOfLine));

            Assert.IsNull(unsupported, $"The document must not be refused under {DescribeLineEnding(endOfLine)} line endings: {unsupported?.Reason}");
        }
    }

    /// <summary>
    /// Verifies that whole-document formatting refuses a document whose long array initializer holds a comment in front of its
    /// last comma, on a later line, reports that comment, returns the document unchanged, and stays within the budget
    /// </summary>
    [TestMethod]
    public void LongArrayInitializerWithShapeAtLastCommaIsRefusedWithinBudget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var text = NormalizeLineEndings(CreateArrayInitializerDocument(true), endOfLine);
            var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken);

            var stopwatch = Stopwatch.StartNew();
            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            stopwatch.Stop();

            Assert.IsNotNull(result.Unsupported, $"The document must be refused under {endingName} line endings.");
            Assert.AreEqual(ListSeparatorCommentReason, result.Unsupported.Reason);
            AssertLocationIsShape(result.Unsupported.Location, text, ShapeComment, 0, endingName);
            Assert.AreEqual(text, result.Output.GetText(TestContext.CancellationToken).ToString(), $"The refused document must be returned unchanged under {endingName} line endings.");
            AssertWithinBudget(stopwatch.Elapsed, $"whole-document formatting of a refused list under {endingName} line endings");
        }
    }

    /// <summary>
    /// Verifies that the node-level check a code fix asks about a long collection initializer it formats reports the comment in
    /// front of the initializer's last comma, and stays within the budget
    /// </summary>
    [TestMethod]
    public void LongCollectionInitializerWithShapeAtLastCommaIsFoundByTheNodeLevelCheckWithinBudget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var source = CreateDocument("System.Collections.Generic.List<int> M()\n    {\n        return new System.Collections.Generic.List<int> { ", " };\n    }", true);
            var text = NormalizeLineEndings(source, endOfLine);
            var initializer = Parse(source, endOfLine).DescendantNodes().OfType<InitializerExpressionSyntax>().Single();

            var unsupported = MeasureFindUnsupportedSyntax(initializer, endingName);

            Assert.IsNotNull(unsupported, $"The initializer must be refused under {endingName} line endings.");
            Assert.AreEqual(ListSeparatorCommentReason, unsupported.Reason);
            AssertLocationIsShape(unsupported.Location, text, ShapeComment, 0, endingName);
        }
    }

    /// <summary>
    /// Verifies that the node-level check a code fix asks about a long argument list reports the comment in front of the list's
    /// last comma, and stays within the budget
    /// </summary>
    [TestMethod]
    public void LongArgumentListWithShapeAtLastCommaIsFoundByTheNodeLevelCheckWithinBudget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var source = CreateDocument("void M(params int[] a)\n    {\n        M(", ");\n    }", true);
            var text = NormalizeLineEndings(source, endOfLine);
            var argumentList = Parse(source, endOfLine).DescendantNodes().OfType<ArgumentListSyntax>().Single();

            var unsupported = MeasureFindUnsupportedSyntax(argumentList, endingName);

            Assert.IsNotNull(unsupported, $"The argument list must be refused under {endingName} line endings.");
            Assert.AreEqual(ListSeparatorCommentReason, unsupported.Reason);
            AssertLocationIsShape(unsupported.Location, text, ShapeComment, 0, endingName);
        }
    }

    /// <summary>
    /// Verifies that the node-level check of a long enum member list that holds no shape answers that the enum is not refused,
    /// and stays within the budget
    /// </summary>
    [TestMethod]
    public void LongEnumWithoutShapeIsCheckedByTheNodeLevelCheckWithinBudget()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var enumDeclaration = Parse(CreateEnumDocument(), endOfLine).DescendantNodes().OfType<EnumDeclarationSyntax>().Single();

            var unsupported = MeasureFindUnsupportedSyntax(enumDeclaration, endingName);

            Assert.IsNull(unsupported, $"The enum must not be refused under {endingName} line endings: {unsupported?.Reason}");
        }
    }

    /// <summary>
    /// Creates a document whose class holds one array field with a long initializer
    /// </summary>
    /// <param name="withShape">Whether a comment stands in front of the initializer's last comma, on a later line</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateArrayInitializerDocument(bool withShape)
    {
        return CreateDocument("private static readonly int[] _a = new[] { ", " };", withShape);
    }

    /// <summary>
    /// Creates a document whose class holds one member with a long list of the numbers <c>0</c> to <see cref="ElementCount"/>
    /// on one line
    /// </summary>
    /// <param name="prefix">The member's text in front of the list</param>
    /// <param name="suffix">The member's text behind the list</param>
    /// <param name="withShape">Whether a comment stands in front of the list's last comma, which then starts a later line</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateDocument(string prefix, string suffix, bool withShape)
    {
        var builder = new StringBuilder();

        builder.Append("namespace N;\n\ninternal class C\n{\n    ");
        builder.Append(prefix);
        builder.Append(string.Join(", ", Enumerable.Range(0, ElementCount)));

        if (withShape)
        {
            builder.Append(' ').Append(ShapeComment).Append("\n            , ");
        }
        else
        {
            builder.Append(", ");
        }

        builder.Append(ElementCount);
        builder.Append(suffix);
        builder.Append("\n}\n");

        return builder.ToString();
    }

    /// <summary>
    /// Creates a document with one enum that holds a long member list on one line and no shape
    /// </summary>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateEnumDocument()
    {
        return $"namespace N;\n\ninternal enum E\n{{\n    {string.Join(", ", Enumerable.Range(0, ElementCount).Select(index => $"V{index}"))}\n}}\n";
    }

    /// <summary>
    /// Parses the source text with the given line ending
    /// </summary>
    /// <param name="source">The source text, with LF line endings</param>
    /// <param name="endOfLine">The line ending to use</param>
    /// <returns>The root of the parsed tree</returns>
    private static SyntaxNode Parse(string source, string endOfLine)
    {
        return CSharpSyntaxTree.ParseText(NormalizeLineEndings(source, endOfLine)).GetRoot();
    }

    /// <summary>
    /// Asks <see cref="ReihitsuFormatter.FindUnsupportedSyntax"/> about the node and asserts that the answer took no longer than
    /// the budget
    /// </summary>
    /// <param name="node">The node</param>
    /// <param name="endingName">The name of the line ending, for the message</param>
    /// <returns>The answer</returns>
    private static UnsupportedSyntax MeasureFindUnsupportedSyntax(SyntaxNode node, string endingName)
    {
        var stopwatch = Stopwatch.StartNew();
        var unsupported = ReihitsuFormatter.FindUnsupportedSyntax(node);

        stopwatch.Stop();

        AssertWithinBudget(stopwatch.Elapsed, $"the precheck of the {node.Kind()} under {endingName} line endings");

        return unsupported;
    }

    /// <summary>
    /// Asserts that the elapsed time is within the budget
    /// </summary>
    /// <param name="elapsed">The elapsed time</param>
    /// <param name="what">What was measured, for the message</param>
    private static void AssertWithinBudget(TimeSpan elapsed, string what)
    {
        Assert.IsLessThanOrEqualTo(_budget, elapsed, $"The time of {what} must stay within {_budget.TotalSeconds} s for a list of {ElementCount} elements; it took {elapsed.TotalSeconds:F2} s.");
    }

    #endregion // Methods
}