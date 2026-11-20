using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH8201InheritdocShouldBeUsedCodeFixProvider"/>: overriding members, the documentation comment the fix replaces and the trivia around it, and partial members
/// </summary>
[TestClass]
public class RH8201InheritdocShouldBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH8201InheritdocShouldBeUsedAnalyzer, RH8201InheritdocShouldBeUsedCodeFixProvider>
{
    #region Test data

    /// <summary>
    /// Test source for overridden-method inheritdoc replacement
    /// </summary>
    private const string MethodTestData = """
                                          using System;

                                          namespace TestNamespace
                                          {
                                              internal abstract class TestBase
                                              {
                                                  /// <summary>
                                                  /// Base documentation
                                                  /// </summary>
                                                  public abstract void TestMethod();
                                              }

                                              internal class TestImplementation : TestBase
                                              {
                                                  ///{|#0: <summary>
                                                  /// Implementation documentation
                                                  /// </summary>
                                          |}        public override void TestMethod()
                                                  {
                                                  }
                                              }
                                          }
                                          """;

    /// <summary>
    /// Expected source for overridden-method inheritdoc replacement
    /// </summary>
    private const string MethodResultData = """
                                            using System;

                                            namespace TestNamespace
                                            {
                                                internal abstract class TestBase
                                                {
                                                    /// <summary>
                                                    /// Base documentation
                                                    /// </summary>
                                                    public abstract void TestMethod();
                                                }

                                                internal class TestImplementation : TestBase
                                                {
                                                    /// <inheritdoc/>
                                                    public override void TestMethod()
                                                    {
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>
    /// Test source for overridden-property inheritdoc replacement
    /// </summary>
    private const string PropertyTestData = """
                                            using System;

                                            namespace TestNamespace
                                            {
                                                internal abstract class TestBase
                                                {
                                                    /// <summary>
                                                    /// Base documentation
                                                    /// </summary>
                                                    public abstract int TestProperty { get; set; }
                                                }

