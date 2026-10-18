Role: spec author
Commit: 3a37d15
Verdict: spec written

Repair round for the skeptic's findings in `02-skeptic.md`. I re-ran every input the skeptic cites with my own fixtures through the probe app from 01 (`/tmp/claude-0/-home-user-Reihitsu/94033b8c-43c8-5bb8-9963-d2b62ecb37ee/scratchpad/probe/probe.cs`). All of them reproduce as the skeptic states. I took the skeptic's suggested changes as suggestions only; the edits below are my own. There is still no change doc: the run specifies existing behavior.

## Changed requirements

Shared spec `sdd/specs/shared/comment-placement.md`:

- SHARED-COMMENT-003 — Each comment is reported on its own — changed. A comment in a position that several rules report is now stated to be reported by each of them, with a second example (RH5038 and RH5039 on one comment). This is what the RH5035 and RH5038 non-goals contradicted (finding 5).
- SHARED-COMMENT-006 — A gap that a nested construct lays out belongs to that construct — changed (findings 1 and 4).
  - "Every token, first token included" is replaced by a list that says which tokens of each construct count.
  - Anonymous object: from its `{` on. Switch expression: from `switch` on. Property or positional pattern: from its `{` or `(` on.
  - Collection expression: everything after its own opening bracket. The bracket of a nested collection expression is one of the outer one's tokens (new example: `jagged ?? [` / `// inner` / `[1]]`, not reported).
  - A coverage clause leaves out the Q2 positions: the `new` of an anonymous object, a token of the switched-on expression, a token of a pattern's type, except at an element start.
- SHARED-COMMENT-007 — What counts as a binary operator — changed (finding 6).
  - The `..` of a range does not count; the example has `x` / `// end` / `..2`, reported.
  - A coverage clause leaves out an `is` followed by a name that denotes a constant (Q3).
- SHARED-COMMENT-008 — changed (finding 3). "MUST still be reported" is replaced: a comment after a binary operator is not kept valid by 008, and 004 and 006 decide it.
- SHARED-COMMENT-009 — changed (findings 2 and 3).
  - The comment must be directly before the operator's first character: `.`, the `?` of `?.`, or the `!` of `!.`.
  - These are not kept valid by 009 and are decided by 004 and 006: a comment between `!` and `.`, before the first call, before a member access that is not invoked, or before an element access. New example: `o.ToString()!` / `// text` / `.Trim()`, reported. An existing test already covers it: `VerifyCommentBetweenNullForgivingOperatorAndCallIsReported`.
  - The coverage clause now also leaves out a comment before or after a postfix `++`/`--` that a member access follows, and one before a member access written with `->`.
- SHARED-COMMENT-010 — What a statement header is — added (finding 7). It states the header set and span once. RH5036-001 and RH5039-001 now reference it, and both specs list it under `Uses`.

Rule specs:

- RH5034-001 — changed. A directive between the documentation comment and the comment does not matter; there is a `#region` example.
- RH5034-002 — changed. It now names the accessor and the parameter, with examples of both as not reported.
- RH5035 — two non-goals removed (RH5033 and RH5042 overlap). RH5035-001 changed: it adds the finalizer (example) and the extension block (text only).
- RH5036-001 — changed: it now references SHARED-COMMENT-010.
- RH5036-002 — changed: it adds `try`, `catch`, `finally`, and `checked`, with examples, and has a new Why.
- RH5038 — non-goal "type argument list — RH5046" removed. RH5038-001 changed: it adds target-typed `new(…)` and the indexer assignment in an object initializer, with examples.
- RH5039-001 — changed: the header set now comes from SHARED-COMMENT-010.
- RH5041-001 — changed: it adds the declarators of a `using` statement, with an example.
- RH5042-001 — changed: it adds the event field initializer and the `with` member assignment, with examples.
- RH5042-002 — changed: it adds the `using` alias, with an example.

The ledger gains SHARED-COMMENT-010 at its sorted position. No ID was removed or renumbered.

Checks:

- `verify.py` now compares every rule marked in a block. All 40 example blocks match and compile.
- The prose claims were checked separately:
  - RH5039 also reports the second SHARED-COMMENT-003 example.
  - `unchecked` is not reported by RH5036.
- `SpecConsistencyTests` and `SpecStructureTests` pass (26/26).

## Examples for the tests

- Changed or new requirements whose new example cases have no existing test:
  - SHARED-COMMENT-003: the RH5038 and RH5039 double report.
  - SHARED-COMMENT-006: the jagged collection expression.
  - SHARED-COMMENT-007: `..`.
  - SHARED-COMMENT-010: `await foreach`, deconstructing `foreach`, `switch`.
  - RH5034-001: `#region`.
  - RH5034-002: accessor and parameter.
  - RH5035-001: finalizer.
  - RH5036-002: `try`, `catch`, `finally`, `checked`.
  - RH5038-001: `new(…)` and `{ [0] = 1 }`.
  - RH5041-001: `using` statement.
  - RH5042-001: event field and `with`.
  - RH5042-002: `using` alias.
- These inputs are left out through coverage clauses, so no test may tag them with a requirement:
  - the #1058 Q1 inputs and the Q1 additions below;
  - the Q2 inputs;
  - the Q3 inputs.
  - `VerifyNoDiagnosticForCommentBeforeConditionalElementAccessInArgument` and `VerifyCommentBeforeNullForgivingConditionalLinkInArgumentIsReported` stay untagged.

## Open questions to file

All three belong to thoenissen/Reihitsu#1058, `SDD open questions: sdd/specs/shared/comment-placement.md`. Each text below stands on its own.

