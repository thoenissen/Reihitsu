using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer"/> and <see cref="RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests : AnalyzerTestsBase<RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer, RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying regular usings are reported and fixed when they are not alphabetically ordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task RegularUsingsAreReportedAndFixedWhenTheyAreNotAlphabeticallyOrdered()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that System namespace usings stay isolated from other groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SystemNamespaceUsingsDoNotProduceDiagnosticsWhenSorted()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Threading;
                                using System.Threading.Tasks;

                                using TestNamespace;
                                using TestNamespace.Subnamespace;

                                namespace TestNamespace
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                namespace TestNamespace.Subnamespace
                                {
                                    public class NestedHelper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a sorted System group followed by a sorted Microsoft group does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SystemAndMicrosoftGroupsDoNotProduceDiagnosticsWhenIndividuallySorted()
    {
        const string testCode = """
                                using System.Collections.Immutable;
                                using System.Threading;
                                using System.Threading.Tasks;

                                using Microsoft.CodeAnalysis;
                                using Microsoft.CodeAnalysis.CodeActions;
                                using Microsoft.CodeAnalysis.CodeFixes;
                                using Microsoft.CodeAnalysis.CSharp;

                                namespace Microsoft.CodeAnalysis
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeActions
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeFixes
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CSharp
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Reihitsu.Analyzer.Base
                                {
                                    public class TestClass
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that Microsoft usings are fixed without disturbing the sorted System group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MicrosoftUsingsAreFixedWithoutMovingSortedSystemGroup()
    {
        const string testCode = """
                                using System.Collections.Immutable;
                                using System.Threading;
                                using System.Threading.Tasks;

                                using Microsoft.CodeAnalysis.CSharp;
                                using {|#0:Microsoft.CodeAnalysis|};
                                using Microsoft.CodeAnalysis.CodeActions;
                                using Microsoft.CodeAnalysis.CodeFixes;

                                namespace Microsoft.CodeAnalysis
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeActions
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeFixes
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CSharp
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Reihitsu.Analyzer.Base
                                {
                                    public class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Immutable;
                                 using System.Threading;
                                 using System.Threading.Tasks;

                                 using Microsoft.CodeAnalysis;
                                 using Microsoft.CodeAnalysis.CodeActions;
                                 using Microsoft.CodeAnalysis.CodeFixes;
                                 using Microsoft.CodeAnalysis.CSharp;

                                 namespace Microsoft.CodeAnalysis
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Microsoft.CodeAnalysis.CodeActions
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Microsoft.CodeAnalysis.CodeFixes
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Microsoft.CodeAnalysis.CSharp
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Reihitsu.Analyzer.Base
                                 {
                                     public class TestClass
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that the shared using-ordering code fix does not format the following type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixDoesNotFormatFollowingType()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                    public int Count()
                                    {
                                        var values = new List<int>();
                                        return values.Count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                     public int Count()
                                     {
                                         var values = new List<int>();
                                         return values.Count;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that a file-header banner above the original first using directive stays at the top of
    /// the scope when the code fix demotes that directive, instead of moving into the middle of the
    /// using block (issue #432)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixKeepsBannerAtTopWhenFirstUsingIsDemoted()
    {
        const string testCode = """
                                // Copyright (c) Example Corp. All rights reserved.

                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 // Copyright (c) Example Corp. All rights reserved.

                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    #endregion // Tests
}