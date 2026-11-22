using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5201MethodChainsShouldBeAlignedFormatterTests : FormatterTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter wraps and aligns method chains consistently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System.Linq;
                             
                             internal class Example
                             {
                                 internal int[] Filter(int[] values)
                                 {
                                     return values.Where(obj => obj > 0){|#0:.|}Select(obj => obj)
                                                  .ToArray();
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Linq;
                                 
                                 internal class Example
                                 {
                                     internal int[] Filter(int[] values)
                                     {
                                         return values.Where(obj => obj > 0)
                                                      .Select(obj => obj)
                                                      .ToArray();
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's own output for a wrapped chain whose first invoked link is
    /// introduced by <c>?.</c> after a plain (non-invoked) property access satisfies RH5201 and stays
    /// stable on a second pass, closing the format-fix-format oscillation this shape used to trigger
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesConditionalAccessAfterPlainPropertyAccessViolation()
    {
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Example
                             {
                                 internal sealed class Choice
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Option
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Request
                                 {
                                     public IEnumerable<Choice> Choices { get; set; }
                                 }

                                 internal static List<Option> Convert(Request request)
                                 {
                                     var options = request.Choices?.Select(choice => new Option
                                                                                     {
                                                                                         Name = choice.Name
                                                                                     })
                                                          {|#0:.|}ToList();

                                     return options;
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal sealed class Example
                                 {
                                     internal sealed class Choice
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Option
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Request
                                     {
                                         public IEnumerable<Choice> Choices { get; set; }
                                     }

                                     internal static List<Option> Convert(Request request)
                                     {
                                         var options = request.Choices?.Select(choice => new Option
                                                                                         {
                                                                                             Name = choice.Name
                                                                                         })
                                                                      .ToList();

                                         return options;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's own output for a wrapped chain whose first invoked link is
    /// introduced by <c>!.</c> after a plain (non-invoked) property access satisfies RH5201 and stays
    /// stable on a second pass. <c>!.</c> selects a different anchor token than <c>?.</c>
    /// (<see cref="Reihitsu.Core.FluentChainUtilities.GetInvokedLinkOperator"/> returns the <c>!</c>,
    /// not the following <c>.</c>), so this is the arm most able to drift from the analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesNullForgivingAfterPlainPropertyAccessViolation()
    {
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Example
                             {
                                 internal sealed class Choice
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Option
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Request
                                 {
                                     public IEnumerable<Choice> Choices { get; set; }
                                 }

                                 internal static List<Option> Convert(Request request)
                                 {
                                     var options = request.Choices!.Select(choice => new Option
                                                                                     {
                                                                                         Name = choice.Name
                                                                                     })
                                                          {|#0:.|}ToList();

                                     return options;
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal sealed class Example
                                 {
                                     internal sealed class Choice
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Option
                                     {
                                         public string Name { get; set; }
                                     }

                                     internal sealed class Request
                                     {
                                         public IEnumerable<Choice> Choices { get; set; }
                                     }

                                     internal static List<Option> Convert(Request request)
                                     {
                                         var options = request.Choices!.Select(choice => new Option
                                                                                         {
                                                                                             Name = choice.Name
                                                                                         })
                                                                      .ToList();

                                         return options;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter's output for a chain whose own first dot is wrapped is
    /// RH5201-clean, so the formatter and the analyzer no longer disagree on this shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForWrappedFirstChainDotIsClean()
    {
        const string source = """
                              internal class Example
                              {
                                  internal string Prop { get; set; }

                                  internal static string Run(Example a)
                                  {
                                      return a.Prop?.ToString()
                                                   .Trim();
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter's output for a chain rooted in a single-line array initializer is
    /// RH5201-clean, so the code fix no longer moves a column the formatter puts back
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForChainRootedInSingleLineInitializerIsClean()
    {
        const string source = """
                              using System.Linq;

                              internal class Example
                              {
                                  internal static int[] Run()
                                  {
                                      return new[] { 1, 2, 3 }.Select(item => item)
                                                              .ToArray();
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter's output for a chain whose member name was split from its own dot
    /// is RH5201-clean once the name is rejoined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForRejoinedSplitMemberNameIsClean()
    {
        const string source = """
                              internal class Example
                              {
                                  internal Example Prop { get; set; }

                                  internal Example Call()
                                  {
                                      return this;
                                  }

                                  internal Example Then()
                                  {
                                      return this;
                                  }

                                  internal static Example Run(Example a)
                                  {
                                      return a?.Prop
                                              .Call()
                                              .Then();
                                  }
                              }
                              """;

        await VerifyFormatter(source);
    }

    /// <summary>
    /// Verifies that the formatter breaks the line in front of a link whose operator stands behind the closing
    /// <c>*/</c> of a comment that spans several lines, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-019")]
    [Requirement("SHARED-COMMENT-012")]
    public async Task VerifyFormatterBreaksLinkBehindMultiLineCommentTail()
    {
        const string input = """
                             internal class Example
                             {
                                 internal object Run(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()
                                              /* x
                                     */ .D();

                                     return r;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal object Run(dynamic a)
                                     {
                                         var r = a.B()
                                                  .C()

                                                  /* x
                                         */
                                                  .D();

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, 8, 12, 8, 13, MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter breaks the line in front of a prefix link whose operator stands behind the closing
    /// <c>*/</c> of a comment that spans several lines, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CHAIN-005")]
    [Requirement("SHARED-COMMENT-012")]
    public async Task VerifyFormatterBreaksPrefixLinkBehindMultiLineCommentTail()
    {
        const string input = """
                             internal class Example
                             {
                                 internal object Run(dynamic a)
                                 {
                                     var r = a.B

                                              /* x
                                     */ .B
                                              .C()
                                              .D();

                                     return r;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal object Run(dynamic a)
                                     {
                                         var r = a.B

                                                  /* x
                                         */
                                                  .B
                                                  .C()
                                                  .D();

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, 8, 12, 8, 13, MessageFormat));
    }

    /// <summary>
    /// Verifies that a link whose line starts with the end of a comment that spans several lines outside the anchor's
    /// column is reported although its operator stands in that column, and that the formatter breaks the line in front of
    /// the link, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5201-002")]
    public async Task VerifyFormatterBreaksLinkInAnchorColumnBehindMisalignedCommentTail()
    {
        const string input = """
                             internal class Example
                             {
                                 internal object Run(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()
                                              /* x
                                     */       .D();

                                     return r;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal object Run(dynamic a)
                                     {
                                         var r = a.B()
                                                  .C()

                                                  /* x
                                         */
                                                  .D();

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, 8, 18, 8, 19, MessageFormat));
    }

    /// <summary>
    /// Verifies that the first call behind a wrapped prefix link, the reported shape, is reported, and that the formatter
    /// starts it on its own line, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5201-006")]
    public async Task VerifyFormatterBreaksFirstCallBehindWrappedPrefixLink()
    {
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Variable
                             {
                                 internal string Identifier { get; set; }
                             }

                             internal sealed class Declaration
                             {
                                 internal List<Variable> Variables { get; set; }
                             }

                             internal sealed class Statement
                             {
                                 internal Declaration Declaration { get; set; }
                             }

                             internal static class Example
                             {
                                 internal static IEnumerable<string> Run(Statement declaration)
                                 {
                                     var names = declaration.Declaration
                                                            .Variables{|#0:.|}Select(v => v.Identifier);

                                     return names;
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 internal sealed class Variable
                                 {
                                     internal string Identifier { get; set; }
                                 }

                                 internal sealed class Declaration
                                 {
                                     internal List<Variable> Variables { get; set; }
                                 }

                                 internal sealed class Statement
                                 {
                                     internal Declaration Declaration { get; set; }
                                 }

                                 internal static class Example
                                 {
                                     internal static IEnumerable<string> Run(Statement declaration)
                                     {
                                         var names = declaration.Declaration
                                                                .Variables
                                                                .Select(v => v.Identifier);

                                         return names;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the first call behind a first link that a comment holds below the root is reported, and that the
    /// formatter starts it on its own line, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5201-006")]
    public async Task VerifyFormatterBreaksFirstCallBehindCommentHeldFirstLink()
    {
        const string input = """
                             internal class Example
                             {
                                 internal Example Next { get; set; }

                                 internal Example Self()
                                 {
                                     return this;
                                 }

                                 internal static Example Run(Example c)
                                 {
                                     var b = c // keep
                                             .Next{|#0:.|}Self();

                                     return b;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal Example Next { get; set; }

                                     internal Example Self()
                                     {
                                         return this;
                                     }

                                     internal static Example Run(Example c)
                                     {
                                         var b = c // keep
                                                 .Next
                                                 .Self();

                                         return b;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a link behind the closing <c>*/</c> of a comment that starts its own line is reported although the
    /// <c>*/</c> stands in the anchor column, and that the formatter breaks the line in front of the link, so its output is
    /// clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5201-002")]
    public async Task VerifyFormatterBreaksLinkBehindOwnLineCommentTailInAnchorColumn()
    {
        const string input = """
                             internal class Example
                             {
                                 internal object Run(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                              */ {|#0:.|}D();

                                     return r;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal object Run(dynamic a)
                                     {
                                         var r = a.B()
                                                  .C()

                                                  /* x
                                                  */
                                                  .D();

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a link behind a second comment behind the closing <c>*/</c> of a comment that starts its own line is
    /// reported although the <c>*/</c> stands in the anchor column, and that the formatter breaks the line in front of the
    /// link, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5201-002")]
    public async Task VerifyFormatterBreaksLinkBehindSecondCommentBehindOwnLineCommentTailInAnchorColumn()
    {
        const string input = """
                             internal class Example
                             {
                                 internal object Run(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* x
                                              */ /* y */ {|#0:.|}D();

                                     return r;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal object Run(dynamic a)
                                     {
                                         var r = a.B()
                                                  .C()

                                                  /* x
                                                  */ /* y */
                                                  .D();

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a link behind the closing <c>*/</c> of a comment that starts behind another comment on its own line is
    /// reported although the <c>*/</c> stands in the anchor column, and that the formatter breaks the line in front of the
    /// link, so its output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5201-002")]
    public async Task VerifyFormatterBreaksLinkBehindTailOfCommentStartingBehindOwnLineCommentInAnchorColumn()
    {
        const string input = """
                             internal class Example
                             {
                                 internal object Run(dynamic a)
                                 {
                                     var r = a.B()
                                              .C()

                                              /* w */ /* x
                                              */ {|#0:.|}D();

                                     return r;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal object Run(dynamic a)
                                     {
                                         var r = a.B()
                                                  .C()

                                                  /* w */ /* x
                                                  */
                                                  .D();

                                         return r;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, MessageFormat));
    }

    #endregion // Tests
}