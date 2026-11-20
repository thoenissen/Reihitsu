using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer"/>
/// </summary>
[TestClass]
public class RH3209GetOnlyPropertiesShouldUseAnExpressionBodyFormatterTests : FormatterTestsBase<RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that formatting converts every reported get-only property, so its diagnostic is cleared, and stays stable
    /// on a second pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int _value;

                                 public int A {|RH3209:{ get => _value; }|}

                                 public int B
                                 {|RH3209:{
                                     get
                                     {
                                         return _value;
                                     }
                                 }|}

                                 public int D {|RH3209:{ get { throw new InvalidOperationException(); } }|}

                                 public int E {|RH3209:{ get => _value; }|} // a
                             }
                             """;

        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     private int _value;

                                     public int A => _value;

                                     public int B => _value;

                                     public int D => throw new InvalidOperationException();

                                     public int E => _value; // a
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the formatter keeps the accessor list of a property it keeps for a comment or a directive, which the
    /// analyzer reports
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    [Requirement("SHARED-GETONLY-007")]
    public async Task VerifyFormatterKeepsPropertiesWithCommentsAndDirectives()
    {
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int A // note
                                 {|RH3209:{
                                     get => _value;
                                 }|}

                                 public int B
                                 {|RH3209:{
                             #if DEBUG
                                     get => 1;
                             #else
                                     get => 2;
                             #endif
                                 }|}
                             }
                             """;

        await VerifyFormatterKeepsReportedShape(input);
    }

    /// <summary>
    /// Verifies that properties whose shape the formatter keeps are neither reported nor changed by formatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task VerifyFormatterKeepsUnreportedProperties()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 private int _value;

                                 public int A
                                 {
                                     get => _value;
                                     set => _value = value;
                                 }

                                 public int B { get; }

                                 public int D
                                 {
                                     [Obsolete]
                                     get => _value;
                                 }

                                 public int E
                                 {
                                     get
                                     {
                                         var value = _value;

                                         return value;
                                     }
                                 }

                                 public int F
                                 {
                                     get => field;
                                 } = 5;

                                 public int G => _value;
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}