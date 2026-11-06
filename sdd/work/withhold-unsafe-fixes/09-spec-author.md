Role: spec author
Commit: 096b6592
Verdict: spec written

Repair round 1. It carries out lead decisions 15, 16, and 17 (`00-lead.md`) and the auditor's findings 1, 2, and 3 (`08-auditor.md`). All edits are clarifications in the bug-fix lane: each writes down what unchanged requirements (SHARED-FIX-002, SHARED-FIX-003, SHARED-FIX-007, SHARED-DIAG-011) and maintainer answers 1, 2, and 4 already demand. No change doc. My prompt restated the lead's decided rows; it carried no intended fix or verdict beyond them to set aside.

## Changed requirements

- RH3106-002 — now "An argument or operand whose binding depends on the parameter list keeps its parentheses" — clarified (decision 16).
  - Also reported: an operand of a binary operator where overload resolution chooses among user-defined operators.
  - The fix is withheld when the operator would no longer bind to the same operator.
  - Under Fix All, the condition is decided on the code the earlier fixes in the document produce (decision 15).
  - New examples:
    - two user-defined `+` operators: reported, no fix;
    - one operator: fixed to `c + delegate { }`;
    - Fix All on `Run(delegate () { }, delegate () { })` with two two-parameter overloads: the first list is removed and the second stays reported.
- RH3106-003 — clarified (decision 16, maintainer answer 2). The positions are now a list. Added:
  - an element of an implicitly typed array creation, `new[] { … }`;
  - the expression returned by a lambda that has no delegate type as its target and whose return type is inferred, as an expression body or in a `return` statement.

  Parentheses, a conditional branch, and a switch-expression arm take the position of the enclosing expression. That covers `var e = (delegate () { });` and writes down the conditional under `var`, which the audit found already not reported. Counter-examples that stay reported: `Action e = (delegate () { });`, `new Action[] { delegate () { } }`, and `Func<Action> f = () => delegate () { };`.
- RH3004-006 — clarified (decision 17). The parameter list, `=>`, and the braces are laid out as the formatter lays them out, and both braces sit in the anchor column. The block's statements, their comments, and the blank lines between them are moved code under SHARED-FIX-007's moved-code bullet:
  - they keep their token spacing, line breaks, braces, and blank lines;
  - every line moves by the same column delta, so the first line stands one level inside the braces;
  - statements from a one-line block move together onto one line between the braces.

  New example: `int y = x+1;`, `if(x>1) Console.WriteLine(y);`, two blank lines, and the one-line block `{ int y = x+1; Console.WriteLine(y); }`.
- RH3004-003 — clarified: the second bullet points to RH3004-006 for the layout of the kept statements.
- RH3005-008, RH3101-005, RH3105-005 — clarified (decision 15). Under Fix All, the condition is decided on the code the earlier fixes in the document produce. Each requirement gets a Fix All example built from the audit's input:
  - RH3105: `M(this.A < this.B, this.C2 > (this.D))` → `M(A < B, {|RH3105:this|}.C2 > (D))`;
  - RH3101: the same with `base.`;
  - RH3005: `M(Max > G, (7) < B)` → `M(G < Max, (7) {|RH3005:<|} B)`.
- SHARED-FIX-002 — clarified (decision 15). The old text required correcting every fix offered on the starting code and also matching the sequential result, which cannot both hold for interacting instances. The new sentence: a diagnostic whose fix is offered on the starting code but withheld once the fixes in front of it in the document have run is not covered. Fix All decides each guard on the code those fixes produce, as sequential application in document order does, and leaves that diagnostic reported. A pointer example to RH3105's Fix All example is added.
- `Uses`: RH3005, RH3101, RH3105, and RH3106 gain SHARED-FIX-002, which they now reference.
- No rule page changed: these are withheld fixes and narrowed reports in corner cases.

Row 15 check of the other rules: RH3201-006 already says "also when Fix All removes them together". The audit found that RH3001, RH3004, RH3104, and RH3107 do not interact under Fix All, so they need no sentence.

## Examples for the tests

- RH3106-002:
  - the operator block with two operators (no fix);
  - the block with one overload and one operator (fixed);
  - the Fix All block with two arguments (first fixed, second reported).
