# Lead — raw-string-tab-alignment

- Issue: thoenissen/Reihitsu#1432 — FMT-RAW: align tab-indented multi-line raw strings. Labels: enhancement, formatter, `agent-filed` (goals are proposals).
- Slug: `raw-string-tab-alignment`
- Branch: `claude/modest-turing-gg3lyc` (session-designated). Draft PR: #1923.
- Lane: **behavior change** — the issue requests a layout for an input the spec lists as a non-goal (`sdd/specs/formatter/raw-string-alignment.md`); a new requirement and a change doc are needed.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.
- Issue context (summarized): the issue's text names `RawStringAlignmentPhase`; per `AGENTS.md`, raw-string alignment now lives in `IndentationPhase`. The issue's one comment (agent-added, unverified) notes a possibly related case: an interpolation-hole line inside a tab-indented raw string aligned by character count rather than visually; it asks that this be checked with a test first and dropped if the prefix is already replaced by the time the hole is aligned.
- No maintainer clarifications yet.

## Maintainer answers

- Batch 1 (before test implementer), questions 1-4 of `01-spec-author.md`: maintainer chose the recommended option for all — 1A (stripped indentation written as spaces), 2A (also rewrite literals already aligned in characters behind tabs), 3A (RH5202 fix writes spaces too; withheld for the rare nested-tab-literal class, decision 1), 4A (partial whitespace-only line: one space per character after the move).

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | RH5202-007 third bullet is false: the RH5202 fix keeps tabs in a nested raw literal aligned in characters behind tabs (skeptic n01), which FMT-RAW-010 would then rewrite (SHARED-FIX-007). Option A: fix rewrites nested literal too; B: withhold the outer fix when a nested raw literal's closing whitespace holds a tab | B — withhold the fix for that class; spec author amends RH5202-007's third bullet and records the gap in `sdd/specs/analyzer/RH5202.md` | Nested raw literal in a hole of a tab-indented interpolated raw literal is a rare class; cheap check in the existing nested-literal loop; *Special cases do not grow the source* | Withholding a fix for a rare input class puts nothing in question (README, *What reaches the maintainer*) | pending |
| 2 | skeptic | One owner for the stripped-indentation rewrite (FC-015) | The rewrite is stated once, in a shared helper used by both the formatter and the RH5202 fix (the skeptic names `RawStringLiteralUtilities`); implementer picks the exact place | Avoid two statements of one rule (architecture finding otherwise) | Internal structure | pending |
| 3 | skeptic | RH5601-002 `Why:` cites FMT-TAB-003 keeping raw-string tabs, stale under FMT-RAW-010 | Spec author amends the `Why:` wording only; requirement unchanged | Keep the spec consistent; SHARED-DIAG-003 still keeps code rules out of literal text | Spec wording only | pending |
| 4 | spec author | AGENTS.md IndentationPhase row should name the rewrite | Spec author adds it in the amendment round (text only) | Phase-ownership table must name what the phase owns | Text only | pending |

## Follow-up candidates

- RH5202 does not report a multi-line raw literal aligned in characters behind tabs (P11 gap in RH5202 itself; behavior change of RH5202, output question in `02-skeptic.md`, skeptic recommends reporting it). Source: `02-skeptic.md`.

## State

- Last step finished: skeptic (02-skeptic.md, 93d38d6): proceed; two spec amendments, one follow-up.
- Batch 1 answered: 1A, 2A, 3A, 4A.
- Now: fresh spec author round (03-spec-author.md).
- Next after answers: one fresh spec author round: answers into change doc `Decisions` + spec, Status Accepted, amendments of decisions 1, 3, 4. Then test implementer (tests to update named in 02-skeptic.md).
