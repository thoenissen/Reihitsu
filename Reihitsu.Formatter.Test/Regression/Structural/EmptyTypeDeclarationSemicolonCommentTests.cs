using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="Reihitsu.Formatter.Pipeline.StructuralTransforms.EmptyTypeDeclarationSemicolonTransform"/> —
/// a comment between the type header and the open brace must not be deleted when the empty body is removed (issue #414)
/// </summary>
[TestClass]
public class EmptyTypeDeclarationSemicolonCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a comment between a class header and its open brace blocks the semicolon conversion
    /// </summary>
    [TestMethod]
    public void CommentBeforeOpenBraceOfEmptyClassIsPreserved()
    {
        const string input = """
                             public class C
                             // why this type is empty
                             {
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between a struct header and its open brace blocks the semicolon conversion
    /// </summary>
    [TestMethod]
    public void CommentBeforeOpenBraceOfEmptyStructIsPreserved()
    {
        const string input = """
                             public struct S
                             // why this type is empty
                             {
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between an interface header and its open brace blocks the semicolon conversion
    /// </summary>
    [TestMethod]
    public void CommentBeforeOpenBraceOfEmptyInterfaceIsPreserved()
    {
        const string input = """
                             public interface I
                             // why this type is empty
                             {
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between a record header and its open brace blocks the semicolon conversion
    /// </summary>
    [TestMethod]
    public void CommentBeforeOpenBraceOfEmptyRecordIsPreserved()
    {
        const string input = """
                             public record R
                             // why this type is empty
                             {
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between a record struct header and its open brace blocks the semicolon conversion
    /// </summary>
    [TestMethod]
    public void CommentBeforeOpenBraceOfEmptyRecordStructIsPreserved()
    {
        const string input = """
                             public record struct RS
                             // why this type is empty
                             {
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Methods
}