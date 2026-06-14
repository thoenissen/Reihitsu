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

    #endregion // Methods
}