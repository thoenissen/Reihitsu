Role: spec author
Commit: 25e2f73
Verdict: spec written

Revision after the skeptic's `03-skeptic.md`. `01` and `02` stay as they were. This file lists the delta, the updated test mapping, and the **complete, final open-questions list** for every spec path, merged from 01, 02, and this round.

## How the findings were re-derived

I re-ran every finding I acted on with `scripts/apply-fix.sh <rule> <fixtures>` (LF and CRLF) against the unchanged code:

- **A.1 (nested pairs):**
  - `(((int)o))` becomes `((int)o)`;
  - `(((int)o)).ToString()` becomes `((int)o).ToString()`;
  - `((() => a))` becomes `(() => a)`.
- **A.2 (positions):**
  - `[Obsolete(("x"))]`, `x => (x)`, `a switch { _ => (a) }`, and `(typeof(int)).Name` are not reported;
  - `void M(int a = (1))` is reported and fixed.
- **A.3:** `1 == 1 == b` becomes `b == 1 == 1`.
- **A.4:** a `///<summary>` comment and `[Obsolete] public  C( int value ) => …` are relaid out as a whole.
- **B.8:** a `///` comment inside a method body counts as a documentation line, so the empty `//` above it is reported and removed.
- **C.9:** `case var x when x == ""` is not reported, while a switch-expression arm `when y == ""` is fixed.
- **C.10:** a trailing `int _a; //` between `//` lines is not reported.
- **D:**
  - the RH3004 cast operand, `nameof(Nullable<int>.HasValue)`, and `Nullable<int>.Equals(1, 2)` give `invalid-result`;
  - `var a = delegate () { };` becomes `delegate  { }`;
  - `return/**/a;` becomes `returna;`.

Not re-run by me: the compile errors that need `csc`. The output texts above are mine; the error codes are the skeptic's:

- CS0019 for `b == 1 == 1`;
- CS8917 for `var a = delegate  { };`;
- CS9058 under C# 11 for `using N = int?;`. I did not run the alias case at all.

A.5 and A.6 are wording findings and needed no fixture. B.7 is the RH3101 Q1 case already reproduced in 01.

All examples of the changed specs were re-run through the extractor from 01: 44 cases, all matching. The one exception is RH3201-003's unterminated comment, which does not parse in the runner; it is covered by `UnterminatedMultiLineCommentAtEndOfFileIsNotReported`. `SpecStructureTests` and `SpecConsistencyTests` pass (26/26). The coverage and retired-tag checks fail until the tests are tagged, as before.

## Changed requirements

All are in this PR's own new specs, so they are changed rather than retired. The retired RH3202 IDs are untouched.

- **RH3002-001** — narrowed positions:
  - "variable or member initializer" becomes "the value after `=` in a variable, member, or parameter declaration" (the example gains `int d = (1)`);
  - "an expression body (`=>`)" becomes "the expression body of a member or a local function";
  - "an argument" becomes "an argument of an argument list".
