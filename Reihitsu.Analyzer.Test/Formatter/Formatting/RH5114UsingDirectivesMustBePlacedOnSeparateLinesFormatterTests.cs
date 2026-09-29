using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5114UsingDirectivesMustBePlacedOnSeparateLinesFormatterTests : FormatterTestsBase<RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter splits same-group directives separated by a space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSplitsSpaceSeparatedDirectives()
    {
        const string input = """
                             using System.Collections.Generic; {|#0:using System.Linq;|}

                             internal class TestClass
                             {
                                 private int _value;
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                     private int _value;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter keeps a block comment on the line of the previous directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsBlockCommentOnThePreviousLine()
    {
        const string input = """
                             using System; /* core */ {|#0:using System.Linq;|}

                             internal class TestClass
                             {
                                 private int _value;
                             }
                             """;
        const string fixedData = """
                                 using System; /* core */
                                 using System.Linq;

                                 internal class TestClass
                                 {
                                     private int _value;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter splits directives of different groups and separates the groups by a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSplitsCrossGroupDirectivesWithBlankLine()
    {
        const string input = """
                             using System; {|#0:using Microsoft.Win32;|}

                             internal class TestClass
                             {
                                 private int _value;
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 using Microsoft.Win32;

                                 internal class TestClass
                                 {
                                     private int _value;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter splits directives sharing a line inside a block namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSplitsDirectivesInsideNamespace()
    {
        const string input = """
                             namespace Example
                             {
                                 using System; {|#0:using System.Linq;|}

                                 internal class TestClass
                                 {
                                     private int _value;
                                 }
                             }
                             """;
        const string fixedData = """
                                 namespace Example
                                 {
                                     using System;
                                     using System.Linq;

                                     internal class TestClass
                                     {
                                         private int _value;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter splits directives sharing a line in a block that a preprocessor directive keeps from being reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSplitsDirectivesInBlockWithPreprocessorDirective()
    {
        const string input = """
                             using System.Linq; {|#0:using System;|}
                             #pragma warning disable CS8019
                             using System.IO;
                             #pragma warning restore CS8019

                             internal class TestClass
                             {
                                 private int _value;
                             }
                             """;
        const string fixedData = """
                                 using System.Linq;
                                 using System;
                                 #pragma warning disable CS8019
                                 using System.IO;
                                 #pragma warning restore CS8019

                                 internal class TestClass
                                 {
                                     private int _value;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5114UsingDirectivesMustBePlacedOnSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5114MessageFormat));
    }

    #endregion // Tests
}