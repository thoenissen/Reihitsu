using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer"/> and <see cref="RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests : AnalyzerTestsBase<RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer, RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider>
{
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

        await Verify(testCode, fixedCode, Diagnostics(RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH0608MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH0608MessageFormat));
    }
}