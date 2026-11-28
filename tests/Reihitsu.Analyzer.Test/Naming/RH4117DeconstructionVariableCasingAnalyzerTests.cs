using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4117DeconstructionVariableCasingAnalyzer"/> and <see cref="RH4117DeconstructionVariableCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4117DeconstructionVariableCasingAnalyzerTests : BatchCodeFixTestsBase<RH4117DeconstructionVariableCasingAnalyzer, RH4117DeconstructionVariableCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for deconstruction variables that are not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    [Requirement("RH4117-002")]
    public async Task VerifyDiagnosticsForDeconstructionVariableAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            var ({|#0:ResultCount|}, retryCount) = (42, 2);

                                            return ResultCount + retryCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             var (resultCount, retryCount) = (42, 2);

                                             return resultCount + retryCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies multiple deconstruction variables can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    public async Task VerifyDiagnosticsForMultipleDeconstructionVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            var ({|#0:ResultCount|}, {|#1:RetryCount|}) = (42, 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies diagnostics are reported for variables in nested deconstructions and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    [Requirement("RH4117-002")]
    public async Task VerifyDiagnosticForNestedDeconstructionVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            var (outerValue, ({|#0:ResultCount|}, innerValue)) = (1, (2, 3));

                                            return outerValue + ResultCount + innerValue;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             var (outerValue, (resultCount, innerValue)) = (1, (2, 3));

                                             return outerValue + resultCount + innerValue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for camelCase deconstruction variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    public async Task VerifyNoDiagnosticsForCamelCaseDeconstructionVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            var (resultCount, retryCount) = (42, 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the variables of a nested var deconstruction and of a deconstructing foreach statement are reported once each, while camelCase names are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    public async Task VariablesOfVarDeconstructionsAreCheckedInAssignmentsAndForeachStatements()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M((int, (int, int)) value, List<(int, int)> pairs)
                                    {
                                        var (count, ({|RH4117:Total|}, {|RH4117:item_count|})) = value;

                                        foreach (var ({|RH4117:Key|}, entry) in pairs)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames deconstruction variables and every reference to them to the derived camelCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-002")]
    public async Task FixRenamesDeconstructionVariablesAndTheirReferencesToCamelCase()
    {
        const string testCode = """
                                class C
                                {
                                    int M((int, (int, int)) value)
                                    {
                                        var ({|RH4117:First|}, ({|RH4117:Second_Value|}, third)) = value;

                                        return First + Second_Value + third;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M((int, (int, int)) value)
                                     {
                                         var (first, (secondValue, third)) = value;

                                         return first + secondValue + third;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a deconstruction variable keeps its diagnostic but gets no fix when its new name would hide a field that an
    /// unqualified reference in the method binds to today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameWouldHideAFieldUsedUnqualified()
    {
        const string testCode = """
                                public class Loader
                                {
                                    private int total;

                                    public int Load()
                                    {
                                        var ({|RH4117:Total|}, retry) = (42, 2);

                                        total = Total + retry;

                                        return total;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that the fix writes a leading acronym of the variable name in lowercase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [Requirement("RH4117-002")]
    public async Task FixWritesALeadingAcronymInLowercase()
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        var ({|RH4117:HTTPClient|}, {|RH4117:IOStream|}) = (1, 2);

                                        return HTTPClient + IOStream;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M()
                                     {
                                         var (httpClient, ioStream) = (1, 2);

                                         return httpClient + ioStream;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that no fix is offered for a variable whose derived name is a reserved keyword, while the other variable of
    /// the deconstruction is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-013")]
    [Requirement("RH4117-002")]
    public async Task NoFixIsOfferedWhenTheDerivedNameIsAReservedKeyword()
    {
        const string testCode = """
                                class C
                                {
                                    int M()
                                    {
                                        var ({|RH4117:Object|}, {|RH4117:Total|}) = (1, 2);

                                        return Object + Total;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M()
                                     {
                                         var ({|RH4117:Object|}, total) = (1, 2);

                                         return Object + total;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostic whose fix is withheld stays and is marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                            });
    }

    /// <summary>
    /// Verifies every variable of a parenthesized designation and every variable declared inside a tuple on the left of a
    /// deconstruction or of a foreach statement is reported, while an existing variable the tuple assigns is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    public async Task VariablesDeclaredInsideATupleOfADeconstructionAreReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M((int, (int, int)) value, (int, int) pair, List<(int, int)> pairs, int existing)
                                    {
                                        var (count, ({|RH4117:Total|}, {|RH4117:item_count|})) = value;
                                        (var {|RH4117:First|}, int second) = pair;
                                        (int {|RH4117:Third|}, var ({|RH4117:Fourth|}, fifth)) = value;
                                        (existing, var {|RH4117:Sixth|}) = pair;

                                        foreach (var ({|RH4117:Key|}, entry) in pairs)
                                        {
                                        }

                                        foreach ((var {|RH4117:Name|}, var size) in pairs)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames the variables declared inside a tuple on the left of a deconstruction and of a foreach
    /// statement, together with their references
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    [Requirement("RH4117-002")]
    public async Task FixRenamesVariablesDeclaredInsideATupleOfADeconstruction()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                class C
                                {
                                    int M((int, int) pair, List<(int, int)> pairs)
                                    {
                                        (var {|RH4117:First|}, var {|RH4117:Second|}) = pair;
                                        (int {|RH4117:Third|}, var ({|RH4117:Fourth|}, {|RH4117:Fifth|})) = (1, pair);

                                        foreach ((var {|RH4117:Key|}, var {|RH4117:Value|}) in pairs)
                                        {
                                        }

                                        return First + Second + Third + Fourth + Fifth;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;

                                 class C
                                 {
                                     int M((int, int) pair, List<(int, int)> pairs)
                                     {
                                         (var first, var second) = pair;
                                         (int third, var (fourth, fifth)) = (1, pair);

                                         foreach ((var key, var value) in pairs)
                                         {
                                         }

                                         return first + second + third + fourth + fifth;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix renames the variables of a var pattern and of a positional subpattern, together with their references
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-002")]
    [Requirement("RH4117-003")]
    public async Task FixRenamesTheVariablesOfADeconstructionPattern()
    {
        const string testCode = """
                                class C
                                {
                                    int M((int, int) pair)
                                    {
                                        if (pair is var ({|RH4117:Left|}, {|RH4117:Right|}))
                                        {
                                            return Left + Right;
                                        }

                                        return pair is (var {|RH4117:Head|}, _) ? Head : 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M((int, int) pair)
                                     {
                                         if (pair is var (left, right))
                                         {
                                             return left + right;
                                         }

                                         return pair is (var head, _) ? head : 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix renames the variables of a var pattern in an is expression and in a switch statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-002")]
    [Requirement("RH4117-003")]
    public async Task FixRenamesTheVariablesOfAVarPatternInAnIsExpressionAndASwitchStatement()
    {
        const string testCode = """
                                class C
                                {
                                    int M((int, int) pair)
                                    {
                                        if (pair is var ({|RH4117:First|}, {|RH4117:Second|}))
                                        {
                                            return First + Second;
                                        }

                                        switch (pair)
                                        {
                                            case var ({|RH4117:Left|}, {|RH4117:Right|}):
                                                return Left + Right;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M((int, int) pair)
                                     {
                                         if (pair is var (first, second))
                                         {
                                             return first + second;
                                         }

                                         switch (pair)
                                         {
                                             case var (left, right):
                                                 return left + right;
                                         }

                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix renames every variable that receives a whole element of a positional pattern, however its
    /// subpattern is spelled, and leaves a property pattern variable of an element alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-002")]
    [Requirement("RH4117-003")]
    public async Task FixRenamesEveryVariableThatReceivesAWholeElementOfAPositionalPattern()
    {
        const string testCode = """
                                class C
                                {
                                    int M((object, object) pair)
                                    {
                                        if (pair is (var {|RH4117:Third|}, int {|RH4117:Fourth|}))
                                        {
                                            return (int)Third + Fourth;
                                        }

                                        if (pair is (int {|RH4117:Number|} and > 0, string { Length: > 0 } {|RH4117:Text|}))
                                        {
                                            return Number + Text.Length;
                                        }

                                        if (pair is ({ } {|RH4117:Any|}, string { Length: var Size }))
                                        {
                                            return Any.GetHashCode() + Size;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     int M((object, object) pair)
                                     {
                                         if (pair is (var third, int fourth))
                                         {
                                             return (int)third + fourth;
                                         }

                                         if (pair is (int number and > 0, string { Length: > 0 } text))
                                         {
                                             return number + text.Length;
                                         }

                                         if (pair is ({ } any, string { Length: var Size }))
                                         {
                                             return any.GetHashCode() + Size;
                                         }

                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the variables of var patterns and of positional subpatterns are reported in is expressions, nested positional
    /// patterns, switch statements, and switch expressions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-003")]
    public async Task VariablesOfADeconstructionPatternAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    int M((int, int) pair, (object, (int, int)) nested, (object, object) objects)
                                    {
                                        if (pair is var ({|RH4117:First|}, {|RH4117:Second|}))
                                        {
                                            return First + Second;
                                        }

                                        if (nested is (int {|RH4117:Count|}, (var {|RH4117:Inner|}, var outer)))
                                        {
                                            return Count + Inner + outer;
                                        }

                                        if (objects is (int {|RH4117:Number|} and > 0, string { Length: > 0 } {|RH4117:Text|}))
                                        {
                                            return Number + Text.Length;
                                        }

                                        if (objects is ({ } {|RH4117:Any|}, (int {|RH4117:Wrapped|})))
                                        {
                                            return Any.GetHashCode() + Wrapped;
                                        }

                                        switch (pair)
                                        {
                                            case var ({|RH4117:Left|}, {|RH4117:Right|}):
                                                return Left + Right;
                                        }

                                        return 0;
                                    }

                                    int N((int, int) pair) => pair switch
                                    {
                                        (var {|RH4117:Head|}, _) => Head,
                                    };
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies pattern variables that receive no whole element of a positional pattern are not reported, next to one that does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-003")]
    public async Task PatternVariablesThatAreNoDeconstructionVariablesAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(object input, (int, int) pair, (string, int) named, int[] values)
                                    {
                                        if (pair is (1, 2) Both)
                                        {
                                        }

                                        if (named is ({ Length: var Size }, _))
                                        {
                                        }

                                        if (named is (string { Length: var Chars } {|RH4117:Whole|}, _))
                                        {
                                        }

                                        if (values is [var Head, .. var Rest])
                                        {
                                        }

                                        if (input is int Value)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the deconstruction variables of nested tuples, nested designations, typed, named, nested, and wrapped
    /// positional subpatterns, positional patterns inside property and list patterns, and switch statements and switch
    /// expressions are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    [Requirement("RH4117-003")]
    public async Task DeconstructionVariablesInNestedAndSwitchPositionsAreReported()
    {
        const string testCode = """
                                using System.Collections.Generic;

                                record Point(int X, int Y);

                                class Holder
                                {
                                    public (int, int) Pair { get; set; }
                                }

                                class C
                                {
                                    int M((int, int) pair, (object, object) objects, ((int, int), int) nested, (int[], int) withList, Point point, Holder holder, (int, int)[] pairs, int existing)
                                    {
                                        ((var {|RH4117:First|}, var {|RH4117:Second|}), var {|RH4117:Third|}) = nested;
                                        (existing, var {|RH4117:Fourth|}) = pair;

                                        foreach ((var {|RH4117:Name|}, (var {|RH4117:Low|}, var {|RH4117:High|})) in new List<(int, (int, int))>())
                                        {
                                        }

                                        if (nested is var (({|RH4117:Left|}, {|RH4117:Right|}), {|RH4117:Last|}))
                                        {
                                        }

                                        if (nested is ((var {|RH4117:Inner|}, var {|RH4117:Outer|}), var {|RH4117:Tail|}))
                                        {
                                        }

                                        if (point is Point(var {|RH4117:PointX|}, var {|RH4117:PointY|}))
                                        {
                                        }

                                        if (holder is { Pair: (var {|RH4117:Key|}, _) })
                                        {
                                        }

                                        if (pairs is [(var {|RH4117:Head|}, _), ..])
                                        {
                                        }

                                        if (objects is (Item1: var {|RH4117:Named|}, _))
                                        {
                                        }

                                        if (objects is (> 0 and int {|RH4117:Positive|}, _))
                                        {
                                        }

                                        if (objects is ((var {|RH4117:Wrapped|}), (int {|RH4117:Typed|})))
                                        {
                                        }

                                        if (withList is ([_, _] {|RH4117:Items|}, _))
                                        {
                                        }

                                        if (nested is ((1, 2) {|RH4117:Matched|}, _))
                                        {
                                        }

                                        switch (objects)
                                        {
                                            case (var {|RH4117:Start|}, var {|RH4117:End|}):
                                                break;
                                        }

                                        return pair switch
                                        {
                                            var ({|RH4117:Lower|}, {|RH4117:Upper|}) => Lower + Upper,
                                        };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies pattern variables that receive no whole element of a positional pattern are not reported: a parenthesized
    /// top-level pattern, a list pattern element and slice, also inside a positional pattern, a property pattern variable,
    /// a whole-pattern variable, a var pattern, and an out variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4117-001")]
    [Requirement("RH4117-003")]
    public async Task PatternVariablesThatReceiveNoWholeElementAreNotReported()
    {
        const string testCode = """
                                class Holder
                                {
                                    public (int, int) Pair { get; set; }
                                }

                                class C
                                {
                                    void M(object input, (object, object) objects, (int[], int) withList, int[] values, Holder holder)
                                    {
                                        if (input is (var Paren))
                                        {
                                        }

                                        if (values is [var First, .. var Rest])
                                        {
                                        }

                                        if (withList is ([var Element, ..], _))
                                        {
                                        }

                                        if (holder is { Pair: var Whole })
                                        {
                                        }

                                        if (objects is (var item, _) Outer)
                                        {
                                        }

                                        if (input is var Value)
                                        {
                                        }

                                        int.TryParse("1", out var Parsed);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a pattern variable keeps its diagnostic but gets no fix when its new name would collide with a local of
    /// the same scope
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task NoFixIsOfferedWhenTheNewNameOfAPatternVariableWouldCollide()
    {
        const string testCode = """
                                class C
                                {
                                    int M((int, int) pair)
                                    {
                                        var left = 1;

                                        if (pair is var ({|RH4117:Left|}, right))
                                        {
                                            return Left + right + left;
                                        }

                                        return left;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies the fix keeps the name of an anonymous object member whose whole expression is the renamed variable, while a
    /// member named after a variable that is not renamed is left alone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    [Requirement("RH4117-002")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterTheVariable()
    {
        const string testCode = """
                                class C
                                {
                                    string M((int, int) pair)
                                    {
                                        var ({|RH4117:First|}, second) = pair;

                                        return new { First, second }.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     string M((int, int) pair)
                                     {
                                         var (first, second) = pair;

                                         return new { First = first, second }.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the fix keeps the name of an anonymous object member named after a pattern variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberNamedAfterAPatternVariable()
    {
        const string testCode = """
                                class C
                                {
                                    object M((int, int) pair)
                                    {
                                        return pair is (var {|RH4117:Head|}, _) ? new { Head } : null;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     object M((int, int) pair)
                                     {
                                         return pair is (var head, _) ? new { Head = head } : null;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            var (outerValue, ({|#0:ResultCount|}, {|#1:RetryCount|})) = (1, (2, 3));

                                            return outerValue + ResultCount + RetryCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             var (outerValue, (resultCount, retryCount)) = (1, (2, 3));

                                             return outerValue + resultCount + retryCount;
                                         }
                                     }
                                 }
                                 """;

        // Verifies multiple variables in a nested deconstruction are renamed together by Fix All in one iteration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}