- **RH3002-002** — the receiver content list gains "a pair of parentheses that is not reported itself" (the example gains `(((object)s)).ToString()`). The "any other content MUST NOT be reported" clause is reduced to binary, conditional, cast, and `switch` expressions.
- **RH3002-006** — rewritten: of nested pairs, one stays. Around a cast, a lambda, an anonymous method, or a `switch` expression, the innermost pair stays; where the position needs a pair, the outermost pair stays. The new example shows both sides.
- **RH3003-002** — "a `case` label" becomes "the value or pattern of a `case` label".
- **RH3004-003** — now applies only to an anonymous method that is not the operand of a cast.
- **RH3005-004** — now applies only to a comparison whose operands are not themselves comparisons.
- **RH3005-005** — no longer claims a check of the swapped comparison. The fix is withheld for `dynamic` and for a comparison bound to a user-defined operator.
- **RH3101-004** — the example uses a non-virtual `M()` (RH3101-001's example keeps the virtual one: the report is fine).
- **RH3103-001, RH3103-004** — "any type position" becomes "where it stands as a type in a declaration, in `typeof`, as an array element type, or as a type argument". The fix is stated only for those positions.
- **RH3106-001** — narrowed to an anonymous method converted to the delegate type of the variable it initializes.
- **RH3201-002** — narrowed:
  - the empty `//` must stand alone on its line;
  - only a `///` comment that documents a declaration does not count as a `//` line.

  `documentation/rules/RH3201.md` already says "a `///` documentation comment", which stays consistent; the page is unchanged.
- **RH3201-005** — the second bullet now requires whitespace or the end of the line after the comment.
- **SHARED-BODY-001** — now references FMT-STRUCT-007 instead of restating its conditions. The examples are unchanged.
- **SHARED-BODY-003** — retitled "The fix converts the body as the formatter does":
  - it states only that `=> expression ;` is replaced by exactly the block FMT-STRUCT-004/-005/-006 produce, laid out as the formatter lays it out;
  - the sentence about relaying out the rest of the member is gone, and so is the reference to FMT-PIPE-010 (removed from `Uses`);
  - the RH3203 example no longer has a constructor initializer or a base class, so nothing outside the body changes in it.
- **SHARED-EMPTY-002** — "keep the trivia in front of `(` and after `)`" becomes "a comment in front of `(` MUST stay". The RH3107 example gains `Serializable /* a */()`, which becomes `Serializable /* a */`.

Not acted on: the expression-tree point (per the lead), and the RH3101 rule page's virtual-member example, which matches today's fix. That page is a follow-up candidate if RH3101 Q1 is decided as a bug.

## Test mapping, where requirements changed

The rest of the mapping in 01/02 stands.

- **RH3002-002:** `NestedParenthesesAroundCastInMemberAccessChainReportTheOuterPair` and `NestedParenthesesAroundSwitchExpressionInMemberAccessChainReportTheOuterPair` (the reported outer pair around a pair that stays).
- **RH3002-006:**
  - outermost side: `NestedParenthesesAroundBinaryExpressionInMemberAccessChainKeepTheNecessaryPair` and the other `…KeepTheNecessaryPair` tests;
  - innermost side: the two tests named above for RH3002-002. The initializer form `(((int)o))` and the lambda form `((() => a))` have no test yet.
- **RH3002-001:** the parameter default `int d = (1)` has no test yet. Do not tag a test of an attribute argument, a lambda body, or a switch arm (RH3002 Q1).
- **RH3003-002:** do not tag a `when`-clause test (RH3003 Q1).
- **RH3005-005:** the existing `AsymmetricUserDefinedOperator*`, `UserDefinedEqualityOperatorIsNotFixed`, and `DynamicOperandIsNotFixed` still fit.
- **RH3101-004:** `BaseQualifierOnInheritedMethodIsReportedAndFixed` uses a non-virtual base method and can be tagged. Any test whose fixed member is virtual in an unsealed type pins RH3101 Q1 and stays untagged.
- **RH3106-001:** `VerifyDiagnosticAndCodeFix` fits (its anonymous methods initialize `Action` variables). Pin `var a = delegate () { };` and the overload case only as untagged characterization tests.
- **RH3201-002:** `EmptySingleLineCommentInsideCommentBlockIsNotReported` and `EmptySingleLineCommentBeforeFourSlashCommentIsNotReported` still fit. The trailing `//` (Q6) and the body `///` (Q5) cases stay untagged.
- **SHARED-BODY-003:** tag only tests whose input has nothing to relay out outside `=> … ;` (no attribute on the declaration's line, no documentation comment to reflow, no constructor initializer on the signature line, and no irregular signature spacing). Pin the member-wide relayout only as an untagged characterization test (shared/expression-body-conversion.md Q1). Of the tests named in 02, I found none with such input in `RH3202…`, `RH3203…`, or `RH3205…`–`RH3208…AnalyzerTests` (by grep, not by reading every test).
- **SHARED-EMPTY-002:** the new example (`Serializable /* a */()`) has no test yet.

## Open questions to file — complete list

There is one `SDD open questions: <spec path>` issue per path, all new, each a sub-issue of thoenissen/Reihitsu#912. "P1" for a fix means SHARED-FIX-003. Unless stated otherwise, each case was reproduced with `scripts/apply-fix.sh` on unchanged code, LF and CRLF alike.

### sdd/specs/analyzer/RH3002.md

**Q1 — Decided by principle P11 (#912).** Each of these redundant pairs is not reported:

```csharp
using System;

class C
{
    [Obsolete(("x"))]
    int M(int a, bool c)
    {
        if ((c)) { }
        while ((c)) { }
        Func<int, int> f = x => (x);
        var n = (typeof(int)).Name;
        var s = a switch { _ => (a) };
        return a + (a);
    }
}
```

The positions are an `if`/`while` condition, a lambda body, a switch-expression arm, an attribute argument, and a binary operand. Also not reported is a receiver whose content is not in RH3002-002's list, such as `typeof`. All are unnecessary parentheses, the construct RH3002 names. Touches RH3002-001, RH3002-002.

### sdd/specs/analyzer/RH3003.md

**Q1 — Decided by principle P11 (#912).** Input:

```csharp
class C
{
    int M(string s)
    {
        switch (s)
        {
            case var x when x == "":
                return 1;
        }

        return s switch { var y when y == "" => 2, _ => 3 };
    }
}
```

Real output: the `""` in the `case … when` clause is not reported, while the one in the switch-expression arm's `when` clause is reported and fixed to `string.Empty`. A `when` clause needs no constant, so the exemption for the whole `case` label is wider than RH3003-002's reason. Touches RH3003-002.

### sdd/specs/analyzer/RH3004.md

**Q1.** Input: `Func<int, int> a = static delegate (int x) { return x; };` Real output: `Func<int, int> a = (int x) => x;`. The fix drops the user-written `static` modifier. Competing readings: keep it (`static (int x) => x`), or withhold the fix. P1 does not apply, because neither compilation nor run time changes. Touches RH3004-003.

**Q2 — Decided by principle P10 (#912).** Input: `Func<int, int> a = delegate (int x)` followed by a block on its own lines (`{`, `var y = x + 1;`, `return y;`, `};`). Real output: `Func<int, int> a = (int x) => {`, followed by the unchanged statement lines. The line break and indentation in front of `{` are dropped. Touches RH3004-003.

**Q3 — Decided by principle P1 (#912), compile error.** Input: `var f = (Func<int, int>)delegate (int x) { return x + 1; };` Real output: `var f = (Func<int, int>)(int x) => x + 1;`, which does not parse (`invalid-result`), because a lambda cannot be a cast operand without parentheses. Touches RH3004-003.

### sdd/specs/analyzer/RH3005.md

**Q1 — Decided by principle P1 (#912), compile error.** Input (with `bool b`): `if (1 == 1 == b) { }`. Real output: reported, and the fix writes `if (b == 1 == 1) { }`. Per the skeptic's `csc` run, that gives `error CS0019: Operator '==' cannot be applied to operands of type 'bool' and 'int'`. The swapped text is written without parentheses and bound speculatively, and a failed binding without candidates counts as built in. Touches RH3005-004, RH3005-005.

### sdd/specs/analyzer/RH3006.md

**Q1.** Input (CRLF line endings):

```csharp
class C
{
    bool M(bool a, bool b, bool c)
    {
        return a ||
            /* x */ b && c;
    }
}
```

Real output: `/* x */ (b && c);` as expected, but `apply-fix` reports `[line-ending-drift]`, with `CRLF=7, LF=1`. The LF arm is clean, and so are inputs without a line break before the operand. A fix must not introduce a line ending the document does not use. Touches RH3006-003.

### sdd/specs/analyzer/RH3101.md

**Q1 — Decided by principle P1 (#912), run time.** Input: `class B { public virtual int M() => 1; }`, `class D : B { public int Run() { return base.M(); } }`, `class E : D { public override int M() => 2; }`. Real output: reported, and the fix writes `return M();`. That turns the non-virtual call to `B.M` into a virtual call, so `new E().Run()` returns 2 instead of 1. This happens whenever the member is virtual and the type is not sealed; the override may live outside the compilation. Touches RH3101-001 (the report itself holds under SHARED-FIX-003) and RH3101-004.

**Q2 — Decided by principle P1 (#912), comment text.** Input: `return base /* a */ . /* b */ M();` (with `M` inherited, not virtual). Real output: `return M();`, with both comments deleted. Touches RH3101-004.

### sdd/specs/analyzer/RH3102.md

**Q1 — Decided by principle P10 (#912)** (P9 fits as well). Input: `a++; ;` on one line, followed by `a++;` on the next line. Real output: `        a++;         a++;` on one line — the removed empty statement took the line break after it. Touches RH3102-003.

### sdd/specs/analyzer/RH3103.md

**Q1 — fix part decided by principle P1 (#912), compile error.** The analyzer reports every generic name that binds to `Nullable<T>`, whatever its syntactic position, and the fix writes `T?` where `T?` cannot stand:

- `typeof(Nullable<>)` becomes `typeof(?)` (`invalid-result`). An unbound generic has no shorthand, so the report itself has no correction.
- `nameof(Nullable<int>.HasValue)` becomes `nameof(int?.HasValue)` (`invalid-result`).
- `bool M() => Nullable<int>.Equals(1, 2);` becomes `int?.Equals(1, 2)` (`invalid-result`).
- `using N = System.Nullable<int>;` becomes `using N = int?;`, which per the skeptic's `csc` run under C# 11 gives `CS9058: Feature 'using type alias' is not available in C# 11.0`. I did not re-run this case.

Competing readings for the first three: do not report outside type positions, or report without a fix. Touches RH3103-001, RH3103-004.

### sdd/specs/analyzer/RH3104.md

**Q1 — Decided by principle P1 (#912), comment text.** Inputs: `var a = new int(/* x */);` and `var a = new /* x */ int();`. Real output: `var a = default(int);` in both, with the comment deleted. Touches RH3104-004.

### sdd/specs/analyzer/RH3105.md

**Q1 — Decided by principle P1 (#912), comment text.** Input: `return this /* a */ . /* b */ _a;` Real output: `return _a;`, with both comments deleted. Touches RH3105-004.

### sdd/specs/analyzer/RH3106.md

**Q1 — fix part decided by principle P1 (#912), compile error.** Without a parameter list, an anonymous method converts to every delegate type, so removing `()` can break code whose conversion target is not fixed:

- `Run(delegate () { });`, with overloads `Run(Action)` and `Run(Action<int>)`, becomes `Run(delegate  { });`, which gives `error CS0121: The call is ambiguous` (confirmed with `csc` in 01).
- `var a = delegate () { }; a();` becomes `var a = delegate  { };`, which gives `error CS8917: The delegate type could not be inferred` (per the skeptic's `csc` run). Here the parentheses are needed, so the report itself is open too.

Competing readings: withhold the fix where the target is not fixed, or do not report such an anonymous method. Touches RH3106-001, SHARED-EMPTY-002.

### sdd/specs/analyzer/RH3201.md

**Q1 — Decided by principle P13 (#912).** Inputs:

- an empty `//` between a `/* First. */` line and a `// Second.` line;
- an empty `/* */` line between two `//` lines.

Real output: both are reported and removed. Under P13, a line holding only a `/* */` comment counts like a `//` line. Touches RH3201-002.

**Q2 — Decided by principle P9 (#912).** Input: `    /**/ int _a;`. Real output: ` int _a;`, with the line's indentation removed along with the comment. Touches RH3201-005.

**Q3 — Decided by principle P10 (#912).** Input: a whitespace-only `/*` … `*/` comment spanning three lines, directly after `{`. Real output: the comment's first and last lines go, but an empty line stays where the comment was, directly after `{`. Touches RH3201-005.

**Q4 — Decided by principle P1 (#912), compile error.** Input: `return/**/a;`. Real output: `returna;` — removing a comment that separates two tokens joins them into a new identifier. Touches RH3201-005.

**Q5 — Decided by principle P13 (#912).** Input, inside a method body:

```csharp
// First.
//
/// Second.
M();
```

Real output: the empty `//` is reported and removed. A `///` that documents nothing is an ordinary comment under P13, so the empty line sits inside a block of ordinary comments. The check reads the line's text (`///` but not `////`), not whether the comment documents anything. Touches RH3201-002.

**Q6 — Decided by principle P11 (#912).** Input:

```csharp
class C
{
    // First.
    int _a; //
    // Second.
    int _b;
}
```

Real output: the empty trailing `//` is not reported. The exemption checks only that the lines above and below start with `//`, not that the empty comment stands alone; a trailing empty comment is no paragraph break. Touches RH3201-002.

### sdd/specs/analyzer/RH3202.md

**Q1 — no principle marking** (maintainer's decision 4; P9 and P10 point in opposite directions). Input: `class C { private int _value;`, followed by two blank lines, then `public int GetValue() => _value; }`. Real output: the method gets its block body, and only one blank line is left above it. RH3202-005 says the fix keeps the blank lines around the method. Competing readings:

- (a) a bug against RH3202-005 / P9: keep both lines;
- (b) intended under P10: the lines are the formatted member's own leading trivia, and RH3202-005 is clarified to "keeps the separation".

The same holds for RH3203. Touches RH3202-005, SHARED-BODY-003.

### sdd/specs/shared/expression-body-conversion.md

**Q1 — no principle marking** (the lead's instruction; the same split as RH3202 Q1). Input (RH3203):

```csharp
using System;

class C
{
    private int _value;

    ///<summary>x</summary>
    [Obsolete] public  C( int value ) => _value = value;
}
```

Real output:

```csharp
using System;

class C
{
    private int _value;

    /// <summary>
    /// x
    /// </summary>
    [Obsolete]
    public C(int value)
    {
        _value = value;
    }
}
```

The diagnostic covers only `=> _value = value`, but the fix relays out the whole member: the documentation comment, the attribute's line, and the signature's spacing. A constructor initializer on the signature line moves to its own line as well (`: this(…)`, `: base(…)`). This holds for every SHARED-BODY rule, RH3202 included. Competing readings:

- (a) P9, fix scope: only `=> expression ;` becomes the block, and the rest keeps its layout (`[Obsolete] public  C( int value )` followed by the block);
- (b) P10, fix layout, the current behavior: the member is the code the fix changes and is laid out as a whole.

Touches SHARED-BODY-003.

### sdd/specs/shared/empty-parentheses.md

**Q1 — Decided by principle P1 (#912), comment text.** Inputs: `Action a = delegate ( /* none */ ) { };` (RH3106) and `[Obsolete(/* none */)]` (RH3107). Real output: `delegate  { }` and `[Obsolete]`, with the comment inside the list deleted. Touches SHARED-EMPTY-002.

**Q2 — Decided by principle P10 (#912).** Inputs: `Action a = delegate () { };` and `[Obsolete /* a */ ( ) /* b */]`. Real output: `delegate  { }` and `[Obsolete /* a */  /* b */]`, a double space left by the whitespace on both sides of the removed list. Touches SHARED-EMPTY-002.

## Decisions needed

_None._
