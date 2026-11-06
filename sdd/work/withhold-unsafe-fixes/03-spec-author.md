Role: spec author
Commit: 8890e28e
Verdict: #1257: spec written; #1279: spec written; #1281: spec written; #1282: spec written; #1417: no change; #1422: spec written

This amends `01-spec-author.md`. It carries out the maintainer's answers to batch 1 (Q1 A, Q2 A, Q3 A) and lead decisions 4 and 7 from `00-lead.md`. All edits are clarifications in the bug-fix lane, so no change doc is needed.

## thoenissen/Reihitsu#1257

### Changed requirements

- RH3001-007 — now titled "A fix that would parse as a generic name is withheld" — clarified. The condition is "would parse as a generic name"; `(` behind the operator is one example, not the only one.
- RH3005-008 — now titled "A swap that would parse as a generic name is withheld" — clarified. The garbled sentence is repaired: "a `<` and a later `>`, one of them the mirrored operator the fix writes, would read as a type argument list, as they do when the `>` is followed by `(`".
- RH3101-005 — now "A fix that would parse as a generic name or a cast is withheld" — clarified. It adds the cast reading: the member name alone in parentheses, followed by `(`, `!`, or `with`. New example: `(base.A)(5)`.
- RH3105-005 — the same widening. New examples: `(this.A)(5)` and `(this.S)!.Length`. Counter-example: `(this.N) - 1` is fixed to `(N) - 1`.
- RH3101-004 and RH3105-004 — clarified. They now name the requirements that withhold inside their scope ("unless RH3101-005, RH3101-006, or RH3101-007 withholds the fix"; "neither RH3105-005 nor RH3105-006 withholds the fix"), so they no longer demand a rewrite those requirements forbid.

### Examples for the tests

RH3001-007, RH3005-008, RH3101-005 (generic and cast), RH3105-005 (generic, cast, and the holding `- 1` counter-example). The skeptic's matrix in `02-skeptic.md` lists the further failing rows to cover.

### Output questions

_None._

### Internal choices

_None beyond lead decision 4._

### Follow-up candidates

_None new._

## thoenissen/Reihitsu#1279

### Changed requirements

- RH3004-003 — clarified. The second bullet now reads "the block's statements, unchanged, otherwise". The example outputs for `b`, `c`, and `d` are now multi-line and aligned as the formatter writes them (maintainer answer 1, A):
  - `{` under `(` for `b` and `d`;
  - `{` under `async` for `c`;
  - the trailing `/* c */` stays behind `return x;`.
- RH3004-005 — clarified. Its Why now says that a block body in front of an operator draws warning CS8848 (`() => { } + g`).
- RH3004-006 — The lambda is laid out as the formatter lays it out — added. It requires SHARED-FIX-007 with FMT-BREAK-002 and FMT-INDENT-028, whether the block stood on one line or on lines of its own, and also with `{` on the line after the parameter list. An expression body stays on the line of `=>`. The example is the issue's next-line case, plus a single `return` that becomes an expression body.
- Header `Uses`: FMT-BREAK-002, FMT-INDENT-028.

### Examples for the tests

RH3004-003 (the changed multi-line outputs; the existing one-line tests change with them), RH3004-005, RH3004-006.

### Output questions

_None._ Q1 is answered.

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1281

### Changed requirements

- RH3106-001 — clarified to what the analyzer reports: the empty parameter list of every anonymous method, wherever it stands, unless RH3106-003 applies.
- RH3106-002 — clarified and widened (lead decision 4), now titled "An argument whose binding depends on the parameter list keeps its parentheses".
  - Positions: an argument of an invocation, an object creation (including target-typed `new(…)`), a constructor initializer, or an element access.
  - The fix is withheld when the call would bind to another member, become ambiguous (CS0121), or fail (CS0411).
  - New example: a constructor initializer `base(delegate () { })` with two constructors, and the inferred `Run<T>(T t)`.
- RH3106-003 — An anonymous method that takes its natural type is not reported — added (maintainer answer 2, A).
  - Not reported: an implicitly typed local, and a variable, assignment, return value, or cast whose type is not a delegate type, such as `object` or `System.Delegate`.
  - Counter-example: a delegate target through a conditional expression stays reported.