- RH3106-003: the not-reported block now has `(delegate …)` under `var`, the conditional under `var`, `new[] { … }`, and both lambda forms. The reported block adds a parenthesized delegate target, an explicit array, and `Func<Action>`.
- RH3004-006: the new block, with `x+1`, `if(x>1)`, two blank lines, and the one-line two-statement block. The auditor's multi-line `///` case follows from the same requirement: its lines move by the delta.
- RH3005-008, RH3101-005, RH3105-005: one Fix All test each, built from the new examples.

## Output questions

**RH3004 · Use lambda syntax — one-line block with several statements** (within #1279, does not block the run; the spec is written on the recommendation)

Input:
```csharp
Action<int> c = delegate (int x) { int y = x+1; Console.WriteLine(y); };
```

Before (`main`):
```csharp
Action<int> c = (int x) => { int y = x+1; Console.WriteLine(y); };
```

After, A (recommended, now in RH3004-006): the statements are moved code and keep their line.
```csharp
Action<int> c = (int x) =>
                {
                    int y = x+1; Console.WriteLine(y);
                };
```

After, B: the statements are also laid out as the formatter lays them out.
```csharp
Action<int> c = (int x) =>
                {
                    int y = x + 1;
                    Console.WriteLine(y);
                };
```

Why A: SHARED-FIX-007's moved-code bullet keeps line breaks and token spacing "also where formatting the result would change them". Decision 17 applies that bullet to the block's statements, and maintainer answer 1 decided only where the braces go. Requirement IDs: RH3004-006, SHARED-FIX-007. If B is chosen, RH3004-006 gains an exception for statements that shared the block's line, and the `c` example changes.

## Internal choices

- SHARED-FIX-002 gets the interacting-instance sentence once, in the shared spec. The four rule specs only point to it. Alternative: rule sentences alone, which would contradict SHARED-FIX-002's first sentence.
- When the order of the fixes matters, it is document order, the reading of decision 15's "the document the earlier fixes produced". Alternative: leave the order open, which leaves the Fix All result undefined for interacting instances.
- RH3106-003: parentheses, a conditional branch, and a switch-expression arm pass on the position of the enclosing expression. That is one general sentence instead of listing `var e = (…)` alone. It matches what the audit found holding for conditional and switch arms under `var`.
- RH3106-002: an operand counts only for a binary operator. A user-defined unary operator cannot take a delegate operand, and C#'s built-in delegate `+` and `-` take their target from the other operand.

## Follow-up candidates

- Spec gap (RH3106), no output decided: an anonymous method returned by a lambda that is itself an argument whose return type method type inference decides, e.g. `Run(() => delegate () { })` with `static void Run<T>(Func<T> f)`. Without the list the call fails (CS0411). Today the anonymous method is reported, and SHARED-FIX-003 already forbids the fix. Whether it is reported (like RH3106-002) or not (like RH3106-003) is not stated. Recommendation: reported, fix withheld, as RH3106-002's inference case. No issue's case depends on it.

## Amendment — RH3106-003 example

Commit: 1f4dfe6e. Verdict: spec written. A clarification in the bug-fix lane. No normative outcome changes, and no rule page changes. My prompt carried no intended fix or verdict to set aside.

- RH3106-003 — clarified.
  - Removed `var g = b ? delegate () { } : delegate () { };` from the not-reported example: it does not compile (CS0173). A switch expression of anonymous-method arms under `var` does not compile either (CS8506). My earlier note above, that the conditional "under `var`" was written down as not reported, was wrong: that input does not exist.
  - Added a sentence after the parentheses/conditional/switch sentence. A conditional or switch expression whose branches are all anonymous methods has no natural type, so under `var` it does not compile. Its anonymous methods reach RH3106-003 through a target that is not a delegate type, such as `object`. This is explanatory only; the position rule is unchanged.
  - Added a second not-reported example that shows the rule with a target that compiles: `object g = b ? delegate () { } : delegate () { };` and a returned `object` switch expression with arms `delegate () { }` and `(delegate () { })`.
- Test check, with no tests edited:
  - `AnonymousMethodsThatTakeTheirNaturalTypeAreNotReported` now matches the first not-reported example verbatim.
  - `AnonymousMethodInAConditionalOrASwitchArmWithAnObjectTargetIsNotReported` matches the new second example verbatim.
  - No differences.
- `scripts/test.sh --project architecture`: 115 passed.
