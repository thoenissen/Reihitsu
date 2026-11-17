Role: spec author
Commit: bd4df7b
Verdict: spec written

Repair round 1 (audit 11, finding 1), on the maintainer's answer 5A recorded in `00-lead.md`. My prompt carried no intended fix or hoped-for verdict beyond the recorded answer.

## Changed requirements

- FMT-BREAK-039 — An argument list starts on the line of the code in front of it — changed (wording, one example added; no new ID):
  - Second bullet: the false precedent "as it is after a join of FMT-BREAK-015 or FMT-BREAK-020 inside it" is replaced. A list is judged as the line it becomes wherever the argument list stands in an element — directly, and inside a construct nested in the element whose lines FMT-BREAK-015 does not join (interpolation hole, query clause after the first, array or `stackalloc` initializer), in a chain or not. The other side of that qualifier (FC-002) is named: a collection expression nested in an element stays judged as written (first bullet).
  - *Why*: the precedent is narrowed to what holds on `main` — a list after a join of FMT-BREAK-015, a conditional expression after the joins of FMT-BREAK-015 and FMT-BREAK-020 — and the nested case is justified by the answer, not by precedent.
  - New example: interpolation hole (chain `x.ToString⏎()` and non-chain `Bar⏎(1)`), query clause, array initializer → one line each.

Change doc `sdd/changes/2026-11/argument-list-below-name.md`:
- *What*: the list sentence no longer claims FMT-BREAK-020 as a list precedent; "Their output does not change" is replaced by: FMT-BREAK-015's output does not change, FMT-BREAK-020's changes in the one case of change 7.
- Change 3's last sentence narrowed to "directly in an element" and pointed to change 7.
- New visible output change **7** (whole-document input/before/after for the four members, plus the RH5302 fix as the code-fix consequence, checked with `scripts/apply-fix.sh` on `main` and the branch).
- *Decisions*: answer 5A added to the maintainer's answers; "No output differs from the three statements it replaces" replaced by the one differing case and its reason (the chain join ran after the list layout on `main`); the FMT-BREAK-022 decision no longer cites FMT-BREAK-020 as a list precedent.
- *Alternatives*: option B (one argument per line, as on `main` for a chain's argument list) added as not chosen.
- Status left `Accepted`; *Affected requirements* unchanged.

No rule page changes (no rule reports the shape; RH5102 reports nothing on these inputs, checked with `apply-fix.sh`).

Evidence: `scripts/format.sh` on the new example's input with the branch CLI gives exactly the spec's output (apart from the final newline the formatter removes) and, with `main`'s CLI (worktree of eb89003), the change doc's *before* of change 7. `scripts/test.sh --project architecture`: 153 passed.

## Examples for the tests

FMT-BREAK-039, the new example "an argument list inside an interpolation hole, a query clause, or an array initializer of a list element" — chain member (`Baz($"{x.ToString⏎()}", 2)`) and non-chain member (`Baz($"{Bar⏎(1)}", 2)`) at least, plus one of query/initializer, as the auditor suggested. Code-fix side: change 7's RH5302 example.

## Output questions

_None._

## Internal choices

- The new visible change is appended as change 7 rather than renumbering changes 6 (code fixes) onward, so references to "change 6" in the run's handovers stay valid. Alternative: insert it as change 4 and renumber.
- The spec states the nested-construct case through its cause (constructs nested in an element whose lines FMT-BREAK-015 does not join) with the four kinds as examples, rather than as a closed list.

## Follow-up candidates

_None_ beyond those in `00-lead.md`. Observed but not pursued (outside the run, not an argument list): other FMT-BREAK-020 joins inside such nested constructs — a member-name or element-access split in a chain — still run after the list layout and keep the list one argument per line (checked on the branch: `Baz($"{x.⏎ToString()}", 2);` → `Baz($"{x.ToString()}",⏎    2);`); this run neither specifies nor changes them. A candidate only if the lead judges a member-name split inside a hole common enough; otherwise a rare input to drop.
