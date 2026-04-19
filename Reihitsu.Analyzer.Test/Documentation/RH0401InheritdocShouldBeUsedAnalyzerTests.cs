using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0401InheritdocShouldBeUsedAnalyzer"/> and <see cref="RH0401InheritdocShouldBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0401InheritdocShouldBeUsedAnalyzerTests : AnalyzerTestsBase<RH0401InheritdocShouldBeUsedAnalyzer, RH0401InheritdocShouldBeUsedCodeFixProvider>
{
    #region Test data

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
        await Verify(MethodTestData, MethodResultData, Diagnostics(RH0401InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0401MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForProperty()
    {
        await Verify(PropertyTestData, PropertyResultData, Diagnostics(RH0401InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0401MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEvent()
    {
        await Verify(EventTestData, EventResultData, Diagnostics(RH0401InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0401MessageFormat, 1));
    }

    /// <summary>
    /// Verifying diagnostic for overridden indexer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForIndexer()
    {
        await Verify(IndexerTestData, IndexerResultData, Diagnostics(RH0401InheritdocShouldBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0401MessageFormat, 1));
    }

    #endregion // Methods
}