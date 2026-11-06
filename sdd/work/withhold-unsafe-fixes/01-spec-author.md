Role: spec author
Commit: da45a0fa
Verdict: #1257: spec written; #1279: open question; #1281: open question; #1282: spec written; #1417: spec written; #1422: spec written

All changes are clarifications for the bug-fix lane: each new or widened requirement either writes down today's output or records an input that SHARED-DIAG-011 or SHARED-FIX-003 already excludes from a report or a fix. No change doc. My prompt carried no intended fix or verdict to set aside.

## thoenissen/Reihitsu#1257

### Changed requirements

- RH3001-007 — A fix that would read as a generic name is withheld — added
- RH3005-008 — A swap that would read as a generic name is withheld — added
- RH3101-005 — A fix that would read as a generic name is withheld — added
- RH3105-005 — A fix that would read as a generic name is withheld — added

RH3001-004, RH3005-004, RH3101-004, and RH3105-004 already left these inputs out of their conditions and stay unchanged. Each new requirement withholds the fix only where the rewritten code would parse as a generic name. It says outright that it does not withhold the fix where the rewrite still parses as the original, so a broader guard would break the spec.

### Examples for the tests

RH3001-007, RH3005-008, RH3101-005, RH3105-005. The `...WritesAGenericReading` characterization tests in the four test classes become "reported, no code fix" tests tagged with these IDs.

### Output questions

_None._

### Internal choices

