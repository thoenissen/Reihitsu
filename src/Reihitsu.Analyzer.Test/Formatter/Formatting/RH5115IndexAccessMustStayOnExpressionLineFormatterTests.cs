using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5115IndexAccessMustStayOnExpressionLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5115IndexAccessMustStayOnExpressionLineFormatterTests : FormatterTestsBase<RH5115IndexAccessMustStayOnExpressionLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Members whose initializer, expression body, or constructor initializer holds an index access below its expression
    /// </summary>
    private const string MemberContextsTestData = """
                                                  class C
                                                  {
                                                      private static readonly int[] Values = [1, 2];
                                                      private int _first = Values
                                                          {|RH5115:[|}0];
                                                      private int[] _items = [1];

                                                      public C(int[] arr)
                                                          : this(arr
                                                              {|RH5115:[|}0], 1)
                                                      {
                                                      }

                                                      public C(int a, int b)
                                                      {
                                                      }

                                                      public int Second { get; } = Values
                                                          {|RH5115:[|}1];

                                                      public int Third => _items
                                                          {|RH5115:[|}0];

                                                      public int this[int i] => i;

                                                      int Self()
                                                      {
                                                          return this
                                                              {|RH5115:[|}0];
                                                      }
                                                  }
                                                  """;

    /// <summary>
    /// The formatted <see cref="MemberContextsTestData"/>: every access is joined to its expression
    /// </summary>
    private const string MemberContextsResultData = """
                                                    class C
                                                    {
                                                        private static readonly int[] Values = [1, 2];
                                                        private int _first = Values[0];
                                                        private int[] _items = [1];

                                                        public C(int[] arr)
                                                            : this(arr[0], 1)
                                                        {
                                                        }

                                                        public C(int a, int b)
                                                        {
                                                        }

                                                        public int Second { get; } = Values[1];

                                                        public int Third => _items[0];

                                                        public int this[int i] => i;

                                                        int Self()
                                                        {
                                                            return this[0];
                                                        }
                                                    }
                                                    """;

    /// <summary>
    /// Statements whose expressions hold an index access below its expression in many enclosing contexts
    /// </summary>
    private const string ExpressionContextsTestData = """"
                                                      #nullable enable
                                                      using System;
                                                      using System.Collections.Generic;
                                                      using System.Linq;

                                                      class C
                                                      {
                                                          int M(int[] arr, int k, bool f, Dictionary<string, int[]> d)
                                                          {
                                                              var a = k switch
                                                                      {
                                                                          1 => arr
                                                                              {|RH5115:[|}0],
                                                                          _ => 0
                                                                      };
                                                              var b = from v in arr
                                                                          {|RH5115:[|}0..1] select v;
                                                              Func<int> g = () => arr
                                                                  {|RH5115:[|}0];
                                                              var h = $"{arr
                                                                  {|RH5115:[|}0]:D2}";
                                                              var i = $"""
                                                                       {arr
                                                                       {|RH5115:[|}0]}
                                                                       """;
                                                              ref int j = ref arr
                                                                  {|RH5115:[|}0];
                                                              var l = ((int[])arr)
                                                                  {|RH5115:[|}0];
                                                              var m = arr
                                                                  {|RH5115:[|}^1];
                                                              var n = arr
                                                                  {|RH5115:[|}1..];
                                                              var o = d["k"]
                                                                  {|RH5115:[|}0];
                                                              var p = new int[] { 1, 2 }
                                                                  {|RH5115:[|}0];
                                                              var q = (f ? arr : arr)
                                                                  {|RH5115:[|}0];

                                                              foreach (var r in d
                                                                           {|RH5115:[|}"k"])
                                                              {
                                                              }

                                                              (arr
                                                                  {|RH5115:[|}0], arr[1]) = (1, 2);

                                                              var s = new int[arr
                                                                  {|RH5115:[|}0]];
                                                              var t = """
                                                                      abc
                                                                      """
                                                                  {|RH5115:[|}0];

                                                              if (arr
                                                                  {|RH5115:[|}0] > 0)
                                                              {
                                                              }

                                                              return arr
                                                                  {|RH5115:[|}0];
                                                          }
                                                      }
                                                      """";

    /// <summary>
    /// The formatted <see cref="ExpressionContextsTestData"/>: every access is joined to its expression
    /// </summary>
    private const string ExpressionContextsResultData = """"
                                                        #nullable enable
                                                        using System;
                                                        using System.Collections.Generic;
                                                        using System.Linq;

                                                        class C
                                                        {
                                                            int M(int[] arr, int k, bool f, Dictionary<string, int[]> d)
                                                            {
                                                                var a = k switch
                                                                        {
                                                                            1 => arr[0],
                                                                            _ => 0
                                                                        };
                                                                var b = from v in arr[0..1] select v;
                                                                Func<int> g = () => arr[0];
                                                                var h = $"{arr[0]:D2}";
                                                                var i = $"""
                                                                         {arr[0]}
                                                                         """;
                                                                ref int j = ref arr[0];
                                                                var l = ((int[])arr)[0];
                                                                var m = arr[^1];
                                                                var n = arr[1..];
                                                                var o = d["k"][0];
                                                                var p = new int[] { 1, 2 }[0];
                                                                var q = (f ? arr : arr)[0];

                                                                foreach (var r in d["k"])
                                                                {
                                                                }

                                                                (arr[0], arr[1]) = (1, 2);

                                                                var s = new int[arr[0]];
                                                                var t = """
                                                                        abc
                                                                        """[0];

                                                                if (arr[0] > 0)
                                                                {
                                                                }

                                                                return arr[0];
                                                            }
                                                        }
                                                        """";

    /// <summary>
    /// Chains with links whose attached index accesses stand below the code in front of them
    /// </summary>
    private const string ChainContextsTestData = """
                                                 using System.Linq;

                                                 class C
                                                 {
                                                     void M(dynamic x, int[] arr, int[] items)
                                                     {
                                                         var a = x.B()
                                                             {|RH5115:[|}0]
                                                             {|RH5115:[|}1]
                                                             .C();
                                                         var b = x.B(arr
                                                             {|RH5115:[|}0])
                                                             {|RH5115:[|}1]
                                                             .C();
                                                         var f = items.Where(v => v > 0)
                                                                      .ToArray()
                                                                      {|RH5115:[|}0];
                                                         var g = x
                                                             {|RH5115:[|}0]
                                                             .B()
                                                             .C();
                                                     }
                                                 }
                                                 """;

    /// <summary>
    /// The formatted <see cref="ChainContextsTestData"/>: every access is joined and every chain laid out
    /// </summary>
    private const string ChainContextsResultData = """
                                                   using System.Linq;

                                                   class C
                                                   {
                                                       void M(dynamic x, int[] arr, int[] items)
                                                       {
                                                           var a = x.B()[0][1]
                                                                    .C();
                                                           var b = x.B(arr[0])[1]
                                                                    .C();
                                                           var f = items.Where(v => v > 0)
                                                                        .ToArray()[0];
                                                           var g = x[0].B()
                                                                       .C();
                                                       }
                                                   }
                                                   """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that index accesses below their expression in member initializers, an expression body, and a constructor
    /// initializer are reported, that the formatter joins them, and that the formatted code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterJoinsReportedAccessesInMemberContexts()
    {
        await VerifyFormatter(MemberContextsTestData, MemberContextsResultData);
    }

    /// <summary>
    /// Verifies that index accesses below their expression in many enclosing expression contexts are reported, that the
    /// formatter joins them, and that the formatted code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterJoinsReportedAccessesInExpressionContexts()
    {
        await VerifyFormatter(ExpressionContextsTestData, ExpressionContextsResultData);
    }

    /// <summary>
    /// Verifies that index accesses of chains with links are reported, that the formatter joins them and lays out each chain,
    /// and that the formatted code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterJoinsReportedAccessesInChainsWithLinks()
    {
        await VerifyFormatter(ChainContextsTestData, ChainContextsResultData);
    }

    /// <summary>
    /// Verifies that an index access below its expression in a <c>fixed</c> statement's header is reported, that the formatter
    /// joins it, and that the formatted code is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterJoinsAReportedAccessInAFixedStatementHeader()
    {
        const string input = """
                             class C
                             {
                                 unsafe void M(int[] arr)
                                 {
                                     fixed (int* p = &arr
                                                 {|RH5115:[|}0])
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 class C
                                 {
                                     unsafe void M(int[] arr)
                                     {
                                         fixed (int* p = &arr[0])
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input, fixedData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that the formatted code of every context stays unchanged and unreported when it is formatted again
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5115-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormattedContextsStayUnchangedAndUnreported()
    {
        await VerifyFormatter(MemberContextsResultData);
        await VerifyFormatter(ExpressionContextsResultData);
        await VerifyFormatter(ChainContextsResultData);
    }

    #endregion // Tests
}