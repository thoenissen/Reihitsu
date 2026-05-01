using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/> and <see cref="RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests : AnalyzerTestsBase<RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer, RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that no diagnostic is reported when there is only a single using directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticForSingleUsing()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when all usings belong to the same root namespace and no blank line separates them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticForSingleGroupAlreadySorted()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when usings are already organized into groups with correct blank lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenAlreadyOrganized()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                using Alpha;
                                using Alpha.Sub;

                                using Bravo;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Bravo
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when regular, static, and alias usings are already grouped correctly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenUsingTypesAreAlreadyGrouped()
    {
        const string testCode = """
                                using System;

                                using Alpha;
                                using Alpha.Sub;

                                using static System.Math;

                                using static Bravo.Helper;

                                using TextAlias = System.String;

                                using BravoAlias = Bravo.Helper;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Bravo
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a diagnostic is reported on the first using when groups are missing blank line separators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DiagnosticWhenMissingBlankLineBetweenGroups()
    {
        const string testCode = """
                                using {|#0:System|};
                                using System.Collections.Generic;
                                using System.Linq;
                                using Alpha;
                                using Alpha.Sub;
                                using Bravo;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Bravo
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 using Alpha;
                                 using Alpha.Sub;

                                 using Bravo;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Alpha.Sub
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Bravo
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix also sorts usings alphabetically within each group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixSortsUsingWithinGroups()
    {
        const string testCode = """
                                using {|#0:System.Linq|};
                                using System;
                                using System.Collections.Generic;
                                using Alpha.Sub;
                                using Alpha;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 using Alpha;
                                 using Alpha.Sub;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Alpha.Sub
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that a diagnostic is reported when an extra blank line exists within a group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DiagnosticWhenExtraBlankLineWithinGroup()
    {
        const string testCode = """
                                using {|#0:System|};

                                using System.Collections.Generic;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that System group is placed before other groups in the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixPlacesSystemGroupFirst()
    {
        const string testCode = """
                                using {|#0:Alpha|};
                                using System;
                                using System.Linq;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using Alpha;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that static usings are separated from regular usings and grouped by root namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticUsingsAreSeparatedFromRegularUsings()
    {
        const string testCode = """
                                using {|#0:System|};
                                using static Alpha.Helper;
                                using Alpha;
                                using static System.Math;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Alpha;

                                 using static System.Math;

                                 using static Alpha.Helper;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that alias usings are grouped by root namespace of their target and kept after other using types
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AliasUsingsAreGroupedByRootNamespace()
    {
        const string testCode = """
                                using {|#0:System|};
                                using SysLinq = System.Linq;
                                using AlphaAlias = Alpha.Something;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Something
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using SysLinq = System.Linq;

                                 using AlphaAlias = Alpha.Something;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Something
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that alias usings are placed after static usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AliasUsingsAreSeparatedFromStaticUsings()
    {
        const string testCode = """
                                using {|#0:System|};
                                using Alias = Alpha.Helper;
                                using static System.Math;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using static System.Math;

                                 using Alias = Alpha.Helper;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that global using directives are organized by root namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task GlobalUsingDirectivesAreOrganized()
    {
        const string testCode = """
                                global using {|#0:System|};
                                global using System.Linq;
                                global using Alpha;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 global using System;
                                 global using System.Linq;

                                 global using Alpha;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when using directives are zero in a compilation unit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticForNoUsings()
    {
        const string testCode = """
                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that non-System groups are ordered alphabetically by root namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NonSystemGroupsAreOrderedAlphabetically()
    {
        const string testCode = """
                                using {|#0:Zebra.Something|};
                                using Alpha.Something;
                                using System;

                                public class TestClass
                                {
                                }

                                namespace Alpha.Something
                                {
                                }

                                namespace Zebra.Something
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Alpha.Something;

                                 using Zebra.Something;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha.Something
                                 {
                                 }

                                 namespace Zebra.Something
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH0390MessageFormat));
    }

    #endregion // Members
}