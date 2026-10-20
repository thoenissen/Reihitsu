using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7207UsingDirectivesShouldBeOrganizedIntoGroupsFormatterTests : FormatterTestsBase<RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter sorts and groups using directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using {|#0:Bravo|};
                             using Alpha.Sub;
                             using static System.Math;
                             using System;
                             using Alpha;
                             using TextAlias = System.String;
                             
                             internal class Example;
                             
                             namespace Alpha
                             {
                                 internal class Placeholder;
                             }
                             
                             namespace Alpha.Sub
                             {
                                 internal class Placeholder;
                             }
                             
                             namespace Bravo
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 
                                 using Alpha;
                                 using Alpha.Sub;
                                 
                                 using Bravo;
                                 
                                 using static System.Math;
                                 
                                 using TextAlias = System.String;
                                 
                                 internal class Example;
                                 
                                 namespace Alpha
                                 {
                                     internal class Placeholder;
                                 }
                                 
                                 namespace Alpha.Sub
                                 {
                                     internal class Placeholder;
                                 }
                                 
                                 namespace Bravo
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter separates a global-qualified case variant from the exact System group without detaching its comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task GlobalQualifiedCaseVariantMatchesAnalyzerGroupingAndIsIdempotent()
    {
        const string input = """
                             using {|#0:global::SYSTEM.Text|}; // Keep with the case variant
                             using System.Text;

                             namespace SYSTEM.Text
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System.Text;

                                 using global::SYSTEM.Text; // Keep with the case variant

                                 namespace SYSTEM.Text
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter treat the directives of the compilation unit and of a namespace as separate blocks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-001")]
    public async Task FormatterOrdersEveryUsingBlockOnItsOwn()
    {
        const string input = """
                             using {|RH7207:System.Linq|};
                             using System;

                             namespace N
                             {
                                 using {|RH7207:Reihitsu.Core|};
                                 using System.Text;

                                 class C
                                 {
                                     int x;
                                 }
                             }

                             namespace Reihitsu.Core
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;

                                 namespace N
                                 {
                                     using System.Text;

                                     using Reihitsu.Core;

                                     class C
                                     {
                                         int x;
                                     }
                                 }

                                 namespace Reihitsu.Core
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter order regular, then static, then alias directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-003")]
    public async Task FormatterPlacesRegularThenStaticThenAliasDirectives()
    {
        const string input = """
                             using static {|RH7207:System.Math|};
                             using A = System.Text;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 using static System.Math;

                                 using A = System.Text;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter group directives by section, kind, and root namespace, and aliases by their target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public async Task FormatterGroupsBySectionKindAndRootNamespace()
    {
        const string input = """
                             using Zeta = {|RH7207:Microsoft.CodeAnalysis.SyntaxNode|};
                             using static System.Math;
                             using Reihitsu.Core;
                             using Alpha = System.Text.StringBuilder;
                             using Microsoft.CodeAnalysis;
                             using System.Linq;
                             using static Reihitsu.Core.CasingUtilities;
                             using System;

                             class C
                             {
                                 int x;
                             }

                             namespace Microsoft.CodeAnalysis
                             {
                                 internal class SyntaxNode;
                             }

                             namespace Reihitsu.Core
                             {
                                 internal class CasingUtilities;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;

                                 using Microsoft.CodeAnalysis;

                                 using Reihitsu.Core;

                                 using static System.Math;

                                 using static Reihitsu.Core.CasingUtilities;

                                 using Alpha = System.Text.StringBuilder;

                                 using Zeta = Microsoft.CodeAnalysis.SyntaxNode;

                                 class C
                                 {
                                     int x;
                                 }

                                 namespace Microsoft.CodeAnalysis
                                 {
                                     internal class SyntaxNode;
                                 }

                                 namespace Reihitsu.Core
                                 {
                                     internal class CasingUtilities;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter place only the exact System root first and treat a case variant as an ordinary root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-005")]
    public async Task FormatterPlacesOnlyTheExactSystemRootFirst()
    {
        const string input = """
                             using {|RH7207:System.Linq|};
                             using SYSTEM.Text;
                             using System;

                             class C
                             {
                                 int x;
                             }

                             namespace SYSTEM.Text
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;

                                 using SYSTEM.Text;

                                 class C
                                 {
                                     int x;
                                 }

                                 namespace SYSTEM.Text
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter order the other roots alphabetically and keep roots that differ only in case as separate groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-006")]
    public async Task FormatterOrdersOtherRootsAlphabetically()
    {
        const string input = """
                             using {|RH7207:Systematic|};
                             using system.Text;
                             using foo.Bar;
                             using Foo.Baz;
                             using System;

                             class C
                             {
                                 int x;
                             }

                             namespace Systematic
                             {
                                 internal class Placeholder;
                             }

                             namespace system.Text
                             {
                                 internal class Placeholder;
                             }

                             namespace foo.Bar
                             {
                                 internal class Placeholder;
                             }

                             namespace Foo.Baz
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 using Foo.Baz;

                                 using foo.Bar;

                                 using system.Text;

                                 using Systematic;

                                 class C
                                 {
                                     int x;
                                 }

                                 namespace Systematic
                                 {
                                     internal class Placeholder;
                                 }

                                 namespace system.Text
                                 {
                                     internal class Placeholder;
                                 }

                                 namespace foo.Bar
                                 {
                                     internal class Placeholder;
                                 }

                                 namespace Foo.Baz
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter sort aliases by alias name within the group of their target root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    public async Task FormatterSortsAliasesByAliasNameWithinTheirGroup()
    {
        const string input = """
                             using Zeta = {|RH7207:System.Text.StringBuilder|};
                             using Beta = Microsoft.CodeAnalysis.SyntaxNode;
                             using Alpha = Microsoft.CodeAnalysis.SyntaxToken;
                             using Gamma = System.Console;

                             class C
                             {
                                 int x;
                             }

                             namespace Microsoft.CodeAnalysis
                             {
                                 internal class SyntaxNode;
                             }

                             namespace Microsoft.CodeAnalysis
                             {
                                 internal class SyntaxToken;
                             }
                             """;
        const string fixedData = """
                                 using Gamma = System.Console;
                                 using Zeta = System.Text.StringBuilder;

                                 using Alpha = Microsoft.CodeAnalysis.SyntaxToken;
                                 using Beta = Microsoft.CodeAnalysis.SyntaxNode;

                                 class C
                                 {
                                     int x;
                                 }

                                 namespace Microsoft.CodeAnalysis
                                 {
                                     internal class SyntaxNode;
                                 }

                                 namespace Microsoft.CodeAnalysis
                                 {
                                     internal class SyntaxToken;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter keep directives whose keys differ only in case in their source order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    public async Task FormatterKeepsEqualKeysInTheirSourceOrder()
    {
        const string input = """
                             using {|RH7207:System.Xml|};
                             using System.collections;
                             using System.Collections;

                             class C
                             {
                                 int x;
                             }

                             namespace System.collections
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System.collections;
                                 using System.Collections;
                                 using System.Xml;

                                 class C
                                 {
                                     int x;
                                 }

                                 namespace System.collections
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer and the formatter expect a blank line between two groups and none within a group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-008")]
    public async Task FormatterPlacesABlankLineBetweenGroupsAndNoneWithinAGroup()
    {
        const string input = """
                             using {|RH7207:System|};

                             using System.Linq;
                             using Microsoft.CodeAnalysis;

                             class C
                             {
                                 int x;
                             }

                             namespace Microsoft.CodeAnalysis
                             {
                                 internal class Placeholder;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;

                                 using Microsoft.CodeAnalysis;

                                 class C
                                 {
                                     int x;
                                 }

                                 namespace Microsoft.CodeAnalysis
                                 {
                                     internal class Placeholder;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the analyzer does not report and the formatter does not reorder a block containing a conditional directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task FormatterLeavesABlockWithAConditionalDirectiveUnchanged()
    {
        const string input = """
                             using System.Linq;
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the analyzer does not report and the formatter does not reorder a block whose first directive is led by a nullable directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task FormatterLeavesABlockLedByANullableDirectiveUnchanged()
    {
        const string input = """
                             #nullable enable
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that a conditional directive after the last using directive does not keep the analyzer and the formatter from organizing the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task FormatterOrganizesABlockFollowedByAConditionalDirective()
    {
        const string input = """
                             using {|RH7207:System.Linq|};
                             using System;
                             #if DEBUG
                             class D
                             {
                                 int y;
                             }
                             #endif

                             class C
                             {
                                 int x;
                             }
                             """;
        const string fixedData = """
                                 using System;
                                 using System.Linq;
                                 #if DEBUG
                                 class D
                                 {
                                     int y;
                                 }
                                 #endif

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    #endregion // Tests
}