                                                internal class TestImplementation : TestBase
                                                {
                                                    ///{|#0: <summary>
                                                    /// Implementation documentation
                                                    /// </summary>
                                            |}        public override int TestProperty
                                                    {
                                                        get
                                                        {
                                                            return 0;
                                                        }
                                                        set
                                                        {
                                                        }
                                                    }
                                                }
                                            }
                                            """;

    /// <summary>
    /// Expected source for overridden-property inheritdoc replacement
    /// </summary>
    private const string PropertyResultData = """
                                              using System;

                                              namespace TestNamespace
                                              {
                                                  internal abstract class TestBase
                                                  {
                                                      /// <summary>
                                                      /// Base documentation
                                                      /// </summary>
                                                      public abstract int TestProperty { get; set; }
                                                  }

                                                  internal class TestImplementation : TestBase
                                                  {
                                                      /// <inheritdoc/>
                                                      public override int TestProperty
                                                      {
                                                          get
                                                          {
                                                              return 0;
                                                          }
                                                          set
                                                          {
                                                          }
                                                      }
                                                  }
                                              }
                                              """;

    /// <summary>
    /// Test source for overridden-event inheritdoc replacement
    /// </summary>
    private const string EventTestData = """
                                         using System;

                                         namespace TestNamespace
                                         {
                                             internal abstract class TestBase
                                             {
                                                 /// <summary>
                                                 /// Base documentation
                                                 /// </summary>
                                                 public abstract event EventHandler TestEvent;
                                             }

                                             internal class TestImplementation : TestBase
                                             {
                                                 ///{|#0: <summary>
                                                 /// Implementation documentation
                                                 /// </summary>
                                         |}        public override event EventHandler TestEvent
                                                 {
                                                     add { }
                                                     remove { }
                                                 }
                                             }
                                         }
                                         """;

    /// <summary>
    /// Expected source for overridden-event inheritdoc replacement
    /// </summary>
    private const string EventResultData = """
                                           using System;

                                           namespace TestNamespace
                                           {
                                               internal abstract class TestBase
                                               {
                                                   /// <summary>
                                                   /// Base documentation
                                                   /// </summary>
                                                   public abstract event EventHandler TestEvent;
                                               }

                                               internal class TestImplementation : TestBase
                                               {
                                                   /// <inheritdoc/>
                                                   public override event EventHandler TestEvent
                                                   {
                                                       add { }
                                                       remove { }
                                                   }
                                               }
                                           }
                                           """;

    /// <summary>
    /// Test source for overridden-indexer inheritdoc replacement
    /// </summary>
    private const string IndexerTestData = """
                                           using System;

                                           namespace TestNamespace
                                           {
                                               internal abstract class TestBase
                                               {
                                                   /// <summary>
                                                   /// Base documentation
                                                   /// </summary>
                                                   public abstract int this[int i] { get; set; }
                                               }

                                               internal class TestImplementation : TestBase
                                               {
                                                   ///{|#0: <summary>
                                                   /// Implementation documentation
                                                   /// </summary>
                                           |}        public override int this[int i]
                                                   {
                                                       get
                                                       {
                                                           return 0;
                                                       }
                                                       set
                                                       {
                                                       }
                                                   }
                                               }
                                           }
                                           """;

    /// <summary>
    /// Expected source for overridden-indexer inheritdoc replacement
    /// </summary>
    private const string IndexerResultData = """
                                             using System;

                                             namespace TestNamespace
                                             {
                                                 internal abstract class TestBase
                                                 {
                                                     /// <summary>
                                                     /// Base documentation
                                                     /// </summary>
                                                     public abstract int this[int i] { get; set; }
                                                 }

                                                 internal class TestImplementation : TestBase
                                                 {
                                                     /// <inheritdoc/>
                                                     public override int this[int i]
                                                     {
                                                         get
                                                         {
                                                             return 0;
                                                         }
                                                         set
                                                         {
                                                         }
                                                     }
                                                 }
                                             }
                                             """;

    /// <summary>
    /// Specification example source with an indentation other than four spaces per level, indented directives, and a
    /// member behind the closing delimiter of a delimited documentation comment
    /// </summary>
    private const string IndentedSpecificationExampleSource = """
                                                              /// <summary>
                                                              /// Base
                                                              /// </summary>
                                                              public abstract class B
                                                              {
                                                                  /// <summary>
                                                                  /// Runs
                                                                  /// </summary>
                                                                  public abstract void Run();

                                                                  /// <summary>
                                                                  /// Stops
                                                                  /// </summary>
                                                                  public abstract void Stop();
                                                              }

                                                              /// <summary>
                                                              /// Derived
                                                              /// </summary>
                                                              public class C : B
                                                              {
                                                                    #if DEBUG
                                                                    #endif
                                                                    ///{|#0: <summary>
                                                                    /// Runs here
                                                                    /// </summary>
                                                              |}      #pragma warning disable CS0168
                                                                    public override void Run()
                                                                    {
                                                                    }

                                                                /**{|#1: <summary>Stops here</summary> */|} public override void Stop()
                                                                {
                                                                }
                                                              }
                                                              """;

    /// <summary>
    /// Expected result of the specification example with an indentation other than four spaces per level
    /// </summary>
    private const string IndentedSpecificationExampleFixedSource = """
                                                                   /// <summary>
                                                                   /// Base
                                                                   /// </summary>
                                                                   public abstract class B
                                                                   {
                                                                       /// <summary>
                                                                       /// Runs
                                                                       /// </summary>
                                                                       public abstract void Run();

                                                                       /// <summary>
                                                                       /// Stops
                                                                       /// </summary>
                                                                       public abstract void Stop();
                                                                   }

                                                                   /// <summary>
                                                                   /// Derived
                                                                   /// </summary>
                                                                   public class C : B
                                                                   {
                                                                         #if DEBUG
                                                                         #endif
                                                                         /// <inheritdoc/>
                                                                         #pragma warning disable CS0168
                                                                         public override void Run()
                                                                         {
                                                                         }

                                                                     /// <inheritdoc/>
                                                                     public override void Stop()
                                                                     {
                                                                     }
                                                                   }
                                                                   """;

    #endregion // Test data

    #region Methods

    /// <summary>
    /// Verifying diagnostic for overridden method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForMethod()
    {
        await Verify(MethodTestData, MethodResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForProperty()
    {
        await Verify(PropertyTestData, PropertyResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForEvent()
    {
        await Verify(EventTestData, EventResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that documented field-like override events are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    public async Task VerifyDiagnosticForFieldLikeOverrideEvent()
    {
        const string testData = """
                                using System;

                                internal abstract class TestBase
                                {
                                    /// <summary>Base documentation</summary>
                                    public virtual event EventHandler TestEvent;
                                }

                                internal class TestImplementation : TestBase
                                {
                                    ///{|#0: <summary>
                                    /// Implementation documentation
                                    /// </summary>
                                |}    public override event EventHandler TestEvent;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal abstract class TestBase
                                  {
                                      /// <summary>Base documentation</summary>
                                      public virtual event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : TestBase
                                  {
                                      /// <inheritdoc/>
                                      public override event EventHandler TestEvent;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that field-like override events already documented with &lt;inheritdoc/&gt; do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyNoDiagnosticForFieldLikeOverrideEventWithInheritdoc()
    {
        const string testData = """
                                using System;

                                internal abstract class TestBase
                                {
                                    public virtual event EventHandler TestEvent;
                                }

                                internal class TestImplementation : TestBase
                                {
                                    /// <inheritdoc/>
                                    public override event EventHandler TestEvent;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that undocumented field-like override events do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyNoDiagnosticForUndocumentedFieldLikeOverrideEvent()
    {
        const string testData = """
                                using System;

                                internal abstract class TestBase
                                {
                                    public virtual event EventHandler TestEvent;
                                }

                                internal class TestImplementation : TestBase
                                {
                                    public override event EventHandler TestEvent;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that documented field-like events without the override modifier do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-002")]
    public async Task VerifyNoDiagnosticForDocumentedNonOverrideFieldLikeEvent()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    /// <summary>Event documentation</summary>
                                    public event EventHandler TestEvent;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-line documentation comment on a field-like override event is replaced with
    /// &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyMultiLineDocumentationForFieldLikeOverrideEventIsReplaced()
    {
        const string testData = """
                                using System;

                                internal abstract class TestBase
                                {
                                    public virtual event EventHandler TestEvent;
                                }

                                internal class TestImplementation : TestBase
                                {
                                    /**{|#0: <summary>Implementation documentation</summary> */|}
                                    public override event EventHandler TestEvent;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal abstract class TestBase
                                  {
                                      public virtual event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : TestBase
                                  {
                                      /// <inheritdoc/>
                                      public override event EventHandler TestEvent;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All replaces documentation on multiple field-like override events in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration()
    {
        const string testData = """
                                using System;

                                internal abstract class TestBase
                                {
                                    public virtual event EventHandler First;
                                    public virtual event EventHandler Second;
                                }

                                internal class TestImplementation : TestBase
                                {
                                    ///{|#0: <summary>First implementation</summary>
                                |}    public override event EventHandler First;

                                    ///{|#1: <summary>Second implementation</summary>
                                |}    public override event EventHandler Second;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal abstract class TestBase
                                  {
                                      public virtual event EventHandler First;
                                      public virtual event EventHandler Second;
                                  }

                                  internal class TestImplementation : TestBase
                                  {
                                      /// <inheritdoc/>
                                      public override event EventHandler First;

                                      /// <inheritdoc/>
                                      public override event EventHandler Second;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifying diagnostic for overridden indexer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForIndexer()
    {
        await Verify(IndexerTestData, IndexerResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that only the flagged documentation comment is replaced when a member carries a second documentation comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyFirstDocumentationCommentIsReplaced()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        ///{|#0: <summary>
                                        /// Implementation documentation
                                        /// </summary>
                                |}
                                        /// <summary>
                                        /// Second documentation
                                        /// </summary>
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>

                                          /// <summary>
                                          /// Second documentation
                                          /// </summary>
                                          public override void TestMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment spanning several lines is replaced with
    /// &lt;inheritdoc/&gt; instead of the code fix registering a no-op action
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForMultiLineDocumentationComment()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0:
                                         * Implementation documentation
                                         */|}
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          public override void TestMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment written on a single line is replaced with
    /// &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForSingleLineFormMultiLineDocumentationComment()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          public override void TestMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment on an overridden property is replaced with
    /// &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifyDiagnosticForMultiLineDocumentationCommentOnProperty()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract int TestProperty { get; set; }
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                        public override int TestProperty { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract int TestProperty { get; set; }
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          public override int TestProperty { get; set; }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a leading comment placed before a multi-line (/** */) documentation comment is preserved
    /// when the documentation comment is replaced with &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySurroundingTriviaIsPreservedForMultiLineDocumentationComment()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        // Leading comment
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          // Leading comment
                                          /// <inheritdoc/>
                                          public override void TestMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that only the flagged multi-line (/** */) documentation comment is replaced when a member
    /// carries a second documentation comment.
    /// The surviving second comment is intentional and matches the single-line behavior asserted by
    /// <see cref="VerifyOnlyFirstDocumentationCommentIsReplaced"/>: the analyzer flags only the first
    /// documentation comment, so the fix replaces only that one. The compiler concatenates both comments into
    /// a single member entry in the generated XML documentation file (verified: the member emits
    /// &lt;inheritdoc/&gt; followed by the surviving &lt;summary&gt;) and reports no warning for the pair, so
    /// the fixed code neither drops documentation nor introduces a compiler diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOnlyFirstMultiLineDocumentationCommentIsReplaced()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                        /** <summary>Second documentation</summary> */
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          /** <summary>Second documentation</summary> */
                                          public override void TestMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that the synthesized &lt;inheritdoc/&gt; trivia replacing a multi-line (/** */) documentation
    /// comment uses the document's end-of-line sequence, whatever the environment's is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyMultiLineDocumentationCommentReplacementUsesDocumentEndOfLine()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /** <summary>Implementation documentation</summary> */
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.Contains("/// <inheritdoc/>\n        public override void TestMethod()", fixedSource);
    }

    /// <summary>
    /// Verifies that a trailing comment sharing the line with a multi-line (/** */) documentation comment
    /// survives the replacement and keeps its own line. The fix drops the line break that terminated the
    /// replaced comment, so this guards that deletion against consuming the trailing comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyTrailingCommentIsPreservedWhenMultiLineDocumentationCommentIsReplaced()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|} // trailing
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          // trailing
                                          public override void TestMethod()
                                          {
                                          }
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a preprocessor directive between a multi-line (/** */) documentation comment and the
    /// member keeps its own line when the comment is replaced. The fix drops the line break that terminated
    /// the replaced comment, so this guards that deletion against joining the directive onto the
    /// &lt;inheritdoc/&gt; line, which would not compile
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyDirectiveIsPreservedWhenMultiLineDocumentationCommentIsReplaced()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                #if true
                                        public override void TestMethod()
                                        {
                                        }
                                #endif
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                  #if true
                                          public override void TestMethod()
                                          {
                                          }
                                  #endif
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 1));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal abstract class BaseType
                              {
                                  /// <summary>Base docs.</summary>
                                  public abstract void Execute();
                              }
                              
                              internal class DerivedType : BaseType
                              {
                                  /// <summary>Implementation docs.</summary>
                                  public override void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that the synthesized &lt;inheritdoc/&gt; trivia uses the environment's end-of-line sequence
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySynthesizedInheritdocTriviaUsesEnvironmentEndOfLine()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /// <summary>
                                        /// Implementation documentation
                                        /// </summary>
                                        public override void TestMethod()
                                        {
                                        }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.Contains($"/// <inheritdoc/>{System.Environment.NewLine}", fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: the documentation comment of an override, an implicit implementation, and a field-like event whose events all implement interface events is reported, while a nested inheritdoc satisfies the rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    public async Task VerifySpecificationExampleReportsDocumentedMembersThatInheritTheirDocumentation()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void Run();

                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  public abstract int Value { get; }
                              }

                              /// <summary>
                              /// Notifier
                              /// </summary>
                              public interface INotifier
                              {
                                  /// <summary>
                                  /// Raised first
                                  /// </summary>
                                  event EventHandler First;

                                  /// <summary>
                                  /// Raised second
                                  /// </summary>
                                  event EventHandler Second;

                                  /// <summary>
                                  /// Raised third
                                  /// </summary>
                                  event EventHandler Third;
                              }

                              /// <summary>
                              /// Derived
                              /// </summary>
                              public class C : B, IDisposable, INotifier
                              {
                                  ///{|#0: <summary>Raised</summary>
                              |}    public event EventHandler First, Second;

                                  /// <summary>
                                  /// Raised as well
                                  /// </summary>
                                  public event EventHandler Third, Fourth;

                                  ///{|#1: <summary>Runs here</summary>
                              |}    public override void Run()
                                  {
                                  }

                                  /// <inheritdoc/>
                                  /// <remarks>
                                  /// <inheritdoc/>
                                  /// </remarks>
                                  public override int Value => 0;

                                  ///{|#2: <summary>Disposes</summary>
                              |}    public void Dispose()
                                  {
                                  }

                                  public override string ToString() => "";
                              }
                              """;

        await Verify(source, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the specification example: an overload, a member beside an explicit implementation, a base member only a derived type maps, an interface member hiding a base interface member, and an explicit implementation with inheritdoc are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-002")]
    public async Task VerifySpecificationExampleDoesNotReportMembersThatOnlyResembleInheritedOnes()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void Run();
                              }

                              /// <summary>
                              /// Derived contract
                              /// </summary>
                              public interface J : I
                              {
                                  /// <summary>
                                  /// Runs differently
                                  /// </summary>
                                  new void Run();
                              }

                              /// <summary>
                              /// Base
                              /// </summary>
                              public class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public void Run()
                                  {
                                  }
                              }

                              /// <summary>
                              /// Derived
                              /// </summary>
                              public class D : B, I
                              {
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : I
                              {
                                  /// <inheritdoc/>
                                  void I.Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Runs as well
                                  /// </summary>
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Runs with a value
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  public void Run(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: with an #if in the file of the type, the override and the implicit
    /// implementation are both reported; the fix replaces the documentation of the override and is withheld for the
    /// implicit implementation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("RH8201-004")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifySpecificationExampleConditionalInterfaceListWithholdsTheFixOnlyForImplicitImplementations()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void Run();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : B, IDisposable
                              #if FEATURE
                                  , ICloneable
                              #endif
                              {
                                  ///{|#0: <summary>Runs here</summary>
                              |}    public override void Run()
                                  {
                                  }

                                  ///{|RH8201: <summary>
                                  /// Disposes
                                  /// </summary>
                              |}    public void Dispose()
                                  {
                                  }

                              #if FEATURE
                                  /// <inheritdoc/>
                                  public object Clone() => this;
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   /// <summary>
                                   /// Base
                                   /// </summary>
                                   public abstract class B
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>
                                   /// Service
                                   /// </summary>
                                   public class C : B, IDisposable
                                   #if FEATURE
                                       , ICloneable
                                   #endif
                                   {
                                       /// <inheritdoc/>
                                       public override void Run()
                                       {
                                       }

                                       ///{|RH8201: <summary>
                                       /// Disposes
                                       /// </summary>
                                   |}    public void Dispose()
                                       {
                                       }

                                   #if FEATURE
                                       /// <inheritdoc/>
                                       public object Clone() => this;
                                   #endif
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     },
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a region directive in the file does not keep an implicit implementation from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-003")]
    [Requirement("RH8201-004")]
    public async Task VerifySpecificationExampleRegionDirectiveDoesNotSuppressImplicitImplementation()
    {
        const string source = """
                              using System;

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : IDisposable
                              {
                                  #region Methods

                                  ///{|#0: <summary>Disposes</summary>
                              |}    public void Dispose()
                                  {
                                  }

                                  #endregion
                              }
                              """;

        await Verify(source, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: the fix replaces a single-line and a delimited documentation comment with inheritdoc, keeping the ordinary comment before it, the attribute list after it, and the comment that followed the closing delimiter on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-004")]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleReplacesCommentWithInheritdoc()
    {
        const string source = """
                              /// <summary>
                              /// Base
                              /// </summary>
                              public abstract class B
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public abstract void Run();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  public abstract void Stop();
                              }

                              /// <summary>
                              /// Derived
                              /// </summary>
                              public class C : B
                              {
                                  // Ordinary comment
                                  ///{|#0: <summary>
                                  /// Runs here
                                  /// </summary>
                                  /// <remarks>
                                  /// Differs
                                  /// </remarks>
                              |}    [System.Obsolete]
                                  public override void Run()
                                  {
                                  }

                                  /**{|#1: <summary>Stops here</summary> */|} // Trailing
                                  public override void Stop()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>
                                   /// Base
                                   /// </summary>
                                   public abstract class B
                                   {
                                       /// <summary>
                                       /// Runs
                                       /// </summary>
                                       public abstract void Run();

                                       /// <summary>
                                       /// Stops
                                       /// </summary>
                                       public abstract void Stop();
                                   }

                                   /// <summary>
                                   /// Derived
                                   /// </summary>
                                   public class C : B
                                   {
                                       // Ordinary comment
                                       /// <inheritdoc/>
                                       [System.Obsolete]
                                       public override void Run()
                                       {
                                       }

                                       /// <inheritdoc/>
                                       // Trailing
                                       public override void Stop()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the fix keeps preprocessor directives at column 0 before and after the replaced comment in the leading trivia of the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsColumnZeroDirectivesAroundComment()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public abstract class B
                              {
                                  /// <summary>Runs</summary>
                                  public abstract void Run();
                              }

                              /// <summary>Derived</summary>
                              public class C : B
                              {
                              #pragma warning disable CS0618
                                  ///{|#0: <summary>Runs here</summary>
                              |}#pragma warning restore CS0618
                                  public override void Run()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   /// <summary>Base</summary>
                                   public abstract class B
                                   {
                                       /// <summary>Runs</summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>Derived</summary>
                                   public class C : B
                                   {
                                   #pragma warning disable CS0618
                                       /// <inheritdoc/>
                                   #pragma warning restore CS0618
                                       public override void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example with an indentation other than four spaces per level: the fix keeps the
    /// indented directives, the member lines, and the braces in their columns, and puts a member that followed the
    /// closing delimiter on the line after the inheritdoc line, in the column of the opening delimiter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedComment()
    {
        await Verify(IndentedSpecificationExampleSource, IndentedSpecificationExampleFixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example with an indentation other than four spaces per level in a document whose
    /// line breaks are CRLF: every line break of the result is CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleKeepsIndentationDirectivesAndMemberBehindDelimitedCommentWithCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(IndentedSpecificationExampleSource),
                     NormalizeToCarriageReturnLineFeed(IndentedSpecificationExampleFixedSource),
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the first specification example of the fix in a document whose line breaks are CRLF: the ordinary
    /// comment, the attribute list, and the comment that followed the closing delimiter keep their lines, and every
    /// line break of the result is CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleReplacesCommentWithInheritdocWithCarriageReturnLineFeed()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>
                                                       /// Base
                                                       /// </summary>
                                                       public abstract class B
                                                       {
                                                           /// <summary>
                                                           /// Runs
                                                           /// </summary>
                                                           public abstract void Run();

                                                           /// <summary>
                                                           /// Stops
                                                           /// </summary>
                                                           public abstract void Stop();
                                                       }

                                                       /// <summary>
                                                       /// Derived
                                                       /// </summary>
                                                       public class C : B
                                                       {
                                                           // Ordinary comment
                                                           ///{|#0: <summary>
                                                           /// Runs here
                                                           /// </summary>
                                                           /// <remarks>
                                                           /// Differs
                                                           /// </remarks>
                                                       |}    [System.Obsolete]
                                                           public override void Run()
                                                           {
                                                           }

                                                           /**{|#1: <summary>Stops here</summary> */|} // Trailing
                                                           public override void Stop()
                                                           {
                                                           }
                                                       }
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>
                                                            /// Base
                                                            /// </summary>
                                                            public abstract class B
                                                            {
                                                                /// <summary>
                                                                /// Runs
                                                                /// </summary>
                                                                public abstract void Run();

                                                                /// <summary>
                                                                /// Stops
                                                                /// </summary>
                                                                public abstract void Stop();
                                                            }

                                                            /// <summary>
                                                            /// Derived
                                                            /// </summary>
                                                            public class C : B
                                                            {
                                                                // Ordinary comment
                                                                /// <inheritdoc/>
                                                                [System.Obsolete]
                                                                public override void Run()
                                                                {
                                                                }

                                                                /// <inheritdoc/>
                                                                // Trailing
                                                                public override void Stop()
                                                                {
                                                                }
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix keeps every line break of a CRLF document as CRLF when nothing but the reported comment
    /// stands in the member's leading trivia
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsCarriageReturnLineFeedOfPlainMember()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>Base</summary>
                                                       public abstract class B
                                                       {
                                                           /// <summary>Runs</summary>
                                                           public abstract void Run();
                                                       }

                                                       /// <summary>Derived</summary>
                                                       public class C : B
                                                       {
                                                           ///{|#0: <summary>
                                                           /// Runs here
                                                           /// </summary>
                                                       |}    public override void Run()
                                                           {
                                                           }
                                                       }
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>Base</summary>
                                                            public abstract class B
                                                            {
                                                                /// <summary>Runs</summary>
                                                                public abstract void Run();
                                                            }

                                                            /// <summary>Derived</summary>
                                                            public class C : B
                                                            {
                                                                /// <inheritdoc/>
                                                                public override void Run()
                                                                {
                                                                }
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps an indented region directive and the blank line behind it in front of the replaced
    /// comment, with every line break of a CRLF document kept as CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsIndentedRegionAndBlankLineWithCarriageReturnLineFeed()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       /// <summary>Base</summary>
                                                       public abstract class B
                                                       {
                                                           /// <summary>Runs</summary>
                                                           public abstract void Run();
                                                       }

                                                       /// <summary>Derived</summary>
                                                       public class C : B
                                                       {
                                                           #region B

                                                           ///{|#0: <summary>
                                                           /// Runs here
                                                           /// </summary>
                                                       |}    public override void Run()
                                                           {
                                                           }

                                                           #endregion // B
                                                       }
                                                       """);
        var fixedSource = NormalizeToCarriageReturnLineFeed("""
                                                            /// <summary>Base</summary>
                                                            public abstract class B
                                                            {
                                                                /// <summary>Runs</summary>
                                                                public abstract void Run();
                                                            }

                                                            /// <summary>Derived</summary>
                                                            public class C : B
                                                            {
                                                                #region B

                                                                /// <inheritdoc/>
                                                                public override void Run()
                                                                {
                                                                }

                                                                #endregion // B
                                                            }
                                                            """);

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps an indented nullable directive and an indented delimited ordinary comment between
    /// the replaced comment and the member in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsIndentedNullableDirectiveAndDelimitedCommentBehindComment()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public abstract class B
                              {
                                  /// <summary>Runs</summary>
                                  public abstract void Run();
                              }

                              /// <summary>Derived</summary>
                              public class C : B
                              {
                                  ///{|#0: <summary>Runs here</summary>
                              |}    #nullable enable
                                  /* Note */
                                  public override void Run()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   /// <summary>Base</summary>
                                   public abstract class B
                                   {
                                       /// <summary>Runs</summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>Derived</summary>
                                   public class C : B
                                   {
                                       /// <inheritdoc/>
                                       #nullable enable
                                       /* Note */
                                       public override void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps an indented if group with the code it excludes, and its else and endif
    /// directives, in front of the replaced comment in their columns
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifyFixKeepsIndentedConditionalGroupWithExcludedCodeInFrontOfComment()
    {
        const string source = """
                              /// <summary>Base</summary>
                              public abstract class B
                              {
                                  /// <summary>Runs</summary>
                                  public abstract void Run();
                              }

                              /// <summary>Derived</summary>
                              public class C : B
                              {
                                  #if NEVER
                                  int _excluded;
                                  #else
                                  #endif
                                  ///{|#0: <summary>Runs here</summary>
                              |}    public override void Run()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   /// <summary>Base</summary>
                                   public abstract class B
                                   {
                                       /// <summary>Runs</summary>
                                       public abstract void Run();
                                   }

                                   /// <summary>Derived</summary>
                                   public class C : B
                                   {
                                       #if NEVER
                                       int _excluded;
                                       #else
                                       #endif
                                       /// <inheritdoc/>
                                       public override void Run()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example of a partial member judged part by part: both documented parts of a method and
    /// the documented implementing part of a property are reported, and the defining part of the property, which uses
    /// inheritdoc, is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-004")]
    public async Task VerifySpecificationExamplePartialMemberIsJudgedPartByPart()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();

                                  /// <summary>
                                  /// Value
                                  /// </summary>
                                  int P { get; }
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  ///{|RH8201: <summary>Runs here</summary>
                              |}    public partial void M();

                                  ///{|RH8201: <summary>Runs there</summary>
                              |}    public partial void M()
                                  {
                                  }

                                  /// <inheritdoc/>
                                  public partial int P { get; }

                                  ///{|RH8201: <summary>Value here</summary>
                              |}    public partial int P => 0;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example of the fix on a partial member: the fix of each part replaces only that part's
    /// comment, and applying both gives inheritdoc on both parts
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-005")]
    public async Task VerifySpecificationExampleFixOfPartialMemberReplacesEachPart()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Runs here</summary>
                                |}    public partial void M();

                                    ///{|RH8201: <summary>Runs there</summary>
                                |}    public partial void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial void M();

                                     /// <inheritdoc/>
                                     public partial void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of the implementing part of a partial method replaces only that part's comment and leaves the
    /// defining part, which stays reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyFixOfImplementingPartLeavesDefiningPartReported()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    /// <summary>Runs here</summary>
                                    public partial void M();

                                    /// <summary>Runs there</summary>
                                    public partial void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     ///{|RH8201: <summary>Runs here</summary>
                                 |}    public partial void M();

                                     /// <inheritdoc/>
                                     public partial void M()
                                     {
                                     }
                                 }
                                 """;
        const string implementingComment = " <summary>Runs there</summary>\n";
        var implementingStart = testData.IndexOf(implementingComment, System.StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, implementingStart);

        const string markedData = """
                                  /// <summary>
                                  /// Contract
                                  /// </summary>
                                  public interface I
                                  {
                                      /// <summary>
                                      /// Runs
                                      /// </summary>
                                      void M();
                                  }

                                  /// <summary>
                                  /// Service
                                  /// </summary>
                                  public partial class C : I
                                  {
                                      ///{|RH8201: <summary>Runs here</summary>
                                  |}    public partial void M();

                                      ///{|RH8201: <summary>Runs there</summary>
                                  |}    public partial void M()
                                      {
                                      }
                                  }
                                  """;

        await Verify(markedData);

        var actual = await TryApplyCodeFixAtAsync(testData,
                                                  RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId,
                                                  root => Microsoft.CodeAnalysis.Location.Create(root.SyntaxTree, new Microsoft.CodeAnalysis.Text.TextSpan(implementingStart, implementingComment.Length)));

        Assert.AreEqual(RemoveMarkup(fixedData), actual);

        await Verify(fixedData);
    }

    /// <summary>
    /// Verifies that the documented parts of a partial method whose defining part uses inheritdoc and whose implementing
    /// part carries a summary are judged on their own: only the implementing part is reported, and the mirrored shape
    /// reports only the defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyOnlyThePartWithoutInheritdocOfPartialMethodIsReported()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void M();

                                  /// <summary>
                                  /// Stops
                                  /// </summary>
                                  void N();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C : I
                              {
                                  /// <inheritdoc/>
                                  public partial void M();

                                  ///{|RH8201: <summary>Runs there</summary>
                              |}    public partial void M()
                                  {
                                  }

                                  ///{|RH8201: <summary>Stops here</summary>
                              |}    public partial void N();

                                  /// <inheritdoc/>
                                  public partial void N()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that both documented parts of a partial property and of a partial indexer implementing interface members
    /// are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyBothPartsOfPartialPropertyAndIndexerAreDetectedAndFixed()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    int P { get; }

                                    /// <summary>
                                    /// Item
                                    /// </summary>
                                    /// <param name="index">Index</param>
                                    int this[int index] { get; }
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Value here</summary>
                                |}    public partial int P { get; }

                                    ///{|RH8201: <summary>Value there</summary>
                                |}    public partial int P => 0;

                                    ///{|RH8201: <summary>Item here</summary>
                                |}    public partial int this[int index] { get; }

                                    ///{|RH8201: <summary>Item there</summary>
                                |}    public partial int this[int index] => index;
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Value
                                     /// </summary>
                                     int P { get; }

                                     /// <summary>
                                     /// Item
                                     /// </summary>
                                     /// <param name="index">Index</param>
                                     int this[int index] { get; }
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial int P { get; }

                                     /// <inheritdoc/>
                                     public partial int P => 0;

                                     /// <inheritdoc/>
                                     public partial int this[int index] { get; }

                                     /// <inheritdoc/>
                                     public partial int this[int index] => index;
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that both documented parts of a partial event implementing an interface event, whose defining part is a
    /// field-like event, are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyBothPartsOfPartialEventAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Raised
                                    /// </summary>
                                    event EventHandler E;
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Raised here</summary>
                                |}    public partial event EventHandler E;

                                    ///{|RH8201: <summary>Raised there</summary>
                                |}    public partial event EventHandler E
                                    {
                                        add { }
                                        remove { }
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Raised
                                     /// </summary>
                                     event EventHandler E;
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial event EventHandler E;

                                     /// <inheritdoc/>
                                     public partial event EventHandler E
                                     {
                                         add { }
                                         remove { }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that both documented parts of a partial method declared override are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyBothPartsOfOverridingPartialMethodAreDetectedAndFixed()
    {
        const string testData = """
                                /// <summary>
                                /// Base
                                /// </summary>
                                public abstract class B
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    public abstract void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : B
                                {
                                    ///{|RH8201: <summary>Runs here</summary>
                                |}    public override partial void M();

                                    ///{|RH8201: <summary>Runs there</summary>
                                |}    public override partial void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Base
                                 /// </summary>
                                 public abstract class B
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     public abstract void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : B
                                 {
                                     /// <inheritdoc/>
                                     public override partial void M();

                                     /// <inheritdoc/>
                                     public override partial void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the documented parts of a partial method implementing an interface member, declared in two
    /// documents, are each reported, and that fixing every diagnostic replaces the comment in both documents
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    [Requirement("RH8201-005")]
    public async Task VerifyPartsOfPartialMethodInTwoDocumentsAreDetectedAndFixed()
    {
        const string testData = """
                                /// <summary>
                                /// Contract
                                /// </summary>
                                public interface I
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M();
                                }

                                /// <summary>
                                /// Service
                                /// </summary>
                                public partial class C : I
                                {
                                    ///{|RH8201: <summary>Runs here</summary>
                                |}    public partial void M();
                                }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Contract
                                 /// </summary>
                                 public interface I
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M();
                                 }

                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C : I
                                 {
                                     /// <inheritdoc/>
                                     public partial void M();
                                 }
                                 """;
        const string otherData = """
                                 /// <summary>
                                 /// Service
                                 /// </summary>
                                 public partial class C
                                 {
                                     ///{|RH8201: <summary>Runs there</summary>
                                 |}    public partial void M()
                                     {
                                     }
                                 }
                                 """;
        const string otherFixedData = """
                                      /// <summary>
                                      /// Service
                                      /// </summary>
                                      public partial class C
                                      {
                                          /// <inheritdoc/>
                                          public partial void M()
                                          {
                                          }
                                      }
                                      """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFixedData));
                     });
    }

    /// <summary>
    /// Verifies that the documented parts of a partial method that inherits nothing are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8201-001")]
    public async Task VerifyDocumentedPartsOfPartialMethodInheritingNothingAreNotReported()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public partial class C
                              {
                                  /// <summary>Runs here</summary>
                                  public partial void M();

                                  /// <summary>Runs there</summary>
                                  public partial void M()
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System;

                                namespace TestNamespace
                                {
                                    internal abstract class TestBase
                                    {
                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract void TestMethod();

                                        /// <summary>
                                        /// Base documentation
                                        /// </summary>
                                        public abstract int TestProperty { get; set; }
                                    }

                                    internal class TestImplementation : TestBase
                                    {
                                        /**{|#0: <summary>Implementation documentation</summary> */|}
                                        public override void TestMethod()
                                        {
                                        }

                                        /**{|#1: <summary>Implementation documentation</summary> */|}
                                        public override int TestProperty { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  namespace TestNamespace
                                  {
                                      internal abstract class TestBase
                                      {
                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract void TestMethod();

                                          /// <summary>
                                          /// Base documentation
                                          /// </summary>
                                          public abstract int TestProperty { get; set; }
                                      }

                                      internal class TestImplementation : TestBase
                                      {
                                          /// <inheritdoc/>
                                          public override void TestMethod()
                                          {
                                          }

                                          /// <inheritdoc/>
                                          public override int TestProperty { get; set; }
                                      }
                                  }
                                  """;

        // Verifies that Fix All replaces every multi-line (/** */) documentation comment in a type in one iteration, which is the common shape when a type overrides several documented members
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}