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
| 1 | skeptic | RH5202-007 third bullet is false: the RH5202 fix keeps tabs in a nested raw literal aligned in characters behind tabs (skeptic n01), which FMT-RAW-010 would then rewrite (SHARED-FIX-007). Option A: fix rewrites nested literal too; B: withhold the outer fix when a nested raw literal's closing whitespace holds a tab | B — withhold the fix for that class; spec author amends RH5202-007's third bullet and records the gap in `sdd/specs/analyzer/RH5202.md` | Nested raw literal in a hole of a tab-indented interpolated raw literal is a rare class; cheap check in the existing nested-literal loop; *Special cases do not grow the source* | Withholding a fix for a rare input class puts nothing in question (README, *What reaches the maintainer*) | 422eebc |
| 2 | skeptic | One owner for the stripped-indentation rewrite (FC-015) | The rewrite is stated once, in a shared helper used by both the formatter and the RH5202 fix (the skeptic names `RawStringLiteralUtilities`); implementer picks the exact place | Avoid two statements of one rule (architecture finding otherwise) | Internal structure | ed3d5e7 |
| 3 | skeptic | RH5601-002 `Why:` cites FMT-TAB-003 keeping raw-string tabs, stale under FMT-RAW-010 | Spec author amends the `Why:` wording only; requirement unchanged | Keep the spec consistent; SHARED-DIAG-003 still keeps code rules out of literal text | Spec wording only | 422eebc |
| 4 | spec author | AGENTS.md IndentationPhase row should name the rewrite | Spec author adds it in the amendment round (text only) | Phase-ownership table must name what the phase owns | Text only | 422eebc |
| 5 | test implementer | A comment in `RH5202…HoleTests.GetFixAllScenario` says a raw literal can never nest in another (false for interpolated raw strings) | Adjacent fix: cleaner corrects the comment wording | Comment-only, in a file the run changes | Test comment only | 24fd601 |
| 6 | implementer | `RH5202…HoleTests.VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsIndentedWithTabs` expects one Fix All iteration; under RH5202-008 the outer fix is offered only after the nested fix, so Fix All needs two | Test is wrong: test implementer sets two Fix All iterations for that test; final output unchanged | RH5202-008's second example states the two-step sequence; convergence still holds | Only the iteration count changes; the final fixed output is the same | 4fb0363 |

## Follow-up candidates

- RH5202 does not report a multi-line raw literal aligned in characters behind tabs (P11 gap in RH5202 itself; behavior change of RH5202, output question in `02-skeptic.md`, skeptic recommends reporting it). Source: `02-skeptic.md`.

## State

- Last step finished: skeptic (02-skeptic.md, 93d38d6): proceed; two spec amendments, one follow-up.
- Batch 1 answered: 1A, 2A, 3A, 4A.
- Spec author round done (03-spec-author.md, 422eebc): change doc Accepted; RH5202-008 (withhold, rare nested-tab class, also when the nested literal is reported); RH5601-002 Why; AGENTS.md row.
- Test implementer done (04-test-implementer.md, 1111881): 26 formatter + 19 analyzer tests red, all this run's; architecture green. Note: no RH5202 fix test for `$$` (markup limitation; formatter tests cover it).
- Implementer done (05-implementer.md, ed3d5e7): all pass except one disputed test (decision 6).
- Decision 6 done (06-test-implementer.md, 4fb0363).
- Cleaner done (07-cleaner.md, 24fd601): clean, 0 warnings.
- Spec checker done (08-spec-checker.md, 534d5ea): consistent, no findings.
- Merged origin/main (cfa445f, #1921: build config only; no overlap) as 6655052; Release build green, 0 warnings.
- Audited SHA: 1a0127edc904eeb29b1dddc96838f1b27d0ebea4 (main cfa445f). Auditor (09-auditor.md, 340c71c): pass with one doc finding (two `<param name="edit">` docs in RawStringLineShifts.cs).
- Cleaner doc fix (10-cleaner.md, e60a8e3): text-only proof PASS vs audited SHA; no second audit needed.
- Next: validator (11-validator.md), then publish.
- Next after answers: one fresh spec author round: answers into change doc `Decisions` + spec, Status Accepted, amendments of decisions 1, 3, 4. Then test implementer (tests to update named in 02-skeptic.md).
