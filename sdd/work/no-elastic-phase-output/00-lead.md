# Lead — no-elastic-phase-output

- Issue: thoenissen/Reihitsu#1516 — "FMT-PIPE: no phase output carries elastic trivia" — labels `enhancement`, `formatter`, `maintenance`, `agent-filed` (goals are proposals).
- Slug: `no-elastic-phase-output`
- Branch: `claude/trusting-johnson-d4cgsd` (session-designated)
- Draft PR: thoenissen/Reihitsu#1822
- Lane: **behavior change** — the issue requests a new pipeline requirement (FMT-PIPE) and phase changes to meet it, i.e. new specified behavior, not an observed-versus-expected difference against an existing requirement.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator (all run; none skipped).
- Maintainer clarifications: none.

## Maintainer answers

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Spec author | Requirement wording: "no phase adds elastic trivia" vs the issue's "no phase output carries" | Take "adds" (FMT-PIPE-034), with a non-goal for elastic trivia a node-level caller passes in | Identical for document/tree entry points; "carries" would force phases to strip trivia from code-fix input and change code-fix spacing no one asked about | No formatter output changes (spec author ran format on the example; suite outputs unchanged); code-fix outputs untouched | 05b79d7 |
| 2 | Spec author | Where the rule lives | Stated once in pipeline spec; FMT-STRUCT-026 points to it | One statement of a rule | Spec text only | 05b79d7 |
| 3 | Skeptic | What the line-break phase's accessor attribute-list merge writes instead of the elastic comma | A comma with no whitespace behind it (the horizontal-spacing phase owns same-line spaces); one sentence in FMT-BREAK-032 states it so its recorded space exception does not grow. The code-fix merge path (RH5502–RH5531, SHARED-ATTR-018) keeps its current output | Keeps the spacing phase the sole owner of spaces; skeptic's recommendation | Both readings give identical final output; code-fix outputs must stay unchanged and their tests assert them | bf9462d |
| 4 | Skeptic | FMT-PIPE-034's claim that the document-level tree "is parsed from text" | Qualify it to the inputs this repository's callers pass | AGENTS.md, Supported inputs; FormatDocumentAsync has no production caller | Spec wording only | bf9462d |
| 5 | Skeptic | Issue goals (agent-filed) | "returns" → "adds"; keep the round-trip assertion on every formatter fixture; add that the code-fix suites keep their outputs; do not use the issue's stale `get=>""` example (already fixed by FMT-STRUCT-026) | Evidence in 01/02 handovers | No output changes anywhere | — |
| 6 | Skeptic | Follow-up candidate: merge code fixes rely on Roslyn post-processing for the comma space | Dropped, no issue | Nothing visible is wrong; no defect | Outputs correct today | — |
| 7 | Lead | Continue or fresh spec author for the amendment | Fresh (context growth 94k, cache cold) | Section 4 table | — | — |
| 8 | Spec author (03) | A comma the first list already held, with a space in the input | Written without whitespace too, like every comma of the merged list (FMT-BREAK-032) | One rule for every comma; the merge already rebuilds it today | Spacing phase writes the space back; final output identical | bf9462d |

## Follow-up candidates

(none yet)

## Audited SHA

(none yet)

## Open worklist / next step

- Done: spec author (01), commit 05b79d7; lane check confirmed behavior-change. Change doc Draft, no visible output changes, no output questions.
- Done: skeptic (02), commit 7ba84bc, verdict proceed; no output questions, no escalation 10.
- Done: spec author amendment (03), bf9462d; change doc Accepted.
- Batch point before test implementer: no output questions open, no stop.
- Next: test implementer (04).
