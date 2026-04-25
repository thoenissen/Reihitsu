using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0207EventNameCasingAnalyzer"/> and <see cref="RH0207EventNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0207EventNameCasingAnalyzerTests : AnalyzerTestsBase<RH0207EventNameCasingAnalyzer, RH0207EventNameCasingCodeFixProvider>
{
    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Text;
                                using System.Threading.Tasks;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test class
                                    /// </summary>
                                    public class TestClass
                                    {
                                        /// <summary>
                                        /// Test event
                                        /// </summary>
                                        public event EventHandler {|#0:testEvent|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 using System.Text;
                                 using System.Threading.Tasks;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test class
                                     /// </summary>
                                     public class TestClass
                                     {
                                         /// <summary>
                                         /// Test event
                                         /// </summary>
                                         public event EventHandler TestEvent;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0207EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0207MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase event name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseEvent()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public event EventHandler DataReceived;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for an event with a generic handler type and wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForGenericEventHandlerWrongCasing()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public event EventHandler<EventArgs> {|#0:dataReceived|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public event EventHandler<EventArgs> DataReceived;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0207EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0207MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for multiple events declared in a single declaration with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleEventsInSameDeclaration()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public event EventHandler {|#0:firstEvent|}, {|#1:secondEvent|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public event EventHandler FirstEvent, SecondEvent;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0207EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0207MessageFormat, 2));
    }

    /// <summary>
    /// Verifying diagnostics for an event declared in an interface with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForEventInInterface()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public interface IEventSource
                                    {
                                        event EventHandler {|#0:dataChanged|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public interface IEventSource
                                     {
                                         event EventHandler DataChanged;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0207EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0207MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a static event with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForStaticEventWrongCasing()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        public static event EventHandler {|#0:instanceCreated|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         public static event EventHandler InstanceCreated;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0207EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0207MessageFormat));
    }
}