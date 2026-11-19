using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer"/>
/// </summary>
[TestClass]
public class RH3210GetOnlyIndexersShouldUseAnExpressionBodyFormatterTests : FormatterTestsBase<RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that formatting converts every reported get-only indexer, so its diagnostic is cleared, and stays stable
    /// on a second pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3210-001")]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private readonly int[] _values = new int[3];

                                 public int this[int index] {|RH3210:{ get => _values[index]; }|}

                                 public int this[long index]
                                 {|RH3210:{
                                     get
                                     {
                                         return _values[index];
                                     }
                                 }|}

                                 public int this[string key] {|RH3210:{ get { throw new NotSupportedException(key); } }|}
                             }
                             """;

        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     private readonly int[] _values = new int[3];

                                     public int this[int index] => _values[index];

                                     public int this[long index] => _values[index];

                                     public int this[string key] => throw new NotSupportedException(key);
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the formatter keeps the accessor list of an indexer it keeps for a comment or a directive, which the
    /// analyzer reports
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    [Requirement("SHARED-GETONLY-007")]
    public async Task VerifyFormatterKeepsIndexersWithCommentsAndDirectives()
    {
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int this[int index] // note
                                 {|RH3210:{
                                     get => index;
                                 }|}

                                 public int this[long index]
                                 {|RH3210:{
                                     get => _value;
                             #if WRITABLE
                                     set => _value = value;
                             #endif
                                 }|}
                             }
                             """;

        await VerifyFormatterKeepsReportedShape(input);
    }

    /// <summary>
    /// Verifies that indexers whose shape the formatter keeps are neither reported nor changed by formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task VerifyFormatterKeepsUnreportedIndexers()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private readonly int[] _values = new int[3];

                                 public int this[int index]
                                 {
                                     get => _values[index];
                                     set => _values[index] = value;
                                 }

                                 public int this[long index]
                                 {
                                     [Obsolete]
                                     get => _values[index];
                                 }

                                 public int this[byte index] => _values[index];
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}