using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Full-pipeline tests for the grouping and ordering of using directives by the value of their names: a verbatim
/// prefix, a Unicode escape, and whitespace or comments between a name's tokens do not decide the group or the order,
/// and an alias whose target is not a name forms the last group of its section
/// </summary>
[TestClass]
public class UsingDirectiveNameValueTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the specification example: a directive written with a verbatim prefix belongs to the <c>System</c>
    /// group and sorts after <c>System</c>, as the same name without the prefix does
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void VerbatimNameBelongsToTheSystemGroup()
    {
        // Arrange
        const string input = """
                             using Microsoft.Win32;
                             using @System.IO;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using @System.IO;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a directive whose name holds a Unicode escape belongs to the <c>System</c>
    /// group, sorts after <c>System</c>, and keeps its spelling
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void UnicodeEscapedNameBelongsToTheSystemGroupAndKeepsItsSpelling()
    {
        // Arrange
        const string input = """
                             using Microsoft.Win32;
                             using \u0053ystem.IO;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using \u0053ystem.IO;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a name with whitespace between its tokens is placed in the <c>System</c> group in the first pass,
    /// so a second pass changes nothing
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    [Requirement("FMT-PIPE-004")]
    public void NameWithWhitespaceBetweenItsTokensBelongsToTheSystemGroupInOnePass()
    {
        // Arrange
        const string input = """
                             using Microsoft.Win32;
                             using System . IO;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.IO;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a name with a comment between its tokens belongs to the <c>System</c> group and keeps the comment
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void NameWithCommentBetweenItsTokensBelongsToTheSystemGroup()
    {
        // Arrange
        const string input = """
                             using Microsoft.Win32;
                             using System/* x */.IO;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System/* x */.IO;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace around the <c>::</c> of a <c>global::</c> qualifier does not take the directive out of
    /// the <c>System</c> group
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void GlobalQualifierWithWhitespaceBelongsToTheSystemGroup()
    {
        // Arrange
        const string input = """
                             using Microsoft.Win32;
                             using global :: System.IO;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using global :: System.IO;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: <c>System.@Linq</c> sorts as <c>System.Linq</c>, after
    /// <c>System.Collections</c>, so the block is left in its order
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    public void VerbatimSegmentSortsByItsValue()
    {
        // Arrange
        const string input = """
                             using System.Collections;
                             using System.@Linq;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: whitespace inside a type-argument list is not part of the sort key, so
    /// <c>Comparer&lt; int &gt;</c> stays after <c>Comparer&lt;byte&gt;</c>
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    [Requirement("FMT-PIPE-004")]
    public void TypeArgumentWhitespaceDoesNotDecideTheOrderOfStaticDirectivesInOrder()
    {
        // Arrange
        const string input = """
                             using static System.Collections.Generic.Comparer<byte>;
                             using static System.Collections.Generic.Comparer< int >;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using static System.Collections.Generic.Comparer<byte>;
                                using static System.Collections.Generic.Comparer<int>;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a static directive whose type-argument list holds whitespace is sorted by its value in the first
    /// pass, so a second pass changes nothing
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    [Requirement("FMT-PIPE-004")]
    public void TypeArgumentWhitespaceDoesNotDecideTheOrderOfStaticDirectivesOutOfOrder()
    {
        // Arrange
        const string input = """
                             using static System.Collections.Generic.Comparer< int >;
                             using static System.Collections.Generic.Comparer<byte>;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using static System.Collections.Generic.Comparer<byte>;
                                using static System.Collections.Generic.Comparer<int>;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a static directive whose type name has a verbatim prefix is in one group with the other static
    /// directives of <c>System</c>, sorted by its value
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void VerbatimStaticTypeNameBelongsToTheSystemStaticGroup()
    {
        // Arrange
        const string input = """
                             using static @System.Math;
                             using static System.Console;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using static System.Console;
                                using static @System.Math;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an alias whose target has a verbatim prefix is in one group with the other aliases of <c>System</c>
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void VerbatimAliasTargetBelongsToTheSystemAliasGroup()
    {
        // Arrange
        const string input = """
                             using B = System.Text;

                             using A = @System.IO;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using A = @System.IO;
                                using B = System.Text;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an alias whose target holds whitespace between its tokens is in one group with the other aliases
    /// of <c>System</c> in the first pass, so a second pass changes nothing
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    [Requirement("FMT-PIPE-004")]
    public void AliasTargetWithWhitespaceBelongsToTheSystemAliasGroupInOnePass()
    {
        // Arrange
        const string input = """
                             using B = System.Text;

                             using A = System . IO;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using A = System.IO;
                                using B = System.Text;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the global section groups a verbatim name by its value too
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    [Requirement("SHARED-USING-002")]
    public void GlobalSectionGroupsVerbatimNameByItsValue()
    {
        // Arrange
        const string input = """
                             global using Microsoft.Win32;
                             global using @System.IO;
                             global using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                global using System;
                                global using @System.IO;

                                global using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: an alias of <c>int</c> forms its own group after the alias group of
    /// <c>System</c>
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void AliasOfAPredefinedTypeFormsTheLastGroup()
    {
        // Arrange
        const string input = """
                             using System;

                             using B = int;

                             using A = System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                using A = System.Text;

                                using B = int;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block that already ends with an alias of <c>int</c> as a group of its own is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void BlockEndingWithAnAliasOfAPredefinedTypeIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using A = System.Text;

                             using B = int;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an alias of a tuple type forms the last group of its section
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void AliasOfATupleTypeFormsTheLastGroup()
    {
        // Arrange
        const string input = """
                             using System;

                             using P = (int X, int Y);

                             using A = System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                using A = System.Text;

                                using P = (int X, int Y);

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an alias of an array type forms the last group of its section
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void AliasOfAnArrayTypeFormsTheLastGroup()
    {
        // Arrange
        const string input = """
                             using B = int[];

                             using A = System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using A = System.Text;

                                using B = int[];

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an alias of a nullable type, whose underlying type is a name, forms the last group of its section
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void AliasOfANullableTypeFormsTheLastGroup()
    {
        // Arrange
        const string input = """
                             using B = System.Int32?;

                             using A = System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using A = System.Text;

                                using B = System.Int32?;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an alias of a predefined type comes after the alias group of a root other than <c>System</c> too
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public void AliasOfAPredefinedTypeComesAfterTheAliasGroupOfAnotherRoot()
    {
        // Arrange
        const string input = """
                             using B = int;
                             using Z = Microsoft.Win32.Registry;
                             using A = System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using A = System.Text;

                                using Z = Microsoft.Win32.Registry;

                                using B = int;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an alias of a predefined type forms the last group of the global section
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    [Requirement("SHARED-USING-002")]
    public void AliasOfAPredefinedTypeFormsTheLastGroupOfTheGlobalSection()
    {
        // Arrange
        const string input = """
                             global using B = int;
                             global using A = System.Text;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                global using A = System.Text;

                                global using B = int;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}