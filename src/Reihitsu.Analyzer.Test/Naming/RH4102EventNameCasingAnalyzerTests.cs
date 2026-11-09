using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
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
    /// Verifies that a field-like event override whose rename would cascade to an override in another source type of an event declared in a referenced assembly keeps its diagnostic but gets no fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToAFieldLikeEventOverrideOfAReferencedAssemblyEvent()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public abstract class MetaBase
                                      {
                                          public abstract event System.EventHandler changed;
                                      }
                                  }
                                  """;

        const string testCode = """
                                using System;

                                using Meta;

                                public class Middle : MetaBase
                                {
                                    public override event EventHandler {|RH4102:changed|};
                                }

                                public class Leaf : Middle
                                {
                                    public override event EventHandler {|RH4102:changed|};
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(reference));
    }

    /// <summary>
    /// Verifies that an event override with accessors whose rename would cascade to an override in another source type of an event declared in a referenced assembly keeps its diagnostic but gets no fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenTheRenameCascadesToACustomEventOverrideOfAReferencedAssemblyEvent()
    {
        const string metaSource = """
                                  namespace Meta
                                  {
                                      public abstract class MetaBase
                                      {
                                          public abstract event System.EventHandler opened;
                                      }
                                  }
                                  """;

        const string testCode = """
                                using System;

                                using Meta;

                                public class Middle : MetaBase
                                {
                                    public override event EventHandler {|RH4102:opened|}
                                    {
                                        add
                                        {
                                        }
                                        remove
                                        {
                                        }
                                    }
                                }

                                public class Leaf : Middle
                                {
                                    public override event EventHandler {|RH4102:opened|}
                                    {
                                        add
                                        {
                                        }
                                        remove
                                        {
                                        }
                                    }
                                }
                                """;

        var reference = await CompiledAssemblyReference.CreateAsync("Meta", metaSource);

        await VerifyCustomEventWithReference(testCode, testCode, reference);
    }

    /// <summary>
    /// Verifies that one Fix All renames a field-like event and an event with accessors of one document, whichever of the
    /// two it is started from and in every scope
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-002")]
    [Requirement("RH4102-003")]
    public async Task FixAllRenamesAFieldLikeEventAndAnEventWithAccessorsTogether()
    {
        const string testCode = """
                                using System;

                                public class C
                                {
                                    public event EventHandler dataChanged;
                                    public event EventHandler saved { add { } remove { } }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class C
                                 {
                                     public event EventHandler DataChanged;
                                     public event EventHandler Saved { add { } remove { } }
                                 }
                                 """;

        await VerifyFixAllFromEveryDiagnosticAsync(testCode, fixedCode, 2, fixAllEqualsSingleFixes: true);
    }

    /// <summary>
    /// Verifies that one Fix All renames both declarators of a field-like event declaration and an event with accessors,
    /// whichever of the three it is started from and in every scope
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-002")]
    [Requirement("RH4102-003")]
    public async Task FixAllRenamesSeveralFieldLikeEventDeclaratorsAndAnEventWithAccessorsTogether()
    {
        const string testCode = """
                                using System;

                                public class C
                                {
                                    public event EventHandler saved
                                    {
                                        add { }
                                        remove { }
                                    }

                                    public event EventHandler opened, closed;

                                    public void Raise()
                                    {
                                        opened?.Invoke(this, EventArgs.Empty);
                                        closed?.Invoke(this, EventArgs.Empty);
                                        saved += (sender, args) => { };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class C
                                 {
                                     public event EventHandler Saved
                                     {
                                         add { }
                                         remove { }
                                     }

                                     public event EventHandler Opened, Closed;

                                     public void Raise()
                                     {
                                         Opened?.Invoke(this, EventArgs.Empty);
                                         Closed?.Invoke(this, EventArgs.Empty);
                                         Saved += (sender, args) => { };
                                     }
                                 }
                                 """;

        await VerifyFixAllFromEveryDiagnosticAsync(testCode, fixedCode, 3, fixAllEqualsSingleFixes: true);
    }

    /// <summary>
    /// Verifies that one Fix All skips a field-like event and an event with accessors that would receive the same new
    /// name in one type, and renames the third event, whichever of the three it is started from
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-010")]
    [Requirement("RH4102-003")]
    public async Task FixAllSkipsAFieldLikeEventAndAnEventWithAccessorsThatWouldCollide()
    {
        const string testCode = """
                                using System;

                                public class C
                                {
                                    public event EventHandler dataChanged;

                                    public event EventHandler DATA_CHANGED
                                    {
                                        add { }
                                        remove { }
                                    }

                                    public event EventHandler saved;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class C
                                 {
                                     public event EventHandler dataChanged;

                                     public event EventHandler DATA_CHANGED
                                     {
                                         add { }
                                         remove { }
                                     }

                                     public event EventHandler Saved;
                                 }
                                 """;

        await VerifyFixAllFromEveryDiagnosticAsync(testCode, fixedCode, 3, fixAllEqualsSingleFixes: false);
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

    /// <summary>
    /// Verifies a custom-event code fix with the dedicated internal provider in a project that references the given assembly
    /// </summary>
    /// <param name="testCode">Test source with markup for every expected diagnostic</param>
    /// <param name="fixedCode">Expected fixed source</param>
    /// <param name="reference">Reference to an assembly the test project uses</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private static async Task VerifyCustomEventWithReference(string testCode, string fixedCode, MetadataReference reference)
    {
        var test = new CSharpCodeFixVerifierTest<RH4102EventNameCasingAnalyzer, RH4102CustomEventNameCasingCodeFixProvider>
                   {
                       TestCode = testCode,
                       FixedCode = fixedCode,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        test.TestState.AdditionalReferences.Add(reference);

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies Fix All on a document with several RH4102 diagnostics: for every diagnostic, every code fix provider of
    /// the analyzer assembly that offers a fix for it, and every Fix All scope, one Fix All application started from that
    /// diagnostic must produce the expected text, compile, and leave no diagnostic the expected text does not keep.
    /// Every provider is used, so the test holds however many providers fix RH4102
    /// </summary>
    /// <param name="source">Source without markup</param>
    /// <param name="expected">Expected text after Fix All</param>
    /// <param name="diagnosticCount">Number of diagnostics the analyzer reports on the source</param>
    /// <param name="fixAllEqualsSingleFixes">Whether applying the single fixes one after another must give the same text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyFixAllFromEveryDiagnosticAsync(string source, string expected, int diagnosticCount, bool fixAllEqualsSingleFixes)
    {
        var providers = GetEventNameCodeFixProviders();

        using (var workspace = new AdhocWorkspace())
        {
            var (solution, documentId) = CreateEventSolution(workspace, source);

            var diagnostics = await GetEventDiagnosticsAsync(solution.GetDocument(documentId)).ConfigureAwait(false);

            Assert.HasCount(diagnosticCount, diagnostics);

            var fixAllRuns = 0;

            foreach (var diagnostic in diagnostics)
            {
                var startText = await diagnostic.Location.SourceTree.GetTextAsync(CancellationToken.None).ConfigureAwait(false);
                var startName = startText.ToString(diagnostic.Location.SourceSpan);

                foreach (var provider in providers)
                {
                    var actions = await RegisterActionsAsync(provider, solution.GetDocument(documentId), diagnostic).ConfigureAwait(false);

                    if (actions.Count == 0)
                    {
                        continue;
                    }

                    foreach (var scope in new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution })
                    {
                        var context = new FixAllContext(solution.GetDocument(documentId),
                                                        provider,
                                                        scope,
                                                        actions[0].EquivalenceKey,
                                                        provider.FixableDiagnosticIds,
                                                        new EventFixAllDiagnosticProvider(solution, diagnostics),
                                                        CancellationToken.None);
                        var fixAllAction = await provider.GetFixAllProvider()
                                                         .GetFixAsync(context)
                                                         .ConfigureAwait(false);
                        var fixedSolution = solution;

                        if (fixAllAction != null)
                        {
                            var operation = (await fixAllAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false)).OfType<ApplyChangesOperation>()
                                                                                                                                 .Single();

                            fixedSolution = operation.ChangedSolution;
                        }

                        var fixedText = (await fixedSolution.GetDocument(documentId).GetTextAsync(CancellationToken.None).ConfigureAwait(false)).ToString();

                        Assert.AreEqual(expected,
                                        fixedText,
                                        $"Fix All of {provider.GetType().Name} in scope {scope}, started from '{startName}', produced another text.");

                        var compilation = await fixedSolution.GetDocument(documentId).Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);

                        Assert.IsEmpty(compilation.GetDiagnostics(CancellationToken.None).Where(static item => item.Severity == DiagnosticSeverity.Error),
                                       $"Fix All of {provider.GetType().Name} in scope {scope}, started from '{startName}', added a compile error.");

                        fixAllRuns++;
                    }
                }
            }

            Assert.IsGreaterThan(0, fixAllRuns, "No provider offered a fix for any diagnostic.");

            if (fixAllEqualsSingleFixes)
            {
                var singleFixesText = await ApplySingleFixesOneAfterAnotherAsync(solution, documentId, providers).ConfigureAwait(false);

                Assert.AreEqual(expected, singleFixesText, "Applying the single fixes one after another produced another text.");
            }
        }
    }

    /// <summary>
    /// Applies the first fix offered for the first diagnostic that has one, again and again, until no diagnostic is
    /// offered a fix
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="documentId">ID of the document</param>
    /// <param name="providers">Code fix providers</param>
    /// <returns>The text of the document after every single fix</returns>
    private static async Task<string> ApplySingleFixesOneAfterAnotherAsync(Solution solution, DocumentId documentId, IReadOnlyList<CodeFixProvider> providers)
    {
        for (var iteration = 0; iteration < 10; iteration++)
        {
            var document = solution.GetDocument(documentId);
            var diagnostics = await GetEventDiagnosticsAsync(document).ConfigureAwait(false);
            CodeAction action = null;

            foreach (var diagnostic in diagnostics)
            {
                foreach (var provider in providers)
                {
                    var actions = await RegisterActionsAsync(provider, document, diagnostic).ConfigureAwait(false);

                    if (actions.Count > 0)
                    {
                        action = actions[0];

                        break;
                    }
                }

                if (action != null)
                {
                    break;
                }
            }

            if (action == null)
            {
                break;
            }

            var operation = (await action.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false)).OfType<ApplyChangesOperation>()
                                                                                                           .Single();

            solution = operation.ChangedSolution;
        }

        return (await solution.GetDocument(documentId).GetTextAsync(CancellationToken.None).ConfigureAwait(false)).ToString();
    }

    /// <summary>
    /// Gets every code fix provider of the code fix assembly that fixes RH4102
    /// </summary>
    /// <returns>The providers</returns>
    private static List<CodeFixProvider> GetEventNameCodeFixProviders()
    {
        return typeof(RH4102EventNameCasingCodeFixProvider).Assembly
                                                           .GetTypes()
                                                           .Where(static type => typeof(CodeFixProvider).IsAssignableFrom(type) && type.IsAbstract == false)
                                                           .Select(static type => (CodeFixProvider)Activator.CreateInstance(type, nonPublic: true))
                                                           .Where(static provider => provider.FixableDiagnosticIds.Contains(RH4102EventNameCasingAnalyzer.DiagnosticId))
                                                           .OrderBy(static provider => provider.GetType().Name, StringComparer.Ordinal)
                                                           .ToList();
    }

    /// <summary>
    /// Creates a solution with one project that holds the source
    /// </summary>
    /// <param name="workspace">Workspace</param>
    /// <param name="source">Source</param>
    /// <returns>The solution and the ID of the document</returns>
    private static (Solution Solution, DocumentId DocumentId) CreateEventSolution(AdhocWorkspace workspace, string source)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var references = (trustedPlatformAssemblies?.Split(Path.PathSeparator) ?? []).Select(static path => MetadataReference.CreateFromFile(path));
        var solution = workspace.CurrentSolution
                                .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
                                .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                .AddMetadataReferences(projectId, references)
                                .AddDocument(documentId, "Test.cs", SourceText.From(source));

        return (solution, documentId);
    }

    /// <summary>
    /// Gets the RH4102 diagnostics of the document's project
    /// </summary>
    /// <param name="document">Document</param>
    /// <returns>The diagnostics in source order</returns>
    private static async Task<ImmutableArray<Diagnostic>> GetEventDiagnosticsAsync(Document document)
    {
        var compilation = await document.Project.GetCompilationAsync(CancellationToken.None).ConfigureAwait(false);
        var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RH4102EventNameCasingAnalyzer()))
                                           .GetAnalyzerDiagnosticsAsync(CancellationToken.None)
                                           .ConfigureAwait(false);

        return [.. diagnostics.OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start)];
    }

    /// <summary>
    /// Gets the code actions a provider registers for a diagnostic
    /// </summary>
    /// <param name="provider">Code fix provider</param>
    /// <param name="document">Document</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>The registered code actions</returns>
    private static async Task<List<CodeAction>> RegisterActionsAsync(CodeFixProvider provider, Document document, Diagnostic diagnostic)
    {
        var actions = new List<CodeAction>();

        if (provider.FixableDiagnosticIds.Contains(diagnostic.Id))
        {
            var context = new CodeFixContext(document,
                                             diagnostic,
                                             (action, _) => actions.Add(action),
                                             CancellationToken.None);

            await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);
        }

        return actions;
    }

    #endregion // Tests

    #region Types

