using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests for a comment separated from the arrow of an expression-bodied void member by a blank line: the comment
/// stands between the arrow and the expression, so the formatter refuses the input as unsupported instead of placing the
/// comment inside the block that would replace the expression body
/// </summary>
[TestClass]
public class ExpressionBodyConversionCommentPlacementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a comment behind a blank line behind the arrow of a void method, a constructor, or a local function refuses
    /// the input as unsupported: the input is returned unchanged, and the comment is reported
    /// </summary>
    /// <param name="input">The source text</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("public class C\n{\n    public void M() =>\n\n        // c\n        Call();\n\n    public void Call()\n    {\n    }\n}", DisplayName = "Void method")]
    [DataRow("public class C\n{\n    public C() =>\n\n        // c\n        Call();\n\n    public void Call()\n    {\n    }\n}", DisplayName = "Constructor")]
    [DataRow("public class C\n{\n    public void M()\n    {\n        void Local() =>\n\n            // c\n            Call();\n\n        Local();\n    }\n\n    public void Call()\n    {\n    }\n}", DisplayName = "Local function")]
    public void CommentBehindABlankLineBehindTheArrowIsUnsupported(string input)
    {
        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, "// c");
    }

    #endregion // Methods
}