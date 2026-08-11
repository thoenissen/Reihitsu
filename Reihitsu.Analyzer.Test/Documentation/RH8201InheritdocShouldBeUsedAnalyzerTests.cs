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
public class RH8201InheritdocShouldBeUsedAnalyzerTests : AnalyzerTestsBase<RH8201InheritdocShouldBeUsedAnalyzer, RH8201InheritdocShouldBeUsedCodeFixProvider>
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

        await Verify(testData, resultData, Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 1));
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
    /// &lt;inheritdoc/&gt; instead of the code fix registering a no-op action (issue #463)
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
    /// &lt;inheritdoc/&gt; (issue #463)
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
    /// &lt;inheritdoc/&gt; (issue #463)
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
    /// when the documentation comment is replaced with &lt;inheritdoc/&gt; (issue #463)
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
    /// carries a second documentation comment (issue #463).
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
    /// comment uses the document's detected CRLF end-of-line sequence (issue #463)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineDocumentationCommentReplacementUsesDetectedCarriageReturnLineFeedEndOfLine()
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

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.Contains("/// <inheritdoc/>\r\n        public override void TestMethod()", fixedSource);
    }

    /// <summary>
    /// Verifies that a trailing comment sharing the line with a multi-line (/** */) documentation comment
    /// survives the replacement and keeps its own line. The fix drops the line break that terminated the
    /// replaced comment, so this guards that deletion against consuming the trailing comment (issue #463)
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
    /// &lt;inheritdoc/&gt; line, which would not compile (issue #463)
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
    /// Verifies that Fix All replaces every multi-line (/** */) documentation comment in a type in one
    /// iteration, which is the common shape when a type overrides several documented members (issue #463)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleMultiLineDocumentationCommentsAreFixedInOneFixAllIteration()
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

        await Verify(testData,
                     resultData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH8201InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH8201MessageFormat, 2));
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
    /// Verifies that the synthesized &lt;inheritdoc/&gt; trivia uses the document's detected CRLF end-of-line
    /// sequence instead of <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line
    /// endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySynthesizedInheritdocTriviaUsesDetectedCarriageReturnLineFeedEndOfLine()
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

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.Contains("/// <inheritdoc/>\r\n", fixedSource);
    }

    #endregion // Methods
}