- RH3201-006 — clarified and widened (lead decision 4). It withholds whenever only comments separate two tokens that would merge, without "on its line". It covers a multi-line comment and each of several adjacent comments, also under Fix All. New examples for both.
- `documentation/rules/RH3101.md`: `Derived` is `internal` in both blocks (maintainer answer 3, A), so the example compiles (no CS0060).

### Examples for the tests

RH3106-001, RH3106-002 (overloads, constructor initializer, inference, and the holding single overload), RH3106-003 (both blocks), RH3201-006 (single, multi-line, and adjacent comments, the last also through Fix All).

### Output questions

_None._ Q2 and Q3 are answered.

### Internal choices

- RH3106-003 covers only positions where no delegate type is the target and none is chosen by overload resolution or type inference. An argument whose parameter type is `object` stays under RH3106-002: reported, fix withheld when the binding would fail. This keeps one rule per position kind, as lead decision 4 describes the argument positions. Alternative: treat such an argument as a natural-type position and stop reporting it.

### Follow-up candidates

_None new._ The RH3106-001 scope gap from `01-spec-author.md` is closed.

## thoenissen/Reihitsu#1282

### Changed requirements

- SHARED-PARENS-002 — clarified (lead decision 7, adjacent fix). The whitespace the removal leaves is laid out as the formatter lays it out (SHARED-FIX-007): one space between `delegate` and the body, and no space in front of the `]` or `,` behind an attribute. A new example shows both outputs, `delegate { }` and `[Obsolete /* a */]`. I added it because the requirement's wording, "remove the list from `(` to `)`", read on its own, matched the double space the existing tests pin.

### Examples for the tests

The new SHARED-PARENS-002 example, split by rule: the RH3106 tests that pin `delegate  { }` change to `delegate { }`.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1417

Nothing to do: no answer or decision in this round touches RH3103.

## thoenissen/Reihitsu#1422

### Changed requirements

- RH3104-005 — clarified (lead decision 4). The positions are named explicitly:
  - the expression body of a function returning `void`;
  - an `async` function whose return type carries no value (`Task`, `ValueTask`);
  - a constructor, a finalizer, and a `set`, `init`, `add`, or `remove` accessor.

  A `get` accessor and a `Func<S>` lambda are named as not such positions. The example adds a constructor, a setter, and `Func<Task> t = async () => new S();`.

### Examples for the tests

RH3104-005: one test per position row, including the async `Task` lambda, the constructor, and the setter.

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## Whole run

- `sdd/specs/requirement-ids.txt`: RH3004-006 and RH3106-003 inserted in sorted order.
- No code, tests, or fixtures touched.

## Amendment — maintainer answer 4 (#1281)

Commit: 49f260ca

### Changed requirements

- RH3106-003 — clarified (maintainer answer 4, A). It now also exempts an argument whose parameter type is not a delegate type, when no other overload is a candidate for the call and no type argument is inferred from the argument. Example: `Run(delegate () { })` with `Run(object o)` as the only `Run` is not reported. An argument whose binding is decided by competing overloads or type inference stays under RH3106-002. The internal choice recorded above under #1281, which kept such an argument under RH3106-002, is reversed by this answer.
- RH3106-002 — its scope sentence now ends with "unless RH3106-003 applies".

### Examples for the tests

RH3106-003 (the `Run(object)` call is added to the not-reported block). RH3106-002 is unchanged: `Run(Action<int>)` + `Run(object)` stays reported with the fix withheld.

## Amendment — maintainer answer 5 (#1281)

Commit: 3a7eea52

### Changed requirements

- RH3201-007 — A comment directly in front of a token keeps the whitespace before it — added (maintainer answer 5, A). The case: code precedes the comment on its line, spaces or tabs separate them, and the next token follows the comment directly. The fix removes the comment alone and keeps the whitespace: `return /**/a;` → `return a;` (today it writes `returna;`). RH3201-006 is unchanged and consistent with it: where no whitespace stands in front of the comment either (`return/**/a`), the fix stays withheld. The ID is in `requirement-ids.txt`.

### Examples for the tests

RH3201-007: `return /**/a;` → `return a;`. This replaces the expectation of `FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens`, which `04-test-implementer.md` names.
