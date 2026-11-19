using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7309RegionsShouldFollowCategoryOrderAnalyzer"/>
/// </summary>
public partial class RH7309RegionsShouldFollowCategoryOrderAnalyzerTests
{
    #region Tests

    /// <summary>
    /// Verifies the specification example in the documentation mode None: the comment behind the closing brace is an
    /// ordinary comment that moves with its block, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixMovesAnOrdinaryCommentBehindTheLastCodeOfABlockWithTheBlockInDocumentationModeNone()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  } /// <summary>M</summary>

                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Methods

                                       public void M()
                                       {
                                       }

                                       #endregion // Methods

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       } /// <summary>M</summary>

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
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
    /// Verifies that a documentation comment above the <c>#endregion</c> line of the last block, which documents nothing,
    /// withholds the fix when the block moves: behind the moved block, it would document the first member of the block
    /// that follows
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentAboveTheLastEndRegionLineWouldDocumentAMember()
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

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  /// <summary>Stray</summary>
                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment behind the last code of the last block, which documents nothing, withholds
    /// the fix when the block moves: in front of the block that follows it, it would document that block's first member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentBehindTheLastCodeOfTheLastBlockWouldDocumentAMember()
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

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  } /// <summary>Stray</summary>

                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment above the <c>#endregion</c> line of a last block that keeps its place, which
    /// documents nothing before and after the reorder, does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixKeepsADocumentationCommentThatDocumentsNothingInALastBlockThatKeepsItsPlace()
    {
        const string source = """
                              using System;

                              public class C : IDisposable, IComparable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields

                                  #region IComparable

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  /// <summary>Stray</summary>
                                  #endregion // IComparable
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable, IComparable
                                   {
                                       #region Fields

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable

                                       #region IComparable

                                       public int CompareTo(object obj)
                                       {
                                           return 0;
                                       }

                                       /// <summary>Stray</summary>
                                       #endregion // IComparable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a conditional group that completes the initializer of a field of the moved block stands between two
    /// tokens of the declaration, at no boundary of whole units, so no code fix is offered
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
    /// Verifies that a conditional group behind the accessor list of a property of the moved block whose excluded branch
    /// holds an initializer, which is neither whole members nor whole attribute lists, withholds the fix
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
    /// Verifies that a conditional group between two modifiers of a field of the moved block stands at no boundary of whole
    /// units, so no code fix is offered
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
    /// Verifies that a conditional group in front of a field of the moved block whose branches hold its modifiers, which
    /// are neither whole members nor whole attribute lists, withholds the fix
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
    /// Verifies that a group of whole accessors with an accessor without a body in one branch, in a property whose other
    /// branch has an accessor with a body, refuses the reorder like the <c>field</c> keyword: a build that compiles that
    /// accessor gives the property a backing field, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAPropertyOfAStructHasAnAutoAccessorOnlyInOneBranchOfItsAccessorList(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int Handle
                                  {
                              #if DEBUG
                                      get;
                              #else
                                      get => 0;
                              #endif
                                  }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the counterpart of a group of whole accessors: when every branch declares an accessor with a body, no
    /// branch holds an accessor without a body, the group wraps whole accessors, and the code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task CodeFixMovesABlockOfAStructAcrossAPropertyWhoseAccessorListBranchesAreAllBodied(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int Handle
                                  {
                              #if DEBUG
                                      get => 1;
                              #else
                                      get => 0;
                              #endif
                                  }

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

                                   public struct S : IDisposable
                                   {
                                       #region Fields

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       public int Handle
                                       {
                                   #if DEBUG
                                           get => 1;
                                   #else
                                           get => 0;
                                   #endif
                                       }

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the SHARED-ORDER-008 example of the <c>field</c> clause for a region reorder: without <c>TRACE</c> defined,
    /// a build with <c>TRACE</c> gives <c>Value</c> a backing field, and the reorder would put <c>_x</c> in front of it, so
    /// no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupInAnAccessorBodyOfAStructUsesTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get
                                      {
                              #if TRACE
                                          return field;
                              #else
                                          return 0;
                              #endif
                                      }
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
    /// Verifies the example of a conditional accessor without a body: the group of whole accessors holds <c>get;</c> in
    /// one branch while the other branch has <c>get =&gt; 0;</c>, which refuses the reorder like the <c>field</c>
    /// keyword, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAConditionalAccessorWithoutABodyGivesAPropertyOfAStructABackingField(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                              #if DEBUG
                                      get;
                              #else
                                      get => 0;
                              #endif
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
    /// Verifies the counterpart of the conditional accessor example: with <c>get =&gt; 0;</c> and <c>set { }</c> inside a
    /// group, no accessor lacks a body, the group wraps whole accessors, and the fix moves the <c>Fields</c> block in front
    /// of the <c>IComparable</c> block
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task CodeFixMovesABlockOfAStructAcrossAPropertyWhoseConditionalAccessorHasABody(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                              #if DEBUG
                                      set { }
                              #endif
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
                                   #if DEBUG
                                           set { }
                                   #endif
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
    /// Verifies that a group of whole accessors whose active branch holds <c>set;</c>, in a property whose <c>get =&gt; 0;</c>
    /// has a body, refuses the reorder of a struct's blocks like the <c>field</c> keyword, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAnActiveConditionalAccessorWithoutABodyStandsNextToAnAccessorWithABody(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                              #if !DEBUG
                                      set;
                              #endif
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
    /// Verifies that a group of whole accessors whose excluded branch holds <c>set;</c>, in a property whose
    /// <c>get =&gt; 0;</c> has a body, refuses the reorder of a struct's blocks like the <c>field</c> keyword, so no code fix
    /// is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAnExcludedConditionalAccessorWithoutABodyStandsNextToAnAccessorWithABody(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                              #if DEBUG
                                      set;
                              #endif
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
    /// Verifies the <c>field</c> clause for a group of whole accessors: the excluded <c>set =&gt; field = value;</c> gives
    /// <c>Value</c> a backing field in a build with <c>DEBUG</c>, so the reorder of the struct's blocks is refused and no
    /// code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupOfWholeAccessorsOfAStructPropertyUsesTheFieldKeyword(string endOfLine)
    {
        const string source = """
                              public struct S : System.IComparable
                              {
                                  #region IComparable

                                  public int Value
                                  {
                                      get => 0;
                              #if DEBUG
                                      set => field = value;
                              #endif
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

    /// <summary>
    /// Verifies the spec example: in a CRLF document whose line break behind a field of a moved block is LF, the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-007")]
    public async Task NoFixIsOfferedForALineFeedInAMovedBlockOfACarriageReturnLineFeedDocument()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(LineEndingSource.Replace("_a;", "_a;LF", System.StringComparison.Ordinal)).Replace("LF\r\n", "\n", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF inside a comment of a moved block withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-007")]
    public async Task NoFixIsOfferedForALineFeedInsideACommentOfAMovedBlock()
    {
        var source = LineEndingSource.Replace("    public void Dispose()", "    /* oneLF\n    two */\n    public void Dispose()", System.StringComparison.Ordinal);

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source).Replace("LF\r\n", "\n", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies the spec example: in a CRLF document whose line break behind the field between the blocks is LF, the fix is
    /// offered, the blocks change places, and every line break stays as it was, that LF included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-007")]
    public async Task FixIsOfferedForALineFeedBetweenTheBlocksOfACarriageReturnLineFeedDocument()
    {
        var plainSource = LineEndingSource.Replace("{|RH7309:", string.Empty, System.StringComparison.Ordinal)
                                          .Replace("|}", string.Empty, System.StringComparison.Ordinal);

        var source = NormalizeToCarriageReturnLineFeed(plainSource.Replace("_between;", "_between;LF", System.StringComparison.Ordinal)).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);
        var expected = NormalizeToCarriageReturnLineFeed(LineEndingFixedSource.Replace("_between;", "_between;LF", System.StringComparison.Ordinal)).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(source));
    }

    /// <summary>
    /// Verifies the spec example: in a document whose line breaks are all a lone carriage return, the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-007")]
    public async Task NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(LineEndingSource, "\r"));
    }

    /// <summary>
    /// Verifies that in a document whose line breaks are all CRLF the fix of the spec example is offered and keeps every
    /// line break CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-007")]
    public async Task FixIsOfferedInADocumentWhoseLineBreaksAreAllCarriageReturnLineFeed()
    {
        await VerifyCodeFixWithEndOfLine(LineEndingSource, LineEndingFixedSource, "\r\n");
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF in a region block that keeps its place among three blocks does not withhold
    /// the fix, the two other blocks change places, and the LF stays where it was
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-007")]
    public async Task FixIsOfferedForALineFeedInABlockThatKeepsItsPlaceOfACarriageReturnLineFeedDocument()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region Fields

                                  private int _a;LF

                                  #endregion // Fields

                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  #region Methods

                                  private void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;
        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       private int _a;LF

                                       #endregion // Fields

                                       #region Methods

                                       private void M()
                                       {
                                       }

                                       #endregion // Methods

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        var input = NormalizeToCarriageReturnLineFeed(source).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);
        var expected = NormalizeToCarriageReturnLineFeed(fixedSource).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(input));
    }

    /// <summary>
    /// Verifies the specification example in which the regions of the <c>#if</c> and the <c>#else</c> branch of one
    /// conditional directive are not compared, where the <c>#else</c> branch is compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsInTheIfAndElseBranchOfOneConditionalDirectiveAreNotCompared()
    {
        await Verify(RegionsInIfAndElseBranchSource);
    }

    /// <summary>
    /// Verifies the specification example in which the regions of the <c>#if</c> and the <c>#else</c> branch of one
    /// conditional directive are not compared, where the <c>#if</c> branch is compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsInTheIfAndElseBranchOfOneConditionalDirectiveAreNotComparedWhereTheSymbolIsDefined()
    {
        await Verify(RegionsInIfAndElseBranchSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies that the regions of the <c>#if</c> and the <c>#elif</c> branch of one conditional directive are not
    /// compared, neither where the first branch is compiled nor where no branch is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionsInTheIfAndElifBranchOfOneConditionalDirectiveAreNotCompared()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }

                                #if DEBUG
                                    #region IDisposable

                                    private int _a;

                                    #endregion
                                #elif TRACE
                                    #region Fields

                                    private int _b;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
        await Verify(testData, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the specification example in which a region in the compiled branch after a region outside every branch
    /// is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-005")]
    public async Task RegionInTheCompiledBranchAfterARegionOutsideEveryBranchIsReportedWithoutACodeFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                #if DEBUG
                                    {|RH7309:#region Diagnostics|}

                                    private int _calls;

                                    #endregion
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example in which a region in an excluded branch after a region outside every branch is
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionInAnExcludedBranchAfterARegionOutsideEveryBranchIsNotReported()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                #if DEBUG
                                    #region Diagnostics

                                    private int _calls;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in an excluded branch does not count as an earlier region of a region outside every branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionInAnExcludedBranchDoesNotCountAsAnEarlierRegion()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion
                                #else
                                    public void Dispose()
                                    {
                                    }
                                #endif

                                    #region Fields

                                    private int _a;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in the compiled branch counts as an earlier region of a region outside every branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionInTheCompiledBranchCountsAsAnEarlierRegion()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion
                                #else
                                    public void Dispose()
                                    {
                                    }
                                #endif

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example in which a region in an excluded branch is a block whose conditional directive
    /// lies outside it, so a later region is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-005")]
    public async Task NoCodeFixWhenARegionInAnExcludedBranchStandsInFrontOfTheReportedRegions()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    #region Diagnostics

                                    private int _calls;

                                    #endregion
                                #endif

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the region of a nested enum belongs to the enum and is not compared with the regions of the type
    /// that holds it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionOfANestedEnumIsNotComparedWithTheRegionsOfTheType()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    private enum E
                                    {
                                        #region Fields
                                        A,
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two regions inside a nested enum are not top-level regions of the type that holds the enum, so they
    /// are neither reported nor reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsInsideANestedEnumAreNotReported()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }

                                    private enum E
                                    {
                                        #region IDisposable
                                        A,
                                        #endregion

                                        #region Values
                                        B,
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the top-level region blocks of a type around a nested enum with a region of
    /// its own, and leaves the enum and its region in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixReordersTheBlocksOfTheTypeAroundANestedEnumWithARegion()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    private enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private int _a;

                                     #endregion

                                     private enum E
                                     {
                                         #region Values
                                         A,
                                         #endregion
                                     }

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the blocks of the type when the reported block starts with a nested class,
    /// and leaves the nested class untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixReordersTheTypeWhenTheReportedBlockStartsWithANestedClass()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private class Other
                                    {
                                    }

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private class Other
                                     {
                                     }

                                     private int _a;

                                     #endregion

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the blocks of the type when the reported block starts with a nested record
    /// that carries an attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    public async Task CodeFixReordersTheTypeWhenTheReportedBlockStartsWithAnAttributedNestedRecord()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    [Serializable]
                                    private record R(int A);

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     [Serializable]
                                     private record R(int A);

                                     private int _a;

                                     #endregion

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the blocks of the type when the reported block starts with a nested class
    /// whose own regions are in order, and leaves the regions of the nested class in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-FIX-005")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixReordersTheTypeWhenTheReportedBlockStartsWithANestedClassWhoseRegionsAreInOrder()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private class Other : IDisposable
                                    {
                                        #region Fields

                                        private int _b;

                                        #endregion

                                        #region IDisposable

                                        public void Dispose()
                                        {
                                        }

                                        #endregion
                                    }

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private class Other : IDisposable
                                     {
                                         #region Fields

                                         private int _b;

                                         #endregion

                                         #region IDisposable

                                         public void Dispose()
                                         {
                                         }

                                         #endregion
                                     }

                                     private int _a;

                                     #endregion

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix for the diagnostic of the outer type reorders the blocks of the outer type only, when
    /// the reported block starts with a nested class whose own regions are out of order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixForTheOuterTypeLeavesTheRegionsOfANestedClassAtTheStartOfTheReportedBlockInPlace()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private class Other : IDisposable
                                    {
                                        #region IDisposable

                                        public void Dispose()
                                        {
                                        }

                                        #endregion

                                        {|RH7309:#region Fields|}

                                        private int _b;

                                        #endregion
                                    }

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string expected = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region Fields

                                    private class Other : IDisposable
                                    {
                                        #region IDisposable

                                        public void Dispose()
                                        {
                                        }

                                        #endregion

                                        #region Fields

                                        private int _b;

                                        #endregion
                                    }

                                    private int _a;

                                    #endregion

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);

        var source = RemoveMarkup(testData);
        var fixedSource = await TryApplyCodeFixAtAsync(source,
                                                       RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                       root => root.DescendantNodes(descendIntoTrivia: true)
                                                                   .OfType<RegionDirectiveTriviaSyntax>()
                                                                   .ElementAt(1)
                                                                   .GetLocation());

        Assert.AreEqual(expected, fixedSource);
    }

    /// <summary>
    /// Verifies the union example of RH7309-002: a custom region after an interface region of a union is reported on
    /// its region directive, and the fix orders the regions as in a struct
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-003")]
    [Requirement("RH7309-004")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task UnionCustomRegionAfterInterfaceRegionIsReportedAndReordered(string endOfLine)
    {
        const string testCode = """
                                using System;

                                public union U(int, string) : IComparable
                                {
                                    #region IComparable

                                    public int CompareTo(object o)
                                    {
                                        return 0;
                                    }

                                    #endregion // IComparable

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public union U(int, string) : IComparable
                                 {
                                     #region Methods

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region IComparable

                                     public int CompareTo(object o)
                                     {
                                         return 0;
                                     }

                                     #endregion // IComparable
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine), test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests
}