#pragma warning disable RH2101 // The provider is a focused test adapter owned by this fixture
    /// <summary>
    /// Supplies the diagnostics of the Fix All tests that use every RH4102 provider
    /// </summary>
    private sealed class EventFixAllDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        #region Fields

        /// <summary>
        /// Diagnostics
        /// </summary>
        private readonly ImmutableArray<Diagnostic> _diagnostics;

        /// <summary>
        /// Solution containing the diagnostics
        /// </summary>
        private readonly Solution _solution;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="solution">Solution containing the diagnostics</param>
        /// <param name="diagnostics">Diagnostics</param>
        public EventFixAllDiagnosticProvider(Solution solution, ImmutableArray<Diagnostic> diagnostics)
        {
            _solution = solution;
            _diagnostics = diagnostics;
        }

        #endregion // Constructor

        #region DiagnosticProvider

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            var diagnostics = _diagnostics.Where(diagnostic => diagnostic.Location.SourceTree != null
                                                               && _solution.GetDocument(diagnostic.Location.SourceTree)?.Id == document.Id);

            return Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>([]);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            var diagnostics = _diagnostics.Where(diagnostic => diagnostic.Location.SourceTree != null
                                                               && _solution.GetDocument(diagnostic.Location.SourceTree)?.Project.Id == project.Id);

            return Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
        }

        #endregion // DiagnosticProvider
    }

#pragma warning restore RH2101

    #endregion // Types

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