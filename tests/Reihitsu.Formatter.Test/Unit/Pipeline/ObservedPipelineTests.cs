using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests that <see cref="ObservedPipeline"/> observes a fixture under the parse options it is given: parsed with their
/// preprocessor symbols, and formatted with the language version they resolve to, as every formatter pass of the test
/// helpers is
/// </summary>
[TestClass]
public class ObservedPipelineTests
{
    #region Constants

    /// <summary>
    /// The name under which the pipeline observer reports the structural transform phase
    /// </summary>
    private const string StructuralTransformPhaseName = "StructuralTransformPhase";

    /// <summary>
    /// The name under which the pipeline observer reports the indentation phase
    /// </summary>
    private const string IndentationPhaseName = "IndentationPhase";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// The line endings every test runs under
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that an empty class body becomes a semicolon under the default language version, and keeps its braces under
    /// C# 11, which has no semicolon body for a class, so the observed phases run with the language version of the given
    /// parse options
    /// </summary>
    [TestMethod]
    public void RunFormatsWithTheLanguageVersionOfTheGivenParseOptions()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = $"class C{endOfLine}{{{endOfLine}}}";

            // Act
            var withDefault = ObservedPipeline.GetPhaseOutput(input, endOfLine, StructuralTransformPhaseName).ToFullString();
            var withCSharp11 = ObservedPipeline.GetPhaseOutput(input, endOfLine, StructuralTransformPhaseName, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11)).ToFullString();

            // Assert
            Assert.AreEqual("class C;", withDefault);
            Assert.AreEqual(input, withCSharp11);
        }
    }

    /// <summary>
    /// Verifies that code inside a conditional branch the given symbols activate is formatted by the observed phases, and
    /// is disabled text that no phase changes without them
    /// </summary>
    [TestMethod]
    public void RunParsesWithThePreprocessorSymbolsOfTheGivenParseOptions()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var input = $"#if FEATURE{endOfLine}class C{endOfLine}{{{endOfLine}        int _value;{endOfLine}}}{endOfLine}#endif";

            // Act
            var withSymbol = ObservedPipeline.GetPhaseOutput(input, endOfLine, IndentationPhaseName, CSharpParseOptions.Default.WithPreprocessorSymbols("FEATURE")).ToFullString();
            var withoutSymbol = ObservedPipeline.GetPhaseOutput(input, endOfLine, IndentationPhaseName).ToFullString();

            // Assert
            Assert.Contains($"{endOfLine}    int _value;", withSymbol);
            Assert.AreEqual(input, withoutSymbol);
        }
    }

    #endregion // Methods
}