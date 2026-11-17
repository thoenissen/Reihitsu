Role: spec author
Commit: 158de0b
Verdict: change doc written

The prompt carried no intended fix or suspected cause beyond the issue text; the issue's proposals (delete RH5302-005, the guards to delete) were checked against the code, and one was adjusted (see *Internal choices*).

## Changed requirements

- FMT-UNSUP-011 — Comments and directives between a binary operator and its right operand are unsupported — added (`sdd/specs/formatter/unsupported-syntax.md`, placed after FMT-UNSUP-010; ID added to `sdd/specs/requirement-ids.txt`).
- FMT-UNSUP-005 — Formatted output is never unsupported — changed: its example `1 + /* c */ 2` holds the new shape; it is now `1 /* c */ + 2` (checked with `scripts/format.sh` on `main`: split into `_a = 1 /* c */ + 2;` and `_b;`).
- FMT-BREAK-017 — Binary operators start the continuation line — changed: `Why` states that the join never meets the shape (FMT-UNSUP-011), as FMT-BREAK-018 does for FMT-UNSUP-008; `Uses` gains FMT-UNSUP-011.
- RH5302-005 — retitled "The fix is withheld for a comment or directive behind an operator" — changed: the fix is withheld when the chain it formats holds the shape anywhere (reported operator, other operators of the chain, binary operators inside an operand), through FMT-UNSUP-007/SHARED-FIX-010, no own check; `Uses` drops FMT-BREAK-001, gains FMT-UNSUP-007, FMT-UNSUP-011, SHARED-FIX-010.
- `AGENTS.md` precheck row names the fifth shape.
- Change doc: `sdd/changes/2026-11/binary-operator-comment-unsupported.md` (Status: Draft), with *Visible output changes* 1–6 and *Special cases* (Deleted / Checked and kept).
- No rule page changes (`documentation/rules/RH5302.md` describes no withheld fix; general-notes covers it).

## Examples for the tests

- FMT-UNSUP-011: refused block (reason/location: `// c1`, line 5, column 22), directive block (refused in every build — test with and without `DEBUG`), other-side block (formatted), plus the bullet examples (`o is /* c */ string`, `a && b && /* c */ c`, `A⏎#if X⏎| B⏎#endif⏎| C` in both builds, a node-level input that is the right operand) and the tie rules: `[Obsolete("old" + /* why */ " API")] private int _e;` gives FMT-UNSUP-004's reason (confirmed on `main`: CLI `(5,23)`, FMT-UNSUP-004 reason, exit 2); `int c = 1 + /* c */ 2;` gives FMT-UNSUP-011's; `private int _a = 1 +⏎#if X⏎2⏎#else⏎3⏎#endif⏎, _b;` gives FMT-UNSUP-010's.
- FMT-UNSUP-005: changed example.
- RH5302-005: the reported-no-fix block (four statements), the comment-in-front block with its fixed output, and the outside-the-chain block with its fixed output. Both fixed outputs were produced on `main` with `scripts/apply-fix.sh RH5302` and are unchanged by this change.
- Change doc *Visible output changes* 1–6: every *Before* was produced on `main` (`scripts/format.sh`, `scripts/apply-fix.sh RH5302`, `scripts/apply-fix.sh RH3202`). CLI columns computed from the inputs: (5,22), (5,21), (4,21), (8,1).

Checks run: every new spec example and change-doc input parses and runs through `scripts/format.sh` on `main` (the other-side block is formatted, adding only the FMT-BLANK-011 blank line above `// c1`). `scripts/test.sh --project architecture`: 184 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: "FMT-UNSUP-011: no test covers it", which the test implementer's first tagged test resolves; no Uses/ID/owner failure.

## Output questions

_None_ beyond the visible output changes the change doc lists, which the lead turns into output questions. Two of them may go beyond what the maintainer pictured in the escalation-10 answer and are worth showing explicitly: change 3 (a same-line block comment behind an operator, `x * /* scale */ 2`, now refuses the whole file) and change 4 (a conditional operand behind a trailing operator, `A |⏎#if DEBUG⏎B |⏎#endif⏎C`, now refuses the file in every build). Both follow from the answer's wording ("comment or directive between a binary operator and its right operand"), and narrowing either would keep the deleted guards (directives) or contradict the FMT-UNSUP-008 precedent (same line).

## Internal choices

- RH5302-005 changed in place (as RH5308-005 was for FMT-UNSUP-008) instead of retired as the `agent-filed` issue proposes: the visible behavior it states remains and widens to the whole chain, and the existing `[Requirement("RH5302-005")]` tests keep a home. Alternative: retire it and rely on SHARED-FIX-010 alone.
- Where FMT-UNSUP-004 (attribute argument of a declaration) or FMT-UNSUP-010 (directive in a field with several declarators, values included) refuses the same trivia, their reason and location stay — today's CLI output for those inputs is unchanged. Alternative: FMT-UNSUP-011's reason wins, which would change today's CLI message (would be an output question).
- "Binary operator" is SHARED-COMMENT-007's definition, referenced, not restated as a list of kinds; pattern operators, `is` patterns, assignments, `..`, `?`/`:` are named only as the other side. Added SHARED-COMMENT-007 and FMT-BREAK-017 to the spec's `Uses`.
- `sdd/specs/formatter/indentation.md` non-goal (line 19) still names "the right operand of a wrapped binary expression" as an unowned continuation column. After this change a supported input may never leave a right operand at a line start, but I did not establish that for every path, so the non-goal is left as is.

## Follow-up candidates

_None._
