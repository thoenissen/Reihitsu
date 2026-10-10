using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8201InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH8201InheritdocShouldBeUsedCodeFixProvider"/>
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

    #endregion // Test data

    #region Methods

    /// <summary>
    /// Verifying diagnostic for overridden method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMethod()
    {
        await Verify(MethodTestData, MethodResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForProperty()
    {
        await Verify(PropertyTestData, PropertyResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEvent()
    {
        await Verify(EventTestData, EventResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that documented field-like override events are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
                                |}        public override event EventHandler TestEvent;
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
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that field-like override events already documented with &lt;inheritdoc/&gt; do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All replaces documentation on multiple field-like override events in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
                                |}        public override event EventHandler First;

                                    ///{|#1: <summary>Second implementation</summary>
                                |}        public override event EventHandler Second;
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
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 2));
    }

    /// <summary>
    /// Verifying diagnostic for overridden indexer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForIndexer()
    {
        await Verify(IndexerTestData, IndexerResultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment spanning several lines is replaced with
    /// &lt;inheritdoc/&gt; instead of the code fix registering a no-op action
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment written on a single line is replaced with
    /// &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a multi-line (/** */) documentation comment on an overridden property is replaced with
    /// &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a leading comment placed before a multi-line (/** */) documentation comment is preserved
    /// when the documentation comment is replaced with &lt;inheritdoc/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that the synthesized &lt;inheritdoc/&gt; trivia replacing a multi-line (/** */) documentation
    /// comment uses the environment's end-of-line sequence
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineDocumentationCommentReplacementUsesEnvironmentEndOfLine()
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

        Assert.Contains($"/// <inheritdoc/>{System.Environment.NewLine}        public override void TestMethod()", fixedSource);
    }

    /// <summary>
    /// Verifies that a trailing comment sharing the line with a multi-line (/** */) documentation comment
    /// survives the replacement and keeps its own line. The fix drops the line break that terminated the
    /// replaced comment, so this guards that deletion against consuming the trailing comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a preprocessor directive between a multi-line (/** */) documentation comment and the
    /// member keeps its own line when the comment is replaced. The fix drops the line break that terminated
    /// the replaced comment, so this guards that deletion against joining the directive onto the
    /// &lt;inheritdoc/&gt; line, which would not compile
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that a documented implicit interface method implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitInterfaceMethod()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface property implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitInterfaceProperty()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int TestProperty { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public int TestProperty => 0;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int TestProperty { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public int TestProperty => 0;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface indexer implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitInterfaceIndexer()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public int this[int index] => index;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int this[int index] { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public int this[int index] => index;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface event implementation with accessors is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitInterfaceEvent()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public event EventHandler TestEvent
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

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public event EventHandler TestEvent
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface field-like event implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitInterfaceFieldLikeEvent()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public event EventHandler TestEvent;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public event EventHandler TestEvent;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented field-like event is detected and fixed when every declarator implements an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFieldLikeEventWhoseDeclaratorsAllImplementInterfaceEvents()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler First;

                                    event EventHandler Second;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public event EventHandler First, Second;
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler First;

                                      event EventHandler Second;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public event EventHandler First, Second;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented field-like event is not reported when only its first declarator implements an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFieldLikeEventWhoseSecondDeclaratorImplementsNothing()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler First;
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public event EventHandler First, Second;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented field-like event is not reported when only its second declarator implements an interface event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFieldLikeEventWhoseFirstDeclaratorImplementsNothing()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler Second;
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public event EventHandler First, Second;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit interface method implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitInterfaceMethod()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void ITest.TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      void ITest.TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit interface property implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitInterfaceProperty()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int TestProperty { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    int ITest.TestProperty => 0;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int TestProperty { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      int ITest.TestProperty => 0;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit interface indexer implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitInterfaceIndexer()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    int this[int index] { get; }
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    int ITest.this[int index] => index;
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      int this[int index] { get; }
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      int ITest.this[int index] => index;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit interface event implementation is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitInterfaceEvent()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    event EventHandler ITest.TestEvent
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

        const string resultData = """
                                  using System;

                                  internal interface ITest
                                  {
                                      event EventHandler TestEvent;
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      event EventHandler ITest.TestEvent
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

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of an interface declared in metadata is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMetadataInterfaceImplementation()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of a generic interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForGenericInterfaceImplementation()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IEquatable<TestImplementation>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public bool Equals(TestImplementation other)
                                    {
                                        return ReferenceEquals(this, other);
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IEquatable<TestImplementation>
                                  {
                                      /// <inheritdoc/>
                                      public bool Equals(TestImplementation other)
                                      {
                                          return ReferenceEquals(this, other);
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of a member declared on a base interface is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplementationOfInheritedInterfaceMember()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived : IBase
                                {
                                }

                                internal class TestImplementation : IDerived
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface IBase
                                  {
                                      void TestMethod();
                                  }

                                  internal interface IDerived : IBase
                                  {
                                  }

                                  internal class TestImplementation : IDerived
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented virtual member implementing an interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForVirtualInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public virtual void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public virtual void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented member implementing members of several interfaces is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMemberImplementingSeveralInterfaces()
    {
        const string testData = """
                                internal interface IFirst
                                {
                                    void TestMethod();
                                }

                                internal interface ISecond
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : IFirst, ISecond
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface IFirst
                                  {
                                      void TestMethod();
                                  }

                                  internal interface ISecond
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestImplementation : IFirst, ISecond
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a member implementing several interfaces is not reported when it selects the inherited documentation with &lt;inheritdoc cref="..."/&gt;
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMemberImplementingSeveralInterfacesWithInheritdocCref()
    {
        const string testData = """
                                internal interface IFirst
                                {
                                    void TestMethod();
                                }

                                internal interface ISecond
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : IFirst, ISecond
                                {
                                    /// <inheritdoc cref="IFirst.TestMethod"/>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implementation of a static abstract interface method is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStaticAbstractInterfaceMethod()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    static abstract void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public static void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      static abstract void TestMethod();
                                  }

                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public static void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit implementation of a static abstract interface operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitStaticAbstractOperator()
    {
        const string testData = """
                                internal interface IAddable<T>
                                    where T : IAddable<T>
                                {
                                    static abstract T operator +(T left, T right);
                                }

                                internal readonly struct Amount : IAddable<Amount>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public static Amount operator +(Amount left, Amount right) => left;
                                }
                                """;

        const string resultData = """
                                  internal interface IAddable<T>
                                      where T : IAddable<T>
                                  {
                                      static abstract T operator +(T left, T right);
                                  }

                                  internal readonly struct Amount : IAddable<Amount>
                                  {
                                      /// <inheritdoc/>
                                      public static Amount operator +(Amount left, Amount right) => left;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented explicit implementation of a static abstract interface operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitStaticAbstractOperator()
    {
        const string testData = """
                                internal interface IAddable<T>
                                    where T : IAddable<T>
                                {
                                    static abstract T operator +(T left, T right);
                                }

                                internal readonly struct Amount : IAddable<Amount>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    static Amount IAddable<Amount>.operator +(Amount left, Amount right) => left;
                                }
                                """;

        const string resultData = """
                                  internal interface IAddable<T>
                                      where T : IAddable<T>
                                  {
                                      static abstract T operator +(T left, T right);
                                  }

                                  internal readonly struct Amount : IAddable<Amount>
                                  {
                                      /// <inheritdoc/>
                                      static Amount IAddable<Amount>.operator +(Amount left, Amount right) => left;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implementation of a static abstract interface conversion operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForStaticAbstractConversionOperator()
    {
        const string testData = """
                                internal interface IConvertible<T>
                                    where T : IConvertible<T>
                                {
                                    static abstract implicit operator int(T value);
                                }

                                internal readonly struct Amount : IConvertible<Amount>
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public static implicit operator int(Amount value) => 0;
                                }
                                """;

        const string resultData = """
                                  internal interface IConvertible<T>
                                      where T : IConvertible<T>
                                  {
                                      static abstract implicit operator int(T value);
                                  }

                                  internal readonly struct Amount : IConvertible<Amount>
                                  {
                                      /// <inheritdoc/>
                                      public static implicit operator int(Amount value) => 0;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented operator that implements no interface member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForOperatorWithoutInterface()
    {
        const string testData = """
                                internal readonly struct Amount
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public static Amount operator +(Amount left, Amount right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented conversion operator that implements no interface member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConversionOperatorWithoutInterface()
    {
        const string testData = """
                                internal readonly struct Amount
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public static implicit operator int(Amount value) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit implementation of a base interface member inside an interface is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForInterfaceMemberOverridingBaseInterfaceMember()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived : IBase
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void IBase.TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface IBase
                                  {
                                      void TestMethod();
                                  }

                                  internal interface IDerived : IBase
                                  {
                                      /// <inheritdoc/>
                                      void IBase.TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented interface member hiding a base interface member with new is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInterfaceMemberHidingBaseInterfaceMember()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived : IBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    new void TestMethod();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member declared by an interface itself is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInterfaceOwnMember()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    void TestMethod();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member re-implementing an interface member with new is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReimplementationWithNew()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public new void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal class TestBase : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }

                                  internal class TestImplementation : TestBase, ITest
                                  {
                                      /// <inheritdoc/>
                                      public new void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented member hiding an inherited implementation is not reported when its type does not list the interface itself
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForHidingMemberWithoutReimplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public new void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented base member is not reported when only a derived type lists the interface it satisfies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForBaseMemberWhenOnlyDerivedTypeListsInterface()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestBase
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented overload of an implemented member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForOverloadOfImplementedMember()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod(int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented public member is not reported when an explicit implementation of the same interface member exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPublicMemberBesideExplicitImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    void ITest.TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented member of a type implementing an interface is not reported when it implements no interface member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMemberImplementingNoInterface()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <inheritdoc/>
                                    public void TestMethod()
                                    {
                                    }

                                    /// <summary>Implementation documentation</summary>
                                    public void OtherMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the documented defining declaration of a partial method implementing an interface member is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForDocumentedDefiningPartialInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal partial class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public partial void TestMethod();

                                    public partial void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void TestMethod();
                                  }

                                  internal partial class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public partial void TestMethod();

                                      public partial void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that the documented implementing declaration of a partial method implementing an interface member is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDocumentedImplementingPartialInterfaceImplementation()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void TestMethod();
                                }

                                internal partial class TestImplementation : ITest
                                {
                                    public partial void TestMethod();

                                    /// <summary>Implementation documentation</summary>
                                    public partial void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that override, implicit, and explicit interface implementation diagnostics in one document are fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForOverrideAndInterfaceImplementationsAreFixedTogether()
    {
        const string testData = """
                                internal interface ITest
                                {
                                    void First();

                                    void Second();
                                }

                                internal abstract class TestBase
                                {
                                    public abstract void Third();
                                }

                                internal class TestImplementation : TestBase, ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void First()
                                    {
                                    }

                                    ///{|#1: <summary>Implementation documentation</summary>
                                |}    void ITest.Second()
                                    {
                                    }

                                    ///{|#2: <summary>Implementation documentation</summary>
                                |}    public override void Third()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal interface ITest
                                  {
                                      void First();

                                      void Second();
                                  }

                                  internal abstract class TestBase
                                  {
                                      public abstract void Third();
                                  }

                                  internal class TestImplementation : TestBase, ITest
                                  {
                                      /// <inheritdoc/>
                                      public void First()
                                      {
                                      }

                                      /// <inheritdoc/>
                                      void ITest.Second()
                                      {
                                      }

                                      /// <inheritdoc/>
                                      public override void Third()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitImplementationWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when the interface list of its type contains disabled text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitImplementationWhenAnotherInterfaceIsDisabled()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                #if FEATURE
                                    , ICloneable
                                #endif
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when another partial declaration of its type has a conditionally compiled interface list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForImplicitImplementationWhenOtherPartialDeclarationIsConditional()
    {
        const string testData = """
                                using System;

                                internal partial class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                }

                                internal partial class TestImplementation
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented field-like event implementing an interface event is not reported when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForFieldLikeEventWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal interface ITest
                                {
                                    event EventHandler TestEvent;
                                }

                                internal class TestImplementation
                                #if !FEATURE
                                    : ITest
                                #endif
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public event EventHandler TestEvent;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented explicit interface implementation is detected and fixed even when the interface list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExplicitImplementationWhenInterfaceListIsConditional()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation
                                #if !FEATURE
                                    : IDisposable
                                #endif
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    void IDisposable.Dispose()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation
                                  #if !FEATURE
                                      : IDisposable
                                  #endif
                                  {
                                      /// <inheritdoc/>
                                      void IDisposable.Dispose()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented override is detected and fixed even when the base list of its type is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOverrideWhenBaseListIsConditional()
    {
        const string testData = """
                                internal abstract class TestBase
                                {
                                    public abstract void TestMethod();
                                }

                                internal class TestImplementation
                                #if !FEATURE
                                    : TestBase
                                #endif
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public override void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal abstract class TestBase
                                  {
                                      public abstract void TestMethod();
                                  }

                                  internal class TestImplementation
                                  #if !FEATURE
                                      : TestBase
                                  #endif
                                  {
                                      /// <inheritdoc/>
                                      public override void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when directives appear only inside the type body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitImplementationWhenDirectiveIsInsideTypeBody()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    #region Methods

                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                      #region Methods

                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }

                                      #endregion // Methods
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when a conditional region encloses the partial declaration that lists the interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenConditionalRegionEnclosesPartialDeclaration()
    {
        const string testData = """
                                using System;

                                internal partial class TestImplementation
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void Dispose()
                                    {
                                    }
                                }

                                #if !FEATURE
                                internal partial class TestImplementation : IDisposable
                                {
                                }
                                #endif
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when conditional regions supply alternative headers for its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenConditionalRegionSuppliesAlternativeHeaders()
    {
        const string testData = """
                                using System;

                                #if !FEATURE
                                internal class TestImplementation : IDisposable
                                {
                                #else
                                internal class TestImplementation
                                {
                                #endif
                                    /// <summary>Implementation documentation</summary>
                                    public void Dispose()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when the implemented interface member is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenInterfaceMemberIsConditional()
    {
        const string testData = """
                                internal interface ITest
                                {
                                #if !FEATURE
                                    void TestMethod();
                                #endif
                                }

                                internal class TestImplementation : ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when the base list of an implemented interface is conditionally compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenInterfaceBaseListIsConditional()
    {
        const string testData = """
                                internal interface IBase
                                {
                                    void TestMethod();
                                }

                                internal interface IDerived
                                #if !FEATURE
                                    : IBase
                                #endif
                                {
                                }

                                internal class TestImplementation : IDerived
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when the file declaring its type contains any conditional region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenFileOfTypeContainsUnrelatedConditionalRegion()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void Dispose()
                                    {
                                #if DEBUG
                                        Console.WriteLine();
                                #endif
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when the file contains directives other than conditional regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForImplicitImplementationWhenFileContainsOnlyPragmaDirective()
    {
        const string testData = """
                                using System;

                                internal class TestImplementation : IDisposable
                                {
                                #pragma warning disable CS0168
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void Dispose()
                                    {
                                    }
                                #pragma warning restore CS0168
                                }
                                """;

        const string resultData = """
                                  using System;

                                  internal class TestImplementation : IDisposable
                                  {
                                  #pragma warning disable CS0168
                                      /// <inheritdoc/>
                                      public void Dispose()
                                      {
                                      }
                                  #pragma warning restore CS0168
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is not reported when the file declaring the
    /// implemented interface contains a conditional region, even though the file declaring the type contains none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenInterfaceFileContainsConditionalRegion()
    {
        const string testData = """
                                internal class TestImplementation : ITest
                                {
                                    /// <summary>Implementation documentation</summary>
                                    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string interfaceData = """
                                     internal interface ITest
                                     {
                                         void TestMethod();
                                     #if FEATURE
                                         void OtherMethod();
                                     #endif
                                     }
                                     """;

        await Verify(testData, test => test.TestState.Sources.Add(("/0/Test1.cs", interfaceData)));
    }

    /// <summary>
    /// Verifies that a documented implicit interface implementation is detected and fixed when only a file that
    /// declares neither its type nor its interfaces contains a conditional region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenOnlyUnrelatedFileContainsConditionalRegion()
    {
        const string testData = """
                                internal class TestImplementation : ITest
                                {
                                    ///{|#0: <summary>Implementation documentation</summary>
                                |}    public void TestMethod()
                                    {
                                    }
                                }
                                """;

        const string resultData = """
                                  internal class TestImplementation : ITest
                                  {
                                      /// <inheritdoc/>
                                      public void TestMethod()
                                      {
                                      }
                                  }
                                  """;

        const string interfaceData = """
                                     internal interface ITest
                                     {
                                         void TestMethod();
                                     }
                                     """;

        const string unrelatedData = """
                                     internal class Unrelated
                                     {
                                     #if FEATURE
                                         public void OtherMethod()
                                         {
                                         }
                                     #endif
                                     }
                                     """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.TestState.Sources.Add(("/0/Test2.cs", unrelatedData));
                         test.FixedState.Sources.Add(("/0/Test1.cs", interfaceData));
                         test.FixedState.Sources.Add(("/0/Test2.cs", unrelatedData));
                     },
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
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
                                  Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}