### Addition to Q1 — postfix `++`/`--` and `->` as chain links

Rules affected: the region rules RH5035, RH5038, RH5039, RH5043, RH5044, RH5046, and RH5047.

Input. Each chain is an argument of `object Use(params object[] v) => v;`, with the comment on its own line before the last part. `c` is an instance of `class P { public int Q; public P B() => this; }`, and `p` is an `S*` for `unsafe struct S { public S* A() => null; public S* B() => null; }`.

- `Use(c.B().Q` / `// c` / `++.ToString())` — not reported. The comment is treated as standing before a later call, though it stands before a `++`.
- `Use(c.B().Q++` / `// c` / `.ToString())` — reported by RH5038 (and by RH5048). `.ToString()` is an invoked member access after the invoked `.B()`, so it is a later call.
- `Use(p->A()` / `// c` / `->B())` — not reported, though `->` is not one of the link operators `.`, `?.`, `!.`, `!?.`.

Why it looks wrong: the exemption is meant for a comment that describes the later call after it. SHARED-CHAIN-001 knows neither `->` nor a postfix `++`/`--` in front of `.` as a link operator. The comment rules treat any postfix operator in front of `.` the way they treat the `!` of `!.`, and they accept every member-access operator, `->` included.

Readings:
- (a) The exemption follows SHARED-CHAIN-001, the reading Q1 (a) proposes. Then the first and third inputs are reported, and the second is not.
- (b) The current behavior is intended and is specified as it is.

Recommendation: (a), together with Q1.

Touches SHARED-COMMENT-004, SHARED-COMMENT-009, and SHARED-CHAIN-001.

### Q2 — Does a nested construct own the part written in front of its opening delimiter?

Rules affected: the region rules RH5035, RH5038, RH5039, RH5043, RH5044, RH5046, and RH5047.

Inside a region, a comment is not reported when a construct between the token after it and the region lays out its own lines (an anonymous object, a switch expression, a property pattern, …). The analyzer counts every token of such a construct, unless that token starts an element of the region. That includes the `new` of an anonymous object, every token of the expression a switch expression switches on, and every token of the type in front of a property pattern. A comment there is therefore not reported, while the same position before a construct that lays out no lines of its own is reported.

Input. RH5038 unless another rule is named. Each pair is a questioned input followed by a comparable one. `Use` takes `params object[]`, `Use1(object value)`, and `class D { public int A; }`.

- `Use(o ??` / `// c` / `new { First = 1 })` — not reported. `Use(d ??` / `// c` / `new D { A = 1 })` — reported.
- `Use1(value:` / `// c` / `new { First = 1 })` — not reported. `Use1(value:` / `// c` / `1)` — reported.
- `Use(x +` / `// c` / `o switch { _ => 1 })` — not reported by RH5038 (RH5049 reports it). `Use(d ??` / `// c` / `(o switch { _ => d }))` — reported.
- RH5039: `if (a &&` / `// c` / `o switch { _ => true })` — not reported. `if (a &&` / `// c` / `b)` — reported.
- `Use(1, items` / `// c` / `.Length switch { _ => 1 })` — not reported. `Use(1, items` / `// c` / `.Length)` — reported.
- `Use(o is` / `// c` / `string { Length: 1 })` — not reported. `Use(o is` / `// c` / `string s)` — reported.

Why it looks wrong: a comment before the `new` of an anonymous object, inside the switched-on expression, or before a pattern's type stands outside the part the construct lays out (its braces or its arms). The comparable input with an object creation, a plain operand, or a declaration pattern is reported. The rule pages say a comment "inside a nested block, initializer, switch expression, pattern, or query" is not reported.

Readings:
- (a) The current behavior is intended: every questioned input stays unreported.
- (b) A construct owns its opening delimiter and what follows it. Its leading part (an anonymous object's `new`, the switched-on expression, a pattern's type) belongs to the element, so each questioned input is reported like the input after it.

Both readings leave these unchanged: a comment before the `{` of an anonymous object or a property pattern, before `switch`, and every element start. The formatter's list-element join decides with the same table (FMT-BREAK-015), so reading (b) changes the formatter too, or splits the two copies of the table.

Recommendation: (b), with the formatter changed in the same decision.

Touches SHARED-COMMENT-004, SHARED-COMMENT-006, and FMT-BREAK-015.

### Q3 — Does an `is` followed by a name count as a type test?

Rules affected: the region rules (through SHARED-COMMENT-008) and RH5049.

Input, with `enum Color { Red }`, `object M(params object[] v) => v;`, and `object o`:

- `M(o` / `// c` / `is Color.Red)` — not reported by RH5038: the `is` counts as a binary operator.
- `_ = o is` / `// c` / `Color.Red;` — reported by RH5049.
- For comparison, `M(o` / `// c` / `is null)` — reported by RH5038, and `_ = o is` / `// c` / `null;` — not reported by RH5049.

Why it looks wrong: `o is Color.Red` is a constant pattern, like `o is null`. It counts as a type test only because the parser reads a name after `is` as a type, before the compiler binds it as a constant.

Readings:
- (a) Syntax decides: anything written as a type counts, a constant such as `Color.Red` included. This is the current behavior.
- (b) Only a real type test counts. Then the first input is reported and the second is not. This needs the semantic model, which these syntax-tree analyzers do not use.

Recommendation: (a). The stakes are low, and the rules stay syntax-only.

Touches SHARED-COMMENT-007, SHARED-COMMENT-008, and RH5049-001.

## Decisions needed

_None._
