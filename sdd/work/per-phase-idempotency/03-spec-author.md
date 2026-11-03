Role: spec author
Commit: dc3bbb3
Verdict: spec written

Amendment per `00-lead.md`, Decisions taken #3–#6. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the decisions recorded in `00-lead.md`.

## Changed requirements

- FMT-PIPE-027 — No phase changes the formatter's output — added (unchanged from `01-spec-author.md` except below); now a clarification without a change doc (Decision #3).
  - First qualifier: "a phase that runs alone on it may change it" replaced by "the requirement does not cover it". The old wording granted a permission nothing verifies; the formatter test helper checks such outputs too and is green (Decision #6), so the spec now only leaves them out of scope instead of stating what a phase may do there.
- FMT-PIPE-004 — Idempotency — clarified: the pointer to FMT-PIPE-027 stays.
- Change doc `sdd/changes/2026-11/per-phase-idempotency.md` removed. No spec, README, or other document referred to it (`grep -rn per-phase-idempotency` outside `sdd/work/` finds nothing).

FMT-PIPE-027 names no tests, so Decisions #4 (tags on the example test, `CheckFindsNoChangeInFormattedSource`, `CheckRunsTheProductionPhasesInPipelineOrder`, `CheckFindsNoChangeAboveCommentLedUsingDirectiveOfTheSameGroup`) and #5 (example test through `FormatSyntaxTree`, which the requirement names) needed no spec change; the requirement's normative sentence already names `FormatSyntaxTree`, and its "every phase, each sub-phase of the preparation phase included" is what `CheckRunsTheProductionPhasesInPipelineOrder` establishes. The test list the change doc carried now lives only in Decision #4.

Reasons the change doc carried that the spec does not, for the PR description: the exception list is removed so a future reversal is a defect against FMT-PIPE-027 rather than an accepted list entry; the list was already empty on `main` at `fe24dcc`, so its removal turns no passing test into a failing one. The node-level exclusion and the text comparison are already qualifiers with reasons in FMT-PIPE-027.

`scripts/lane-check.sh tests --head worktree`: `LANE CHECK: confirmed — claimed tests, diff needs only docs`. Its header line reads `TEXT-ONLY PROOF: FAIL — … 1 of 5 changed file(s) carry compiled behavior` although all five listed files are classified `Text` and none is C#; the lane verdict is confirmed regardless. Reported for the lead; not investigated further.

## Examples for the tests

- FMT-PIPE-027, examples 1 and 2, and the first qualifier's formatted case `class C{void M(){int x=1;}}`, as in `01-spec-author.md`; per Decision #5 the "is formatter output" leg goes through `ReihitsuFormatter.FormatSyntaxTree`, under LF and CRLF.

## Output questions

_None._

## Internal choices

- Reworded the first qualifier rather than leaving it; alternative: keep "may change it", which states an allowance the stricter helper check (Decision #6) contradicts in practice.

## Follow-up candidates

_None new._ The candidates in `00-lead.md` stand.
