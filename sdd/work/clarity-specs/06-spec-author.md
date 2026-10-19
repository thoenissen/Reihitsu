Role: spec author
Commit: 1201d26
Verdict: spec written

Revision after `05-skeptic.md`. `01`–`04` stay as they were. This file carries the delta, the test mapping for every requirement changed since `04`, and again the **complete, final open-questions list** for every spec path.

## How the findings were re-derived

I re-ran every finding below with `scripts/apply-fix.sh` against the unchanged code:

- **RH3002 (finding 1).**
  - `return ((_d))();` → `no-diagnostic`.
  - `return (((_d)))();` → only the outermost pair is fixed, giving `((_d))()`.
- **RH3002 (finding 7).** All of these are fixed: `int P { get => (_f); }`, `var t = ((a), 1);`, `new C((a))`, and `x[(a)]`.
- **RH3005 (finding 2).** The `1 < v` fixture gives `no-fix-offered`.
- **RH3004 (finding 6).** `Func<int> f = delegate () { return 1; } + h;` → `Func<int> f = () => 1 + h;`. The CS0019 compile error is the skeptic's `csc` result, not mine.
- **RH3201 (finding 8).** `return /**/a;` → `returna;`.
- **Findings 3, 4, 5, and the RH3106/RH3103 scopes of 6.** These follow from inputs that rounds 03/04 had already reproduced, set against the new wording.
- **Finding 9.** I confirmed the SHARED-FIX-003-tagged test `VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals` by grep.

The examples of every changed spec were re-run through the extractor: 37 cases, plus 7 for RH3005 after its last edit, all matching except RH3201-003's unterminated comment, which does not parse in the runner (covered by the existing test). `SpecStructureTests` and `SpecConsistencyTests` pass (26/26).

## Changed requirements

All are this PR's own new requirements.

- **RH3002-001.** The position list now reads:
  - "the expression body (`=>`) of a member, an accessor, or a local function";
  - "an argument of an invocation, an object creation, or an element access";
  - "an element of a tuple".

  The example gains `int Q { get => (_f); }` and `var t = ((a), 1);`. Attribute arguments now plainly fall outside the list (RH3002 Q1).
- **RH3002-006.** Rewritten to match the code:
  - the lead-in no longer claims that one pair always stays (`return ((b));` reports both);
  - first case: around a cast, a lambda, an anonymous method, or a `switch` expression, when the outermost pair stands in an RH3002-001 position, every pair but the innermost is reported;
  - second case: as the receiver of RH3002-002, the target of RH3002-005, or the operand of a throw expression, every pair but the outermost is reported;
  - RH3002-004 is no longer named, since `((_d))()` reports neither pair and `(((_d)))()` only the outermost. The example is unchanged.
- **RH3003-001.** Narrowed to a literal "that does not stand inside a `case` label", so the `when` clause of RH3003 Q1 is covered by neither requirement.
- **RH3004-003.** The scope now also excludes "the left operand of a binary operator" (RH3004 Q3).
- **RH3005-004.** Now also scoped to comparisons "for which RH3005-005 does not withhold the fix".
- **RH3005-005.** Adds the case "the comparison with its operands swapped and the operator mirrored would bind to an operator that a type declares itself", with the `1 < v` example (reported, no fix).
- **RH3103-001 and RH3103-004.**
  - "one type argument" becomes "one written type argument", which excludes `typeof(Nullable<>)`;
  - "as a type in a declaration" becomes "as the type of a field, a parameter, a local variable, or a method's return value", which excludes a using alias.
- **RH3106-001.** Narrowed to an anonymous method that is "the initializer of a variable whose declared type is a delegate type", which excludes `var`.
- **RH3201-001.** No longer covers an empty `//` whose line above and line below each start with `//`; it points to RH3201-002 for those.
- **RH3201-002.** Not reported: an empty `//` alone on its line between lines that start with a `//` comment that is not a `///` comment, or with `////`. Reported: when a neighbor is a `///` comment that documents a declaration. RH3201 Q5 (a `///` that documents nothing) and Q6 (a trailing `//`) are now covered by neither requirement; the spec is silent on them. `documentation/rules/RH3201.md` stays consistent and is unchanged.
- **SHARED-EMPTY-002.** Scoped to a list that "holds nothing but whitespace between `(` and `)`" (SHARED-EMPTY Q1 is excluded).

## Test mapping for the requirements changed since 04

- **RH3002-001:**
  - accessor form `int Q { get => (_f); }` — no test yet;
  - tuple element `var t = ((a), 1);` — no test yet;
  - existing tests: `UnnecessaryParenthesesIn{Return,Throw,Assignment,Argument,ExpressionBodiedMember}AreReportedAndFixed`;
  - the parameter-default form has no test yet;
  - do not tag attribute-argument, lambda-body, or switch-arm tests (RH3002 Q1).
