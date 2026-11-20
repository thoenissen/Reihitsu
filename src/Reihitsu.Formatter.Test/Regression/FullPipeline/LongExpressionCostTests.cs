using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Regression tests for the cost of whole-document formatting, as the CLI runs it, on a long chain of directly nested conditional
/// expressions and on a parent with many children: the cost must grow about in proportion to the length of the input, not with
/// its square. Each test formats one input of a base size and one eight times as large, and compares the memory the formatting
/// thread allocates and the time it takes. The comparison is a ratio, never an absolute budget: the allocated memory is exact
/// for a given input, and the time is the shortest of several runs, so a loaded machine slows both sizes and does not move the
/// ratio far. A cost in proportion to the length gives a ratio of about eight, a cost with the square of the length about
/// sixty-four, and the bound lies between them
/// </summary>
[TestClass]
[DoNotParallelize]
public class LongExpressionCostTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The factor between the length of the large input and the length of the base input
    /// </summary>
    private const int GrowthFactor = 8;

    /// <summary>
    /// The largest accepted ratio between the cost of the large input and the cost of the base input. A cost in proportion to
    /// the length gives about <see cref="GrowthFactor"/>, a cost with the square of the length about the square of it
    /// </summary>
    private const double MaximumCostRatio = 16;

    /// <summary>
    /// The number of runs of the base input, of which the shortest counts
    /// </summary>
    private const int BaseRuns = 5;

    /// <summary>
    /// The largest number of runs of the large input, of which the shortest counts; the runs stop once the ratio is within the
    /// bound
    /// </summary>
    private const int LargeRuns = 3;

    /// <summary>
    /// The depth of the base input of a conditional chain
    /// </summary>
    private const int BaseConditionalDepth = 100;

    /// <summary>
    /// The number of children of the base input of a long list
    /// </summary>
    private const int BaseListLength = 500;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that a <c>return</c> statement with a chain of conditionals nested in the false branch, one branch per line, is
    /// formatted at a cost in proportion to the chain's depth
    /// </summary>
    [TestMethod]
    public void ConditionalChainInFalseBranchOneBranchPerLineGrowsLinearly()
    {
        AssertCostGrowsLinearly("a chain of conditionals in the false branch, one branch per line", CreateFalseBranchChainOnePerLine, BaseConditionalDepth);
    }

    /// <summary>
    /// Verifies that a <c>return</c> statement with a chain of conditionals nested in the false branch, all on one line, is
    /// formatted at a cost in proportion to the chain's depth
    /// </summary>
    [TestMethod]
    public void ConditionalChainInFalseBranchOnOneLineGrowsLinearly()
    {
        AssertCostGrowsLinearly("a chain of conditionals in the false branch on one line", depth => CreateMethodReturning(CreateFalseBranchChain(depth)), BaseConditionalDepth);
    }

    /// <summary>
    /// Verifies that a <c>return</c> statement with a chain of conditionals nested in the true branch is formatted at a cost in
    /// proportion to the chain's depth
    /// </summary>
    [TestMethod]
    public void ConditionalChainInTrueBranchGrowsLinearly()
    {
        AssertCostGrowsLinearly("a chain of conditionals in the true branch", depth => CreateMethodReturning(CreateTrueBranchChain(depth)), BaseConditionalDepth);
    }

    /// <summary>
    /// Verifies that a field initializer with a chain of conditionals nested in the false branch is formatted at a cost in
    /// proportion to the chain's depth
    /// </summary>
    [TestMethod]
    public void ConditionalChainInFieldInitializerGrowsLinearly()
    {
        AssertCostGrowsLinearly("a chain of conditionals in a field initializer", depth => CreateClass($"    private static readonly int _value = {CreateFalseBranchChain(depth)};\n"), BaseConditionalDepth);
    }

    /// <summary>
    /// Verifies that a chain of conditionals each nested in parentheses in the false branch of the one before is formatted at a
    /// cost in proportion to the chain's depth
    /// </summary>
    [TestMethod]
    public void ParenthesizedConditionalChainGrowsLinearly()
    {
        AssertCostGrowsLinearly("a chain of parenthesized conditionals", CreateParenthesizedChain, BaseConditionalDepth);
    }

    /// <summary>
    /// Verifies that an array initializer with many elements on one line is formatted at a cost in proportion to its length
    /// </summary>
    [TestMethod]
    public void ArrayInitializerOnOneLineGrowsLinearly()
    {
        AssertCostGrowsLinearly("an array initializer on one line", count => CreateMethodBody($"var values = new[] {{ {CreateNumbers(count, ", ")} }};\n\n        return values.Length;"), BaseListLength);
    }

    /// <summary>
    /// Verifies that an array initializer with many elements, sixteen on each line, is formatted at a cost in proportion to its
    /// length
    /// </summary>
    [TestMethod]
    public void ArrayInitializerWithSeveralElementsPerLineGrowsLinearly()
    {
        AssertCostGrowsLinearly("an array initializer with sixteen elements per line", CreateWrappedArrayInitializer, BaseListLength);
    }

    /// <summary>
    /// Verifies that a collection expression with many elements on one line is formatted at a cost in proportion to its length
    /// </summary>
    [TestMethod]
    public void CollectionExpressionGrowsLinearly()
    {
        AssertCostGrowsLinearly("a collection expression", count => CreateMethodBody($"int[] values = [{CreateNumbers(count, ", ")}];\n\n        return values.Length;"), BaseListLength);
    }

    /// <summary>
    /// Verifies that an argument list with many arguments on one line is formatted at a cost in proportion to its length
    /// </summary>
    [TestMethod]
    public void ArgumentListGrowsLinearly()
    {
        AssertCostGrowsLinearly("an argument list", count => CreateClass($"    internal void M(params int[] values)\n    {{\n        M({CreateNumbers(count, ", ")});\n    }}\n"), BaseListLength);
    }

    /// <summary>
    /// Verifies that an enum with many members, one per line, is formatted at a cost in proportion to the number of members
    /// </summary>
    [TestMethod]
    public void EnumMembersGrowLinearly()
    {
        AssertCostGrowsLinearly("an enum with one member per line", count => $"namespace N;\n\ninternal enum E\n{{\n{string.Join(",\n", Enumerable.Range(0, count).Select(index => $"    V{index}"))}\n}}\n", BaseListLength);
    }

    /// <summary>
    /// Verifies that a class with many fields, a blank line between each two, is formatted at a cost in proportion to the
    /// number of fields
    /// </summary>
    [TestMethod]
    public void ClassMembersGrowLinearly()
    {
        AssertCostGrowsLinearly("a class with one field per line", count => CreateClass(string.Concat(Enumerable.Range(0, count).Select(index => $"    private int _field{index};\n\n"))), BaseListLength);
    }

    /// <summary>
    /// Verifies that a block with many statements is formatted at a cost in proportion to the number of statements
    /// </summary>
    [TestMethod]
    public void BlockStatementsGrowLinearly()
    {
        AssertCostGrowsLinearly("a block with one statement per line", count => CreateClass($"    internal void M()\n    {{\n{string.Concat(Enumerable.Range(0, count).Select(_ => "        M();\n"))}    }}\n"), BaseListLength);
    }

    /// <summary>
    /// Creates a document whose class holds the given members
    /// </summary>
    /// <param name="members">The members, each line ended with a line feed and indented by four spaces</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateClass(string members)
    {
        return $"namespace N;\n\ninternal class C\n{{\n{members}}}\n";
    }

    /// <summary>
    /// Creates a document whose class holds one method returning an <see langword="int"/> with the given body
    /// </summary>
    /// <param name="statements">The statements of the method, the first one without indentation</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateMethodBody(string statements)
    {
        return CreateClass($"    internal int M(int k)\n    {{\n        {statements}\n    }}\n");
    }

    /// <summary>
    /// Creates a document whose class holds one method that returns the given expression
    /// </summary>
    /// <param name="expression">The expression</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateMethodReturning(string expression)
    {
        return CreateMethodBody($"return {expression};");
    }

    /// <summary>
    /// Creates the numbers from zero, separated by the given text
    /// </summary>
    /// <param name="count">The number of numbers</param>
    /// <param name="separator">The text between two numbers</param>
    /// <returns>The numbers</returns>
    private static string CreateNumbers(int count, string separator)
    {
        return string.Join(separator, Enumerable.Range(0, count));
    }

    /// <summary>
    /// Creates a chain of conditionals, each nested directly in the false branch of the one before, on one line
    /// </summary>
    /// <param name="depth">The number of conditionals</param>
    /// <returns>The expression</returns>
    private static string CreateFalseBranchChain(int depth)
    {
        return $"{string.Join(" : ", Enumerable.Range(0, depth).Select(index => $"k == {index} ? {index}"))} : -1";
    }

    /// <summary>
    /// Creates a chain of conditionals, each nested directly in the true branch of the one before, on one line
    /// </summary>
    /// <param name="depth">The number of conditionals</param>
    /// <returns>The expression</returns>
    private static string CreateTrueBranchChain(int depth)
    {
        return $"{string.Concat(Enumerable.Range(0, depth).Select(index => $"k > {index} ? "))}0{string.Concat(Enumerable.Range(0, depth).Select(index => $" : {index}"))}";
    }

    /// <summary>
    /// Creates a document with a method that returns a chain of conditionals, each nested directly in the false branch of the one
    /// before, with one false branch per line
    /// </summary>
    /// <param name="depth">The number of conditionals</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateFalseBranchChainOnePerLine(int depth)
    {
        var branches = string.Concat(Enumerable.Range(1, depth - 1).Select(index => $"\n               : k == {index} ? {index}"));

        return CreateMethodReturning($"k == 0 ? 0{branches}\n               : -1");
    }

    /// <summary>
    /// Creates a document with a method that returns a chain of conditionals, each nested in parentheses in the false branch of
    /// the one before
    /// </summary>
    /// <param name="depth">The number of conditionals</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateParenthesizedChain(int depth)
    {
        return CreateMethodReturning($"{string.Concat(Enumerable.Range(0, depth).Select(index => $"k == {index} ? {index} : ("))}-1{new string(')', depth)}");
    }

    /// <summary>
    /// Creates a document with a method that holds an array initializer with sixteen elements per line
    /// </summary>
    /// <param name="count">The number of elements</param>
    /// <returns>The source text, with LF line endings</returns>
    private static string CreateWrappedArrayInitializer(int count)
    {
        var lines = Enumerable.Range(0, (count + 15) / 16)
                              .Select(line => $"                {string.Join(", ", Enumerable.Range(line * 16, Math.Min(16, count - (line * 16))))}");
        var elements = string.Join(",\n", lines);

        return CreateMethodBody($"var values = new[]\n                     {{\n{elements}\n                     }};\n\n        return values.Length;");
    }

    /// <summary>
    /// Formats the document once and returns the memory the formatting thread allocated and the time the formatting took
    /// </summary>
    /// <param name="text">The source text</param>
    /// <returns>The allocated bytes and the elapsed time</returns>
    private (long AllocatedBytes, TimeSpan Elapsed) Measure(string text)
    {
        var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken);

        tree.GetRoot(TestContext.CancellationToken);

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

        stopwatch.Stop();

        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        Assert.IsNull(result.Unsupported, $"The input must not be refused: {result.Unsupported?.Reason}");

        return (allocated, stopwatch.Elapsed);
    }

    /// <summary>
    /// Formats the document of the base size and the document <see cref="GrowthFactor"/> times as large, under LF and CRLF line
    /// endings, and asserts that neither the allocated memory nor the shortest time grows by more than
    /// <see cref="MaximumCostRatio"/>
    /// </summary>
    /// <param name="description">What the input holds, for the message</param>
    /// <param name="createDocument">Creates the document, with LF line endings, for a given length</param>
    /// <param name="baseLength">The length of the base input</param>
    private void AssertCostGrowsLinearly(string description, Func<int, string> createDocument, int baseLength)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var baseText = NormalizeLineEndings(createDocument(baseLength), endOfLine);
            var largeText = NormalizeLineEndings(createDocument(baseLength * GrowthFactor), endOfLine);

            Measure(baseText);

            var baseCost = Enumerable.Range(0, BaseRuns).Select(_ => Measure(baseText)).ToList();
            var baseBytes = baseCost.Min(cost => cost.AllocatedBytes);
            var baseTime = baseCost.Min(cost => cost.Elapsed);
            var largeBytes = long.MaxValue;
            var largeTime = TimeSpan.MaxValue;

            for (var run = 0; run < LargeRuns; run++)
            {
                var largeCost = Measure(largeText);

                largeBytes = Math.Min(largeBytes, largeCost.AllocatedBytes);
                largeTime = largeCost.Elapsed < largeTime ? largeCost.Elapsed : largeTime;

                if ((double)largeBytes / baseBytes <= MaximumCostRatio
                    && largeTime.Ticks <= baseTime.Ticks * MaximumCostRatio)
                {
                    break;
                }
            }

            var byteRatio = (double)largeBytes / baseBytes;
            var timeRatio = (double)largeTime.Ticks / Math.Max(1, baseTime.Ticks);
            var lengths = $"{baseLength} and {baseLength * GrowthFactor}";

            TestContext.WriteLine($"{description}, {endingName}: allocated {baseBytes / 1e6:F1} MB and {largeBytes / 1e6:F1} MB (ratio {byteRatio:F1}), shortest time {baseTime.TotalMilliseconds:F0} ms and {largeTime.TotalMilliseconds:F0} ms (ratio {timeRatio:F1})");

            Assert.IsLessThanOrEqualTo(MaximumCostRatio, byteRatio, $"Formatting {description} of lengths {lengths} under {endingName} line endings must allocate memory in proportion to the length: {baseBytes / 1e6:F1} MB and {largeBytes / 1e6:F1} MB, a ratio of {byteRatio:F1}.");
            Assert.IsLessThanOrEqualTo(MaximumCostRatio, timeRatio, $"Formatting {description} of lengths {lengths} under {endingName} line endings must take time in proportion to the length: {baseTime.TotalMilliseconds:F0} ms and {largeTime.TotalMilliseconds:F0} ms, a ratio of {timeRatio:F1}.");
        }
    }

    #endregion // Methods
}