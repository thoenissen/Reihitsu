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

## Amendment — rows 21 and 22

Commit: 22c659a0. Both are clarifications in the bug-fix lane: no change doc, no rule page. My prompt carried the lead's decided rows and no other intended fix or verdict.

### Changed requirements

- RH3004-006 — clarified (row 22). The fix is offered for a block that holds a directive: RH3004-004 withholds it only for a comment or directive *outside* the parameter list and the body, and RH3004-003 only keeps the block for one, so row 22 applies. The column shift of the moved statements is now measured on the line on which the block's first statement starts, from the first character of the moved code on it, not on the block's first line; a directive or comment line in front of the first statement does not decide it. A block without a statement measures on its first line. New example: an `#if DEBUG` / `#else` / `#endif` block whose first line is `#if` in column 0. Both statements land one level inside the braces (column 28), and the directive lines move by the same 16 columns (column 16), as the existing "every line moves by the same number of columns" already states.
- RH3106-003 — clarified (row 21). The requirement now says plainly that a conditional with another branch, a switch expression with another arm, and an implicitly typed array creation with another element that is not an anonymous function and has a delegate type give the anonymous method that delegate type as its target. Its list is then reported, also under `var`. New example on the reported side:
  - `var x = b ? delegate () { } : someAction;`
  - `var y = i switch { 1 => delegate () { }, _ => someAction };`
  - `var z = new[] { delegate () { }, someAction };`

### Examples for the tests

- RH3004-006: the `#if` block example (a test against row 22's defect).
- RH3106-003: the three `var` lines. The switch arm and the array element follow from the same rule as the conditional. Reading `TakesNaturalType` at 22c659a0 suggests both are not reported either today, because the arm resolves to the switch expression under `var` and the element's parent is an implicit array initializer. So they are probably the same defect as row 21, and the repair should cover them in the same step.

### Output questions

**1 · RH3004 · Use lambda syntax — a column-0 directive inside the moved block.** RH3004-006 says every line of the moved block moves by the same number of columns. A conditional directive in column 0 therefore lands in the shifted column, and the formatter then moves it back to column 0 (FMT-INDENT-009). The example above is written to the requirement as it stands. Row 22 decides only the statements, not the directive lines. Answering B would change only the directive lines of the example and the test.

Input:
```csharp
        Action<int> b = delegate (int x)
        {
#if DEBUG
            Console.WriteLine(x);
#else
            Console.WriteLine(-x);
#endif
        };
```

Before (`main`): not run by me. The lead should fill it in from `main` (the layout path differs from this run's). At 22c659a0 the shift is measured on the `#if` line, so the statements land one level too deep, which is the row 22 defect.

After, A (the requirement as written):
```csharp
        Action<int> b = (int x) =>
                        {
                #if DEBUG
                            Console.WriteLine(x);
                #else
                            Console.WriteLine(-x);
                #endif
                        };
```

After, B (recommended):
```csharp
        Action<int> b = (int x) =>
                        {
#if DEBUG
                            Console.WriteLine(x);
#else
                            Console.WriteLine(-x);
#endif
                        };
```

Why B: a conditional directive in column 0 belongs to no nesting depth. Keeping it where the author wrote it is the directive's own layout under SHARED-FIX-007, and the result needs no second formatting diff.

### Internal choices

- RH3106-003: I also wrote down the switch arm and the implicit array element, not only the conditional the row names. All three are the same rule. Alternative: the conditional alone, with the other two left as follow-ups.
- RH3004-006: the shift is measured "from the first character of the moved code on that line", so a first statement that starts behind a comment or on the opening brace's line is still well defined. Alternative: measure from the statement's first token, which goes wrong behind a `/* c */` on the same line.

### Follow-up candidates

_None._

## Amendment — answer 7

Commit: 41e37b15. This is a clarification in the bug-fix lane, so it needs no change doc and no rule page change. My prompt carried the maintainer's answer (B) and the standing instruction, and no other intended fix or verdict.

### Changed requirements

- RH3004-006 — clarified (answer 7).
  - In the block the fix moves, a preprocessor directive line that starts in column 0 stays in column 0. `#region` and `#endregion` lines are the exception.
  - Every other line moves by the number of columns measured on the first statement's line. Those other lines are the statements, comment lines, a directive line in another column, and a `#region`/`#endregion` line in column 0.
  - The `#if` example now keeps `#if`, `#else`, and `#endif` in column 0, with the statements in column 28.
  - A second example covers the other side of the qualifier (FC-002). In it, a column-0 `#region`/`#endregion` and an `#if`/`#endif` indented by 12 columns move with the statements by 16 columns, to columns 16 and 28.

### Examples for the tests

- RH3004-006: the conditional-directive example, which changes the expected output of the existing row 22 test.
- RH3004-006: the new region and indented-directive example.

### Decided on recommendation

- RH3004-006: answer 7 names conditional directives. The requirement extends "stays in column 0" to every directive the formatter keeps in column 0 (FMT-INDENT-009): `#pragma`, `#nullable`, `#define`, `#undef`, `#line`, `#error`, and `#warning`. It excludes `#region`/`#endregion`, which the formatter indents. The reason is the one behind answer 7: such a directive belongs to no nesting depth, and keeping it avoids a second formatting diff. The alternative was conditional directives only, which would shift a column-0 `#pragma` that the formatter then moves back.
- RH3004-006: a directive that does not start in column 0 moves with the statements. It is moved code (SHARED-FIX-007), and answer 7 only decides column 0. The alternative was to move it to column 0, which would be layout the moved-code bullet does not allow.

### Output questions

_None._ The standing instruction applies.

### Internal choices

_None._

### Follow-up candidates

_None._

## Amendment — row 25

Commit: b7307e27. This is a clarification in the bug-fix lane, so it needs no change doc and no rule page change. My prompt carried the lead's decision (row 25) and no verdict beyond it.

### Changed requirements

- RH3004-006 — clarified (row 25).
  - The shift is measured on the block's first statement line: the first line, in source order, on which a statement starts, whether the statement is active code or disabled text of an inactive branch. A line of disabled text counts when it holds anything other than whitespace and comments.
  - The fix's output MUST NOT depend on which preprocessor symbols are defined.
  - A block in which no line holds a statement, active or disabled, is measured on its first line.
  - The `#region` example now states that it gives the same output with and without `DEBUG`. The expected output is unchanged: without `DEBUG`, the disabled `Console.WriteLine(x);` at column 12 decides, which is the same 16-column shift.
  - New example (one side, FC-002): a disabled statement at column 12 in front of an active statement at column 16. Both builds move the lines by 16 columns. Measuring on active code alone would give 12 without `DEBUG`.
  - New example (other side): a block that holds only a comment, at column 4, is measured on its first line and the comment moves to column 28.

### Examples for the tests

- RH3004-006: the `#region` example, now also without `DEBUG` (the reading the test implementer left untested).
- RH3004-006: the new disabled-before-active example, with and without `DEBUG`.
- RH3004-006: the comment-only block example. The existing test `FixMeasuresTheShiftOfABlockWithoutAStatementOnItsFirstLine` may already cover it.

### Decided on recommendation

- RH3004-006: a line of disabled text counts as a statement line when it holds anything other than whitespace and comments. Disabled text is not parsed, so "holds a statement" needs a test that works on text. The alternative was to count every non-blank disabled line. Under that alternative, a comment line in an inactive branch would decide the shift, while the same comment in active code would not.

### Output questions

_None._ The standing instruction applies.

### Internal choices

_None._

### Follow-up candidates

_None._
