using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for single-letter method parameters and local variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForSingleLetterMethodParameterAndLocalVariables()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(List<int> items, int {|#0:x|})
                                    {
                                        for (int {|#1:i|} = 0; i < items.Count; i++)
                                        {
                                            var {|#2:y|} = items[i] + x;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH4119MessageFormat, 3));
    }

    /// <summary>
    /// Verifies diagnostics are reported for a catch variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForCatchVariable()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        try
                                        {
                                            throw new Exception();
                                        }
                                        catch (Exception {|#0:e|})
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH4119MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for deconstruction variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForDeconstructionVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var ({|#0:x|}, {|#1:y|}) = GetCoordinates();
                                    }

                                    private static (int Left, int Top) GetCoordinates()
                                    {
                                        return (1, 2);
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH4119MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a single-letter simple lambda parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForSimpleLambdaParameter()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(List<int> items)
                                    {
                                        var active = items.Where({|#0:p|} => p > 0).ToList();
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH4119MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for single-letter parenthesized lambda parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForParenthesizedLambdaParameters()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int, int> add = ({|#0:p|}, {|#1:q|}) => p + q;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH4119MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a single-letter anonymous delegate parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForAnonymousDelegateParameter()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int> increment = delegate(int {|#0:x|}) { return x + 1; };
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH4119MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive lambda parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDescriptiveLambdaParameters()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(List<int> items)
                                    {
                                        var active = items.Where(item => item > 0).ToList();
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive local identifiers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDescriptiveIdentifiers()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(List<int> items, int offset)
                                    {
                                        for (int index = 0; index < items.Count; index++)
                                        {
                                            var item = items[index] + offset;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for discards
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDiscards()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int _)
                                    {
                                        (_, var item) = GetCoordinates();
                                    }

                                    private static (int Left, int Top) GetCoordinates()
                                    {
                                        return (1, 2);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for override parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOverrideParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public abstract class TestBase
                                {
                                    public abstract void Process(int item);
                                }

                                public class TestClass : TestBase
                                {
                                    public override void Process(int x)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for explicit interface implementation parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForExplicitInterfaceImplementationParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public interface ITestHandler
                                {
                                    void Process(int item);
                                }

                                public class TestClass : ITestHandler
                                {
                                    void ITestHandler.Process(int x)
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for record primary constructor parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRecordPrimaryConstructorParameters()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public record TestRecord(int x);
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}