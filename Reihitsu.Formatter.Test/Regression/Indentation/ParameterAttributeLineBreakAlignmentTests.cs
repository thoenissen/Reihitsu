using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// A parameter type that stays on its own line after an attribute list, because trivia forbids joining the two lines,
/// is aligned with the parameter list instead of the base indentation
/// </summary>
[TestClass]
public class ParameterAttributeLineBreakAlignmentTests : FormatterTestsBase
{
    #region Properties

    /// <summary>
    /// The test context
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a positional record parameter type stays aligned after an attribute list followed by a line comment
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete] // keep
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a targeted attribute list followed by a line comment keeps the parameter type aligned
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterTargetedAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete] // keep
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a second record parameter type stays aligned after an attribute list and a line comment
    /// </summary>
    [TestMethod]
    public void SecondRecordParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example(int Other,
                                                            [Obsolete] // keep
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an ordinary method parameter type stays aligned after an attribute list and a line comment
    /// </summary>
    [TestMethod]
    public void MethodParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete] // keep
                                                 int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter type stays aligned after an attribute list followed by a pragma directive
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndPragmaStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete]
                             #pragma warning disable CS0618
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter type stays aligned after an attribute list followed by a block comment on its own line
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndBlockCommentLineStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([param: Obsolete]
                                                            /* keep */ int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter type stays aligned after attribute lists separated by a conditional directive
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListsSeparatedByConditionalDirectiveStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete]
                             #if DEBUG
                                                            [CLSCompliant(false)]
                             #endif
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the reported record scenario with CRLF line endings only
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndLineCommentStaysAlignedUnderCrLf()
    {
        // Arrange
        const string input = "using System;\r\n\r\nnamespace Demo;\r\n\r\ninternal sealed record Example([Obsolete] // keep\r\n                               int Id);";

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var actual = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), new FormattingContext("\r\n"), TestContext.CancellationToken).ToFullString();

        // Assert
        Assert.AreEqual(input, actual);
    }

    #endregion // Methods
}