- **RH3002-006:**
  - first case: `NestedParenthesesAroundCastInMemberAccessChainReportTheOuterPair` and `NestedParenthesesAroundSwitchExpressionInMemberAccessChainReportTheOuterPair` (the receiver whose content is a cast or `switch` — the outermost pair stands in a receiver position, which RH3002-002 also covers). The initializer form `((((int)o)))` and the lambda form `((() => a))` have no test yet;
  - second case: `NestedParenthesesAroundBinaryExpressionInMemberAccessChainKeepTheNecessaryPair`, `NestedParenthesesAroundRefConditionalAssignmentTargetKeepTheNecessaryPair`, `NestedParenthesesAroundConditionalInThrowExpressionKeepTheNecessaryPair`;
  - `((_d))()` and `(((_d)))()` are RH3002-004 behavior and stay out of RH3002-006's tags.
- **RH3003-001:** the existing `…IsReportedAndFixed` tests still fit. RH3003 Q1's `when` clause stays an untagged characterization test.
- **RH3004-003:** the existing conversion tests fit. The cast-operand and `+ h` cases stay untagged characterization tests.
- **RH3005-004:** `YodaConditionWith*IsReportedAndFixed`.
- **RH3005-005:** the existing `AsymmetricUserDefinedOperator*`, `UserDefinedEqualityOperatorIsNotFixed`, and `DynamicOperandIsNotFixed`; `AsymmetricUserDefinedOperatorWithUnrelatedMirrorIsNotFixed` binds the original comparison to a declared operator, so it does not cover the swapped-only case. The `1 < v` example needs a new test.
- **RH3103-001/-004:** unchanged mapping. `typeof(Nullable<>)`, `nameof(Nullable<int>.HasValue)`, `Nullable<int>.Equals(1, 2)`, and the using alias stay untagged characterization tests.
- **RH3106-001:** `VerifyDiagnosticAndCodeFix` fits (`Action action = delegate() { }`). `var a = delegate () { };` and the overload case stay untagged.
- **RH3201-001:** the `Empty…IsReportedAndFixed` tests.
- **RH3201-002:**
  - not reported: `EmptySingleLineCommentInsideCommentBlockIsNotReported`, `EmptySingleLineCommentBeforeFourSlashCommentIsNotReported`;
  - reported: the `///` neighbor that documents a declaration has no test yet;
  - Q5 and Q6 stay untagged.
- **SHARED-EMPTY-002:** the RH3106/RH3107 single tests fit (no comment inside the list). `( /* none */ )` stays untagged.

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

**Q3 — Decided by principle P1 (#912), compile error.** Input: `var f = (Func<int, int>)delegate (int x) { return x + 1; };` Real output: `var f = (Func<int, int>)(int x) => x + 1;`, which does not parse (`invalid-result`), because a lambda cannot be a cast operand without parentheses. Second instance: `Func<int> f = delegate () { return 1; } + h;` (with `Func<int> h`) compiles, but the fix writes `Func<int> f = () => 1 + h;`, where the expression body takes in `+ h`. The skeptic's `csc` run reports `error CS0019: Operator '+' cannot be applied to operands of type 'int' and 'Func<int>'`. With a block body (`delegate () { } + g` becomes `() => { } + g`), the result compiles. The lambda text is spliced in without parentheses, so it regroups with a cast before it or a binary operator after it. Touches RH3004-003.

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

**Q4 — Decided by principle P1 (#912), compile error.** Inputs: `return/**/a;` and `return /**/a;`. Real output: `returna;` for both. The fix also removes the whitespace in front of the comment, so any comment directly followed by code joins the tokens on its two sides into a new identifier. Touches RH3201-005.

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

The diagnostic covers only `=> _value = value`, but the fix relays out the whole member: the documentation comment, the attribute's line, and the signature's spacing. A constructor initializer on the signature line moves to its own line as well (`: this(…)`, `: base(…)`). This holds for every SHARED-BODY rule, RH3202 included. `shared/code-fixes.md` on `main` already shows this relayout as expected output: the SHARED-FIX-003 example "layout, for RH3202" rewrites `///<summary>runs</summary>` into three lines. Its covering test is `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixChangesOnlyTheLayoutOfDocumentationCommentsAndRawStringLiterals`, tagged `SHARED-FIX-003`. Reading (a) would change both. Competing readings:

- (a) P9, fix scope: only `=> expression ;` becomes the block, and the rest keeps its layout (`[Obsolete] public  C( int value )` followed by the block);
- (b) P10, fix layout, the current behavior: the member is the code the fix changes and is laid out as a whole.

Touches SHARED-BODY-003, and SHARED-FIX-003's "layout, for RH3202" example with its test named above.

### sdd/specs/shared/empty-parentheses.md

**Q1 — Decided by principle P1 (#912), comment text.** Inputs: `Action a = delegate ( /* none */ ) { };` (RH3106) and `[Obsolete(/* none */)]` (RH3107). Real output: `delegate  { }` and `[Obsolete]`, with the comment inside the list deleted. Touches SHARED-EMPTY-002.

**Q2 — Decided by principle P10 (#912).** Inputs: `Action a = delegate () { };` and `[Obsolete /* a */ ( ) /* b */]`. Real output: `delegate  { }` and `[Obsolete /* a */  /* b */]`, a double space left by the whitespace on both sides of the removed list. Touches SHARED-EMPTY-002.

## Decisions needed

_None._
