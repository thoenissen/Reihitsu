using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="Pipeline.StructuralTransforms.Rewriter.EmptyTypeDeclarationSemicolonTransform"/> —
/// every empty declaration kind must still convert to semicolon form once the transform's guard is consolidated
/// onto <c>EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely</c>. A declaration kind passed
/// as a literal instead of the visited node's own kind would silently stop one of these from converting, most
/// notably <c>record struct</c> against the plain <c>record</c> kind
/// </summary>
[TestClass]
public class EmptyTypeDeclarationSemicolonConversionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an empty class converts to semicolon form
    /// </summary>
    [TestMethod]
    public void EmptyClassConvertsToSemicolonForm()
    {
        const string input = """
                             public class C
                             {
                             }
                             """;

        const string expected = "public class C;";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty struct converts to semicolon form
    /// </summary>
    [TestMethod]
    public void EmptyStructConvertsToSemicolonForm()
    {
        const string input = """
                             public struct S
                             {
                             }
                             """;

        const string expected = "public struct S;";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty interface converts to semicolon form
    /// </summary>
    [TestMethod]
    public void EmptyInterfaceConvertsToSemicolonForm()
    {
        const string input = """
                             public interface I
                             {
                             }
                             """;

        const string expected = "public interface I;";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty record converts to semicolon form
    /// </summary>
    [TestMethod]
    public void EmptyRecordConvertsToSemicolonForm()
    {
        const string input = """
                             public record R
                             {
                             }
                             """;

        const string expected = "public record R;";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty record struct converts to semicolon form. This is the kind boundary the
    /// consolidation onto <c>EmptyTypeDeclarationSemicolonAnalysisUtilities.CanConvertSafely</c> must not
    /// regress: passing a fixed <c>SyntaxKind.RecordDeclaration</c> instead of the visited node's own kind would
    /// make <c>CanConvertSafely</c>'s <c>IsKind(declarationKind)</c> check refuse every record struct
    /// </summary>
    [TestMethod]
    public void EmptyRecordStructConvertsToSemicolonForm()
    {
        const string input = """
                             public record struct RS
                             {
                             }
                             """;

        const string expected = "public record struct RS;";

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}