- The withheld class is recorded as a requirement in each rule spec (`Code-fix coverage`: record the gap in the rule's spec). Alternative: rely on SHARED-FIX-003 alone.
- The `-004` conditions were not widened to cover the safe inputs they exclude (such as `s > !x` with a user-defined operator and no `<`). Today's output for those inputs stays unspecified. Alternative: widen each condition to "unless RH####-00x withholds", which would fix today's output for them.

### Follow-up candidates

- Spec completeness, no visible output in question: RH3001-004, RH3005-004, RH3101-004, and RH3105-004 still leave out safe inputs that today's fix rewrites, such as a second `<`/`>` in the largest expression that cannot form a generic reading. A follow-up could state today's output for them.

## thoenissen/Reihitsu#1279

### Changed requirements

- RH3004-005 — An operand of a cast or a binary operator withholds the fix — added. It covers the exclusion in RH3004-003 exactly, as the triage comment narrowed it. Parentheses and `static` stay with #1436. The counter-side examples are inside parentheses, `(Func<int, int>)(delegate …)`, and a compound assignment, `g += delegate () { return 1; };`; both use expression bodies so they do not depend on question 1.
- The layout part (SHARED-FIX-007) is not written yet. It waits for output question 1.

### Examples for the tests

RH3004-005. The skeptic should confirm that the right-operand case `g + delegate () { }` → `g + () => { }` really does not compile or changes meaning. The requirement withholds every binary operand, as triage decided.

### Output questions

**1 · RH3004 · Use lambda syntax — anonymous method whose block body is on one line**

Two unchanged requirements disagree. The RH3004-003 example pins `(int x) => { Console.WriteLine(x); }` on one line. SHARED-FIX-007 requires the changed code to be laid out as the formatter lays it out, and the formatter breaks every lambda block (FMT-BREAK-002) and aligns it under the lambda (FMT-INDENT-028). RH5417 reports the one-line result and offers no fix for lambdas (RH5417-004). Requirement IDs: RH3004-003, SHARED-FIX-007, FMT-BREAK-002, FMT-INDENT-028.

The issue's own case, `{` on the next line, is decided by the issue, the triage comment, and SHARED-FIX-007. Both options give the same output for it:

```csharp
// Input
        Action<int> b = delegate (int x)
        {
            Console.WriteLine(x);
        };
// After (A and B), as the formatter lays it out
        Action<int> b = (int x) =>
                        {
                            Console.WriteLine(x);
                        };
```

Only the one-line block is in question.

Input:
```csharp
        Action<int> b = delegate (int x) { Console.WriteLine(x); };
```

Before (`main`):
```csharp
        Action<int> b = (int x) => { Console.WriteLine(x); };
```

After, A (recommended), as the formatter lays it out:
```csharp
        Action<int> b = (int x) =>
                        {
                            Console.WriteLine(x);
                        };
```

After, B: unchanged from `main`, one line. RH5417 then reports it with no fix.

Why A: formatting the fix's result then changes nothing, and the result is not reported again by RH5417, which offers no fix for a lambda body.

If A is chosen, RH3004-003's examples for `b`, `c`, and `d` become multi-line and a layout requirement RH3004-006 is added. If B is chosen, RH3004-006 is limited to blocks that span several lines.

### Internal choices

_None beyond the above._

### Follow-up candidates

_None._ Parentheses and `static`/`async` are already #1436.

## thoenissen/Reihitsu#1281

### Changed requirements

- RH3005-007 — A swap that would group the operands differently is withheld — added (`1 == 1 == b`)
- RH3101-007 — A virtual member in a type that is not sealed withholds the fix — added (`base.M()` with a virtual `M` in an unsealed type)
- RH3106-002 — An argument whose overload depends on the parameter list keeps its parentheses — added. The argument stays reported, which clarifies today's report; the fix is withheld where the call would bind elsewhere or become ambiguous (CS0121).
- RH3201-006 — A comment that keeps two tokens apart withholds the fix — added (`return/**/a`)
- `documentation/rules/RH3101.md`: the example's `BuildName` is no longer `virtual` in either block. Under RH3101-007, the old example shows a correction the fix no longer makes and that would change what runs. `RuleDocumentationExampleTests` requires the Correction block to match the fix's output. What is reported and how to fix it stay the same for the user.
- The RH3103 part is handled under #1417.

### Examples for the tests

RH3005-007, RH3101-007 (two examples: unsealed type withheld, sealed type fixed), RH3106-002 (two examples: overloads withheld, single overload fixed), RH3201-006. The characterization test `FixOfComparisonWithComparisonOperandWritesTheSwapWithoutParentheses` becomes a "no code fix" test for RH3005-007.

### Output questions

**2 · RH3106 · Unnecessary delegate parentheses should be removed — anonymous method assigned to `var` (C# 10 or later)**

Input:
```csharp
var a = delegate () { };
```

Before (`main`): reported; the fix writes `var a = delegate { };`, which does not compile (CS8917: the delegate type could not be inferred).

After, A (recommended): not reported.

After, B: reported; no fix offered.

Why it is in question: the issue, written before SHARED-DIAG-011, asks for B. SHARED-DIAG-011 says a construct is not reported where "the position itself rules it out with a compile error, such as a position that requires a compile-time constant". An implicitly typed local needs a value with a natural type, and `delegate { }` has none, so A follows. Requirement IDs: SHARED-DIAG-011, SHARED-FIX-003, RH3106-001.

Why A: as with a constant position, no code the user can write without the parentheses compiles in that position. The answer also decides the other positions that need a natural type, such as `Delegate d = delegate () { };` and `object o = delegate () { };`.

Nothing for this input goes into the spec until the answer.

### Internal choices

- RH3005-007 withholds only when the swap would regroup the operands. It does not withhold for every comparison operand, so `true == a < b` → `a < b == true` keeps today's fix. Alternative: withhold for any comparison operand (simpler, but gives up safe inputs).
- RH3201-006 withholds only when the two tokens would merge, which keeps today's fix for `a +/* */b`. Alternative: withhold whenever code touches the comment on both sides.
- RH3101-007 condition: the member is `virtual` or a non-`sealed` `override`, and the type containing the access is not sealed. It covers element access through a virtual indexer as well.

### Follow-up candidates

- Spec gap, no visible output in question: RH3106-001 states the report only for "the initializer of a variable whose declared type is a delegate type", while the analyzer reports every empty parameter list (arguments included; RH3106-002 now states the argument case). After question 2 is answered, RH3106-001 could be widened to today's full scope.
- Spec gap: RH3201-005 does not specify a comment at the start of a line followed by code (`/* */ int _a;`), or a comment with code directly on both sides where the tokens would not merge.

## thoenissen/Reihitsu#1282

### Changed requirements

- SHARED-PARENS-003 — A comment inside the list withholds the fix — added to the shared spec, as the issue suggests (one removal shared by RH3106 and RH3107). Both rule headers now list it under `Uses`. Covers `delegate ( /* none */ ) { }` and `[Obsolete(/* none */)]`.
- RH3101-006 — A comment between `base` and the member name withholds the fix — added. A comment in an element access stays behind the `this` the fix writes; that is today's output and serves as the counter-example.
- RH3104-006 — A comment inside the creation withholds the fix — added (`new int(/* x */)`)
- RH3105-006 — A comment between `this` and the member name withholds the fix — added

### Examples for the tests

SHARED-PARENS-003 (one example each for RH3106 and RH3107), RH3101-006, RH3104-006, RH3105-006.

### Output questions

_None._

### Internal choices

- The comment withholds are recorded per rule, as RH3005-006 and RH3103-005 already do, rather than left to SHARED-FIX-003 alone.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1417

### Changed requirements

- RH3103-001 — clarified: also the target of a using alias when compiled with C# 12 or later, and "unless RH3103-002 or RH3103-007 applies".
- RH3103-004 — clarified: the fix also covers a using alias target. This is today's output, pinned by `FixOfNullableGenericInUsingAliasWritesTheShorthand`. The issue says the fix is withheld for the alias; on `main` it is offered and writes `using N = int?;`.
- RH3103-006 — retired: a dotted `nameof` argument is now not reported, so the requirement's "reported, fix withheld" no longer holds.
- RH3103-007 — Other positions where `T?` cannot be written are not reported — added:
  - dotted or `global::` `nameof` argument;
  - member-access receiver, including `nameof(Nullable<int>.HasValue)`;
  - `typeof(Nullable<>)`;
  - `using static System.Nullable<int>;` (from the issue comment);
  - using alias target below C# 12.

  A type argument in any of these positions stays reported.
- `documentation/rules/RH3103.md`: removed the sentence "No code fix is offered for a dotted `Nullable<T>` … as the argument of `nameof` …". That input is no longer reported, and a withheld fix does not belong on a rule page (`create-rule-doc`). The issue's "rule page states the exempt positions" is dropped for the same reason: exceptions stay off rule pages.

### Examples for the tests

RH3103-007 (one block for C# 11 and earlier, nothing reported; one for C# 12 and later, only the alias reported and fixed). Tests now tagged RH3103-006 (`DottedNullableGenericAsNameofArgumentDoesNotOfferFix`, `GlobalDottedNullableGenericAsNameofArgumentDoesNotOfferFix`) must be retagged RH3103-007 and turned into "not reported" tests. The characterization tests `FixOfUnboundNullableGenericInTypeofWritesAQuestionMarkOnly`, `FixOfNullableGenericQualifyingAMemberInNameofWritesTheShorthand`, and `FixOfNullableGenericReceivingAStaticCallWritesTheShorthand` become "not reported" tests for RH3103-007. `FixOfNullableGenericInUsingAliasWritesTheShorthand` gets RH3103-004 and needs a C# 11 counterpart.

### Output questions

_None._

### Internal choices

- The other positions go into a new RH3103-007; RH3103-002 stays unchanged. Alternative: widen RH3103-002 in place, which changes an existing requirement's scope.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1422

### Changed requirements

- RH3104-001 — clarified: "unless RH3104-002 or RH3104-005 applies".
- RH3104-005 — A creation that stands as a statement is not reported — added. It covers every position where only a statement expression is valid: an expression statement, a `for` initializer or iterator, and the expression body of a function that returns no value, including `Action a = () => new S();` and `void N() => new S();`. The issue names only `new S();`, but SHARED-DIAG-011 excludes every one of these for the same reason (CS0201). The counter-examples are a discard assignment, a lambda that returns `S`, and a `return`.
- RH5417-001 — clarified: "unless RH5417-005 applies".
- RH5417-005 — A body inside a non-verbatim interpolation is not reported before C# 11 — added. It covers any depth. A verbatim interpolated string's holes accept line breaks in every version, so a body there stays reported; the skeptic should confirm this against C# 10. Every body stays reported from C# 11 on.
- Rule pages unchanged: these are exceptions.

### Examples for the tests

RH3104-005 (two blocks: not reported, reported), RH5417-005 (three blocks: C# 10 not reported, C# 11 reported, verbatim reported).

### Output questions

_None._

### Internal choices

- RH3104-005 covers every statement-expression position, not only the expression statement. Alternative: only `new S();`, which would leave the spec contradicting SHARED-DIAG-011 for the other positions.

### Follow-up candidates

_None._

## Whole run

- `sdd/specs/requirement-ids.txt`: 16 IDs inserted in sorted order (RH3001-007, RH3004-005, RH3005-007, RH3005-008, RH3101-005, RH3101-006, RH3101-007, RH3103-007, RH3104-005, RH3104-006, RH3105-005, RH3105-006, RH3106-002, RH3201-006, RH5417-005, SHARED-PARENS-003); `LC_ALL=C sort -c` passed before the insert.
- No code, tests, or fixtures touched.
