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
/// for a given input, and the time is the cheapest of several alternating samples of about equal length, measured both as
/// processor time and as wall-clock time, so a loaded machine slows both sizes alike and does not move the ratio far. A cost in
/// proportion to the length gives a ratio of about eight, a cost with the square of the length about sixty-four, and the bound
/// lies between them
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
    /// The number of rounds that always run; a round takes one sample of each size, the base input formatted
    /// <see cref="GrowthFactor"/> times in a row and then the large input once, and the cheapest sample of each size counts
    /// </summary>
    private const int MinimumRounds = 3;

    /// <summary>
    /// The largest number of rounds; after <see cref="MinimumRounds"/>, the rounds stop once the memory and the time are within the bound
    /// </summary>
    private const int MaximumRounds = 9;

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
    /// Gets the shorter of two time spans
    /// </summary>
    /// <param name="first">The first time span</param>
    /// <param name="second">The second time span</param>
    /// <returns>The shorter time span</returns>
    private static TimeSpan Min(TimeSpan first, TimeSpan second)
    {
        return first < second ? first : second;
    }

    /// <summary>
    /// Formats the document the given number of times in a row and returns, per formatting, the memory the formatting thread
    /// allocated, the processor time the process used, and the wall-clock time that passed
    /// </summary>
    /// <param name="text">The source text</param>
    /// <param name="repetitions">How often the document is formatted</param>
    /// <returns>The cost of one formatting</returns>
    private (long AllocatedBytes, TimeSpan ProcessorTime, TimeSpan Elapsed) Measure(string text, int repetitions)
    {
        var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: TestContext.CancellationToken);

        tree.GetRoot(TestContext.CancellationToken);

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var processorTimeBefore = Environment.CpuUsage.TotalTime;
        var stopwatch = Stopwatch.StartNew();

        for (var repetition = 0; repetition < repetitions; repetition++)
        {
            var result = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported, $"The input must not be refused: {result.Unsupported?.Reason}");
        }

        stopwatch.Stop();

        var processorTime = Environment.CpuUsage.TotalTime - processorTimeBefore;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        return (allocated / repetitions, processorTime / repetitions, stopwatch.Elapsed / repetitions);
    }

    /// <summary>
    /// Formats the document of the base size and the document <see cref="GrowthFactor"/> times as large, under LF and CRLF line
    /// endings, and asserts that neither the allocated memory nor the time grows by more than <see cref="MaximumCostRatio"/>.
    /// Each sample of the base size formats it <see cref="GrowthFactor"/> times in a row, so a sample of either size lasts about
    /// as long when the cost is in proportion to the length, and a pause of the machine weighs on both alike. The samples of the
    /// two sizes alternate, and the cheapest of each size counts. The time counts as grown only when both the processor time and
    /// the wall-clock time have grown: the processor time ignores the time other processes take from the machine, the wall-clock
    /// time ignores work the runtime does on other threads, and a cost with the square of the length raises both
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

            Measure(baseText, GrowthFactor);

            var baseBytes = long.MaxValue;
            var largeBytes = long.MaxValue;
            var baseProcessorTime = TimeSpan.MaxValue;
            var largeProcessorTime = TimeSpan.MaxValue;
            var baseElapsed = TimeSpan.MaxValue;
            var largeElapsed = TimeSpan.MaxValue;
            var byteRatio = double.MaxValue;
            var processorTimeRatio = double.MaxValue;
            var elapsedRatio = double.MaxValue;

            for (var round = 1; round <= MaximumRounds; round++)
            {
                var baseCost = Measure(baseText, GrowthFactor);
                var largeCost = Measure(largeText, 1);

                baseBytes = Math.Min(baseBytes, baseCost.AllocatedBytes);
                largeBytes = Math.Min(largeBytes, largeCost.AllocatedBytes);
                baseProcessorTime = Min(baseProcessorTime, baseCost.ProcessorTime);
                largeProcessorTime = Min(largeProcessorTime, largeCost.ProcessorTime);
                baseElapsed = Min(baseElapsed, baseCost.Elapsed);
                largeElapsed = Min(largeElapsed, largeCost.Elapsed);
                byteRatio = (double)largeBytes / Math.Max(1, baseBytes);
                processorTimeRatio = (double)largeProcessorTime.Ticks / Math.Max(1, baseProcessorTime.Ticks);
                elapsedRatio = (double)largeElapsed.Ticks / Math.Max(1, baseElapsed.Ticks);

                if (round >= MinimumRounds
                    && byteRatio <= MaximumCostRatio
                    && Math.Min(processorTimeRatio, elapsedRatio) <= MaximumCostRatio)
                {
                    break;
                }
            }

            var lengths = $"{baseLength} and {baseLength * GrowthFactor}";
            var times = $"processor time {baseProcessorTime.TotalMilliseconds:F1} ms and {largeProcessorTime.TotalMilliseconds:F1} ms (ratio {processorTimeRatio:F1}), wall-clock time {baseElapsed.TotalMilliseconds:F1} ms and {largeElapsed.TotalMilliseconds:F1} ms (ratio {elapsedRatio:F1})";

            TestContext.WriteLine($"{description}, {endingName}: allocated {baseBytes / 1e6:F1} MB and {largeBytes / 1e6:F1} MB (ratio {byteRatio:F1}), {times}");

            Assert.IsLessThanOrEqualTo(MaximumCostRatio, byteRatio, $"Formatting {description} of lengths {lengths} under {endingName} line endings must allocate memory in proportion to the length: {baseBytes / 1e6:F1} MB and {largeBytes / 1e6:F1} MB, a ratio of {byteRatio:F1}.");
            Assert.IsLessThanOrEqualTo(MaximumCostRatio, Math.Min(processorTimeRatio, elapsedRatio), $"Formatting {description} of lengths {lengths} under {endingName} line endings must take time in proportion to the length: {times}.");
        }
    }

    #endregion // Methods
}