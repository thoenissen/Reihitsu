Role: spec author
Commit: 7974c5d
Verdict: spec written (Q7 B carried into the specs and the change doc)

How the statements below were established: `00-lead.md` (Maintainer answers, Q7 B) and `03-spec-author.md` (Q7) read; the RH7108 one-line "before" measured with `scripts/apply-fix.sh RH7108` on this branch (no code change since main), LF and CRLF agreeing: the list is spread over four lines with `add` first. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 50 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for RH7004-007, SHARED-ORDER-009, SHARED-ORDER-010, unchanged from 03 and closed by the test implementer's tagged tests.

Ignored input: none in my prompt.

## thoenissen/Reihitsu#969, #1263, #1151, #1604

No change in this round; see 03-spec-author.md.

## thoenissen/Reihitsu#1335

### Changed requirements

- SHARED-ORDER-009 — A move moves whole lines — changed: when the whole accessor list stands on one line (its opening and closing brace on the same line), the rule does not refuse the move; the fix swaps the accessors within that line, the whitespace between braces and accessors stays at its position (SHARED-ORDER-002), and no line moves, so SHARED-FIX-007's column shift does not apply. Why extended; RH7107 one-line example added.
- SHARED-FIX-007 — *Moved code* — clarified: no shift for an accessor swapped within a one-line list. `Uses` of `shared/code-fixes.md` gains SHARED-ORDER-009 (now referenced).
- RH7107-004 — changed: states the one-line swap (by reference to SHARED-ORDER-009) and adds the Q7 example with bodies (`B { set => _b = value; get => _b; }` → `B { get => _b; set => _b = value; }`); the `{ init; get; }` example is unchanged.
- RH7108-004 — changed: the same sentence and a one-line event example (`{ remove => …; add => …; }` → `{ add => …; remove => …; }`).
- Change doc `sdd/changes/2026-11/moved-code-keeps-its-lines.md`: affected requirements, *What* (whole-line moves), a new visible output change "An accessor list on one line stays on one line" (RH7107 measured in 03, RH7108 measured here), a Decision "Accessors on one line swap places within the line" (Q7 B), and Q7 A under *Alternatives*. Status stays Draft.

### Examples for the tests

SHARED-ORDER-009 (the new one-line RH7107 example), RH7107-004 (both one-line examples), RH7108-004 (the one-line event example). The spanning-lines refusal example of SHARED-ORDER-009 stays.

### Output questions

_None._

### Internal choices

- The one-line swap is stated in SHARED-ORDER-009 and referenced from RH7107-004/RH7108-004, not restated there. Alternative: a requirement of its own per rule — rejected, it would duplicate a shared rule.
- Comments inside a one-line accessor list are not decided separately: SHARED-ORDER-002 (attached trivia travels, separators stay) governs them, as for every move. A `/* */` inside a one-line accessor list is rare.

### Follow-up candidates

_None._

## Whole run

- Files: `sdd/specs/shared/ordering-move-safety.md`, `sdd/specs/shared/code-fixes.md`, `sdd/specs/analyzer/RH7107.md`, `sdd/specs/analyzer/RH7108.md`, `sdd/changes/2026-11/moved-code-keeps-its-lines.md`.
- No rule page changed: RH7107 and RH7108 pages stay true (the fix still puts `get`/`add` first).
- No new requirement ID.
