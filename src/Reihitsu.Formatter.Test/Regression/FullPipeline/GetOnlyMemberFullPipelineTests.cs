using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class GetOnlyMemberFullPipelineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that get-only properties and indexers become expression-bodied members while members that need
    /// their accessor list keep it
    /// </summary>
    [TestMethod]
    public void GetOnlyMembersBecomeExpressionBodiedMembers()
    {
        // Arrange
        const string input = """
                             using System;

                             public class Sample
                             {
                                 private readonly int[] _values = new int[3];
                                 private int _value;

                                 public int A
                                 {
                                     get
                                     {
                                         return _value;
                                     }
                                 }

                                 public int B { get => _value; }

                                 public int C { get { throw new InvalidOperationException(); } }

                                 public ref int D { get { return ref _value; } }

                                 public int this[int index] => _values[index];

                                 public int this[long index]
                                 {
                                     get
                                     {
                                         return _values[index];
                                     }
                                 }

                                 public int E
                                 {
                                     get => _value;
                                     set => _value = value;
                                 }

                                 public int F
                                 {
                                     [Obsolete]
                                     get => _value;
                                 }

                                 public int G
                                 {
                                     get
                                     {
                                         var copy = _value;

                                         return copy;
                                     }
                                 }

                                 public int H
                                 {
                                     get
                                     {
                                         // Explains the value
                                         return _value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                public class Sample
                                {
                                    private readonly int[] _values = new int[3];
                                    private int _value;

                                    public int A => _value;

                                    public int B => _value;

                                    public int C => throw new InvalidOperationException();

                                    public ref int D => ref _value;

                                    public int this[int index] => _values[index];

                                    public int this[long index] => _values[index];

                                    public int E
                                    {
                                        get => _value;
                                        set => _value = value;
                                    }

                                    public int F
                                    {
                                        [Obsolete]
                                        get => _value;
                                    }

                                    public int G
                                    {
                                        get
                                        {
                                            var copy = _value;

                                            return copy;
                                        }
                                    }

                                    public int H
                                    {
                                        get
                                        {
                                            // Explains the value
                                            return _value;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}