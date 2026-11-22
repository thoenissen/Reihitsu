using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7309RegionsShouldFollowCategoryOrderCodeFixProvider"/> on moves that would change the order of initializers or the layout of a struct, in the active code and in other configurations
/// </summary>
[TestClass]
public class RH7309RegionsShouldFollowCategoryOrderInitializationOrderTests : BatchCodeFixTestsBase<RH7309RegionsShouldFollowCategoryOrderAnalyzer, RH7309RegionsShouldFollowCategoryOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies the specification example: moving the block would run a static initializer before the one that reads its
    /// field, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAnInitializerReadsAFieldOfABlockItWouldCross()
    {
        const string source = """
                              using System;

                              public class C : IComparable
                              {
                                  #region IComparable

                                  private static readonly int Y = X + 1;

                                  public int CompareTo(object obj) => Y;

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private static readonly int X = 5;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would put a static initializer of a block in front of a static
    /// initializer between the blocks that runs before it today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAnInitializerWouldChangeOrderWithAMemberBetweenTheBlocks()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int Y = X + 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  private static readonly int X = 5;

                                  {|RH7309:#region Fields|}

                                  private static int _z = 1;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would swap two instance field initializers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenTwoInstanceInitializersWouldChangeOrder()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private int _a = 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _b = 2;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would swap the initializer of an event field and the initializer
    /// of a property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAnEventInitializerAndAPropertyInitializerWouldChangeOrder()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public event EventHandler Changed = delegate { };

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  public int Count { get; set; } = 1;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would swap an initializer that another build configuration
    /// compiles with an initializer of the other block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenAnInitializerInDisabledTextWouldChangeOrder()
    {
        const string source = """
                              using System;

                              public class C : IComparable
                              {
                                  #region IComparable

                              #if DEBUG
                                  private static readonly int Y = X + 1;
                              #endif

                                  public int CompareTo(object obj) => 0;

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private static readonly int X = 5;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a static and an instance initializer, which run at different times, do not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersAStaticAndAnInstanceInitializer()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static int _s = 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _i = 2;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       private int _i = 2;

                                       #endregion // Fields

                                       #region IDisposable

                                       private static int _s = 1;

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that constant fields, which have no runtime initializer, do not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersConstantFields()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private const int A = 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private const int B = A;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       private const int B = A;

                                       #endregion // Fields

                                       #region IDisposable

                                       private const int A = 1;

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that initializers of nested types in the blocks, which belong to other contexts, do not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersInitializersOfNestedTypes()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private class Inner
                                  {
                                      public int A = 1;
                                  }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  public void M()
                                  {
                                  }

                                  private class Other
                                  {
                                      public int B = 2;
                                  }

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       public void M()
                                       {
                                       }

                                       private class Other
                                       {
                                           public int B = 2;
                                       }

                                       #endregion // Fields

                                       #region IDisposable

                                       private class Inner
                                       {
                                           public int A = 1;
                                       }

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: moving the block would change the order of the struct's instance fields, so
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixForTheInstanceFieldsOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: the backing field of an auto-property counts like a declared field of the
    /// struct, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixForAnAutoPropertyOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int Handle { get; set; }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the backing field of an instance field-like event counts like a declared field of the struct, so no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixForAnInstanceFieldLikeEventOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public event EventHandler Changed;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the backing field of a property that uses the <c>field</c> keyword counts like a declared field of
    /// the struct, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixForAPropertyThatUsesTheFieldKeywordOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int Handle { get => field; set => field = value; }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would change the order of the instance fields of a class
    /// declared with sequential layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixForTheInstanceFieldsOfAClassWithSequentialLayout()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld for the instance fields of a partial class whose sequential layout another part
    /// declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixForTheInstanceFieldsOfAPartialClassWithSequentialLayoutOnAnotherPart()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public partial class P
                              {
                              }

                              public partial class P : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix reorders the instance fields of a partial struct whose automatic layout another part
    /// declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersTheInstanceFieldsOfAPartialStructWithAutoLayoutOnAnotherPart()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Auto)]
                              public partial struct S
                              {
                              }

                              public partial struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;
                                   using System.Runtime.InteropServices;

                                   [StructLayout(LayoutKind.Auto)]
                                   public partial struct S
                                   {
                                   }

                                   public partial struct S : IDisposable
                                   {
                                       #region Fields

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       private int _handle;

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a property that reads a member named <c>field</c> before C# 14 has no backing field, so the reorder
    /// keeps the order of the struct's instance fields and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeFixReordersAStructWhosePropertyReadsAMemberNamedFieldBeforeCSharp14()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int P
                                  {
                                      get
                                      {
                                          return field;
                                      }
                                  }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static int field;

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public struct S : IDisposable
                                   {
                                       #region Fields

                                       private static int field;

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       public int P
                                       {
                                           get
                                           {
                                               return field;
                                           }
                                       }

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test => test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.CSharp13))
                                                                                      : solution));
    }

    /// <summary>
    /// Verifies that a conditional group that completes the initializer of a field of the moved block lies in the moved
    /// span, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static readonly int B
                              #if DEBUG
                                      = Log("b")
                              #endif
                                      ;

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group inside a field of the moved block whose excluded branch ends the field and begins
    /// another declaration withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenExcludedCodeInsideAFieldOfTheMovedBlockDeclaresAnInitializedField(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static readonly int B
                              #if DEBUG
                                      ;
                                  private static readonly int Z = Log("z")
                              #endif
                                      ;

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group behind the accessor list of a property of the moved block lies in the moved
    /// span, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAPropertyOfTheMovedBlockHasAnInitializerBehindItsAccessorListOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static int B { get; }
                              #if DEBUG
                                      = Log("b");
                              #endif

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between two modifiers of a field of the moved block lies in the moved span, so
    /// no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockIsAnInstanceFieldOfAStructOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _a;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private
                              #if !DEBUG
                                      static
                              #endif
                                      int _b;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the other side of the split <c>static</c>: a field of a struct that only a build with <c>DEBUG</c>
    /// declares <c>static</c> is an instance field in the active code already, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockIsAnInstanceFieldOfAStructInTheActiveCode(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _a;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int _b;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group that completes the initializer of a field between the region blocks, which the
    /// reorder crosses, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenACrossedFieldBetweenTheBlocksHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  private static readonly int G
                              #if DEBUG
                                      = Log("g")
                              #endif
                                      ;

                                  {|RH7309:#region Fields|}

                                  private static readonly int A = Log("a");

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group at the start of the moved block whose branches begin a declaration that ends
    /// behind the group withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenExcludedCodeThatEndsInTheHeadOfAFieldDeclaresAnInitializedField(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                              #if DEBUG
                                  private static readonly int X = Log("x");
                                  private static readonly int Y
                              #else
                                  private static readonly int Y
                              #endif
                                      ;

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the SHARED-ORDER-008 example of a member that an <c>#if</c> splits inside a region block: the <c>#endif</c>
    /// stands inside the declaration of <c>Y</c>, and the excluded branch begins a declaration that it does not end, so no
    /// code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAnIfSplitsAMemberInsideTheMovedBlock(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                              #if DEBUG
                                  private static readonly int X = Log("x");
                                  private static readonly int Y =
                              #else
                                  private static readonly int Y =
                              #endif
                                      Log("y");

                                  #endregion // Fields

                                  private static int Log(string name) => 0;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group in front of a field of the moved block whose branches hold its modifiers lies
    /// in the moved span, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockIsStaticOnlyInAGroupInFrontOfItsFirstToken(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  private static int _a = Log("a");

                                  {|RH7309:#region Fields|}

                              #if DEBUG
                                  private static
                              #else
                                  private
                              #endif
                                  int _b = Log("b");

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group in front of a field of the moved block of a struct whose branches write different
    /// modifiers withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockOfAStructLacksStaticOnlyInAnExcludedBranch(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _a;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                              #if DEBUG
                                  internal
                              #else
                                  private static
                              #endif
                                  int _b;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a property of a struct with <c>get =&gt; 0;</c> and <c>set;</c> declares a backing field, an instance
    /// field of the layout, because one of its accessors lacks a body: moving the <c>Fields</c> block in front of it would
    /// put <c>_x</c> in front of that backing field, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAPropertyOfAStructHasAnAccessorWithoutABodyNextToAnAccessorWithABody(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                                      set;
                                  }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the counterpart of an accessor without a body: with <c>get =&gt; 0;</c> and <c>set { }</c>, no accessor lacks
    /// a body, the property declares no backing field, and the fix moves the <c>Fields</c> block in front of the
    /// <c>IComparable</c> block
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task CodeFixMovesABlockOfAStructAcrossAPropertyWhoseAccessorsAllHaveABody(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                                      set { }
                                  }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   public struct S : System.IComparable
                                   {
                                       #region Fields

                                       private int _x;

                                       #endregion // Fields

                                       #region IComparable

                                       public int Value
                                       {
                                           get => 0;
                                           set { }
                                       }

                                       public int CompareTo(object obj)
                                       {
                                           return 0;
                                       }

                                       #endregion // IComparable
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the counterpart of the layout: in a class, whose field order sets no layout, a property with
    /// <c>get =&gt; 0;</c> and <c>set;</c> keeps the fix, which moves the <c>Fields</c> block in front of the
    /// <c>IComparable</c> block
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task CodeFixMovesABlockOfAClassAcrossAPropertyWithAnAccessorWithoutABodyNextToAnAccessorWithABody(string endOfLine)
    {
        const string source = """
                              public class S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                                      set;
                                  }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   public class S : System.IComparable
                                   {
                                       #region Fields

                                       private int _x;

                                       #endregion // Fields

                                       #region IComparable

                                       public int Value
                                       {
                                           get => 0;
                                           set;
                                       }

                                       public int CompareTo(object obj)
                                       {
                                           return 0;
                                       }

                                       #endregion // IComparable
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group around the <c>StructLayout</c> attribute of a class makes the class count as one
    /// whose field order sets its layout: in a build with <c>X</c>, the class has sequential layout, and the reorder would
    /// put <c>_x</c> in front of the backing field of <c>Value</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAConditionalDirectiveStandsInTheAttributeListsOfTheType(string endOfLine)
    {
        const string source = """
                              using System.Runtime.InteropServices;

                              #if X
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public class S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get;
                                      set;
                                  }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the implementing declaration of a partial property of a struct with <c>get =&gt; field;</c> and
    /// <c>set;</c> counts as declaring a backing field, an instance field of the layout: moving the <c>Fields</c> block in
    /// front of it would put <c>_x</c> in front of that backing field, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenTheImplementingDeclarationOfAPartialPropertyOfAStructHasAnAccessorWithoutABodyAndUsesTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public partial struct S
                              {
                                  public partial int Value { get; set; }
                              }

                              public partial struct S : System.IComparable
                              {
                                  #region IComparable

                                  public partial int Value { get => field; set; }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the implementing declaration of a partial property of a struct with <c>get =&gt; 0;</c> and
    /// <c>set;</c> counts as declaring a backing field, because one of its accessors lacks a body: moving the
    /// <c>Fields</c> block in front of it would put <c>_x</c> in front of that backing field, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenTheImplementingDeclarationOfAPartialPropertyOfAStructHasAnAccessorWithoutABody(string endOfLine)
    {
        const string source = """
                              public partial struct S
                              {
                                  public partial int Value { get; set; }
                              }

                              public partial struct S : System.IComparable
                              {
                                  #region IComparable

                                  public partial int Value { get => 0; set; }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the defining declaration of a partial property of a struct, whose accessors all lack a body,
    /// counts as declaring a backing field, as every property with an accessor without a body does: moving the
    /// <c>Fields</c> block in front of it is refused, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenTheDefiningDeclarationOfAPartialPropertyOfAStructStandsInTheCrossedBlock(string endOfLine)
    {
        const string source = """
                              public partial struct S
                              {
                                  public partial int Value { get => 0; set { } }
                              }

                              public partial struct S : System.IComparable
                              {
                                  #region IComparable

                                  public partial int Value { get; set; }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the counterpart of the partial property declarations: an implementing declaration whose accessors all
    /// have a body and that does not use the <c>field</c> keyword declares no backing field, and the fix moves the
    /// <c>Fields</c> block in front of the <c>IComparable</c> block
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task CodeFixMovesABlockOfAStructAcrossTheImplementingDeclarationOfAPartialPropertyWhoseAccessorsAllHaveABody(string endOfLine)
    {
        const string source = """
                              public partial struct S
                              {
                                  public partial int Value { get; set; }
                              }

                              public partial struct S : System.IComparable
                              {
                                  #region IComparable

                                  public partial int Value { get => 0; set { } }

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   public partial struct S
                                   {
                                       public partial int Value { get; set; }
                                   }

                                   public partial struct S : System.IComparable
                                   {
                                       #region Fields

                                       private int _x;

                                       #endregion // Fields

                                       #region IComparable

                                       public partial int Value { get => 0; set { } }

                                       public int CompareTo(object obj)
                                       {
                                           return 0;
                                       }

                                       #endregion // IComparable
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a group in front of the first region block whose place the fix changes, whose excluded branch closes
    /// the type's body and opens a struct, makes the type count as one whose field order sets its layout: in a build with
    /// <c>X</c>, the reorder would put <c>_x</c> in front of <c>_handle</c> in the struct <c>D</c>, so no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupInFrontOfTheMovedBlocksClosesTheTypeAndOpensAStruct(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region Constants

                                  private const int Zero = 0;

                                  #endregion // Constants

                              #if X
                              }

                              public struct D : IDisposable
                              {
                              #endif
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional <c>StructLayout</c> attribute list on another declaration of a partial class makes the
    /// class count as one whose field order sets its layout: in a build with <c>X</c>, the reorder would put <c>_x</c> in
    /// front of <c>_handle</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAConditionalGroupWrapsTheLayoutAttributeOfAnotherDeclarationOfAPartialClass(string endOfLine)
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              #if X
                              [StructLayout(LayoutKind.Sequential)]
                              #endif
                              public partial class C
                              {
                              }

                              public partial class C : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _x;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes the provided text with the given line break
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line break</param>
    /// <returns>The text with every line break replaced by <paramref name="endOfLine"/></returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    /// <summary>
    /// Creates the expected diagnostic message for the given region description
    /// </summary>
    /// <param name="regionDescription">Region description</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionDescription)
    {
        return string.Format(MessageFormat, regionDescription);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase, IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region WidgetBase|}

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase

                                    {|#1:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class WidgetBase
                                 {
                                     public virtual void Reset()
                                     {
                                     }
                                 }

                                 internal class Widget : WidgetBase, IWidget
                                 {
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                     #region WidgetBase

                                     public override void Reset()
                                     {
                                     }

                                     #endregion // WidgetBase

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        // Both diagnostics register the fix that reorders every top-level region of the same type into
        // canonical order, so the two computed fixes are byte-for-byte identical text changes: the strongest
        // form of interference in this batch, where the second fix is discarded as a duplicate of the first
        // and the already-canonical result still has to satisfy the scenario in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                              index => CreateMessage(index == 0 ? "WidgetBase" : "Methods"),
                                              2));
    }

    #endregion // BatchCodeFixTestsBase
}