using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Unit tests for <see cref="FormattingTextAnalysisUtilities"/>
/// </summary>
[TestClass]
public class FormattingTextAnalysisUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that general string line collection covers literals and interpolated variants consistently
    /// </summary>
    [TestMethod]
    public void GetStringLineIndicesReturnsLinesForAllSupportedStringVariants()
    {
        const string source = """"
                              internal class Sample
                              {
                                  private static void Test(int value)
                                  {
                                      var regular = "regular";
                                      var utf8 = "utf8"u8;
                                      var regularInterpolated = $"regular {value}";
                                      var verbatim = @"line 1
                                                       line 2";
                                      var verbatimInterpolated = $@"line {value}
                                                                    line 2";
                                      var raw = """
                                                raw line
                                                """;
                                      var rawInterpolated = $$"""
                                                            raw {{value}}
                                                            """;
                                      var singleLineRaw = """single""";
                                      var singleLineRawInterpolated = $"""value {value}""";
                                  }
                              }
                              """";
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
        var root = syntaxTree.GetRoot(TestContext.CancellationToken);
        var sourceText = syntaxTree.GetText(TestContext.CancellationToken);
        var expectedLineIndices = GetLineIndicesForVariables(sourceText,
                                                             root,
                                                             "regular",
                                                             "utf8",
                                                             "regularInterpolated",
                                                             "verbatim",
                                                             "verbatimInterpolated",
                                                             "raw",
                                                             "rawInterpolated",
                                                             "singleLineRaw",
                                                             "singleLineRawInterpolated");
        var actualLineIndices = FormattingTextAnalysisUtilities.GetStringLineIndices(root, sourceText);

        CollectionAssert.AreEquivalent(expectedLineIndices.ToArray(), actualLineIndices.ToArray());
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the line indices occupied by the initializer spans of the specified variables
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="root">Syntax root</param>
    /// <param name="variableNames">Variable names</param>
    /// <returns>The occupied line indices</returns>
    private static HashSet<int> GetLineIndicesForVariables(SourceText sourceText, SyntaxNode root, params string[] variableNames)
    {
        var variableNamesSet = new HashSet<string>(variableNames);
        var lineIndices = new HashSet<int>();

        foreach (var variable in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            if (variableNamesSet.Contains(variable.Identifier.ValueText) == false)
            {
                continue;
            }

            Assert.IsNotNull(variable.Initializer);

            foreach (var lineIndex in GetIntersectingLineIndices(sourceText, variable.Initializer.Value.FullSpan))
            {
                lineIndices.Add(lineIndex);
            }
        }

        return lineIndices;
    }

    /// <summary>
    /// Gets the line indices touched by the specified span
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="span">Span to map</param>
    /// <returns>The touched line indices</returns>
    private static IEnumerable<int> GetIntersectingLineIndices(SourceText sourceText, TextSpan span)
    {
        var startLineIndex = sourceText.Lines.GetLineFromPosition(span.Start).LineNumber;
        var endPosition = span.Length == 0 ? span.End : span.End - 1;
        var endLineIndex = sourceText.Lines.GetLineFromPosition(endPosition).LineNumber;

        return Enumerable.Range(startLineIndex, endLineIndex - startLineIndex + 1);
    }

    #endregion // Methods
}