Role: spec author
Commit: dd28671
Verdict: spec written

The prompt carried no intended fix or suspected cause; the decisions and answers in `00-lead.md` (decisions 5, 7; batch-1 answers) are applied as binding. `scripts/test.sh --project architecture --filter Sdd`: every spec structure and consistency check passes; the only failure is `EveryRequirementOfAnEnforcedSpecIsCovered` for RH5107-004, FMT-BREAK-038, and FMT-INDENT-033, which the test implementer's `[Requirement]` tests close. Today's outputs used in examples that this change does not alter (array layout, held commas in arrays, property-pattern columns) were checked with `scripts/format.sh` on scratch fixtures on `main`.

## thoenissen/Reihitsu#1737

### Changed requirements

- RH5107-004 — No fix is offered when a comment stands behind the comma on its line — added.
- RH5107-003 — The fix moves the comma behind the previous parameter — changed: now opens "Where RH5107-004 does not withhold it".
- RH5107 header `Uses` gains FMT-BREAK-012, RH5035-001, SHARED-FIX-003; one non-goal added (moving the comment with the comma).

Change doc `sdd/changes/2026-11/rh5107-comment-behind-leading-comma.md` rewritten for decision 5: withheld class, the skeptic's corrections (fix result is not stable under the formatter for Q/R — FMT-BLANK-011 adds a blank line; today's fix violates SHARED-FIX-003 for Q/R), option B recorded as unavailable, issue goal reworded. `Status: Draft` (the lead sets `Accepted`). Rule page unchanged: withheld fix for a corner case, `general-notes.md` covers it.

### Examples for the tests

RH5107-004: `P`, `Q`, `R` reported, no code fix (and the skeptic's X, V under Fix All, W lambda, multi-line `/* a⏎ b */` behind the comma, CRLF); `T` keeps its fix with the shown result. Also a list mixing one commented and one uncommented leading comma: Fix All moves only the uncommented one.

### Output questions

_None._ Withholding for a rare class (RH5035-001) is not in question.

### Internal choices

- The withheld class is "any comment behind the comma on its line", including `, /* c */ int b` which SHARED-FIX-003 leaves undecided, so the guard is one check of the comma's trailing trivia and no fix result differs from the formatter. Alternative: withhold only Q/R (the SHARED-FIX-003 violations) and keep the fix for P.

### Follow-up candidates

- RH5107's fix delegates its layout to the formatter instead of its own text edit (already in `00-lead.md`).

## thoenissen/Reihitsu#1738

### Changed requirements

- FMT-BREAK-038 — A comma that starts a line in an initializer, a collection expression, or a pattern ends the element's line — added.
- FMT-INDENT-033 — A comma that starts a line in an initializer, a collection expression, or a pattern takes its elements' column — added.
- FMT-BREAK-021 — changed: array initializers keep their layout "apart from a comma that starts a line, which FMT-BREAK-038 moves"; `Why:` reconciled (the move keeps the array's line count).
- FMT-BREAK-022 — clarified: pointer to FMT-BREAK-038 for its commas.
- `line-breaks.md` `Uses` gains FMT-INDENT-033; `indentation.md` `Uses` gains FMT-BREAK-038 and a non-goal for switch-expression arms, positional patterns, and base lists.
- Ledger: FMT-BREAK-038, FMT-INDENT-033, RH5107-004 inserted at their sorted positions. No new code owners, so `sdd/specs/README.md` is unchanged.

Change doc `sdd/changes/2026-11/initializer-leading-comma.md` rewritten: answers Q1 (A with arrays moving the comma) and Q2 (not declared) recorded under `Decisions`, single After block, skeptic's corrections (RH5301/RH5303/RH5304 fix paths, held comma alone on its line). `Status: Draft`.

### Examples for the tests

- FMT-BREAK-038: its three examples (moving comma with/without comments, trailing comma, collection expression, anonymous object, `with`, property pattern; arrays `e`, `f`, `int[] y = { 1⏎, 2 }`; held commas in object initializer, collection expression, property pattern, and arrays).
- FMT-INDENT-033: its example (held-comma lines in each construct, including `int[] x = { 1 // k⏎, 2 };`).
- From the skeptic's list, still to cover: a directive-held comma (`X = 3⏎#if A⏎, Y = 4⏎#endif`, parsed with `A` defined), nested `Items = { … }` and complex element initializer, `new[] { … }` and `stackalloc`, a property pattern directly behind `case`, nesting inside an argument and a lambda, a multi-line `/* a⏎ b */` behind the comma, CRLF and a second pass; and the code-fix paths RH5301, RH5303, RH5304 on a leading-comma input, whose result must equal whole-document formatting (SHARED-FIX-007).

### Output questions

_None._ One reading the lead should confirm against the answer: a **held** comma in an array initializer keeps the element behind it on its line (`1 // k⏎, 2` stays `, 2`, only the column changes, as on `main` apart from the column). Reason: the maintainer's change to A concerns the comma that moves; for the held comma, A kept array line breaks as written, and the line-break phase leaves it as today. If the lead reads the answer's held-comma sentence as covering arrays too (`,` alone, `2` on the next line), the `g`/`x` examples of FMT-BREAK-038 and FMT-INDENT-033 and the change doc's `g` change; nothing else does.

### Internal choices

- New requirements FMT-BREAK-038 and FMT-INDENT-033 rather than paragraphs in FMT-BREAK-021/022 and FMT-INDENT-022 to -026 (decision 4).
- Parenthesized patterns are not in FMT-BREAK-038's list: they hold one pattern and no comma.
- Blank lines between an element and a comma that moves go with the join (FMT-BREAK-030); not restated in FMT-BREAK-038.

### Follow-up candidates

Already in `00-lead.md` from the skeptic: a comma that starts a line between switch-expression arms, in a positional pattern, and in a base list (now also an indentation non-goal). _No new ones._
