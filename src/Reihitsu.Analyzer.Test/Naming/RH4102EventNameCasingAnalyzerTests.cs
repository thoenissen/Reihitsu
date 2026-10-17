using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4102EventNameCasingAnalyzer"/> and <see cref="RH4102EventNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4102EventNameCasingAnalyzerTests : BatchCodeFixTestsBase<RH4102EventNameCasingAnalyzer, RH4102EventNameCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-001")]
    [Requirement("RH4102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase event name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-001")]
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
    [Requirement("RH4102-001")]
    [Requirement("RH4102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an event declared in an interface with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-001")]
    [Requirement("RH4102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for a static event with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-001")]
    [Requirement("RH4102-003")]
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

        await Verify(testCode, fixedCode, Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat));
    }

    /// <summary>
    /// Verifies that property-style event declarations are checked and renamed at the event identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-001")]
    [Requirement("RH4102-003")]
    public async Task VerifyPropertyStyleEventIsDetectedAndFixed()
    {
        const string testCode = """
                                using System;

                                internal class EventSource
                                {
                                    public event EventHandler {|#0:dataChanged|}
                                    {
                                        add { }
                                        remove { }
                                    }

                                    public void Subscribe()
                                    {
                                        dataChanged += OnDataChanged;
                                        dataChanged -= OnDataChanged;
                                    }

                                    private void OnDataChanged(object sender, EventArgs args) { }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class EventSource
                                 {
                                     public event EventHandler DataChanged
                                     {
                                         add { }
                                         remove { }
                                     }

                                     public void Subscribe()
                                     {
                                         DataChanged += OnDataChanged;
                                         DataChanged -= OnDataChanged;
                                     }

                                     private void OnDataChanged(object sender, EventArgs args) { }
                                 }
                                 """;

        await VerifyCustomEvent(testCode, fixedCode, 1);
    }

    /// <summary>
    /// Verifies the event rename retargets subscriptions and references
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-003")]
    public async Task VerifyEventRenameRetargetsSubscriptionsAndReferences()
    {
        const string testCode = """
                                using System;

                                internal class EventSource
                                {
                                    public event EventHandler {|#0:dataChanged|};

                                    public void Subscribe()
                                    {
                                        dataChanged += OnDataChanged;
                                        dataChanged?.Invoke(this, EventArgs.Empty);
                                    }

                                    private void OnDataChanged(object sender, EventArgs args) { }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class EventSource
                                 {
                                     public event EventHandler DataChanged;

                                     public void Subscribe()
                                     {
                                         DataChanged += OnDataChanged;
                                         DataChanged?.Invoke(this, EventArgs.Empty);
                                     }

                                     private void OnDataChanged(object sender, EventArgs args) { }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat));
    }

    /// <summary>
    /// Verifies two field-like events are fixed in one Fix All iteration
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-003")]
    public async Task VerifyTwoEventsAreFixedInOneFixAllIteration()
    {
        const string testCode = """
                                using System;

                                internal class EventSource
                                {
                                    public event EventHandler {|#0:firstChanged|};

                                    public event EventHandler {|#1:secondChanged|};
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class EventSource
                                 {
                                     public event EventHandler FirstChanged;

                                     public event EventHandler SecondChanged;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat, 2));
    }

    /// <summary>
    /// Verifies two custom events are fixed by their dedicated provider in one Fix All iteration
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-003")]
    public async Task VerifyTwoCustomEventsAreFixedInOneFixAllIteration()
    {
        const string testCode = """
                                using System;

                                internal class EventSource
                                {
                                    public event EventHandler {|#0:firstChanged|}
                                    {
                                        add { }
                                        remove { }
                                    }

                                    public event EventHandler {|#1:secondChanged|}
                                    {
                                        add { }
                                        remove { }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class EventSource
                                 {
                                     public event EventHandler FirstChanged
                                     {
                                         add { }
                                         remove { }
                                     }

                                     public event EventHandler SecondChanged
                                     {
                                         add { }
                                         remove { }
                                     }
                                 }
                                 """;

        await VerifyCustomEvent(testCode, fixedCode, 2, fixAll: true);
    }

    /// <summary>
    /// Verifies explicit-interface event implementations are not diagnosed separately from their interface declaration
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-002")]
    [Requirement("RH4102-003")]
    public async Task VerifyExplicitInterfaceEventImplementationIsExcluded()
    {
        const string testCode = """
                                using System;

                                internal interface IEventSource
                                {
                                    event EventHandler {|#0:dataChanged|};
                                }

                                internal class EventSource : IEventSource
                                {
                                    event EventHandler IEventSource.dataChanged
                                    {
                                        add { }
                                        remove { }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal interface IEventSource
                                 {
                                     event EventHandler DataChanged;
                                 }

                                 internal class EventSource : IEventSource
                                 {
                                     event EventHandler IEventSource.DataChanged
                                     {
                                         add { }
                                         remove { }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat));
    }

    /// <summary>
    /// Verifies that interface events, every declarator of a field-like event, static events, events with accessors, and overrides and implicit implementations of source events are each reported on their identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-001")]
    public async Task EveryKindOfEventIsCheckedWithOneDiagnosticPerEvent()
    {
        const string testCode = """
                                using System;

                                public interface ISource
                                {
                                    event EventHandler {|RH4102:changed|};
                                }

                                public class Source
                                {
                                    public event EventHandler {|RH4102:opened|}, {|RH4102:closed|};

                                    public static event EventHandler {|RH4102:instanceCreated|};

                                    public event EventHandler DataReceived;

                                    public event EventHandler {|RH4102:saved|}
                                    {
                                        add { }
                                        remove { }
                                    }
                                }

                                public abstract class BaseSource
                                {
                                    public abstract event EventHandler {|RH4102:updated|};
                                }

                                public class Derived : BaseSource, ISource
                                {
                                    public override event EventHandler {|RH4102:updated|};

                                    public event EventHandler {|RH4102:changed|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an explicit interface event implementation is not reported, and that the fix for the interface event renames it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-002")]
    public async Task ExplicitInterfaceEventImplementationIsNotReportedAndIsRenamedWithTheInterfaceEvent()
    {
        const string testCode = """
                                using System;

                                internal interface ISource
                                {
                                    event EventHandler {|RH4102:dataChanged|};
                                }

                                internal class Source : ISource
                                {
                                    event EventHandler ISource.dataChanged
                                    {
                                        add { }
                                        remove { }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal interface ISource
                                 {
                                     event EventHandler DataChanged;
                                 }

                                 internal class Source : ISource
                                 {
                                     event EventHandler ISource.DataChanged
                                     {
                                         add { }
                                         remove { }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames a field-like event to PascalCase together with its subscription and its invocation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-003")]
    public async Task FixRenamesFieldLikeEventAndEverySubscriptionAndInvocation()
    {
        const string testCode = """
                                using System;

                                internal class Source
                                {
                                    public event EventHandler {|RH4102:dataChanged|};

                                    public void Subscribe()
                                    {
                                        dataChanged += OnChanged;
                                        dataChanged?.Invoke(this, EventArgs.Empty);
                                    }

                                    private void OnChanged(object sender, EventArgs args)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class Source
                                 {
                                     public event EventHandler DataChanged;

                                     public void Subscribe()
                                     {
                                         DataChanged += OnChanged;
                                         DataChanged?.Invoke(this, EventArgs.Empty);
                                     }

                                     private void OnChanged(object sender, EventArgs args)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames an event with accessors to PascalCase together with its subscription and unsubscription
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4102-003")]
    public async Task FixRenamesEventWithAccessorsAndEverySubscription()
    {
        const string testCode = """
                                using System;

                                internal class Source
                                {
                                    public event EventHandler {|#0:saved|}
                                    {
                                        add { }
                                        remove { }
                                    }

                                    public void Subscribe()
                                    {
                                        saved += OnChanged;
                                        saved -= OnChanged;
                                    }

                                    private void OnChanged(object sender, EventArgs args)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 internal class Source
                                 {
                                     public event EventHandler Saved
                                     {
                                         add { }
                                         remove { }
                                     }

                                     public void Subscribe()
                                     {
                                         Saved += OnChanged;
                                         Saved -= OnChanged;
                                     }

                                     private void OnChanged(object sender, EventArgs args)
                                     {
                                     }
                                 }
                                 """;

        await VerifyCustomEvent(testCode, fixedCode, 1);
    }

    /// <summary>
    /// Verifies a custom-event code fix with the dedicated internal provider
    /// </summary>
    /// <param name="testCode">Test source</param>
    /// <param name="fixedCode">Expected fixed source</param>
    /// <param name="diagnosticCount">Expected diagnostic count</param>
    /// <param name="fixAll">Whether the provider must fix all diagnostics in one iteration</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private static async Task VerifyCustomEvent(string testCode, string fixedCode, int diagnosticCount, bool fixAll = false)
    {
        var test = new CSharpCodeFixVerifierTest<RH4102EventNameCasingAnalyzer, RH4102CustomEventNameCasingCodeFixProvider>
                   {
                       TestCode = testCode,
                       FixedCode = fixedCode,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        test.ExpectedDiagnostics.AddRange(Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat, diagnosticCount));

        if (fixAll)
        {
            test.NumberOfFixAllIterations = 1;
        }

        await test.RunAsync();
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
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

        // Verifying diagnostics for multiple events declared in a single declaration with wrong casing
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4102EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4102MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}