Role: spec author
Commit: b71499e
Verdict: spec written

Inputs: `00-lead.md` (*Maintainer answers*, batch 1: `1A, 2B`; lead accepts the change doc), `01-spec-author.md`, `02-skeptic.md` (Output questions 1 and 2), the change doc and `sdd/specs/analyzer/RH5202.md` at b71499e, `documentation/rules/RH5202.md`. The prompt carried no intended fix, cause, or hoped-for verdict beyond the recorded answers.

Files written:

- `sdd/changes/2026-11/rh5202-tab-aligned-raw-strings.md` — `Status: Accepted`. *What* now says the rule page gains one sentence. *Visible output changes* gains item 4 (the smart-tab literal of answer 1A, input / before / after) and item 6 (the rule-page sentence of answer 2B, before / after); the former item 4 is now item 5. *Decisions* gains "A literal written with tabs and aligned with spaces is reported" (1A) and "The rule page names the spaces" (2B); the sentence of "The diagnostic message stays" that said the rule page stays as short as it is is removed. *Alternatives considered* gains the two rejected options.
- `sdd/specs/analyzer/RH5202.md` — RH5202-009 only, see below.
- `documentation/rules/RH5202.md` — *Description* ends with the sentence of answer 2B, verbatim: "The indentation in front of the closing marker, and the same indentation of the content lines, is written with spaces, not tabs." No formatter mention; BOM kept.

## Changed requirements

- RH5202-009 — A literal aligned behind tabs is reported — clarified: the condition bullet "A tab in front of the closing quotes" states that any tab in that whitespace counts, also indentation tabs followed by aligning spaces; a new example shows the smart-tab literal (two tabs and eight spaces, column 10) reported and the fix writing 10 spaces in front of `x` and of the closing quotes, the tabs on lines outside the literal kept.

No requirement ID added or removed; `requirement-ids.txt` unchanged.

## Examples for the tests

- RH5202-009 — the new smart-tab example (answer 1A), in addition to the examples listed in `01-spec-author.md`. It is the skeptic's candidate row 9.

## Checks run

- `scripts/apply-fix.sh RH5202` on the smart-tab input (branch at b71499e, no production change), LF and CRLF: `no-diagnostic` — the change doc's *before*.
- `scripts/format.sh` on the same input moves the literal to column 16 because the formatter also replaces the code's tabs; the literal's own whitespace is written as one space per character (FMT-RAW-010). The spec's *after* is the code fix's result alone (column 10, 10 spaces), derived from RH5202-007 with a zero shift; it is not built yet.
- `scripts/test.sh --project architecture`: 197 passed, 1 failed — `RequirementCoverageTests`: "RH5202-009: no test covers it", the same expected failure as in `01`, until the test implementer tags a test.
- `scripts/lane-check.sh behavior-change --head worktree`: `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.

## Output questions

_None._

## Internal choices

- The rule-page sentence goes at the end of *Description*, as the answer states; *How to fix it* and the examples stay unchanged (the answer names only *Description*). Alternative: also a tab example on the page — not asked for, and the page stays short.
- The answer dates in *Decisions* are 2026-11-26 (the day the batch was answered in this run).

## Follow-up candidates

_None._
